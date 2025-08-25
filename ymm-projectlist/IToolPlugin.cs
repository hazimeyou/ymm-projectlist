using YukkuriMovieMaker.Plugin;

namespace ymm_projectlist
{
    public class MyToolPlugin : IToolPlugin
    {
        public string Name => "MyToolPlugin";
        public Type ViewModelType => typeof(ToolViewModel);
        public Type ViewType => typeof(ToolView);
    }
}
