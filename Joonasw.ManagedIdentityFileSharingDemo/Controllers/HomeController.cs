using Joonasw.ManagedIdentityFileSharingDemo.Models;
using Joonasw.ManagedIdentityFileSharingDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
                Files = await _fileService.GetFilesAsync(User, HttpContext.RequestAborted)
            };
            return View(model);
        }

        [HttpPost("/upload")]
        public async Task<IActionResult> Upload(IndexModel model)
        {
            // Do some additional checks
            if (ModelState.IsValid)
            {
                if (model.NewFile.FileName.Length > 256)
                {
                    ModelState.AddModelError(nameof(IndexModel.NewFile), "File name must be max 256 characters");
                }
                else if ((model.NewFile.ContentType?.Length ?? 0) > 128)
                {
                    ModelState.AddModelError(nameof(IndexModel.NewFile), "Content type of file must be max 128 characters");
                }
                else if (model.NewFile.Length > 1 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(IndexModel.NewFile), "File max size 1 MB");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _fileService.UploadFileAsync(model.NewFile, User, HttpContext.RequestAborted);
                    return RedirectToAction(nameof(Index));
                }
                catch (SpaceUnavailableException e)
                {
                    ModelState.AddModelError(nameof(IndexModel.NewFile), e.Message);
                }
            }

            // Re-hydrate the Files collection
            model.Files = await _fileService.GetFilesAsync(User, HttpContext.RequestAborted);
            return View(nameof(Index), model);
        }

        [AcceptVerbs("GET", "HEAD", Route = "/download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var (stream, fileName, contentType) = await _fileService.DownloadFileAsync(id, User, HttpContext.RequestAborted);
                return File(stream, contentType, fileName);
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
        }

        [HttpPost("/delete")]
        public async Task<IActionResult> Delete(Guid fileToDelete)
        {
            try
            {
                await _fileService.DeleteFileAsync(fileToDelete, User, HttpContext.RequestAborted);
                return RedirectToAction(nameof(Index));
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
        }

        [AcceptVerbs("GET", "HEAD", Route = "/privacy")]
        [AllowAnonymous]
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
