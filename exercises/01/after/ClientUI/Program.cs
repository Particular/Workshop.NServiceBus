using Messages;
using NServiceBus;
using System;
using System.Threading.Tasks;
using Shared.Configuration;

Console.Title = "ClientUI";

var endpointConfiguration = new EndpointConfiguration("ClientUI");

endpointConfiguration.Configure();

var endpointInstance = await Endpoint.Start(endpointConfiguration);

await RunLoop(endpointInstance);

await endpointInstance.Stop();


static async Task RunLoop(IEndpointInstance endpointInstance)
{
    while (true)
    {
        Console.WriteLine("Press 'P' to place an order, or 'Q' to quit.");
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

                // Send the command to the local endpoint
                Console.WriteLine($"Sending PlaceOrder command, OrderId = {command.OrderId}");
                await endpointInstance.SendLocal(command);

                break;

            case ConsoleKey.Q:
                return;

            default:
                Console.WriteLine("Unknown input. Please try again.");
                break;
        }
    }
}