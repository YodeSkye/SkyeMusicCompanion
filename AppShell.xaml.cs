
namespace SkyeMusicCompanion
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Navigating += OnShellNavigating;
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

        public void SetReconnectVisible(bool visible)
        {
            ReconnectItem.IsVisible = visible;
        }

    }
}
