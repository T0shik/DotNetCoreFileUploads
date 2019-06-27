using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HowToFileDisplay.Services
{
    public class FileManager
    {
        private IHostingEnvironment _env;
        private List<File> Files;
        private int Id;

        private readonly List<(int Width, int Height)> imgSizes = new List<(int, int)>
            {
                (480, 270),
                (640, 360),
                (1280, 720),
                (1920, 1080)
            };

        public FileManager(IHostingEnvironment env)
        {
            _env = env;
            Files = new List<File>();
        }

        public File GetFile(int id) => Files.FirstOrDefault(x => x.Id == id);

        public File GetFile(int id, int width) => Files.FirstOrDefault(x => x.Id == id && x.Width == width);

        public IEnumerable<File> GetFiles() => Files;

        public IEnumerable<int> GetOptimizedFiles() =>
            Files
            .Where(x => x.Width > 0)
            .Select(x => x.Id)
            .Distinct();

        public void SaveFile(IFormFile file)
        {
            var name = RandomName();
            var save_path = Path.Combine(_env.WebRootPath, name);

            using (var fileStream = new FileStream(save_path, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(fileStream);
            }

            Files.Add(new File
            {
                Id = GetId(),
                RelativePath = $"/{name}",
                GlobalPath = save_path
            });
        }

        private string RandomName(string prefix = "") =>
            $"img{prefix}_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}.png";

        public void SaveFileOptimize(IFormFile file)
        {
            ProcessImageSettings settings = new ProcessImageSettings
            {
                ResizeMode = CropScaleMode.Crop,
                SaveFormat = FileFormat.Jpeg,
                JpegQuality = 100,
                JpegSubsampleMode = ChromaSubsampleMode.Subsample420
            };

            int id = GetId();

            foreach (var (Width, Height) in imgSizes)
            {
                settings.Width = Width;
                settings.Height = Height;

                var name = RandomName(Width.ToString());
                var save_path = Path.Combine(_env.WebRootPath, name);

                using (var fileStream = new FileStream(save_path, FileMode.Create))
                {
                    MagicImageProcessor.ProcessImage(file.OpenReadStream(), fileStream, settings);
                }

                Files.Add(new File
                {
                    Id = id,
                    Width = Width,
                    RelativePath = $"/{name}",
                    GlobalPath = save_path
                });
            }
        }

        private int GetId() => Id++;

        public FileStream GetImageStream(int id, int width)
        {
            var path = GetFile(id, GetBestWidth(width)).GlobalPath;

            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        private int GetBestWidth(int width)
        {
            foreach (var (Width, Height) in imgSizes)
                if (Width >= width)
                    return Width;

            return imgSizes[imgSizes.Count - 1].Width;
        }
    }

    public class File
    {
        public int Id { get; set; }
        public int Width { get; set; }
        public string RelativePath { get; set; }
        public string GlobalPath { get; set; }
    }
}
