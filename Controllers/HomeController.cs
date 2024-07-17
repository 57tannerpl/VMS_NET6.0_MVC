using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VMS_NET6._0_MVC.Models;

namespace VMS_NET6._0_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = GetUploadedVideos();
            return View("Catalogue", model);
        }

        public IActionResult Catalogue()
        {
            var model = GetUploadedVideos();
            return PartialView(model);
        }

        public IActionResult Upload()
        {
            return PartialView();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            Console.WriteLine(files.Count);//debug line

            if (files == null || files.Count == 0)
            {
                ViewBag.Message = "No file selected for upload.";
                return View("Upload");
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in files)
            {
                if (file.Length > 209715200) // 200MB in bytes
                {
                    ViewBag.Message = "An error occurred whilst uploading file(s). Response Code 413. Please try again.";
                    return View("Upload");
                }

                if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".mp4")
                {
                    var filePath = Path.Combine(uploadsFolder, file.FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    ViewBag.Message = "Only MP4 files are allowed.";
                    return View("Upload");
                }
            }

            return RedirectToAction("Index");
        }

        private List<VideoFileInfo> GetUploadedVideos()
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var videoFiles = Directory.GetFiles(uploadsFolder, "*.mp4")
                                      .Select(file => new FileInfo(file))
                                      .Select(fileInfo => new VideoFileInfo
                                      {
                                          FileName = fileInfo.Name,
                                          FileSize = fileInfo.Length / 1024 // Size in KB
                                      }).ToList();

            return videoFiles;
        }

    }
}