using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.InterFace.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class StorageBlobServices : IStorageBlobService
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        public StorageBlobServices(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration["AzureStorage:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
            }
            _containerName = _configuration["AzureStorage:ContainerName"] ?? "uploads";
        }

        public async Task<List<string>> UploadFileAsync(List<IFormFile> files, string folderName = "Images")
        {
            if (_blobServiceClient == null)
            {
                throw new InvalidOperationException("AzureStorage:ConnectionString is missing or invalid in appsettings.json.");
            }

            // 1. التحقق من القائمة قبل البدء في الـ Loop
            if (files == null || files.Count == 0)
                throw new ArgumentException("File list is empty");

            var uploadedUrls = new List<string>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // إنشاء الحاوية مرة واحدة فقط خارج الـ Loop
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // 2. رفع الملفات بالتوازي (Parallel) لسرعة أفضل مع Async
            var uploadTasks = files.Select(async file =>
            {
                if (file.Length > 0)
                {
                    var blobName = $"{folderName}/{Guid.NewGuid()}_{file.FileName}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
                    }

                    return blobClient.Uri.ToString();
                }
                return null;
            });

            var results = await Task.WhenAll(uploadTasks);
            return results.Where(url => url != null).ToList();
        }








    }
}
