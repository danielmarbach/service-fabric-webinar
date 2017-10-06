using NServiceBus;

namespace Messages_Stateless
{
    public class OrderCreated : IEvent
    {
        public int OrderId { get; set; }
        public int ConfirmationId { get; set; }
    }
}
