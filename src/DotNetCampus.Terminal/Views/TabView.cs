using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DotNetCampus.Terminal.Views;

public class TabView : Control
{
    public static readonly StyledProperty<double> CornerRadiusProperty = AvaloniaProperty.Register<TabView, double>(
        nameof(CornerRadius));
    public static readonly StyledProperty<double> BorderThicknessProperty = AvaloniaProperty.Register<TabView, double>(
        nameof(BorderThickness));
    public static readonly StyledProperty<IBrush?> BorderBrushProperty = Border.BorderBrushProperty.AddOwner<TabView>();
    public static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<TabView>();

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var size = Bounds.Size;
        var radius = CornerRadius;
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(-radius, size.Height), true);
            ctx.ArcTo(new Point(0, size.Height - radius), new Size(radius, radius), 0, false, SweepDirection.CounterClockwise);
            ctx.LineTo(new Point(0, radius));
            ctx.ArcTo(new Point(radius, 0), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(size.Width - radius, 0));
            ctx.ArcTo(new Point(size.Width, radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(size.Width, size.Height - radius));
            ctx.ArcTo(new Point(size.Width + radius, size.Height), new Size(radius, radius), 0, false, SweepDirection.CounterClockwise);
        }

        context.DrawGeometry(Background, new Pen(BorderBrush, BorderThickness), geometry);
    }
}
