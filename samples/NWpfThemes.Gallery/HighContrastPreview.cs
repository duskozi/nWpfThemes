using System;

namespace NWpfThemes.Gallery;

/// <summary>
/// The High Contrast theme with a fixed palette, for previewing it while Windows high contrast is off.
/// It is a normal custom theme, defined in SimulatedHighContrast.xaml.
/// </summary>
internal static class HighContrastPreview
{
    public static Theme Theme { get; } = new(
        "High Contrast (simulated)",
        new Uri("/NWpfThemes.Gallery;component/SimulatedHighContrast.xaml", UriKind.Relative));
}
