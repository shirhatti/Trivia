using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaServer
{
    public class TriviaBankEntry
    {
        public string Question { get; set; }
        public Guid QuestionID { get; set; }
        public IEnumerable<string> Answers { get; set; }
        public int CorrectAnswer { get; set; }
    }
}
