namespace SuperBlazorComponents.Components.SuperLayout;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Bouton externe pour basculer l'affichage du panneau de chat IA.
/// </summary>
public partial class SuperChatButton : ComponentBase, IDisposable
{
    /// <summary>
    /// Référence au SuperLayout parent.
    /// </summary>
    [CascadingParameter]
    public SuperLayout? ParentLayout { get; set; }

    /// <summary>
    /// Icône Font Awesome (sans préfixe de style). Par défaut: "fa-comments".
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "fa-comments";

    /// <summary>
    /// Style de l'icône Font Awesome. Par défaut: Solid.
    /// </summary>
    [Parameter]
    public SuperIconStyle IconStyle { get; set; } = SuperIconStyle.Solid;

    /// <summary>
    /// Texte optionnel affiché à côté de l'icône.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Tooltip du bouton. Par défaut: "Chat IA".
    /// </summary>
    [Parameter]
    public string Tooltip { get; set; } = "Chat IA";

    /// <summary>
    /// Classe CSS additionnelle.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Callback déclenché lors du clic sur le bouton.
    /// </summary>
    [Parameter]
    public EventCallback OnToggle { get; set; }

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

    internal string IconCssClass
    {
        get
        {
            var prefix = IconStyle switch
            {
                SuperIconStyle.Regular => "fa-regular",
                SuperIconStyle.Brands => "fa-brands",
                SuperIconStyle.Duotone => "fa-duotone",
                _ => "fa-solid"
            };
            return $"{prefix} {Icon}";
        }
    }

    internal string ActiveClass => ParentLayout?.ChatPanelState == ChatState.Open
        ? "super-chat-active"
        : "";

    private async Task OnToggleClick()
    {
        ParentLayout?.ToggleChatPanel();

        if (OnToggle.HasDelegate)
        {
            await OnToggle.InvokeAsync();
        }
    }

    public void Dispose()
    {
        if (ParentLayout is not null)
        {
            ParentLayout.OnChatPanelStateChanged -= HandleChatPanelStateChanged;
        }
    }
}
