namespace ClientUI
{
    using Messages.Commands;
    using NServiceBus;
    using NServiceBus.Logging;
    using System;
    using System.Threading.Tasks;

    internal class Program
    {
        private static readonly ILog log = LogManager.GetLogger<Program>();
        private static readonly Random rnd = new Random();

        private static async Task Main()
        {
            Console.Title = "ClientUI";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(c =>
                !string.IsNullOrEmpty(c.Namespace) && c.Namespace.EndsWith("Messages.Commands"));

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            await RunLoop(endpointInstance);

            await endpointInstance.Stop().ConfigureAwait(false);
        }

        private static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            log.Info("- Press 'P' to place an order");
            log.Info("- Press 'Q' to quit.");

            while (true)
            {
                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.P:
                        // Instantiate the command
                        var command = new PlaceOrder
                        {
                            CustomerId = rnd.Next(10),
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command
                        log.Info(
                            $"Sending PlaceOrder command, CustomerId = {command.CustomerId}, OrderId = {command.OrderId}");
                        await endpointInstance.Send(command);
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }
    }
}