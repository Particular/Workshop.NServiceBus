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

            endpointConfiguration.UseTransport<LearningTransport>();
            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(c =>
                !string.IsNullOrEmpty(c.Namespace) && c.Namespace.EndsWith("Messages.Commands"));
            conventions.DefiningEventsAs(c =>
                !string.IsNullOrEmpty(c.Namespace) && c.Namespace.EndsWith("Messages.Events"));


            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}