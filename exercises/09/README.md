# Exercise 9: Monitoring

## Overview

Our endpoints are now production ready, but we're still missing monitoring and management.

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
1. Update all endpoints configure the endpoint using `builder.ConfigureWorkshopEndpoint(endpointName);`
  - NOTE: Routing needs to be configured for the `ClientUI` using `builder.ConfigureWorkshopEndpoint(endpointName, c => c.Routing.RouteToEndpoint(typeof(PlaceOrder), "Sales"));`

## Exercise 9.2

Start the platform

### Step 1

1. Make sure that you delete the containers from exercise 6 to avoid RabbitMQ urls to clash.

### Step 2

Start the platform either using SQL Server or RabbitMQ by executing `docker compose up -d` in `/docker/platform-rabbitmq`.

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

### Step 1

Simulate monitoring the Maple and Alphine shipping partners in the Shipping endpoints by adding a [Custom Check](https://docs.particular.net/monitoring/custom-checks/) for each one of them.

### Step 2

Run the endpoints and open *ServicePulse*. Verify how the custom check is reporting its status on the custom check view.

### Step 3

Simulate that Maple failes on even minutes and see how the status changes in ServicePulse.

## Exercise 9.4

### Step 1

Setup *ServiceInsight* as instructed on the [Readme of the repo](https://github.com/Particular/Workshop.NServiceBus).

### Step 2

1. Check out sequence diagram view to vizualize the flow of messages from ClientUI all the way to shipping
1. Go to the saga view and check the state of some of the active sagas.
