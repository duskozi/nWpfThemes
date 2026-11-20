using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace NWpfThemes;

/// <summary>The kind of system backdrop drawn behind a window. See <see cref="WindowBackdrop"/>.</summary>
public enum WindowBackdropType
{
    /// <summary>No backdrop: the window has its normal theme background.</summary>
    None,

    /// <summary>Mica where Windows supports it (Windows 11 22H2 and later), otherwise none.</summary>
    Auto,

    /// <summary>Mica: a subtle tint of the desktop wallpaper. Windows 11 22H2 and later.</summary>
    Mica,

    /// <summary>Acrylic: a blur of whatever is behind the window. Windows 11 22H2 and later.</summary>
    Acrylic,

    /// <summary>Tabbed: a stronger wallpaper tint, meant for windows with tabs in the title bar. Windows 11 22H2 and later.</summary>
    Tabbed,

    /// <summary>
    /// A blur of whatever is behind the window on Windows 10 1803 and later, through an undocumented
    /// Windows function. It is unofficial: Windows may change or remove it, and on some machines the
    /// window lags while it is dragged. Use it only as an explicit choice.
    /// </summary>
    Windows10Blur,
}

/// <summary>
/// Puts a system backdrop (Mica or Acrylic) behind a window, so the desktop shows through the theme's
/// background. Windows draws it, so it can show what is behind the window, which the in-app
/// <see cref="Glass"/> blur cannot.
/// </summary>
/// <remarks>
/// <para>
/// Set <c>nwt:WindowBackdrop.Type="Auto"</c> on a window. Where the requested backdrop is not supported,
/// or while Windows high contrast is on, the window keeps its normal solid background. While a backdrop
/// is on, the window background becomes the theme's <c>NwtWindowBackdropTintBrush</c> and
/// <see cref="IsActiveProperty"/> is <see langword="true"/>; <see cref="ThemedWindow"/> then makes its title
/// bar see-through too.
/// </para>
/// <para>
/// <see cref="Glass.Enabled"/> turns backdrops off for the whole app; <see cref="WindowBackdropType.None"/>
/// turns one window's off.
/// </para>
/// </remarks>
public static class WindowBackdrop
{
    /// <summary>The backdrop the window asks for. <see cref="WindowBackdropType.None"/> by default.</summary>
    public static readonly DependencyProperty TypeProperty = DependencyProperty.RegisterAttached(
        "Type",
        typeof(WindowBackdropType),
        typeof(WindowBackdrop),
        new PropertyMetadata(WindowBackdropType.None, OnTypeChanged));

    private static readonly DependencyPropertyKey IsActivePropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "IsActive",
        typeof(bool),
        typeof(WindowBackdrop),
        new PropertyMetadata(false));

    /// <summary><see langword="true"/> while a system backdrop shows behind the window. Read-only.</summary>
    public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

    /// <summary>The theme key of the window background while a backdrop is on.</summary>
    public const string TintBrushKey = "NwtWindowBackdropTintBrush";

    private const int Windows11_22H2 = 22621;
    private const int Windows10_1803 = 17134;

    private static readonly ConditionalWeakTable<Window, State> s_states = new();
    private static readonly List<WeakReference<Window>> s_windows = new();

    static WindowBackdrop()
    {
        Glass.EnabledChanged += static (_, _) => RefreshAll();
        ThemeManager.ThemeChanged += static (_, _) => RefreshAll();
        SystemParameters.StaticPropertyChanged += static (_, e) =>
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast))
            {
                RefreshAll();
            }
        };
    }

    /// <summary>Gets the backdrop the window asks for.</summary>
    public static WindowBackdropType GetType(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return (WindowBackdropType)window.GetValue(TypeProperty);
    }

    /// <summary>Asks for a backdrop behind the window. Takes effect at once, also on a window that is already shown.</summary>
    public static void SetType(Window window, WindowBackdropType value)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.SetValue(TypeProperty, value);
    }

    /// <summary>Gets whether a system backdrop shows behind the window.</summary>
    public static bool GetIsActive(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return (bool)window.GetValue(IsActiveProperty);
    }

    /// <summary>The backdrop that <paramref name="requested"/> turns into on this computer, ignoring <see cref="Glass.Enabled"/>.</summary>
    public static WindowBackdropType Resolve(WindowBackdropType requested)
    {
        if (SystemParameters.HighContrast || !OperatingSystem.IsWindows())
        {
            return WindowBackdropType.None;
        }

        bool windows11 = OperatingSystem.IsWindowsVersionAtLeast(10, 0, Windows11_22H2);
        return requested switch
        {
            WindowBackdropType.Auto => windows11 ? WindowBackdropType.Mica : WindowBackdropType.None,
            WindowBackdropType.Mica or WindowBackdropType.Acrylic or WindowBackdropType.Tabbed =>
                windows11 ? requested : WindowBackdropType.None,
            WindowBackdropType.Windows10Blur =>
                OperatingSystem.IsWindowsVersionAtLeast(10, 0, Windows10_1803) ? requested : WindowBackdropType.None,
            _ => WindowBackdropType.None,
        };
    }

    private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
        {
            return;
        }

        lock (s_windows)
        {
            s_windows.RemoveAll(r => !r.TryGetTarget(out _));
            if (!s_windows.Exists(r => r.TryGetTarget(out Window? w) && ReferenceEquals(w, window)))
            {
                s_windows.Add(new WeakReference<Window>(window));
            }
        }

        Apply(window);
    }

    private static void RefreshAll()
    {
        Window[] windows;
        lock (s_windows)
        {
            s_windows.RemoveAll(r => !r.TryGetTarget(out _));
            var list = new List<Window>();
            foreach (WeakReference<Window> reference in s_windows)
            {
                if (reference.TryGetTarget(out Window? window))
                {
                    list.Add(window);
                }
            }

            windows = list.ToArray();
        }

        foreach (Window window in windows)
        {
            window.Dispatcher.BeginInvoke(new Action(() => Apply(window)));
        }
    }

    private static void Apply(Window window)
    {
        State state = s_states.GetValue(window, static _ => new State());
        IntPtr hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            if (!state.WaitingForHandle)
            {
                state.WaitingForHandle = true;
                window.SourceInitialized += (_, _) =>
                {
                    state.WaitingForHandle = false;
                    Apply(window);
                };
            }

            return;
        }

        WindowBackdropType wanted = Glass.Enabled ? Resolve(GetType(window)) : WindowBackdropType.None;
        if (wanted == WindowBackdropType.None)
        {
            TurnOff(window, hwnd, state);
        }
        else
        {
            TurnOn(window, hwnd, state, wanted);
        }
    }

    private static void TurnOn(Window window, IntPtr hwnd, State state, WindowBackdropType type)
    {
        if (state.Applied == WindowBackdropType.None)
        {
            state.OriginalChrome = window.ReadLocalValue(WindowChrome.WindowChromeProperty);
            state.OriginalBackground = window.ReadLocalValue(Window.BackgroundProperty);
        }

        // The backdrop shows only where the frame reaches into the client area, so extend it everywhere.
        if (WindowChrome.GetWindowChrome(window) is { } chrome)
        {
            if (chrome.GlassFrameThickness != new Thickness(-1))
            {
                WindowChrome.SetWindowChrome(window, new WindowChrome
                {
                    CaptionHeight = chrome.CaptionHeight,
                    CornerRadius = chrome.CornerRadius,
                    GlassFrameThickness = new Thickness(-1),
                    NonClientFrameEdges = chrome.NonClientFrameEdges,
                    ResizeBorderThickness = chrome.ResizeBorderThickness,
                    UseAeroCaptionButtons = chrome.UseAeroCaptionButtons,
                });
            }

            // A window that draws its own caption buttons would otherwise also show Windows' buttons
            // through the extended frame.
            if (window is ThemedWindow && !state.RemovedSystemMenu)
            {
                int style = GetWindowLong(hwnd, GWL_STYLE);
                if ((style & WS_SYSMENU) != 0)
                {
                    SetWindowLong(hwnd, GWL_STYLE, style & ~WS_SYSMENU);
                    state.RemovedSystemMenu = true;
                }
            }
        }
        else
        {
            var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
            _ = DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        if (HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } target })
        {
            target.BackgroundColor = Colors.Transparent;
        }

        int dark = IsDarkTheme(window) ? 1 : 0;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));

        if (type == WindowBackdropType.Windows10Blur)
        {
            // Windows draws the tint itself here, so the window background must be fully clear.
            window.Background = Brushes.Transparent;
            SetAccent(hwnd, ACCENT_ENABLE_ACRYLICBLURBEHIND, TintColor(window));
        }
        else
        {
            if (state.Applied == WindowBackdropType.Windows10Blur)
            {
                SetAccent(hwnd, ACCENT_DISABLED, 0);
            }

            window.SetResourceReference(Window.BackgroundProperty, TintBrushKey);
            int backdrop = type switch
            {
                WindowBackdropType.Acrylic => DWMSBT_TRANSIENTWINDOW,
                WindowBackdropType.Tabbed => DWMSBT_TABBEDWINDOW,
                _ => DWMSBT_MAINWINDOW,
            };
            _ = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
        }

        state.Applied = type;
        window.SetValue(IsActivePropertyKey, true);
    }

    private static void TurnOff(Window window, IntPtr hwnd, State state)
    {
        if (state.Applied == WindowBackdropType.None)
        {
            return;
        }

        if (state.Applied == WindowBackdropType.Windows10Blur)
        {
            SetAccent(hwnd, ACCENT_DISABLED, 0);
        }
        else
        {
            int none = DWMSBT_NONE;
            _ = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref none, sizeof(int));
        }

        Restore(window, WindowChrome.WindowChromeProperty, state.OriginalChrome);
        Restore(window, Window.BackgroundProperty, state.OriginalBackground);

        if (WindowChrome.GetWindowChrome(window) is null)
        {
            var margins = default(Margins);
            _ = DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        if (state.RemovedSystemMenu)
        {
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) | WS_SYSMENU);
            state.RemovedSystemMenu = false;
        }

        if (HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } target } && WindowChrome.GetWindowChrome(window) is null)
        {
            target.BackgroundColor = SystemColors.WindowColor;
        }

        state.Applied = WindowBackdropType.None;
        window.SetValue(IsActivePropertyKey, false);
    }

    private static void Restore(Window window, DependencyProperty property, object? original)
    {
        if (original is null || original == DependencyProperty.UnsetValue)
        {
            window.ClearValue(property);
        }
        else
        {
            window.SetValue(property, original);
        }
    }

    private static bool IsDarkTheme(Window window)
    {
        if (window.TryFindResource(ThemeKeys.BackgroundColor) is not Color c)
        {
            return false;
        }

        return ((0.2126 * c.R) + (0.7152 * c.G) + (0.0722 * c.B)) < 128;
    }

    /// <summary>The tint as the ABGR value Windows 10 expects.</summary>
    private static int TintColor(Window window)
    {
        Color c = window.TryFindResource(TintBrushKey) is SolidColorBrush brush ? brush.Color : Color.FromArgb(0xCC, 0x20, 0x20, 0x20);
        return (c.A << 24) | (c.B << 16) | (c.G << 8) | c.R;
    }

    private static void SetAccent(IntPtr hwnd, int state, int gradientColor)
    {
        var accent = new AccentPolicy { AccentState = state, AccentFlags = 2, GradientColor = gradientColor };
        int size = Marshal.SizeOf<AccentPolicy>();
        IntPtr pointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(accent, pointer, false);
            var data = new WindowCompositionAttributeData { Attribute = WCA_ACCENT_POLICY, Data = pointer, SizeOfData = size };
            _ = SetWindowCompositionAttribute(hwnd, ref data);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private sealed class State
    {
        public WindowBackdropType Applied { get; set; }

        public object? OriginalChrome { get; set; }

        public object? OriginalBackground { get; set; }

        public bool RemovedSystemMenu { get; set; }

        public bool WaitingForHandle { get; set; }
    }

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMSBT_NONE = 1;
    private const int DWMSBT_MAINWINDOW = 2;
    private const int DWMSBT_TRANSIENTWINDOW = 3;
    private const int DWMSBT_TABBEDWINDOW = 4;
    private const int WCA_ACCENT_POLICY = 19;
    private const int ACCENT_DISABLED = 0;
    private const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4;
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x00080000;

    [StructLayout(LayoutKind.Sequential)]
    private struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int value);
}
