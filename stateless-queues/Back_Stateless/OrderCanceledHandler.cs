using System.Threading.Tasks;
using Messages_Stateless;
using Microsoft.EntityFrameworkCore;
using NServiceBus;

namespace Back_Stateless
{
    public class OrderCanceledHandler : IHandleMessages<OrderCanceled>
    {
        public async Task Handle(OrderCanceled message, IMessageHandlerContext context)
        {
            using (var orderContext = context.SynchronizedStorageSession.FromCurrentSession())
            {
                var order = await orderContext.Orders.SingleOrDefaultAsync(o => o.OrderId == message.OrderId);
                orderContext.Remove(order);
                await orderContext.SaveChangesAsync();
            }
        }
    }
}