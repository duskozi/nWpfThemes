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

    /// <summary>The themes that ship with the library.</summary>
    public static IReadOnlyList<Theme> BuiltInThemes { get; } = new[] { Dark, Light, HighContrast, Hardware };

    /// <summary>The location of the dictionary with the control styles.</summary>
    public static Uri ControlStylesSource { get; } = new(AssemblyPath + "Styles/Controls.xaml", UriKind.Relative);

    /// <summary>Loads a new instance of the theme dictionary.</summary>
    public ResourceDictionary CreateDictionary() => new() { Source = Source };

    /// <inheritdoc />
    public override string ToString() => Name;

    private static Theme BuiltIn(string name, string path) => new(name, new Uri(AssemblyPath + path, UriKind.Relative));
}
