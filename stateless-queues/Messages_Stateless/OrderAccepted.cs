using System;
using NServiceBus;

namespace Messages_Stateless
{
    public class OrderAccepted : IEvent
    {
        public Guid OrderId { get; set; }
    }
}