namespace Sales
{
    using Messages;
    using NServiceBus;
    using NServiceBus.Logging;
    using System.Threading.Tasks;

    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private static readonly ILog log = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            log.Info($"Received PlaceOrder, OrderId = {message.OrderId}");
            return Task.CompletedTask;
        }
    }
}