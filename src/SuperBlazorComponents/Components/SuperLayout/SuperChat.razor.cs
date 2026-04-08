namespace SuperBlazorComponents.Components.SuperLayout;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Panneau de chat latéral droit, masqué par défaut, activable via SuperChat.
/// </summary>
public partial class SuperChat : ComponentBase, IDisposable
{
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

            if (ParentLayout is not null)
            {
                styles.Add($"width: {ParentLayout.CurrentChatPanelWidth}px");
            }

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

    public void Dispose()
    {
        if (ParentLayout is not null)
        {
            ParentLayout.OnChatPanelStateChanged -= HandleChatPanelStateChanged;
        }
    }
}
