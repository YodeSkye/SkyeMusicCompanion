
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion
{
    public partial class App : Application
    {
        public static CompanionConnection Connection { get; } = new();

        public App()
        {
            InitializeComponent();
            Application.Current!.UserAppTheme = AppTheme.Unspecified;
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        protected override void OnStart()
        {
            base.OnStart();

            // Attach handlers
            Connection.Connected += OnConnected;
            Connection.Disconnected += OnDisconnected;
            Connection.ServerClosing += OnServerClosing;
            Connection.NowPlayingReceived += OnNowPlayingReceived;
            Connection.UnknownMessageReceived += OnUnknownMessageReceived;

            // Start connection AFTER window exists
            _ = Connection.ConnectAsync(Settings.HostServerIp, Settings.HostServerPort);

        }

        private void OnConnected()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var root = Application.Current?.Windows[0].Page;

                if (root is MainPage mp)
                {
                    mp.SetConnectionState(true);
                }
                else if (root is NavigationPage nav && nav.CurrentPage is MainPage mp2)
                {
                    mp2.SetConnectionState(true);
                }
                else if (root is Shell shell && shell.CurrentPage is MainPage mp3)
                {
                    mp3.SetConnectionState(true);
                }
            });
            var shell = Application.Current?.Windows[0].Page as AppShell;
            shell?.SetReconnectVisible(false);
        }
        private void OnDisconnected()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var root = Application.Current?.Windows[0].Page;

                if (root is MainPage mp)
                {
                    mp.SetConnectionState(false);
                    mp.ClearNowPlaying();
                }
                else if (root is NavigationPage nav && nav.CurrentPage is MainPage mp2)
                {
                    mp2.SetConnectionState(false);
                    mp2.ClearNowPlaying();
                }
                else if (root is Shell shell && shell.CurrentPage is MainPage mp3)
                {
                    mp3.SetConnectionState(false);
                    mp3.ClearNowPlaying();
                }
            });
            var shell = Application.Current?.Windows[0].Page as AppShell;
            shell?.SetReconnectVisible(true);
        }
        private void OnServerClosing()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var root = Application.Current?.Windows[0].Page;

                if (root is MainPage mp)
                {
                    mp.SetConnectionState(false);
                    mp.ClearNowPlaying();
                }
                else if (root is NavigationPage nav && nav.CurrentPage is MainPage mp2)
                {
                    mp2.SetConnectionState(false);
                    mp2.ClearNowPlaying();
                }
                else if (root is Shell shell && shell.CurrentPage is MainPage mp3)
                {
                    mp3.SetConnectionState(false);
                    mp3.ClearNowPlaying();
                }
            });

            // Close socket cleanly
            Connection.Disconnect();

            var shell = Application.Current?.Windows[0].Page as AppShell;
            shell?.SetReconnectVisible(true);

            Log.Write("SERVER CLOSING — Companion disconnected gracefully.");
        }
        private void OnNowPlayingReceived(string payload)
        {
            var now = NowPlayingParser.Parse(payload);
            Log.Write("ONNOWPLAYINGRECEIVED" + Environment.NewLine + $"{now.CurrentTitle}" + Environment.NewLine + $"{now.Title}" + Environment.NewLine + $"{now.Path}" + Environment.NewLine + $"{now.ArtworkBase64?.Length ?? 0}" + Environment.NewLine + $"{now.Position}" + Environment.NewLine + $"{now.Duration}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var root = Application.Current?.Windows[0].Page;

                if (root is MainPage mp)
                {
                    mp.UpdateNowPlaying(now);
                    return;
                }

                if (root is NavigationPage nav && nav.CurrentPage is MainPage mp2)
                {
                    mp2.UpdateNowPlaying(now);
                    return;
                }

                if (root is Shell shell && shell.CurrentPage is MainPage mp3)
                {
                    mp3.UpdateNowPlaying(now);
                    return;
                }
            });

        }
        private void OnUnknownMessageReceived(string type)
        {
            Log.Write("UNKNOWN MESSAGE: " + type);
        }
    }
}
