using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Codle.Api.Middleware
{
    public class StreamResult : IActionResult
    {
        private readonly CancellationToken _requestAborted;
        private readonly Action<Stream, CancellationToken> _onStreaming;

        public StreamResult(Action<Stream, CancellationToken> onStreaming, CancellationToken requestAborted)
        {
            _requestAborted = requestAborted;
            _onStreaming = onStreaming;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var stream = context.HttpContext.Response.Body;
            context.HttpContext.Response.GetTypedHeaders().ContentType = new MediaTypeHeaderValue("text/event-stream");
            _onStreaming(stream, _requestAborted);
            return Task.CompletedTask;
        }
    }
}
