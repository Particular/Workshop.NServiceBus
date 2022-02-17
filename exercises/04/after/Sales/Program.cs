using System;
using System.Threading.Tasks;
using NServiceBus;

namespace Sales
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "Sales";

            var endpointConfiguration = new EndpointConfiguration("Sales");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            // endpointConfiguration.Recoverability().Immediate(settings => settings.NumberOfRetries(2));
            // endpointConfiguration.Recoverability().Delayed(settings => settings.NumberOfRetries(5));

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}