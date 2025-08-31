using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using ymm_projectlist.sousawindow;
using ymm_projectlist.urakata;

namespace ymm_projectlist
{
    public partial class FileListWindow : Window
    {
        public ObservableCollection<TimelineItemModel> TimelineItems { get; set; } = new ObservableCollection<TimelineItemModel>();

        public RelayCommand RelinkCommand { get; }
        public RelayCommand ProxyCommand { get; }
        public RelayCommand RelinkSelectedCommand { get; }
        public RelayCommand ProxySelectedCommand { get; }

        // DirectorySelectorで選択されたフォルダのパスを保持
        public string SelectedFolder { get; set; }

        // JSON ファイルパスを保持
        private readonly string projectJsonPath;

        public FileListWindow(string jsonPath)
        {
            InitializeComponent();
            DataContext = this;

            projectJsonPath = jsonPath;

            // コマンド設定
            RelinkCommand = new RelayCommand(param => ReLinkItem(param as TimelineItemModel));
            ProxyCommand = new RelayCommand(param => ProxyItem(param as TimelineItemModel));
            RelinkSelectedCommand = new RelayCommand(param => ReLinkSelected());
            ProxySelectedCommand = new RelayCommand(param => ProxySelected());

            LoadTimelineItems(projectJsonPath);
        }

        private async void LoadTimelineItems(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return;

            string json = await File.ReadAllTextAsync(jsonPath);
            using var doc = JsonDocument.Parse(json);
            var timelines = doc.RootElement.GetProperty("Timelines");

            foreach (var timeline in timelines.EnumerateArray())
            {
                if (!timeline.TryGetProperty("Items", out var items)) continue;

                foreach (var item in items.EnumerateArray())
                {
                    if (!item.TryGetProperty("FilePath", out var fileProp)) continue;

                    string filePath = fileProp.GetString();
                    string fileName = Path.GetFileName(filePath);

                    TimelineItems.Add(new TimelineItemModel
                    {
                        FileName = fileName,
                        FullPath = filePath,
                        IsChecked = false,
                        Status = File.Exists(filePath) ? "存在" : "未検出",
                        RowBackground = File.Exists(filePath) ? Brushes.Transparent : Brushes.Red
                    });
                }
            }
        }

        private void ReLinkItem(TimelineItemModel item)
        {
            if (item == null) return;

            if (File.Exists(item.FullPath))
            {
                item.RowBackground = Brushes.Transparent;
                item.Status = "存在";
                return;
            }

            if (string.IsNullOrEmpty(SelectedFolder))
            {
                item.Status = "フォルダ未選択";
                return;
            }

            item.RowBackground = Brushes.Red;
            item.Status = "再リンク中";

            string fileName = Path.GetFileName(item.FullPath);
            List<string> foundFiles = new List<string>();
            try
            {
                foundFiles = Directory.GetFiles(SelectedFolder, fileName, SearchOption.AllDirectories).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                item.Status = "アクセス拒否";
                return;
            }

            if (foundFiles.Count == 0)
            {
                item.Status = "未検出";
                return;
            }

            item.FullPath = foundFiles[0]; // 最初の候補を選択
            item.RowBackground = Brushes.LightGreen;
            item.Status = "再リンク成功";

            // 再リンク成功したら JSON を更新
            SaveTimelineItems();
        }

        private void ProxyItem(TimelineItemModel item)
        {
            if (item == null) return;
            // プロキシ生成処理
        }

        private void ReLinkSelected()
        {
            foreach (var item in TimelineItems.Where(t => t.IsChecked))
            {
                ReLinkItem(item);
            }
        }

        private void ProxySelected()
        {
            foreach (var item in TimelineItems.Where(t => t.IsChecked))
            {
                ProxyItem(item);
            }
        }
        private void SaveTimelineItems()
        {
            if (!File.Exists(projectJsonPath)) return;

            try
            {
                string json = File.ReadAllText(projectJsonPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement.Clone();

                // JSONを書き換えるためにDictionaryに変換
                var rootDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

                if (rootDict.ContainsKey("Timelines") && rootDict["Timelines"] is JsonElement timelinesElem && timelinesElem.ValueKind == JsonValueKind.Array)
                {
                    var timelinesList = new List<Dictionary<string, object>>();

                    foreach (var timelineElem in timelinesElem.EnumerateArray())
                    {
                        var timelineDict = JsonSerializer.Deserialize<Dictionary<string, object>>(timelineElem.GetRawText())!;
                        if (timelineDict.ContainsKey("Items") && timelineDict["Items"] is JsonElement itemsElem && itemsElem.ValueKind == JsonValueKind.Array)
                        {
                            var itemsList = new List<Dictionary<string, object>>();
                            foreach (var itemElem in itemsElem.EnumerateArray())
                            {
                                var itemDict = JsonSerializer.Deserialize<Dictionary<string, object>>(itemElem.GetRawText())!;
                                if (itemDict.ContainsKey("FilePath"))
                                {
                                    string originalPath = itemDict["FilePath"]?.ToString() ?? "";
                                    var matched = TimelineItems.FirstOrDefault(t => Path.GetFileName(t.FullPath) == Path.GetFileName(originalPath));
                                    if (matched != null)
                                    {
                                        itemDict["FilePath"] = matched.FullPath; // FilePathのみ更新
                                    }
                                }
                                itemsList.Add(itemDict);
                            }
                            timelineDict["Items"] = itemsList;
                        }
                        timelinesList.Add(timelineDict);
                    }

                    rootDict["Timelines"] = timelinesList;

                    string updatedJson = JsonSerializer.Serialize(rootDict, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(projectJsonPath, updatedJson);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JSON 保存中にエラー: {ex}");
            }
        }

    }
}
