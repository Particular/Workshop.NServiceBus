
# Exercise 6: Hosting

Important: Before attempting the exercise, please ensure you have followed the instructions for preparing your machine and that you have read the instructions for running the exercise solutions.

## Overview

Now that we understand the fundamentals of NServiceBus it is time to host our solution and make it ready for production!

In this exercise you'll learn:

- How to host an NServiceBus endpoint in production as background process
- Make the process compatible with automated deployments
- Host endpoints in Docker

## Exercise 6.1: Hosting using Generic Host

These days the common way to host anything is using Microsoft's [Generic Host](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host). Most Microsoft documentation is related to ASP.NET but we can perfectly host NServiceBus endpoints using the Generic Host. It extends capabilities for running NServiceBus endpoints in Docker containers, but we'll get back to that later in this exercise.

On Windows, endpoints need to be Windows Services so that they can run in the background.

Documentation:

- https://docs.particular.net/nservicebus/hosting/
- https://docs.particular.net/nservicebus/dotnet-templates/
- https://docs.particular.net/nservicebus/hosting/windows-service
- https://docs.particular.net/nservicebus/hosting/docker-host/


### Step 1

Read the guidance on the *dotnet templates* and *nuget bootstrapping* for Windows Services.

Create an **empty folder** anywhere on disk and run the following commands. The first command will download and install the Particular templates. Because currently these exercises run on .NET 6.0 and NServiceBus 8.x we need to specify a specific version of the templates:

```
dotnet new install ParticularTemplates::5.*
```
The following command will create a new C# project. The `--framework` option was 
```
dotnet new nsbendpoint --name MyEndpoint --hosting WindowsService --transport LearningTransport --persistence LearningPersistence --framework net8.0
```

Now execute the following commands to change the folder to the project and then add a handler:

```
cd MyEndpoint
dotnet new nsbhandler --name ClassName --messagetype MyMessage
```

Open up the project (`.csproj`) file in Visual Studio or JetBrains Rider to see what the output is.

### Step 2

We already have existing projects in our exercise folder, neither the *dotnet templates* and *nuget bootstrapping* method will work on an existing project but we will use the template as inspiration.

We will start with the `Billing` project and slowly add several lines of code to make it work with the Generic Host and NServiceBus.

### Step  3

Add an assembly reference to `NServiceBus.Extensions.Hosting` version `3.0.0` or later. This references `Microsoft.Extensions.Hosting` and many other NuGet packages needed for the Generic Host in .NET.

### Step 4

Let's have a look at the `Program` class in `Billing` project. We'll modify it in a way so that it's easy to copy over to the other projects. For this reason we start with the following line. This way we can copy the entire content of this file, modify this line to also modify the different endpoints.

```
var endpointName = "Billing";
```

### Step 5

Add the following lines, which make sure we can use `appsettings.json` files, environment variables, command-line paramters, etc. This has nothing to do with NServiceBus, but we'll use it later.

```
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();
```

### Step 6

The following line will use the Generic Host and start it.

```
var host = Host.CreateDefaultBuilder(args).Build();
```

However nothing will be started, we want to use NServiceBus.

```
var host = Host.CreateDefaultBuilder(args)
.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration(endpointName);

    return endpointConfiguration;
}).Build();
```

### Step 7

Create a `Configure()` method that takes our `endpointConfiguration` and adds additional configuration to it. In following exercises, we'll use a similar feature, but in a `Shared` assembly. It contains configuration that is literally used by all our endpoints. For now, we'll create this method and copy the code over to other endpoints in a later step.

```
EndpointConfiguration Configure(
    EndpointConfiguration endpointConfiguration,
    Action<RoutingSettings<LearningTransport>> configureRouting = null!)
{
    var transport = endpointConfiguration.UseTransport<LearningTransport>();
    endpointConfiguration.UsePersistence<LearningPersistence>();
    
    endpointConfiguration.SendFailedMessagesTo("error");
}
```

This  is enough to start our endpoint, but we want a little bit more...

### Step 8

We'll first add a serializer:

```
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
```

Because this is not production code, but an exercise, we don't want messages to continuously be retried with a delay. You might want to figure out how to immediately fail all messages?

```
endpointConfiguration.Recoverability().Delayed(c => c.NumberOfRetries(0));
```

We won't be using audit messages, but you can make it work by adding the following line:

```
endpointConfiguration.AuditProcessedMessagesTo("audit");
```

Let's add conventions.

```
var conventions = endpointConfiguration.Conventions();

conventions.DefiningCommandsAs(n =>
    !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Commands"));
conventions.DefiningEventsAs(n =>
    !string.IsNullOrEmpty(n.Namespace) && n.Namespace.EndsWith("Messages.Events"));
```

Lastly we'll bind any potential routing information:

```
var routing = transport.Routing();
configureRouting?.Invoke(routing);
```

### Step 7

Now in our main code, call this new `Configure()` method:

```
var endpointConfiguration = new EndpointConfiguration(endpointName);
Configure(endpointConfiguration);
```

Should we want to add routing information, we'd use it like this. But we don't need that for Billing, we're only subscribing to messages. But we need a similar approach in later exercises.

```
var endpointConfiguration = new EndpointConfiguration(endpointName);
Configure(endpointConfiguration, r =>
{
    r.RouteToEndpoint(typeof(MyMessage), "MyEndpoint");
});
```

## Exercise 6.2: Running installers only during setup/installation

We could call `endpointConfiguration.EnableInstallers()` but this is not recommended for production. Processes often need additional permissions to create queues and storage schemas and in production it is preferred to run least privileged. Let's see how we can make this optional.

### Step 1

Open `Program.cs` of Billing.

### Step 2

We already set configuration to load environment variables, etc. Let's see if we can create a switch mapping. This has nothing to do with NServiceBus, but it's nice to know anyway. Add the following somewhere at the top of the code:

```
var switchMappings = new Dictionary<string, string>()
{
    { "--ri", "RunInstallers" },
};
```

Unfortunately we can't do `myApplication.exe RunInstallers` as .NET does not support this. We need to do `runInstallers=true`. But with this, we can add a shortcut and use `--ri=true`. We need to add the `switchMappings` as an argument to the `AddCommandLine()` method call.

```
.AddCommandLine(args, switchMappings)
```

Now we can add the command-line argument to this project.

### Step 3

Copy the following code _after_ setting the `ConfigurationBuilder`

```c#
var shouldIRunInstallers = (Environment.UserInteractive && Debugger.IsAttached) ||
                           !string.IsNullOrEmpty(configuration["RunInstallers"]);
```

### Step 4

Now we'll verify if `shouldRunInstallers` is set and potentially call `EnableInstallers()`

```
.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration(endpointName);
    Configure(endpointConfiguration);

    if (shouldIRunInstallers)
    {
        endpointConfiguration.EnableInstallers();
    }
    
    // More code hereafter
}
```


## Exercise 6.3: Override host identifier

NServiceBus add some diagnostical meta data to each message it send to the audit queue. It stores some hosting info to identify process and this meta data is used in the particular platform tools to identify logical endpoints. The default implementation included the path where the executable is running from. This conflicts when every new deployment is deployed to a new folder. This is commonly done when using automated deployments like Octopus Deploy. Or when scaling out using Docker.


Documentation:

- https://docs.particular.net/nservicebus/hosting/override-hostid


### Step 1

Read the guidance and identify what by default is used to identify an endpoint instance.

### Step 2

Copy the following function to `Program.cs` . The function converts a string to a deterministic Guid:

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

Add the following lines to `UseNServiceBus`

```c#
// Get a unique id
var displayName = System.Net.Dns.GetHostName();
var identifier = StringToGuid("Billing@" + displayName);
```

### Step 4

Based on the [guidance](https://docs.particular.net/nservicebus/hosting/override-hostid) extend the `endpointConfiguration` with the right calls using the values from Step 3.

## Exercise 6.4: Critical errors

What happens when critical errors occur? This might occur in one thread of NServiceBus, while other threads are busy processing other messages. We need to try and gracefully shut down the endpoint. This is critical as otherwise we might occur message loss and/or database transactions that might be rolled back that should not happen.

### Step 1

Copy the following code. It tries to shutdown the endpoint and log the exception.

```
async Task OnCriticalError(ICriticalErrorContext context, CancellationToken token)
{
    var fatalMessage = string.Empty;
    try
    {
        fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
        // Try to stop the endpoint.
        // When it is stopped, attempts to send messages will cause an ObjectDisposedException.
        await context.Stop(token);
    }
    finally
    {
        Exit(fatalMessage, context.Exception);
    }
}

void Exit(string failedToStart, Exception exception)
{
    try
    {
        log.Fatal(failedToStart, exception);
    }
    finally
    {
        Environment.FailFast(failedToStart, exception);
    }
}
```

### Step 2

Now we need to add this to our endpoint configuration.

```
endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
```

## Exercise 6.5: Using configuration in Sales and Shipping

Let's copy over the configuration to Sales and Shipping.

### Step 1

Copy over all the code from `Program.cs` from `Billing` into `Sales` and `Shipping`

### Step 2

Rename the endpoints according to the endpoint they are in. As otherwise all endpoints are called `Billing`.

```
var endpointName = "Shipping";
```



## Exercise 6.6: Hosting in Docker containers

NServiceBus endpoints can be hosted in Docker containers.

### Step 1

If you're running .NET SDK 8.0.200 or newer _or_ you're creating a container for a website, you don't have to do anything. In our exercise, we'll first need to add a NuGet package `Microsoft.Net.Build.Containers`.

### Step 2

It's important to gracefully stop the NServiceBus endpoint instance running in the Docker container once it is signaled to shut down. This happens in Linux using a `SIGTERM` signal. Quite similar to `CTRL+C` in Windows console applications.

Change the host using the following code:

```c#
var host = Host.CreateDefaultBuilder(args)
    .UseConsoleLifetime()
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })    
    .UseNServiceBus(context =>    
```

The `UseConsoleLifetime()` makes sure that the `SIGTERM` is provided and the means that the following line, which we already have in our code, will be executed:

``` 
endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
```

The logging statement means logging will occur in the Docker console as well.

> [!NOTE]
>
> If you ever want to create a project using our [templates](https://docs.particular.net/nservicebus/dotnet-templates/), you can do so using the following command:
> `dotnet new nsbendpoint --hosting docker`

### Step 3

We need to make sure a license is included in the Docker container. Edit the `.csproj` file and add the following:

```
    <ItemGroup>
        <None Update="..\..\License.xml" CopyToOutputDirectory="PreserveNewest" />
    </ItemGroup>
```

This copies the included license from the `.\Workshop.NServiceBus\exercises\License.xml` location into the container.

### Step 4

You don't have to execute this, but for reference, we'd build the container images using the following commands:

```
dotnet publish Billing --os linux --arch x64 /t:PublishContainer
dotnet publish Sales --os linux --arch x64 /t:PublishContainer
dotnet publish Shipping --os linux --arch x64 /t:PublishContainer
```

### Step 5

There is a `docker-compose.yml` in the solution folder that can start the containers.

> [!WARNING]
>
> There's currently no point in starting them, they containers would not work

Remember that the endpoints all use the *LearningTransport*. This means the transport will try to find the solution file and create a `.learningtransport` folder there, from which each endpoint in the solution will read its messages. This **will not work** in Docker containers.

If you check the `docker-compose.yml` file, you can see an example where each container depends on the RabbitMQ container.

It could be a nice challenge to get this up and running using RabbitMQ, modifying the `Program.cs` code to support this transport (adding the correct NuGet package and all). But this is not required to complete this exercise.
