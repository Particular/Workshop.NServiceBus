namespace ClientUI
{
    using Messages;
    using NServiceBus;
    using NServiceBus.Logging;
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    internal class Program
    {
        private static ILog log;

        private static async Task Main()
        {
            log = LogManager.GetLogger<Program>();

            Console.Title = "ClientUI";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

            endpointConfiguration.UseSerialization<SystemJsonSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                                                 ;

            await RunLoop(endpointInstance)
                ;

            await endpointInstance.Stop()
                                  ;
        }

        private static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            while (true)
            {
                log.Info("Press 'P' to place an order, or 'Q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.P:
                        // Instantiate the command
                        var command = new PlaceOrder
                        {
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command
                        log.Info($"Sending PlaceOrder command, OrderId = {command.OrderId}");
                        await endpointInstance.Send(command)
                                              ;

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