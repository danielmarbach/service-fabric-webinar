using System.Collections.Generic;
using System.Linq;
using Back_Stateless.Data;
using Back_Stateless.Model;
using Microsoft.AspNetCore.Mvc;

namespace Back_Stateless.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        public IEnumerable<Order> Orders()
        {
            using (var context = new SalesDbContext())
            {
                return context.Orders.OrderBy(o => o.SubmittedOn);
            }
        }
    }
}
