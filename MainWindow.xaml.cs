using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace LightBarForKeyboard
{
    public partial class MainWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int left, top, right, bottom; }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [DllImport("shell32.dll")]
        public static extern int SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        private const int ABM_NEW = 0x0000;
        private const int ABM_REMOVE = 0x0001;
        private const int ABM_QUERYPOS = 0x0002;
        private const int ABM_SETPOS = 0x0003;
        private const int ABE_BOTTOM = 3;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        public const double DefaultHeight = 40.0;
        public const double MinAllowedHeight = 40.0;
        public const double MaxAllowedHeight = 400.0;

        public static readonly Version CurrentAppVersion = new Version(1, 1, 0);
        private UpdateInfo? availableUpdate = null;
        private bool isDownloadingUpdate = false;

        private double currentHeight = DefaultHeight;
        private double lastExpandedHeight = 120.0;
        private long lastToggleTimestamp = 0;

        private bool isDocked = false;
        private bool isAppBarRegistered = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            SetDockedMode();
            UpdateHeightMenuChecks();
            UpdateModeMenuChecks();
            UpdateColorMenuChecks("White");
            await CheckForUpdatesInBackgroundAsync();
        }

        public void ApplyHeight(double newHeight)
        {
            newHeight = Math.Clamp(newHeight, MinAllowedHeight, MaxAllowedHeight);

            if (Math.Abs(newHeight - currentHeight) < 0.1 && Math.Abs(this.Height - newHeight) < 0.1)
            {
                return;
            }

            double oldHeight = currentHeight;
            currentHeight = newHeight;

            if (newHeight > DefaultHeight)
            {
                lastExpandedHeight = newHeight;
            }

            if (isDocked)
            {
                SetDockedMode();
            }
            else
            {
                double delta = newHeight - oldHeight;
                double targetTop = this.Top - delta;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                if (targetTop < 0)
                {
                    targetTop = 0;
                }
                else if (targetTop + newHeight > screenHeight)
                {
                    targetTop = Math.Max(0, screenHeight - newHeight);
                }

                this.Top = targetTop;
                this.Height = newHeight;
            }

            UpdateHeightMenuChecks();
        }

        public void ToggleHeight()
        {
            long now = Environment.TickCount64;
            if (now - lastToggleTimestamp < 250)
            {
                return;
            }
            lastToggleTimestamp = now;

            if (Math.Abs(currentHeight - DefaultHeight) < 1.0)
            {
                double target = lastExpandedHeight > DefaultHeight ? lastExpandedHeight : 120.0;
                ApplyHeight(target);
            }
            else
            {
                ApplyHeight(DefaultHeight);
            }
        }

        private void Window_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ToggleHeight();
                e.Handled = true;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double step = 20.0;
            double targetHeight = currentHeight + (e.Delta > 0 ? step : -step);
            ApplyHeight(targetHeight);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                ToggleHeight();
                e.Handled = true;
                return;
            }

            if (!isDocked && e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MenuToggleBoost_Click(object sender, RoutedEventArgs e)
        {
            ToggleHeight();
        }

        private void MenuRevertDefault_Click(object sender, RoutedEventArgs e)
        {
            ApplyHeight(DefaultHeight);
        }

        private void Height40_Click(object sender, RoutedEventArgs e) => ApplyHeight(40);
        private void Height80_Click(object sender, RoutedEventArgs e) => ApplyHeight(80);
        private void Height120_Click(object sender, RoutedEventArgs e) => ApplyHeight(120);
        private void Height160_Click(object sender, RoutedEventArgs e) => ApplyHeight(160);
        private void Height200_Click(object sender, RoutedEventArgs e) => ApplyHeight(200);

        private void UpdateHeightMenuChecks()
        {
            if (MenuItemH40 != null) MenuItemH40.IsChecked = Math.Abs(currentHeight - 40) < 1.0;
            if (MenuItemH80 != null) MenuItemH80.IsChecked = Math.Abs(currentHeight - 80) < 1.0;
            if (MenuItemH120 != null) MenuItemH120.IsChecked = Math.Abs(currentHeight - 120) < 1.0;
            if (MenuItemH160 != null) MenuItemH160.IsChecked = Math.Abs(currentHeight - 160) < 1.0;
            if (MenuItemH200 != null) MenuItemH200.IsChecked = Math.Abs(currentHeight - 200) < 1.0;

            bool isExpanded = Math.Abs(currentHeight - DefaultHeight) >= 1.0;

            if (MenuRevertItem != null)
            {
                MenuRevertItem.IsEnabled = isExpanded;
            }

            if (MenuBoostToggleItem != null)
            {
                MenuBoostToggleItem.Header = isExpanded
                    ? "⚡ Revert to Default Size (40px)"
                    : $"⚡ Boost Light ({lastExpandedHeight:0}px)";
            }
        }

        private void UpdateModeMenuChecks()
        {
            if (MenuDockedItem != null) MenuDockedItem.IsChecked = isDocked;
            if (MenuFloatingItem != null) MenuFloatingItem.IsChecked = !isDocked;
        }

        private void UpdateColorMenuChecks(string selected)
        {
            if (MenuColorWhite != null) MenuColorWhite.IsChecked = (selected == "White");
            if (MenuColorGreen != null) MenuColorGreen.IsChecked = (selected == "Green");
            if (MenuColorRed != null) MenuColorRed.IsChecked = (selected == "Red");
            if (MenuColorYellow != null) MenuColorYellow.IsChecked = (selected == "Yellow");
        }

        private void MenuDocked_Click(object sender, RoutedEventArgs e)
        {
            if (!isDocked)
            {
                SetDockedMode();
                UpdateModeMenuChecks();
            }
        }

        private void MenuFloating_Click(object sender, RoutedEventArgs e)
        {
            if (isDocked)
            {
                isDocked = false;
                UnregisterAppBar();
                UpdateModeMenuChecks();
            }
        }

        private void ColorWhite_Click(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(Colors.White);
            UpdateColorMenuChecks("White");
        }

        private void ColorGreen_Click(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            UpdateColorMenuChecks("Green");
        }

        private void ColorRed_Click(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            UpdateColorMenuChecks("Red");
        }

        private void ColorYellow_Click(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(Color.FromRgb(255, 191, 0));
            UpdateColorMenuChecks("Yellow");
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SetDockedMode()
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            
            PresentationSource source = PresentationSource.FromVisual(this);
            double dpiY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;
            double dpiX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;

            APPBARDATA data = new APPBARDATA();
            data.cbSize = Marshal.SizeOf(data);
            data.hWnd = hWnd;

            if (!isAppBarRegistered)
            {
                SHAppBarMessage(ABM_NEW, ref data);
                isAppBarRegistered = true;
            }

            data.uEdge = ABE_BOTTOM;
            data.rc.left = 0;
            data.rc.right = GetSystemMetrics(SM_CXSCREEN);
            data.rc.top = 0;
            data.rc.bottom = GetSystemMetrics(SM_CYSCREEN);

            SHAppBarMessage(ABM_QUERYPOS, ref data);

            int physicalHeight = (int)(currentHeight * dpiY);
            data.rc.top = data.rc.bottom - physicalHeight;

            SHAppBarMessage(ABM_SETPOS, ref data);

            this.Left = data.rc.left / dpiX;
            this.Top = data.rc.top / dpiY;
            this.Width = (data.rc.right - data.rc.left) / dpiX;
            this.Height = currentHeight;

            isDocked = true;
        }

        private void UnregisterAppBar()
        {
            if (isAppBarRegistered)
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                APPBARDATA data = new APPBARDATA();
                data.cbSize = Marshal.SizeOf(data);
                data.hWnd = hWnd;
                SHAppBarMessage(ABM_REMOVE, ref data);
                isAppBarRegistered = false;
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            UnregisterAppBar();
        }

        private async Task CheckForUpdatesInBackgroundAsync()
        {
            try
            {
                await Task.Delay(2000);
                UpdateInfo? update = await UpdateService.CheckForUpdatesAsync(CurrentAppVersion);
                if (update != null)
                {
                    availableUpdate = update;
                    ShowUpdateAvailable(update);
                }
            }
            catch
            {
                // Background check should not fail or disrupt the application
            }
        }

        private void ShowUpdateAvailable(UpdateInfo update)
        {
            TxtUpdateVersion.Text = update.TagName;
            PillUpdate.Visibility = Visibility.Visible;
            if (MenuUpdateAvailable != null)
            {
                MenuUpdateAvailable.Header = $"🚀 Update Available: {update.TagName} (Click to Install)";
                MenuUpdateAvailable.Visibility = Visibility.Visible;
            }
            if (MenuUpdateSeparator != null)
            {
                MenuUpdateSeparator.Visibility = Visibility.Visible;
            }
        }

        private async void MenuCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateInfo? update = await UpdateService.CheckForUpdatesAsync(CurrentAppVersion);
                if (update != null)
                {
                    availableUpdate = update;
                    ShowUpdateAvailable(update);

                    string releaseNotesSnippet = !string.IsNullOrWhiteSpace(update.ReleaseNotes)
                        ? $"\n\nRelease Notes:\n{(update.ReleaseNotes.Length > 300 ? update.ReleaseNotes.Substring(0, 300) + "..." : update.ReleaseNotes)}"
                        : "";

                    MessageBoxResult result = MessageBox.Show(
                        $"A new version ({update.TagName}) of LightBar For Keyboard is available!{releaseNotesSnippet}\n\nWould you like to download and install it now?",
                        "Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        await StartUpdateProcessAsync();
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"You are already using the latest version of LightBar For Keyboard (v{CurrentAppVersion.ToString(3)}).",
                        "Up to Date",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to check for updates at this time.\n\nError: {ex.Message}",
                    "Check for Updates",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void PillUpdate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            await StartUpdateProcessAsync();
        }

        private async void MenuUpdateAvailable_Click(object sender, RoutedEventArgs e)
        {
            await StartUpdateProcessAsync();
        }

        private void BtnDismissUpdate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            PillUpdate.Visibility = Visibility.Collapsed;
        }

        private async Task StartUpdateProcessAsync()
        {
            if (availableUpdate == null || isDownloadingUpdate)
            {
                return;
            }

            isDownloadingUpdate = true;
            PillUpdate.Visibility = Visibility.Visible;
            ProgressUpdate.Visibility = Visibility.Visible;
            ProgressUpdate.Value = 0;
            TxtUpdateAction.Text = " • Starting download...";

            try
            {
                await UpdateService.DownloadAndInstallAsync(availableUpdate, percent =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressUpdate.Value = percent;
                        TxtUpdateAction.Text = $" • Downloading... ({percent:0}%)";
                    });
                });
            }
            catch (Exception ex)
            {
                isDownloadingUpdate = false;
                ProgressUpdate.Visibility = Visibility.Collapsed;
                TxtUpdateAction.Text = " • Download failed (Click to retry)";

                MessageBoxResult result = MessageBox.Show(
                    $"Failed to download the update automatically: {ex.Message}\n\nWould you like to open the GitHub Releases page to download it manually?",
                    "Update Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    UpdateService.OpenInBrowser(availableUpdate.ReleaseUrl);
                }
            }
        }
    }
}