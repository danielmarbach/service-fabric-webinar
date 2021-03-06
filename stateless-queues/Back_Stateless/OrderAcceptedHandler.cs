using System.Threading.Tasks;
using Messages_Stateless;
using Microsoft.EntityFrameworkCore;
using NServiceBus;

namespace Back_Stateless
{
    // TODO: 2.7
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        public async Task Handle(OrderAccepted message, IMessageHandlerContext context)
        {
            using (var orderContext = context.SynchronizedStorageSession.FromCurrentSession())
            {
                var order = await orderContext.Orders.SingleOrDefaultAsync(o => o.OrderId == message.OrderId);
                if (order == null)
                {
                    return;
                }
                order.Accepted = true;
                await orderContext.SaveChangesAsync();
            }
        }
    }
}