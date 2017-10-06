using System.Collections.Generic;

namespace Front_Stateless.Models
{
    public class OrderViewModel
    {
        public OrderModel NewOrder { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }
}
