﻿using NServiceBus.CustomChecks;
using NServiceBus.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

internal class ThirdPartyMonitor : CustomCheck
{
    private const string url = "https://google.com";
    private static readonly ILog log = LogManager.GetLogger<ThirdPartyMonitor>();

    public ThirdPartyMonitor()
        : base(
            $"Monitor {url}",
            "Monitor 3rd Party ",
            TimeSpan.FromSeconds(10))
    {
    }

    public override async Task<CheckResult> PerformCheck()
    {
        var start = Stopwatch.StartNew();
        try
        {
            if (DateTime.UtcNow.Minute % 2 == 0)
                throw new InvalidOperationException("Current minute is even so I'm failing.");
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            using (var response = await client.GetAsync(url).ConfigureAwait(false))
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