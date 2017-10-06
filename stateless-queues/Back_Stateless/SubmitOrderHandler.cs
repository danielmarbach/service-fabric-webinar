using System;
using System.Threading.Tasks;
using Back_Stateless.Data;
using Messages_Stateless;
using NServiceBus;

namespace Back_Stateless
{
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            var order = new Model.Order
            {
                ConfirmationId = message.ConfirmationId,
                SubmittedOn = message.SubmittedOn,
                ProcessedOn = DateTime.UtcNow
            };

            using (var dbContext = new SalesDbContext())
            {
                dbContext.Orders.Add(order);

                await dbContext.SaveChangesAsync();
            }

            ServiceEventSource.Current.Write(nameof(SubmitOrder), message);
        }
    }
}