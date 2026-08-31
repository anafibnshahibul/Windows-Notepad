using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Markdig;

namespace AdvancedNotepad.App;

public partial class MarkdownPreviewControl : UserControl
{
    public static readonly DependencyProperty MarkdownProperty =
        DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MarkdownPreviewControl),
            new PropertyMetadata(string.Empty, OnMarkdownChanged));

    public string Markdown
    {
        get => (string)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    private readonly Timer _debounce;
    private readonly MarkdownPipeline _pipeline;
    private bool _webViewReady;

    public MarkdownPreviewControl()
    {
        InitializeComponent();

        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        _debounce = new Timer(200) { AutoReset = false };
        _debounce.Elapsed += (_, _) => Dispatcher.Invoke(RenderNow);

        Loaded += async (_, _) =>
        {
            await PreviewView.EnsureCoreWebView2Async();
            _webViewReady = true;
            RenderNow();
        };
    }

    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MarkdownPreviewControl)d;
        control._debounce.Stop();
        control._debounce.Start();
    }

    private void RenderNow()
    {
        if (!_webViewReady) return;

        string html = Markdig.Markdown.ToHtml(Markdown ?? string.Empty, _pipeline);
        string page = $"""
            <html><head><meta charset='utf-8'>
            <style>
                body {{ font-family: 'Segoe UI', sans-serif; padding: 16px; background: transparent; }}
                pre {{ background: #1e1e1e; color: #d4d4d4; padding: 8px; border-radius: 6px; overflow-x: auto; }}
                code {{ font-family: 'Cascadia Code', monospace; }}
            </style></head>
            <body>{html}</body></html>
            """;

        PreviewView.NavigateToString(page);
    }
}