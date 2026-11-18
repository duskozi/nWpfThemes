using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NWpfThemes;

/// <summary>
/// Switches themes while the app runs, either for the whole application or for one part of a window.
/// </summary>
/// <example>
/// <code>
/// ThemeManager.ApplyTheme(Theme.Light);            // whole app
/// ThemeManager.ApplyTheme(sidePanel, Theme.Dark);  // one element and everything inside it
/// </code>
/// In XAML, <c>nwt:ThemeManager.Theme="Hardware"</c> themes one element and its children.
/// </example>
public static class ThemeManager
{
    private static readonly List<Theme> s_themes = new(Theme.BuiltInThemes);
    private static readonly ConditionalWeakTable<ResourceDictionary, Theme> s_requested = new();
    private static readonly List<WeakReference<FrameworkElement>> s_scopes = new();
    private static bool s_followSystemHighContrast = true;

    static ThemeManager()
    {
        SystemParameters.StaticPropertyChanged += OnSystemParameterChanged;
    }

    /// <summary>Raised after a theme has been applied.</summary>
    public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>The built-in themes followed by any themes added with <see cref="Register"/>.</summary>
    public static IReadOnlyList<Theme> Themes => s_themes;

    /// <summary>
    /// When <see langword="true"/> (the default), the High Contrast theme is used everywhere while Windows high
    /// contrast is on, whatever theme was asked for. The requested theme comes back when high contrast is turned off.
    /// </summary>
    public static bool FollowSystemHighContrast
    {
        get => s_followSystemHighContrast;
        set
        {
            if (s_followSystemHighContrast == value)
            {
                return;
            }

            s_followSystemHighContrast = value;
            Refresh();
        }
    }

    /// <summary>
    /// The theme the application asked for, found in <see cref="Application.Resources"/>,
    /// or <see langword="null"/> when no theme is merged there.
    /// </summary>
    public static Theme? CurrentTheme =>
        Application.Current is { } app && FindThemeDictionary(app.Resources) is { } found
            ? RequestedThemeOf(found.Dictionary)
            : null;

    /// <summary>Adds a custom theme so it can be found by name and listed in <see cref="Themes"/>.</summary>
    /// <exception cref="InvalidOperationException">A theme with the same name is already registered.</exception>
    public static void Register(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        if (Find(theme.Name) is not null)
        {
            throw new InvalidOperationException($"A theme named '{theme.Name}' is already registered.");
        }

        s_themes.Add(theme);
    }

    /// <summary>Finds a registered theme by name, ignoring case and spaces ("HighContrast" finds "High Contrast").</summary>
    public static Theme? Find(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        string wanted = Normalize(name);
        return s_themes.FirstOrDefault(t => Normalize(t.Name) == wanted);
    }

    /// <summary>Applies a theme to the whole application.</summary>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Application"/>.</exception>
    public static void ApplyTheme(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        Application app = Application.Current
            ?? throw new InvalidOperationException("There is no Application. Use ApplyTheme(element, theme) instead.");
        Theme applied = Replace(app.Resources, theme);
        ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(theme, applied, null));
    }

    /// <summary>Applies a registered theme, by name, to the whole application.</summary>
    public static void ApplyTheme(string name) => ApplyTheme(Find(name) ?? throw UnknownTheme(name));

    /// <summary>
    /// Applies a theme to one element and everything inside it. The control styles must already be merged
    /// in the application (or in a parent of the element).
    /// </summary>
    public static void ApplyTheme(FrameworkElement scope, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(theme);
        Theme applied = Replace(scope.Resources, theme);
        TrackScope(scope);
        ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(theme, applied, scope));
    }

    /// <summary>Removes a theme applied with <see cref="ApplyTheme(FrameworkElement, Theme)"/>, so the element inherits again.</summary>
    public static void ClearTheme(FrameworkElement scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (FindThemeDictionary(scope.Resources) is { } found)
        {
            found.Owner.MergedDictionaries.RemoveAt(found.Index);
        }

        lock (s_scopes)
        {
            s_scopes.RemoveAll(w => !w.TryGetTarget(out FrameworkElement? e) || ReferenceEquals(e, scope));
        }
    }

    /// <summary>Identifies the <c>ThemeManager.Theme</c> attached property.</summary>
    public static readonly DependencyProperty ThemeProperty = DependencyProperty.RegisterAttached(
        "Theme",
        typeof(string),
        typeof(ThemeManager),
        new PropertyMetadata(null, OnThemePropertyChanged));

    /// <summary>Gets the theme name set on an element with the <c>ThemeManager.Theme</c> attached property.</summary>
    public static string? GetTheme(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (string?)element.GetValue(ThemeProperty);
    }

    /// <summary>
    /// Themes an element and everything inside it by theme name. Set it to <see langword="null"/> to
    /// inherit the theme again.
    /// </summary>
    public static void SetTheme(FrameworkElement element, string? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(ThemeProperty, value);
    }

    /// <summary>Loads the dictionary for a theme, using High Contrast instead while Windows high contrast is on.</summary>
    internal static ResourceDictionary CreateThemeDictionary(Theme requested)
    {
        ResourceDictionary dictionary = Effective(requested).CreateDictionary();
        s_requested.AddOrUpdate(dictionary, requested);
        return dictionary;
    }

    internal static Exception UnknownTheme(string? name) =>
        new ArgumentException(
            $"No theme named '{name}'. Known themes: {string.Join(", ", s_themes.Select(t => t.Name))}.",
            nameof(name));

    private static Theme Effective(Theme requested) =>
        s_followSystemHighContrast && SystemParameters.HighContrast ? Theme.HighContrast : requested;

    private static void OnThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        if (e.NewValue is string name && name.Length > 0)
        {
            ApplyTheme(element, Find(name) ?? throw UnknownTheme(name));
        }
        else
        {
            ClearTheme(element);
        }
    }

    private static Theme Replace(ResourceDictionary target, Theme requested)
    {
        ResourceDictionary dictionary = CreateThemeDictionary(requested);
        if (FindThemeDictionary(target) is { } found)
        {
            found.Owner.MergedDictionaries[found.Index] = dictionary;
        }
        else
        {
            target.MergedDictionaries.Insert(0, dictionary);
        }

        return Effective(requested);
    }

    private static Theme RequestedThemeOf(ResourceDictionary dictionary)
    {
        if (s_requested.TryGetValue(dictionary, out Theme? requested))
        {
            return requested;
        }

        string? name = OwnValue(dictionary, ThemeKeys.ThemeName) as string;
        return (name is null ? null : Find(name)) ?? new Theme(name ?? "Custom", dictionary.Source ?? new Uri("about:blank"));
    }

    /// <summary>Finds the merged dictionary that directly defines <see cref="ThemeKeys.ThemeName"/>, depth first.</summary>
    internal static (ResourceDictionary Owner, int Index, ResourceDictionary Dictionary)? FindThemeDictionary(ResourceDictionary root)
    {
        for (int i = 0; i < root.MergedDictionaries.Count; i++)
        {
            ResourceDictionary child = root.MergedDictionaries[i];
            if (OwnValue(child, ThemeKeys.ThemeName) is not null)
            {
                return (root, i, child);
            }

            if (FindThemeDictionary(child) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }

    private static object? OwnValue(ResourceDictionary dictionary, string key)
    {
        foreach (object k in dictionary.Keys)
        {
            if (Equals(k, key))
            {
                return dictionary[key];
            }
        }

        return null;
    }

    private static void TrackScope(FrameworkElement scope)
    {
        lock (s_scopes)
        {
            s_scopes.RemoveAll(w => !w.TryGetTarget(out FrameworkElement? e) || ReferenceEquals(e, scope));
            s_scopes.Add(new WeakReference<FrameworkElement>(scope));
        }
    }

    private static void OnSystemParameterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.HighContrast))
        {
            Application.Current?.Dispatcher.BeginInvoke(Refresh);
        }
    }

    /// <summary>Applies every requested theme again, so a change to high contrast takes effect.</summary>
    private static void Refresh()
    {
        if (CurrentTheme is { } appTheme)
        {
            ApplyTheme(appTheme);
        }

        List<FrameworkElement> scopes = new();
        lock (s_scopes)
        {
            foreach (WeakReference<FrameworkElement> w in s_scopes)
            {
                if (w.TryGetTarget(out FrameworkElement? e))
                {
                    scopes.Add(e);
                }
            }
        }

        foreach (FrameworkElement scope in scopes)
        {
            if (FindThemeDictionary(scope.Resources) is { } found)
            {
                ApplyTheme(scope, RequestedThemeOf(found.Dictionary));
            }
        }
    }

    private static string Normalize(string name) => name.Replace(" ", string.Empty).ToUpperInvariant();
}
