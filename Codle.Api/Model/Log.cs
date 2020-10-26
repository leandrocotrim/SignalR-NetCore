using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codle.Api.Model
{
    public class Log
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public override string ToString() => $"{Id} - {Name} - {IsComplete}";
    }
}
