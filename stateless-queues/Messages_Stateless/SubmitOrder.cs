using System;
using NServiceBus;

namespace Messages_Stateless
{
    public class SubmitOrder : ICommand
    {
        public Guid ConfirmationId { get; set; }
        public DateTime SubmittedOn { get; set; }
    }
}