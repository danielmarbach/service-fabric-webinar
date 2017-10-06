using System.Collections.Generic;

namespace Front_Stateful.Models
{
    public class OrderViewModel
    {
        public OrderModel NewOrder { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }
}
