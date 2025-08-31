using System;
using System.IO;
using System.Threading.Tasks;

namespace ymm_projectlist.urakata
{
    public static class LogHelper
    {
        private static readonly string LogFile = Path.Combine(PathManager.PluginDirectory, "ymm-projectlist.log");

        public static async Task WriteAsync(string message)
        {
            try
            {
               // string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                //await File.AppendAllTextAsync(LogFile, log);
            }
            catch { /* ログ失敗は無視 */ }
        }
    }
}
//内部管理
//確定済み(2200)