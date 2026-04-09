using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace SkyeMusicCompanion
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
    events.AddWindows(windows =>
    {
        windows.OnWindowCreated((window) =>
        {
            var appWindow = window.AppWindow;

            // Set initial portrait-ish size
            appWindow.Resize(new Windows.Graphics.SizeInt32(400, 800));

            // Enforce minimum size manually
            window.SizeChanged += (s, e) =>
            {
                int minWidth = 350;
                int minHeight = 600;

                var size = appWindow.Size;

                int newWidth = Math.Max(size.Width, minWidth);
                int newHeight = Math.Max(size.Height, minHeight);

                if (newWidth != size.Width || newHeight != size.Height)
                {
                    appWindow.Resize(new Windows.Graphics.SizeInt32(newWidth, newHeight));
                }
            };
        });
    });
#endif
            });
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
