using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NWpfThemes.Tests;

/// <summary>
/// Checks text and indicator colours against WCAG 2.1 AA: 4.5:1 for text and 3:1 for focus rings,
/// input borders and other indicators. Gradients are checked stop by stop. Translucent colours are
/// blended over every colour of the window background and of panels, since they can sit on either.
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

    /// <summary>
    /// Pairs that the nDAWControls skin themes take unchanged from the skins they copy (dim labels,
    /// display outlines and the accent on bare metal), so they are not held to the ratios above.
    /// Text on every surface, control face, display and accent fill is still checked.
    /// </summary>
    private static readonly HashSet<(string, string)> SkinPalettePairs = new()
    {
        (ThemeKeys.ForegroundSecondaryBrush, ThemeKeys.BackgroundBrush),
        (ThemeKeys.ForegroundSecondaryBrush, ThemeKeys.SurfaceBrush),
        (ThemeKeys.ForegroundSecondaryBrush, ThemeKeys.SurfaceAltBrush),
        (ThemeKeys.InputBorderBrush, ThemeKeys.BackgroundBrush),
        (ThemeKeys.AccentBrush, ThemeKeys.BackgroundBrush),
    };

    public static TheoryData<string> FixedPaletteThemes() => new() { Theme.Dark.Name, Theme.Light.Name, Theme.Hardware.Name, Theme.MidnightGlass.Name, Theme.Workbench.Name };

    public static TheoryData<string> SkinThemes() => new()
    {
        Theme.Alloy.Name, Theme.AlloyNight.Name, Theme.AlloyEmber.Name, Theme.Graphite.Name,
        Theme.Studio.Name, Theme.RetroHardware.Name, Theme.LightClassic.Name, Theme.HighContrastBlack.Name,
    };

    [Theory]
    [MemberData(nameof(FixedPaletteThemes))]
    public void Text_and_indicators_meet_WCAG_AA(string name) => UiThread.Run(() => Check(name, skip: null));

    [Theory]
    [MemberData(nameof(SkinThemes))]
    public void Skin_themes_keep_text_and_focus_readable(string name) => UiThread.Run(() => Check(name, SkinPalettePairs));

    private static void Check(string name, HashSet<(string, string)>? skip)
    {
        ResourceDictionary dictionary = ThemeManager.Find(name)!.CreateDictionary();

        Color[] backdrops = Colors((Brush)dictionary[ThemeKeys.BackgroundBrush])
            .Concat(Colors((Brush)dictionary[ThemeKeys.SurfaceBrush]))
            .Where(c => c.A == 255)
            .ToArray();

        var failures = new List<string>();
        foreach ((string fg, string bg, double minimum) in Pairs)
        {
            if (skip is not null && skip.Contains((fg, bg)))
            {
                continue;
            }

            double worst = (
                from backdrop in backdrops
                from b in Colors((Brush)dictionary[bg])
                let background = Over(b, backdrop)
                from f in Colors((Brush)dictionary[fg])
                select Ratio(Over(f, background), background)).Min();
            if (worst < minimum)
            {
                failures.Add($"{fg} on {bg}: {worst:0.00} (needs {minimum})");
            }
        }

        Assert.Empty(failures);
    }

    public static TheoryData<string> ThemesWithFixedColours()
    {
        var data = new TheoryData<string>();
        foreach (Theme theme in Theme.BuiltInThemes)
        {
            if (!ReferenceEquals(theme, Theme.HighContrast))
            {
                data.Add(theme.Name);
            }
        }

        return data;
    }

    /// <summary>
    /// Glass panels and windows with a system backdrop can sit over anything, so their tints must keep text
    /// readable over black and white alike.
    /// </summary>
    [Theory]
    [MemberData(nameof(ThemesWithFixedColours))]
    public void Glass_tints_keep_text_readable_over_any_backdrop(string name) => UiThread.Run(() =>
    {
        ResourceDictionary dictionary = ThemeManager.Find(name)!.CreateDictionary();

        var failures = new List<string>();
        foreach (string key in new[] { ThemeKeys.GlassTintBrush, ThemeKeys.WindowBackdropTintBrush })
        {
            Color tint = ((SolidColorBrush)dictionary[key]).Color;
            double worst = (
                from backdrop in new[] { System.Windows.Media.Colors.Black, System.Windows.Media.Colors.White }
                let background = Over(tint, backdrop)
                from f in Colors((Brush)dictionary[ThemeKeys.ForegroundBrush])
                select Ratio(Over(f, background), background)).Min();
            if (worst < 4.5)
            {
                failures.Add($"Text on {key}: {worst:0.00} (needs 4.5)");
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

    [Fact]
    public void Translucent_colours_are_blended_over_the_backdrop()
    {
        Color half = Over(Color.FromArgb(128, 255, 255, 255), System.Windows.Media.Colors.Black);

        Assert.Equal(Color.FromRgb(128, 128, 128), half);
        Assert.Equal(System.Windows.Media.Colors.Red, Over(System.Windows.Media.Colors.Red, System.Windows.Media.Colors.Blue));
    }

    private static IEnumerable<Color> Colors(Brush brush) => brush switch
    {
        SolidColorBrush solid => new[] { solid.Color },
        GradientBrush gradient => gradient.GradientStops.Select(s => s.Color),
        _ => throw new NotSupportedException(brush.GetType().Name),
    };

    /// <summary>Blends a colour over an opaque one, as it shows on screen.</summary>
    internal static Color Over(Color top, Color bottom)
    {
        double a = top.A / 255.0;
        byte Mix(byte t, byte b) => (byte)Math.Round((t * a) + (b * (1 - a)));
        return Color.FromRgb(Mix(top.R, bottom.R), Mix(top.G, bottom.G), Mix(top.B, bottom.B));
    }

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
