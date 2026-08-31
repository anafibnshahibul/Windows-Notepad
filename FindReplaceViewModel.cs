using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdvancedNotepad.App;

public partial class FindReplaceViewModel : ObservableObject
{
    [ObservableProperty] private string findText = string.Empty;
    [ObservableProperty] private string replaceText = string.Empty;
    [ObservableProperty] private bool useRegex;
    [ObservableProperty] private bool caseSensitive;
    [ObservableProperty] private int matchCount;
    [ObservableProperty] private int currentMatchIndex;

    public event Action<string, bool, bool>? FindRequested;
    public event Action<string, string, bool, bool>? ReplaceAllRequested;
    public event Action? FindNextRequested;
    public event Action? FindPreviousRequested;

    [RelayCommand]
    private void Find() => FindRequested?.Invoke(FindText, UseRegex, CaseSensitive);

    [RelayCommand]
    private void FindNext() => FindNextRequested?.Invoke();

    [RelayCommand]
    private void FindPrevious() => FindPreviousRequested?.Invoke();

    [RelayCommand]
    private void ReplaceAll() => ReplaceAllRequested?.Invoke(FindText, ReplaceText, UseRegex, CaseSensitive);
}