using System;
using System.Threading.Tasks;
using Messages_Stateless;
using NServiceBus;

namespace Back_Stateless
{
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        private OrderContext orderContext;

        public SubmitOrderHandler(OrderContext orderContext)
        {
            this.orderContext = orderContext;
        }

        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            var order = new Order
            {
                ConfirmationId = message.ConfirmationId,
                SubmittedOn = message.SubmittedOn,
                ProcessedOn = DateTime.UtcNow
            };

            orderContext.Orders.Add(order);

            await orderContext.SaveChangesAsync();
        }
    }
}