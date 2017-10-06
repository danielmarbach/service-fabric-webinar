using System;

namespace Back_Cold

{
    public class Order
    {
        public int OrderId { get; set; }
        public int ConfirmationId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime StoredOn { get; set; }
    }
}
