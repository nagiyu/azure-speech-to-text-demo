using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace BlobStorageService.Tests.Services
{
    [TestClass]
    public class BlobStorageServiceTest
    {
        private IConfiguration? configuration;

        [TestInitialize]
        public void TestInitialize()
        {
            var basePath = Directory.GetCurrentDirectory();

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configuration = builder.Build();
        }

        [TestMethod]
        public async Task GetItemsTest()
        {
            if (configuration == null)
            {
                Assert.Fail("Configuration is null.");
                return;
            }

            var containerName = configuration["ContainerName"];

            var blobStorageService = new BlobStorageService.Services.BlobStorageService(configuration);

            // Act
            var items = await blobStorageService.GetItems(containerName);

            Debug.WriteLine(string.Join(", ", items));

            // Assert
            Assert.IsTrue(items.Any());
        }

        [TestMethod]
        public async Task UploadFileTest()
        {
            if (configuration == null)
            {
                Assert.Fail("Configuration is null.");
                return;
            }

            // Arrange
            var containerName = configuration["ContainerName"];
            var filePath = configuration["UploadFilePath"];

            var blobStorageService = new BlobStorageService.Services.BlobStorageService(configuration);

            // Act
            await blobStorageService.UploadFile(containerName, filePath);

            // Assert
            var items = await blobStorageService.GetItems(containerName);
            Assert.IsTrue(items.Contains(Path.GetFileName(filePath)));
        }
    }
}
