﻿namespace Sales
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
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}