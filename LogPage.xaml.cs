
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class LogPage : ContentPage
{
    private const int MaxLogBytes = 200_000; // 200 KB max loaded into UI

    public LogPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Let UI render first
        await Task.Delay(50);

        // Load + truncate log asynchronously
        var text = await Task.Run(() => ReadTruncatedLog());

        LogLabel.Text = text;

        ScrollToBottom();
    }

    private string ReadTruncatedLog()
    {
        try
        {
            string path = Log.LogPath;

            if (!File.Exists(path))
                return "(log empty)";

            var bytes = File.ReadAllBytes(path);

            // If file is small, return whole thing
            if (bytes.Length <= MaxLogBytes)
                return File.ReadAllText(path);

            // Otherwise truncate from the BEGINNING
            int start = bytes.Length - MaxLogBytes;
            var slice = bytes.AsSpan(start);

            return System.Text.Encoding.UTF8.GetString(slice);
        }
        catch
        {
            return "(error reading log)";
        }
    }

    private void OnClearLogClicked(object sender, EventArgs e)
    {
        Log.Clear();
        LogLabel.Text = "(log cleared)";
    }

    private async void OnSaveLogClicked(object sender, EventArgs e)
    {
        try
        {
            var logText = await Task.Run(() => Log.ReadAll());

#if WINDOWS10_0_19041_0
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });
            picker.SuggestedFileName = "companion_log";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current?.Windows[0].Handler.PlatformView);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                File.WriteAllText(file.Path, logText);
                await DisplayAlertAsync("Saved", $"Log saved to:\n{file.Path}", "OK");
            }
#elif ANDROID
            var downloads = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads
            )!.AbsolutePath;

            var filePath = Path.Combine(downloads, "companion_log.txt");
            File.WriteAllText(filePath, logText);

            await DisplayAlertAsync("Saved", $"Log saved to Downloads:\n{filePath}", "OK");
#endif
        }
        catch (Exception ex)
        {
            Log.Write("SaveLog error: " + ex);
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private void ScrollToBottom()
    {
        Dispatcher.Dispatch(async () =>
        {
            await LogScroll.ScrollToAsync(0, LogLabel.Height, false);
        });
    }
}
