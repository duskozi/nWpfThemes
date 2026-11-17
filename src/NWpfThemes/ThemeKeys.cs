using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace NWpfThemes;

/// <summary>
/// The public resource keys that every theme defines. Control styles look these up with
/// <c>DynamicResource</c>, so replacing the theme dictionary restyles every control at once.
/// </summary>
/// <remarks>
/// In XAML, use the key string directly, for example <c>{DynamicResource NwtAccentBrush}</c>.
/// In code, use these constants with <c>SetResourceReference</c> or <c>FindResource</c>.
/// </remarks>
public static class ThemeKeys
{
    /// <summary>The display name of the theme. Every theme dictionary defines it; ThemeManager uses it to find the active theme. Type: <see cref="string"/>.</summary>
    public const string ThemeName = "NwtThemeName";

    /// <summary>The font used for text in controls. Type: <see cref="FontFamily"/>.</summary>
    public const string FontFamily = "NwtFontFamily";

    /// <summary>The font used inside text inputs and displays. Type: <see cref="FontFamily"/>.</summary>
    public const string InputFontFamily = "NwtInputFontFamily";

    /// <summary>The normal font size. Type: <see cref="double"/>.</summary>
    public const string FontSize = "NwtFontSize";

    /// <summary>A smaller font size for secondary text. Type: <see cref="double"/>.</summary>
    public const string FontSizeSmall = "NwtFontSizeSmall";

    /// <summary>A larger font size for headers. Type: <see cref="double"/>.</summary>
    public const string FontSizeLarge = "NwtFontSizeLarge";

    /// <summary>The corner radius of buttons, inputs and other controls. Type: <see cref="CornerRadius"/>.</summary>
    public const string ControlCornerRadius = "NwtControlCornerRadius";

    /// <summary>The corner radius of menus, popups and tooltips. Type: <see cref="CornerRadius"/>.</summary>
    public const string OverlayCornerRadius = "NwtOverlayCornerRadius";

    /// <summary>The border thickness of controls. Type: <see cref="Thickness"/>.</summary>
    public const string ControlBorderThickness = "NwtControlBorderThickness";

    /// <summary>The inner padding of buttons and inputs. Type: <see cref="Thickness"/>.</summary>
    public const string ControlPadding = "NwtControlPadding";

    /// <summary>The minimum height of buttons, inputs and combo boxes. Type: <see cref="double"/>.</summary>
    public const string ControlMinHeight = "NwtControlMinHeight";

    /// <summary>The stroke thickness of the keyboard focus indicator. Type: <see cref="double"/>.</summary>
    public const string FocusStrokeThickness = "NwtFocusStrokeThickness";

    /// <summary>The window background colour. Type: <see cref="Color"/>.</summary>
    public const string BackgroundColor = "NwtBackgroundColor";

    /// <summary>The colour of panels, menus and popups. Type: <see cref="Color"/>.</summary>
    public const string SurfaceColor = "NwtSurfaceColor";

    /// <summary>The main text colour. Type: <see cref="Color"/>.</summary>
    public const string ForegroundColor = "NwtForegroundColor";

    /// <summary>The accent colour. Type: <see cref="Color"/>.</summary>
    public const string AccentColor = "NwtAccentColor";

    /// <summary>The border colour. Type: <see cref="Color"/>.</summary>
    public const string BorderColor = "NwtBorderColor";

    /// <summary>The colour of drop shadows under popups. Type: <see cref="Color"/>.</summary>
    public const string ShadowColor = "NwtShadowColor";

    /// <summary>The window background. Type: <see cref="Brush"/>.</summary>
    public const string BackgroundBrush = "NwtBackgroundBrush";

    /// <summary>The background of panels, lists, menus and popups. Type: <see cref="Brush"/>.</summary>
    public const string SurfaceBrush = "NwtSurfaceBrush";

    /// <summary>An alternative surface for headers, status bars and alternating rows. Type: <see cref="Brush"/>.</summary>
    public const string SurfaceAltBrush = "NwtSurfaceAltBrush";

    /// <summary>The main text colour on backgrounds and surfaces. Type: <see cref="Brush"/>.</summary>
    public const string ForegroundBrush = "NwtForegroundBrush";

    /// <summary>Secondary text, such as headers and hints. Type: <see cref="Brush"/>.</summary>
    public const string ForegroundSecondaryBrush = "NwtForegroundSecondaryBrush";

    /// <summary>Text of disabled controls. Type: <see cref="Brush"/>.</summary>
    public const string ForegroundDisabledBrush = "NwtForegroundDisabledBrush";

    /// <summary>The border of controls and panels. Type: <see cref="Brush"/>.</summary>
    public const string BorderBrush = "NwtBorderBrush";

    /// <summary>The border of a control under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string BorderHoverBrush = "NwtBorderHoverBrush";

    /// <summary>Separator lines and grid lines. Type: <see cref="Brush"/>.</summary>
    public const string SeparatorBrush = "NwtSeparatorBrush";

    /// <summary>The face of buttons and other clickable controls. Type: <see cref="Brush"/>.</summary>
    public const string ControlBackgroundBrush = "NwtControlBackgroundBrush";

    /// <summary>The face of a clickable control under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string ControlHoverBrush = "NwtControlHoverBrush";

    /// <summary>The face of a pressed control. Type: <see cref="Brush"/>.</summary>
    public const string ControlPressedBrush = "NwtControlPressedBrush";

    /// <summary>The face of a disabled control. Type: <see cref="Brush"/>.</summary>
    public const string ControlDisabledBrush = "NwtControlDisabledBrush";

    /// <summary>Text on control faces. Type: <see cref="Brush"/>.</summary>
    public const string ControlForegroundBrush = "NwtControlForegroundBrush";

    /// <summary>Text on a control face that is under the mouse or pressed. Type: <see cref="Brush"/>.</summary>
    public const string ControlHoverForegroundBrush = "NwtControlHoverForegroundBrush";

    /// <summary>An inner edge that makes controls look raised. Transparent in flat themes. Type: <see cref="Brush"/>.</summary>
    public const string ControlBevelBrush = "NwtControlBevelBrush";

    /// <summary>The background of text inputs, progress tracks and other displays. Type: <see cref="Brush"/>.</summary>
    public const string InputBackgroundBrush = "NwtInputBackgroundBrush";

    /// <summary>Text inside inputs and displays. Type: <see cref="Brush"/>.</summary>
    public const string InputForegroundBrush = "NwtInputForegroundBrush";

    /// <summary>The border of inputs. Type: <see cref="Brush"/>.</summary>
    public const string InputBorderBrush = "NwtInputBorderBrush";

    /// <summary>An inner edge that makes inputs look recessed. Transparent in flat themes. Type: <see cref="Brush"/>.</summary>
    public const string InputBevelBrush = "NwtInputBevelBrush";

    /// <summary>The accent: checked states, default buttons, progress and slider fill. Type: <see cref="Brush"/>.</summary>
    public const string AccentBrush = "NwtAccentBrush";

    /// <summary>The accent under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string AccentHoverBrush = "NwtAccentHoverBrush";

    /// <summary>The accent when pressed. Type: <see cref="Brush"/>.</summary>
    public const string AccentPressedBrush = "NwtAccentPressedBrush";

    /// <summary>Text and glyphs on the accent. Type: <see cref="Brush"/>.</summary>
    public const string AccentForegroundBrush = "NwtAccentForegroundBrush";

    /// <summary>The background of selected items. Type: <see cref="Brush"/>.</summary>
    public const string SelectionBrush = "NwtSelectionBrush";

    /// <summary>Text of selected items. Type: <see cref="Brush"/>.</summary>
    public const string SelectionForegroundBrush = "NwtSelectionForegroundBrush";

    /// <summary>The background of selected items when their control does not have focus. Type: <see cref="Brush"/>.</summary>
    public const string SelectionInactiveBrush = "NwtSelectionInactiveBrush";

    /// <summary>Text of selected items when their control does not have focus. Type: <see cref="Brush"/>.</summary>
    public const string SelectionInactiveForegroundBrush = "NwtSelectionInactiveForegroundBrush";

    /// <summary>The background of list and menu items under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string HoverBrush = "NwtHoverBrush";

    /// <summary>Text of list and menu items under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string HoverForegroundBrush = "NwtHoverForegroundBrush";

    /// <summary>The keyboard focus indicator. Type: <see cref="Brush"/>.</summary>
    public const string FocusBrush = "NwtFocusBrush";

    /// <summary>Arrows, chevrons and other glyphs. Type: <see cref="Brush"/>.</summary>
    public const string GlyphBrush = "NwtGlyphBrush";

    /// <summary>The track of sliders and scroll bars. Type: <see cref="Brush"/>.</summary>
    public const string TrackBrush = "NwtTrackBrush";

    /// <summary>Scroll bar thumbs. Type: <see cref="Brush"/>.</summary>
    public const string ThumbBrush = "NwtThumbBrush";

    /// <summary>A scroll bar thumb under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string ThumbHoverBrush = "NwtThumbHoverBrush";

    /// <summary>A scroll bar thumb being dragged. Type: <see cref="Brush"/>.</summary>
    public const string ThumbPressedBrush = "NwtThumbPressedBrush";

    /// <summary>The tooltip background. Type: <see cref="Brush"/>.</summary>
    public const string ToolTipBackgroundBrush = "NwtToolTipBackgroundBrush";

    /// <summary>The tooltip text. Type: <see cref="Brush"/>.</summary>
    public const string ToolTipForegroundBrush = "NwtToolTipForegroundBrush";

    /// <summary>The tooltip border. Type: <see cref="Brush"/>.</summary>
    public const string ToolTipBorderBrush = "NwtToolTipBorderBrush";

    /// <summary>The title bar of an active ThemedWindow. Type: <see cref="Brush"/>.</summary>
    public const string TitleBarBackgroundBrush = "NwtTitleBarBackgroundBrush";

    /// <summary>The title text of an active ThemedWindow. Type: <see cref="Brush"/>.</summary>
    public const string TitleBarForegroundBrush = "NwtTitleBarForegroundBrush";

    /// <summary>The title bar of an inactive ThemedWindow. Type: <see cref="Brush"/>.</summary>
    public const string TitleBarInactiveBackgroundBrush = "NwtTitleBarInactiveBackgroundBrush";

    /// <summary>The title text of an inactive ThemedWindow. Type: <see cref="Brush"/>.</summary>
    public const string TitleBarInactiveForegroundBrush = "NwtTitleBarInactiveForegroundBrush";

    /// <summary>The minimize, maximize and restore buttons under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string CaptionButtonHoverBrush = "NwtCaptionButtonHoverBrush";

    /// <summary>The glyphs of caption buttons under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string CaptionButtonHoverForegroundBrush = "NwtCaptionButtonHoverForegroundBrush";

    /// <summary>The close button under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string CloseButtonHoverBrush = "NwtCloseButtonHoverBrush";

    /// <summary>The close glyph under the mouse. Type: <see cref="Brush"/>.</summary>
    public const string CloseButtonHoverForegroundBrush = "NwtCloseButtonHoverForegroundBrush";

    /// <summary>Validation errors. Type: <see cref="Brush"/>.</summary>
    public const string ErrorBrush = "NwtErrorBrush";

    /// <summary>Every key with the type its value must have. A complete theme defines all of them.</summary>
    public static IReadOnlyList<KeyValuePair<string, Type>> All { get; } = new KeyValuePair<string, Type>[]
    {
        new(ThemeName, typeof(string)),
        new(FontFamily, typeof(FontFamily)),
        new(InputFontFamily, typeof(FontFamily)),
        new(FontSize, typeof(double)),
        new(FontSizeSmall, typeof(double)),
        new(FontSizeLarge, typeof(double)),
        new(ControlCornerRadius, typeof(CornerRadius)),
        new(OverlayCornerRadius, typeof(CornerRadius)),
        new(ControlBorderThickness, typeof(Thickness)),
        new(ControlPadding, typeof(Thickness)),
        new(ControlMinHeight, typeof(double)),
        new(FocusStrokeThickness, typeof(double)),
        new(BackgroundColor, typeof(Color)),
        new(SurfaceColor, typeof(Color)),
        new(ForegroundColor, typeof(Color)),
        new(AccentColor, typeof(Color)),
        new(BorderColor, typeof(Color)),
        new(ShadowColor, typeof(Color)),
        new(BackgroundBrush, typeof(Brush)),
        new(SurfaceBrush, typeof(Brush)),
        new(SurfaceAltBrush, typeof(Brush)),
        new(ForegroundBrush, typeof(Brush)),
        new(ForegroundSecondaryBrush, typeof(Brush)),
        new(ForegroundDisabledBrush, typeof(Brush)),
        new(BorderBrush, typeof(Brush)),
        new(BorderHoverBrush, typeof(Brush)),
        new(SeparatorBrush, typeof(Brush)),
        new(ControlBackgroundBrush, typeof(Brush)),
        new(ControlHoverBrush, typeof(Brush)),
        new(ControlPressedBrush, typeof(Brush)),
        new(ControlDisabledBrush, typeof(Brush)),
        new(ControlForegroundBrush, typeof(Brush)),
        new(ControlHoverForegroundBrush, typeof(Brush)),
        new(ControlBevelBrush, typeof(Brush)),
        new(InputBackgroundBrush, typeof(Brush)),
        new(InputForegroundBrush, typeof(Brush)),
        new(InputBorderBrush, typeof(Brush)),
        new(InputBevelBrush, typeof(Brush)),
        new(AccentBrush, typeof(Brush)),
        new(AccentHoverBrush, typeof(Brush)),
        new(AccentPressedBrush, typeof(Brush)),
        new(AccentForegroundBrush, typeof(Brush)),
        new(SelectionBrush, typeof(Brush)),
        new(SelectionForegroundBrush, typeof(Brush)),
        new(SelectionInactiveBrush, typeof(Brush)),
        new(SelectionInactiveForegroundBrush, typeof(Brush)),
        new(HoverBrush, typeof(Brush)),
        new(HoverForegroundBrush, typeof(Brush)),
        new(FocusBrush, typeof(Brush)),
        new(GlyphBrush, typeof(Brush)),
        new(TrackBrush, typeof(Brush)),
        new(ThumbBrush, typeof(Brush)),
        new(ThumbHoverBrush, typeof(Brush)),
        new(ThumbPressedBrush, typeof(Brush)),
        new(ToolTipBackgroundBrush, typeof(Brush)),
        new(ToolTipForegroundBrush, typeof(Brush)),
        new(ToolTipBorderBrush, typeof(Brush)),
        new(TitleBarBackgroundBrush, typeof(Brush)),
        new(TitleBarForegroundBrush, typeof(Brush)),
        new(TitleBarInactiveBackgroundBrush, typeof(Brush)),
        new(TitleBarInactiveForegroundBrush, typeof(Brush)),
        new(CaptionButtonHoverBrush, typeof(Brush)),
        new(CaptionButtonHoverForegroundBrush, typeof(Brush)),
        new(CloseButtonHoverBrush, typeof(Brush)),
        new(CloseButtonHoverForegroundBrush, typeof(Brush)),
        new(ErrorBrush, typeof(Brush)),
    };
}
