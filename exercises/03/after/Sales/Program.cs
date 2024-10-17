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
            endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop();
        }
    }
}