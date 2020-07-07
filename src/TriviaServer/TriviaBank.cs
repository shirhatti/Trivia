using System;
using System.Collections.Generic;

namespace TriviaServer
{
    public static class TriviaBank
    {
        public static IDictionary<Guid, TriviaQuestion> Questions = new Dictionary<Guid, TriviaQuestion>();

        static TriviaBank()
        {
            AddQuestion(
                prompt: "What does the g in gRPC stand for?",
                answers: new string[] { "google", "gRPC", "general purpose", "good" },
                correctAnswer: 1);
            AddQuestion(
                prompt: "What's not hard in computer science?",
                answers: new string[] { "off-by-1 errors", "naming things", "building a gRPC implementation", "cache invalidation" },
                correctAnswer: 2);
        }

        private static void AddQuestion(string prompt, string[] answers, int correctAnswer)
        {
            var questionID = Guid.NewGuid();

            Questions[questionID] = new TriviaQuestion
            {
                Question = prompt,
                QuestionID = questionID,
                Answers = answers,
                CorrectAnswer = correctAnswer
            };
        }
    }
}
