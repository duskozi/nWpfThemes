using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NWpfThemes;

/// <summary>
/// A window whose title bar and frame are drawn by the theme. Derive your windows from it instead of
/// <see cref="Window"/>; it looks like a normal window when the control styles are not merged.
/// </summary>
public class ThemedWindow : Window
{
    private static readonly DependencyPropertyKey MaximizedBorderThicknessPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(MaximizedBorderThickness),
            typeof(Thickness),
            typeof(ThemedWindow),
            new PropertyMetadata(new Thickness(8)));

    /// <summary>Identifies the <see cref="MaximizedBorderThickness"/> property.</summary>
    public static readonly DependencyProperty MaximizedBorderThicknessProperty = MaximizedBorderThicknessPropertyKey.DependencyProperty;

    /// <summary>Identifies the <see cref="TitleBarContent"/> property.</summary>
    public static readonly DependencyProperty TitleBarContentProperty = DependencyProperty.Register(
        nameof(TitleBarContent),
        typeof(object),
        typeof(ThemedWindow),
        new PropertyMetadata(null));

    /// <summary>Creates the window and hooks up the caption button commands.</summary>
    public ThemedWindow()
    {
        SetResourceReference(StyleProperty, typeof(ThemedWindow));

        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, _) => SystemCommands.CloseWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, _) => SystemCommands.MinimizeWindow(this), CanMinimize));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, _) => SystemCommands.MaximizeWindow(this), CanResize));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (_, _) => SystemCommands.RestoreWindow(this), CanResize));
        CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));
    }

    /// <summary>
    /// How far the window reaches past the screen edges when maximized. The template pads its content by this
    /// much so nothing is cut off.
    /// </summary>
    public Thickness MaximizedBorderThickness => (Thickness)GetValue(MaximizedBorderThicknessProperty);

    /// <summary>Optional content shown in the title bar, to the left of the caption buttons.</summary>
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    /// <inheritdoc />
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        UpdateMaximizedBorderThickness();
    }

    /// <inheritdoc />
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        UpdateMaximizedBorderThickness();
    }

    private void UpdateMaximizedBorderThickness()
    {
        const int SM_CXFRAME = 32;
        const int SM_CYFRAME = 33;
        const int SM_CXPADDEDBORDER = 92;

        DpiScale dpi = VisualTreeHelper.GetDpi(this);
        int padded = GetSystemMetrics(SM_CXPADDEDBORDER);
        double x = (GetSystemMetrics(SM_CXFRAME) + padded) / dpi.DpiScaleX;
        double y = (GetSystemMetrics(SM_CYFRAME) + padded) / dpi.DpiScaleY;
        if (x > 0 && y > 0)
        {
            SetValue(MaximizedBorderThicknessPropertyKey, new Thickness(x, y, x, y));
        }
    }

    private void CanMinimize(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ResizeMode != ResizeMode.NoResize;

    private void CanResize(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip;

    private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
    {
        Point screen = e.Parameter is Point p ? p : PointToScreen(Mouse.GetPosition(this));
        DpiScale dpi = VisualTreeHelper.GetDpi(this);
        SystemCommands.ShowSystemMenu(this, new Point(screen.X / dpi.DpiScaleX, screen.Y / dpi.DpiScaleY));
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);
}
