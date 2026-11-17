using System;
using System.Linq;
using System.Windows;

namespace NWpfThemes.Gallery;

/// <summary>Turns the simulated high contrast palette on and off for the whole app.</summary>
internal static class HighContrastPreview
{
    private static readonly Uri Source = new("/NWpfThemes.Gallery;component/SimulatedHighContrast.xaml", UriKind.Relative);

    public static bool IsEnabled
    {
        get => Find() is not null;
        set
        {
            ResourceDictionary? existing = Find();
            if (value && existing is null)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = Source });
            }
            else if (!value && existing is not null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(existing);
            }
        }
    }

    private static ResourceDictionary? Find() =>
        Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source is { } s && s.OriginalString.EndsWith("SimulatedHighContrast.xaml", StringComparison.Ordinal));
}
