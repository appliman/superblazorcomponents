using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.SuperMarkdownEditor;

public partial class SuperMarkdownEditor : ComponentBase, IAsyncDisposable
{
    private const string ModulePath = "./_content/SuperBlazorComponents/Components/SuperMarkdownEditor/SuperMarkdownEditor.razor.js";

    private IJSObjectReference? _module;
    private DotNetObjectReference<SuperMarkdownEditor>? _dotnet;
    private ElementReference _renderedEditorRef;
    private ElementReference _sourceEditorRef;
    private string _editorId = $"sme-{Guid.NewGuid():N}";
    private string _renderedEditorId = $"sme-rendered-{Guid.NewGuid():N}";
    private string _sourceEditorId = $"sme-source-{Guid.NewGuid():N}";
    private bool _isFocused;
    private bool _valueSyncPending;
    private string _lastSyncedValue = string.Empty;
    private bool _renderedView = true;
    private string _renderedHtml = string.Empty;

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public bool Disabled
    {
        get => _disabled;
        set => _disabled = value;
    }

    private bool _disabled;

    [Parameter]
    public int MinHeight { get; set; } = 150;

    [Parameter]
    public int MaxHeight { get; set; }

    [Parameter]
    public bool ShowToolbar { get; set; } = true;

    [Parameter]
    public bool RenderedView
    {
        get => _renderedView;
        set => _renderedView = value;
    }

    [Parameter]
    public EventCallback<bool> RenderedViewChanged { get; set; }

    [Parameter]
    public string? EditorId { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(EditorId))
        {
            _editorId = EditorId;
        }

        _renderedEditorId = $"{_editorId}-rendered";
        _sourceEditorId = $"{_editorId}-source";

        var value = Value ?? string.Empty;
        if (!string.Equals(value, _lastSyncedValue, StringComparison.Ordinal))
        {
            _valueSyncPending = true;
        }

        _renderedHtml = RenderMarkdown(value);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            _dotnet = DotNetObjectReference.Create(this);

            var value = Value ?? string.Empty;
            await _module.InvokeVoidAsync("initialize", _renderedEditorRef, _sourceEditorRef, _dotnet, value);
            _lastSyncedValue = value;
            _valueSyncPending = false;
            _renderedHtml = RenderMarkdown(value);
            return;
        }

        if (!_valueSyncPending || _module is null)
        {
            return;
        }

        var currentValue = Value ?? string.Empty;
        await _module.InvokeVoidAsync("setValue", _renderedEditorRef, _sourceEditorRef, currentValue, false);
        _lastSyncedValue = currentValue;
        _valueSyncPending = false;
        _renderedHtml = RenderMarkdown(currentValue);
    }

    [JSInvokable]
    public async Task OnContentChanged(string markdown)
    {
        _lastSyncedValue = markdown;
        _valueSyncPending = false;
        _renderedHtml = RenderMarkdown(markdown);

        if (Value != markdown)
        {
            Value = markdown;
            await ValueChanged.InvokeAsync(markdown);
        }
    }

    [JSInvokable]
    public void OnFocusChanged(bool focused)
    {
        _isFocused = focused;
        StateHasChanged();
    }

    private async Task OnRenderedViewChanged(bool renderedView)
    {
        if (_module is not null)
        {
            var markdown = await _module.InvokeAsync<string>("getValue", _renderedEditorRef, _sourceEditorRef);
            _lastSyncedValue = markdown;
            _renderedHtml = RenderMarkdown(markdown);
        }

        _renderedView = renderedView;

        if (RenderedViewChanged.HasDelegate)
        {
            await RenderedViewChanged.InvokeAsync(renderedView);
        }

        StateHasChanged();
    }

    private static string RenderMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var html = new System.Text.StringBuilder();
        var inUnorderedList = false;
        var inOrderedList = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                if (inUnorderedList)
                {
                    html.Append("</ul>");
                    inUnorderedList = false;
                }

                if (inOrderedList)
                {
                    html.Append("</ol>");
                    inOrderedList = false;
                }

                continue;
            }

            if (trimmed.StartsWith("### ", StringComparison.Ordinal))
            {
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                html.Append("<h3>").Append(FormatInline(trimmed[4..])).Append("</h3>");
                continue;
            }

            if (trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                html.Append("<h2>").Append(FormatInline(trimmed[3..])).Append("</h2>");
                continue;
            }

            if (trimmed.StartsWith("# ", StringComparison.Ordinal))
            {
                CloseLists(html, ref inUnorderedList, ref inOrderedList);
                html.Append("<h1>").Append(FormatInline(trimmed[2..])).Append("</h1>");
                continue;
            }

            if (trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed.StartsWith("* ", StringComparison.Ordinal))
            {
                if (inOrderedList)
                {
                    html.Append("</ol>");
                    inOrderedList = false;
                }

                if (!inUnorderedList)
                {
                    html.Append("<ul>");
                    inUnorderedList = true;
                }

                html.Append("<li>").Append(FormatInline(trimmed[2..])).Append("</li>");
                continue;
            }

            if (IsOrderedListItem(trimmed, out var orderedContent))
            {
                if (inUnorderedList)
                {
                    html.Append("</ul>");
                    inUnorderedList = false;
                }

                if (!inOrderedList)
                {
                    html.Append("<ol>");
                    inOrderedList = true;
                }

                html.Append("<li>").Append(FormatInline(orderedContent)).Append("</li>");
                continue;
            }

            CloseLists(html, ref inUnorderedList, ref inOrderedList);
            html.Append("<p>").Append(FormatInline(trimmed)).Append("</p>");
        }

        CloseLists(html, ref inUnorderedList, ref inOrderedList);
        return html.ToString();
    }

    private static void CloseLists(System.Text.StringBuilder html, ref bool inUnorderedList, ref bool inOrderedList)
    {
        if (inUnorderedList)
        {
            html.Append("</ul>");
            inUnorderedList = false;
        }

        if (inOrderedList)
        {
            html.Append("</ol>");
            inOrderedList = false;
        }
    }

    private static bool IsOrderedListItem(string text, out string content)
    {
        var dotIndex = text.IndexOf(". ", StringComparison.Ordinal);
        if (dotIndex <= 0)
        {
            content = string.Empty;
            return false;
        }

        var prefix = text[..dotIndex];
        if (!int.TryParse(prefix, out _))
        {
            content = string.Empty;
            return false;
        }

        content = text[(dotIndex + 2)..];
        return true;
    }

    private static string FormatInline(string text)
    {
        var html = System.Net.WebUtility.HtmlEncode(text);
        html = System.Text.RegularExpressions.Regex.Replace(html, @"`(.+?)`", "<code>$1</code>");
        html = System.Text.RegularExpressions.Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        html = System.Text.RegularExpressions.Regex.Replace(html, @"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", "<em>$1</em>");
        return html;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose", _renderedEditorRef, _sourceEditorRef);
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _dotnet?.Dispose();
    }
}
