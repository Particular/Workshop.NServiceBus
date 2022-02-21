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
        static IEndpointInstance endpointInstance;

        static async Task Main()
        {
            Console.Title = "ClientUI";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();
            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            var metrics = endpointConfiguration.EnableMetrics();
            metrics.SendMetricDataToServiceControl("particular.monitoring", TimeSpan.FromSeconds(0.5));
            endpointConfiguration.SendHeartbeatTo(
                serviceControlQueue: "particular.servicecontrol",
                frequency: TimeSpan.FromSeconds(60),
                timeToLive: TimeSpan.FromSeconds(30));

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
            conventions.DefiningEventsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(RegisterNewUser).Assembly, "UserRegistration");

            endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            await RunLoop();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }

        static ILog log = LogManager.GetLogger<Program>();
        static Random rnd = new Random();

        static async Task RunLoop()
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

            var command = new RegisterNewUser()
            {
                UserId = Guid.NewGuid(),
                Name = name,
                EmailAddress = $"{name}@particular.net"
            };

            Console.WriteLine($"\tAsked to register {name}");

            return endpointInstance.Send(command);
        }

        static void RegisterBatchOfUsers()
        {
            var random = new Random();
            var users = new[] { "Dennis", "Ramon", "Mauro", "Adam", "Udi", "David", "Szymon", "Tomasz", "Sean", "Kim" };

            Parallel.For(0, 10, i =>
            {
                RegisterNewUser(users[random.Next(9)]).GetAwaiter().GetResult();
            });
        }

        static string RemoveWhitespace(string name)
        {
            name = Regex.Replace(name, @"\s+", "");
            return name;
        }
    }
}