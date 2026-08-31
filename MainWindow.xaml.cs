using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdvancedNotepad.App;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainViewModel(new AutoSaveService(), new SessionStateService());
        DataContext = ViewModel;

        Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

        PreviewKeyDown += MainWindow_PreviewKeyDown;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        bool alt = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

        switch (e.Key)
        {
            case Key.N when ctrl:
                ViewModel.NewTabCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.O when ctrl:
                await ViewModel.OpenFileCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.S when ctrl:
                await ViewModel.SaveCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
            case Key.H when ctrl:
                ViewModel.ToggleFindReplaceCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F11:
                ViewModel.ToggleZenModeCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.D when ctrl:
                ViewModel.ActiveTab?.DuplicateLineCommand.Execute(GetCaretOffset());
                e.Handled = true;
                break;
            case Key.Up when alt:
                ViewModel.ActiveTab?.MoveLineUpCommand.Execute(GetCaretOffset());
                e.Handled = true;
                break;
            case Key.Down when alt:
                ViewModel.ActiveTab?.MoveLineDownCommand.Execute(GetCaretOffset());
                e.Handled = true;
                break;
            case Key.F5:
                ViewModel.ActiveTab?.InsertTimestampCommand.Execute(GetCaretOffset());
                e.Handled = true;
                break;
            case Key.OemPlus when ctrl:
                ViewModel.ZoomPercent = Math.Min(300, ViewModel.ZoomPercent + 10);
                e.Handled = true;
                break;
            case Key.OemMinus when ctrl:
                ViewModel.ZoomPercent = Math.Max(50, ViewModel.ZoomPercent - 10);
                e.Handled = true;
                break;
        }
    }

    private int GetCaretOffset()
    {
        var editor = FindActiveEditor();
        return editor?.CaretOffset ?? 0;
    }

    private ICSharpCode.AvalonEdit.TextEditor? FindActiveEditor()
    {
        if (EditorTabs.SelectedContentPresenter is null) return null;
        return FindVisualChild<ICSharpCode.AvalonEdit.TextEditor>(EditorTabs);
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T match) return match;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    private void EditorTabs_TabDragReorder(object sender, MouseEventArgs e)
    {
        // Tab reorder/tear-out drag logic goes here (extension point)
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var session = new SessionStateService();
        foreach (var tab in ViewModel.Tabs)
            if (tab.IsDirty) session.PersistTab(tab);
    }
}