using AutoMapper;
using Integration.Messages.Commands;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
using UserRegistration.Messages.Commands;
using UserRegistration.Messages.Events;

[DesignerCategory("Code")]
internal class ProgramService : ServiceBase
{
    private static readonly ILog logger;
    private IEndpointInstance endpoint;

    static ProgramService()
    {
        logger = LogManager.GetLogger<ProgramService>();
    }

    public static void Main()
    {
        using (var service = new ProgramService())
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new Exception("Services only work on Windows");
            }

            // to run interactive from a console or as a windows service
            if (ServiceHelper.IsService())
            {
                Run(service);
                return;
            }

            Console.Title = "UserRegistration";
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
            var endpointConfiguration = new EndpointConfiguration("UserRegistration");

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();
            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            var metrics = endpointConfiguration.EnableMetrics();
            metrics.SendMetricDataToServiceControl("particular.monitoring", TimeSpan.FromSeconds(0.5));
            endpointConfiguration.SendHeartbeatTo(
                "particular.servicecontrol",
                TimeSpan.FromSeconds(60),
                TimeSpan.FromSeconds(30));

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(n =>
                !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
            conventions.DefiningEventsAs(n =>
                !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SendVerificationEmail).Assembly, "Integration");

            endpointConfiguration.EnableInstallers();

            endpoint = await Endpoint.Start(endpointConfiguration)
                                     .ConfigureAwait(false);
            PerformStartupOperations();
        }
        catch (Exception exception)
        {
            Exit("Failed to start", exception);
        }
    }

    private void Exit(string failedToStart, Exception exception)
    {
        logger.Fatal(failedToStart, exception);
        Environment.FailFast(failedToStart, exception);
    }

    private void PerformStartupOperations()
    {
        Mapper.Initialize(cfg => cfg.CreateMap<RegisterNewUser, UserVerificationStarted>());
    }

    private Task OnCriticalError(ICriticalErrorContext context)
    {
        var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
        Exit(fatalMessage, context.Exception);
        return Task.FromResult(0);
    }

    protected override void OnStop()
    {
        endpoint?.Stop().GetAwaiter().GetResult();
    }
}