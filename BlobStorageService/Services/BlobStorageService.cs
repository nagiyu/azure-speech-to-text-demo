using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Azure.Storage.Blobs;

namespace BlobStorageService.Services
{
    public class BlobStorageService
    {
        private readonly IConfiguration configuration;

        public BlobStorageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<IEnumerable<string>> GetItems(string containerName)
        {
            var container = CreateBlobContainerClient(containerName);

            var blobItems = container.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, string.Empty);

            var items = new List<string>();

            await foreach (var blobItem in blobItems)
            {
                items.Add(blobItem.Name);
            }

            return items;
        }

        public async Task UploadFile(string containerName, string filePath, string fileName = "")
        {
            var container = CreateBlobContainerClient(containerName);

            using var stream = File.OpenRead(filePath);
            await container.UploadBlobAsync(fileName == "" ? Path.GetFileName(filePath) : fileName, stream);
        }

        public async Task DeleteFile(string containerName, string fileName)
        {
            var container = CreateBlobContainerClient(containerName);

            await container.DeleteBlobAsync(fileName);
        }

        private BlobContainerClient CreateBlobContainerClient(string containerName)
        {
            var connectionString = configuration["ConnectionString"];
            return new BlobContainerClient(connectionString, containerName);
        }
    }
}
