using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Logging;

var log = LogManager.GetLogger<Program>();

var switchMappings = new Dictionary<string, string>()
{
    { "-ri", "RunInstallers" },
};

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args, switchMappings)
    .Build();

var shouldIRunInstallers = (Environment.UserInteractive && Debugger.IsAttached) || !string.IsNullOrEmpty(configuration["RunInstallers"]);

var host = Host.CreateDefaultBuilder(args)
    .UseNServiceBus(context =>
    {
        var endpointConfiguration = new EndpointConfiguration("Billing");
        Configure(endpointConfiguration);

        // Add another deserializer for backwards compatibility
        endpointConfiguration.AddDeserializer<XmlSerializer>();

        // We don't want to run installers in production, except explicitly stated
        if (shouldIRunInstallers)
        {
            endpointConfiguration.EnableInstallers();
        }

        // For uniquely identifying each instance
        // This is particularly important in cloud scenarios that automatically scale out
        var displayName = System.Net.Dns.GetHostName();
        var identifier = StringToGuid("Billing@" + displayName);
        //
        var endpointIdentity = endpointConfiguration.UniquelyIdentifyRunningInstance();
        endpointIdentity.UsingCustomDisplayName(displayName);
        endpointIdentity.UsingCustomIdentifier(identifier);

        // Make sure critical exceptions are caught
        endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

        return endpointConfiguration;
    }).Build();

var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

Console.Title = hostEnvironment.ApplicationName;

host.Run();

return;

async Task OnCriticalError(ICriticalErrorContext context, CancellationToken token)
{
    var fatalMessage = string.Empty;
    try
    {
        fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
        // Try to stop the endpoint.
        // When it is stopped, attempts to send messages will cause an ObjectDisposedException.
        await context.Stop(token);
    }
    finally
    {
        Exit(fatalMessage, context.Exception);
    }
}

void Exit(string failedToStart, Exception exception)
{
    log.Fatal(failedToStart, exception);
    Environment.FailFast(failedToStart, exception);
}

Guid StringToGuid(string value)
{
    using (var md5 = MD5.Create())
    {
        var hash = md5.ComputeHash(Encoding.Default.GetBytes(value));
        return new Guid(hash);
    }
}

EndpointConfiguration Configure(
    EndpointConfiguration endpointConfiguration,
    Action<RoutingSettings<LearningTransport>> configureRouting = null!)
{
    endpointConfiguration.UseSerialization<SystemJsonSerializer>();
    endpointConfiguration.Recoverability().Delayed(c => c.NumberOfRetries(0));

    var transport = endpointConfiguration.UseTransport<LearningTransport>();

    endpointConfiguration.UsePersistence<LearningPersistence>();

    endpointConfiguration.SendFailedMessagesTo("error");
    endpointConfiguration.AuditProcessedMessagesTo("audit");

    var conventions = endpointConfiguration.Conventions();

    conventions.DefiningCommandsAs(n =>
        !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
    conventions.DefiningEventsAs(n =>
        !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));

    endpointConfiguration.EnableInstallers();

    var routing = transport.Routing();
    configureRouting?.Invoke(routing);

    return endpointConfiguration;
}