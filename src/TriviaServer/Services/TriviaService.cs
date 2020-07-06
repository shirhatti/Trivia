using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TriviaGame;
using TriviaServer.Hubs;

namespace TriviaServer
{
    public class TriviaService : Trivia.TriviaBase
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly TriviaLobby _lobby;
        private readonly IHubContext<ScoreHub> _hubContext;

        public TriviaService(ILoggerFactory loggerFactory, TriviaLobby lobby, ILogger<TriviaService> logger, IHubContext<ScoreHub> hubContext)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _lobby = lobby;
            _hubContext = hubContext;
        }

        public override async Task<Game> StartTrivia(Player request, ServerCallContext context)
        {
            var player = new TriviaPlayer(request.Name, _loggerFactory);
            _lobby.AddPlayer(player);

            player.ScoreUpdated += ScoreUpdated;

            await player.ReadyTask;

            return new Game { GameID = player.Game.ID.ToString() };
        }

        private void ScoreUpdated(object sender, EventArgs e)
        {
            var players = new List<string>();
            var scores = new List<int>();

            foreach (var game in _lobby.Games.Values)
            {
                foreach (var player in game.Players)
                {
                    players.Add(player.Name);
                    scores.Add(player.Score);
                }
            }

            _hubContext.Clients.All.SendAsync("UpdateScore", new
            {
                Names = players.ToArray(),
                Scores = scores.ToArray()
            });
        }

        public override async Task PlayTrivia(IAsyncStreamReader<TriviaAnswer> requestStream, IServerStreamWriter<global::TriviaGame.TriviaQuestion> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("Playing trivia");
            var playerName = context.RequestHeaders.SingleOrDefault(h => h.Key == "playername")?.Value;
            var gameId = context.RequestHeaders.SingleOrDefault(h => h.Key == "gameid")?.Value;

            _logger.LogInformation($"Starting trivia game with {playerName} in {gameId} with ");
            var game = _lobby.Games[Guid.Parse(gameId)];
            var player = game.GetPlayer(playerName);

            var responseHeader = new Metadata();
            responseHeader.Add("numberofquestions", TriviaBank.Questions.Count.ToString());

            await context.WriteResponseHeadersAsync(responseHeader);
            await player.Play(requestStream, responseStream);
        }

        public override Task<TriviaResult> TriviaScore(TriviaSession request, ServerCallContext context)
        {
            return Task.FromResult(new TriviaResult
            {
                Score = _lobby
                    .Games[Guid.Parse(request.Game.GameID)]
                    .GetPlayer(request.Player.Name).Score
            });
        }
    }
}
