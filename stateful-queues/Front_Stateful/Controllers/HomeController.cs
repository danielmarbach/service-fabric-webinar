using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Front_Stateful.Models;
using Messages_Stateful;
using NServiceBus;

namespace Front_Stateful.Controllers
{
    public class HomeController : Controller
    {
        private IMessageSession messageSession;

        static Random random = new Random();

        public HomeController(IMessageSession session)
        {
            messageSession = session;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Order()
        {
            var orderId = random.Next();
            try
            {
                await messageSession.Send(new SubmitOrder(orderId));
                return View("Index", new SuccessModel { OrderId = orderId });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("error", e.Message);
                return View("Index");
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
