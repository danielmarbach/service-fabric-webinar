using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private static int numberOfCalls;

        // PUT api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id)
        {
            await Task.Delay(1000);

            if(Interlocked.Increment(ref numberOfCalls) % 2 == 0)
            {
                return BadRequest();
            }
            return Accepted();
        }
    }
}
