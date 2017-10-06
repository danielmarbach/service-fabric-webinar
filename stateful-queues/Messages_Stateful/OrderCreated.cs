using NServiceBus;

namespace Messages_Stateful
{
    public class OrderCreated : IEvent
    {
        public int OrderId { get; set; }
        public int ConfirmationId { get; set; }
    }
}
