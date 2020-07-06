using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaServer
{
    public class TriviaGame
    {
        private ILogger _logger;

        public Guid ID { get; }
        public IEnumerable<TriviaPlayer> Players { get; }

        public TriviaGame(IEnumerable<TriviaPlayer> players, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TriviaGame>();
            Players = players;
            ID = Guid.NewGuid();
        }

        public async Task PlayGameAsync()
        {
            _logger.LogInformation("Starting trivia game");

            // Wait for all users to connect
            await Task.WhenAll(Players.Select(p => p.ConnectedTask));

            foreach (var question in TriviaBank.Questions.Values)
            {
                _logger.LogInformation($"Sending question with id {question.QuestionID}");

                // Send question to players
                foreach (var player in Players)
                {
                    player.SendQuestion(question);
                }

                // Wait for all users to answer the given the question
                await Task.WhenAll(Players.Select(p => p.QuestionAnsweredTask));
            }

            _logger.LogInformation("Trivia questions completed");
        }

        public TriviaPlayer GetPlayer(string name) => Players.Single(p => p.Name == name);
    }
}
