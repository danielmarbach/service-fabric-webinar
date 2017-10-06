using System;
using System.Threading.Tasks;
using Messages_Stateful;
using NServiceBus;

namespace Back_Cold
{
    public class UpdateOrderColdStorageHandler : IHandleMessages<UpdateOrderColdStorage>
    {
        public UpdateOrderColdStorageHandler(OrderContext orderContext)
        {
            this.orderContext = orderContext;
        }

        public async Task Handle(UpdateOrderColdStorage message, IMessageHandlerContext context)
        {
            var order = new Order
            {
                OrderId = message.OrderId,
                ConfirmationId = message.ConfirmationId,
                SubmittedOn = message.SubmittedOn,
                CreatedOn = message.ProcessedOn,
                StoredOn = DateTime.UtcNow
            };

            orderContext.Orders.Add(order);

            await orderContext.SaveChangesAsync();
        }

        OrderContext orderContext;
    }
}