using System;
using NServiceBus;

namespace Messages_Stateless
{
    public class OrderCreated : IEvent
    {
        public Guid OrderId { get; set; }
    }
}
