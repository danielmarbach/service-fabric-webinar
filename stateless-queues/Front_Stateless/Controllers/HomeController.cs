using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Front_Stateless.Models;
using NServiceBus;

namespace Front_Stateless.Controllers
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
            await messageSession.Send(new Order(random.Next()));
            return View("Index");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
