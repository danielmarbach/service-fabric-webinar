using System.Threading.Tasks;
using Messages_Stateless;
using NServiceBus;

namespace Back_Stateless
{
    // TODO: 2.2
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            using (var orderContext = context.SynchronizedStorageSession.FromCurrentSession())
            {
                var order = message.ToOrder();
                orderContext.Orders.Add(order);

                await context.Publish(new OrderCreated
                {
                    OrderId = order.OrderId
                });

                await orderContext.SaveChangesAsync();
            }
        }
    }
}