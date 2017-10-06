using System;

namespace Back
{
    public class Order
    {
        static Random random = new Random();

        public Order()
        {
            OrderId = random.Next();
        }

        public int OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
