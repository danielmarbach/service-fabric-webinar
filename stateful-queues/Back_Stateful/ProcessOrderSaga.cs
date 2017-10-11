using System;
using System.Threading.Tasks;
using Messages_Stateful;
using NServiceBus;

namespace Back_Stateful
{
    public class ProcessOrderSaga :
        Saga<ProcessOrderSaga.OrderData>,
        IAmStartedByMessages<SubmitOrder>,
        IHandleMessages<OrderCreated>,
        IHandleMessages<CancelOrder>,
        IHandleTimeouts<ProcessOrderSaga.BuyersRemorseIsOver>
    {
        public Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            Data.ConfirmationId = message.ConfirmationId;

            return RequestTimeout(context, TimeSpan.FromSeconds(10), new BuyersRemorseIsOver());
        }

        public Task Timeout(BuyersRemorseIsOver state, IMessageHandlerContext context)
        {
            MarkAsComplete();

            var orderAccepted = new OrderAccepted
            {
                ConfirmationId = Data.ConfirmationId,
                OrderId = Data.OrderId,
            };
            return context.Publish(orderAccepted);
        }

        public Task Handle(OrderCreated message, IMessageHandlerContext context)
        {
            Data.ConfirmationId = message.ConfirmationId;
            Data.OrderId = message.OrderId;
            Data.OrderCreated = true;

            return Task.CompletedTask;
        }

        public Task Handle(CancelOrder message, IMessageHandlerContext context)
        {
            MarkAsComplete();

            var orderCancelled = new OrderCanceled
            {
                ConfirmationId = message.ConfirmationId,
                OrderId = Data.OrderId
            };
            return context.Publish(orderCancelled);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderData> mapper)
        {
            mapper.ConfigureMapping<SubmitOrder>(message => message.ConfirmationId)
                .ToSaga(sagaData => sagaData.ConfirmationId);
            mapper.ConfigureMapping<OrderCreated>(message => message.ConfirmationId)
                .ToSaga(sagaData => sagaData.ConfirmationId);
            mapper.ConfigureMapping<CancelOrder>(message => message.ConfirmationId)
                .ToSaga(sagaData => sagaData.ConfirmationId);
        }

        public class OrderData :
            ContainSagaData
        {
            public int ConfirmationId { get; set; }

            public bool OrderCreated { get; set; }
            public int OrderId { get; set; }
        }

        public class BuyersRemorseIsOver
        {
        }
    }
}