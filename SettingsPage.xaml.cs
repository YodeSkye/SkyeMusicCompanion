
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // Load saved values
        IpEntry.Text = Settings.HostServerIp;
        PortEntry.Text = Settings.HostServerPort.ToString();
        TapActionPicker.SelectedIndex = Settings.PlaylistTapAction == TapAction.Play ? 0 : 1;
        KeepAwakeSwitch.IsToggled = Settings.KeepScreenAwake;
    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        IpEntry.Unfocus();
        PortEntry.Unfocus();
        TapActionPicker.Unfocus();
        await App.Connection.ReconnectAsync();
    }

    private void OnIpChanged(object sender, EventArgs e)
    {
        Settings.HostServerIp = IpEntry.Text;
    }
    private void OnPortChanged(object sender, EventArgs e)
    {
        if (int.TryParse(PortEntry.Text, out int port))
            Settings.HostServerPort = port;
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
