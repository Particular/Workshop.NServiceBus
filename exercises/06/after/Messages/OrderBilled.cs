namespace Messages.Events
{
    using NServiceBus;

    public class OrderBilled
    {
        public string OrderId { get; set; }
    }
}