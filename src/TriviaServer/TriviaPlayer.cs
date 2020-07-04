using Grpc.Core;
using Microsoft.Extensions.Logging;
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
        private Dictionary<Guid, TaskCompletionSource<object>> _questionAnsweredTasks = new Dictionary<Guid, TaskCompletionSource<object>>();

        private Dictionary<Guid, TriviaBankEntry> _questions = new Dictionary<Guid, TriviaBankEntry>();
        private IServerStreamWriter<TriviaQuestion> _responseStream;
        private ILogger _logger;
        public int Score { get; set; }
        // TODO: Time taken?
        public string Name { get; }

        public Task<Game> ReadyTask => _readyTcs.Task;
        public Task<object> ConnectedTask => _connectedTcs.Task;

        public TriviaPlayer(string name, ILoggerFactory loggerFactory)
        {
            Name = name;
            _logger = loggerFactory.CreateLogger<TriviaPlayer>();
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
                    _logger.LogInformation($"Received answer for {answer.QuestionID}: {answer.Answer}");

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

            return responseTask;
        }

        public void SendQuestion(TriviaBankEntry entry)
        {
            _questionAnsweredTasks[entry.QuestionID] = new TaskCompletionSource<object>();
            _questions[entry.QuestionID] = entry;

            var question = new TriviaQuestion()
            {
                Question = entry.Question,
                QuestionID = entry.QuestionID.ToString()
            };
            question.Answers.Add(entry.Answers);

            _responseStream.WriteAsync(question);
        }

        public Task QuestionAnsweredTask(Guid questionID) => _questionAnsweredTasks[questionID].Task;
    }
}
