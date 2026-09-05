using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.InterFace.Services
{
    public interface IStorageBlobService
    {
        Task<List<string>> UploadFileAsync(List< IFormFile> file);
    }
}
