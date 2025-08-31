using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Shell;
using ymm_projectlist.urakata;

namespace ymm_projectlist.sousawindow
{
    public class TimelineItemModel : INotifyPropertyChanged
    {
        private string fileName;
        private string fullPath;
        private bool isChecked;
        private Brush rowBackground = Brushes.Transparent;
        private string status;

        public string FileName
        {
            get => fileName;
            set { fileName = value; OnPropertyChanged(); }

        }

        public string FullPath
        {
            get => fullPath;
            set { fullPath = value; OnPropertyChanged(); }
        }

        public bool IsChecked
        {
            get => isChecked;
            set { isChecked = value; OnPropertyChanged(); }
        }

        public Brush RowBackground
        {
            get => rowBackground;
            set { rowBackground = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            LogHelper.WriteAsync($"TimelineItemModel:FileName: {FileName}");
            LogHelper.WriteAsync($"TimelineItemModel:FullPath: {FullPath}");
            LogHelper.WriteAsync($"TimelineItemModel:IsChecked: {IsChecked}");
            LogHelper.WriteAsync($"TimelineItemModel:RowBackground: {RowBackground}");
            LogHelper.WriteAsync($"TimelineItemModel:Status: {Status}");
            LogHelper.WriteAsync($"TimelineItemModel:PropertyChanged: {PropertyChanged}");
            LogHelper.WriteAsync($"TimelineItemModel:name: {name}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
