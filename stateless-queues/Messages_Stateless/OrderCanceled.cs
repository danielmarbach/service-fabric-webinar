using System;
using NServiceBus;

namespace Messages_Stateless
{
    public class OrderCanceled : IEvent
    {
        public Guid OrderId { get; set; }
    }
}