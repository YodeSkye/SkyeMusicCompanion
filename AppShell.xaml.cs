using System.Windows.Input;

namespace SkyeMusicCompanion;

public partial class AppShell : Shell
{
    public ICommand ExitAppCommand { get; }

    public AppShell()
    {
        InitializeComponent();
        Navigating += OnShellNavigating;

        // Initialize the Exit command for the Flyout menu
        ExitAppCommand = new Command(OnExitClicked);
        BindingContext = this;

        // Set version and build number from MAUI AppInfo
        VersionLabel.Text = $"v{AppInfo.Current.VersionString} (Build {AppInfo.Current.BuildString})";
    }

    private void OnExitClicked()
    {
        try
        {
            // 1. Explicitly invoke connection cleanup
            var connection = IPlatformApplication.Current?.Services.GetService<Services.CompanionConnection>();
            connection?.Disconnect();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during exit disconnect: {ex.Message}");
        }

#if ANDROID
        // 2. Kill the Android process entirely to drop the socket and clear the tray notification
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            var activity = Platform.CurrentActivity;
            activity?.FinishAndRemoveTask();
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        });
#else
        // This cleanly closes the app on Windows, macOS, etc.
        Application.Current?.Quit();
#endif
    }

    private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        // Detect navigation to the reconnect item
        if (e.Target?.Location.OriginalString.Contains("ReconnectRoute") == true)
        {
            e.Cancel(); // stop Shell from navigating to the blank page

            // Close the flyout
            Shell.Current.FlyoutIsPresented = false;

            // Perform reconnect
            await App.Connection.ReconnectAsync();
        }
    }

    private async void OnReconnectMenuClicked(object sender, EventArgs e)
    {
        await App.Connection.ReconnectAsync();
    }
}