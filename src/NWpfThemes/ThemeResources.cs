using System.Windows;

namespace NWpfThemes;

/// <summary>
/// A resource dictionary that brings in the control styles and a theme in one line:
/// <code>&lt;Application.Resources&gt;&lt;nwt:ThemeResources Theme="Dark" /&gt;&lt;/Application.Resources&gt;</code>
/// </summary>
/// <remarks>
/// Use <see cref="ThemeManager.ApplyTheme(Theme)"/> to switch themes afterwards. If you would rather merge
/// dictionaries yourself, merge <see cref="NWpfThemes.Theme.ControlStylesSource"/> and one theme's
/// <see cref="NWpfThemes.Theme.Source"/>.
/// </remarks>
public class ThemeResources : ResourceDictionary
{
    private string _theme = NWpfThemes.Theme.Dark.Name;

    /// <summary>Creates the dictionary with the Dark theme.</summary>
    public ThemeResources()
    {
        MergedDictionaries.Add(ThemeManager.CreateThemeDictionary(NWpfThemes.Theme.Dark));
        MergedDictionaries.Add(new ResourceDictionary { Source = NWpfThemes.Theme.ControlStylesSource });
    }

    /// <summary>
    /// The name of the theme to start with: <c>Dark</c>, <c>Light</c>, <c>High Contrast</c>, <c>Hardware</c>,
    /// or the name of a theme added with <see cref="ThemeManager.Register(NWpfThemes.Theme)"/>.
    /// </summary>
    public string Theme
    {
        get => _theme;
        set
        {
            Theme theme = ThemeManager.Find(value) ?? throw ThemeManager.UnknownTheme(value);
            _theme = theme.Name;
            MergedDictionaries[0] = ThemeManager.CreateThemeDictionary(theme);
        }
    }
}
