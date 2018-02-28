using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace ClientUI
{
    using System.Text.RegularExpressions;
    using UserRegistration.Messages.Commands;

    class Program
    {
        static async Task Main()
        {
            Console.Title = "ClientUI";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");
            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(c => !string.IsNullOrEmpty(c.Namespace) && c.Namespace.EndsWith("Messages.Commands"));

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(RegisterNewUser).Assembly, "UserRegistration");

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            await RunLoop(endpointInstance);

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }

        static ILog log = LogManager.GetLogger<Program>();
        static Random rnd = new Random();

        static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            log.Info("- Press 'R' to register a new user");
            log.Info("- Press 'Q' to quit.");

            while (true)
            {
                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.P:
                        await RegisterNewUser(endpointInstance);
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }

        private static Task RegisterNewUser(IEndpointInstance endpointInstance)
        {
            Console.Write("\tEnter name: ");
            var name = RemoveWhitespace(Console.ReadLine());

            var command = new RegisterNewUser()
            {
                UserId = Guid.NewGuid(),
                Name = name,
                Email = $"{name}@particular.net"
            };

            Console.WriteLine($"\tAsked to register {name}");

            return endpointInstance.Send(command);
        }

        static string RemoveWhitespace(string name)
        {
            name = Regex.Replace(name, @"\s+", "");
            return name;
        }
    }
}