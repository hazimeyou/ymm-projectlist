using System;
using System.IO;
using System.Threading.Tasks;

namespace ymm_projectlist
{
    public static class LogHelper
    {
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ymm-projectlist.log");

        public static async Task WriteAsync(string message)
        {
            try
            {
                string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                await File.AppendAllTextAsync(LogFile, log);
            }
            catch { /* ログ失敗は無視 */ }
        }
    }
}
