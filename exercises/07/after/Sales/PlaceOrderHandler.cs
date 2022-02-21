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
            log.Info($"Received PlaceOrder, OrderId = {message.OrderId}");

            // This is normally where some business logic would occur

            var orderPlaced = new OrderPlaced
            {
                CustomerId = message.CustomerId,
                OrderId = message.OrderId
            };
            return context.Publish(orderPlaced);
        }
    }
}