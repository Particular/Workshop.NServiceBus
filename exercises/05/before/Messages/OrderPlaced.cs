namespace Messages.Events
{
    using NServiceBus;

    public class OrderPlaced
    {
        public string OrderId { get; set; }
    }
}