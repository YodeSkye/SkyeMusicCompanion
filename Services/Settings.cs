
namespace SkyeMusicCompanion.Services;

public static class Settings
{
    public static string HostServerIp
    {
        get => Preferences.Get(nameof(HostServerIp), "192.168.1.1:5050");
        set => Preferences.Set(nameof(HostServerIp), value);
    }
    public static string StreamServerIp
    {
        get => Preferences.Get(nameof(StreamServerIp), "192.168.1.1:5051");
        set => Preferences.Set(nameof(StreamServerIp), value);
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
