using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaServer
{
    public class TriviaGame
    {
        private IEnumerable<TriviaPlayer> _players = new List<TriviaPlayer>();

        public TriviaGame(IEnumerable<TriviaPlayer> players)
        {
            _players = players;
        }

        public async Task PlayAsync()
        {
            foreach (var triviaQuestion in TriviaBank.DefaultBank)
            {
                foreach (var player in _players)
                {
                    player.SendQuestion(triviaQuestion);
                }

                // Wait for all users to answer

                await Task.WhenAll(_players.Select(p => p.AnswerTask));

                // Score
                foreach (var player in _players)
                {
                    if (string.Equals((await player.AnswerTask).Answer, triviaQuestion.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        player.Score++;
                    }
                }

                // Reset

                foreach (var player in _players)
                {
                    player.ResetAnswer();
                }
            }

            foreach (var player in _players)
            {
                player.FinishPlaying();
            }
        }
    }
}
