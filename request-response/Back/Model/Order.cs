namespace Back.Model
{
    using System;

    public class Order
    {
        static Random random = new Random();

        public Order()
        {
            OrderId = random.Next();
        }

        public int Id => OrderId;
        public int OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
