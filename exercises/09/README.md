# Exercise 9: Monitoring



## Overview

Our endpoints are now production ready but one thing missing which is monitoring. 



In this exercise you'll learn:

- Setup a *ServiceControl* instance
- Setup a *ServiceControl Monitoring* instance 
- Configure an endpoint to use Platform Monitoring
- Setup a custom check
- Setup heartbeats
- How to use ServiceInsight





## Exercise 9.1 

We need to install some components and configure these to use a transport



### Step 1

Setup *ServiceControl* , *ServiceControl Monitoring* , and *ServicePulse* as instructed in the [Monitoring Setup Tutorial](https://docs.particular.net/tutorials/monitoring-setup/) and **select MSMQ as the transport**.

- https://docs.particular.net/tutorials/monitoring-setup/



### Step 2

Enable auditing for the endpoints of the exercise by adding the following to the `endpointConfiguration`:

```c#
endpointConfiguration.AuditProcessedMessagesTo("audit");
```



### Step 3

Add and enable the NServiceBus monitoring plugin as instructed in the tutorial:

- https://docs.particular.net/tutorials/monitoring-setup/#configure-nservicebus-endpoints-monitoring

### Step 4

The reporting interval is currently 2 seconds. Set this to 0.5 seconds for testing and demo purposes.

### Step 5

Look at the dashboard and identify which message endpoint and more specifically type has performance issues.

### Step 6

Write down 4 ways to increase the throughput besides optimizing the handler in `notes.txt`

### Step 4

Increase the maximum concurrency of the processing endpoint to 10.

### Step 5

Verify that *Critical time* of the endpoint is actually decreasing.



## Exercise 9.2 

NOTE: This exercise assumed that ServiceInsight is already installed. See the [prerequisites](../../README.md#preparing-your-machine-for-the-workshop) for details.

### Step 1

Launch *ServiceInsight*.

### Step 2

In the search enter message name *SendVerificationReminderEmail* that was slowly processed in the previous exercise.

### Step 3

Select any of the returned messages and see how it affects the latency of the process.



## Exercise 9.3

There is a critical external API. The system doesn't process many messages for it but we want the system to be proactively reporting this API is down.

Documentation:

- https://docs.particular.net/monitoring/custom-checks/



### Step 1

Open project *Integration*.

### Step 2

Add the Nuget package `NServiceBus.CustomChecks` version **2.x.x**.

### Step 3

Add a reference to `System.Net.Http` 

### Step 4

Add the following custom check code the project

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

Make sure the class inherits from the correct base class as can be found in the custom check documentation.

### Step 6

Run the endpoints and open *ServicePulse*, see how the custom check is reporting its status on the custom check view.



## Exercise 9.4



Documentation:

- https://docs.particular.net/monitoring/heartbeats/



### Step 1

Add the package `NServiceBus.Heartbeat` version **2.x.x* to all endpoints.

### Step 2

Extend the endpoint configuration with the following heartbeat configuration.

```c#
endpointConfiguration.SendHeartbeatTo(
    serviceControlQueue: "particular.servicecontrol",
    frequency: TimeSpan.FromSeconds(60),
    timeToLive: TimeSpan.FromSeconds(30));
```

### Step 3

Verify if heartbeats are being send to ServiceControl by opening ServicePulse and see if the endpoints are listed on the heartbeats screen.

### Step 4

The heartbeat frequency is set to a high value which results in the endpoint to become in a faulty state. Increase the *heartbeat graceperiod* to 2 minutes based on the following documentation;

- https://docs.particular.net/servicecontrol/creating-config-file#plugin-specific-servicecontrolheartbeatgraceperiod



NOTE: We increase the graceperiod to 2 minutes because we want to endpoint to first rely on the hosting environment to perform recovery. If a Windows Service crashes then Windows by default will restart the process after 1 minute. Only when an operator is really required to analyze the issue we want the endpoint to be shown as not running.
