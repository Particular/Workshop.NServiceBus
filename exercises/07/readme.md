# Exercise 07 : Priority Queues

In this exercise we'll investigate a different way of mimicking priority queues through publish/subscribe. This way the publisher is not aware of a message, customer or anything being either a regular or a priority message. Two (or more) receiving endpoints will decide if the incoming message is a regular or priority message.

For endpoints that have to deal with very high throughput, this may not be the best solution, since both endpoints will receive each message separately. The main benefit we gain here, is the loose coupling. The business requirements of Billing are not leaked into the Sales endpoint. An additional benefit is that, since they each have their own queue, they can be scaled out independently, etc.

In the end it's always a trade-off. Each scenario is unique though and needs to be considered separately.

## Overview

In this exercise we'll differentiate between regular customers and strategic customers and deal with each in their own endpoint. However, we want the sending endpoint to be agnostic of any differentiation that needs to be made based on customer-specific information.

You'll learn:

- How to set up two endpoints that are almost identical but serve a different process
- Use Publish/Subscribe differently than how we've learned so far

## Exercise 7.1

### Step 1

- In the solution check the `Sales` endpoint, specifically the class `PlaceOrderHandler`.
  - It receives a `PlaceOrder` command.
  - It immediately publishes an `OrderPlaced` event.
    For the Billign endpoint, it matters whether the order that was placed was for a *regular* or a *strategic* customer. As you can tell, the `Sales` endpoint has no knowledge of this.

### Step 2

- Check the `Billing` endpoint, specifically the `OrderPlacedHandler`.
  - It subscribed to the previously published `OrderPlaced` event.
  - At this moment it does not differentiate between *regular* or *strategic* customers.

### Step 3

- On the file system, make a copy of the `Billing` endpoint.
  - Conveniently, on the file system the folder is already named `Billing.Regular`.
  - The new folder should be named `Billing.Strategic`.
- In Visual Studio first rename the current `Billing` project to `Billing.Regular`.
  - Otherwise loading the other project will fail due to name duplication.
- In Visual Studio now add the newly copied project to your solution.
  - It will be called `Billing`.
- Rename the newly added project to `Billing.Strategic`.

### Step 4

Now both projects will automatically subscribe to the `OrderPlaced` event and process the message in exactly the same way. After all they are copies of each other.

We need to make sure both differentiate between *regular* and *strategic* customers.

- There is another project in the solution called `Billing.Shared`. Reference it from both `Billing.Regular` and `Billing.Strategic`.
- In the handler, create a static `List<int>` called `strategicCustomers` and fill it by calling `Customers.GetStrategicCustomers()` from the added project.
  - This way the list is only retrieved once. In memory it's lightning fast, but if we retrieve this list from a datastore, we do not want to load the list every single time.
- Open the `OrderPlacedHandler` in `Billing.Regular`.
- Using `strategicCustomers.Contains(message.CustomerId)`, check if the incoming message contains a customerId that is strategic.
- If it is indeed a strategic customer, ignore this message inside this handler.
  - Perhaps log that we are ignoring this customer.

The result should be that the handler logs when it is ignoring a message, or log with the line of code that was already there that the message was received with a specific `OrderId`.

### Step 5

- Do the same for `Billing.StrategicCustomer`, but now ignore all messsages that were placed by regular customers.

### Step 6

- In the `ClientUI` project, open the class `Program` and find out what happens if you press the `P` key.
- This should result in the creation of a random CustomerId between 0 and 10, which is then used to send the `PlaceOrder` message to `Sales` and set everything in motion.

### Step 7

Whenever a CustomerId is either 3, 4 or 5 -according to Billing.Shared-, the customer should be ignored by the `Billing.Regular` endpoint and processed by `Billing.Strategic`, or vice versa.

Verify this behavior by paying attention to the output in the console window for both endpoints.

## Conclusion

We've seen an alternative way of dealing with priority queues, where the sending endpoint is completely unaware of *regular* or *strategic* customers.

We used 3 customer identifiers (`CustomerId`) from `Billing.Shared` to decide whether a message contained a *regular* or *strategic* customer. If this list is either too large or takes too long to load, we might need to load the customer's information to figure out their status. This puts quite a load on the database, as both endpoints will independently retrieve this information. They are unaware of each other.

Besides the point that there are ways to solve this, the *main idea* here is that you have options.

- Particular Software gets a lot of questions why a single endpoint cannot have two queues, to separate important messages from less important messages. We don't think that is the solution.
- With different logical (and physical) endpoints we have options to scale up/out per endpoint.
  - We could decide to scale out the strategic customer endpoint to give it more processing power.
  - We could decide to scale out the regular customer endpoint. The strategic customer endpoint might occasionally process a customer, but 98% of the messages are processed by the regular customer endpoint.
  - We might have different types of datastores for each endpoint. Different Service Level Agreement (SLA) for each endpoint, where we get warnings in our monitoring software when the strategic endpoint reaches a certain threshold. While with the regular endpoint this is less of an issue.
  - We could decide to use more/other/different ways to split up the processing of these messages.

We hope you learned that with messaging, handlers and endpoints you obtain new ways of dealing with data flowing through your system on multiple levels.
