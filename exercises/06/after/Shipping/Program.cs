using NServiceBus;
using NServiceBus.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

[DesignerCategory("Code")]
internal class Program
{
    private static readonly ILog logger;
    private IEndpointInstance endpoint;

    static Program()
    {
        LogManager.Use<NLogFactory>();
        NLog.LogManager.Configuration.DefaultCultureInfo = CultureInfo.InvariantCulture;
        logger = LogManager.GetLogger<Program>();

        AppDomain.CurrentDomain.UnhandledException += (sender, ea) =>
            LogManager.GetLogger("UnhandledException")
                      .Fatal(ea.ExceptionObject.GetType().Name, (Exception)ea.ExceptionObject);
        AppDomain.CurrentDomain.FirstChanceException += (sender, ea) =>
            LogManager.GetLogger("FirstChanceException." + ea.Exception.GetType().Name)
                      .Debug(ea.Exception.Message, ea.Exception);
    }

    private static async Task Main(string[] args)
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

        using (var service = new Program())
        {
            // to run interactive from a console or as a windows service
            if (!Environment.UserInteractive)
            {
                Run(service);
                return;
            }

            Console.Title = "Shipping";
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

    private async Task AsyncOnStart()
    {
        try
        {
            var endpointConfiguration = CreateConfiguration();
            if (Environment.UserInteractive && Debugger.IsAttached) endpointConfiguration.EnableInstallers();
            endpoint = await Endpoint.Start(endpointConfiguration)
                                     .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Exit("Failed to start", exception);
        }
    }

    private static void Exit(string failedToStart, Exception exception)
    {
        logger.Fatal(failedToStart, exception);
        //TODO: When using an external logging framework it is important to flush any pending entries prior to calling FailFast
        // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
        NLog.LogManager.Shutdown();

        //TODO: https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
        Environment.FailFast(failedToStart, exception);
    }

    private static Task OnCriticalError(ICriticalErrorContext context)
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

    private static EndpointConfiguration CreateConfiguration()
    {
        var endpointConfiguration = new EndpointConfiguration("Shipping");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        var transport = endpointConfiguration.UseTransport<LearningTransport>();

        endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
        endpointConfiguration.AddDeserializer<XmlSerializer>();

        var conventions = endpointConfiguration.Conventions();
        conventions.DefiningEventsAs(
            type => type.Namespace != null && type.Namespace.EndsWith(".Events") ||
                    typeof(IEvent).IsAssignableFrom(type)
        );

        endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

        var displayName = Dns.GetHostName();
        var identifier = StringToGuid("Shipping@" + displayName);

        var endpointIdentity = endpointConfiguration.UniquelyIdentifyRunningInstance();
        endpointIdentity.UsingCustomDisplayName(displayName);
        endpointIdentity.UsingCustomIdentifier(identifier);

        return endpointConfiguration;
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