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
            var dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<int, Order>>(transaction, "orders")
                .ConfigureAwait(false);

            var order = new Order
            {
                ConfirmationId = message.ConfirmationId,
                SubmittedOn = message.SubmittedOn,
                ProcessedOn = DateTime.UtcNow
            };

            await dictionary.AddOrUpdateAsync(transaction, order.OrderId, _ => order, (_, __) => order)
                .ConfigureAwait(false);

            await context.Publish(new OrderCreated
            {
                ConfirmationId = message.ConfirmationId,
                OrderId = order.OrderId
            }).ConfigureAwait(false);

            await context.Send(new UpdateOrderColdStorage
            {
                OrderId = order.OrderId,
                ConfirmationId = order.ConfirmationId,
                SubmittedOn = order.SubmittedOn,
                ProcessedOn = order.ProcessedOn
            }).ConfigureAwait(false);

            ServiceEventSource.Current.Write(nameof(SubmitOrder), message);
        }
    }
}