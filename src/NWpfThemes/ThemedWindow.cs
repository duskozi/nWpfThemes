using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

namespace NWpfThemes;

/// <summary>
/// A window whose title bar and frame are drawn by the theme. Derive your windows from it instead of
/// <see cref="Window"/>; it looks like a normal window when the control styles are not merged.
/// </summary>
/// <remarks>
/// Set <see cref="IsRollUpEnabled"/> to let the window roll up to just its title bar, like a window shade.
/// A button left of the minimize button rolls it up and down, and <see cref="ToggleRollUpCommand"/> does the
/// same from a key binding or menu.
/// </remarks>
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

    /// <summary>Identifies the <see cref="IsRollUpEnabled"/> property.</summary>
    public static readonly DependencyProperty IsRollUpEnabledProperty = DependencyProperty.Register(
        nameof(IsRollUpEnabled),
        typeof(bool),
        typeof(ThemedWindow),
        new PropertyMetadata(false, OnIsRollUpEnabledChanged));

    /// <summary>Identifies the <see cref="IsRolledUp"/> property.</summary>
    public static readonly DependencyProperty IsRolledUpProperty = DependencyProperty.Register(
        nameof(IsRolledUp),
        typeof(bool),
        typeof(ThemedWindow),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsRolledUpChanged, CoerceIsRolledUp));

    private double _unrolledHeight;
    private double _unrolledMinHeight;
    private double _unrolledMaxHeight;
    private double _unrolledMinWidth;
    private double _unrolledMaxWidth;
    private SizeToContent _unrolledSizeToContent;
    private bool _wasMaximized;
    private bool _changingRollUp;

    /// <summary>Rolls the window up to its title bar, or back down. Bind it to a key or menu item.</summary>
    public static RoutedUICommand ToggleRollUpCommand { get; } = new("Roll up", nameof(ToggleRollUpCommand), typeof(ThemedWindow));

    /// <summary>Creates the window and hooks up the caption button commands.</summary>
    public ThemedWindow()
    {
        SetResourceReference(StyleProperty, typeof(ThemedWindow));

        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, _) => SystemCommands.CloseWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, _) => SystemCommands.MinimizeWindow(this), CanMinimize));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, _) => Maximize(), CanResize));
        CommandBindings.Add(new CommandBinding(ToggleRollUpCommand, (_, _) => IsRolledUp = !IsRolledUp, CanRollUp));
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

    /// <summary>
    /// Shows the roll-up button in the title bar and lets <see cref="ToggleRollUpCommand"/> work.
    /// <see langword="false"/> by default.
    /// </summary>
    public bool IsRollUpEnabled
    {
        get => (bool)GetValue(IsRollUpEnabledProperty);
        set => SetValue(IsRollUpEnabledProperty, value);
    }

    /// <summary>
    /// <see langword="true"/> while the window is rolled up to its title bar. Setting it rolls the window up or
    /// down; it stays <see langword="false"/> unless <see cref="IsRollUpEnabled"/> is on. While rolled up the
    /// window cannot be resized. A maximized window is restored while rolled up and maximized again when it
    /// rolls back down; maximizing a rolled-up window rolls it down first.
    /// </summary>
    public bool IsRolledUp
    {
        get => (bool)GetValue(IsRolledUpProperty);
        set => SetValue(IsRolledUpProperty, value);
    }

    /// <inheritdoc />
    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        // Maximized from outside the caption buttons, for example by double-clicking the title bar.
        if (WindowState == WindowState.Maximized && IsRolledUp && !_changingRollUp)
        {
            _wasMaximized = false;
            IsRolledUp = false;

            // The size limits of the rolled-up window applied while Windows maximized it, so maximize again.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                WindowState = WindowState.Normal;
                WindowState = WindowState.Maximized;
            }));
        }
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

    private static void OnIsRollUpEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        d.CoerceValue(IsRolledUpProperty);

    private static object CoerceIsRolledUp(DependencyObject d, object value) =>
        ((ThemedWindow)d).IsRollUpEnabled ? value : false;

    private static void OnIsRolledUpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (ThemedWindow)d;
        window._changingRollUp = true;
        try
        {
            if ((bool)e.NewValue)
            {
                window.RollUp();
            }
            else
            {
                window.RollDown();
            }
        }
        finally
        {
            window._changingRollUp = false;
        }
    }

    private void RollUp()
    {
        _wasMaximized = WindowState == WindowState.Maximized;
        if (_wasMaximized)
        {
            WindowState = WindowState.Normal;
        }

        _unrolledHeight = double.IsNaN(Height) ? ActualHeight : Height;
        _unrolledMinHeight = MinHeight;
        _unrolledMaxHeight = MaxHeight;
        _unrolledMinWidth = MinWidth;
        _unrolledMaxWidth = MaxWidth;
        _unrolledSizeToContent = SizeToContent;

        double captionHeight = WindowChrome.GetWindowChrome(this)?.CaptionHeight ?? 32;
        double rolledHeight = captionHeight + BorderThickness.Top + BorderThickness.Bottom;
        double width = ActualWidth;

        SizeToContent = SizeToContent.Manual;
        MinHeight = 0;
        MaxHeight = rolledHeight;
        Height = rolledHeight;
        MinHeight = rolledHeight;
        MinWidth = width;
        MaxWidth = width;
    }

    private void RollDown()
    {
        MinWidth = _unrolledMinWidth;
        MaxWidth = _unrolledMaxWidth;
        MinHeight = _unrolledMinHeight;
        MaxHeight = _unrolledMaxHeight;
        if (_unrolledHeight > 0)
        {
            Height = _unrolledHeight;
        }

        SizeToContent = _unrolledSizeToContent;
        if (_wasMaximized)
        {
            _wasMaximized = false;
            WindowState = WindowState.Maximized;
        }
    }

    private void Maximize()
    {
        IsRolledUp = false;
        SystemCommands.MaximizeWindow(this);
    }

    private void CanRollUp(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = IsRollUpEnabled && WindowState != WindowState.Minimized;

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
