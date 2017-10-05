using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back.Data;
using Back.Model;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers
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

        // PUT api/orders/
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CreateOrderRequest newOrder)
        {
            var order = new Order
            {
                SubmittedOn = newOrder.SubmittedOn,
                CreatedOn = DateTime.UtcNow
            };

            var response = new CreateOrderResponse();

            try
            {
                using (var context = new SalesDbContext())
                {
                    context.Orders.Add(order);

                    await context.SaveChangesAsync();

                    response.NewOrder = order;

                    return Accepted(response);
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<string>{ex.Message};
                return BadRequest(response);
            }
        }
    }
}
