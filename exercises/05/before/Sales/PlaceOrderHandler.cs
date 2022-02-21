#pragma warning disable 162

namespace Sales
{
    using Messages;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private static readonly ILog log = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            log.Info($"Received PlaceOrder, OrderId = {message.OrderId}");

            // This is normally where some business logic would occur

            var orderPlaced = new OrderPlaced
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderPlaced);
        }
    }
}

#pragma warning restore 162