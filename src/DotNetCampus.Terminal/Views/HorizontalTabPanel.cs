using Avalonia;
using Avalonia.Controls;

namespace DotNetCampus.Terminal.Views;

public class HorizontalTabPanel : Panel
{
    static HorizontalTabPanel()
    {
        AffectsMeasure<HorizontalTabPanel>(MaxTabLengthProperty, MinTabLengthProperty);
    }

    private double _maxTabLength = double.PositiveInfinity;
    private double _minTabLength = 0d;
    private double _containerLength;

    public static readonly DirectProperty<HorizontalTabPanel, double> MaxTabLengthProperty = AvaloniaProperty.RegisterDirect<HorizontalTabPanel, double>(
        nameof(MaxTabLength), o => o.MaxTabLength, (o, v) => o.MaxTabLength = v);
    public static readonly DirectProperty<HorizontalTabPanel, double> MinTabLengthProperty = AvaloniaProperty.RegisterDirect<HorizontalTabPanel, double>(
        nameof(MinTabLength), o => o.MinTabLength, (o, v) => o.MinTabLength = v);

    public static readonly DirectProperty<HorizontalTabPanel, double> ContainerLengthProperty = AvaloniaProperty.RegisterDirect<HorizontalTabPanel, double>(
        nameof(ContainerLength), o => o.ContainerLength, (o, v) => o.ContainerLength = v);

    public double MaxTabLength
    {
        get => _maxTabLength;
        set => SetAndRaise(MaxTabLengthProperty, ref _maxTabLength, value);
    }

    public double MinTabLength
    {
        get => _minTabLength;
        set => SetAndRaise(MinTabLengthProperty, ref _minTabLength, value);
    }

    public double ContainerLength
    {
        get => _containerLength;
        set => SetAndRaise(ContainerLengthProperty, ref _containerLength, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children;
        var count = children.Count;
        if (count == 0)
        {
            return new Size();
        }

        var tabLength = availableSize.Width / count;
        tabLength = Math.Clamp(tabLength, MinTabLength, MaxTabLength);

        var tabSize = new Size(tabLength, availableSize.Height);
        foreach (var child in children)
        {
            child.Measure(tabSize);
        }

        return new Size(tabLength * count, 0);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        var count = children.Count;
        if (count == 0)
        {
            return finalSize;
        }

        var tabLength = finalSize.Width / count;
        tabLength = Math.Clamp(tabLength, MinTabLength, MaxTabLength);

        for (var i = 0; i < count; i++)
        {
            var child = children[i];
            child.Arrange(new Rect(i * tabLength, 0, tabLength, finalSize.Height));
        }

        return finalSize;
    }
}
