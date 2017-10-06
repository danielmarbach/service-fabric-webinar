using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Front;
using Microsoft.AspNetCore.Mvc;
using Front_Stateless.Models;
using Front_Statelesss;
using Messages_Stateless;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using NServiceBus;

namespace Front_Stateless.Controllers
{
    public class HomeController : Controller
    {
        private IMessageSession messageSession;

        static Random random = new Random();

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

        public HomeController(IMessageSession session)
        {
            messageSession = session;
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
    }
}
