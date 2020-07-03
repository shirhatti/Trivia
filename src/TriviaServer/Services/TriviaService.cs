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
        private readonly ILogger<TriviaService> _logger;
        private readonly TriviaLobby _lobby;

        public TriviaService(ILogger<TriviaService> logger, TriviaLobby lobby)
        {
            _logger = logger;
            _lobby = lobby;
        }

        public override Task<Game> StartTrivia(Player request, ServerCallContext context)
        {
            var player = new TriviaPlayer(request.Name);
            _lobby.AddPlayer(player);

            return player.ReadyTask;
        }

        public override Task PlayTrivia(IAsyncStreamReader<TriviaAnswer> requestStream, IServerStreamWriter<TriviaQuestion> responseStream, ServerCallContext context)
        {
            var playerName = context.RequestHeaders.Single(h => h.Key == "PlayerName").Value;
            var gameId = context.RequestHeaders.Single(h => h.Key == "GameID").Value;
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
