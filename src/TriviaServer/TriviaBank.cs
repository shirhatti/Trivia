using System;
using System.Collections.Generic;

namespace TriviaServer
{
    public class TriviaBank
    {
        public static IEnumerable<TriviaBankEntry> DefaultBank = new TriviaBankEntry[]
        {
            new TriviaBankEntry {
                Question = "Equal to roughly 746 watts, what animal-based unit is used to measure the rate at which work is done?",
                QuestionID = Guid.NewGuid(),
                Answers = new string[] { "HorsePower", "Donkeystrength", "Llamathrust", "Zebraforce" },
                CorrectAnswer = 1
            },
            new TriviaBankEntry {
                Question = "Which of the following is the largest?",
                QuestionID = Guid.NewGuid(),
                Answers = new string[] { "Peanut", "Elephant", "Moon", "Kettle" },
                CorrectAnswer = 3
            }
        };
    }
}
