using System;
using System.Collections.Generic;
using System.Windows;

namespace NWpfThemes;

/// <summary>
/// A theme: a name and the location of a <see cref="ResourceDictionary"/> that defines every key in
/// <see cref="ThemeKeys"/>.
/// </summary>
public sealed class Theme
{
    private const string AssemblyPath = "/NWpfThemes;component/";

    /// <summary>Creates a theme from a resource dictionary location.</summary>
    /// <param name="name">The name shown to users and used by <see cref="ThemeManager.Find(string)"/>.</param>
    /// <param name="source">
    /// The dictionary to load, for example
    /// <c>new Uri("/MyApp;component/Themes/Ocean.xaml", UriKind.Relative)</c>.
    /// </param>
    public Theme(string name, Uri source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(source);
        Name = name;
        Source = source;
    }

    /// <summary>The theme name.</summary>
    public string Name { get; }

    /// <summary>The location of the theme dictionary.</summary>
    public Uri Source { get; }

    /// <summary>A neutral dark theme.</summary>
    public static Theme Dark { get; } = BuiltIn("Dark", "Themes/Dark.xaml");

    /// <summary>A neutral light theme.</summary>
    public static Theme Light { get; } = BuiltIn("Light", "Themes/Light.xaml");

    /// <summary>A theme that takes every colour from the Windows system colours.</summary>
    public static Theme HighContrast { get; } = BuiltIn("High Contrast", "Themes/HighContrast.xaml");

    /// <summary>A hardware-style theme with lit metal keys, bevels and recessed displays.</summary>
    public static Theme Hardware { get; } = BuiltIn("Hardware", "Themes/Hardware.xaml");

    /// <summary>Lit brushed silver with dark recessed displays and orange accents (the nDAWControls Alloy skin).</summary>
    public static Theme Alloy { get; } = BuiltIn("Alloy", "Themes/Alloy.xaml");

    /// <summary>Dark anodised metal with dark displays and orange accents (the nDAWControls AlloyNight skin).</summary>
    public static Theme AlloyNight { get; } = BuiltIn("Alloy Night", "Themes/AlloyNight.xaml");

    /// <summary>Champagne metal with amber displays (the nDAWControls AlloyEmber skin).</summary>
    public static Theme AlloyEmber { get; } = BuiltIn("Alloy Ember", "Themes/AlloyEmber.xaml");

    /// <summary>Dark graphite with an orange accent (the nDAWControls Graphite skin).</summary>
    public static Theme Graphite { get; } = BuiltIn("Graphite", "Themes/Graphite.xaml");

    /// <summary>Dark blue-grey with a blue accent (the nDAWControls Studio skin).</summary>
    public static Theme Studio { get; } = BuiltIn("Studio", "Themes/Studio.xaml");

    /// <summary>Warm dark brown with amber displays (the nDAWControls RetroHardware skin).</summary>
    public static Theme RetroHardware { get; } = BuiltIn("Retro Hardware", "Themes/RetroHardware.xaml");

    /// <summary>Light grey with a blue accent (the nDAWControls LightClassic skin).</summary>
    public static Theme LightClassic { get; } = BuiltIn("Light Classic", "Themes/LightClassic.xaml");

    /// <summary>
    /// Black and white with a yellow accent, as a fixed palette (the nDAWControls HighContrast skin).
    /// <see cref="HighContrast"/> follows the Windows colours instead.
    /// </summary>
    public static Theme HighContrastBlack { get; } = BuiltIn("High Contrast Black", "Themes/HighContrastBlack.xaml");

    /// <summary>The themes that ship with the library.</summary>
    public static IReadOnlyList<Theme> BuiltInThemes { get; } = new[]
    {
        Dark, Light, HighContrast, Hardware,
        Alloy, AlloyNight, AlloyEmber, Graphite, Studio, RetroHardware, LightClassic, HighContrastBlack,
    };

    /// <summary>The location of the dictionary with the control styles.</summary>
    public static Uri ControlStylesSource { get; } = new(AssemblyPath + "Styles/Controls.xaml", UriKind.Relative);

    /// <summary>Loads a new instance of the theme dictionary.</summary>
    public ResourceDictionary CreateDictionary() => new() { Source = Source };

    /// <inheritdoc />
    public override string ToString() => Name;

    private static Theme BuiltIn(string name, string path) => new(name, new Uri(AssemblyPath + path, UriKind.Relative));
}
