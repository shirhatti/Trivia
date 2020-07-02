using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using TriviaGame;

namespace TriviaServer
{
    public class TriviaPlayer
    {
        private IAsyncStreamReader<TriviaAnswer> _requestStream;
        private IServerStreamWriter<TriviaQuestion> _responseStream;
        private TaskCompletionSource<object> _finishedPlayingTcs = new TaskCompletionSource<object>();
        private TaskCompletionSource<TriviaAnswer> _answerTcs = new TaskCompletionSource<TriviaAnswer>();

        public int Score { get; set; }

        public Task<object> FinishedPlayingTask => _finishedPlayingTcs.Task;

        public Task<TriviaAnswer> AnswerTask => _answerTcs.Task;

        public TriviaPlayer(IAsyncStreamReader<TriviaAnswer> requestStream, IServerStreamWriter<TriviaQuestion> responseStream)
        {
            _requestStream = requestStream;
            _responseStream = responseStream;

            var responseTask = Task.Run(async () =>
            {
                await foreach (var answer in requestStream.ReadAllAsync())
                {
                    _answerTcs.SetResult(answer);
                }
            });
        }

        public void ResetAnswer()
        {
            _answerTcs = new TaskCompletionSource<TriviaAnswer>();
        }

        public void SendQuestion(TriviaBankEntry entry)
        {
            var question = new TriviaQuestion()
            {
                Question = entry.Question
            };
            question.Answers.Add(entry.Answers);

            _responseStream.WriteAsync(question);
        }

        public void FinishPlaying()
        {
            _finishedPlayingTcs.SetResult(null);
        }
    }
}
