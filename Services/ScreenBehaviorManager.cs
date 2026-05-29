
namespace SkyeMusicCompanion.Services;

public static class ScreenBehaviorManager
{
    public static void ApplyBehavior()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Setting OFF → always allow sleep
            if (!Settings.KeepScreenAwake)
            {
                DeviceDisplay.KeepScreenOn = false;
                Log.Write("SCREEN: KeepScreenOn = false (setting off)");
                return;
            }

            // Normalize the playstate to uppercase
            string state = App.Connection.now.PlayState?.ToUpperInvariant() ?? "";

            // Only one special case: STOPPED
            if (state == "STOPPED")
            {
                DeviceDisplay.KeepScreenOn = false;
                Log.Write("SCREEN: KeepScreenOn = false (stopped)");
            }
            else
            {
                DeviceDisplay.KeepScreenOn = true;
                Log.Write($"SCREEN: KeepScreenOn = true ({state})");
            }
        });
    }

    public static void ResetToSystem()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DeviceDisplay.KeepScreenOn = false;
            Log.Write("SCREEN: KeepScreenOn = false (reset)");
        });
    }
}
