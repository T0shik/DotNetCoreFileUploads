using HowToVideoFiles.BackgroundTasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Enums;

namespace HowToVideoFiles.Controllers
{
    public class HomeController : Controller
    {
        private string _dir;
        private IBackgroundQueue _queue;

        public HomeController(
            IHostingEnvironment env,
            IBackgroundQueue queue)
        {
            _dir = env.WebRootPath;
            _queue = queue;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            using (var fileStream =
                new FileStream(Path.Combine(_dir, "file.mp4"), FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult TrimVideo(double start, double end)
        {
            _queue.QueueTask(async token =>
            {
                await ConvertVideo(start, end, token);
            });

            return RedirectToAction("Index");
        }

        public async Task<bool> ConvertVideo(double start, double end, CancellationToken ct)
        {
            try
            {
                var input = Path.Combine(_dir, "file.mp4");
                var output = Path.Combine(_dir, "converted.mp4");

                FFmpeg.ExecutablesPath = Path.Combine(_dir, "ffmpeg");

                var startSpan = TimeSpan.FromSeconds(start);
                var endSpan = TimeSpan.FromSeconds(end);
                var duration = endSpan - startSpan;

                var info = await MediaInfo.Get(input);

                var videoStream = info.VideoStreams.First()
                    .SetCodec(VideoCodec.H264)
                    .SetSize(VideoSize.Hd480)
                    .Split(startSpan, duration);

                await Conversion.New()
                    .AddStream(videoStream)
                    .SetOutput(output)
                    .Start(ct);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return true;
        }

    }
}
