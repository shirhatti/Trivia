using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaServer
{
    public class TriviaBankEntry
    {
        public string Question { get; set; }
        public IEnumerable<string> Answers { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
