namespace ymm_projectlist.meinwindow
{
    public class ProjectListViewModel : INotifyPropertyChanged
    {
        private string _dataFile;
        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();

        private ImageSource _backgroundImage;
        public ImageSource BackgroundImage
        {
            get => _backgroundImage;
            set { _backgroundImage = value; OnPropertyChanged(); }
        }


        public async Task AddProjectAsync(string ymmpPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ymmpPath) || !File.Exists(ymmpPath))
                    return;

                // すでに存在するパスは無視
                if (Projects.Any(p => p.Path == ymmpPath))
                    return;

                var fileInfo = new FileInfo(ymmpPath);
                string thumbPath = await ThumbnailGenerator.GenerateThumbnailAsync(ymmpPath);

                var project = new ProjectModel
                {
                    Name = Path.GetFileName(ymmpPath),
                    Path = ymmpPath,
                    ThumbnailPath = thumbPath,
                    LastModified = fileInfo.LastWriteTime
                };

                Projects.Add(project);
                await LoadThumbnailAsync(project);

                // 更新日時でソート
                var sorted = Projects.OrderByDescending(p => p.LastModified).ToList();
                Projects.Clear();
                foreach (var p in sorted) Projects.Add(p);

                SaveProjects();
                LogHelper.WriteAsync($"[ProjectListViewModel] AddProjectAsync: {ymmpPath}");
            }
            catch (Exception ex)
            {
                LogHelper.WriteAsync($"[ProjectListViewModel] AddProjectAsync Error: {ex}");
            }
        }

        // 同期呼び出し用ラッパー
        public void AddProject(string ymmpPath) => _ = AddProjectAsync(ymmpPath);

        public void OpenProject(ProjectModel project)
        {
            try
            {
                if (project == null || !File.Exists(project.Path)) return;

                var psi = new ProcessStartInfo
                {
                    FileName = project.Path,
                    UseShellExecute = true
                };
                Process.Start(psi);
                LogHelper.WriteAsync($"[ProjectListViewModel] OpenProject: {project.Path}");
            }
            catch (Exception ex)
            {
                LogHelper.WriteAsync($"[ProjectListViewModel] OpenProject Error: {ex}");
            }
        }

        private void SaveProjects()
        {
            try
            {
                var paths = Projects.Select(p => p.Path).ToArray();
                File.WriteAllText(_dataFile, JsonSerializer.Serialize(paths));
                LogHelper.WriteAsync($"[ProjectListViewModel] SaveProjects: {_dataFile}");
            }
            catch (Exception ex)
            {
                LogHelper.WriteAsync($"[ProjectListViewModel] SaveProjects Error: {ex}");
            }
        }

        public ProjectListViewModel()
        {
            string dllDir = Path.GetDirectoryName(YukkuriMovieMaker.Commons.AppDirectories.PluginDirectory);
            _dataFile = Path.Combine(dllDir,"plugin","ymm-projectlist", "projects.json");
            LogHelper.WriteAsync($"[ProjectListViewModel] dllDir: {dllDir}");
            LogHelper.WriteAsync($"[ProjectListViewModel] _dataFile: {_dataFile}");

            LoadBackgroundImage();
            _ = LoadProjectsAsync();
        }

        public async Task LoadProjectsAsync()
        {
            try
            {
                if (!File.Exists(_dataFile))
                {
                    //      await LogHelper.WriteAsync("projects.json が存在しません。");
                    return;
                }

                string[] ymmpPaths = JsonSerializer.Deserialize<string[]>(await File.ReadAllTextAsync(_dataFile)) ?? Array.Empty<string>();

                foreach (var ymmpPath in ymmpPaths)
                {
                    LogHelper.WriteAsync($"[ProjectListViewModel] ymmpPaths: {ymmpPaths}");
                    if (!File.Exists(ymmpPath))
                    {
                        //       await LogHelper.WriteAsync($"ファイルが存在しません: {ymmpPath}");
                        continue;
                    }

                    var fileInfo = new FileInfo(ymmpPath);
                    string thumbPath = await ThumbnailGenerator.GenerateThumbnailAsync(ymmpPath);
                    LogHelper.WriteAsync($"[ProjectListViewModel] fileInfo: {fileInfo}");
                    LogHelper.WriteAsync($"[ProjectListViewModel] thumbPath: {thumbPath}");
                    var project = new ProjectModel
                    {
                        Name = Path.GetFileName(ymmpPath),
                        Path = ymmpPath,
                        ThumbnailPath = thumbPath,
                        LastModified = fileInfo.LastWriteTime
                    };

                    Projects.Add(project);

                    _ = LoadThumbnailAsync(project);

                    //    await LogHelper.WriteAsync($"プロジェクトロード: {ymmpPath}");
                }

                var sorted = Projects.OrderByDescending(p => p.LastModified).ToList();
                Projects.Clear();
                foreach (var p in sorted) Projects.Add(p);
            }
            catch (Exception ex)
            {
                //    await LogHelper.WriteAsync($"LoadProjectsAsync例外: {ex}");
            }
        }

        public async Task LoadThumbnailAsync(ProjectModel project)
        {
            try
            {
                string thumbPath = project.ThumbnailPath;
                if (!File.Exists(thumbPath))
                {
                    string dllDir = Path.GetDirectoryName(YukkuriMovieMaker.Commons.AppDirectories.PluginDirectory);
                    thumbPath = Path.Combine(dllDir, "Images", "placeholder.png");
                }

                await Task.Run(() =>
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(thumbPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.DecodePixelWidth = 320;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            project.Thumbnail = bitmap;
                        });
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteAsync($"[LoadThumbnailAsync] 画像読み込み失敗: {thumbPath}, {ex.Message}");
                    }
                });


                // await LogHelper.WriteAsync($"サムネイル読み込み成功: {thumbPath}");
            }
            catch (Exception ex)
            {
                //    await LogHelper.WriteAsync($"LoadThumbnailAsync例外: {ex}");
            }
        }
       // public void AddProject(string ymmpPath)
       // {
       //     if (!File.Exists(ymmpPath)) return;
       //
        //    var fileInfo = new FileInfo(ymmpPath);
        //    var project = new ProjectModel
        ///    {
       //         Name = Path.GetFileName(ymmpPath),
        //        Path = ymmpPath,
       //         ThumbnailPath = "", // サムネイルは後でロード
       //         LastModified = fileInfo.LastWriteTime
       //     };

      //      Projects.Add(project);
      //  }

        private void LoadBackgroundImage()
        {
            try
            {
                string dllDir = Path.GetDirectoryName(YukkuriMovieMaker.Commons.AppDirectories.PluginDirectory);
                string imagePath = Path.Combine(dllDir, "ymm-projectlist", "Images", "bg.png");
                LogHelper.WriteAsync($"[ProjectListViewModel] imagePath: {imagePath}");
                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    BackgroundImage = bitmap;
                    //   _ = LogHelper.WriteAsync($"背景画像読み込み成功: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                // _ = LogHelper.WriteAsync($"背景画像読み込み失敗: {ex}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
