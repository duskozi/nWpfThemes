using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xunit;

namespace NWpfThemes.Tests;

public class ControlStyleTests
{
    private static readonly Type[] StyledTypes =
    {
        typeof(Button), typeof(RepeatButton), typeof(ToggleButton), typeof(CheckBox), typeof(RadioButton),
        typeof(TextBox), typeof(PasswordBox), typeof(ComboBox), typeof(ComboBoxItem), typeof(Slider),
        typeof(ScrollBar), typeof(ScrollViewer), typeof(ListBox), typeof(ListBoxItem), typeof(ListView),
        typeof(ListViewItem), typeof(GridViewColumnHeader), typeof(TreeView), typeof(TreeViewItem),
        typeof(DataGrid), typeof(DataGridColumnHeader), typeof(DataGridRowHeader), typeof(DataGridRow),
        typeof(DataGridCell), typeof(TabControl), typeof(TabItem), typeof(Menu), typeof(MenuItem),
        typeof(ContextMenu), typeof(ToolTip), typeof(ProgressBar), typeof(GroupBox), typeof(Expander),
        typeof(StatusBar), typeof(StatusBarItem), typeof(ToolBar), typeof(ToolBarTray), typeof(Label),
        typeof(Separator), typeof(ResizeGrip), typeof(ThemedWindow),
    };

    public static TheoryData<string> BuiltInThemeNames()
    {
        var data = new TheoryData<string>();
        foreach (Theme theme in Theme.BuiltInThemes)
        {
            data.Add(theme.Name);
        }

        return data;
    }

    [Fact]
    public void Control_styles_cover_every_control_in_scope() => UiThread.Run(() =>
    {
        var styles = new ResourceDictionary { Source = Theme.ControlStylesSource };

        Type[] missing = StyledTypes.Where(t => styles[t] is not Style).ToArray();

        Assert.Empty(missing);
    });

    [Fact]
    public void Toolbar_item_styles_are_defined() => UiThread.Run(() =>
    {
        var styles = new ResourceDictionary { Source = Theme.ControlStylesSource };
        ResourceKey[] keys =
        {
            ToolBar.ButtonStyleKey, ToolBar.ToggleButtonStyleKey, ToolBar.CheckBoxStyleKey, ToolBar.RadioButtonStyleKey,
            ToolBar.TextBoxStyleKey, ToolBar.ComboBoxStyleKey, ToolBar.SeparatorStyleKey,
            MenuItem.SeparatorStyleKey, StatusBar.SeparatorStyleKey, GridView.GridViewScrollViewerStyleKey,
        };

        Assert.All(keys, key => Assert.IsType<Style>(styles[key]));
    });

    [Theory]
    [MemberData(nameof(BuiltInThemeNames))]
    public void Every_control_builds_its_template_in_every_theme(string name) => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(ThemeManager.Find(name)!);
        var panel = new StackPanel();
        var root = new Border { Child = panel, Width = 800 };

        List<FrameworkElement> elements = SampleControls.Create();
        foreach (FrameworkElement element in elements)
        {
            panel.Children.Add(element);
        }

        Control[] controls = elements.OfType<Control>()
            .Concat(elements.OfType<ToolBarTray>().SelectMany(t => t.ToolBars))
            .ToArray();

        root.Measure(new Size(800, double.PositiveInfinity));
        root.Arrange(new Rect(root.DesiredSize));
        root.UpdateLayout();
        UiThread.DoEvents();

        var failures = controls
            .Where(c => c.Template is null || VisualTreeHelper.GetChildrenCount(c) == 0)
            .Select(c => c.GetType().Name)
            .ToArray();
        Assert.Empty(failures);
    });

    [Theory]
    [MemberData(nameof(BuiltInThemeNames))]
    public void Styles_pick_up_theme_values(string name) => UiThread.Run(() =>
    {
        Theme theme = ThemeManager.Find(name)!;
        UiThread.ResetApplicationResources(theme);
        var button = new Button();
        var textBox = new TextBox();
        var panel = new StackPanel { Children = { button, textBox } };
        panel.Measure(new Size(200, 200));

        Assert.Same(Application.Current.FindResource(ThemeKeys.ControlBackgroundBrush), button.Background);
        Assert.Same(Application.Current.FindResource(ThemeKeys.InputBackgroundBrush), textBox.Background);
        Assert.Same(Application.Current.FindResource(ThemeKeys.FontFamily), button.FontFamily);
    });

    [Fact]
    public void Switching_theme_restyles_existing_controls() => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(Theme.Dark);
        var button = new Button();
        var panel = new StackPanel { Children = { button } };
        panel.Measure(new Size(200, 200));
        object darkBackground = button.Background;

        ThemeManager.ApplyTheme(Theme.Light);
        panel.Measure(new Size(200, 200));

        Assert.NotSame(darkBackground, button.Background);
        Assert.Same(Application.Current.FindResource(ThemeKeys.ControlBackgroundBrush), button.Background);
    });

    [Fact]
    public void Every_Nwt_resource_reference_in_the_styles_exists()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "Sources", "Styles");
        string[] files = Directory.GetFiles(folder, "*.xaml");
        Assert.NotEmpty(files);

        var known = ThemeKeys.All.Select(k => k.Key).ToHashSet();
        foreach (string file in files)
        {
            foreach (Match m in Regex.Matches(File.ReadAllText(file), "x:Key=\"(Nwt\\w+)\""))
            {
                known.Add(m.Groups[1].Value);
            }
        }

        var unknown = files
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), "(?:Dynamic|Static)Resource (Nwt\\w+)")
                .Select(m => $"{Path.GetFileName(f)}: {m.Groups[1].Value}"))
            .Where(r => !known.Contains(r[(r.IndexOf(' ') + 1)..]))
            .Distinct()
            .ToArray();

        Assert.Empty(unknown);
    }

    [Fact]
    public void Styles_never_hard_code_colours()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "Sources", "Styles");
        var offenders = Directory.GetFiles(folder, "*.xaml")
            .SelectMany(f => File.ReadAllLines(f).Select((line, i) => (File: Path.GetFileName(f), Line: i + 1, Text: line)))
            .Where(l => Regex.IsMatch(l.Text, "=\"#[0-9A-Fa-f]{6,8}\"") && !l.Text.Contains("GradientStop", StringComparison.Ordinal))
            .Select(l => $"{l.File}:{l.Line}")
            .ToArray();

        Assert.Empty(offenders);
    }
}
