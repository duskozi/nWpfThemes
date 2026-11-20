using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace NWpfThemes.Tests;

public class GlassTests
{
    [Fact]
    public void Blur_keeps_a_flat_colour_unchanged()
    {
        int colour = unchecked((int)0xFF336699);
        int[] pixels = Enumerable.Repeat(colour, 20 * 10).ToArray();

        FastBlur.Gaussian(pixels, 20, 10, 3);

        Assert.All(pixels, p => Assert.Equal(colour, p));
    }

    [Fact]
    public void Blur_spreads_a_bright_dot_and_keeps_it_centred()
    {
        const int size = 21;
        int[] pixels = Enumerable.Repeat(unchecked((int)0xFF000000), size * size).ToArray();
        pixels[(10 * size) + 10] = unchecked((int)0xFFFFFFFF);

        FastBlur.Gaussian(pixels, size, size, 2);

        static int Green(int p) => (p >> 8) & 0xFF;
        int centre = Green(pixels[(10 * size) + 10]);
        Assert.InRange(centre, 1, 254);
        Assert.True(Green(pixels[(10 * size) + 12]) > 0);
        Assert.Equal(Green(pixels[(10 * size) + 8]), Green(pixels[(10 * size) + 12]));
        Assert.Equal(Green(pixels[(8 * size) + 10]), Green(pixels[(12 * size) + 10]));
        Assert.Equal(0, Green(pixels[0]));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(8)]
    public void Box_radii_grow_with_the_blur(double sigma)
    {
        int[] radii = FastBlur.BoxRadii(sigma, 3);

        Assert.Equal(3, radii.Length);
        Assert.All(radii, r => Assert.InRange(r, 0, (int)(sigma * 2) + 1));
        Assert.True(radii.Max() - radii.Min() <= 1);
    }

    [Fact]
    public void Backdrop_is_inherited_by_panels() => UiThread.Run(() =>
    {
        var backdrop = new Border();
        var panel = new StackPanel();
        var group = new GroupBox();
        panel.Children.Add(group);

        Glass.SetBackdrop(panel, backdrop);

        Assert.Same(backdrop, Glass.GetBackdrop(group));
        Assert.Equal(Glass.DefaultBlurRadius, Glass.GetBlurRadius(group));
    });

    [Fact]
    public void Layer_draws_the_blurred_backdrop_behind_a_panel() => UiThread.Run(() =>
    {
        var backdrop = new Border { Background = Brushes.Red };
        var layer = new GlassBackdropLayer { Tint = null };
        var content = new Grid { Children = { layer } };
        Glass.SetBackdrop(content, backdrop);
        var root = new Grid { Width = 200, Height = 100, Children = { backdrop, content } };

        root.Measure(new Size(200, 100));
        root.Arrange(new Rect(0, 0, 200, 100));
        root.UpdateLayout();

        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(200, 100, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(layer);
        int[] pixel = new int[1];
        bitmap.CopyPixels(new Int32Rect(100, 50, 1, 1), pixel, 4, 0);

        Assert.Equal(unchecked((int)0xFFFF0000), pixel[0]);
    });
}
