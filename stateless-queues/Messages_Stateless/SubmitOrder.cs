using System;
using NServiceBus;

namespace Messages_Stateless
{
    public class SubmitOrder : ICommand
    {
        public Guid OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
    }
}