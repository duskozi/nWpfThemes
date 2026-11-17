using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NWpfThemes.Tests;

/// <summary>
/// Checks text and indicator colours against WCAG 2.1 AA: 4.5:1 for text and 3:1 for focus rings,
/// input borders and other indicators. Gradients are checked stop by stop.
/// </summary>
public class ContrastTests
{
    private static readonly (string Foreground, string Background, double Minimum)[] Pairs =
    {
        (ThemeKeys.ForegroundBrush, ThemeKeys.BackgroundBrush, 4.5),
        (ThemeKeys.ForegroundBrush, ThemeKeys.SurfaceBrush, 4.5),
        (ThemeKeys.ForegroundBrush, ThemeKeys.SurfaceAltBrush, 4.5),
        (ThemeKeys.ForegroundBrush, ThemeKeys.HoverBrush, 4.5),
        (ThemeKeys.ForegroundSecondaryBrush, ThemeKeys.BackgroundBrush, 4.5),
        (ThemeKeys.ForegroundSecondaryBrush, ThemeKeys.SurfaceBrush, 4.5),
        (ThemeKeys.ForegroundSecondaryBrush, ThemeKeys.SurfaceAltBrush, 4.5),
        (ThemeKeys.ControlForegroundBrush, ThemeKeys.ControlBackgroundBrush, 4.5),
        (ThemeKeys.ControlHoverForegroundBrush, ThemeKeys.ControlHoverBrush, 4.5),
        (ThemeKeys.ControlHoverForegroundBrush, ThemeKeys.ControlPressedBrush, 4.5),
        (ThemeKeys.InputForegroundBrush, ThemeKeys.InputBackgroundBrush, 4.5),
        (ThemeKeys.AccentForegroundBrush, ThemeKeys.AccentBrush, 4.5),
        (ThemeKeys.AccentForegroundBrush, ThemeKeys.AccentHoverBrush, 4.5),
        (ThemeKeys.AccentForegroundBrush, ThemeKeys.AccentPressedBrush, 4.5),
        (ThemeKeys.SelectionForegroundBrush, ThemeKeys.SelectionBrush, 4.5),
        (ThemeKeys.SelectionInactiveForegroundBrush, ThemeKeys.SelectionInactiveBrush, 4.5),
        (ThemeKeys.HoverForegroundBrush, ThemeKeys.HoverBrush, 4.5),
        (ThemeKeys.ToolTipForegroundBrush, ThemeKeys.ToolTipBackgroundBrush, 4.5),
        (ThemeKeys.TitleBarForegroundBrush, ThemeKeys.TitleBarBackgroundBrush, 4.5),
        (ThemeKeys.TitleBarInactiveForegroundBrush, ThemeKeys.TitleBarInactiveBackgroundBrush, 3),
        (ThemeKeys.CaptionButtonHoverForegroundBrush, ThemeKeys.CaptionButtonHoverBrush, 4.5),
        (ThemeKeys.CloseButtonHoverForegroundBrush, ThemeKeys.CloseButtonHoverBrush, 4.5),
        (ThemeKeys.ErrorBrush, ThemeKeys.BackgroundBrush, 4.5),
        (ThemeKeys.ErrorBrush, ThemeKeys.SurfaceBrush, 4.5),
        (ThemeKeys.FocusBrush, ThemeKeys.BackgroundBrush, 3),
        (ThemeKeys.FocusBrush, ThemeKeys.SurfaceBrush, 3),
        (ThemeKeys.InputBorderBrush, ThemeKeys.BackgroundBrush, 3),
        (ThemeKeys.AccentBrush, ThemeKeys.BackgroundBrush, 3),
        (ThemeKeys.GlyphBrush, ThemeKeys.SurfaceBrush, 3),
    };

    public static TheoryData<string> FixedPaletteThemes() => new() { Theme.Dark.Name, Theme.Light.Name, Theme.Hardware.Name };

    [Theory]
    [MemberData(nameof(FixedPaletteThemes))]
    public void Text_and_indicators_meet_WCAG_AA(string name) => UiThread.Run(() =>
    {
        ResourceDictionary dictionary = ThemeManager.Find(name)!.CreateDictionary();

        var failures = new List<string>();
        foreach ((string fg, string bg, double minimum) in Pairs)
        {
            double worst = Colors((Brush)dictionary[fg])
                .SelectMany(f => Colors((Brush)dictionary[bg]).Select(b => Ratio(f, b)))
                .Min();
            if (worst < minimum)
            {
                failures.Add($"{fg} on {bg}: {worst:0.00} (needs {minimum})");
            }
        }

        Assert.Empty(failures);
    });

    [Fact]
    public void Contrast_ratio_matches_known_values()
    {
        Assert.Equal(21, Ratio(System.Windows.Media.Colors.Black, System.Windows.Media.Colors.White), 2);
        Assert.Equal(1, Ratio(System.Windows.Media.Colors.Gray, System.Windows.Media.Colors.Gray), 2);
    }

    private static IEnumerable<Color> Colors(Brush brush) => brush switch
    {
        SolidColorBrush solid => new[] { solid.Color },
        GradientBrush gradient => gradient.GradientStops.Select(s => s.Color),
        _ => throw new NotSupportedException(brush.GetType().Name),
    };

    internal static double Ratio(Color a, Color b)
    {
        double la = Luminance(a);
        double lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance(Color c)
    {
        static double Channel(byte value)
        {
            double v = value / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(c.R)) + (0.7152 * Channel(c.G)) + (0.0722 * Channel(c.B));
    }
}
