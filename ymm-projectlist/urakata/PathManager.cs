using System;
using System.IO;
using System.Reflection;
using System.Windows;
using YukkuriMovieMaker;
using YukkuriMovieMaker.Commons;
namespace ymm_projectlist.urakata
{
    public static class PathManager
    {
        // アプリのルートディレクトリ
        public static string AppPath => AppDirectories.AppPath;
        public static string AppDirectory => AppDirectories.AppDirectory;
        public static string UserDirectory => AppDirectories.UserDirectory;
        public static string PluginDirectory => AppDirectories.PluginDirectory;
        public static string BackupDirectory => AppDirectories.BackupDirectory;
        public static string TemporaryDirectory => AppDirectories.TemporaryDirectory;
        public static string ResourceDirectory => AppDirectories.ResourceDirectory;


        public static void EnsureDirectories()
        {
            LogHelper.WriteAsync($"[PathManager] AppPath: {AppPath}");
            LogHelper.WriteAsync($"[INPathManagerFO] AppDirectory: {AppDirectory}");
            LogHelper.WriteAsync($"[PathManager] UserDirectory: {UserDirectory}");
            LogHelper.WriteAsync($"[PathManager] PluginDirectory: {PluginDirectory}");
            LogHelper.WriteAsync($"[PathManager] BackupDirectory: {BackupDirectory}");
            LogHelper.WriteAsync($"[PathManager] TemporaryDirectory: {TemporaryDirectory}");
            LogHelper.WriteAsync($"[PathManager] ResourceDirectory: {ResourceDirectory}");

        }

        //内部管理
        //ログ強化済み(2130)
        //パス管理済み(2130)

    }
}
