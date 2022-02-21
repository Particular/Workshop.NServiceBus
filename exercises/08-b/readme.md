# Exercise 08: Sagas

Users that register themselves in our system, need to confirm their email address using a verification code. If users don't respond to the initial email, we want to send a reminder after 5 seconds. If they don't respond in the next 5 seconds after the reminder is sent, we want the registration to be cancelled and removed.

## Overview

In this exercise you'll learn:

- How to work with sagas.
- How sagas can orchestrate your business processes.
- How sagas should delegate work to handlers.
- How timeouts can provide a solution when no messages arrive any longer.

## Exercise

### Step 1

Verify the setup for `ClientUI` and `UserRegistration` endpoints.

- Notice that the UserRegistration endpoint was set up using our dotnet template. We will later work with the Particular Software dotnet template to add another endpoint.
- Notice that both endpoints work with the `LearningTransport` and `NewtonsoftSerializer`.
- Notice that the ClientUI allows to register either a single user or a batch of users.

### Step 2

We want to send a command from `ClientUI` to `UserRegistration` to register the user.

- Create a new command to register a user in the `UserRegistration.Messages` assembly.
    - Add three properties for User Id of the type `Guid`, a name and an email address.
- In the `ClientUI` endpoint, in the `Program.cs` file, instantiate and send the command.
- Add the [routing](https://docs.particular.net/nservicebus/messaging/routing?version=core_7#command-routing) to make sure the command arrives in the `UserRegistration` endpoint.

### Step 3

We want a Saga to process this command.

- Add the [saga](https://docs.particular.net/nservicebus/sagas/) to process the incoming command to register a new user.
    - It is a good practice to have the class that defines the state of the saga embedded in the saga.
- Send a timeout that will arrive in 5 seconds
    - This will be used later to send the reminder.
    - Upon receiving the timeout, mark the saga as completed. For now we will use this for testing purposes.
    - Timeouts are exclusively used by sagas, so they don't have to be added to the assembly containing all messages.
- Test if this works and the saga is removed after 5 seconds of inactivity.

### Step 4

We want to send an email to the user with a verification code. Instead of the saga doing this, we want to delegate the work. We will send this email from an `Integration` endpoint. This endpoint will be responsible for integration with an SMTP server and later in this exercise a CRM system to actually create the user.

In this step we will add the new endpoint.

- [Read the documentation](https://docs.particular.net/nservicebus/dotnet-templates) to install the Particular Software dotnet templates.
- Install the templates and create and add a new *Windows Service* to the solution. The name of the project should be `Integration`.
- Set up the Integration endpoint so it is able to communicate with the other endpoints, like transport, etc.
- Create an additional assembly for the messages that are owned by the `Integration` endpoint.

### Step 5

We will now create the command and send it to the `Integration` endpoint from the `UserRegistration` endpoint.

- Create a new command. The idea is that other endpoints can use this command to have the `Integration` endpoint send an email with a verification code to the newly registered user.
    - Because the `Integration` endpoint is the owner for this command, it should be placed inside the newly created messages assembly.
    - Add the three properties for User Id of the type `Guid`, a name and an email address. Also add a string property to include the verification code.
    - This message will be a command, because any other endpoint will explicitly tell the Integration endpoint to send an email. This is a well known task for other endpoints. Theoretically, you could also send an email after a `UserRegistered` event, which holds the same properties.

- The saga should send the command immediately after the initial registration command arrives from the user interface. This means it will both request a timeout _and_ send the command.
  The saga will create the verification code that would be added to the email. When the user receives the email and clicks the link, the verification code will be sent to the saga and can be verified.

  Here is some sample code to create a very short verification code. For this exercise we do not need a very long one, but don't use this short one in production:
  ``var verificationCode = Guid.NewGuid().ToString("n").Substring(0, 4);``

- Create a handler in the `Integration` endpoint to process this command.

    - We don't have an SMTP server to send messages. We will find another way to do this in the next step.

- Test and verify if the command arrives at the `Integration` endpoint and is properly processed. It is expected that this takes a few iterations to get right. Try to figure out what is going wrong, because this is a valuable lesson for the future. But also don't hesitate to ask your trainer for help if you can't solve it.

### Step 6

Instead of using an SMTP server, we will notify the ClientUI endpoint about the verification code. It will then be able to use the verification code, to submit it to the saga.

- In the `Integration` endpoint where we just received the command to send an email, publish an event that the email has been sent. Include the `UserId` and the `VerificationCode.`

- In the `ClientUI` endpoint create a handler to receive this event.

    - Use the verification code in the incoming event to send a new command to the saga. This command will be used by the saga to verify the verification code and approve the registration.

    - Use [delayed delivery](https://docs.particular.net/nservicebus/messaging/delayed-delivery) to mimic the fact that an email might take time to arrive. Create a static `Random` object in the handler to create a random number of seconds. It would look like this:

      ```c#
      public class VerificationEmailSentHandler : IHandleMessages<VerificationEmailSent>
      {
          static Random rnd = new Random();

          public Task Handle(VerificationEmailSent message, IMessageHandlerContext context)
          {
              var secondsToWait = rnd.Next(12);
              log.Info($"Received [{message.VerificationCode}] - Waiting {secondsToWait} seconds to respond.");

              // Send command using delayed delivery
          }
      }
      ```

### Step 7

We will now confirm the command in the saga and end it if the verification code matches the one stored in the saga state.

- In the saga process, handle the newly created incoming command from the `ClientUI` endpoint.
- Verify if the verification code matches the one stored in the saga state
- If it matches, send a command to the `Integration` endpoint to create the new user.
- Complete the saga

### Step 8

The current timeout received in the saga doesn't do anything. We need to send another email and force the registration process to expire if no response is received after that.

- In the method that receives the timeout, send a command to email the user a reminder to complete their registration.
- Then, request a new timeout in 5 seconds.
    - Make sure there is a handler to respond to this message, but don't actually send an email. Also don't publish an event like we did with the initial email. This is just to make the entire business process complete.
- Make sure we are able to process the new timeout message.
- Immediately end the saga. Any future incoming messages will be ignored because the saga instnace does not exist anymore.

### Step 9

Imagine the saga is the first to receive the register new user command. The saga should not store the data, because that would add additional work to the transaction and lock resources.

We could theoretically add a new handler based on the same event, but the saga and the new handler would still be executed in the same *Unit of Work*.

We can however immediately publish an event that there is a possible user registration. When it arrives, we would store the information from that event inside our own database.

- Publish a new event with the exact same properties as the incoming command to register a new user.
- Create a handler inside the same UserRegistration endpoint to process this message.
- Output to the console that the message is received and will be stored inside the datastore.
- We don't actually store the data, since we don't have a datastore available.

If you're interested, you can read documentation on how to obtain the connection to the NServiceBus persistence's datastore and use that connection to store your own business data.

- [SQL Persister](https://docs.particular.net/persistence/sql/accessing-data)
- [RavenDb](https://docs.particular.net/persistence/ravendb/#shared-session)
- [Service Fabric](https://docs.particular.net/persistence/service-fabric/transaction-sharing)
- [NHibernate](https://docs.particular.net/persistence/nhibernate/accessing-data)
