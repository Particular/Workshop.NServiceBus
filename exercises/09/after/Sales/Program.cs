using System;
using Microsoft.Extensions.Hosting;

var endpointName = "Sales";

Console.Title = endpointName;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorkshopEndpoint(endpointName);

await builder.Build().RunAsync();