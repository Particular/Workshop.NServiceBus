namespace Shared.Configuration;

public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration Configure(
        this EndpointConfiguration endpointConfiguration,
        Action<RoutingSettings<LearningTransport>> configureRouting = null!)
    {
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.Recoverability().Delayed(c => c.NumberOfRetries(0));

        var transport = endpointConfiguration.UseTransport<LearningTransport>();

        var routing = transport.Routing();

        endpointConfiguration.UsePersistence<LearningPersistence>();

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        var conventions = endpointConfiguration.Conventions();

        endpointConfiguration.EnableInstallers();

        configureRouting?.Invoke(routing);

        return endpointConfiguration;
    }
}