using NServiceBus;
using NServiceBus.Logging;
using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;

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
            // to run interactive from a console or as a windows service
            if (ServiceHelper.IsService())
            {
                Run(service);
                return;
            }

            Console.Title = "Integration";
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
            var endpointConfiguration = new EndpointConfiguration("Integration");

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.UsePersistence<LearningPersistence>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(n =>
                !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
            conventions.DefiningEventsAs(n =>
                !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));

            endpointConfiguration.EnableInstallers();

            endpoint = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
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