using NServiceBus;

namespace Messages_Stateless
{
    public class SubmitOrder : ICommand
    {
        public SubmitOrder(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; private set; }
    }
}