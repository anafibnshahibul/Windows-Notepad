using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;

namespace AdvancedNotepad.App;

public static class SyntaxDetector
{
    public static IHighlightingDefinition? DetectFromExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();

        string? name = ext switch
        {
            ".cs" => "C#",
            ".c" or ".h" => "C++",
            ".cpp" or ".hpp" or ".cc" => "C++",
            ".xml" or ".xaml" or ".xhtml" => "XML",
            ".html" or ".htm" => "HTML",
            ".css" => "CSS",
            ".json" => "JSON",
            ".py" => "Python",
            ".sql" => "SQL",
            ".md" or ".markdown" => "MarkDown",
            _ => null
        };

        if (name is null) return null;
        return HighlightingManager.Instance.GetDefinition(name);
    }

    public static void RegisterCustomHighlighting()
    {
        RegisterFromResource("Python", "Resources/Python.xshd");
        RegisterFromResource("SQL", "Resources/SQL.xshd");
        RegisterFromResource("MarkDown", "Resources/Markdown.xshd");
    }

    private static void RegisterFromResource(string name, string relativePath)
    {
        string full = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(full)) return;

        using var reader = new XmlTextReader(full);
        var def = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        HighlightingManager.Instance.RegisterHighlighting(name, new[] { "." + name.ToLower() }, def);
    }
}