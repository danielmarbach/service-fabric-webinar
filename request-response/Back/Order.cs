using System;

namespace Back
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
