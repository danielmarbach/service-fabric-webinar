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
            Data.OrderId = message.OrderId;

            return RequestTimeout(context, TimeSpan.FromSeconds(10), new BuyersRemorseIsOver());
        }

        public Task Timeout(BuyersRemorseIsOver state, IMessageHandlerContext context)
        {
            MarkAsComplete();

            var orderAccepted = new OrderAccepted
            {
                OrderId = Data.OrderId,
            };
            return context.Publish(orderAccepted);
        }

        public Task Handle(OrderCreated message, IMessageHandlerContext context)
        {
            Data.OrderId = message.OrderId;
            Data.OrderCreated = true;

            return Task.CompletedTask;
        }

        public Task Handle(CancelOrder message, IMessageHandlerContext context)
        {
            MarkAsComplete();

            var orderCancelled = new OrderCanceled
            {
                OrderId = Data.OrderId
            };
            return context.Publish(orderCancelled);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderData> mapper)
        {
            mapper.ConfigureMapping<SubmitOrder>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
            mapper.ConfigureMapping<OrderCreated>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
            mapper.ConfigureMapping<CancelOrder>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
        }

        public class OrderData :
            ContainSagaData
        {
            public bool OrderCreated { get; set; }
            public Guid OrderId { get; set; }
        }

        public class BuyersRemorseIsOver
        {
        }
    }
}