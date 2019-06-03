# Exercise 5: Message serialization, wire compatibility and type resolution

Important: Before attempting the exercise, please ensure you have followed the instructions for preparing your machine and that you have read the instructions for running the exercise solutions.

## Overview

In this exersice you will learn how to configure a serializer, how to configure multiple serializer in order to receive multiple wire formats and how you can use messages types that do not use the NServiceBus marker interfaces so that you can use POCO's.

## Exersize 5.1:  Naming conventions

The Billing and Shipping endpoints needs to be subscribe to the `OrderPlaced` event published by Sales.

Useful documentation:

https://docs.particular.net/nservicebus/messaging/unobtrusive-mode

### Step 1

Run all solutions, select the ClientUI process and send an order. Notice that the Shipping endpoint receives the OrderBilled event but not the OrderPlaced event. 

### Step 2

Inspect the messages types OrderBilled and OrderPlaced. Notice that the OrderBilled event type inherits from marker interface `IEvent` and OrderPlaced does not.

###  Step 4

Compare the configurations of Billing and Shipping. Notice that Billing has a naming convention defined and Shipping does not.

### Step 5

Fix the configuration of Shipping so that it will succesfully subscribe to the OrderPlaced event from Sales.

### Step 6

Stop all processes and delete the `.learningtransport` folder, restart all endpoints. Validate if Shipping now receives both OrderBilled and OrderPlaced event.

### Step 7

Make sure the naming convention for events on the Shipping endpoint supports both the naming convention for OrderPlaced but also the marker interface for the OrderBilled event.


## Exercise 5.2: Support multiple wire formats

A system very often has to deal with multiple wire formats. Either because of legacy or because or combining systems.

The following documentation can be used to help implement the solution.

- https://docs.particular.net/nservicebus/serialization
- https://docs.particular.net/nservicebus/serialization/xml
- https://docs.particular.net/nservicebus/serialization/json
- https://docs.particular.net/nservicebus/serialization/newtonsoft


### Step 1

Run the current solution and see that the Billing is correctly subscribed to Sales. Send a few messages via ClientUI and notice how they are processed by Sales, which then publishes an event, which is processed by Billing.

### Step 2

Modify the configuration of Billing. Configure the endpoint to use the JSON serializer;

### Step 3

Run the endpoints and send a few orders via ClientUI. Notice that Billing now receives messages that it fails to process

### Step 4

Configure Billing to also support the previous XML Serializer to deserialize messages.

### Step 5

Run all endpoints and validate if Billing now correctly processes messages. Notice that Billing now correctly processes messages from Sales, but that Shipping does not correctly process messages from Billing.

### Step 5

Upgrade all endpoints to now use JSON as the default message format but make sure that they still support XML for old messages.

### Step 6

Verify if all endpoints are now sending messages in JSON format and that no processing errors occur. Open the `learningtransport\audit\.bodies` folder and see that you now have bodies with JSON and XML format. Notice that the order messages audit  will still be in XML. This is important to remember as the same applies to any queue when transitioning to another serializer. Backwards compatibility is important.

