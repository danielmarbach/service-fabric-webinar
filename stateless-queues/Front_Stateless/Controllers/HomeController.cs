using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Front_Stateless.Models;
using Messages_Stateless;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using NServiceBus;

namespace Front_Stateless.Controllers
{
    public class HomeController : Controller
    {

        public HomeController(IMessageSession session, FabricClient fabricClient, HttpClient httpClient, IApplicationLifetime appLifetime)
        {
            this.httpClient = httpClient;
            applicationLifetime = appLifetime;
            messageSession = session;

            var uriBuilder = new ServiceUriBuilder("Back_Stateless");
            backServiceUri = uriBuilder.Build();
        }

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


        [HttpPost]
        public async Task<IActionResult> Order()
        {
            var model = new OrderViewModel
            {
                NewOrder = new OrderModel
                {
                    ConfirmationId = Guid.NewGuid(),
                    SubmittedOn = DateTime.UtcNow
                },
                Errors = new List<string>()
            };

            try
            {
                await messageSession.Send(new SubmitOrder
                {
                    ConfirmationId = model.NewOrder.ConfirmationId,
                    SubmittedOn = model.NewOrder.SubmittedOn
                });
            }
            catch (Exception e)
            {
                model.Errors.Add(e.Message);
            }

            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        IMessageSession messageSession;

        IApplicationLifetime applicationLifetime;
        Uri backServiceUri;
        HttpClient httpClient;
    }
}
