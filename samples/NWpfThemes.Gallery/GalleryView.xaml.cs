using System.Windows.Controls;

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
    }

    private sealed record Person(string Name, int Age, bool Active);
}
