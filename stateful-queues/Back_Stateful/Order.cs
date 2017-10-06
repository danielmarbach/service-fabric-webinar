using System;

namespace Back_Stateful
{
    public class Order
    {
        public const string OrdersDictionaryName = "orders";

        static Random random = new Random();

        public Order()
        {
            OrderId = random.Next();
        }

        public int OrderId { get; set; }
        public int ConfirmationId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}
