using Microsoft.Extensions.Hosting;

public record Customizations(EndpointConfiguration EndpointConfiguration, RoutingSettings Routing);

public static class SharedConventions
{
    public static HostApplicationBuilder ConfigureWorkshopEndpoint(this HostApplicationBuilder builder, string endpointName, Action<Customizations>? customize = null)
    {
        var endpointConfiguration = new EndpointConfiguration(endpointName);

        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), "host=localhost");
        var routing = endpointConfiguration.UseTransport(transport);

         endpointConfiguration.UsePersistence<LearningPersistence>();

        SetCommonEndpointSettings(endpointConfiguration);

        // Endpoint-specific customization
        customize?.Invoke(new Customizations(endpointConfiguration, routing));

        builder.UseNServiceBus(endpointConfiguration);
        return builder;
    }
    
    static void SetCommonEndpointSettings(EndpointConfiguration endpointConfiguration)
    {
        // disable diagnostic writer to prevent docker errors
        // in production each container should map a volume to write diagnostic
        endpointConfiguration.CustomDiagnosticsWriter((_, _) => Task.CompletedTask);
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.EnableInstallers();
        
        endpointConfiguration.ConnectToServicePlatform(new ServicePlatformConnectionConfiguration
        {
            Heartbeats = new()
            {
                Enabled = true,
                HeartbeatsQueue = "Particular.ServiceControl",
            },
            CustomChecks = new()
            {
                Enabled = true,
                CustomChecksQueue = "Particular.ServiceControl"
            },
            ErrorQueue = "error",
            SagaAudit = new()
            {
                Enabled = true,
                SagaAuditQueue = "audit"
            },
            MessageAudit = new()
            {
                Enabled = true,
                AuditQueue = "audit"
            },
            
            Metrics = new()
            {
                Enabled = true,
                MetricsQueue = "Particular.Monitoring",
                Interval = TimeSpan.FromSeconds(1)
            }
        });
    }
}