namespace Messages.Events
{
    public class OrderPlaced 
    {
        public int CustomerId { get; set; }
        public string OrderId { get; set; }
    }
}