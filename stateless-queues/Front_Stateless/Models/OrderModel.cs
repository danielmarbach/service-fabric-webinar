using System;

namespace Front_Stateless.Models
{
    public class OrderModel
    {
        public Guid OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}
