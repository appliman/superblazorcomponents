using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.SuperHtmlEditor;

public partial class SuperHtmlEditor : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<SuperHtmlEditor>? _dotnet;

    private ElementReference _editorRef;
    private ElementReference _monacoRef;
    private ElementReference _toolbarRef;

    private bool _isHtmlMode;
    private bool _monacoLoading;
    private bool _monacoReady;
    private bool _isFocused;

    // Toolbar state mirrors
    private bool _boldActive;
    private bool _italicActive;
    private bool _underlineActive;
    private string _textColor = "#000000";
    private string _bgColor = "#ffff00";

    // ── Parameters ───────────────────────────────────────────────────────────

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

    /// <summary>Minimum height of the editable area in pixels.</summary>
    [Parameter]
    public int MinHeight { get; set; } = 150;

    /// <summary>Maximum height of the editable area in pixels. 0 means no limit.</summary>
    [Parameter]
    public int MaxHeight { get; set; } = 0;

    /// <summary>Height of the Monaco editor panel in pixels when in HTML mode.</summary>
    [Parameter]
    public int MonacoHeight { get; set; } = 300;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SuperBlazorComponents/Components/SuperHtmlEditor/SuperHtmlEditor.razor.js");

        _dotnet = DotNetObjectReference.Create(this);

        await _module.InvokeVoidAsync("initialize", _editorRef, _toolbarRef, _dotnet, Value ?? "");
    }

    // ── JS → .NET callbacks ──────────────────────────────────────────────────

    [JSInvokable]
    public async Task OnContentChanged(string html)
    {
        if (Value != html)
        {
            Value = html;
            await ValueChanged.InvokeAsync(html);
        }
    }

    [JSInvokable]
    public void OnFocusChanged(bool focused)
    {
        _isFocused = focused;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnSelectionStateChanged(bool bold, bool italic, bool underline)
    {
        _boldActive = bold;
        _italicActive = italic;
        _underlineActive = underline;
        StateHasChanged();
    }

    // ── Toolbar actions ──────────────────────────────────────────────────────

    private async Task ToggleBold()
    {
        await ExecCommand("bold", null);
    }

    private async Task ToggleItalic()
    {
        await ExecCommand("italic", null);
    }

    private async Task ToggleUnderline()
    {
        await ExecCommand("underline", null);
    }

    private async Task OnFontFamilyChange(ChangeEventArgs e)
    {
        var family = e.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(family))
        {
            await ExecCommand("fontName", family);
        }
    }

    private async Task OnFontSizeChange(ChangeEventArgs e)
    {
        var size = e.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(size))
        {
            await ExecCommand("fontSize", size);
        }
    }

    private async Task OnTextColorChange(ChangeEventArgs e)
    {
        var color = e.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(color))
        {
            _textColor = color;
            await ExecCommand("foreColor", color);
        }
    }

    private async Task OnBgColorChange(ChangeEventArgs e)
    {
        var color = e.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(color))
        {
            _bgColor = color;
            await ExecCommand("hiliteColor", color);
        }
    }

    private async Task ExecCommand(string command, string? value)
    {
        if (_module is null || _isHtmlMode)
        {
            return;
        }

        await _module.InvokeVoidAsync("execCommand", _editorRef, command, value);
    }

    // ── HTML mode toggle ─────────────────────────────────────────────────────

    private async Task ToggleHtmlMode()
    {
        if (_module is null)
        {
            return;
        }

        if (!_isHtmlMode)
        {
            // WYSIWYG → HTML source: read current HTML then launch Monaco
            var html = await _module.InvokeAsync<string>("getHtml", _editorRef);
            _isHtmlMode = true;
            _monacoLoading = !_monacoReady;
            StateHasChanged();

            if (!_monacoReady)
            {
                await _module.InvokeVoidAsync("loadMonaco", _monacoRef, html);
                _monacoReady = true;
                _monacoLoading = false;
                StateHasChanged();
            }
            else
            {
                await _module.InvokeVoidAsync("setMonacoValue", html);
            }
        }
        else
        {
            // HTML source → WYSIWYG: read Monaco value, update editor
            var html = await _module.InvokeAsync<string>("getMonacoValue");
            _isHtmlMode = false;
            StateHasChanged();
            await _module.InvokeVoidAsync("setHtml", _editorRef, html);
            Value = html;
            await ValueChanged.InvokeAsync(html);
        }
    }

    // ── IAsyncDisposable ─────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose", _editorRef);
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected — ignore
            }
        }

        _dotnet?.Dispose();
    }
}
