using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;
using AutoMapper;
using Integration.Messages.Commands;
using NServiceBus;
using NServiceBus.Logging;
using UserRegistration.Messages.Commands;
using UserRegistration.Messages.Events;

[DesignerCategory("Code")]
class ProgramService : ServiceBase
{
    IEndpointInstance endpoint;

    static ILog logger;

    static ProgramService()
    {
        logger = LogManager.GetLogger<ProgramService>();
    }

    public static void Main()
    {
        using (var service = new ProgramService())
        {
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

    async Task AsyncOnStart()
    {
        try
        {
            var endpointConfiguration = new EndpointConfiguration("UserRegistration");

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            var persistence = endpointConfiguration.UsePersistence<InMemoryPersistence>();
            var transport = endpointConfiguration.UseTransport<MsmqTransport>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            var metrics = endpointConfiguration.EnableMetrics();
            metrics.SendMetricDataToServiceControl("particular.monitoring", TimeSpan.FromSeconds(0.5));
            endpointConfiguration.SendHeartbeatTo(
                serviceControlQueue: "particular.servicecontrol",
                frequency: TimeSpan.FromSeconds(60),
                timeToLive: TimeSpan.FromSeconds(30));

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
            conventions.DefiningEventsAs(n => !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));

            var routing = transport.Routing();
            routing.RegisterPublisher(typeof(UserVerificationStarted), "UserRegistration");
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

    void Exit(string failedToStart, Exception exception)
    {
        logger.Fatal(failedToStart, exception);
        Environment.FailFast(failedToStart, exception);
    }

    void PerformStartupOperations()
    {
        Mapper.Initialize(cfg => cfg.CreateMap<RegisterNewUser, UserVerificationStarted>());
    }

    Task OnCriticalError(ICriticalErrorContext context)
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