using NServiceBus;

namespace Messages_Stateful
{
    public class CancelOrder : ICommand
    {
        public int ConfirmationId { get; set; }
    }
}