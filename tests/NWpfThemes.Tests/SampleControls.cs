using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NWpfThemes.Tests;

/// <summary>One of each control in scope, in a few interesting states.</summary>
internal static class SampleControls
{
    public static List<FrameworkElement> Create()
    {
        var gridView = new GridView();
        gridView.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new System.Windows.Data.Binding() });

        var dataGrid = new DataGrid { ItemsSource = new[] { new Row("Ada", 36), new Row("Alan", 41) }, Height = 120 };

        var menu = new Menu();
        var file = new MenuItem { Header = "File" };
        file.Items.Add(new MenuItem { Header = "Open", InputGestureText = "Ctrl+O" });
        file.Items.Add(new MenuItem { Header = "Recent", Items = { new MenuItem { Header = "a.txt" } } });
        menu.Items.Add(file);
        menu.Items.Add(new MenuItem { Header = "Help" });

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem { Header = "Copy", IsCheckable = true, IsChecked = true });
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(new MenuItem { Header = "More", Items = { new MenuItem { Header = "Inside" } } });

        var tree = new TreeView();
        var node = new TreeViewItem { Header = "Root", IsExpanded = true };
        node.Items.Add(new TreeViewItem { Header = "Child", IsSelected = true });
        tree.Items.Add(node);

        var toolBar = new ToolBar();
        toolBar.Items.Add(new Button { Content = "Cut" });
        toolBar.Items.Add(new ToggleButton { Content = "Bold" });
        toolBar.Items.Add(new Separator());
        toolBar.Items.Add(new CheckBox { Content = "Wrap" });
        toolBar.Items.Add(new TextBox { Text = "Find" });
        toolBar.Items.Add(new ComboBox { Items = { "One" }, SelectedIndex = 0 });
        var tray = new ToolBarTray();
        tray.ToolBars.Add(toolBar);

        var tabs = new TabControl();
        tabs.Items.Add(new TabItem { Header = "One", Content = "First" });
        tabs.Items.Add(new TabItem { Header = "Two" });

        var statusBar = new StatusBar();
        statusBar.Items.Add(new StatusBarItem { Content = "Ready" });
        statusBar.Items.Add(new Separator());

        return new List<FrameworkElement>
        {
            new Button { Content = "Button" },
            new Button { Content = "Default", IsDefault = true },
            new RepeatButton { Content = "Repeat" },
            new ToggleButton { Content = "Toggle", IsChecked = true },
            new CheckBox { Content = "Check", IsChecked = true },
            new CheckBox { Content = "Three", IsThreeState = true, IsChecked = null },
            new RadioButton { Content = "Radio", IsChecked = true },
            new TextBox { Text = "Text" },
            new PasswordBox { Password = "secret" },
            new ComboBox { Items = { "A", "B" }, SelectedIndex = 0 },
            new ComboBox { Items = { "A", "B" }, IsEditable = true, Text = "Edit" },
            new Slider { Value = 4, TickPlacement = TickPlacement.Both },
            new Slider { Orientation = System.Windows.Controls.Orientation.Vertical, Height = 80 },
            new ProgressBar { Value = 50 },
            new ProgressBar { IsIndeterminate = true },
            new ProgressBar { Orientation = System.Windows.Controls.Orientation.Vertical, Height = 60, Value = 20 },
            new ScrollBar { Orientation = System.Windows.Controls.Orientation.Horizontal, Maximum = 10, ViewportSize = 2 },
            new ScrollViewer { Height = 50, Content = new Border { Height = 200 } },
            new ListBox { Items = { "One", "Two" }, SelectedIndex = 0 },
            new ListView { Items = { "Plain" } },
            new ListView { View = gridView, Items = { "Row" } },
            tree,
            dataGrid,
            tabs,
            menu,
            contextMenu,
            new ToolTip { Content = "Tip" },
            new GroupBox { Header = "Group", Content = "Content" },
            new Expander { Header = "Expander", Content = "Content", IsExpanded = true },
            new Expander { Header = "Up", ExpandDirection = ExpandDirection.Up },
            statusBar,
            tray,
            new Label { Content = "Label" },
            new Separator(),
        };
    }

    private sealed record Row(string Name, int Age);
}
