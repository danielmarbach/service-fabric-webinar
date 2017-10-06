using System;
using NServiceBus;

namespace Messages_Stateless
{
    public class SubmitOrder : ICommand
    {
        public int ConfirmationId { get; set; }
        public DateTime SubmittedOn { get; set; }
    }
}