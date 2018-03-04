using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace Billing
{
    using Messages.Events;

    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        static ILog log = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            log.Info($"Billing has received OrderPlaced, OrderId = {message.OrderId}");

            return Task.CompletedTask;
        }
    }
}