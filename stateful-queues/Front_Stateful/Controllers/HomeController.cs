using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Front_Stateful.Models;
using Messages_Stateful;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using NServiceBus;

namespace Front_Stateful.Controllers
{
    public class HomeController : Controller
    {
        static Random random = new Random();

        public HomeController(IMessageSession session, FabricClient fabricClient, HttpClient httpClient, IApplicationLifetime appLifetime)
        {
            this.httpClient = httpClient;
            applicationLifetime = appLifetime;
            messageSession = session;
            this.fabricClient = fabricClient;

            var uriBuilder = new ServiceUriBuilder("Back_Stateful");
            backServiceUri = uriBuilder.Build();
        }

        public async Task<IActionResult> Index()
        {
            var partitions = await fabricClient.QueryManager.GetPartitionListAsync(backServiceUri);

            var orderTasks = new List<Task<List<OrderModel>>>();

            foreach (var partition in partitions)
            {
                async Task<List<OrderModel>> queryPartition()
                {
                    var getUrl = new HttpServiceUriBuilder()
                        .SetServiceName(backServiceUri)
                        .SetPartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey)
                        .SetEndpointName("KestrelListener")
                        .SetServicePathAndQuery("/api/orders")
                        .Build();

                    var response = await httpClient.GetAsync(getUrl, applicationLifetime.ApplicationStopping);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return new List<OrderModel>();
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<OrderModel>>(json);
                }

                orderTasks.Add(queryPartition());
            }

            var allPartitionOrders = await Task.WhenAll(orderTasks);

            return View(new IndexViewModel
            {
                Orders = allPartitionOrders.SelectMany(o => o)
            });
        }


        [HttpPost]
        public async Task<IActionResult> Order()
        {
            var model = new OrderViewModel
            {
                NewOrder = new OrderModel
                {
                    ConfirmationId = random.Next(),
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
        FabricClient fabricClient;

        IApplicationLifetime applicationLifetime;
        Uri backServiceUri;
        HttpClient httpClient;
    }
}
