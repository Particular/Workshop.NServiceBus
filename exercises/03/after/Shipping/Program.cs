namespace Shipping
{
    using NServiceBus;
    using System;
    using System.Threading.Tasks;
    using Shared.Configuration;

    internal class Program
    {
        private static async Task Main()
        {
            Console.Title = "Shipping";

            var endpointConfiguration = new EndpointConfiguration("Shipping");

            endpointConfiguration.Configure();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}