
namespace SkyeMusicCompanion.Services;

public static class ScreenBehaviorManager
{
    public static void ApplyBehavior()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!Settings.KeepScreenAwake)
            {
                ResetToSystem();
                return;
            }

            DeviceDisplay.KeepScreenOn = true;
            Log.Write("SCREEN: KeepScreenOn = true");
        });
    }

    public static void ResetToSystem()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DeviceDisplay.KeepScreenOn = false;
            Log.Write("SCREEN: KeepScreenOn = false");
        });
    }
}
