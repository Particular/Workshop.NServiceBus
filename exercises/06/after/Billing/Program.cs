using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus;

Console.Title = "Billing";

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration("Billing");

endpointConfiguration.UseSerialization<SystemJsonSerializer>();

endpointConfiguration.UseTransport(new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), "host=rabbitmq"));
endpointConfiguration.CustomDiagnosticsWriter((_, __) => Task.CompletedTask);
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

await builder.Build().RunAsync();