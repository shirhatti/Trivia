using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaServer
{
    public class TriviaLobby
    {
        private const int PlayersPerGame = 2;
        private ConcurrentQueue<TriviaPlayer> _players = new ConcurrentQueue<TriviaPlayer>();
        private Dictionary<Guid, TriviaGame> _games = new Dictionary<Guid, TriviaGame>();
        private object _gameStartLock = new object();
        private ILoggerFactory _loggerFactory;

        public TriviaLobby(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void AddPlayer(TriviaPlayer player)
        {
            _players.Enqueue(player);

            if (_players.Count >= PlayersPerGame)
            {
                var readyPlayers = new List<TriviaPlayer>();

                lock (_gameStartLock)
                {
                    for (int i = 0; i < PlayersPerGame; i++)
                    {
                        _players.TryDequeue(out var readyPlayer);
                        readyPlayers.Add(readyPlayer);
                    }
                }

                var game = new TriviaGame(readyPlayers, _loggerFactory);
                _games[game.ID] = game;

                foreach (var readyPlayer in readyPlayers)
                {
                    readyPlayer.StartGame(game);
                }

                Task.Run(() => game.StartGameAsync());
            }
        }

        public TriviaGame GetGame(Guid guid) => _games[guid];
    }
}
