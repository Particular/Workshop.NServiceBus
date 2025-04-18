using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Logging;
using Shared;

var log = LogManager.GetLogger<Program>();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var host = Host.CreateDefaultBuilder(args)
    .UseNServiceBus(context =>
    {
        var endpointConfiguration = new EndpointConfiguration("Billing.Regular");
        endpointConfiguration.Configure();
        endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

        return endpointConfiguration;
    }).Build();

var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

Console.Title = hostEnvironment.ApplicationName;

host.Run();

return;

Task OnCriticalError(ICriticalErrorContext context, CancellationToken token = default)
{
    var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
    Exit(fatalMessage, context.Exception);
    return Task.FromResult(0);
}

void Exit(string failedToStart, Exception exception)
{
    log.Fatal(failedToStart, exception);
    Environment.FailFast(failedToStart, exception);
}