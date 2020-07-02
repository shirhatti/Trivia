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

        public override Task PlayTrivia(IAsyncStreamReader<TriviaAnswer> requestStream, IServerStreamWriter<TriviaQuestion> responseStream, ServerCallContext context)
        {
            var player = new TriviaPlayer(requestStream, responseStream);
            _lobby.AddPlayer(player);

            return player.FinishedPlayingTask;
        }
    }
}
