using System;

namespace Front.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
