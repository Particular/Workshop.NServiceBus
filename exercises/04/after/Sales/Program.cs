using Shared.Configuration;

namespace Sales
{
    using NServiceBus;
    using System;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main()
        {
            Console.Title = "Sales";

            var endpointConfiguration = new EndpointConfiguration("Sales");

            endpointConfiguration.Configure();

            // TODO: Wat moet hier mee?
            
            // endpointConfiguration.Recoverability().Immediate(settings => settings.NumberOfRetries(2));
            // endpointConfiguration.Recoverability().Delayed(settings => settings.NumberOfRetries(5));

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}