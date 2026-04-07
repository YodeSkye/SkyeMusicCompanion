
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

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Write("UNHANDLED: " + e.ExceptionObject);
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log.Write("UNOBSERVED: " + e.Exception);
                e.SetObserved();
            };

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
        public static void OnUI(Action action)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Log.Write("UI error: " + ex);
                }
            });
        }
        public static MainPage? GetMainPage()
        {
            var win = Application.Current?.Windows.FirstOrDefault();
            if (win?.Page is MainPage mp)
                return mp;

            if (win?.Page is NavigationPage nav && nav.CurrentPage is MainPage mp2)
                return mp2;

            if (win?.Page is Shell shell && shell.CurrentPage is MainPage mp3)
                return mp3;

            return null;
        }

        //private void OnConnected()
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        var root = Application.Current?.Windows[0].Page;

        //        if (root is MainPage mp)
        //        {
        //            mp.SetConnectionState(true);
        //        }
        //        else if (root is NavigationPage nav && nav.CurrentPage is MainPage mp2)
        //        {
        //            mp2.SetConnectionState(true);
        //        }
        //        else if (root is Shell shell && shell.CurrentPage is MainPage mp3)
        //        {
        //            mp3.SetConnectionState(true);
        //        }
        //    });
        //    var shell = Application.Current?.Windows[0].Page as AppShell;
        //    shell?.SetReconnectVisible(false);
        //}
        private void OnConnected()
        {
            OnUI(() =>
            {
                var mp = GetMainPage();
                if (mp == null) return;

                mp.SetConnectionState(true);
            });

            OnUI(() =>
            {
                if (Application.Current?.Windows.FirstOrDefault()?.Page is AppShell shell)
                    shell.SetReconnectVisible(false);
            });
        }
        private void OnDisconnected()
        {
            OnUI(() =>
            {
                var mp = GetMainPage();
                if (mp == null) return;

                mp.SetConnectionState(false);
                mp.ClearNowPlaying();
            });

            OnUI(() =>
            {
                if (Application.Current?.Windows.FirstOrDefault()?.Page is AppShell shell)
                    shell.SetReconnectVisible(true);
            });
        }
        private void OnServerClosing()
        {
            OnUI(() =>
            {
                var mp = GetMainPage();
                if (mp == null) return;

                mp.SetConnectionState(false);
                mp.ClearNowPlaying();
            });

            Connection.Disconnect();

            OnUI(() =>
            {
                if (Application.Current?.Windows.FirstOrDefault()?.Page is AppShell shell)
                    shell.SetReconnectVisible(true);
            });

            Log.Write("SERVER CLOSING — Companion disconnected gracefully.");
        }
        private void OnNowPlayingReceived(string payload)
        {
            var now = NowPlayingParser.Parse(payload);
            Log.Write("ONNOWPLAYINGRECEIVED" + Environment.NewLine +
                      $"{now.CurrentTitle}" + Environment.NewLine +
                      $"{now.Title}" + Environment.NewLine +
                      $"{now.Path}" + Environment.NewLine +
                      $"{now.ArtworkBase64?.Length ?? 0}" + Environment.NewLine +
                      $"{now.Position}" + Environment.NewLine +
                      $"{now.Duration}");

            OnUI(() =>
            {
                var mp = GetMainPage();
                if (mp == null) return;

                mp.UpdateNowPlaying(now);
            });
        }
        private void OnUnknownMessageReceived(string type)
        {
            Log.Write("UNKNOWN MESSAGE: " + type);
        }

    }
}
