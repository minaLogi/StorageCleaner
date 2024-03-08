using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace StorageCleaner.Controls
{
    public sealed partial class DuplicationsCard : UserControl
    {
        private DuplicatingFiles DuplicatingFiles { get; set; }
        public DuplicationsCard(DuplicatingFiles duplicatingFiles)
        {
            this.InitializeComponent();
            DuplicatingFiles = duplicatingFiles;
            FileNameBlock.Text = DuplicatingFiles.Name.First();
            CreateFileCard();
        }

        private void CreateFileCard()
        {
            for(int i = 0; i< DuplicatingFiles.Name.Count; i++)
            {
                FileCard f = new(DuplicatingFiles.Name[i], DuplicatingFiles.FullPath[i]);
                FilesStackPanel.Children.Add(f);
            }
        }
    }
}
