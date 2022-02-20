namespace ClientUI
{
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System;
    using System.Diagnostics;
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

                //logging.AddEventLog();
                logging.AddConsole();
            });

            builder.UseNServiceBus(ctx =>
            {
                //     LogManager.Use<NLogFactory>();
                //     NLog.LogManager.Configuration.DefaultCultureInfo = System.Globalization.CultureInfo.InvariantCulture;
                //     log = LogManager.GetLogger<Program>();
                var endpointConfiguration = new EndpointConfiguration("ClientUI");
                endpointConfiguration.AuditProcessedMessagesTo("audit");

                var transport = endpointConfiguration.UseTransport<LearningTransport>();
                endpointConfiguration.SendFailedMessagesTo("error");
                endpointConfiguration.EnableInstallers();

                var routing = transport.Routing();
                routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

                endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
                endpointConfiguration.AddDeserializer<XmlSerializer>();

                return endpointConfiguration;
            });

            return builder.ConfigureServices(services => { services.AddHostedService<Worker>(); });
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
    }
}