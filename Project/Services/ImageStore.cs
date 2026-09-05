using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ImageStore
    {
        private readonly IWebHostEnvironment hosting;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public ImageStore(IWebHostEnvironment hosting)
        {
            this.hosting = hosting;
        }

        public async Task<List<string>> StoreImageAsync(List<IFormFile> images)
        {
            string uploads = Path.Combine(hosting.WebRootPath, "Images");

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var resultImages = new List<string>();

            if (images != null)
            {
                foreach (var img in images)
                {
                    if (img == null || img.Length == 0 || img.Length > MaxFileSize) continue;

                    string ext = Path.GetExtension(img.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(ext)) continue;

                    string fileName = Guid.NewGuid().ToString() + ext;
                    string fullPath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }

                    resultImages.Add(fileName);
                }
            }

            return resultImages;
        }

        public List<string> StoreImage(List<IFormFile> images)
        {
            return StoreImageAsync(images).GetAwaiter().GetResult();
        }

        public void DeleteImage(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)) return;

            try
            {
                string cleanFileName = Path.GetFileName(imageName);
                string fullPath = Path.Combine(hosting.WebRootPath, "Images", cleanFileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Ignore file in use or permission errors
            }
        }

        public void DeleteImages(IEnumerable<string>? images)
        {
            if (images == null) return;

            foreach (var img in images)
            {
                DeleteImage(img);
            }
        }
    }
}
