﻿using System.Threading.Tasks;
using Messages_Stateless;
using Microsoft.EntityFrameworkCore;
using NServiceBus;

namespace Back_Stateless
{
    // TODO: 2.2
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
            var sqlPersistenceSession = context.SynchronizedStorageSession.SqlPersistenceSession();
            optionsBuilder.UseSqlServer(sqlPersistenceSession.Connection);

            using (var orderContext = new OrderContext(optionsBuilder.Options))
            {
                orderContext.Database.UseTransaction(sqlPersistenceSession.Transaction);
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