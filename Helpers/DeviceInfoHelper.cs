
namespace SkyeMusicCompanion.Helpers
{
    public static class DeviceInfoHelper
    {
        public static string GetDeviceName()
        {
            return DeviceInfo.Name ?? "UnknownDevice";
        }
    }
}
