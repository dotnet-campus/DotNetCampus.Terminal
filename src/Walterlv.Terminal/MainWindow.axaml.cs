using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Walterlv.Terminal;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeWindowSize();

        SunshineBorder.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var min = Math.Min(e.NewSize.Width, e.NewSize.Height);
        var brush = (LinearGradientBrush)SunshineBorder.Background!;
        brush.EndPoint = new RelativePoint(min / 2, min, RelativeUnit.Absolute);
    }

    private void InitializeWindowSize()
    {
        var designWidth = Width;
        var designHeight = Height;

        var screen = Screens.ScreenFromWindow(this);
        if (screen is null)
        {
            Width = designWidth;
            Height = designHeight;
            return;
        }

        var desiredHeight = screen.WorkingArea.Height / screen.Scaling * 0.75;
        var height = Math.Min(designHeight, desiredHeight);
        var width = height * 4 / 3;

        Width = width;
        Height = height;
    }
}
