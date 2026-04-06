
namespace SkyeMusicCompanion.Models;

public class NowPlaying
{
    public string PlayState { get; set; } = "";
    public string CurrentTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string Path { get; set; } = "";
    public int Duration { get; set; } = 0;
    public int Position { get; set; } = 0;
    public string ArtworkBase64 { get; set; } = "";
}
