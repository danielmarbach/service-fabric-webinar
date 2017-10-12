using System;

namespace Front_Stateful.Models
{
    public class OrderModel
    {
        public Guid OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
        public bool Accepted { get; set; }
    }
}
