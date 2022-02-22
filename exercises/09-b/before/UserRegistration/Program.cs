namespace UserRegistration
{
    using AutoMapper;
    using Integration.Messages.Commands;
    using Messages.Commands;
    using Messages.Events;
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
        Console.Title = "UserRegistration";

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
            var endpointConfiguration = new EndpointConfiguration("UserRegistration");

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();
            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
            conventions.DefiningEventsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SendVerificationEmail).Assembly, "Integration");

            endpointConfiguration.EnableInstallers();

            PerformStartupOperations();

            return endpointConfiguration;
        });

        return builder.ConfigureServices(services => { });

    }

    private static void PerformStartupOperations()
    {
        Mapper.Initialize(cfg => cfg.CreateMap<RegisterNewUser, UserVerificationStarted>());
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