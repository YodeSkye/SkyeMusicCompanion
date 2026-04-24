
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion
{
    public partial class MainPage : ContentPage
    {

        private bool _isMuted;
        private bool _suppressVolumeEvent; // prevents feedback loop
        private int _lastSentVolume = -1;

        public MainPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            App.Connection.Connected += OnConnected;
            App.Connection.Disconnected += OnDisconnected;
            bool isConnected = App.Connection.IsConnected;
            SetConnectionState(isConnected);
   
            App.Connection.VolumeReceived += OnVolumeReceived;
            App.Connection.MuteReceived += OnMuteReceived;
            
            App.Connection.RequestNowPlaying();
            App.Connection.RequestVolume();
            App.Connection.RequestMute();
          
      }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            App.Connection.VolumeReceived -= OnVolumeReceived;
            App.Connection.MuteReceived -= OnMuteReceived;
        }

        public void SetConnectionState(bool connected)
        {
            if (connected)
            {
                ConnectionStatusIcon.Source = "connected.png";
            }
            else
            {
                ConnectionStatusIcon.Source = "disconnected.png";
            }
        }
        public async void UpdateNowPlaying(NowPlaying now)
        {
            // Basic text fields
            bool isPlaying = now.PlayState.Equals("Playing", StringComparison.OrdinalIgnoreCase);
            int npduration = now.Duration;
            if (isPlaying && npduration > 0)
            {
                PlayStateLabel.Text = $"{now.PlayState} ({FormatTime(npduration)})";
            }
            else
            {
                PlayStateLabel.Text = now.PlayState;
            }
            CurrentTitleLabel.Text = now.CurrentTitle;
            TitleLabel.Text = now.Title;

            // Duration + Position (convert seconds → mm:ss)
            PositionLabel.Text = FormatTime(now.Position);
            var duration = now.Duration;
            if (duration <= 0)
            {
                DurationLabel.IsVisible = false;
                DurationTextLabel.IsVisible = false;
            }
            else
            {
                DurationLabel.Text = FormatTime(duration);
                DurationLabel.IsVisible = true;
                DurationTextLabel.IsVisible = true;
            }

            // Artwork
            if (!string.IsNullOrWhiteSpace(now.ArtworkBase64))
            {
                try
                {
                    var bytes = Convert.FromBase64String(now.ArtworkBase64);
                    ArtworkImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    ArtworkImage.IsVisible = true;
                }
                catch
                {
                    ArtworkImage.Source = null;
                    ArtworkImage.IsVisible = false;
                }
            }
            else
            {
                // --- FIX FOR STALE ARTWORK ---
                ArtworkImage.Source = null;
                ArtworkImage.IsVisible = false;         
                await Task.Delay(10);                       
                ArtworkImage.Source = null;                
            }
            if (now.PlayState?.Equals("playing", StringComparison.OrdinalIgnoreCase) == true)
            {
                ToggleLabel.ImageSource = "pause32.png";
                ToggleLabel.Text = "Pause";
            }
            else
            {
                ToggleLabel.ImageSource = "play32.png";
                ToggleLabel.Text = "Play";
            }
        }
        public void ClearNowPlaying()
        {
            PlayStateLabel.Text = "";
            CurrentTitleLabel.Text = "";
            TitleLabel.Text = "";
            DurationLabel.Text = "";
            PositionLabel.Text = "";

            ArtworkImage.Source = null;
            ArtworkImage.IsVisible = false;
        }
        private static string FormatTime(int seconds)
        {
            if (seconds <= 0)
                return "0:00";

            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalMinutes}:{ts.Seconds:D2}";
        }

        private void OnConnected()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                VolumeSlider.IsEnabled = true;
                MuteButton.IsEnabled = true;

                // Ask server for real state
                //App.Connection.RequestVolume();
                //App.Connection.RequestMute();
            });
        }
        private void OnDisconnected()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Reset volume slider
                VolumeSlider.Value = 0;

                // Reset mute state
                _isMuted = false;
                MuteButton.ImageSource = "volume.png";

                // Disable controls (optional but recommended)
                VolumeSlider.IsEnabled = false;
                MuteButton.IsEnabled = false;

            });
        }
        private void OnVolumeReceived(string line)
        {
            var parts = line.Split('|');
            if (parts.Length == 2 && int.TryParse(parts[1], out int percent))
            {
                float vol = percent / 100f;

                Dispatcher.Dispatch(() =>
                {
                    _suppressVolumeEvent = true;
                    VolumeSlider.Value = vol;
                    _suppressVolumeEvent = false;
                });
            }
        }
        private void OnMuteReceived(string line)
        {
            var parts = line.Split('|');
            if (parts.Length == 2 && bool.TryParse(parts[1], out bool muted))
            {
                Dispatcher.Dispatch(() =>
                {
                    _isMuted = muted;
                    MuteButton.ImageSource = muted ? "volumemute.png" : "volume.png";
                });
            }
        }
        private void OnVolumeSliderChanged(object sender, ValueChangedEventArgs e)
        {
            if (_suppressVolumeEvent)
                return;

            float v = (float)e.NewValue;
            int percent = (int)(v * 100);

            if (percent == _lastSentVolume)
                return;

            _lastSentVolume = percent;

            App.Connection.SetVolume(percent);
        }
        //private void OnVolumeSliderChanged(object sender, ValueChangedEventArgs e)
        //{
        //    if (_suppressVolumeEvent)
        //        return;

        //    float v = (float)e.NewValue;
        //    int percent = (int)(v * 100);

        //    App.Connection.SetVolume(percent);
        //}
        private void OnMuteClicked(object sender, EventArgs e)
        {
            App.Connection.SetMute(!_isMuted);
        }
        private async void OnToggleClicked(object sender, EventArgs e)
        {
            App.Connection.SendCommand("toggle");
        }
        private async void OnStopClicked(object sender, EventArgs e)
        {
            App.Connection.SendCommand("stop");
        }
        private async void OnNextClicked(object sender, EventArgs e)
        {
            App.Connection.SendCommand("next");
        }
        private async void OnPreviousClicked(object sender, EventArgs e)
        {
            App.Connection.SendCommand("previous");
        }
    }
}
