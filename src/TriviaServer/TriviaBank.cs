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
                prompt: "Equal to roughly 746 watts, what animal-based unit is used to measure the rate at which work is done?",
                answers: new string[] { "HorsePower", "Donkeystrength", "Llamathrust", "Zebraforce" },
                correctAnswer: 0);
            AddQuestion(
                prompt: "Which of the following is the largest?",
                answers: new string[] { "Peanut", "Elephant", "Moon", "Kettle" },
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
