using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SkyeMusicCompanion.Platforms.Android;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnDestroy()
    {
        CleanupConnection();
        base.OnDestroy();
    }
    private static void CleanupConnection()
    {
        try
        {
            var connection = IPlatformApplication.Current?.Services.GetService<Services.CompanionConnection>();
            connection?.Disconnect();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
        }
    }
}