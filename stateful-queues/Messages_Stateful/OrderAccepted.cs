using System;
using NServiceBus;

namespace Messages_Stateful
{
    public class OrderAccepted : IEvent
    {
        public Guid OrderId { get; set; }
    }
}