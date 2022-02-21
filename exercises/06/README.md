
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
- Write the log file to a different path to run least-privilege

## Exercise 6.1: Hosting as a Windows Service

Endpoints need to be Windows Services so that they can run in the background.

Documentation:

- https://docs.particular.net/nservicebus/dotnet-templates#nservicebus-endpoint-windows-service
- https://docs.particular.net/nservicebus/hosting/windows-service#bootstrapping-nuget


### Step 1

Read the guidance on the *dotnet templates* and *nuget bootstrapping* for Windows Services.

### Step 2

We already have existing projects, neither the *dotnet templates* and *nuget bootstrapping* method will work on an existing project but we will use the template as inspiration.

Copy the `ProgramService.cs` into the Billing endpoint project:


```
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
        //TODO: optionally choose a custom logging library
        // https://docs.particular.net/nservicebus/logging/#custom-logging
        // LogManager.Use<TheLoggingFactory>();
        logger = LogManager.GetLogger<ProgramService>();
    }

    public static void Main()
    {
        using (var service = new ProgramService())
        {
            // to run interactive from a console or as a windows service
            if (!Environment.UserInteractive)
            {
                Run(service);
                return;
            }
            Console.Title = "NServiceBusWindowsService";
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
            var endpointConfiguration = new EndpointConfiguration("NServiceBusWindowsService");

            //TODO: optionally choose a different serializer
            // https://docs.particular.net/nservicebus/serialization/
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            //TODO: this is to prevent accidentally deploying to production without considering important actions
            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                //TODO: For production use select a durable transport.
                // https://docs.particular.net/transports/
                endpointConfiguration.UseTransport<LearningTransport>();

                //TODO: For production use select a durable persistence.
                // https://docs.particular.net/persistence/
                endpointConfiguration.UsePersistence<LearningPersistence>();

                //TODO: For production use script the installation.
                endpointConfiguration.EnableInstallers();
            }
            endpoint = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            PerformStartupOperations();
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

        //TODO: https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
        Environment.FailFast(failedToStart, exception);
    }

    void PerformStartupOperations()
    {
        //TODO: perform any startup operations
    }

    static Task OnCriticalError(ICriticalErrorContext context)
    {
        //TODO: Decide if shutting down the process is the best response to a critical error
        // https://docs.particular.net/nservicebus/hosting/critical-errors
        var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
        Exit(fatalMessage, context.Exception);
        return Task.FromResult(0);
    }

    protected override void OnStop()
    {
        endpoint?.Stop().GetAwaiter().GetResult();
        //TODO: perform any shutdown operations
    }
}
```

### Step  3

Add an assembly reference to `System.ServiceProcess`.

### Step 4

Move the `CreateConfiguration` method from `Program.cs` to `ProgramService.cs`.

### Step 5

Remove `Program.cs.`

### Step 6

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

### Step 7

Assign the right value to `Console.Title`.

### Step 8

Add a critical error callback to method `OnCriticalError` to the `CreateConfiguration` function as documented on the following location:

- https://docs.particular.net/nservicebus/hosting/cloud-services-host/critical


### Step 7

Repeat the previous steps for the *Sales* and *Shipping* projects.



## Exercise 6.2: Running installers only during setup/installation

The current endpoints always calls `endpointConfiguration.EnableInstallers()` which is not recommended for production. Processes often need additional permissions to create queues and storage schemas and in production it is prefered to run least privileged.

### Step 1

Open `ProgramService.cs` of Billing.

### Step 2

Remove the line `endpointConfiguration.EnableInstallers()`.

### Step 3

Copy the following code to the start of the `Main` method.

```c#
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
```

### Step 4

Repeat step 2 and 3 for *Sales* and *Shipping*.


## Exercise 6.2: Configure a custom logging framework

The default logging feature only logs to a file and does not support filtering. In this exercise we are going to configure our endpoints to use **NLog**, and write log entries to a file on a path different then the deployed executable.

It is important that the logging framework is configured before *any* NServiceBus code is invoked or else the default logger will be used.

### Step 1

Add nuget package `NServiceBus.NLog` version 2.x.x at the solution level to all projects.

### Step 2

Open the `ProgramService.cs` from *Billing*.

### Step 2

Add the following code to the correct location in the currently opened `ProgramService.cs`:

```c#
NServiceBus.Logging.LogManager.Use<NLogFactory>();
NLog.LogManager.Configuration.DefaultCultureInfo = CultureInfo.InvariantCulture;
```

### Step 3

Run the Billing endpoint and notice startup fails. This is because NLog cannot find its configuration.

### Step 4

Create an `App.config` and add the following NLog related snippets to the configuration.

```xml
<section name="nlog" type="NLog.Config.ConfigSectionHandler, NLog" />
```

```xml
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <targets async="true">
    <target name="file" xsi:type="File" fileName="trace.log" layout="${longdate:universalTime=true}|${level:uppercase=true}|${threadid:padding=2}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}" />
    <target name="console" xsi:type="ColoredConsole" layout="${longdate}|${level:uppercase=true}|${threadid:padding=2}|${logger}|${message}${onexception:${exception:format=message}}" />
  </targets>
  <rules>
    <logger name="*" minlevel="Debug" writeTo="console" />
    <logger name="*" minlevel="Info" writeTo="file" />
  </rules>
</nlog>
```

### Step 5

Run the Billing endpoint, you should now see Debug output to the console. Also check the bin folder and see if it contains a file called `trace.log`.


### Step 6

NLog is configured to write log events asynchronously. We need to make sure that NLog gets the opportunity to flush all its log events when endpoint is terminated or else we risk to loose potentially important information of the last few things that happened.

Open `ProgramService.cs` and add the following line to the `Exit` method:

```c#
NLog.LogManager.Shutdown();
```

## Exercise 6.3: Override host identifier

NServiceBus add some diagnostical meta data to each message it send to the audit queue. It stores some hosting info to identify process and this meta data is used in the particular platform tools to identify logical endpoints. The default implementation included the path where the executable is running from. This conflicts when every new deployment is deployed to a new folder. This is commonly done when using automated deployments like Octopus Deploy.

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


## Advanced Exercise: 6.4: Log errors to the Windows Event Log

Serious errors are often written to the Windows Event Log. We are already using NLog for logging and now configure it to log to the Windows Event Log.

### Step 1

Open a elevated powershell (as admin) and run the following powershell command:

```ps
New-EventLog -LogName Application -Source "RetailDemo"
```

### Step 2

Add the following NLog target to the NLog section of the existing `App.config`.

```xml
<target name="eventlog" xsi:type="EventLog"  layout="${logger}|${message}${onexception:${newline}${exception:format=tostring}}" source="RetailDemo" log="Application"/>
```

### Step 3

Add the following logger to the NLog section:

```xml
<logger name="*" minlevel="Debug" writeTo="eventlog" />
```

### Step 4

Run the endpoint and validate if the events are written the the Windows Application Event Log.


## Advanced Exercise 6.5: Log additional diagnostical data

Some very hard to find issues are exceptions that are handled, but not rethrown. You loose valuable exception information. Usually this happens in code that you control but sometimes this happens in assemblies not maintained by you. There is a way to still capture this information by listening certain appdomain events.

### Step 1

Open the file `ProgramService.cs` from the Billing project.

### Step 2

Add the following lines to log additional errors.

```c#
AppDomain.CurrentDomain.UnhandledException += (sender, args) => LogManager.GetLogger("UnhandledException").Fatal(args.ExceptionObject.GetType().Name, (Exception)args.ExceptionObject);
AppDomain.CurrentDomain.FirstChanceException += (sender, args) => LogManager.GetLogger("FirstChanceException." + args.Exception.GetType().Name).Debug(args.Exception.Message, args.Exception);
```

### Step 3

Add the following log filter to disable the `FirstChanceException` logger:

```xml
<logger name="FirstChanceException.*" minlevel="Debug" final="true" />
```

## Advanced Exercise 6.6: Do not log to application path

In a production environment you would not log to the path where your application assemblies are deployed. The default logger has a `.Directory(string)` method to override the path but we can do something similar with NLog

Documenation:

 - https://docs.particular.net/nservicebus/logging/

### Step 1

Open `App.config` from Billing.

### Step 2

Replace the `filename` attribute of the `file` target with the following value which writes the log file to the TEMP path:

```
${environment:TEMP}\retaildemo\billing.log
```

### Step 3

Verify that the log file is create in the TEMP path, in a production environment the process only requires read permissions on the location where the endpoint executable is deployed.
