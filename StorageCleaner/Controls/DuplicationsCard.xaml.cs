using Microsoft.UI.Xaml.Controls;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace StorageCleaner.Controls
{
    public sealed partial class DuplicationsCard : UserControl
    {
        public DuplicatingFiles DuplicatingFiles { get; set; }
        public DuplicationsCard(DuplicatingFiles duplicatingFiles)
        {
            this.InitializeComponent();
            DuplicatingFiles = duplicatingFiles;
            FileNameBlock.Text = DuplicatingFiles.Name.First();
            CreateFileCard();
            //InitializeAsync(duplicatingFiles);
        }

        private async void InitializeAsync(DuplicatingFiles duplicatingFiles)
        {
            await Task.Run(() =>
            {
                
                
            });
            
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
