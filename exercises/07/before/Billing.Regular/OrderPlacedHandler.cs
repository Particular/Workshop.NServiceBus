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
            log.Info($"Billing has received OrderPlaced, OrderId = {message.OrderId}");

            return Task.CompletedTask;
        }
    }
}