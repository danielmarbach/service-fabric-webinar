using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private OrderContext orderContext;

        public OrdersController(OrderContext context)
        {
            orderContext = context;
        }

        // TODO: 1.3
        [HttpGet]
        public Task<List<Order>> Orders()
        {
            return orderContext.Orders.OrderBy(o => o.SubmittedOn).ToListAsync();
        }

        // TODO: 1.4
        // PUT api/orders/
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CreateOrderRequest newOrder)
        {
            var order = new Order
            {
                SubmittedOn = newOrder.SubmittedOn,
                CreatedOn = DateTime.UtcNow,
                OrderId = newOrder.OrderId
            };

            var response = new CreateOrderResponse();

            try
            {
                orderContext.Orders.Add(order);

                await orderContext.SaveChangesAsync();

                response.NewOrder = order;

                return Accepted(response);
            }
            catch (Exception ex)
            {
                response.Errors = new List<string> {ex.Message};
                return BadRequest(response);
            }
        }
    }
}
