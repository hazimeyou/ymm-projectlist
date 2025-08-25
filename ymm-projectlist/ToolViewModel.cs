using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace ymm_projectlist
{
    public class ToolViewModel : INotifyPropertyChanged
    {
        private string _dataFile;

        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();

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
            string dllDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _dataFile = Path.Combine(dllDir, "projects.json");

            LoadBackgroundImage();
            LoadProjects();

            OpenProjectCommand = new RelayCommand(param =>
            {
                if (param is ProjectModel project)
                    OpenProjectInBackground(project.Path);
            });
        }

        public void AddProject(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Projects.Any(p => string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase)))
            {
                var project = new ProjectModel
                {
                    Name = Path.GetFileName(path),
                    Path = Path.GetFullPath(path)
                };
                Projects.Add(project);
                SaveProjects();
            }
        }

        private void LoadBackgroundImage()
        {
            try
            {
                string dllDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string imagePath = Path.Combine(dllDir, "Images", "bg.png");

                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    BackgroundImage = bitmap;
                }
            }
            catch { BackgroundImage = null; }
        }

        private void OpenProjectInBackground(string path)
        {
            if (!File.Exists(path)) return;

            Task.Run(() =>
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                System.Diagnostics.Process.Start(psi);
            });
        }

        private void SaveProjects()
        {
            try
            {
                var data = Projects.Select(p => p.Path).ToArray();
                File.WriteAllText(_dataFile, JsonSerializer.Serialize(data));
            }
            catch { }
        }

        private void LoadProjects()
        {
            try
            {
                if (!File.Exists(_dataFile)) return;

                var paths = JsonSerializer.Deserialize<string[]>(File.ReadAllText(_dataFile));
                if (paths == null) return;

                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        Projects.Add(new ProjectModel
                        {
                            Name = Path.GetFileName(path),
                            Path = path
                        });
                    }
                }
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _action;
        public RelayCommand(Action<object> action) => _action = action;
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action(parameter);
    }
}
