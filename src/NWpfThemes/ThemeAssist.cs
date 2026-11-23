using System.Windows;

namespace NWpfThemes;

/// <summary>Attached properties that control styles read to pick a variant.</summary>
public static class ThemeAssist
{
    /// <summary>
    /// Marks a button as the main action: it gets the accent glow (<c>NwtAccentShadowEffect</c>) like a
    /// default button. <c>NwtAccentButtonStyle</c> sets it.
    /// </summary>
    public static readonly DependencyProperty IsAccentProperty = DependencyProperty.RegisterAttached(
        "IsAccent", typeof(bool), typeof(ThemeAssist), new FrameworkPropertyMetadata(false));

    /// <summary>Gets <see cref="IsAccentProperty"/>.</summary>
    public static bool GetIsAccent(DependencyObject element) => (bool)element.GetValue(IsAccentProperty);

    /// <summary>Sets <see cref="IsAccentProperty"/>.</summary>
    public static void SetIsAccent(DependencyObject element, bool value) => element.SetValue(IsAccentProperty, value);
}
