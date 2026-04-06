
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            bool isConnected = App.Connection.IsConnected;
            SetConnectionState(isConnected);

            App.Connection.RequestNowPlaying();
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
            PlayStateLabel.Text = now.PlayState;
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
        private string FormatTime(int seconds)
        {
            if (seconds <= 0)
                return "0:00";

            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalMinutes}:{ts.Seconds:D2}";
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
