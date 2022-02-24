# Exercise 9: Monitoring

## Overview

Our endpoints are now production ready, but we're still missing monitoring.

In this exercise you'll learn how to:

- Configure an endpoint to use Platform Monitoring
- Setup a custom check
- Setup heartbeats
- Use ServiceInsight

## Exercise 9.1

We need to install some components and configure these to use a transport.

### Step 1

Setup *ServiceInsight* as instructed on the [Readme of the repo](https://github.com/Particular/Workshop.NServiceBus).

To set up the Platform tools from scratch without the Platform connection package, follow the documentation:

- https://docs.particular.net/tutorials/monitoring-setup/

### Step 2

Enable auditing for the endpoints of the exercise by adding the following to the `endpointConfiguration`:

```c#
endpointConfiguration.AuditProcessedMessagesTo("audit");
```

### Step 3

Add and enable the NServiceBus monitoring plugin as instructed in the tutorial:

https://docs.particular.net/tutorials/monitoring-setup/#configure-nservicebus-endpoints-monitoring

### Step 4

The reporting interval is currently set to 2 seconds. Set this to 0.5 seconds for testing and demo purposes.

### Step 5

Look at the dashboard and identify which endpoint, and more specifically which message type, has performance issues.

### Step 6

Write down 4 ways to increase the throughput besides optimizing the handler in `notes.txt`.

### Step 7

Increase the maximum concurrency of the processing endpoint to 10.

### Step 8

Verify that *Critical time* of the endpoint is actually decreasing.


## Exercise 9.2

NOTE: This exercise assumes that ServiceInsight is installed. See the [prerequisites](../../README.md#preparing-your-machine-for-the-workshop) for details.

### Step 1

Launch *ServiceInsight*.

### Step 2

In the search box, enter message name *SendVerificationReminderEmail*.

### Step 3

Select any of the returned messages and see how it affects the latency of the endpoint.


## Exercise 9.3

There is a critical external API. The system doesn't process many messages for it but we want the system to be proactively reporting this API is down.

Documentation:

https://docs.particular.net/monitoring/custom-checks/

### Step 1

Open project *Integration*.

### Step 2

Add the Nuget package `NServiceBus.CustomChecks` version **2.x.x**.

### Step 3

Add a reference to `System.Net.Http`.

### Step 4

Add the following custom check code the project:

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

### Step 5

Make sure the class inherits from the correct base class as can be found in the [custom check documentation](https://docs.particular.net/monitoring/custom-checks/writing-custom-checks).

### Step 6

Run the endpoints and open *ServicePulse*. Verify how the custom check is reporting its status on the custom check view.


## Exercise 9.4

Documentation:

https://docs.particular.net/monitoring/heartbeats/

### Step 1

Add the package `NServiceBus.Heartbeats` version **2.x.x* to all endpoints.

### Step 2

Extend the endpoint configuration with the following heartbeat configuration:

```c#
endpointConfiguration.SendHeartbeatTo(
    serviceControlQueue: "particular.servicecontrol",
    frequency: TimeSpan.FromSeconds(60),
    timeToLive: TimeSpan.FromSeconds(30));
```

### Step 3

Verify if heartbeats are being sent to ServiceControl by opening ServicePulse and checking whether the endpoints are listed on the Heartbeats screen.

### Step 4

The heartbeat frequency is set to a high value which makes the endpoint appear in a faulted state. Increase the *heartbeat graceperiod* to 2 minutes based on the following documentation:

https://docs.particular.net/servicecontrol/creating-config-file#plugin-specific-servicecontrolheartbeatgraceperiod

NOTE: We increased the graceperiod to 2 minutes because we want the endpoint to rely on the hosting environment for recovery. If a Windows Service crashes, Windows will by default restart the process after 1 minute. We only want an operator to analyze the issue when the service is unable to restart. By setting the graceperiod to 2 minutes, we're ignoring any service restarts from showing up as missing heartbeats.
