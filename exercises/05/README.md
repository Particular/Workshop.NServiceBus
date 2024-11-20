# Exercise 5: Message serialization, wire compatibility, and type resolution

## Overview

In this exercise, you will learn:

- how to use messages types that do not use the NServiceBus marker interfaces.
- how to configure a serializer
- how to configure multiple de-serializers to support receives in multiple wire formats

## Exercise 5.1:  Naming conventions

The `Billing` and `Shipping` endpoints need to be subscribed to the `OrderPlaced`-event published by `Sales`.

Useful documentation:

https://docs.particular.net/nservicebus/messaging/unobtrusive-mode

### Step 1

Start all projects, select the `ClientUI` process and send an order. Notice that the `Shipping` endpoint receives the `OrderBilled`-event but not the `OrderPlaced`-event. 

### Step 2

Inspect the message types `OrderBilled` and `OrderPlaced`. Notice that the OrderBilled event type inherits from marker interface `IEvent` and OrderPlaced does not.

### Step 4

Compare the configurations of `Billing` and `Shipping`. Notice that `Billing` has a naming convention defined and `Shipping` does not.

### Step 5

Fix the configuration of `Shipping` so that it will successfully subscribe to the `OrderPlaced` event from `Sales`.

### Step 6

Stop all processes and delete the `.learningtransport` folder, restart all endpoints. Validate if `Shipping` now receives both `OrderBilled` and `OrderPlaced`-events.

### Step 7

Make sure the naming convention for events on the `Shipping` endpoint follows the naming convention for the `OrderPlaced`-event and use the marker interface for the `OrderBilled` event.

## Exercise 5.2: Support multiple wire formats

A system may have to deal with multiple wire formats. This may be required when evolving legacy systems or to connect multiple sub-systes.

The following documentation can be used to help implement the solution.

- https://docs.particular.net/nservicebus/serialization
- https://docs.particular.net/nservicebus/serialization/xml
- https://docs.particular.net/nservicebus/serialization/json
- https://docs.particular.net/nservicebus/serialization/newtonsoft

### Step 1

Run the current solution and verify that the `Billing` endpoint is correctly subscribed to the `Sales` endpoint. Send a few messages through `ClientUI` and notice how they are processed by `Sales`, which then publishes an event, which is processed by `Billing`.

### Step 2

Modify the configuration for the `Billing` endpoint. Configure the endpoint to use the XML serializer.

### Step 3

Run the endpoints and send a few orders via `ClientUI`. Notice that `Billing` now fails to process the events it's subscribed to.

### Step 4

Configure `Billing` to include support for the previous JSON Serializer to deserialize messages.

### Step 5

Run all endpoints and validate if `Billing` now correctly processes messages. Notice that `Billing` now correctly processes messages from `Sales`, but that `Shipping` does not correctly process messages from Billing.

### Step 6

Verify if all endpoints are now sending messages in JSON format and that no processing errors occur. Open the `learningtransport\audit\.bodies` folder and verify that you now see bodies with JSON and XML format. Notice that the order messages in the audit folder will still be in XML. This is important to remember as the same applies to any queue when transitioning to another serializer. Backward compatibility is important.
