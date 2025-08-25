using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ymm_projectlist
{
    public partial class ToolView : UserControl
    {
        public ToolView()
        {
            InitializeComponent();

            // D&D を有効化
            AllowDrop = true;
            Drop += ToolView_Drop;
        }

        // ファイルを D&D したときの処理
        private void ToolView_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is ToolViewModel vm && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    // .ymmp ファイルのみ追加
                    if (file.EndsWith(".ymmp"))
                        vm.AddProject(file);
                }
            }
        }

        // タイルクリック時の処理
        private void ProjectTile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ToolViewModel vm)
            {
                if (sender is Border border && border.DataContext is ProjectModel project)
                {
                    // プロジェクトを非表示 CMD で起動
                    vm.OpenProjectCommand.Execute(project);
                }
            }
        }
    }
}
