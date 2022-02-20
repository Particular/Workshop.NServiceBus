using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace Billing
{
    using System;
    using System.Collections.Generic;
    using Billing.Shared;
    using Messages.Events;

    public class OrderPlacedHandler :
        IHandleMessages<OrderPlaced>
    {
        static ILog log = LogManager.GetLogger<OrderPlacedHandler>();
        static List<int> strategicCustomers = Customers.GetStrategicCustomers();

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