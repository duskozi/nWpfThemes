using System.Windows;
using System.Windows.Controls;

namespace NWpfThemes.Gallery;

public partial class MainWindow : ThemedWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ThemePicker.ItemsSource = ThemeManager.Themes;
        ThemePicker.SelectedItem = ThemeManager.CurrentTheme ?? Theme.Dark;
    }

    private void OnThemeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ThemePicker.SelectedItem is not Theme theme)
        {
            return;
        }

        bool isHighContrast = ReferenceEquals(theme, Theme.HighContrast);
        SimulateHighContrast.Visibility = isHighContrast && !SystemParameters.HighContrast ? Visibility.Visible : Visibility.Collapsed;
        ApplySelectedTheme();
    }

    private void OnSimulateChanged(object sender, RoutedEventArgs e) => ApplySelectedTheme();

    private void ApplySelectedTheme()
    {
        if (ThemePicker.SelectedItem is not Theme theme)
        {
            return;
        }

        bool simulate = ReferenceEquals(theme, Theme.HighContrast) && SimulateHighContrast.IsChecked == true;
        ThemeManager.ApplyTheme(simulate ? HighContrastPreview.Theme : theme);
    }
}
