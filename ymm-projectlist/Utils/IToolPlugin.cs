
namespace ymm_projectlist.Utils
{
    public class MyToolPlugin : IToolPlugin
    {
        public string Name => "プロジェクトリスト";
        public Type ViewModelType => typeof(ToolViewModel);
        public Type ViewType => typeof(ToolView);
    }
}
