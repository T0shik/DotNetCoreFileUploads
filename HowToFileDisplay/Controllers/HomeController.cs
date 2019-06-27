using HowToFileDisplay.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HowToFileDisplay.Controllers
{
    public class HomeController : Controller
    {
        private FileManager _fileManager;

        public HomeController(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public IActionResult Index() => View(_fileManager.GetFiles());

        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            _fileManager.SaveFile(file);
            return RedirectToAction("Index");
        }

        #region advanced
        public IActionResult Advanced() => View(_fileManager.GetOptimizedFiles());

        [HttpPost]
        public IActionResult Advanced(IFormFile file)
        {
            _fileManager.SaveFileOptimize(file);
            return RedirectToAction("Advanced");
        }

        public IActionResult GetImage(int id, int width) =>
            new FileStreamResult(_fileManager.GetImageStream(id, width), "image/*");

        public IActionResult DownloadImage(int id) =>
             File(_fileManager.GetImageStream(id, 0), "image/*", "image.png");
        #endregion
    }
}
