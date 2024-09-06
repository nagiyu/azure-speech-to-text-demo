using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SpeechTranscriber.Controllers
{
    public class SpeechToTextController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly BlobStorageService.Services.BlobStorageService blobStorageService;

        public SpeechToTextController(IConfiguration configuration, BlobStorageService.Services.BlobStorageService blobStorageService)
        {
            this.configuration = configuration;
            this.blobStorageService = blobStorageService;
        }

        // GET: SpeechToText
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Transcribe(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                ViewBag.Message = "音声ファイルが選択されていません。";
                return View("Index");
            }

            // ファイル名を取得
            var fileName = Path.GetFileName(audioFile.FileName);

            // 一時ファイルに保存
            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            var containerName = configuration["ContainerName"];

            // BlobStorageService で音声ファイルをアップロード
            await blobStorageService.UploadFile(containerName, filePath, fileName);

            // TODO: 文字起こしを実行

            // 一時ファイルを削除
            System.IO.File.Delete(filePath);

            // 結果を表示
            ViewBag.Transcription = "";
            return View("Index");
        }
    }
}
