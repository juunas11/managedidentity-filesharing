using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Joonasw.ManagedIdentityFileSharingDemo.Models;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using Joonasw.ManagedIdentityFileSharingDemo.Services;
using System;
using System.IO;

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
        public async Task<IActionResult> Index()
        {
            var model = new IndexModel
            {
                Files = await _fileService.GetFilesAsync(User)
            };
            return View(model);
        }

        [HttpPost("/upload")]
        public async Task<IActionResult> Upload(IndexModel model)
        {
            if (model.NewFile.Length > 1 * 1024 * 1024)
            {
                throw new Exception($"Too large file {model.NewFile.Length} bytes");
            }

            await _fileService.UploadFileAsync(model.NewFile, User);
            return RedirectToAction(nameof(Index));
        }

        [AcceptVerbs("GET", "HEAD", Route = "/download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var (stream, fileName, contentType) = await _fileService.DownloadFileAsync(id, User);
                return File(stream, contentType, fileName);
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
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
