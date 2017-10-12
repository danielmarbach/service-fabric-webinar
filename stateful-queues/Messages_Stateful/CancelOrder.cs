using System;
using NServiceBus;

namespace Messages_Stateful
{
    public class CancelOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}