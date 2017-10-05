using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Front.Models;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Front.Controllers
{
    public class HomeController : Controller
    {
        private const int MaxQueryRetryCount = 20;

        private static readonly Uri serviceUri;
        private static readonly TimeSpan backoffQueryDelay;

        private static readonly FabricClient fabricClient;

        private static readonly OrderBackendClientFactory communicationFactory;

        static HomeController()
        {
            serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/Back");

            backoffQueryDelay = TimeSpan.FromSeconds(3);

            fabricClient = new FabricClient();

            communicationFactory = new OrderBackendClientFactory(new ServicePartitionResolver(() => fabricClient));
        }

        public async Task<IActionResult> Index()
        {
            var partitionClient = new ServicePartitionClient<OrderBackendClient>(communicationFactory, serviceUri);

            var orders = await partitionClient.InvokeWithRetryAsync(client => client.List());

            var model = new IndexViewModel
            {
                Orders = orders
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Order()
        {
            var partitionClient = new ServicePartitionClient<OrderBackendClient>(communicationFactory, serviceUri);
            var response = await partitionClient.InvokeWithRetryAsync(client => client.Order(new NewOrderRequest{SubmittedOn = DateTime.UtcNow}));

            var model = new OrderViewModel
            {
                NewOrder = response.NewOrder,
                Errors = response.Errors
            };

            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
