using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Joonasw.ManagedIdentityFileSharingDemo.Models;
using Microsoft.AspNetCore.Routing;

namespace Joonasw.ManagedIdentityFileSharingDemo.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        [AcceptVerbs("GET", "HEAD")]
        public IActionResult Index()
        {
            return View();
        }

        [AcceptVerbs("GET", "HEAD", Route = "/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [AcceptVerbs("GET", "HEAD", Route = "/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
