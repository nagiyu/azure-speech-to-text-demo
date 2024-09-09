using System;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SpeechTranscriber.Services
{
    public class AudioConverter
    {
        /// <summary>
        /// 複数チャンネルの音声ファイルをモノラルに変換します。
        /// </summary>
        /// <param name="inputFilePath">入力音声ファイルのパス</param>
        /// <param name="outputFilePath">出力音声ファイルのパス</param>
        public static void ConvertToMono(string inputFilePath, string outputFilePath)
        {
            try
            {
                using (var reader = new MediaFoundationReader(inputFilePath))  // MediaFoundationReaderを使用
                {
                    // モノラル音声の場合、変換をスキップ
                    if (reader.WaveFormat.Channels == 1)
                    {
                        Console.WriteLine("音声はすでにモノラルです。変換はスキップされます。");
                        SaveAsIs(reader, outputFilePath);
                    }
                    else
                    {
                        // ステレオ音声の場合はモノラルに変換
                        ConvertToMonoInternal(reader, outputFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラーが発生しました: {ex.Message}");

                throw;
            }
        }

        private static void ConvertToMonoInternal(WaveStream reader, string outputFilePath)
        {
            var monoFormat = new WaveFormat(reader.WaveFormat.SampleRate, 1);
            var sampleProvider = reader.ToSampleProvider();
            var conversionStream = new StereoToMonoSampleProvider(sampleProvider);

            using (var writer = new WaveFileWriter(outputFilePath, monoFormat))
            {
                float[] buffer = new float[1024];
                int samplesRead;

                while ((samplesRead = conversionStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.WriteSamples(buffer, 0, samplesRead);
                }
            }

            Console.WriteLine("音声のモノラル変換が完了しました！");
        }

        private static void SaveAsIs(WaveStream reader, string outputFilePath)
        {
            // モノラルの音声をそのまま保存
            using (var writer = new WaveFileWriter(outputFilePath, reader.WaveFormat))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, bytesRead);
                }
            }

            Console.WriteLine("モノラル音声がそのまま保存されました！");
        }
    }
}
