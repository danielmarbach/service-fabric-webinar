using System.Threading.Tasks;
using Messages_Stateful;
using Microsoft.ServiceFabric.Data.Collections;
using NServiceBus;
using NServiceBus.Persistence.ServiceFabric;

namespace Back_Stateful
{
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        public async Task Handle(OrderAccepted message, IMessageHandlerContext context)
        {
            var session = context.SynchronizedStorageSession.ServiceFabricSession();
            var stateManager = session.StateManager;
            var transaction = session.Transaction;

            var dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<int, Order>>(transaction, Order.OrdersDictionaryName);
            var orderValue = await dictionary.TryGetValueAsync(transaction, message.OrderId);
            if (orderValue.HasValue)
            {
                var oldOrder = orderValue.Value;
                var newOrder = new Order
                {
                    Accepted = true,
                    ConfirmationId = oldOrder.ConfirmationId,
                    OrderId = oldOrder.OrderId,
                    ProcessedOn = oldOrder.ProcessedOn,
                    SubmittedOn = oldOrder.SubmittedOn
                };
                await dictionary.TryUpdateAsync(transaction, message.OrderId, newOrder, oldOrder);
            }
        }
    }
}