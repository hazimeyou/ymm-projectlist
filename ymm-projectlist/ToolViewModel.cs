using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YukkuriMovieMaker;

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
            string dllDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _dataFile = System.IO.Path.Combine(dllDir, "projects.json");

            LoadBackgroundImage();
            OpenProjectCommand = new RelayCommand(param =>
            {
                if (param is ProjectModel project)
                    OpenProjectInBackground(project.Path);
            });

            _ = LoadProjectsAsync();
        }

        // プロジェクトロード時のサムネイル生成・非同期設定
        public async Task LoadProjectsAsync()
        {
            try
            {
                if (!File.Exists(_dataFile))
                {
                    await LogHelper.WriteAsync("projects.json が存在しません。");
                    return;
                }

                string[] ymmpPaths = JsonSerializer.Deserialize<string[]>(await File.ReadAllTextAsync(_dataFile)) ?? Array.Empty<string>();

                foreach (var ymmpPath in ymmpPaths)
                {
                    if (!File.Exists(ymmpPath))
                    {
                        await LogHelper.WriteAsync($"ファイルが存在しません: {ymmpPath}");
                        continue;
                    }

                    var fileInfo = new System.IO.FileInfo(ymmpPath);
                    string thumbPath = await ThumbnailGenerator.GenerateThumbnailAsync(ymmpPath);

                    var project = new ProjectModel
                    {
                        Name = System.IO.Path.GetFileName(ymmpPath),
                        Path = ymmpPath,
                        ThumbnailPath = thumbPath,
                        LastModified = fileInfo.LastWriteTime
                    };

                    Projects.Add(project);

                    // 非同期でサムネイルを読み込んで UI に反映
                    _ = LoadThumbnailAsync(project);

                    await LogHelper.WriteAsync($"プロジェクトロード: {ymmpPath}");
                }

                // 日付で新しい順に並び替え
                var sorted = Projects.OrderByDescending(p => p.LastModified).ToList();
                Projects.Clear();
                foreach (var p in sorted) Projects.Add(p);
            }
            catch (Exception ex)
            {
                await LogHelper.WriteAsync($"LoadProjectsAsync例外: {ex}");
            }
        }

        public async Task AddProjectAsync(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || Projects.Any(p => string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase)))
                {
                    await LogHelper.WriteAsync($"AddProject無効または重複: {path}");
                    return;
                }

                string thumbPath = await ThumbnailGenerator.GenerateThumbnailAsync(path);
                var fileInfo = new System.IO.FileInfo(path);

                var project = new ProjectModel
                {
                    Name = System.IO.Path.GetFileName(path),
                    Path = path,
                    ThumbnailPath = thumbPath,
                    LastModified = fileInfo.LastWriteTime
                };

                Projects.Add(project);

                // サムネイルを非同期で読み込み
                _ = LoadThumbnailAsync(project);

                // 日付で新しい順に並び替え
                var sorted = Projects.OrderByDescending(p => p.LastModified).ToList();
                Projects.Clear();
                foreach (var p in sorted) Projects.Add(p);

                SaveProjects();
                await LogHelper.WriteAsync($"プロジェクト追加: {path}");
            }
            catch (Exception ex)
            {
                await LogHelper.WriteAsync($"AddProjectAsync例外: {ex}");
            }
        }

        public async Task LoadThumbnailAsync(ProjectModel project)
        {
            try
            {
                string thumbPath = project.ThumbnailPath;

                if (!File.Exists(thumbPath))
                {
                    // プレースホルダを使用
                    string dllDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    thumbPath = Path.Combine(dllDir, "Images", "placeholder.png");
                }

                await Task.Run(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(thumbPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 320;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        project.Thumbnail = bitmap;
                    });
                });

                await LogHelper.WriteAsync($"サムネイル読み込み成功: {thumbPath}");
            }
            catch (Exception ex)
            {
                await LogHelper.WriteAsync($"LoadThumbnailAsync例外: {ex}");
            }
        }


        private void LoadBackgroundImage()
        {
            try
            {
                string dllDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string imagePath = System.IO.Path.Combine(dllDir, "Images", "bg.png");

                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    BackgroundImage = bitmap;
                    _ = LogHelper.WriteAsync($"背景画像読み込み成功: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                _ = LogHelper.WriteAsync($"背景画像読み込み失敗: {ex}");
            }
        }

        private void OpenProjectInBackground(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    _ = LogHelper.WriteAsync($"OpenProject失敗(存在しない): {path}");
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

                _ = LogHelper.WriteAsync($"プロジェクト起動: {path}");
            }
            catch (Exception ex)
            {
                _ = LogHelper.WriteAsync($"OpenProject例外: {ex}");
            }
        }

        private void SaveProjects()
        {
            try
            {
                var data = Projects.Select(p => p.Path).ToArray();
                File.WriteAllText(_dataFile, JsonSerializer.Serialize(data));
                _ = LogHelper.WriteAsync("projects.json 保存成功");
            }
            catch (Exception ex)
            {
                _ = LogHelper.WriteAsync($"SaveProjects例外: {ex}");
            }
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
