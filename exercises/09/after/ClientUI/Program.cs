using ClientUI;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var endpointName = "ClientUI";

Console.Title = endpointName;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorkshopEndpoint(endpointName, "host=localhost", c => c.Routing.RouteToEndpoint(typeof(PlaceOrder), "Sales"));

builder.Services.AddHostedService<InputLoopService>();

await builder.Build().RunAsync();