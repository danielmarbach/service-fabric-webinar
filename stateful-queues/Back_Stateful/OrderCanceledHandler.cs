using System;
using System.Threading.Tasks;
using Messages_Stateful;
using Microsoft.ServiceFabric.Data.Collections;
using NServiceBus;
using NServiceBus.Persistence.ServiceFabric;

namespace Back_Stateful
{
    public class OrderCanceledHandler : IHandleMessages<OrderCanceled>
    {
        public async Task Handle(OrderCanceled message, IMessageHandlerContext context)
        {
            var session = context.SynchronizedStorageSession.ServiceFabricSession();
            var stateManager = session.StateManager;
            var transaction = session.Transaction;

            var dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Order>>(transaction, Order.OrdersDictionaryName);
            await dictionary.TryRemoveAsync(transaction, message.OrderId);
        }
    }
}