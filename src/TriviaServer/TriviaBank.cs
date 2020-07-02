using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaServer
{
    public class TriviaBank
    {
        public static IEnumerable<TriviaBankEntry> DefaultBank = new TriviaBankEntry[]
        {
            new TriviaBankEntry {
                Question = "Equal to roughly 746 watts, what animal-based unit is used to measure the rate at which work is done?",
                Answers = new string[] { "HorsePower", "Donkeystrength", "Llamathrust", "Zebraforce" },
                CorrectAnswer = "HorsePower"
            },
            new TriviaBankEntry {
                Question = "Which of the following is the largest?",
                Answers = new string[] { "Peanut", "Elephant", "Moon", "Kettle" },
                CorrectAnswer = "Moon"
            }
        };
    }
}
