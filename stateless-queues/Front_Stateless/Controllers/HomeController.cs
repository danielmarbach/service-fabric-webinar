using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Net;
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
            #region Same as before

            this.httpClient = httpClient;
            applicationLifetime = appLifetime;

            var uriBuilder = new ServiceUriBuilder("Back_Stateless");
            backServiceUri = uriBuilder.Build();

                #endregion

            messageSession = session;
        }

        #region Same as before

        public async Task<IActionResult> Index()
        {
            Uri getUrl = new HttpServiceUriBuilder()
                .SetServiceName(backServiceUri)
                .SetEndpointName("KestrelListener")
                .SetServicePathAndQuery("/api/orders")
                .Build();

            var response = await httpClient.GetAsync(getUrl, applicationLifetime.ApplicationStopping);

            if (response.StatusCode != HttpStatusCode.OK)
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

            #endregion

        // TODO: 2.1
        [HttpPost]
        public async Task<IActionResult> Order()
        {
            var model = new OrderViewModel
            {
                NewOrder = new OrderModel
                {
                    OrderId = CombGuid.Generate(),
                    SubmittedOn = DateTime.UtcNow
                },
                Errors = new List<string>()
            };

            try
            {
                await messageSession.Send(new SubmitOrder
                {
                    OrderId = model.NewOrder.OrderId,
                    SubmittedOn = model.NewOrder.SubmittedOn
                });
            }
            catch (Exception e)
            {
                model.Errors.Add(e.Message);
            }

            return View(model);
        }

        #region Not important

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        IApplicationLifetime applicationLifetime;
        Uri backServiceUri;
        HttpClient httpClient;

            #endregion

        IMessageSession messageSession;
    }
}
