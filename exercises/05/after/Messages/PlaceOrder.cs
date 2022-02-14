using NServiceBus;

namespace Messages
{
    public class PlaceOrder :
    {
        public string OrderId { get; set; }
    }
}