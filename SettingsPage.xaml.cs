using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // Load saved values
        HostEntry.Text = Settings.HostServerIp;
        StreamEntry.Text = Settings.StreamServerIp;
        TapActionPicker.SelectedIndex = Settings.PlaylistTapAction == TapAction.Play ? 0 : 1;
        KeepAwakeSwitch.IsToggled = Settings.KeepScreenAwake;
    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        HostEntry.Unfocus();
        StreamEntry.Unfocus();
        TapActionPicker.Unfocus();
        await App.Connection.ReconnectAsync();
    }

    private void OnHostChanged(object sender, EventArgs e)
    {
        string rawInput = HostEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(rawInput))
            return;

        // 1. Handle Web URLs (http:// or https://)
        if (Uri.TryCreate(rawInput, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            // Extract port to setting/UI if non-standard port was supplied
            if (!uri.IsDefaultPort)
            {
                Settings.HostServerPort = uri.Port;
            }

            // Clean out port and path segments -> https://stream.skye.com
            string cleanUrl = $"{uri.Scheme}://{uri.Host}";

            Settings.HostServerIp = cleanUrl;
            HostEntry.Text = cleanUrl;

            Log.Write("SETTINGS: HostServerIp updated to " + cleanUrl);
            return;
        }

        // 2. Handle Plain IPs or Domains (e.g. 10.0.0.211:5050 or myhost.local:5050)
        int slashIndex = rawInput.IndexOf('/');
        if (slashIndex >= 0)
            rawInput = rawInput[..slashIndex];

        int colonIndex = rawInput.LastIndexOf(':');
        if (colonIndex > 0 && !rawInput.Contains(']')) // Skip IPv6 brackets
        {
            string portPart = rawInput[(colonIndex + 1)..];

            if (int.TryParse(portPart, out int extractedPort))
            {
                // Update port setting & entry
                Settings.HostServerPort = extractedPort;
            }
        }

        // Keep full 10.0.0.211:5050 string intact for raw IP view
        Settings.HostServerIp = rawInput;
        HostEntry.Text = rawInput;

        Log.Write("SETTINGS: HostServerIp updated to " + rawInput);
    }
    private void OnStreamChanged(object sender, EventArgs e)
    {
        string rawInput = StreamEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(rawInput))
            return;

        if (Uri.TryCreate(rawInput, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            if (!uri.IsDefaultPort)
            {
                Settings.StreamServerPort = uri.Port;
            }

            string cleanUrl = $"{uri.Scheme}://{uri.Host}";

            Settings.StreamServerIp = cleanUrl;
            StreamEntry.Text = cleanUrl;

            Log.Write("SETTINGS: StreamServerIp updated to " + cleanUrl);
            return;
        }

        int slashIndex = rawInput.IndexOf('/');
        if (slashIndex >= 0)
            rawInput = rawInput[..slashIndex];

        int colonIndex = rawInput.LastIndexOf(':');
        if (colonIndex > 0 && !rawInput.Contains(']'))
        {
            string portPart = rawInput[(colonIndex + 1)..];

            if (int.TryParse(portPart, out int extractedPort))
            {
                Settings.StreamServerPort = extractedPort;
            }
        }

        Settings.StreamServerIp = rawInput;
        StreamEntry.Text = rawInput;

        Log.Write("SETTINGS: StreamServerIp updated to " + rawInput);
    }
    private void OnTapActionChanged(object sender, EventArgs e)
    {
        Settings.PlaylistTapAction =
            TapActionPicker.SelectedIndex == 0 ? TapAction.Play : TapAction.Queue;

        Log.Write("SETTINGS: PlaylistTapAction = " + Settings.PlaylistTapAction);
    }
    private void OnKeepAwakeToggled(object sender, ToggledEventArgs e)
    {
        Settings.KeepScreenAwake = e.Value;
        ScreenBehaviorManager.ApplyBehavior();
        Log.Write("SETTINGS: KeepScreenAwake toggled to " + e.Value);
    }
    private async void OnDeleteLogFileClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "Delete Log File",
            "This will permanently delete the Companion log file. Continue?",
            "Delete",
            "Cancel"
        );

        if (!confirm)
            return;

        try
        {
            Log.Clear();   // new helper method
            await DisplayAlertAsync("Done", "Log file deleted.", "OK");
        }
        catch (Exception ex)
        {
            Log.Write("DeleteLogFile error: " + ex);
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}