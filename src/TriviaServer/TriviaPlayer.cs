using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TriviaGame;

namespace TriviaServer
{
    public class TriviaPlayer
    {
        // States
        private TaskCompletionSource<object> _readyTcs = new TaskCompletionSource<object>();
        private TaskCompletionSource<object> _connectedTcs = new TaskCompletionSource<object>();
        private TaskCompletionSource<object> _questionAnswered = new TaskCompletionSource<object>();

        private IServerStreamWriter<global::TriviaGame.TriviaQuestion> _responseStream;
        private ILogger _logger;

        public int Score { get; set; }
        public string Name { get; }
        public TriviaGame Game { get; set; }
        public event EventHandler ScoreUpdated;
        public Task<object> ReadyTask => _readyTcs.Task;
        public Task<object> ConnectedTask => _connectedTcs.Task;
        public Task<object> QuestionAnsweredTask => _questionAnswered.Task;

        public TriviaPlayer(string name, ILoggerFactory loggerFactory)
        {
            Name = name;
            _logger = loggerFactory.CreateLogger<TriviaPlayer>();
        }

        public void StartGame(TriviaGame game)
        {
            Game = game;
            _readyTcs.SetResult(null);
        }

        public Task Play(IAsyncStreamReader<TriviaAnswer> requestStream, IServerStreamWriter<global::TriviaGame.TriviaQuestion> responseStream)
        {
            _responseStream = responseStream;
            var responseTask = Task.Run(async () =>
            {
                await foreach (var answer in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"Received answer for {answer.QuestionID}: {answer.Answer}");

                    var questionID = Guid.Parse(answer.QuestionID);
                    
                    // score answer
                    if (answer.Answer == TriviaBank.Questions[questionID].CorrectAnswer)
                    {
                        Score++;
                    }

                    ScoreUpdated?.Invoke(this, null);

                    // notify question answered
                    _questionAnswered.SetResult(null);
                }
            });

            _connectedTcs.SetResult(null);

            return responseTask;
        }

        public void SendQuestion(TriviaQuestion entry)
        {
            _questionAnswered = new TaskCompletionSource<object>();

            var question = new global::TriviaGame.TriviaQuestion()
            {
                Question = entry.Question,
                QuestionID = entry.QuestionID.ToString()
            };
            question.Answers.Add(entry.Answers);

            _responseStream.WriteAsync(question);
        }
    }
}
