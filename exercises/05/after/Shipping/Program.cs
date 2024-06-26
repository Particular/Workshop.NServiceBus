﻿namespace Shipping
{
    using NServiceBus;
    using System;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main()
        {
            Console.Title = "Shipping";

            var endpointConfiguration = new EndpointConfiguration("Shipping");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.UseSerialization<SystemJsonSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningEventsAs(
                type => type.Namespace != null && type.Namespace.EndsWith(".Events") ||
                        typeof(IEvent).IsAssignableFrom(type)
            );

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}