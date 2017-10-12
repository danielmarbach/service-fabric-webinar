using System;
using NServiceBus;

namespace Messages_Stateful
{
    public class OrderCanceled : IEvent
    {
        public Guid OrderId { get; set; }
    }
}