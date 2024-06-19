# Exercise 3: Publishing events

Follow the steps on: https://docs.particular.net/tutorials/nservicebus-step-by-step/4-publishing-events/

What changed?

- Created 2 new endpoints: Billing and Shipping
- Start both endpoints
- Modify the PlaceOrderHandler to publish an event
- Subscribe to the event from Billing
- Publish an event when the order is billed
- Subscribe to the OrderPlaced and OrderBilled event in Shipping
