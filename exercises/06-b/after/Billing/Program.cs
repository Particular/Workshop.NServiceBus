using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Billing";

        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseWindowsService();

        builder.ConfigureLogging((ctx, logging) =>
        {
            logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));

            logging.AddEventLog();
            logging.AddConsole();

        }).UseNLog();

        builder.UseNServiceBus(ctx =>
        {
            var endpointConfiguration = new EndpointConfiguration("Billing");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            // TODO: optionally choose a different serializer
            // https://docs.particular.net/nservicebus/serialization/
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningEventsAs(
                type => (type.Namespace != null && type.Namespace.EndsWith(".Events"))
            );

            endpointConfiguration.AuditProcessedMessagesTo("audit");

            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                endpointConfiguration.EnableInstallers();
            }

            var displayName = System.Net.Dns.GetHostName();
            var identifier = StringToGuid("Billing@" + displayName);

            var endpointIdentity = endpointConfiguration.UniquelyIdentifyRunningInstance();
            endpointIdentity.UsingCustomDisplayName(displayName);
            endpointIdentity.UsingCustomIdentifier(identifier);

            return endpointConfiguration;
        });

        return builder.ConfigureServices(services => { });

    }

    private static async Task OnCriticalError(ICriticalErrorContext context)
    {
        // https://docs.particular.net/nservicebus/hosting/critical-errors
        var fatalMessage =
            $"The following critical error was encountered:{Environment.NewLine}{context.Error}{Environment.NewLine}Process is shutting down. StackTrace: {Environment.NewLine}{context.Exception.StackTrace}";
        EventLog.WriteEntry(".NET Runtime", fatalMessage, EventLogEntryType.Error);

        try
        {
            await context.Stop().ConfigureAwait(false);
        }
        finally
        {
            Environment.FailFast(fatalMessage, context.Exception);
        }
    }

    private static Guid StringToGuid(string value)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(hash);
        }
    }
}