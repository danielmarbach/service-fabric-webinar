using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Front.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Front.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(FabricClient fabricClient, HttpClient httpClient, IApplicationLifetime appLifetime)
        {
            this.httpClient = httpClient;
            applicationLifetime = appLifetime;

            var uriBuilder = new ServiceUriBuilder("Back");
            backServiceUri = uriBuilder.Build();
        }

        // TODO: 1.1
        public async Task<IActionResult> Index()
        {
            Uri getUrl = new HttpServiceUriBuilder()
                .SetServiceName(backServiceUri)
                .SetEndpointName("KestrelListener")
                .SetServicePathAndQuery("/api/orders")
                .Build();

            var response = await httpClient.GetAsync(getUrl, applicationLifetime.ApplicationStopping);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)response.StatusCode);
            }

            var json = await response.Content.ReadAsStringAsync();

            var orders = JsonConvert.DeserializeObject<List<OrderModel>>(json);

            var model = new IndexViewModel
            {
                Orders = orders
            };

            return View(model);
        }

        // TODO: 1.2
        [HttpPost]
        public async Task<IActionResult> Order()
        {
            Uri putUrl = new HttpServiceUriBuilder()
                .SetServiceName(backServiceUri)
                .SetEndpointName("KestrelListener")
                .SetServicePathAndQuery("/api/orders")
                .Build();

            var newOrder = new NewOrderRequest { SubmittedOn = DateTime.UtcNow, OrderId = CombGuid.Generate() };

            using (var content = new StringContent(JsonConvert.SerializeObject(newOrder), Encoding.UTF8, "application/json"))
            {
                var httpResponse = await httpClient.PutAsync(putUrl, content)
                    .ConfigureAwait(false);

                var json = await httpResponse.Content.ReadAsStringAsync();

                var model = JsonConvert.DeserializeObject<OrderViewModel>(json);

                return View(model);
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        IApplicationLifetime applicationLifetime;
        Uri backServiceUri;
        HttpClient httpClient;
    }
}
