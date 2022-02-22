namespace UserRegistration
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Integration";

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
        });

        builder.UseNServiceBus(ctx =>
        {
            var endpointConfiguration = new EndpointConfiguration("Integration");

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();
            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");

            endpointConfiguration.LimitMessageProcessingConcurrencyTo(10);

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
            conventions.DefiningEventsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));

            endpointConfiguration.EnableInstallers();

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
}