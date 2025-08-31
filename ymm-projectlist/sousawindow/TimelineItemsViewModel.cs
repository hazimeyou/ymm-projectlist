using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using ymm_projectlist.urakata;

namespace ymm_projectlist.sousawindow
{
    public class TimelineItemsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TimelineItemModel> TimelineItems { get; set; } = new();

        private TimelineItemModel _selectedItem;
        public TimelineItemModel SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        public ICommand OpenItemCommand { get; }

        public TimelineItemsViewModel()
        {
            // サンプル用のコマンド
            OpenItemCommand = new RelayCommand(param =>
            {
                if (param is TimelineItemModel item)
                {
                    // 実際はファイルを開くなどの処理
                    _ = LogHelper.WriteAsync($"タイムラインアイテムを開く: {item.FullPath}");
                }
            });
        }

        // ToolViewModel から TimelineItems をセットする用
        public void SetItems(ObservableCollection<TimelineItemModel> items)
        {
            TimelineItems = items;
            OnPropertyChanged(nameof(TimelineItems));
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        private string _fullPath;
        public string FullPath
        {
            get => _fullPath;
            set { _fullPath = value; OnPropertyChanged(); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(); }
        }

        private Brush _rowBackground = Brushes.Transparent;
        public Brush RowBackground
        {
            get => _rowBackground;
            set { _rowBackground = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
