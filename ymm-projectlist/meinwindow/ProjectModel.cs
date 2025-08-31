using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ymm_projectlist.sousawindow;
using ymm_projectlist.urakata;

namespace ymm_projectlist.meinwindow
{
    public class ProjectModel : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _path;
        public string Path
        {
            get => _path;
            set { _path = value; OnPropertyChanged(); }
        }

        private string _thumbnailPath;
        public string ThumbnailPath
        {
            get => _thumbnailPath;
            set
            {
                _thumbnailPath = value;
                LogHelper.WriteAsync($"[ProjectModel] ThumbnailPath: {_thumbnailPath}");
                OnPropertyChanged();
            }
        }
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set { _thumbnail = value;
                LogHelper.WriteAsync($"[ProjectModel] Thumbnail: {Thumbnail}");
                OnPropertyChanged(); }
        }

        private DateTime _lastModified;
        public DateTime LastModified
        {
            get => _lastModified;
            set { _lastModified = value; OnPropertyChanged(); 
                LogHelper.WriteAsync($"[ProjectModel] LastModified: {LastModified}"); 
                OnPropertyChanged(nameof(LastModifiedDisplay)); }
        }

        // 表示用
        public string LastModifiedDisplay => LastModified == DateTime.MinValue
            ? ""
            : LastModified.ToString("yyyy/MM/dd tt h:mm", System.Globalization.CultureInfo.CurrentCulture);

        // タイムラインアイテムを保持
        public ObservableCollection<TimelineItemModel> TimelineItems { get; set; } = new ObservableCollection<TimelineItemModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
