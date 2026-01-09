namespace ymm_projectlist.meinwindow
{
    public class ToolViewModel : INotifyPropertyChanged
    {
        private readonly string _dataFile = Path.Combine(YukkuriMovieMaker.Commons.AppDirectories.PluginDirectory, "ymm-projectlist", "projects.json");

        public ObservableCollection<ProjectModel> Projects { get; set; } = new();
        public ObservableCollection<TimelineItemModel> TimelineItems { get; set; } = new();

        private ProjectModel _selectedProject;
        public ProjectModel SelectedProject
        {
            get => _selectedProject;
            set { _selectedProject = value; OnPropertyChanged(); }
        }

        private ImageSource _backgroundImage;
        public ImageSource BackgroundImage
        {
            get => _backgroundImage;
            set { _backgroundImage = value; OnPropertyChanged(); }
        }

        public ICommand OpenProjectCommand { get; }

        public ToolViewModel()
        {
            PathManager.EnsureDirectories();
           // LoadBackgroundImage();
            OpenProjectCommand = new RelayCommand(param =>
            {
                if (param is ProjectModel project)
                    OpenProjectInBackground(project.Path);
            });
          //  _ = LoadProjectsAsync();
        }


        private void OpenProjectInBackground(string path)//完成
        {
            try
            {
                if (!File.Exists(path))
                {
                    return;
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                _ = LogHelper.WriteAsync($"[OpenProject] 例外: {ex}");
            }
        }



        public class YmmProject
        {
            public string FilePath { get; set; }
            public List<Timeline> Timelines { get; set; } = new();
        }

        public class Timeline
        {
            public string Name { get; set; }
            public List<ItemBase> Items { get; set; } = new();
        }

        public class ItemBase { }

        public class VideoItem : ItemBase
        {
            public string FilePath { get; set; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
