
namespace ymm_projectlist.meinwindow
{
    public partial class ToolView : UserControl
    {
        private ProjectListViewModel _viewModel;

        public ToolView()
        {
            InitializeComponent();

            _viewModel = new ProjectListViewModel();
            DataContext = _viewModel;

            AllowDrop = true;
            Drop += ToolView_Drop;

            // コンストラクタで非同期処理を呼ぶ
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _viewModel.LoadProjectsAsync();
        }


        private void ToolView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    if (file.EndsWith(".ymmp"))
                    {
                        // 今は ProjectListViewModel に AddProjectAsync がないので追加して呼び出す
                        _viewModel.AddProject(file);
                    }
                }
            }
        }

        private void ProjectTile_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProjectModel project)
            {
                var window = new FileListWindow(project.Path);
                window.Show();
            }
        }

        private void ProjectTile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProjectModel project)
            {
                // ここでプロジェクトを開く処理を ProjectListViewModel に追加
                _viewModel.OpenProject(project);
            }
        }
    }
}