using System;
using NServiceBus;

namespace Messages_Stateful
{
    public class UpdateOrderColdStorage : ICommand
    {
        public int OrderId { get; set; }
        public int ConfirmationId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}