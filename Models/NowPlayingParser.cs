
namespace SkyeMusicCompanion.Models;

public static class NowPlayingParser
{
    public static NowPlaying Parse(string payload)
    {
        var parts = payload.Split('|');
        return new NowPlaying
        {
            PlayState = parts.ElementAtOrDefault(1) ?? "",
            CurrentTitle = parts.ElementAtOrDefault(2) ?? "",
            Title = parts.ElementAtOrDefault(3) ?? "",
            Path = parts.ElementAtOrDefault(4) ?? "",
            Duration = ParseInt(parts.ElementAtOrDefault(5)),
            Position = ParseInt(parts.ElementAtOrDefault(6)),
            ArtworkBase64 = (parts.ElementAtOrDefault(7) ?? "").Trim()
        };
    }
    private static int ParseInt(string? s)
    {
        if (int.TryParse(s, out var value))
            return value;
        return 0;
    }
}
