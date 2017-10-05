using System.Collections.Generic;

namespace Back.Model
{
    public class CreateOrderResponse
    {
        public Order NewOrder { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}