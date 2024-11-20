# Exercise 9: Monitoring

## Overview

Our endpoints are now production ready, but we're still missing monitoring.

In this exercise you'll learn how to:

- Configure an endpoint to use Platform Monitoring
- Setup a custom check
- Setup heartbeats
- Use ServiceInsight

## Exercise 9.1

Refactor all endpoints to use to a shared configuration class by:

1. Add a new `Shared` project
1. Add references
    <PackageReference Include="NServiceBus" Version="9.*"/>
    <PackageReference Include="NServiceBus.Extensions.Hosting" Version="3.*"/>
    <PackageReference Include="NServiceBus.RabbitMQ" Version="9.*"/>
    <PackageReference Include="NServiceBus.ServicePlatform.Connector" Version="3.*" />
    <PackageReference Include="NServiceBus.SagaAudit" Version="5.*" />
1. Add the `exercises/09/after/Shared/SharedConventions.cs` to the project
1. Add a reference to the shared project to ClientUI, Sales, Billing and Shipping
1. Update all endpoints configure the endpoint using `builder.ConfigureWorkshopEndpoint(endpointName, "host=localhost");`
  - NOTE: Routing needs to be configured for the `ClientUI` using `builder.ConfigureWorkshopEndpoint(endpointName, "host=localhost", c => c.Routing.RouteToEndpoint(typeof(PlaceOrder), "Sales"));`

## Exercise 9.2

Start the platform

### Step 1

1. Make sure that you delete the containers from exercise 6

### Step 2

Start the platform either using SQL Server or RabbitMQ by executing `docker compose up -d` in `/docker/platform-rabbitmq` (or `/docker/platform-sql` if you want to use SQL server)

### Step 3

Access ServicePulse on http://localhost:9090

### Step 4 

1. Start the endpoints
1. Note that all endpoints show up as running

### Step 5

Experiment with adding load via the `ClientUI` and delays in the handlers to see how the graphs on the Monitoring tab changes.

## Exercise 9.3

### Step 1

Turn off [the immediate](https://docs.particular.net/nservicebus/recoverability/configure-immediate-retries#disabling) and [delayed retries](https://docs.particular.net/nservicebus/recoverability/configure-delayed-retries#disabling-through-code) by modifying the shared code

### Step 2

Simulate failures by throwing errors in your message handlers.

### Step 3

Retry failed messages via ServicePulse

## Exercise 9.4

There is a critical external API. The system doesn't process many messages for it but we want the system to be proactively reporting this API is down.

Documentation:

https://docs.particular.net/monitoring/custom-checks/

### Step 1

Add the following custom check code the Billing project:

```c#
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using NServiceBus.Logging;

class ThirdPartyMonitor
{
    const string url = "https://google.com";
    static ILog log = LogManager.GetLogger<ThirdPartyMonitor>();

    public ThirdPartyMonitor()
        : base(
            id: $"Monitor {url}",
            category: "Monitor 3rd Party ",
            repeatAfter: TimeSpan.FromSeconds(10))
    {
    }

    public override async Task<CheckResult> PerformCheck()
    {
        var start = Stopwatch.StartNew();
        try
        {
            if (DateTime.UtcNow.Minute % 2 == 0) throw new InvalidOperationException("Current minute is even so I'm failing.");
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            using (var response = await client.GetAsync(url)
                .ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Succeeded in contacting {url}");
                    return CheckResult.Pass;
                }
                var error = $"Failed to contact '{url}'. HttpStatusCode: {response.StatusCode}";
                log.Info(error);
                return CheckResult.Failed(error);
            }
        }
        catch (Exception exception)
        {
            var error = $"Failed to contact '{url}'. Duration: {start.Elapsed} Error: {exception.Message}";
            log.Info(error);
            return CheckResult.Failed(error);
        }
    }
}
```

### Step 2

Make sure the class inherits from the correct base class as can be found in the [custom check documentation](https://docs.particular.net/monitoring/custom-checks/writing-custom-checks).

### Step 3

Run the endpoints and open *ServicePulse*. Verify how the custom check is reporting its status on the custom check view.


## Exercise 9.4

### Step 1

Setup *ServiceInsight* as instructed on the [Readme of the repo](https://github.com/Particular/Workshop.NServiceBus).

### Step 2

Go to the saga view and check the state of some of the active sagas.
