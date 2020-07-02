using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaServer
{
    public class TriviaLobby
    {
        private const int PlayersPerGame = 1;
        private ConcurrentQueue<TriviaPlayer> _players = new ConcurrentQueue<TriviaPlayer>();
        private object _gameStartLock = new object();
        
        public void AddPlayer(TriviaPlayer player)
        {
            _players.Enqueue(player);

            if (_players.Count >= PlayersPerGame)
            {
                lock (_gameStartLock)
                {
                    var readyPlayers = new List<TriviaPlayer>();

                    for (int i = 0; i < PlayersPerGame; i++)
                    {
                        _players.TryDequeue(out var readyPlayer);
                        readyPlayers.Add(readyPlayer);
                    }

                    // fire and forget
                    new TriviaGame(readyPlayers).PlayAsync();
                }
            }
        }
    }
}
