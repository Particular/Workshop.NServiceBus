# Exercise 6: Hosting

Important: Before attempting the exercise, please ensure you have followed the instructions for preparing your machine and that you have read the instructions for running the exercise solutions.

## Overview

Now that we understand the fundamentals of NServiceBus it is time to host our solution and make it ready for production!

In this exercise you'll learn:

- How to host an NServiceBus endpoint in production as a windows service
- Configure a non default logging framework and configure it to log outside of the deployment path
- Understand the usage differences of endpoint create and endpoint start
- Make the process compatible with automated deployments

Advanced optional exercises:

- Write errors to the windows event log
- Write the log file to a different path to run least-priviledge

## Exercise 6.1: Hosting as a Windows Service

Endpoints need to be Windows Services so that they can run in the background. We will make use of the generic host to build the windows services.

Documentation:

- https://docs.particular.net/nservicebus/hosting/windows-service#bootstrapping-nuget


### Step 1

Replace the `Program.cs` in the Billing endpoint project with the following code:


```
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Billing";

        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseWindowsService();

        builder.ConfigureLogging((ctx, logging) =>
        {
			//TODO: optionally choose a custom logging library
			// https://docs.particular.net/nservicebus/logging/#custom-logging

            logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));

            logging.AddEventLog();
            logging.AddConsole();
        });

        builder.UseNServiceBus(ctx =>
        {
            var endpointConfiguration = new EndpointConfiguration("Billing");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

			// TODO: optionally choose a different serializer
            // https://docs.particular.net/nservicebus/serialization/
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningEventsAs(
                type => (type.Namespace != null && type.Namespace.EndsWith(".Events"))
            );

            endpointConfiguration.AuditProcessedMessagesTo("audit");

            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            var displayName = System.Net.Dns.GetHostName();
            var identifier = StringToGuid("Billing@" + displayName);

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
}
```

### Step  3

Add an assembly reference to `NServiceBus.Extensions.Hosting` and `Microsoft.Extensions.Hosting.WindowsServices`.

### Step 4

Replace the `new EndpointConfiguration` in the template with a call to `CreateConfiguration()` like the following:

```
var endpointConfiguration = CreateConfiguration();
if (Environment.UserInteractive && Debugger.IsAttached)
{
    endpointConfiguration.EnableInstallers();
}
endpoint = await Endpoint.Start(endpointConfiguration)
    .ConfigureAwait(false);
```

### Step 5

Assign the right value to `Console.Title`.

### Step 6

Add a critical error callback to method `OnCriticalError` to the `CreateConfiguration` function as documented on the following location:

- https://docs.particular.net/nservicebus/hosting/cloud-services-host/critical


### Step 7

Repeat the previous steps for the *Sales* and *Shipping* projects.


## Exercise 6.2: Running installers only during setup/installation

The current endpoints always calls `endpointConfiguration.EnableInstallers()` which is not recommended for production. Processes often need additional permissions to create queues and storage schemas and in production it is prefered to run least privileged.

### Step 1

Open `Program.cs` of Billing.

### Step 2

Remove the line `endpointConfiguration.EnableInstallers()`.

Repeat step 2 and 3 for *Sales* and *Shipping*.


## Exercise 6.2: Override host identifier

NServiceBus adds some diagnostic meta data to each message it send to the audit queue. It stores some hosting info to identify process and this meta data is used in the particular platform tools to identify logical endpoints. The default implementation included the path where the executable is running from. This conflicts when every new deployment is deployed to a new folder. This is commonly done when using automated deployments like Octopus Deploy.

We are using Windows Services, we can safely ignore the path as only a single Windows Service can run for a given machine.


Documentation:

- https://docs.particular.net/nservicebus/hosting/override-hostid


### Step 1

Read the guidance and identify what by default is used to identify an endpoint instance.

### Step 2

Copy the following function to `ProgramService.cs`. The function converts a string to a deterministic guid:

```c#
static Guid StringToGuid(string value)
{
    using (var md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] hash = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(value));
        return new Guid(hash);
    }
}
```

### Step 3

Add the following lines to `CreateConfiguration`

```c#
var displayName = System.Net.Dns.GetHostName();
var identifier = StringToGuid("Billing@" + displayName);
```

### Step 4

Based on the [guidance](https://docs.particular.net/nservicebus/hosting/override-hostid) extend the `endpointConfiguration` with the right calls using the values from Step 3.


## Advanced Exercise: 6.3: Log errors to the Windows Event Log

Serious errors are often written to the Windows Event Log. Since we're already making use of the Microsoft.Extensions.Logging abstraction, we can enable logging to the eventlog with a simple call to:
```
builder.ConfigureLogging((ctx, logging) =>
{
    logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));

    logging.AddEventLog();

});
```

### Step 1

Open a elevated powershell (as admin) and run the following powershell command:

```ps
New-EventLog -LogName Application -Source "RetailDemo"
```

### Step 2

Add the following eventlog section to the`appsettings.json` under the `Logging`-section.

```json
"EventLog": {
    "LogLevel": {
    "Default": "Information"
    }
}
```

### Step 3

Run the endpoint and validate if the events are written the the Windows Application Event Log.


## Advanced Exercise 6.4: Log additional diagnostic data

Some very hard to find issues are exceptions that are handled, but not rethrown. You lose valuable exception information. Usually this happens in code that you control but sometimes this happens in assemblies not maintained by you. There is a way to still capture this information by listening certain appdomain events.

### Step 1

Open the file `Program.cs` from the Billing project.

### Step 2

Add the following lines to log additional errors.

```c#
AppDomain.CurrentDomain.UnhandledException += (sender, args) => LogManager.GetLogger("UnhandledException").Fatal(args.ExceptionObject.GetType().Name, (Exception)args.ExceptionObject);
```
