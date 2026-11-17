using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NWpfThemes.Gallery;

/// <summary>Renders the gallery in every theme to PNG files, for CI and the README.</summary>
internal static class Screenshots
{
    private const double Width = 1280;

    public static void CaptureAll(string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (Theme theme in Theme.BuiltInThemes)
        {
            // The build machine does not run in high contrast, so show the theme with a typical palette.
            ThemeManager.ApplyTheme(ReferenceEquals(theme, Theme.HighContrast) ? HighContrastPreview.Theme : theme);
            string file = Path.Combine(folder, $"gallery-{Slug(theme.Name)}.png");
            Save(Render(), file);
        }
    }

    private static BitmapSource Render()
    {
        var root = new Border { Width = Width, Child = new GalleryView(), UseLayoutRounding = true, SnapsToDevicePixels = true };
        root.SetResourceReference(Border.BackgroundProperty, ThemeKeys.BackgroundBrush);
        root.SetResourceReference(TextElement.ForegroundProperty, ThemeKeys.ForegroundBrush);
        root.SetResourceReference(TextElement.FontFamilyProperty, ThemeKeys.FontFamily);
        root.SetResourceReference(TextElement.FontSizeProperty, ThemeKeys.FontSize);
        TextOptions.SetTextFormattingMode(root, TextFormattingMode.Display);

        for (int pass = 0; pass < 3; pass++)
        {
            root.Measure(new Size(Width, double.PositiveInfinity));
            root.Arrange(new Rect(root.DesiredSize));
            root.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(static () => { }, DispatcherPriority.ContextIdle);
        }

        int height = (int)Math.Ceiling(root.ActualHeight);
        var bitmap = new RenderTargetBitmap((int)Width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(root);
        bitmap.Freeze();
        return bitmap;
    }

    private static void Save(BitmapSource bitmap, string file)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream stream = File.Create(file);
        encoder.Save(stream);
    }

    private static string Slug(string name) => name.Replace(" ", "-").ToLowerInvariant();
}
