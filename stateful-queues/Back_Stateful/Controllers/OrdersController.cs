using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Back_Stateful.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        public OrdersController(IReliableStateManager stateManager, IApplicationLifetime applicationLifetime)
        {
            this.stateManager = stateManager;
            this.applicationLifetime = applicationLifetime;
        }

        // TODO: 3.6
        [HttpGet]
        public async Task<IEnumerable<Order>> Orders()
        {
            var orders = new List<Order>();

            var result = await stateManager.TryGetAsync<IReliableDictionary<Guid, Order>>(Order.OrdersDictionaryName);

            if (!result.HasValue)
            {
                return orders;
            }

            var dictionary = result.Value;

            using (var transaction = stateManager.CreateTransaction())
            {
                var enumerable = await dictionary.CreateEnumerableAsync(transaction);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(applicationLifetime.ApplicationStopping))
                {
                    orders.Add(enumerator.Current.Value);
                }
            }

            return orders;
        }

        readonly IReliableStateManager stateManager;
        readonly IApplicationLifetime applicationLifetime;
    }
}
