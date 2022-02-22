using Messages;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

class Worker : BackgroundService
{
    private readonly IMessageSession messageSession;
    private static ILog log = LogManager.GetLogger<Worker>();

    public Worker(IMessageSession messageSession)
    {
        this.messageSession = messageSession;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
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
                        await messageSession.Send(command).ConfigureAwait(false);

                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
    }
}