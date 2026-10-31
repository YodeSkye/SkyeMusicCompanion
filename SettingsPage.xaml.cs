using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // Load saved values
        HostEntry.Text = Settings.HostServerIp;
        PortEntry.Text = Settings.HostServerPort.ToString();
        StreamPortEntry.Text = Settings.StreamServerPort.ToString();
        TapActionPicker.SelectedIndex = Settings.PlaylistTapAction == TapAction.Play ? 0 : 1;
        KeepAwakeSwitch.IsToggled = Settings.KeepScreenAwake;
    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        HostEntry.Unfocus();
        PortEntry.Unfocus();
        StreamPortEntry.Unfocus();
        TapActionPicker.Unfocus();
        await App.Connection.ReconnectAsync();
    }

    private void OnHostChanged(object sender, EventArgs e)
    {
        string rawInput = HostEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(rawInput))
            return;

        // 1. Strip http:// or https:// if pasted
        if (rawInput.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            rawInput = rawInput[7..];
        else if (rawInput.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            rawInput = rawInput[8..];

        // 2. Strip trailing slashes or path segments (e.g., domain.com/api -> domain.com)
        int slashIndex = rawInput.IndexOf('/');
        if (slashIndex >= 0)
            rawInput = rawInput[..slashIndex];

        // 3. Handle inline port pasting (e.g., skyemusic.duckdns.org:5050)
        int colonIndex = rawInput.LastIndexOf(':');
        if (colonIndex > 0 && !rawInput.Contains(']')) // Ignores IPv6 brackets if used
        {
            string hostPart = rawInput[..colonIndex];
            string portPart = rawInput[(colonIndex + 1)..];

            if (int.TryParse(portPart, out int extractedPort))
            {
                // Automatically update the port setting and UI entry if a port was included in the paste
                Settings.HostServerPort = extractedPort;
                PortEntry.Text = extractedPort.ToString();
            }

            rawInput = hostPart;
        }

        // 4. Save the sanitized host string and update the UI entry display
        Settings.HostServerIp = rawInput;
        HostEntry.Text = rawInput;

        Log.Write("SETTINGS: HostServerIp updated to " + rawInput);
    }
    private void OnPortChanged(object sender, EventArgs e)
    {
        if (int.TryParse(PortEntry.Text, out int port))
            Settings.HostServerPort = port;
    }
    private void OnStreamPortChanged(object sender, EventArgs e)
    {
        if (int.TryParse(StreamPortEntry.Text, out int streamPort))
            Settings.StreamServerPort = streamPort;
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