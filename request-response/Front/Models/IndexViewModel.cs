using System.Collections.Generic;

namespace Front.Models
{
    public class IndexViewModel
    {
        public IEnumerable<OrderModel> Orders { get; set; } = new List<OrderModel>();
    }
}