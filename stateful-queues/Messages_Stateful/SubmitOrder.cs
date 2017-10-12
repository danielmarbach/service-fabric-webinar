using System;
using NServiceBus;

namespace Messages_Stateful
{
    public class SubmitOrder : ICommand
    {
        public Guid OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
    }
}