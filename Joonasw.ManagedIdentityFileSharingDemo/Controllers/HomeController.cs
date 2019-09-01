using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Joonasw.ManagedIdentityFileSharingDemo.Models;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using Joonasw.ManagedIdentityFileSharingDemo.Services;

namespace Joonasw.ManagedIdentityFileSharingDemo.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly FileService _fileService;

        public HomeController(FileService fileService)
        {
            _fileService = fileService;
        }

        [AcceptVerbs("GET", "HEAD")]
        public IActionResult Index()
        {
            var model = new IndexModel();
            return View(model);
        }

        [HttpPost("/upload")]
        public async Task<IActionResult> Upload(IndexModel model)
        {
            await _fileService.UploadFileAsync(model.NewFile, User);
            return RedirectToAction(nameof(Index));
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
