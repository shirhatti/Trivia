using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriviaGame;

namespace TriviaServer
{
    public class TriviaPlayer
    {
        // States
        private TaskCompletionSource<Game> _readyTcs = new TaskCompletionSource<Game>();
        private TaskCompletionSource<object> _connectedTcs = new TaskCompletionSource<object>();
        private TaskCompletionSource<object> _endedTcs = new TaskCompletionSource<object>();
        private Dictionary<Guid, TaskCompletionSource<object>> _questionAnsweredTasks = new Dictionary<Guid, TaskCompletionSource<object>>();

        private Dictionary<Guid, TriviaBankEntry> _questions = new Dictionary<Guid, TriviaBankEntry>();
        private IServerStreamWriter<TriviaQuestion> _responseStream;

        public int Score { get; set; }
        // TODO: Time taken?
        public string Name { get; }

        public Task<Game> ReadyTask => _readyTcs.Task;
        public Task<object> ConnectedTask => _connectedTcs.Task;

        public TriviaPlayer(string name)
        {
            Name = name;
        }

        public void StartGame(TriviaGame game)
        {
            _readyTcs.SetResult(new Game { GameID = game.ID.ToString() });
        }

        public Task Play(IAsyncStreamReader<TriviaAnswer> requestStream, IServerStreamWriter<TriviaQuestion> responseStream)
        {
            _responseStream = responseStream;
            var responseTask = Task.Run(async () =>
            {
                await foreach (var answer in requestStream.ReadAllAsync())
                {
                    var questionID = Guid.Parse(answer.QuestionID);
                    
                    // score answer
                    if (answer.Answer == _questions[questionID].CorrectAnswer)
                    {
                        Score++;
                    }

                    // mark question as answered
                    _questionAnsweredTasks[questionID].SetResult(null);
                }
            });

            _connectedTcs.SetResult(null);

            return _endedTcs.Task;
        }

        public void SendQuestion(TriviaBankEntry entry)
        {
            _questionAnsweredTasks[entry.QuestionID] = new TaskCompletionSource<object>();
            _questions[entry.QuestionID] = entry;

            var question = new TriviaQuestion()
            {
                Question = entry.Question
            };
            question.Answers.Add(entry.Answers);

            _responseStream.WriteAsync(question);
        }

        public void EndGame() => _endedTcs.SetResult(null);

        public Task QuestionAnsweredTask(Guid questionID) => _questionAnsweredTasks[questionID].Task;
    }
}
