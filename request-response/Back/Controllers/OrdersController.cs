using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        // PUT api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id)
        {
            await Task.Delay(1000);

            if(id % 2 == 0)
            {
                var state = new ModelStateDictionary();
                state.AddModelError("Id",$"Only robots submit even Ids: {id}.");
                return BadRequest(state);
            }
            return Accepted();
        }
    }
}
