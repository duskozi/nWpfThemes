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

        ThemeManager.ApplyTheme(theme);
        bool isHighContrast = ReferenceEquals(theme, Theme.HighContrast);
        SimulateHighContrast.Visibility = isHighContrast && !SystemParameters.HighContrast ? Visibility.Visible : Visibility.Collapsed;
        HighContrastPreview.IsEnabled = isHighContrast && SimulateHighContrast.IsChecked == true;
    }

    private void OnSimulateChanged(object sender, RoutedEventArgs e) =>
        HighContrastPreview.IsEnabled = ReferenceEquals(ThemePicker.SelectedItem, Theme.HighContrast) && SimulateHighContrast.IsChecked == true;
}
