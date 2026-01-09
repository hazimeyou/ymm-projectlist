
namespace ymm_projectlist.Utils
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
        //内部管理
        //ログ強化済み(2130)
        //パス管理済み(2130)

    }
}
