using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;

namespace StorageCleaner.Controls
{
    public sealed partial class FileCard : UserControl
    {
        public string FileName;
        public string FullPath;
        public StorageFile PreviewFile;
        public static bool IsReadingFile = false;
        private bool endThumbnailLoading = false;
        public FileCard(string name, string fullPath)
        {
            this.InitializeComponent();
            ToolTip toolTip = new ToolTip();
            toolTip.Content = fullPath;
            ToolTipService.SetToolTip(FilePathBlock, toolTip);
            FileNameBlock.Text = name;
            FilePathBlock.Text = fullPath;
            FileName = name;
            FullPath = fullPath;
            LoadThumbnail();
        }
        private async void LoadThumbnail()
        {
            while ( !endThumbnailLoading && !DuplicationsWindow.IsRunning )
            {
                if (!IsReadingFile)
                {
                    try
                    {
                        IsReadingFile = true;
                        PreviewFile = await StorageFile.GetFileFromPathAsync(FullPath);
                        var thumbItem = await PreviewFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem);
                        var bmp = new BitmapImage();
                        bmp.SetSource(thumbItem);
                        ThumbnailImage.Source = bmp;
                        endThumbnailLoading = true;
                        IsReadingFile = false;
                    }
                    catch
                    {
                        IsReadingFile = false;
                    }
                }
                else
                {
                    await Task.Delay(10);
                }
            }
            
        }
    }
}
