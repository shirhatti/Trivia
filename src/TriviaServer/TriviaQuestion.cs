using System;
using System.Collections.Generic;

namespace TriviaServer
{
    public class TriviaQuestion
    {
        public string Question { get; set; }
        public Guid QuestionID { get; set; }
        public IEnumerable<string> Answers { get; set; }
        public int CorrectAnswer { get; set; }
    }
}
