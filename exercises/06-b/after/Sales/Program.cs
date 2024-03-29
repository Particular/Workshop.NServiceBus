﻿using Messages.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;
using NServiceBus;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    public static void Main(string[] args)
    {
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
            var endpointConfiguration = new EndpointConfiguration("Sales");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Delayed(delayed => delayed.NumberOfRetries(0));

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningEventsAs(
                type =>
                    type == typeof(OrderPlaced)
            );

            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                endpointConfiguration.EnableInstallers();
            }

            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            var displayName = Dns.GetHostName();
            var identifier = StringToGuid("Sales@" + displayName);

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