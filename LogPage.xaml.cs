
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
        LogLabel.Text = Log.ReadAll();
        ScrollToBottom();
    }
    private void OnClearLogClicked(object sender, EventArgs e)
    {
        Log.Clear();
        LogLabel.Text = "(log cleared)";
    }
    private void ScrollToBottom()
    {
        Dispatcher.Dispatch(() =>
        {
            Dispatcher.Dispatch(async () =>
            {
                await LogScroll.ScrollToAsync(0, LogLabel.Height, false);
            });
        });
    }
}