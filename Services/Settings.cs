using System.Text.Json;

namespace SkyeMusicCompanion.Services;

public class ServerProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Default";
    public string Host { get; set; } = "192.168.1.1";
    public int HostPort { get; set; } = 5050;
    public int StreamPort { get; set; } = 5051;

    public ServerProfile Clone() => new()
    {
        Id = Id,
        Name = Name,
        Host = Host,
        HostPort = HostPort,
        StreamPort = StreamPort
    };
}

public static class Settings
{
    private const string ProfilesKey = nameof(Settings) + ".ProfilesJson";
    private const string ActiveProfileIdKey = nameof(Settings) + ".ActiveProfileId";

    public static List<ServerProfile> GetDefaultProfiles() =>
    [
        new ServerProfile
        {
            Id = "lan-default",
            Name = "Home (LAN)",
            Host = "192.168.1.1",
            HostPort = 5050,
            StreamPort = 5051
        },
        new ServerProfile
        {
            Id = "tailscale-default",
            Name = "Tailscale",
            Host = "100.64.0.1",
            HostPort = 5050,
            StreamPort = 5051
        }
    ];

    public static List<ServerProfile> GetProfiles()
    {
        var json = Preferences.Get(ProfilesKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            var defaults = GetDefaultProfiles();
            SaveProfiles(defaults);
            return defaults;
        }

        try
        {
            var list = JsonSerializer.Deserialize<List<ServerProfile>>(json);
            if (list is { Count: > 0 })
                return list;
        }
        catch (Exception ex)
        {
            Log.Write("SETTINGS: Failed to deserialize profiles, resetting defaults: " + ex.Message);
        }

        var fallback = GetDefaultProfiles();
        SaveProfiles(fallback);
        return fallback;
    }

    public static void SaveProfiles(List<ServerProfile> profiles)
    {
        if (profiles.Count == 0)
            profiles = GetDefaultProfiles();

        var json = JsonSerializer.Serialize(profiles);
        Preferences.Set(ProfilesKey, json);
    }

    public static ServerProfile GetActiveProfile()
    {
        var profiles = GetProfiles();
        var activeId = Preferences.Get(ActiveProfileIdKey, string.Empty);

        var match = profiles.FirstOrDefault(p => p.Id == activeId)
                    ?? profiles.FirstOrDefault(p => p.Name == activeId)
                    ?? profiles.First();

        if (Preferences.Get(ActiveProfileIdKey, string.Empty) != match.Id)
            Preferences.Set(ActiveProfileIdKey, match.Id);

        return match;
    }

    public static void SetActiveProfileId(string profileId)
    {
        Preferences.Set(ActiveProfileIdKey, profileId);
    }

    public static void UpsertProfile(ServerProfile profile)
    {
        var profiles = GetProfiles();
        var index = profiles.FindIndex(p => p.Id == profile.Id);

        if (index >= 0)
            profiles[index] = profile.Clone();
        else
            profiles.Add(profile.Clone());

        SaveProfiles(profiles);
        SetActiveProfileId(profile.Id);
    }

    public static bool DeleteProfile(string profileId)
    {
        var profiles = GetProfiles();
        if (profiles.Count <= 1)
            return false;

        var target = profiles.FirstOrDefault(p => p.Id == profileId);
        if (target == null)
            return false;

        profiles.Remove(target);
        SaveProfiles(profiles);

        if (Preferences.Get(ActiveProfileIdKey, string.Empty) == profileId)
            SetActiveProfileId(profiles.First().Id);

        return true;
    }

    // Convenience helpers if other classes want formatted host:port strings
    public static string HostServerIp
    {
        get
        {
            var p = GetActiveProfile();
            return $"{p.Host}:{p.HostPort}";
        }
    }

    public static string StreamServerIp
    {
        get
        {
            var p = GetActiveProfile();
            return $"{p.Host}:{p.StreamPort}";
        }
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