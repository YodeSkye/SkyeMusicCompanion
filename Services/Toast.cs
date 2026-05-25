using Microsoft.Maui.Controls.Shapes;

namespace SkyeMusicCompanion.Services;

public static class Toast
{
    static bool _isShowing;

    public static async Task Show(Grid toastLayer, string message)
    {
        if (_isShowing)
            return;

        _isShowing = true;

        try
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb("#333333")),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Opacity = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 0, 40),
                Padding = new Thickness(14, 8),
                Content = new Label
                {
                    Text = message,
                    TextColor = Colors.White,
                    FontSize = 14
                }
            };

            toastLayer.Children.Add(border);

            await border.FadeToAsync(1, 150);
            await Task.Delay(1200);
            await border.FadeToAsync(0, 150);

            toastLayer.Children.Remove(border);
        }
        finally
        {
            _isShowing = false;
        }
    }
}
