namespace Billing
{
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;
    using Shared;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog log = LogManager.GetLogger<OrderPlacedHandler>();

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            await Task.Delay(1000, context.CancellationToken);

            log.Info($"Billing has received OrderPlaced, OrderId = {message.OrderId}");
        }
    }
}