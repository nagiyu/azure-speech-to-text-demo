using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SpeechTranscriber.Services;

namespace SpeechTranscriber.Controllers
{
    public class SpeechToTextController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly BlobStorageService.Services.BlobStorageService blobStorageService;
        private readonly SpeechToTextService.Services.SpeechToTextService speechToTextService;

        private static string fileName = "";

        public SpeechToTextController(
            IConfiguration configuration, 
            BlobStorageService.Services.BlobStorageService blobStorageService,
            SpeechToTextService.Services.SpeechToTextService speechToTextService)
        {
            this.configuration = configuration;
            this.blobStorageService = blobStorageService;
            this.speechToTextService = speechToTextService;
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

            // 一意のファイル名を Guid.NewGuid() で生成するが、拡張子は変えない
            var extension = Path.GetExtension(audioFile.FileName);
            fileName = $"{Guid.NewGuid()}{extension}";

            // 一時ファイルに保存
            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // AudioConverter で音声ファイルをモノラルに変換
            var monoFilePath = Path.GetTempFileName();
            
            try
            {
                AudioConverter.ConvertToMono(filePath, monoFilePath);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"音声ファイルの変換に失敗しました: {ex.Message}";
                return View("Index");
            }

            var containerName = configuration["ContainerName"];

            // BlobStorageService で音声ファイルをアップロード
            await blobStorageService.UploadFile(containerName, monoFilePath, fileName);

            // 一時ファイルを削除
            System.IO.File.Delete(filePath);
            System.IO.File.Delete(monoFilePath);

            // SpeechToTextService で音声ファイルを変換
            ViewBag.TranscriptionId = await speechToTextService.CreateTranscriptionAsync(fileName);

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> GetTranscriptionStatus(string transcriptionId)
        {
            if (string.IsNullOrEmpty(transcriptionId))
            {
                ViewBag.Message = "TranscriptionId が指定されていません。";
                return View("Index");
            }

            // SpeechToTextService で文字起こしの状態を取得
            var status = await speechToTextService.GetTranscriptionStatusAsync(transcriptionId);
            ViewBag.TranscriptionStatus = status;

            // status が Failed の場合、エラーメッセージとして transcriptionId を表示
            if (status == "Failed")
            {
                ViewBag.Message = transcriptionId;
            }

            ViewBag.TranscriptionId = transcriptionId;

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> GetTranscriptionResult(string transcriptionId)
        {
            if (string.IsNullOrEmpty(transcriptionId))
            {
                ViewBag.Message = "TranscriptionId が指定されていません。";
                return View("Index");
            }

            // 文字起こしファイルの URL を取得
            var transcriptionFileUrl = await speechToTextService.GetTranscriptionFileUrlAsync(transcriptionId);

            // 文字起こし結果を取得
            ViewBag.TranscriptionResult = await speechToTextService.GetCombinedRecognizedPhraseDisplayAsync(transcriptionFileUrl);

            ViewBag.TranscriptionId = transcriptionId;
            ViewBag.TranscriptionStatus = "Succeeded";

            // BlobStorageService で音声ファイルを削除
            var containerName = configuration["ContainerName"];
            await blobStorageService.DeleteFile(containerName, fileName);

            return View("Index");
        }

        [HttpPost]
        public IActionResult ClearViewBags()
        {
            ViewBag.TranscriptionId = null;
            ViewBag.TranscriptionStatus = null;
            ViewBag.TranscriptionResult = null;
            ViewBag.Message = null;

            return View("Index");
        }
    }
}
