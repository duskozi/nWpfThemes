using System;
using System.Windows;

namespace NWpfThemes;

/// <summary>Describes a theme change made by <see cref="ThemeManager"/>.</summary>
public sealed class ThemeChangedEventArgs : EventArgs
{
    internal ThemeChangedEventArgs(Theme requestedTheme, Theme appliedTheme, FrameworkElement? scope)
    {
        RequestedTheme = requestedTheme;
        AppliedTheme = appliedTheme;
        Scope = scope;
    }

    /// <summary>The theme that was asked for.</summary>
    public Theme RequestedTheme { get; }

    /// <summary>
    /// The theme that is now in use. It differs from <see cref="RequestedTheme"/> when Windows high contrast
    /// is on and <see cref="ThemeManager.FollowSystemHighContrast"/> is <see langword="true"/>.
    /// </summary>
    public Theme AppliedTheme { get; }

    /// <summary>The element whose subtree was themed, or <see langword="null"/> for the whole application.</summary>
    public FrameworkElement? Scope { get; }
}
