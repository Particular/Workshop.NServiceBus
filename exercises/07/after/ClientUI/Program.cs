using System;
using System.Threading.Tasks;
using Messages.Commands;
using NServiceBus;
using NServiceBus.Logging;
using Shared;

ILog log = LogManager.GetLogger<Program>();
Random rnd = new Random();

Console.Title = "ClientUI";

var endpointConfiguration = new EndpointConfiguration("ClientUI");
endpointConfiguration.Configure(s =>
{
    s.RouteToEndpoint(typeof(PlaceOrder), "Sales");
});

var endpointInstance = await Endpoint.Start(endpointConfiguration);

await RunLoop(endpointInstance);

await endpointInstance.Stop();

async Task RunLoop(IEndpointInstance endpointInstance)
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
