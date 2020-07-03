using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TriviaGame;

namespace TriviaServer
{
    public class TriviaService : Trivia.TriviaBase
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly TriviaLobby _lobby;

        public TriviaService(ILoggerFactory loggerFactory, TriviaLobby lobby)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger("TriviaService");
            _lobby = lobby;
        }

        public override Task<Game> StartTrivia(Player request, ServerCallContext context)
        {
            var player = new TriviaPlayer(request.Name, _loggerFactory);
            _lobby.AddPlayer(player);

            return player.ReadyTask;
        }

        public override Task PlayTrivia(IAsyncStreamReader<TriviaAnswer> requestStream, IServerStreamWriter<TriviaQuestion> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("Playing trivia");
            var playerName = context.RequestHeaders.SingleOrDefault(h => h.Key == "playername")?.Value;
            var gameId = context.RequestHeaders.SingleOrDefault(h => h.Key == "gameid")?.Value;

            _logger.LogInformation($"Starting trivia game with {playerName} in {gameId}");
            var game = _lobby.GetGame(Guid.Parse(gameId));
            var player = game.GetPlayer(playerName);

            return player.Play(requestStream, responseStream);
        }

        public override Task<TriviaResult> TriviaScore(TriviaSession request, ServerCallContext context)
        {
            return Task.FromResult(new TriviaResult
            {
                Score = _lobby
                    .GetGame(Guid.Parse(request.Game.GameID))
                    .GetPlayer(request.Player.Name).Score
            });
        }
    }
}
