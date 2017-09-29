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

        static Random random = new Random();

        static HomeController()
        {
            serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/Back");

            backoffQueryDelay = TimeSpan.FromSeconds(3);

            fabricClient = new FabricClient();

            communicationFactory = new OrderBackendClientFactory(new ServicePartitionResolver(() => fabricClient));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Order()
        {
            var partitionClient = new ServicePartitionClient<OrderBackendClient>(communicationFactory, serviceUri);
            await partitionClient.InvokeWithRetryAsync(client => client.Order(random.Next()));
            return View("Index");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
