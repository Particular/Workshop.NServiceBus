using System;
using Billing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var endpointName = "Billing";

Console.Title = endpointName;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorkshopEndpoint(endpointName, "host=localhost");

builder.Services.AddSingleton<OrderCalculator>();

await builder.Build().RunAsync();