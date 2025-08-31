using YukkuriMovieMaker.Plugin;
using ymm_projectlist.meinwindow;
namespace ymm_projectlist.urakata
{
    public class MyToolPlugin : IToolPlugin
    {
        public string Name => "プロジェクトリスト";
        public Type ViewModelType => typeof(ToolViewModel);
        public Type ViewType => typeof(ToolView);


    }
}
//内部管理
//ログ強化済み(2130)
//パス管理済み(2130)
