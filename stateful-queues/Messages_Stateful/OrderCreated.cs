using System;
using NServiceBus;

namespace Messages_Stateful
{
    public class OrderCreated : IEvent
    {
        public Guid OrderId { get; set; }
    }
}
