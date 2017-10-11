using System.Threading.Tasks;
using Messages_Stateful;
using NServiceBus;

namespace Back_Cold
{
    public class UpdateOrderColdStorageHandler : IHandleMessages<UpdateOrderColdStorage>
    {
        public UpdateOrderColdStorageHandler(OrderContext orderContext)
        {
            this.orderContext = orderContext;
        }

        public Task Handle(UpdateOrderColdStorage message, IMessageHandlerContext context)
        {
            var order = message.ToOrder();
            orderContext.Orders.Add(order);
            return orderContext.SaveChangesAsync();
        }

        OrderContext orderContext;
    }
}