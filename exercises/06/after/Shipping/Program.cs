using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;
using System.Threading.Tasks;

Console.Title = "Shipping";

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration("Shipping");

endpointConfiguration.UseSerialization<SystemJsonSerializer>();

endpointConfiguration.UseTransport(new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), "host=rabbitmq"));
endpointConfiguration.CustomDiagnosticsWriter((_, __) => Task.CompletedTask);
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

await builder.Build().RunAsync();