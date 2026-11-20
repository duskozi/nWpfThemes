using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NWpfThemes.Gallery;

public partial class GalleryView : UserControl
{
    public GalleryView()
    {
        InitializeComponent();

        Fruits.ItemsSource = new[] { "Apple", "Banana", "Cherry", "Damson", "Elderberry", "Fig", "Grape", "Honeydew", "Kiwi" };
        Files.ItemsSource = new[]
        {
            new { Name = "readme.md", Size = "4 KB", Kind = "Markdown" },
            new { Name = "logo.png", Size = "18 KB", Kind = "Image" },
            new { Name = "app.config", Size = "1 KB", Kind = "Config" },
            new { Name = "data.csv", Size = "220 KB", Kind = "Table" },
        };
        People.ItemsSource = new[]
        {
            new Person("Ada", 36, true),
            new Person("Grace", 85, true),
            new Person("Alan", 41, false),
            new Person("Edsger", 72, true),
            new Person("Barbara", 47, false),
        };

        GlassCard.MouseLeftButtonDown += OnGlassCardPressed;
        GlassCard.MouseMove += OnGlassCardMoved;
        GlassCard.MouseLeftButtonUp += (_, _) => GlassCard.ReleaseMouseCapture();
    }

    private Point _dragOffset;

    private void OnGlassCardPressed(object sender, MouseButtonEventArgs e)
    {
        _dragOffset = e.GetPosition(GlassCard);
        GlassCard.CaptureMouse();
        e.Handled = true;
    }

    // Moving the card only changes Canvas.Left and Canvas.Top; the blur behind it follows on its own.
    private void OnGlassCardMoved(object sender, MouseEventArgs e)
    {
        if (!GlassCard.IsMouseCaptured)
        {
            return;
        }

        Point position = e.GetPosition(GlassLayer);
        double left = Math.Clamp(position.X - _dragOffset.X, 0, Math.Max(0, GlassLayer.ActualWidth - GlassCard.ActualWidth));
        double top = Math.Clamp(position.Y - _dragOffset.Y, 0, Math.Max(0, GlassLayer.ActualHeight - GlassCard.ActualHeight));
        Canvas.SetLeft(GlassCard, left);
        Canvas.SetTop(GlassCard, top);
    }

    private sealed record Person(string Name, int Age, bool Active);
}
