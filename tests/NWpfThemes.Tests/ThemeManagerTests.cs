using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace NWpfThemes.Tests;

public class ThemeManagerTests
{
    [Theory]
    [InlineData("Dark")]
    [InlineData("light")]
    [InlineData("HighContrast")]
    [InlineData("high contrast")]
    [InlineData("HARDWARE")]
    public void Find_ignores_case_and_spaces(string name) => Assert.NotNull(ThemeManager.Find(name));

    [Fact]
    public void Find_returns_null_for_unknown_names() => Assert.Null(ThemeManager.Find("Nope"));

    [Fact]
    public void Built_in_themes_are_listed_first() =>
        Assert.Equal(Theme.BuiltInThemes, ThemeManager.Themes.Take(Theme.BuiltInThemes.Count));

    [Fact]
    public void Registering_a_duplicate_name_throws() =>
        Assert.Throws<InvalidOperationException>(() => ThemeManager.Register(new Theme("dark", Theme.Dark.Source)));

    [Fact]
    public void Apply_replaces_the_theme_instead_of_adding_one() => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(Theme.Dark);
        int before = CountThemeDictionaries(Application.Current.Resources);

        ThemeManager.ApplyTheme(Theme.Light);
        ThemeManager.ApplyTheme(Theme.Hardware);

        Assert.Equal(1, before);
        Assert.Equal(1, CountThemeDictionaries(Application.Current.Resources));
        Assert.Same(Theme.Hardware, ThemeManager.CurrentTheme);
        Assert.Equal("Hardware", Application.Current.FindResource(ThemeKeys.ThemeName));
    });

    [Fact]
    public void Apply_by_name_works_and_rejects_unknown_names() => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(Theme.Dark);

        ThemeManager.ApplyTheme("light");

        Assert.Same(Theme.Light, ThemeManager.CurrentTheme);
        Assert.Throws<ArgumentException>(() => ThemeManager.ApplyTheme("Nope"));
    });

    [Fact]
    public void Apply_adds_a_theme_when_none_is_merged() => UiThread.Run(() =>
    {
        Application.Current.Resources = new ResourceDictionary();

        ThemeManager.ApplyTheme(Theme.Light);

        Assert.Same(Theme.Light, ThemeManager.CurrentTheme);
    });

    [Fact]
    public void Theme_changed_is_raised_with_the_scope() => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(Theme.Dark);
        ThemeChangedEventArgs? seen = null;
        void Handler(object? s, ThemeChangedEventArgs e) => seen = e;
        ThemeManager.ThemeChanged += Handler;
        try
        {
            var panel = new StackPanel();
            ThemeManager.ApplyTheme(panel, Theme.Light);

            Assert.NotNull(seen);
            Assert.Same(panel, seen!.Scope);
            Assert.Same(Theme.Light, seen.RequestedTheme);
        }
        finally
        {
            ThemeManager.ThemeChanged -= Handler;
        }
    });

    [Fact]
    public void Scoped_theme_applies_only_inside_the_element() => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(Theme.Dark);
        var inside = new Button();
        var scope = new StackPanel { Children = { inside } };
        var outside = new Button();
        var root = new StackPanel { Children = { scope, outside } };

        ThemeManager.ApplyTheme(scope, Theme.Light);
        root.Measure(new Size(300, 300));

        Assert.Equal("Light", inside.FindResource(ThemeKeys.ThemeName));
        Assert.Equal("Dark", outside.FindResource(ThemeKeys.ThemeName));
        Assert.Same(scope.FindResource(ThemeKeys.ControlBackgroundBrush), inside.Background);
        Assert.NotSame(inside.Background, outside.Background);

        ThemeManager.ClearTheme(scope);
        Assert.Equal("Dark", inside.FindResource(ThemeKeys.ThemeName));
    });

    [Fact]
    public void Attached_property_themes_a_subtree() => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(Theme.Dark);
        var scope = new Border();

        ThemeManager.SetTheme(scope, "Hardware");
        Assert.Equal("Hardware", scope.FindResource(ThemeKeys.ThemeName));

        ThemeManager.SetTheme(scope, null);
        Assert.Equal("Dark", scope.FindResource(ThemeKeys.ThemeName));
    });

    [Fact]
    public void Theme_resources_starts_with_the_named_theme() => UiThread.Run(() =>
    {
        var resources = new ThemeResources { Theme = "Light" };

        Assert.Equal("Light", resources[ThemeKeys.ThemeName]);
        Assert.IsType<Style>(resources[typeof(Button)]);
        Assert.Throws<ArgumentException>(() => resources.Theme = "Nope");
    });

    [Fact]
    public void Custom_themes_can_be_registered_and_applied() => UiThread.Run(() =>
    {
        UiThread.ResetApplicationResources(Theme.Dark);
        var custom = new Theme("Test Copy Of Light", Theme.Light.Source);
        if (ThemeManager.Find(custom.Name) is null)
        {
            ThemeManager.Register(custom);
        }

        ThemeManager.ApplyTheme(custom.Name);

        Assert.Equal(custom.Name, ThemeManager.CurrentTheme!.Name);
    });

    private static int CountThemeDictionaries(ResourceDictionary root) =>
        root.MergedDictionaries.Sum(d => (d.Keys.Cast<object>().Contains(ThemeKeys.ThemeName) ? 1 : 0) + CountThemeDictionaries(d));
}
