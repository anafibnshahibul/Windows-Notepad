using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace AdvancedNotepad.App;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<EditorTabViewModel> Tabs { get; } = new();

    [ObservableProperty] private EditorTabViewModel? activeTab;
    [ObservableProperty] private bool isZenMode;
    [ObservableProperty] private bool isFindReplaceOpen;
    [ObservableProperty] private double zoomPercent = 100;
    [ObservableProperty] private bool wordWrap;
    [ObservableProperty] private bool markdownPreview;

    private readonly AutoSaveService _autoSave;
    private readonly SessionStateService _session;

    public string[] EncodingOptions { get; } = { "UTF-8", "UTF-8 (BOM)", "UTF-16 LE", "ANSI" };
    public FindReplaceViewModel FindReplaceVM { get; } = new();

    public MainViewModel(AutoSaveService autoSave, SessionStateService session)
    {
        _autoSave = autoSave;
        _session = session;
        RestoreSessionOrCreateBlank();
        _autoSave.Start(Tabs, TimeSpan.FromSeconds(15));
    }

    private void RestoreSessionOrCreateBlank()
    {
        var restored = _session.LoadPendingTabs(); // reads cache under %LocalAppData%
        if (restored.Count > 0)
            foreach (var t in restored) Tabs.Add(t);
        else
            Tabs.Add(new EditorTabViewModel());
        ActiveTab = Tabs[0];
    }

    [RelayCommand]
    private void NewTab() { var t = new EditorTabViewModel(); Tabs.Add(t); ActiveTab = t; }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "All files (*.*)|*.*" };
        if (dlg.ShowDialog() != true) return;
        await OpenFilePathAsync(dlg.FileName);
    }

    public async Task OpenFilePathAsync(string path)
    {
        // For files >20MB use native engine mmap read; else standard StreamReader
        var info = new FileInfo(path);
        string content;
        Encoding enc;

        if (info.Length > 20 * 1024 * 1024)
        {
            (content, enc) = NativeTextEngineInterop.ReadLargeFile(path);
        }
        else
        {
            using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            content = await reader.ReadToEndAsync();
            enc = reader.CurrentEncoding;
        }

        var tab = new EditorTabViewModel
        {
            FilePath = path,
            Title = Path.GetFileName(path),
            Document = new TextDocument(content),
            Encoding = MapEncodingName(enc),
            HighlightingDef = SyntaxDetector.DetectFromExtension(path)
        };
        Tabs.Add(tab);
        ActiveTab = tab;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (ActiveTab is null) return;
        if (string.IsNullOrEmpty(ActiveTab.FilePath))
        {
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            ActiveTab.FilePath = dlg.FileName;
            ActiveTab.Title = Path.GetFileName(dlg.FileName);
        }
        await File.WriteAllTextAsync(ActiveTab.FilePath, ActiveTab.Document.Text, ResolveEncoding(ActiveTab.Encoding));
        ActiveTab.IsDirty = false;
    }

    [RelayCommand]
    private void CloseTab(EditorTabViewModel tab)
    {
        Tabs.Remove(tab);
        if (Tabs.Count == 0) Tabs.Add(new EditorTabViewModel());
        ActiveTab = Tabs[^1];
    }

    [RelayCommand] private void ToggleZenMode() => IsZenMode = !IsZenMode;
    [RelayCommand] private void ToggleFindReplace() => IsFindReplaceOpen = !IsFindReplaceOpen;

    private static string MapEncodingName(Encoding e) => e.EncodingName switch
    {
        var n when n.Contains("Unicode (UTF-8)") => "UTF-8",
        var n when n.Contains("Unicode") => "UTF-16 LE",
        _ => "ANSI"
    };

    private static Encoding ResolveEncoding(string name) => name switch
    {
        "UTF-8 (BOM)" => new UTF8Encoding(true),
        "UTF-16 LE" => Encoding.Unicode,
        "ANSI" => Encoding.GetEncoding(1252),
        _ => new UTF8Encoding(false)
    };
}