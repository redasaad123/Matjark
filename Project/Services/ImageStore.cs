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
        private List<string> _images;

        public ImageStore( Microsoft.AspNetCore.Hosting.IHostingEnvironment hosting)
        {
            this.hosting = hosting;
            _images = new List<string>();
        }

        public List<string> StoreImage(List<IFormFile> images)
        {

            string uploads = Path.Combine(hosting.WebRootPath, "Images");

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            foreach (var img in images)
            {
                string fileName = Path.GetFileName(img.FileName);

                string fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    img.CopyTo(stream);
                }

                _images.Add(fileName);
            }
            return _images;


        }

        
    }
}
