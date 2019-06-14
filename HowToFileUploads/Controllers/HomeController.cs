using HowToFileUploads.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;

namespace HowToFileUploads.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _env;
        private string _dir;

        public HomeController(IHostingEnvironment env)
        {
            _env = env;
            _dir = _env.ContentRootPath;
        }

        public IActionResult Index() => View();

        public IActionResult SingleFile(IFormFile file)
        {
            using (var fileStream = new FileStream(Path.Combine(_dir, "file.png"), FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(fileStream);
            }

            return RedirectToAction("Index");
        }

        public IActionResult MultipleFiles(IEnumerable<IFormFile> files)
        {
            int i = 0;
            foreach (var file in files)
            {
                using (var fileStream = new FileStream(Path.Combine(_dir, $"file{i++}.png"), FileMode.Create, FileAccess.Write))
                {
                    file.CopyTo(fileStream);
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult FileInModel(SomeForm someForm)
        {
            using (var fileStream = new FileStream(
                Path.Combine(_dir, $"{someForm.Name}.png"),
                FileMode.Create,
                FileAccess.Write))
            {
                someForm.File.CopyTo(fileStream);
            }

            return RedirectToAction("Index");
        }

    }
}