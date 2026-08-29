using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ImageStore
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting;

        public ImageStore(Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting)
        {
            this.hosting = hosting;
        }

        public List<string> StoreImage(List<IFormFile> images)
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
                    if (img == null || img.Length == 0) continue;

                    string ext = Path.GetExtension(img.FileName);
                    string fileName = Guid.NewGuid().ToString() + ext;
                    string fullPath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        img.CopyTo(stream);
                    }

                    resultImages.Add(fileName);
                }
            }

            return resultImages;
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
