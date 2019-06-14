using Microsoft.AspNetCore.Http;

namespace HowToFileUploads.Models
{
    public class SomeForm
    {
        public string Name { get; set; }
        public IFormFile File { get; set; }
    }
}
