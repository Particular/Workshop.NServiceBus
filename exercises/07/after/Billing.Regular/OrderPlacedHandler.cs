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
        private static readonly List<int> strategicCustomers = Customers.GetStrategicCustomers();

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            await Task.Delay(1000);

            if (strategicCustomers.Contains(message.CustomerId))
            {
                log.Info($"Ignoring order [{message.OrderId}], as [{message.CustomerId}] is a strategic customer.");
                return;
            }

            log.Info($"Billing has received OrderPlaced, OrderId = {message.OrderId}");
        }
    }
}