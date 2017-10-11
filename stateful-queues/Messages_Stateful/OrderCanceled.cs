using NServiceBus;

namespace Messages_Stateful
{
    public class OrderCanceled : IEvent
    {
        public int OrderId { get; set; }
        public int ConfirmationId { get; set; }
    }
}