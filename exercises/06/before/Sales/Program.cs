﻿using System;
using System.Threading.Tasks;
using Messages.Events;
using NServiceBus;

namespace Sales
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "Sales";

            var endpointConfiguration = CreateConfiguration();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }

        static IEndpointConfiguration CreateConfiguration()
        {
            var endpointConfiguration = new EndpointConfiguration("Sales");
            endpointConfiguration.ForwardReceivedMessagesTo("audit");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Delayed(delayed => delayed.NumberOfRetries(0));

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningEventsAs(
                type =>
                type == typeof(OrderPlaced)
                );

            return endpointConfiguration;
        }
    }
}