using System.Collections.Generic;

namespace Front_Stateful.Models
{
    public class IndexViewModel
    {
        public IEnumerable<OrderModel> Orders { get; set; } = new List<OrderModel>();
    }
}