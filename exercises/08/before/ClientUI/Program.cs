using Shared.Configuration;

namespace ClientUI
{
    using NServiceBus;
    using NServiceBus.Logging;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UserRegistration.Messages.Commands;

    internal class Program
    {
        private static IEndpointInstance endpointInstance;

        private static readonly ILog log = LogManager.GetLogger<Program>();
        private static Random rnd = new Random();

        private static async Task Main()
        {
            Console.Title = "ClientUI";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");
            endpointConfiguration.Configure();

            endpointInstance = await Endpoint.Start(endpointConfiguration);

            await RunLoop();

            await endpointInstance.Stop().ConfigureAwait(false);
        }

        private static async Task RunLoop()
        {
            log.Info("- Press '1' to register a new user and confirm it.");
            log.Info("- Press '2' to register a batch of 10 users.");
            log.Info("- Press 'Q' to quit.");

            while (true)
            {
                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        Console.Write("\tEnter name: ");
                        var name = RemoveWhitespace(Console.ReadLine());
                        await RegisterNewUser(name);
                        break;

                    case ConsoleKey.D2:
                        RegisterBatchOfUsers();
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }


        private static Task RegisterNewUser(string name)
        {
            Console.WriteLine($"\tAsked to register {name}");

            // Send the user registration command

            return Task.CompletedTask;
        }

        private static void RegisterBatchOfUsers()
        {
            var random = new Random();
            var users = new[] { "Dennis", "Ramon", "Mauro", "Adam", "Udi", "David", "Szymon", "Tomasz", "Sean", "Kim" };

            Parallel.For(0, 10, i => { RegisterNewUser(users[random.Next(9)]); });
        }

        private static string RemoveWhitespace(string name)
        {
            name = Regex.Replace(name, @"\s+", "");
            return name;
        }
    }
}