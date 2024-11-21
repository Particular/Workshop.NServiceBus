using Microsoft.Extensions.Hosting;
using System;

var endpointName = "Shipping";

Console.Title = endpointName;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorkshopEndpoint(endpointName);

await builder.Build().RunAsync();