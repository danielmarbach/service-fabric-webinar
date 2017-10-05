using System.Collections.Generic;
using System.Linq;

namespace Back.Model
{
    public class CreateOrderResponse
    {
        public Order NewOrder { get; set; }
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
    }
}