namespace SuperBlazorComponents.Components.SuperLayout;

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

/// <summary>
/// Panneau de chat latéral droit, masqué par défaut, activable via SuperChat.
/// Peut être redimensionné en glissant sa bordure gauche lorsque <see cref="Resizable"/> est <c>true</c>.
/// </summary>
public partial class SuperChat : ComponentBase, IAsyncDisposable
{
    private const string DefaultStoragePrefix = "SuperBlazorComponents.Components.SuperLayout.SuperChat";

    private ElementReference resizerHandle;
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<SuperChat>? dotNetRef;
    private bool resizerInitialized;
    private bool restoredFromStorage;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Référence au SuperLayout parent.
    /// </summary>
    [CascadingParameter]
    public SuperLayout? ParentLayout { get; set; }

    /// <summary>
    /// Contenu principal du panneau de chat.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Contenu personnalisé pour l'en-tête du panneau. Si défini, remplace le titre et le bouton fermer par défaut.
    /// </summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// Contenu personnalisé pour le pied du panneau.
    /// </summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// Titre affiché dans l'en-tête par défaut. Par défaut: "Chat IA".
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Chat IA";

    /// <summary>
    /// Classe CSS additionnelle.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Style inline additionnel.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Active le redimensionnement du panneau de chat via une poignée sur sa bordure gauche. Par défaut: <c>true</c>.
    /// </summary>
    [Parameter]
    public bool Resizable { get; set; } = true;

    /// <summary>
    /// Largeur minimale du panneau de chat en pixels lors du redimensionnement. Par défaut: 240px.
    /// </summary>
    [Parameter]
    public int MinWidth { get; set; } = 240;

    /// <summary>
    /// Largeur maximale du panneau de chat en pixels lors du redimensionnement. Par défaut: 800px.
    /// </summary>
    [Parameter]
    public int MaxWidth { get; set; } = 800;

    /// <summary>
    /// Active la persistance de la largeur dans <c>localStorage</c>. Par défaut: <c>true</c>.
    /// </summary>
    [Parameter]
    public bool EnableStatePersistence { get; set; } = true;

    /// <summary>
    /// Clé de persistance personnalisée. Si non définie, une clé est générée à partir de l'URL.
    /// </summary>
    [Parameter]
    public string? PersistenceKey { get; set; }

    /// <summary>
    /// Événement déclenché à la fin du redimensionnement avec la nouvelle largeur en pixels.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnWidthChanged { get; set; }

    protected override void OnInitialized()
    {
        if (ParentLayout is not null)
        {
            ParentLayout.OnChatPanelStateChanged += HandleChatPanelStateChanged;
        }
    }

    private void HandleChatPanelStateChanged(ChatState previousState, ChatState newState)
    {
        StateHasChanged();
    }

    internal string StateClass => ParentLayout?.ChatPanelState switch
    {
        ChatState.Open => "super-chatpanel-open",
        ChatState.Hidden => "super-chatpanel-hidden",
        _ => "super-chatpanel-hidden"
    };

    internal string StyleString
    {
        get
        {
            var styles = new List<string>();

            if (!string.IsNullOrEmpty(Style))
            {
                styles.Add(Style);
            }

            return string.Join("; ", styles);
        }
    }

    private void OnCloseClick()
    {
        ParentLayout?.SetChatPanelState(ChatState.Hidden);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SuperBlazorComponents/Components/SuperLayout/SuperChat.razor.js");

            await TryRestoreWidthAsync();
        }

        if (jsModule is null)
        {
            return;
        }

        var shouldBeInitialized = Resizable && ParentLayout?.ChatPanelState == ChatState.Open;
        if (shouldBeInitialized && !resizerInitialized)
        {
            dotNetRef ??= DotNetObjectReference.Create(this);
            try
            {
                await jsModule.InvokeVoidAsync(
                    "initChatResizer",
                    resizerHandle,
                    dotNetRef,
                    new { minWidth = MinWidth, maxWidth = MaxWidth });
                resizerInitialized = true;
            }
            catch (JSException)
            {
                // ignore
            }
        }
        else if (!shouldBeInitialized && resizerInitialized)
        {
            try
            {
                await jsModule.InvokeVoidAsync("disposeChatResizer", resizerHandle);
            }
            catch (JSException)
            {
                // ignore
            }
            resizerInitialized = false;
        }
    }

    [JSInvokable]
    public async Task OnResizeEnd(double newWidth)
    {
        var width = (int)Math.Round(newWidth);
        if (ParentLayout is not null)
        {
            ParentLayout.SetChatPanelWidth(width);
        }

        await TryPersistWidthAsync(width);

        if (OnWidthChanged.HasDelegate)
        {
            await OnWidthChanged.InvokeAsync(width);
        }
    }

    private string GetStorageKey()
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            relativePath = "/";
        }

        var key = string.IsNullOrWhiteSpace(PersistenceKey)
            ? $"{relativePath}:{nameof(SuperChat)}"
            : PersistenceKey;

        return $"{DefaultStoragePrefix}:{key}";
    }

    private async Task TryRestoreWidthAsync()
    {
        if (!EnableStatePersistence || restoredFromStorage || jsModule is null)
        {
            return;
        }

        try
        {
            var key = GetStorageKey();
            var value = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (!string.IsNullOrWhiteSpace(value)
                && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                var width = (int)Math.Round(Math.Clamp(parsed, MinWidth, MaxWidth));
                if (ParentLayout is not null)
                {
                    ParentLayout.SetChatPanelWidth(width);
                }
                await jsModule.InvokeVoidAsync("setChatPanelWidth", resizerHandle, width);
            }
        }
        catch (JSException)
        {
            // ignore
        }
        finally
        {
            restoredFromStorage = true;
        }
    }

    private async Task TryPersistWidthAsync(int width)
    {
        if (!EnableStatePersistence)
        {
            return;
        }

        try
        {
            var key = GetStorageKey();
            await JSRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                key,
                width.ToString(CultureInfo.InvariantCulture));
        }
        catch (JSException)
        {
            // ignore
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (ParentLayout is not null)
        {
            ParentLayout.OnChatPanelStateChanged -= HandleChatPanelStateChanged;
        }

        if (jsModule is not null)
        {
            try
            {
                if (resizerInitialized)
                {
                    await jsModule.InvokeVoidAsync("disposeChatResizer", resizerHandle);
                }
                await jsModule.DisposeAsync();
            }
            catch (JSException)
            {
                // ignore
            }
            catch (JSDisconnectedException)
            {
                // ignore
            }
        }

        dotNetRef?.Dispose();
    }
}
