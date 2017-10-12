using System;
using System.Threading.Tasks;
using Messages_Stateful;
using Microsoft.ServiceFabric.Data.Collections;
using NServiceBus;
using NServiceBus.Persistence.ServiceFabric;

namespace Back_Stateful
{
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            var session = context.SynchronizedStorageSession.ServiceFabricSession();
            var stateManager = session.StateManager;
            var transaction = session.Transaction;

            var order = message.ToOrder();

            var dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Order>>(transaction, Order.OrdersDictionaryName);
            await dictionary.AddOrUpdateAsync(transaction, order.OrderId, _ => order, (_, __) => order);

            await context.Publish(new OrderCreated
            {
                OrderId = order.OrderId
            });

            await context.Send(new UpdateOrderColdStorage
            {
                OrderId = order.OrderId,
                SubmittedOn = order.SubmittedOn,
                ProcessedOn = order.ProcessedOn
            }).ConfigureAwait(false);
        }
    }
}