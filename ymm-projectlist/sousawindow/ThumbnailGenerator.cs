using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ymm_projectlist.urakata;
using YukkuriMovieMaker.Plugin;

namespace ymm_projectlist.sousawindow
{
    public static class ThumbnailGenerator
    {

        public static async Task<string> GenerateThumbnailAsync(string ymmpPath)
        {
            string exeDir = YukkuriMovieMaker.Commons.AppDirectories.AppDirectory;
            string userDir = YukkuriMovieMaker.Commons.AppDirectories.UserDirectory;
            string fileName = Path.GetFileNameWithoutExtension(ymmpPath);
            string dllDir = Path.GetDirectoryName(YukkuriMovieMaker.Commons.AppDirectories.PluginDirectory);
            string projectlistplugin = Path.Combine(dllDir,"plugin", "ymm-projectlist");
            string thumbDir = Path.Combine(projectlistplugin, "Images");
            Directory.CreateDirectory(thumbDir);

            string thumbPath = Path.Combine(thumbDir, fileName + ".png");
            string ffmpegPath = Path.Combine(userDir, "resources", "ffmpeg", "ffmpeg.exe");
            LogHelper.WriteAsync($"[ThumbnailGenerator] 編集済みexeDir: {exeDir}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] fileName: {fileName}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] dllDir: {dllDir}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] projectlistplugin: {projectlistplugin}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] thumbDir: {thumbDir}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] thumbPath: {thumbPath}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] ffmpegPath: {ffmpegPath}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] ymmpPath: {ymmpPath}");
            //await LogHelper.WriteAsync($"[INFO] YMMPPath: {ymmpPath}");
            //await LogHelper.WriteAsync($"[INFO] FFmpegPath: {ffmpegPath}");
            //await LogHelper.WriteAsync($"[INFO] ThumbnailPath: {thumbPath}");

            if (!File.Exists(ffmpegPath))
            {
                //await LogHelper.WriteAsync($"[ERROR] ffmpeg.exe が見つかりません: {ffmpegPath}");
                throw new FileNotFoundException("ffmpeg.exe が見つかりません", ffmpegPath);
            }

            if (!File.Exists(ymmpPath))
            {
               // LogHelper.WriteAsync($"[ERROR] YMMPファイルが存在しません: {ymmpPath}");
                throw new FileNotFoundException("YMMPファイルが存在しません", ymmpPath);
            }

            string jsonText = await File.ReadAllTextAsync(ymmpPath);
            JsonNode? root = JsonNode.Parse(jsonText);

            if (root == null)
            {
               // await LogHelper.WriteAsync("[ERROR] YMMPファイルのJSON解析に失敗しました。");
                throw new Exception("YMMPファイルのJSON解析に失敗しました。");
            }

            string? firstVideoPath = root["Timelines"]?
                                       .AsArray()?
                                       .FirstOrDefault()?["Items"]?
                                       .AsArray()?
                                       .FirstOrDefault()?["FilePath"]?.ToString();

            if (string.IsNullOrEmpty(firstVideoPath))
            {
               // await LogHelper.WriteAsync("[WARN] YMMP内の動画ファイルが見つかりません。YMMP自体を入力に使用します。");
                firstVideoPath = ymmpPath;
            }

          //  await LogHelper.WriteAsync($"[INFO] Using video path: {firstVideoPath}");

            string args = $"-i \"{firstVideoPath}\" -vf scale=320:-1 -vframes 1 \"{thumbPath}\" -y";
            // await LogHelper.WriteAsync($"[INFO] FFmpeg args: {args}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] firstVideoPath: {firstVideoPath}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] thumbPath: {thumbPath}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] args: {args}");
            await Task.Run(async () =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) ;
                        _ = LogHelper.WriteAsync($"[FFMPEG STDOUT] {e.Data}");
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) ;
                        _ = LogHelper.WriteAsync($"[FFMPEG STDERR] {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            });

            if (File.Exists(thumbPath)) 
             await LogHelper.WriteAsync($"[SUCCESS] Thumbnail生成完了: {thumbPath}");
            else
                await LogHelper.WriteAsync($"[ERROR] Thumbnail生成失敗: {thumbPath}");

            return thumbPath;
        }
    }
}
