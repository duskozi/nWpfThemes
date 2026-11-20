using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NWpfThemes;

/// <summary>
/// Draws the part of the blurred <see cref="Glass.BackdropProperty">backdrop</see> that lies behind it,
/// covered by <see cref="Tint"/>. Draws nothing when no backdrop is set. The group box, expander and
/// tab control styles put one under their content; use it in your own templates the same way.
/// </summary>
public class GlassBackdropLayer : FrameworkElement
{
    /// <summary>The rounding of the corners. The top-left value is used for all four.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>A translucent fill drawn over the blur so text stays readable.</summary>
    public static readonly DependencyProperty TintProperty = DependencyProperty.Register(
        nameof(Tint),
        typeof(Brush),
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly DependencyProperty BackdropProperty = Glass.BackdropProperty.AddOwner(
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly DependencyProperty BlurRadiusProperty = Glass.BlurRadiusProperty.AddOwner(
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(Glass.DefaultBlurRadius, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    private Rect _drawnBounds = Rect.Empty;

    static GlassBackdropLayer()
    {
        IsHitTestVisibleProperty.OverrideMetadata(typeof(GlassBackdropLayer), new UIPropertyMetadata(false));
        FocusableProperty.OverrideMetadata(typeof(GlassBackdropLayer), new FrameworkPropertyMetadata(false));
    }

    /// <summary>Creates a layer.</summary>
    public GlassBackdropLayer()
    {
        Loaded += (_, _) =>
        {
            LayoutUpdated -= OnLayoutUpdated;
            LayoutUpdated += OnLayoutUpdated;
        };
        Unloaded += (_, _) => LayoutUpdated -= OnLayoutUpdated;
    }

    /// <summary>The rounding of the corners. The top-left value is used for all four.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>A translucent fill drawn over the blur so text stays readable.</summary>
    public Brush? Tint
    {
        get => (Brush?)GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => default;

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        _drawnBounds = Rect.Empty;

        if (GetValue(BackdropProperty) is not UIElement backdrop || RenderSize.Width <= 0 || RenderSize.Height <= 0)
        {
            return;
        }

        Rect? bounds = BoundsIn(backdrop);
        if (bounds is null)
        {
            return;
        }

        BackdropSnapshot snapshot = BackdropSnapshot.For(backdrop);
        snapshot.Attach(this);
        BitmapSource? image = snapshot.GetImage((double)GetValue(BlurRadiusProperty));
        if (image is null)
        {
            return;
        }

        var area = new Rect(RenderSize);
        double radius = Math.Min(CornerRadius.TopLeft, Math.Min(area.Width, area.Height) / 2);
        var blur = new ImageBrush(image)
        {
            Viewbox = bounds.Value,
            ViewboxUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.Fill,
        };
        blur.Freeze();

        drawingContext.DrawRoundedRectangle(blur, null, area, radius, radius);
        if (Tint is { } tint)
        {
            drawingContext.DrawRoundedRectangle(tint, null, area, radius, radius);
        }

        _drawnBounds = bounds.Value;
    }

    /// <summary>This layer's bounds in the backdrop's coordinates, or null when they share no window.</summary>
    private Rect? BoundsIn(UIElement backdrop)
    {
        try
        {
            return TransformToVisual(backdrop).TransformBounds(new Rect(RenderSize));
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    // Moving or scrolling the panel only needs a redraw with a different part of the same image.
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (GetValue(BackdropProperty) is not UIElement backdrop)
        {
            return;
        }

        Rect? bounds = BoundsIn(backdrop);
        if (bounds is { } b && !AreClose(b, _drawnBounds))
        {
            InvalidateVisual();
        }
    }

    private static bool AreClose(Rect a, Rect b) =>
        !a.IsEmpty && !b.IsEmpty
        && Math.Abs(a.X - b.X) < 0.5 && Math.Abs(a.Y - b.Y) < 0.5
        && Math.Abs(a.Width - b.Width) < 0.5 && Math.Abs(a.Height - b.Height) < 0.5;
}
