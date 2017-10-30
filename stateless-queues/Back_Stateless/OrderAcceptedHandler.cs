using System.Threading.Tasks;
using Messages_Stateless;
using Microsoft.EntityFrameworkCore;
using NServiceBus;

namespace Back_Stateless
{
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        public async Task Handle(OrderAccepted message, IMessageHandlerContext context)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
            var sqlPersistenceSession = context.SynchronizedStorageSession.SqlPersistenceSession();
            optionsBuilder.UseSqlServer(sqlPersistenceSession.Connection);

            using (var orderContext = new OrderContext(optionsBuilder.Options))
            {
                orderContext.Database.UseTransaction(sqlPersistenceSession.Transaction);
                var order = await orderContext.Orders.SingleOrDefaultAsync(o => o.OrderId == message.OrderId);
                order.Accepted = true;
                await orderContext.SaveChangesAsync();
            }
        }
    }
}