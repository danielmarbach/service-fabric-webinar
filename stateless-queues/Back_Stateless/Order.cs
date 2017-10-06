using System;

namespace Back_Stateless
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
        public Guid ConfirmationId { get; set; }
        public DateTime ProcessedOn
        {
            get => CreatedOn;
            set => CreatedOn = value;
        }
    }
}
