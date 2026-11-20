using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NWpfThemes.Tests;

public class WindowTests
{
    [Fact]
    public void Window_rolls_up_to_its_title_bar_and_back() => UiThread.Run(() =>
    {
        var window = new ThemedWindow { Width = 300, Height = 400, MinHeight = 200, IsRollUpEnabled = true };
        try
        {
            window.IsRolledUp = true;

            Assert.Equal(32, window.Height);
            Assert.Equal(32, window.MaxHeight);

            window.IsRolledUp = false;

            Assert.Equal(400, window.Height);
            Assert.Equal(200, window.MinHeight);
            Assert.True(double.IsPositiveInfinity(window.MaxHeight));
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void Window_does_not_roll_up_unless_enabled() => UiThread.Run(() =>
    {
        var window = new ThemedWindow { Height = 400 };
        try
        {
            window.IsRolledUp = true;

            Assert.False(window.IsRolledUp);
            Assert.Equal(400, window.Height);
            Assert.False(ThemedWindow.ToggleRollUpCommand.CanExecute(null, window));
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void Turning_roll_up_off_rolls_the_window_down() => UiThread.Run(() =>
    {
        var window = new ThemedWindow { Height = 400, IsRollUpEnabled = true };
        try
        {
            window.IsRolledUp = true;
            window.IsRollUpEnabled = false;

            Assert.False(window.IsRolledUp);
            Assert.Equal(400, window.Height);
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void No_backdrop_resolves_to_none_and_unsupported_ones_fall_back() =>
        Assert.Equal(WindowBackdropType.None, WindowBackdrop.Resolve(WindowBackdropType.None));

    [Fact]
    public void Opaque_fills_skip_the_blur()
    {
        Assert.True(GlassBackdropLayer.IsOpaque(Brushes.Red));
        Assert.False(GlassBackdropLayer.IsOpaque(new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0))));
        Assert.False(GlassBackdropLayer.IsOpaque(new SolidColorBrush(Colors.Red) { Opacity = 0.5 }));
        Assert.False(GlassBackdropLayer.IsOpaque(null));
    }
}
