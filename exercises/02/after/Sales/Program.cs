using NServiceBus;
using System;
using Shared.Configuration;

Console.Title = "Sales";

var endpointConfiguration = new EndpointConfiguration("Sales");

endpointConfiguration.Configure();

var endpointInstance = await Endpoint.Start(endpointConfiguration);

Console.WriteLine("Press Enter to exit.");
Console.ReadLine();

await endpointInstance.Stop();