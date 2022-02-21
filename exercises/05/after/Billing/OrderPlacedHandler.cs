namespace Billing
{
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog log = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            log.Info($"Received OrderPlaced, OrderId = {message.OrderId} - Charging credit card...");

            var orderBilled = new OrderBilled
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderBilled);
        }
    }
}