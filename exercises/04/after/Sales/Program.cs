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
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            endpointConfiguration.Recoverability().Immediate(s => s.NumberOfRetries(2));
            endpointConfiguration.Recoverability().Delayed(s =>
            {
                s.NumberOfRetries(2);
                s.TimeIncrease(TimeSpan.FromSeconds(10));
            });

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}