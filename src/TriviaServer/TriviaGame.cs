using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame;

namespace TriviaServer
{
    public class TriviaGame
    {
        private IEnumerable<TriviaPlayer> _players;

        public Guid ID { get; }

        public TriviaGame(IEnumerable<TriviaPlayer> players)
        {
            _players = players;
            ID = Guid.NewGuid();
        }

        public async Task StartGameAsync()
        {
            // Wait for all users to connect
            await Task.WhenAll(_players.Select(p => p.ConnectedTask));

            foreach (var question in TriviaBank.DefaultBank)
            {
                // Send question to players
                foreach (var player in _players)
                {
                    player.SendQuestion(question);
                }

                // Wait for all users to answer the given the question
                await Task.WhenAll(_players.Select(p => p.QuestionAnsweredTask(question.QuestionID)));
            }

            // Send question to players
            foreach (var player in _players)
            {
                player.EndGame();
            }
        }

        public TriviaPlayer GetPlayer(string name) => _players.Single(p => p.Name == name);
    }
}
