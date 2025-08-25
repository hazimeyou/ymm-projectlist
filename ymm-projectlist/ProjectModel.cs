using System;
using System.ComponentModel;
using System.IO; // ← ここを追加
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ymm_projectlist
{
    public class ProjectModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public string LastModified => File.Exists(Path) ? File.GetLastWriteTime(Path).ToString("yyyy/MM/dd HH:mm") : "";

        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                    _thumbnail = LoadThumbnail();
                return _thumbnail;
            }
            set { _thumbnail = value; OnPropertyChanged(); }
        }

        private ImageSource LoadThumbnail()
        {
            try
            {
                // DLL のディレクトリ
                string dllDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                // Images フォルダは DLL と同じ階層
                string imgDir = System.IO.Path.Combine(dllDir, "Images");

                string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(Path);
                string imgPath = System.IO.Path.Combine(imgDir, fileNameWithoutExt + ".png");

                // 存在しなければ default file.png
                if (!File.Exists(imgPath))
                    imgPath = System.IO.Path.Combine(imgDir, "file.png");

                if (File.Exists(imgPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imgPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }

            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
