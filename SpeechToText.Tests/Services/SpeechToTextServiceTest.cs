using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace SpeechToTextService.Tests.Services
{
    [TestClass]
    public class SpeechToTextServiceTest
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
        public async Task CreateTranscriptionAsyncTest()
        {
            if (configuration == null)
            {
                Assert.Fail("Configuration is null.");
                return;
            }

            var fileName = configuration["FileName"];

            var speechToTextService = new SpeechToTextService.Services.SpeechToTextService(configuration);

            // Act
            var result = await speechToTextService.CreateTranscriptionAsync(fileName);

            Debug.WriteLine(result);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        [TestMethod]
        public async Task GetTranscriptionStatusAsyncTest()
        {
            if (configuration == null)
            {
                Assert.Fail("Configuration is null.");
                return;
            }

            var transcriptionId = configuration["TranscriptionId"];

            var speechToTextService = new SpeechToTextService.Services.SpeechToTextService(configuration);

            // Act
            var result = await speechToTextService.GetTranscriptionStatusAsync(transcriptionId);

            Debug.WriteLine(result);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        [TestMethod]
        public async Task GetTranscriptionFileUrlAsyncTest()
        {
            if (configuration == null)
            {
                Assert.Fail("Configuration is null.");
                return;
            }

            var transcriptionId = configuration["TranscriptionId"];

            var speechToTextService = new SpeechToTextService.Services.SpeechToTextService(configuration);

            // Act
            var result = await speechToTextService.GetTranscriptionFileUrlAsync(transcriptionId);

            Debug.WriteLine(result);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        [TestMethod]
        public async Task GetCombinedRecognizedPhraseDisplayAsyncTest()
        {
            if (configuration == null)
            {
                Assert.Fail("Configuration is null.");
                return;
            }

            var url = configuration["TranscriptionFileUrl"];

            var speechToTextService = new SpeechToTextService.Services.SpeechToTextService(configuration);

            // Act
            var result = await speechToTextService.GetCombinedRecognizedPhraseDisplayAsync(url);

            Debug.WriteLine(result);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }
    }
}