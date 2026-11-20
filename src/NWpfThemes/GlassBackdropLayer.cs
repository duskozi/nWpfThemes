using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NWpfThemes;

/// <summary>
/// Draws the part of the blurred <see cref="Glass.BackdropProperty">backdrop</see> that lies behind it,
/// covered by <see cref="Tint"/>. Draws nothing when there is no backdrop, when glass is turned off, when
/// the tint is opaque, or (unless <see cref="IsOverlay"/>) when it sits on something already frosted.
/// The control styles put one under the content of panels, buttons, inputs and popups; use it in your
/// own templates the same way.
/// </summary>
public class GlassBackdropLayer : FrameworkElement
{
    /// <summary>The rounding of the corners. The top-left value is used for all four.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>The translucent fill drawn over the blur: the panel tint, or a button's face.</summary>
    public static readonly DependencyProperty TintProperty = DependencyProperty.Register(
        nameof(Tint),
        typeof(Brush),
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// <see langword="true"/> for popups and menus, which float above everything and so stay frosted even when
    /// opened from inside a frosted panel.
    /// </summary>
    public static readonly DependencyProperty IsOverlayProperty = DependencyProperty.Register(
        nameof(IsOverlay),
        typeof(bool),
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly DependencyProperty BackdropProperty = Glass.BackdropProperty.AddOwner(
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, OnStateChanged));

    private static readonly DependencyProperty BlurRadiusProperty = Glass.BlurRadiusProperty.AddOwner(
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(Glass.DefaultBlurRadius, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly DependencyProperty GlassEnabledProperty = Glass.IsEnabledProperty.AddOwner(
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, OnStateChanged));

    private static readonly DependencyProperty OverGlassProperty = Glass.IsOverGlassProperty.AddOwner(
        typeof(GlassBackdropLayer),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, OnStateChanged));

    private static readonly List<WeakReference<GlassBackdropLayer>> s_active = new();

    private Rect _drawnBounds = Rect.Empty;
    private bool _tracking;

    static GlassBackdropLayer()
    {
        IsHitTestVisibleProperty.OverrideMetadata(typeof(GlassBackdropLayer), new UIPropertyMetadata(false));
        FocusableProperty.OverrideMetadata(typeof(GlassBackdropLayer), new FrameworkPropertyMetadata(false));
        Glass.EnabledChanged += static (_, _) =>
        {
            s_active.RemoveAll(r => !r.TryGetTarget(out _));
            foreach (WeakReference<GlassBackdropLayer> reference in s_active.ToArray())
            {
                if (reference.TryGetTarget(out GlassBackdropLayer? layer))
                {
                    layer.UpdateTracking();
                    layer.InvalidateVisual();
                }
            }
        };
    }

    /// <summary>Creates a layer.</summary>
    public GlassBackdropLayer()
    {
        Loaded += (_, _) => UpdateTracking();
        Unloaded += (_, _) => StopTracking();
    }

    /// <summary>The rounding of the corners. The top-left value is used for all four.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>The translucent fill drawn over the blur: the panel tint, or a button's face.</summary>
    public Brush? Tint
    {
        get => (Brush?)GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    /// <summary>
    /// <see langword="true"/> for popups and menus, which float above everything and so stay frosted even when
    /// opened from inside a frosted panel.
    /// </summary>
    public bool IsOverlay
    {
        get => (bool)GetValue(IsOverlayProperty);
        set => SetValue(IsOverlayProperty, value);
    }

    /// <summary>The backdrop this layer shows, or <see langword="null"/> when it draws nothing.</summary>
    private UIElement? ActiveBackdrop =>
        Glass.Enabled
        && (bool)GetValue(GlassEnabledProperty)
        && (IsOverlay || !(bool)GetValue(OverGlassProperty))
        && GetValue(BackdropProperty) is UIElement backdrop
            ? backdrop
            : null;

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => default;

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        _drawnBounds = Rect.Empty;

        if (ActiveBackdrop is not { } backdrop || RenderSize.Width <= 0 || RenderSize.Height <= 0 || IsOpaque(Tint))
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

    /// <summary>A fill that hides everything under it, so blurring would be wasted work.</summary>
    internal static bool IsOpaque(Brush? brush) => brush switch
    {
        SolidColorBrush solid => solid.Opacity >= 1 && solid.Color.A == 255,
        GradientBrush gradient => gradient.Opacity >= 1 && gradient.GradientStops.Count > 0 && AllOpaque(gradient.GradientStops),
        _ => false,
    };

    private static bool AllOpaque(GradientStopCollection stops)
    {
        foreach (GradientStop stop in stops)
        {
            if (stop.Color.A != 255)
            {
                return false;
            }
        }

        return true;
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((GlassBackdropLayer)d).UpdateTracking();

    /// <summary>
    /// This layer's bounds in the backdrop's coordinates. Popups live in their own window, so for them the
    /// position goes through screen coordinates. Null when the two are not both on screen.
    /// </summary>
    private Rect? BoundsIn(UIElement backdrop)
    {
        var local = new Rect(RenderSize);
        try
        {
            return TransformToVisual(backdrop).TransformBounds(local);
        }
        catch (InvalidOperationException)
        {
            // Not in the same visual tree, for example inside a popup. Try screen coordinates below.
        }

        try
        {
            if (PresentationSource.FromVisual(this) is null || PresentationSource.FromVisual(backdrop) is null)
            {
                return null;
            }

            Point topLeft = backdrop.PointFromScreen(PointToScreen(local.TopLeft));
            Point bottomRight = backdrop.PointFromScreen(PointToScreen(local.BottomRight));
            return new Rect(topLeft, bottomRight);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    // Follows the layer around only while it actually shows a backdrop, so idle layers cost nothing.
    private void UpdateTracking()
    {
        RegisterForGlobalSwitch();
        bool wanted = IsLoaded && ActiveBackdrop is not null;
        if (wanted == _tracking)
        {
            return;
        }

        if (wanted)
        {
            LayoutUpdated += OnLayoutUpdated;
            _tracking = true;
        }
        else
        {
            StopTracking();
        }
    }

    private void StopTracking()
    {
        if (_tracking)
        {
            LayoutUpdated -= OnLayoutUpdated;
            _tracking = false;
        }
    }

    private bool _registered;

    private void RegisterForGlobalSwitch()
    {
        if (_registered || GetValue(BackdropProperty) is null)
        {
            return;
        }

        _registered = true;
        s_active.RemoveAll(r => !r.TryGetTarget(out _));
        s_active.Add(new WeakReference<GlassBackdropLayer>(this));
    }

    // Moving or scrolling only needs a redraw with a different part of the same image.
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (ActiveBackdrop is not { } backdrop)
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
