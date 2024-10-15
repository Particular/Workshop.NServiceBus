using NServiceBus.Logging;

#pragma warning disable CS0219 // Variable is assigned but its value is never used
var endpointName = "Billing";
#pragma warning restore CS0219 // Variable is assigned but its value is never used

var log = LogManager.GetLogger<Program>();