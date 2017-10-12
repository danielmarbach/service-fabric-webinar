using System;
using Messages_Stateful;

namespace Back_Cold
{
    public static class MessageExtensions
    {
        public static Order ToOrder(this UpdateOrderColdStorage message)
        {
            var order = new Order
            {
                OrderId = message.OrderId,
                SubmittedOn = message.SubmittedOn,
                CreatedOn = message.ProcessedOn,
                StoredOn = DateTime.UtcNow
            };
            return order;
        }
    }
}