using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Back_Stateless.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private OrderContext orderContext;

        public OrdersController(OrderContext context)
        {
            orderContext = context;
        }

        [HttpGet]
        public Task<List<Order>> Orders()
        {
            return orderContext.Orders.OrderBy(o => o.SubmittedOn).ToListAsync();
        }
    }
}
