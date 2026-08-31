using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

namespace AdvancedNotepad.App;

public partial class EditorTabViewModel : ObservableObject
{
    [ObservableProperty] private string title = "Untitled";
    [ObservableProperty] private string? filePath;
    [ObservableProperty] private bool isDirty;
    [ObservableProperty] private TextDocument document = new();
    [ObservableProperty] private string encoding = "UTF-8";
    [ObservableProperty] private IHighlightingDefinition? highlightingDef;
    [ObservableProperty] private int lineCount;
    [ObservableProperty] private int caretColumn;
    [ObservableProperty] private int wordCount;
    [ObservableProperty] private int charCount;

    public EditorTabViewModel()
    {
        document.TextChanged += (_, _) =>
        {
            IsDirty = true;
            LineCount = document.LineCount;
            CharCount = document.TextLength;
            WordCount = CountWords(document.Text);
        };
    }

    private static int CountWords(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    // Alt+Up / Alt+Down
    [RelayCommand]
    private void MoveLineUp(int caretOffset)
    {
        var line = document.GetLineByOffset(caretOffset);
        if (line.LineNumber == 1) return;
        var prev = document.GetLineByNumber(line.LineNumber - 1);
        SwapLines(prev, line);
    }

    [RelayCommand]
    private void MoveLineDown(int caretOffset)
    {
        var line = document.GetLineByOffset(caretOffset);
        if (line.LineNumber == document.LineCount) return;
        var next = document.GetLineByNumber(line.LineNumber + 1);
        SwapLines(line, next);
    }

    private void SwapLines(DocumentLine a, DocumentLine b)
    {
        string textA = document.GetText(a);
        string textB = document.GetText(b);
        document.BeginUpdate();
        document.Replace(a.Offset, a.Length, textB);
        document.Replace(b.Offset, b.Length, textA);
        document.EndUpdate();
    }

    // Ctrl+D
    [RelayCommand]
    private void DuplicateLine(int caretOffset)
    {
        var line = document.GetLineByOffset(caretOffset);
        string text = document.GetText(line);
        document.Insert(line.EndOffset, Environment.NewLine + text);
    }

    [RelayCommand] private void SortLinesAscending() =>
        ReplaceAllLines(document.Text.Split('\n').OrderBy(l => l, StringComparer.OrdinalIgnoreCase));

    [RelayCommand] private void ToUpperSelection(string sel) { /* apply via editor selection API */ }
    [RelayCommand] private void ToLowerSelection(string sel) { /* apply via editor selection API */ }

    // F5
    [RelayCommand]
    private void InsertTimestamp(int caretOffset) =>
        document.Insert(caretOffset, DateTime.Now.ToString("HH:mm dd/MM/yyyy"));

    private void ReplaceAllLines(IEnumerable<string> lines) =>
        document.Text = string.Join("\n", lines);
}