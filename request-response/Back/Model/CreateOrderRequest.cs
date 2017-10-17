namespace Back.Model
{
    using System;

    public class CreateOrderRequest
    {
        public DateTime SubmittedOn { get; set; }

        public Guid OrderId { get; set; }
    }
}
