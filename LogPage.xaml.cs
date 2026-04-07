using Microsoft.Extensions.Logging;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class LogPage : ContentPage
{
	public LogPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LogEditor.Text = Log.ReadAll();
        ScrollToBottom();
    }
    
    private void OnClearLogClicked(object sender, EventArgs e)
    {
        Log.Clear();
        LogEditor.Text = "(log cleared)";
    }
    private async void OnSaveLogClicked(object sender, EventArgs e)
    {
        try
        {
            var logText = Log.ReadAll();
#if WINDOWS10_0_19041_0
        // ---------------- WINDOWS SAVE AS ----------------
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        var txtTypes = new List<string> { ".txt" };
        picker.FileTypeChoices.Add("Text File", txtTypes);
        picker.SuggestedFileName = "companion_log";

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current?.Windows[0].Handler.PlatformView);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            File.WriteAllText(file.Path, logText);
            await DisplayAlertAsync("Saved", $"Log saved to:\n{file.Path}", "OK");
        }
        return;
#elif ANDROID
            // ---------------- ANDROID: SAVE DIRECTLY TO DOWNLOADS ----------------
            var downloads = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads
            )!.AbsolutePath;

            var filePath = Path.Combine(downloads, "companion_log.txt");

            File.WriteAllText(filePath, logText);

            await DisplayAlertAsync("Saved", $"Log saved to Downloads:\n{filePath}", "OK");
            return;
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
        Dispatcher.Dispatch(() =>
        {
            Dispatcher.Dispatch(async () =>
            {
                await LogScroll.ScrollToAsync(0, LogEditor.Height, false);
            });
        });
    }
}
