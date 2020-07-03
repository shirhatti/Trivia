using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame;

namespace TriviaConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Trivia.TriviaClient(channel);

            using var call = client.PlayTrivia();
            TriviaQuestion currentQuestion = null; // need thread-safety

            var responseTask = Task.Run(async () =>
            {
                await foreach (var triviaQuestion in call.ResponseStream.ReadAllAsync())
                {
                    currentQuestion = triviaQuestion;

                    Console.WriteLine($"Question: {triviaQuestion.Question}");
                    var answers = triviaQuestion.Answers.ToList();
                    for (int i = 0; i < answers.Count; i++)
                    {
                        Console.Write($"{i}. {answers[i]} ");
                    }
                    Console.WriteLine();
                }
            });

            while (true)
            {
                var result = Console.ReadLine();
                if (responseTask.IsCompleted)
                {
                    break;
                }

                if (currentQuestion.Answers.Any(a => string.Equals(a, result, StringComparison.OrdinalIgnoreCase)))
                {
                    //await call.RequestStream.WriteAsync(new TriviaAnswer { Answer = result });
                }
            }

            await responseTask;
            Console.ReadKey();
        }
    }
}
