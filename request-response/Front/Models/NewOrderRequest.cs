using System;

namespace Front.Models
{
    public class NewOrderRequest
    {
        public DateTime SubmittedOn { get; set; }

        public Guid OrderId { get; set; }
    }
}
