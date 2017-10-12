using System;
using Messages_Stateless;

namespace Back_Stateless
{
    public static class MessageExtensions
    {
        public static Order ToOrder(this SubmitOrder message)
        {
            var order = new Order
            {
                OrderId = message.OrderId,
                SubmittedOn = message.SubmittedOn,
                ProcessedOn = DateTime.UtcNow
            };
            return order;
        }
    }
}