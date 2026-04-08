namespace SuperBlazorComponents.Components.SuperLayout;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Composant Sidebar responsive avec largeur configurable.
/// </summary>
public partial class SuperSidebar : ComponentBase
{
    /// <summary>
    /// Référence au SuperLayout parent.
    /// </summary>
    [CascadingParameter]
    public SuperLayout? ParentLayout { get; set; }

    /// <summary>
    /// Contenu de navigation de la sidebar.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Contenu de l'en-tête de la sidebar.
    /// </summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// Contenu du pied de la sidebar.
    /// </summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// Classe CSS additionnelle.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Thème visuel optionnel de la sidebar. Exemple: "software".
    /// Si null/vide, le thème Bootstrap (dark/light) s'applique.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// Style inline additionnel.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Affiche un overlay sur mobile quand la sidebar est visible. Par défaut: true.
    /// </summary>
    [Parameter]
    public bool ShowOverlay { get; set; } = true;

    /// <summary>
    /// Callback déclenché au clic sur l'overlay.
    /// </summary>
    [Parameter]
    public EventCallback OnOverlayClicked { get; set; }

	protected override void OnInitialized()
	{
		if (ParentLayout is not null)
		{
			ParentLayout.OnSidebarStateChanged += HandleSidebarStateChanged;
		}
	}

	private void HandleSidebarStateChanged(SidebarState previousState, SidebarState newState)
	{
		StateHasChanged();
	}

    internal string StateClass => ParentLayout?.SidebarState switch
    {
        SidebarState.Expanded => "super-sidebar-expanded",
        SidebarState.Collapsed => "super-sidebar-collapsed",
        SidebarState.Hidden => "super-sidebar-hidden",
        _ => "super-sidebar-expanded"
    };

    internal string ThemeClass => string.IsNullOrWhiteSpace(Theme) ? string.Empty : $"super-theme-{Theme}";

    internal string StyleString
    {
        get
        {
            var styles = new List<string>();

            if (ParentLayout is not null)
            {
                styles.Add($"width: {ParentLayout.CurrentSidebarWidth}px");
            }

            if (!string.IsNullOrEmpty(Style))
            {
                styles.Add(Style);
            }

            return string.Join("; ", styles);
        }
    }

    private async Task OnOverlayClick()
    {
        ParentLayout?.SetSidebarState(SidebarState.Hidden);

        if (OnOverlayClicked.HasDelegate)
        {
            await OnOverlayClicked.InvokeAsync();
        }
    }

    public void Dispose()
    {
        if (ParentLayout is not null)
        {
            ParentLayout.OnSidebarStateChanged -= HandleSidebarStateChanged;
        }
    }
}
