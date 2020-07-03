using Microsoft.Extensions.Logging;
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
        private ILogger _logger;

        public Guid ID { get; }

        public TriviaGame(IEnumerable<TriviaPlayer> players, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("TriviaGame");
            _players = players;
            ID = Guid.NewGuid();
        }

        public async Task StartGameAsync()
        {
            _logger.LogInformation("Starting trivia game");

            // Wait for all users to connect
            await Task.WhenAll(_players.Select(p => p.ConnectedTask));

            foreach (var question in TriviaBank.DefaultBank)
            {
                _logger.LogInformation($"Sending question with id {question.QuestionID}");

                // Send question to players
                foreach (var player in _players)
                {
                    player.SendQuestion(question);
                }

                // Wait for all users to answer the given the question
                await Task.WhenAll(_players.Select(p => p.QuestionAnsweredTask(question.QuestionID)));
            }

            _logger.LogInformation("Trivia questions completed");

            // Send question to players
            foreach (var player in _players)
            {
                player.EndGame();
            }
        }

        public TriviaPlayer GetPlayer(string name) => _players.Single(p => p.Name == name);
    }
}
