using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace StorageCleaner
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow m_appwindow;

        public Window DuplicationsWindow;

        public string WindowTitle { get => _windowTitle;
            set
            {
                Title = value;
                TitleBlock.Text = value;
                _windowTitle = value;
            }
        }
        private string _windowTitle;

        public StorageFolder TargetFolder { get => _targetFolder;
            set
            {
                _targetFolder = value;
                TargetFolderPicker.Text = value.Path;
            }
        }

        private StorageFolder _targetFolder;

        public MainWindow()
        {
            this.InitializeComponent();
            SystemBackdrop = new MicaBackdrop();
            InitializeTitleBar();
            WindowTitle = "StorageCleaner";
            Activated += OnActivated;
        }
        private void InitializeTitleBar()
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
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

        private async void OnActivated(object sender, WindowActivatedEventArgs e)
        {
            await FocusManager.TryFocusAsync(TargetFolderPicker, FocusState.Unfocused);
        }

        private async void OpenFolderPicker(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            FolderPicker folderPicker = new();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(folderPicker, hwnd);
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null) {
                TargetFolder = folder;
                var drive = DriveInfo.GetDrives()
                .Where(x => x.Name == Path.GetPathRoot(folder.Path)).First();
                var label = drive.VolumeLabel == string.Empty ? (string)Application.Current.Resources["LocalDisk"] : drive.VolumeLabel;
                DriveName.Text = label + " (" + drive.Name + ")";
                Debug.WriteLine(drive.AvailableFreeSpace);
                Debug.WriteLine(drive.TotalSize);
                DriveUsageRing.Value = 100 - drive.AvailableFreeSpace * 100 / drive.TotalSize;
            }
        }

        private void StartButtonClicked(object sender, RoutedEventArgs e)
        {
            DuplicationsWindow = new DuplicationsWindow(TargetFolder, IgnoreFileName.IsChecked.Value, (int)WorkerCountSlider.Value);
            DuplicationsWindow.Activate();
            Close();
        }

    }
}
