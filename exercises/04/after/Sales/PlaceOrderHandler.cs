namespace Sales
{
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using System;
    using System.Threading.Tasks;

    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private static readonly ILog log = LogManager.GetLogger<PlaceOrderHandler>();
        private static Random random = new Random();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            // Uncomment to test a systemic exception
            // throw new Exception("BOOM");

            // Uncomment to test a transient exception
            // if (random.Next(0, 5) == 0)
            // {
            //     throw new Exception("Oops");
            // }

            log.Info($"Received PlaceOrder, OrderId = {message.OrderId}");

            var orderPlaced = new OrderPlaced
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderPlaced);
        }
    }
}