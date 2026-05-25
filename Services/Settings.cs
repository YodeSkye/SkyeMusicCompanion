
namespace SkyeMusicCompanion.Services;

public static class Settings
{
    public static string HostServerIp
    {
        get => Preferences.Get(nameof(HostServerIp), "10.0.0.212");
        set => Preferences.Set(nameof(HostServerIp), value);
    }
    public static int HostServerPort
    {
        get => Preferences.Get(nameof(HostServerPort), 5050);
        set => Preferences.Set(nameof(HostServerPort), value);
    }
    public static TapAction PlaylistTapAction
    {
        get => (TapAction)Preferences.Get(nameof(PlaylistTapAction), (int)TapAction.Play);
        set => Preferences.Set(nameof(PlaylistTapAction), (int)value);
    }
    public static bool KeepScreenAwake
    {
        get => Preferences.Get(nameof(KeepScreenAwake), false);
        set => Preferences.Set(nameof(KeepScreenAwake), value);
    }
}
