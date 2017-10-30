using System;

namespace Back_Stateless
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ProcessedOn
        {
            get => CreatedOn;
            set => CreatedOn = value;
        }

        public bool Accepted { get; set; }
    }
}
