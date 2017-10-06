using System.Collections.Generic;

namespace Front_Stateless.Models
{
    public class IndexViewModel
    {
        public IEnumerable<OrderModel> Orders { get; set; } = new List<OrderModel>();
    }
}