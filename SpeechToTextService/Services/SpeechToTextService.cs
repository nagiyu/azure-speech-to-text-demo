using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpeechToTextService.Services
{
    public class SpeechToTextService
    {
        private readonly string region;
        private readonly string subscriptionKey;
        private readonly string storageAccountName;
        private readonly string containerName;

        public SpeechToTextService(IConfiguration configuration)
        {
            region = configuration["SpeechToText:Region"];
            subscriptionKey = configuration["SpeechToText:SubscriptionKey"];
            storageAccountName = configuration["SpeechToText:StorageAccountName"];
            containerName = configuration["SpeechToText:ContainerName"];
        }

        public async Task<string> CreateTranscriptionAsync(string fileName)
        {
            var url = $"https://{region}.api.cognitive.microsoft.com/speechtotext/v3.2/transcriptions";
            var contentUrls = new[] { $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{fileName}" };
            var requestBody = new
            {
                contentUrls,
                locale = "ja-JP",
                displayName = "Speech Transcriber",
                model = (string)null
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            var selfUrl = (jsonResponse["self"]?.ToString()) ?? throw new Exception("Response does not contain 'self' URL.");

            var transcriptionId = selfUrl.Split('/').Last();
            if (string.IsNullOrEmpty(transcriptionId))
            {
                throw new Exception("Unable to extract transcription ID from 'self' URL.");
            }

            return transcriptionId;
        }

        public async Task<string> GetTranscriptionStatusAsync(string transcriptionId)
        {
            var url = $"https://{region}.api.cognitive.microsoft.com/speechtotext/v3.2/transcriptions/{transcriptionId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            var status = jsonResponse["status"]?.ToString() ?? throw new Exception("Response does not contain 'status'.");

            return status;
        }

        public async Task<string> GetTranscriptionFileUrlAsync(string transcriptionId)
        {
            var url = $"https://{region}.api.cognitive.microsoft.com/speechtotext/v3.2/transcriptions/{transcriptionId}/files";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            var transcriptionFile = jsonResponse["values"]?.FirstOrDefault(v => v["kind"]?.ToString() == "Transcription");
            if (transcriptionFile == null)
            {
                throw new Exception("Response does not contain a 'Transcription' file.");
            }

            var contentUrl = transcriptionFile["links"]?["contentUrl"]?.ToString() ?? throw new Exception("Transcription file does not contain 'contentUrl'.");

            return contentUrl;
        }

        public async Task<string> GetCombinedRecognizedPhraseDisplayAsync(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            var combinedRecognizedPhrases = jsonResponse["combinedRecognizedPhrases"]?.FirstOrDefault();
            if (combinedRecognizedPhrases == null)
            {
                throw new Exception("Response does not contain 'combinedRecognizedPhrases'.");
            }

            var display = combinedRecognizedPhrases["display"]?.ToString();
            if (string.IsNullOrEmpty(display))
            {
                throw new Exception("Combined recognized phrase does not contain 'display'.");
            }

            return display;
        }
    }
}
