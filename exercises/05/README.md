# Exercise 5: Message serialization, wire compatibility, and type resolution

## Overview

In this exercise, you will learn:

- how to use messages types that do not use the NServiceBus marker interfaces.
- how to configure a serializer
- how to configure multiple de-serializers to support receives in multiple wire formats

## Exercise 5.1:  Naming conventions

Change all `.Messages` project to no longer reference NServiceBus using https://docs.particular.net/nservicebus/messaging/unobtrusive-mode as guidance.

NOTE: Alternatively you can also use the `NServiceBus.MessageInterfaces` as described in https://docs.particular.net/samples/message-assembly-sharing/

## Exercise 5.2: Support multiple wire formats

A system may have to deal with multiple wire formats. This may be required when evolving legacy systems or to connect multiple sub-systes.

The following documentation can be used to help implement the solution.

- https://docs.particular.net/nservicebus/serialization
- https://docs.particular.net/nservicebus/serialization/xml
- https://docs.particular.net/nservicebus/serialization/json
- https://docs.particular.net/nservicebus/serialization/newtonsoft

### Step 1

Change ClientUI to use xml to seralize messages.

### Step 2

Start the solution and send a message. Notice that Sales now fails to process the message.

### Step 3

Add xml as an additional serializer.

### Step 4

Run the solution again and to make sure messages can be successfully processed again.
