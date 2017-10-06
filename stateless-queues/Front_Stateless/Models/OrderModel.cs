using System;

namespace Front_Stateless.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public Guid ConfirmationId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ProcessedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
