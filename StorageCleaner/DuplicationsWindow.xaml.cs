using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using WinRT.Interop;
using Windows.Storage;
using StorageCleaner.Controls;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace StorageCleaner
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DuplicationsWindow : Window
    {
        private AppWindow m_appwindow;
        public List<ProcessThread> ProcessThreads;
        public static bool IsRunning = true;
        public static DuplicationsWindow Instance { get; private set; }

        private string scanned = string.Empty;

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                Title = value;
                TitleBlock.Text = value;
                _windowTitle = value;
            }
        }
        private string _windowTitle;
        public DuplicationsWindow(StorageFolder folder, bool isIgnoreFileName)
        {
            this.InitializeComponent();
            Closed += CloseButtonClicked;
            Instance = this;
            SystemBackdrop = new MicaBackdrop();
            InitializeTitleBar();
            WindowTitle = (string)Application.Current.Resources["SearchingDuplications"];
            FilesDataBase.IgnoreFileName = isIgnoreFileName;
            ProcessThreads = new() { new ProcessThread(0, ThreadMode.Find, folder) };
            for (int i = 1; i < 8; i++)
            {
                ProcessThreads.Add(new ProcessThread(i, ThreadMode.Calc, folder));
            }
            ProcessThreads.ForEach(p => p.Start());
            UpdateRendering();
        }
        private void InitializeTitleBar()
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.Default);
            m_appwindow = GetAppWindowForCurrentWindow();
            SetTitleBar(AppTitleBar);
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titlebar = m_appwindow.TitleBar;
                titlebar.ExtendsContentIntoTitleBar = true;
                titlebar.ButtonBackgroundColor = Colors.Transparent;
                titlebar.ButtonInactiveBackgroundColor = Colors.Transparent;
                //titlebar.PreferredHeightOption = TitleBarHeightOption.Tall;
            }
        }
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private void CloseButtonClicked(object sender, WindowEventArgs e)
        {
            IsRunning = false;
        }

        private async void UpdateRendering()
        {
            int c;
            while (ProcessThreads.Where(t => t.ThreadMode == ThreadMode.Find).Select(t => t.IsRunning).Contains(true))
            {
                await Task.Delay(500);
                c = ProcessThread.fileCount;
                if(c > 1)
                {
                    WindowTitle = c.ToString()
                    + (string)Application.Current.Resources["FilesFound"];
                }
                else
                {
                    WindowTitle = c.ToString()
                    + (string)Application.Current.Resources["FileFound"];
                }
            }
            c = ProcessThread.fileCount;
            if (c > 1)
            {
                WindowTitle = c.ToString()
                + (string)Application.Current.Resources["FilesFound"];
            }
            else
            {
                WindowTitle = c.ToString()
                + (string)Application.Current.Resources["FileFound"];
            }
            Debug.WriteLine("File collection successed");
            processPr.IsIndeterminate = false;
            processPr.Value = 0;
            while (ProcessThreads.Select(t => t.IsRunning).Contains(true))
            {
                DuplicationsChanged(FilesDataBase.Duplications);
                await Task.Delay(2000);
                updateCount();
            }
            IsRunning = false;
            DuplicationsChanged(FilesDataBase.Duplications);
            
        }

        private void updateCount()
        {
            int c = DuplicationsStackPanel.Children.Count;
            int pt = ProcessThread.checkedCount;
            if(scanned == string.Empty)
            {
                try
                {
                    scanned = (string)Application.Current.Resources["Scanned"];
                }
                catch
                {
                    scanned = string.Empty;
                }
            }
            if (c > 1)
            {
                WindowTitle = c.ToString()
                + (string)Application.Current.Resources["DuplicationsFound"];
                DetailsBlock.Text = scanned + pt.ToString()
                + (string)Application.Current.Resources["Files"];
            }
            else
            {
                WindowTitle = c.ToString()
                + (string)Application.Current.Resources["DuplicationFound"];
                DetailsBlock.Text = scanned + pt.ToString()
                + (string)Application.Current.Resources["File"];
            }
            processPr.Value = 100 * ((float)ProcessThread.checkedCount / (float)ProcessThread.fileCount);
        }

        public void DuplicationsChanged(ConcurrentBag<DuplicatingFiles> duplicatingFiles)
        {
            if (duplicatingFiles.Count != 0)
            {
                DuplicationsStackPanel.Children.Clear();
                foreach (DuplicatingFiles i in duplicatingFiles)
                {
                    DuplicationsCard d = new(i);
                    DuplicationsStackPanel.Children.Add(d);
                }
            }
            
        }
    }
}
