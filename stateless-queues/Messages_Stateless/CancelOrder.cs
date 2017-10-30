using System;
using NServiceBus;

namespace Messages_Stateless
{
    public class CancelOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}