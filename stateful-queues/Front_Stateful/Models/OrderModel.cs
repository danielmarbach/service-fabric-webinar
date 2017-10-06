using System;

namespace Front_Stateful.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public int ConfirmationId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}
