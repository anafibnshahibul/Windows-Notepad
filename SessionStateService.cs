using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;

namespace AdvancedNotepad.App;

public class SessionStateService
{
    private readonly string _cacheDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AdvancedNotepad", "Drafts");

    public SessionStateService() => Directory.CreateDirectory(_cacheDir);

    public void PersistTab(EditorTabViewModel tab)
    {
        var meta = new TabDraft(tab.FilePath, tab.Title, tab.Document.Text, tab.Encoding, tab.IsDirty);
        var file = Path.Combine(_cacheDir, $"{tab.GetHashCode()}.json");
        File.WriteAllText(file, System.Text.Json.JsonSerializer.Serialize(meta));
    }

    public List<EditorTabViewModel> LoadPendingTabs()
    {
        var result = new List<EditorTabViewModel>();
        foreach (var f in Directory.EnumerateFiles(_cacheDir, "*.json"))
        {
            var meta = System.Text.Json.JsonSerializer.Deserialize<TabDraft>(File.ReadAllText(f));
            if (meta is null) continue;
            result.Add(new EditorTabViewModel
            {
                FilePath = meta.FilePath,
                Title = meta.Title + " (recovered)",
                Document = new TextDocument(meta.Content),
                Encoding = meta.Encoding,
                IsDirty = meta.IsDirty
            });
        }
        return result;
    }
}

public record TabDraft(string? FilePath, string Title, string Content, string Encoding, bool IsDirty);