using System.Threading.Tasks;
using Messages_Stateless;
using NServiceBus;

namespace Back_Stateless
{
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        public SubmitOrderHandler(OrderContext orderContext)
        {
            this.orderContext = orderContext;
        }

        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            var order = message.ToOrder();
            orderContext.Orders.Add(order);

            await context.Publish(new OrderCreated
            {
                OrderId = order.OrderId
            });

            await orderContext.SaveChangesAsync();
        }

        OrderContext orderContext;
    }
}