using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NWpfThemes.Tests;

public class ThemeDictionaryTests
{
    public static TheoryData<string> BuiltInThemeNames()
    {
        var data = new TheoryData<string>();
        foreach (Theme theme in Theme.BuiltInThemes)
        {
            data.Add(theme.Name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(BuiltInThemeNames))]
    public void Theme_defines_every_key_with_the_right_type(string name) => UiThread.Run(() =>
    {
        ResourceDictionary dictionary = ThemeManager.Find(name)!.CreateDictionary();

        var problems = new List<string>();
        foreach ((string key, Type type) in ThemeKeys.All)
        {
            if (!dictionary.Contains(key))
            {
                problems.Add($"{key} is missing");
            }
            else if (dictionary[key] is not { } value || !type.IsInstanceOfType(value))
            {
                problems.Add($"{key} is {dictionary[key]?.GetType().Name ?? "null"}, expected {type.Name}");
            }
        }

        Assert.Empty(problems);
    });

    [Theory]
    [MemberData(nameof(BuiltInThemeNames))]
    public void Theme_defines_only_public_keys(string name) => UiThread.Run(() =>
    {
        ResourceDictionary dictionary = ThemeManager.Find(name)!.CreateDictionary();
        var known = ThemeKeys.All.Select(k => k.Key).ToHashSet();

        string[] extra = dictionary.Keys.Cast<object>().Select(k => k.ToString()!).Where(k => !known.Contains(k)).ToArray();

        Assert.Empty(extra);
    });

    [Theory]
    [MemberData(nameof(BuiltInThemeNames))]
    public void Theme_name_key_matches_the_theme(string name) => UiThread.Run(() =>
    {
        ResourceDictionary dictionary = ThemeManager.Find(name)!.CreateDictionary();

        Assert.Equal(name, dictionary[ThemeKeys.ThemeName]);
    });

    [Fact]
    public void Theme_keys_are_unique() =>
        Assert.Equal(ThemeKeys.All.Count, ThemeKeys.All.Select(k => k.Key).Distinct().Count());

    [Fact]
    public void High_contrast_theme_follows_the_system_colours() => UiThread.Run(() =>
    {
        ResourceDictionary dictionary = Theme.HighContrast.CreateDictionary();
        var host = new FrameworkElement();
        host.Resources.MergedDictionaries.Add(dictionary);

        var brush = (SolidColorBrush)host.FindResource(ThemeKeys.SelectionBrush);

        Assert.Equal(SystemColors.HighlightColor, brush.Color);
    });
}
