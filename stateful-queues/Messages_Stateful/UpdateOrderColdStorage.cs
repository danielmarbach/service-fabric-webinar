using System;
using NServiceBus;

namespace Messages_Stateful
{
    public class UpdateOrderColdStorage : ICommand
    {
        public Guid OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}