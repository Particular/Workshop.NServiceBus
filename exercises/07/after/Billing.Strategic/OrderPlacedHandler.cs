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

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            if (!strategicCustomers.Contains(message.CustomerId))
            {
                log.Info($"Ignoring order [{message.OrderId}], as [{message.CustomerId}] is NOT a strategic customer.");
                return Task.CompletedTask;
            }

            log.Info($"Billing has received OrderPlaced, OrderId = {message.OrderId}");
            return Task.CompletedTask;
        }
    }
}