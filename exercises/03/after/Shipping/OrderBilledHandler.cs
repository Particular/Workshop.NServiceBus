namespace Shipping
{
    using Messages;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class OrderBilledHandler : IHandleMessages<OrderBilled>
    {
        private static readonly ILog log = LogManager.GetLogger<OrderBilledHandler>();

        public Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            log.Info($"Received OrderBilled, OrderId = {message.OrderId} - Should we ship now?");
            return Task.CompletedTask;
        }
    }
}