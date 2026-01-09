
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
            string projectlistplugin = Path.Combine(dllDir, "plugin", "ymm-projectlist");
            string thumbDir = Path.Combine(projectlistplugin, "Images");
            Directory.CreateDirectory(thumbDir);

            string thumbPath = Path.Combine(thumbDir, fileName + ".png");
            string ffmpegPath = Path.Combine(userDir, "resources", "ffmpeg", "ffmpeg.exe");

            LogHelper.WriteAsync($"[ThumbnailGenerator] thumbPath: {thumbPath}");
            LogHelper.WriteAsync($"[ThumbnailGenerator] ymmpPath: {ymmpPath}");

            // 既にサムネイルがある場合はスキップ
            if (File.Exists(thumbPath))
            {
                await LogHelper.WriteAsync($"[ThumbnailGenerator] 既存サムネイルを使用: {thumbPath}");
                return thumbPath;
            }

            if (!File.Exists(ffmpegPath))
                throw new FileNotFoundException("ffmpeg.exe が見つかりません", ffmpegPath);

            if (!File.Exists(ymmpPath))
                throw new FileNotFoundException("YMMPファイルが存在しません", ymmpPath);

            string jsonText = await File.ReadAllTextAsync(ymmpPath);
            var root = JsonNode.Parse(jsonText);
            if (root == null)
                throw new Exception("YMMPファイルのJSON解析に失敗しました。");

            string? firstVideoPath = root["Timelines"]?
                                       .AsArray()?
                                       .FirstOrDefault()?["Items"]?
                                       .AsArray()?
                                       .FirstOrDefault()?["FilePath"]?.ToString();

            if (string.IsNullOrEmpty(firstVideoPath))
            {
                firstVideoPath = ymmpPath; // YMMP自体を入力に
            }

            string args = $"-i \"{firstVideoPath}\" -vf scale=320:-1 -vframes 1 \"{thumbPath}\" -y";
            LogHelper.WriteAsync($"[ThumbnailGenerator] args: {args}");

            await Task.Run(() =>
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
                    if (!string.IsNullOrEmpty(e.Data))
                        _ = LogHelper.WriteAsync($"[FFMPEG STDOUT] {e.Data}");
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
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
