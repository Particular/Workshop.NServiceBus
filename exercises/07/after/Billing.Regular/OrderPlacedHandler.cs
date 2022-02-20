using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace Billing
{
    using System;
    using System.Collections.Generic;
    using Billing.Shared;
    using Messages.Events;

    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        static ILog log = LogManager.GetLogger<OrderPlacedHandler>();
        static List<int> strategicCustomers = Customers.GetStrategicCustomers();

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