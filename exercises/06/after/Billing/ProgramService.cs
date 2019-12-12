using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

[DesignerCategory("Code")]
class ProgramService : ServiceBase
{
    IEndpointInstance endpoint;

    static ILog logger;

    static ProgramService()
    {
        LogManager.Use<NLogFactory>();
        if(NLog.LogManager.Configuration==null) throw new Exception("Missing file `NLog.config`");
        NLog.LogManager.Configuration.DefaultCultureInfo = System.Globalization.CultureInfo.InvariantCulture;
        logger = LogManager.GetLogger<ProgramService>();

        AppDomain.CurrentDomain.UnhandledException += (sender, ea) => LogManager.GetLogger("UnhandledException").Fatal(ea.ExceptionObject.GetType().Name, (Exception)ea.ExceptionObject);
        AppDomain.CurrentDomain.FirstChanceException += (sender, ea) => LogManager.GetLogger("FirstChanceException." + ea.Exception.GetType().Name).Debug(ea.Exception.Message, ea.Exception);
    }

    static async Task Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "install")
        {
            await Console.Out.WriteLineAsync("Running installers...")
                .ConfigureAwait(false);
            var endpointConfiguration = CreateConfiguration();
            endpointConfiguration.EnableInstallers();
            await Endpoint.Create(endpointConfiguration)
                .ConfigureAwait(false);
            return;
        }

        using (var service = new ProgramService())
        {
            // to run interactive from a console or as a windows service
            if (!Environment.UserInteractive)
            {
                Run(service);
                return;
            }
            Console.Title = "Billing";
            Console.CancelKeyPress += (sender, e) => { service.OnStop(); };
            service.OnStart(null);
            Console.WriteLine("\r\nPress enter key to stop program\r\n");
            Console.Read();
            service.OnStop();
        }
    }

    protected override void OnStart(string[] args)
    {
        AsyncOnStart().GetAwaiter().GetResult();
    }

    async Task AsyncOnStart()
    {
        try
        {
            var endpointConfiguration = CreateConfiguration();
            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                endpointConfiguration.EnableInstallers();
            }
            endpoint = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Exit("Failed to start", exception);
        }
    }

    static void Exit(string failedToStart, Exception exception)
    {
        logger.Fatal(failedToStart, exception);
        //TODO: When using an external logging framework it is important to flush any pending entries prior to calling FailFast
        // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
        NLog.LogManager.Shutdown();

        //TODO: https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
        Environment.FailFast(failedToStart, exception);
    }

    static Task OnCriticalError(ICriticalErrorContext context)
    {
        // https://docs.particular.net/nservicebus/hosting/critical-errors
        var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
        Exit(fatalMessage, context.Exception);
        return Task.FromResult(0);
    }

    protected override void OnStop()
    {
        endpoint?.Stop().GetAwaiter().GetResult();
    }

    static EndpointConfiguration CreateConfiguration()
    {
        var endpointConfiguration = new EndpointConfiguration("Billing");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        var transport = endpointConfiguration.UseTransport<LearningTransport>();

        endpointConfiguration.UseSerialization<JsonSerializer>();
        endpointConfiguration.AddDeserializer<XmlSerializer>();

        var conventions = endpointConfiguration.Conventions();
        conventions.DefiningEventsAs(
            type => (type.Namespace != null && type.Namespace.EndsWith(".Events"))
            );

        endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

        var displayName = System.Net.Dns.GetHostName();
        var identifier = StringToGuid("Billing@" + displayName);

        var endpointIdentity = endpointConfiguration.UniquelyIdentifyRunningInstance();
        endpointIdentity.UsingCustomDisplayName(displayName);
        endpointIdentity.UsingCustomIdentifier(identifier);

        return endpointConfiguration;
    }

    static Guid StringToGuid(string value)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(value));
            return new Guid(hash);
        }
    }
}
