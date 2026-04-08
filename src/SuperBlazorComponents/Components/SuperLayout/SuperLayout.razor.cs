namespace SuperBlazorComponents.Components.SuperLayout;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>
/// Composant principal de layout responsive basé sur Bootstrap 5.3.
/// Contient SuperHeader, SuperSidebar, SuperBody, SuperFooter et SuperChatPanel.
/// </summary>
public partial class SuperLayout : ComponentBase
{
	private IJSObjectReference? jsModule;
	private SidebarState _sidebarState = SidebarState.Expanded;
	private ChatState _chatPanelState = ChatState.Hidden;
	private Device? _deviceInfo;

	[Inject]
    IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    ILogger<SuperLayout> Logger { get; set; } = default!;

	/// <summary>
	/// Contenu enfant du layout (SuperHeader, SuperSidebar, SuperBody, SuperFooter).
	/// </summary>
	[Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Classe CSS additionnelle pour le conteneur principal.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Style inline additionnel pour le conteneur principal.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Largeur de la sidebar en pixels. Par défaut: 250px.
    /// </summary>
    [Parameter]
    public int SidebarWidth { get; set; } = 250;

    /// <summary>
    /// Largeur de la sidebar réduite en pixels. Par défaut: 40px.
    /// </summary>
    [Parameter]
    public int SidebarCollapsedWidth { get; set; } = 40;

    /// <summary>
    /// Largeur du panneau de chat en pixels. Par défaut: 380px.
    /// </summary>
    [Parameter]
    public int ChatPanelWidth { get; set; } = 380;

    /// <summary>
    /// Indique l'état actuel de la sidebar.
    /// </summary>
    public SidebarState SidebarState
    {
        get => _sidebarState;
        private set
        {
            if (_sidebarState != value)
            {
                var previousState = _sidebarState;
                _sidebarState = value;
                OnSidebarStateChanged?.Invoke(previousState, _sidebarState);
                StateHasChanged();
            }
        }
    }

    /// <summary>
    /// Événement déclenché quand l'état de la sidebar change.
    /// </summary>
    public event Action<SidebarState, SidebarState>? OnSidebarStateChanged;

    /// <summary>
    /// Indique l'état actuel du panneau de chat.
    /// </summary>
    public ChatState ChatPanelState
    {
        get => _chatPanelState;
        private set
        {
            if (_chatPanelState != value)
            {
                var previousState = _chatPanelState;
                _chatPanelState = value;
                OnChatPanelStateChanged?.Invoke(previousState, _chatPanelState);
                StateHasChanged();
            }
        }
    }

    /// <summary>
    /// Événement déclenché quand l'état du panneau de chat change.
    /// </summary>
    public event Action<ChatState, ChatState>? OnChatPanelStateChanged;

    /// <summary>
    /// Bascule l'état de la sidebar (étendue, réduite, masquée).
    /// </summary>
    public void ToggleSidebar()
    {
        SidebarState = SidebarState switch
        {
            SidebarState.Expanded => SidebarState.Collapsed,
            SidebarState.Collapsed => SidebarState.Hidden,
            _ => SidebarState.Expanded
        };
    }

    /// <summary>
    /// Définit l'état de la sidebar.
    /// </summary>
    public void SetSidebarState(SidebarState state)
    {
        SidebarState = state;
    }

    /// <summary>
    /// Bascule l'état du panneau de chat (ouvert/masqué).
    /// </summary>
    public void ToggleChatPanel()
    {
        ChatPanelState = ChatPanelState switch
        {
            ChatState.Hidden => ChatState.Open,
            _ => ChatState.Hidden
        };
    }

    /// <summary>
    /// Définit l'état du panneau de chat.
    /// </summary>
    public void SetChatPanelState(ChatState state)
    {
        ChatPanelState = state;
    }

    /// <summary>
    /// Calcule la largeur actuelle de la sidebar.
    /// </summary>
    public int CurrentSidebarWidth => SidebarState switch
    {
        SidebarState.Expanded => SidebarWidth,
        SidebarState.Collapsed => SidebarCollapsedWidth,
        _ => 0
    };

    /// <summary>
    /// Calcule la largeur actuelle du panneau de chat.
    /// </summary>
    public int CurrentChatPanelWidth => ChatPanelState switch
    {
        ChatState.Open => ChatPanelWidth,
        _ => 0
    };

    /// <summary>
    /// Classe CSS indiquant l'état du layout (sidebar expanded/collapsed/hidden).
    /// </summary>
    internal string LayoutStateClass => SidebarState switch
    {
        SidebarState.Expanded => "super-layout-sidebar-expanded",
        SidebarState.Collapsed => "super-layout-sidebar-collapsed",
        _ => "super-layout-sidebar-hidden"
    };

    /// <summary>
    /// Classe CSS indiquant l'état du panneau de chat (open/hidden).
    /// </summary>
    internal string ChatPanelStateClass => ChatPanelState switch
    {
        ChatState.Open => "super-layout-chatpanel-open",
        _ => "super-layout-chatpanel-hidden"
    };

    internal string StyleString
    {
        get
        {
            var styles = new List<string>
            {
                $"--super-sidebar-width: {SidebarWidth}px",
                $"--super-sidebar-collapsed-width: {SidebarCollapsedWidth}px",
                $"--super-chatpanel-width: {ChatPanelWidth}px"
            };

            if (!string.IsNullOrEmpty(Style))
            {
                styles.Add(Style);
            }

            return string.Join("; ", styles);
        }
    }

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
        {
			jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
				"import", "./_content/SuperBlazorComponents/Components/SuperLayout/SuperLayout.razor.js");

            try
            {
				_deviceInfo = await jsModule.InvokeAsync<Device>("getDeviceInfo");
			}
            catch(Exception ex) 
            {
                Logger.LogError(ex, ex.Message);
            }
		}
	}
}
