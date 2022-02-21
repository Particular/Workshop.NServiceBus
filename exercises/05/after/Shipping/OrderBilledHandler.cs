using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Messages.Events;

namespace Shipping
{
    public class OrderBilledHandler : IHandleMessages<OrderBilled>
    {
        static ILog log = LogManager.GetLogger<OrderBilledHandler>();

        public Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            log.Info($"Received OrderBilled, OrderId = {message.OrderId} - Should we ship now?");
            return Task.CompletedTask;
        }
    }
}