
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
    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        IpEntry.Unfocus();
        PortEntry.Unfocus();
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
    private void OnKeepAwakeToggled(object sender, ToggledEventArgs e)
    {
        Settings.KeepScreenAwake = e.Value;
        Log.Write("SETTINGS: KeepScreenAwake toggled to " + e.Value);
    }
}
