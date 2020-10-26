using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Codle.Api.Middleware;
using Codle.Api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Codle.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1", Deprecated = true)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LogsController : ControllerBase
    {
        // GET: api/Log
        [HttpGet]
        public IEnumerable<Log> Get()
        {
            return _logs;
        }

        // GET: api/Log/5
        [HttpGet("{id}", Name = "Get")]
        public Log Get(long id)
        {
            return _logs.FirstOrDefault(log => log.Id == id);
        }

        // POST: api/Log
        [HttpPost]
        public async Task<ActionResult<Log>> Post([FromBody] Log log)
        {
            if (log == null)
                return BadRequest();

            if (log.Id == 0)
            {
                var max = _logs.Select(l => l.Id).DefaultIfEmpty(0).Max();
                log.Id = max + 1;
            }

            _logs.Add(log);

            await WriteOnStream(log, "Log adicionado");
            await WriteOnStream2(log, "Log adicionado");

            return log;
        }

        // PUT: api/Log/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Log>> Put(long id, [FromBody] Log value)
        {
            var log = _logs.SingleOrDefault(i => i.Id == id);
            if (log != null)
            {
                _logs.Remove(log);
                value.Id = id;
                _logs.Add(value);

                await WriteOnStream(value, "Log atualizado");
                await WriteOnStream2(value, "Log atualizado");

                return log;
            }

            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var log = _logs.SingleOrDefault(i => i.Id == id);
            if (log != null)
            {
                _logs.Remove(log);
                await WriteOnStream(log, "Log removido");
                await WriteOnStream2(log, "Log removido");
                return Ok(new { Description = "Log removido" });
            }

            return BadRequest();
        }

        #region -- Builder --

        public LogsController(IHubContext<Streaming> streaming) => _streaming = streaming;

        #endregion

        #region -- Streaming --

        private readonly IHubContext<Streaming> _streaming;
        private static ConcurrentBag<StreamWriter> _clientSW = new ConcurrentBag<StreamWriter>();
        private static List<Log> _logs = new List<Log>();

        [HttpGet]
        [Route("streaming")]
        public IActionResult Streaming()
        {
            return new StreamResult(
                (stream, cancelToken) => {
                    var wait = cancelToken.WaitHandle;
                    var clientSW = new StreamWriter(stream);
                    _clientSW.Add(clientSW);

                    wait.WaitOne();

                    StreamWriter ignore;
                    _clientSW.TryTake(out ignore);
                },
                HttpContext.RequestAborted);
        }

        private async Task WriteOnStream2(Log log, string action)
        {
            foreach (var _client in _clientSW)
            {
                string jsonData = string.Format("{0}\n", JsonSerializer.Serialize(new { log, action }));
                await _client.WriteAsync(jsonData);
                await _client.FlushAsync();
            }
        }

        private async Task WriteOnStream(Log log, string action)
        {
            string jsonData = string.Format("{0}\n", JsonSerializer.Serialize(new { log, action }));

            //Utiliza o Hub para enviar uma mensagem para ReceiveMessage
            await _streaming.Clients.All.SendAsync("ReceiveMessage", jsonData);

            foreach (var _client in _clientSW)
            {
                await _client.WriteAsync(jsonData);
                await _client.FlushAsync();
            }
        }

        #endregion
    }
}
