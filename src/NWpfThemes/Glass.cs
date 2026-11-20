using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NWpfThemes;

/// <summary>
/// Frosted glass: panels show a blurred copy of a backdrop element behind them. The blur is drawn by
/// the app itself, so it looks the same on Windows 10 and Windows 11 and does not depend on the window
/// manager.
/// </summary>
/// <remarks>
/// <para>
/// Set <c>nwt:Glass.Backdrop</c> on a container. Group boxes, expanders and tab pages inside it then
/// show the blurred backdrop under a translucent tint (<c>NwtGlassTintBrush</c>). Without a backdrop
/// nothing is blurred and panels look as usual.
/// </para>
/// <para>
/// The backdrop is rendered once at a quarter of its size, blurred, and shared by every panel that
/// uses it. Panels that move or scroll only show a different part of that image, so moving them costs
/// nothing extra. The image is rebuilt when the backdrop changes size or the theme changes. If the
/// backdrop animates or changes in other ways, call <see cref="Refresh"/>.
/// </para>
/// <para>
/// The backdrop must be drawn behind the panels but must not contain them, for example a sibling
/// layer underneath the content.
/// </para>
/// </remarks>
/// <example>
/// <code language="xml">
/// &lt;Grid&gt;
///     &lt;Border x:Name="Art"&gt;...pictures, charts, colour...&lt;/Border&gt;
///     &lt;StackPanel nwt:Glass.Backdrop="{Binding ElementName=Art}"&gt;
///         &lt;GroupBox Header="Frosted"&gt;...&lt;/GroupBox&gt;
///     &lt;/StackPanel&gt;
/// &lt;/Grid&gt;
/// </code>
/// </example>
public static class Glass
{
    /// <summary>The default <see cref="BlurRadiusProperty"/>, in device-independent pixels.</summary>
    public const double DefaultBlurRadius = 24;

    /// <summary>
    /// The element whose blurred image shows through glass panels. Inherited, so it can be set once on
    /// a container.
    /// </summary>
    public static readonly DependencyProperty BackdropProperty = DependencyProperty.RegisterAttached(
        "Backdrop",
        typeof(UIElement),
        typeof(Glass),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>How strongly the backdrop is blurred, in device-independent pixels. Inherited.</summary>
    public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.RegisterAttached(
        "BlurRadius",
        typeof(double),
        typeof(Glass),
        new FrameworkPropertyMetadata(DefaultBlurRadius, FrameworkPropertyMetadataOptions.Inherits),
        static value => value is double d && d >= 0 && !double.IsInfinity(d));

    /// <summary>Gets the backdrop of glass panels at this element.</summary>
    public static UIElement? GetBackdrop(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (UIElement?)element.GetValue(BackdropProperty);
    }

    /// <summary>Sets the backdrop of glass panels in this element and everything inside it.</summary>
    public static void SetBackdrop(DependencyObject element, UIElement? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(BackdropProperty, value);
    }

    /// <summary>Gets the blur radius of glass panels at this element.</summary>
    public static double GetBlurRadius(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (double)element.GetValue(BlurRadiusProperty);
    }

    /// <summary>Sets the blur radius of glass panels in this element and everything inside it.</summary>
    public static void SetBlurRadius(DependencyObject element, double value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(BlurRadiusProperty, value);
    }

    /// <summary>
    /// Rebuilds the blurred image of a backdrop, for backdrops whose content changed without changing
    /// size. Several calls in a row are combined into one rebuild.
    /// </summary>
    public static void Refresh(UIElement backdrop)
    {
        ArgumentNullException.ThrowIfNull(backdrop);
        if (BackdropSnapshot.TryGet(backdrop, out BackdropSnapshot? snapshot))
        {
            snapshot.Invalidate();
        }
    }
}

/// <summary>The blurred, downsampled image of one backdrop, shared by all panels that show it.</summary>
internal sealed class BackdropSnapshot
{
    /// <summary>The backdrop is rendered at 1/Downsample of its size before blurring.</summary>
    internal const int Downsample = 4;

    private static readonly ConditionalWeakTable<UIElement, BackdropSnapshot> s_snapshots = new();
    private static readonly List<WeakReference<BackdropSnapshot>> s_all = new();

    private readonly WeakReference<UIElement> _backdrop;
    private readonly List<WeakReference<GlassBackdropLayer>> _layers = new();
    private readonly Dictionary<double, BitmapSource> _images = new();
    private bool _invalidatePending;

    static BackdropSnapshot()
    {
        ThemeManager.ThemeChanged += static (_, _) =>
        {
            lock (s_all)
            {
                s_all.RemoveAll(r => !r.TryGetTarget(out _));
                foreach (WeakReference<BackdropSnapshot> reference in s_all)
                {
                    if (reference.TryGetTarget(out BackdropSnapshot? snapshot))
                    {
                        snapshot.Invalidate();
                    }
                }
            }
        };
    }

    private BackdropSnapshot(UIElement backdrop)
    {
        _backdrop = new WeakReference<UIElement>(backdrop);
        if (backdrop is FrameworkElement element)
        {
            element.SizeChanged += (_, _) => Invalidate();
        }
    }

    public static BackdropSnapshot For(UIElement backdrop) => s_snapshots.GetValue(backdrop, static b =>
    {
        var snapshot = new BackdropSnapshot(b);
        lock (s_all)
        {
            s_all.Add(new WeakReference<BackdropSnapshot>(snapshot));
        }

        return snapshot;
    });

    public static bool TryGet(UIElement backdrop, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out BackdropSnapshot? snapshot) =>
        s_snapshots.TryGetValue(backdrop, out snapshot);

    /// <summary>Remembers a layer so it is redrawn when the image is rebuilt.</summary>
    public void Attach(GlassBackdropLayer layer)
    {
        foreach (WeakReference<GlassBackdropLayer> reference in _layers)
        {
            if (reference.TryGetTarget(out GlassBackdropLayer? existing) && ReferenceEquals(existing, layer))
            {
                return;
            }
        }

        _layers.RemoveAll(r => !r.TryGetTarget(out _));
        _layers.Add(new WeakReference<GlassBackdropLayer>(layer));
    }

    /// <summary>The blurred image for a radius, built on first use. Its size in DIPs equals the backdrop's.</summary>
    public BitmapSource? GetImage(double blurRadius)
    {
        if (_images.TryGetValue(blurRadius, out BitmapSource? image))
        {
            return image;
        }

        if (!_backdrop.TryGetTarget(out UIElement? backdrop))
        {
            return null;
        }

        image = Render(backdrop, blurRadius);
        if (image is not null)
        {
            _images[blurRadius] = image;
        }

        return image;
    }

    /// <summary>Drops the images and redraws the layers once the current layout pass is done.</summary>
    public void Invalidate()
    {
        if (_invalidatePending)
        {
            return;
        }

        _invalidatePending = true;
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            _invalidatePending = false;
            _images.Clear();
            foreach (WeakReference<GlassBackdropLayer> reference in _layers)
            {
                if (reference.TryGetTarget(out GlassBackdropLayer? layer))
                {
                    layer.InvalidateVisual();
                }
            }
        }));
    }

    private static BitmapSource? Render(UIElement backdrop, double blurRadius)
    {
        Size size = backdrop.RenderSize;
        if (size.Width < 1 || size.Height < 1)
        {
            return null;
        }

        int width = Math.Max(1, (int)Math.Ceiling(size.Width / Downsample));
        int height = Math.Max(1, (int)Math.Ceiling(size.Height / Downsample));

        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen())
        {
            var brush = new VisualBrush(backdrop)
            {
                Viewbox = new Rect(size),
                ViewboxUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.Fill,
            };
            context.DrawRectangle(brush, null, new Rect(0, 0, width, height));
        }

        var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);

        int[] pixels = new int[width * height];
        target.CopyPixels(pixels, width * 4, 0);
        FastBlur.Gaussian(pixels, width, height, blurRadius / Downsample / 2);

        // The DPI makes the small image cover exactly the backdrop's size in DIPs, so panels can pick
        // their part of it with the backdrop's own coordinates.
        BitmapSource result = BitmapSource.Create(
            width,
            height,
            96.0 * width / size.Width,
            96.0 * height / size.Height,
            PixelFormats.Pbgra32,
            null,
            pixels,
            width * 4);
        result.Freeze();
        return result;
    }
}

/// <summary>An approximate Gaussian blur made of three box blurs, run on premultiplied 32-bit pixels.</summary>
internal static class FastBlur
{
    /// <summary>Blurs the pixels in place. <paramref name="sigma"/> is in pixels; zero leaves them as they are.</summary>
    public static void Gaussian(int[] pixels, int width, int height, double sigma)
    {
        if (sigma < 0.5 || width < 1 || height < 1)
        {
            return;
        }

        int[] buffer = new int[pixels.Length];
        foreach (int radius in BoxRadii(sigma, 3))
        {
            BoxHorizontal(pixels, buffer, width, height, radius);
            BoxVertical(buffer, pixels, width, height, radius);
        }
    }

    /// <summary>The radii of <paramref name="passes"/> box blurs that together approximate a Gaussian.</summary>
    internal static int[] BoxRadii(double sigma, int passes)
    {
        double ideal = Math.Sqrt((12 * sigma * sigma / passes) + 1);
        int lower = (int)Math.Floor(ideal);
        if (lower % 2 == 0)
        {
            lower--;
        }

        int upper = lower + 2;
        double m = ((12 * sigma * sigma) - (passes * lower * lower) - (4 * passes * lower) - (3 * passes)) / ((-4 * lower) - 4);
        int count = (int)Math.Round(m);

        int[] radii = new int[passes];
        for (int i = 0; i < passes; i++)
        {
            radii[i] = ((i < count ? lower : upper) - 1) / 2;
        }

        return radii;
    }

    private static void BoxHorizontal(int[] source, int[] target, int width, int height, int radius)
    {
        int window = (2 * radius) + 1;
        for (int y = 0; y < height; y++)
        {
            int row = y * width;
            int a = 0, r = 0, g = 0, b = 0;
            for (int i = -radius; i <= radius; i++)
            {
                Add(source[row + Math.Clamp(i, 0, width - 1)], ref a, ref r, ref g, ref b, 1);
            }

            for (int x = 0; x < width; x++)
            {
                target[row + x] = Pack(a, r, g, b, window);
                Add(source[row + Math.Min(x + radius + 1, width - 1)], ref a, ref r, ref g, ref b, 1);
                Add(source[row + Math.Max(x - radius, 0)], ref a, ref r, ref g, ref b, -1);
            }
        }
    }

    private static void BoxVertical(int[] source, int[] target, int width, int height, int radius)
    {
        int window = (2 * radius) + 1;
        for (int x = 0; x < width; x++)
        {
            int a = 0, r = 0, g = 0, b = 0;
            for (int i = -radius; i <= radius; i++)
            {
                Add(source[(Math.Clamp(i, 0, height - 1) * width) + x], ref a, ref r, ref g, ref b, 1);
            }

            for (int y = 0; y < height; y++)
            {
                target[(y * width) + x] = Pack(a, r, g, b, window);
                Add(source[(Math.Min(y + radius + 1, height - 1) * width) + x], ref a, ref r, ref g, ref b, 1);
                Add(source[(Math.Max(y - radius, 0) * width) + x], ref a, ref r, ref g, ref b, -1);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Add(int pixel, ref int a, ref int r, ref int g, ref int b, int sign)
    {
        a += sign * ((pixel >> 24) & 0xFF);
        r += sign * ((pixel >> 16) & 0xFF);
        g += sign * ((pixel >> 8) & 0xFF);
        b += sign * (pixel & 0xFF);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Pack(int a, int r, int g, int b, int window) =>
        ((a / window) << 24) | ((r / window) << 16) | ((g / window) << 8) | (b / window);
}
