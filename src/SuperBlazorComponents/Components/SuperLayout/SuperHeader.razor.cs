namespace SuperBlazorComponents.Components.SuperLayout;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Composant Header avec menu rétractable responsive.
/// </summary>
public partial class SuperHeader : ComponentBase
{
    private bool _navbarExpanded;

    /// <summary>
    /// Référence au SuperLayout parent.
    /// </summary>
    [CascadingParameter]
    public SuperLayout ParentLayout { get; set; } = default!;

    /// <summary>
    /// Contenu principal du header (menu, liens, etc.).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Contenu personnalisé pour la marque/logo.
    /// </summary>
    [Parameter]
    public RenderFragment? Brand { get; set; }

    [Parameter]
    public RenderFragment? Toolbar { get; set; }

	/// <summary>
	/// Texte simple pour la marque (si Brand n'est pas défini).
	/// </summary>
	[Parameter]
    public string? BrandText { get; set; }

    /// <summary>
    /// Contenu affiché à droite du header.
    /// </summary>
    [Parameter]
    public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// Classe CSS additionnelle pour le header.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Indique si le header est sticky (fixé en haut). Par défaut: true.
    /// </summary>
    [Parameter]
    public bool Sticky { get; set; } = true;

    /// <summary>
    /// Affiche le bouton toggle pour la sidebar. Par défaut: true.
    /// </summary>
    [Parameter]
    public bool ShowToggle { get; set; } = true;

    /// <summary>
    /// Classe de couleur Bootstrap pour la navbar (navbar-light, navbar-dark).
    /// </summary>
    [Parameter]
    public string? NavbarClass { get; set; }

    /// <summary>
    /// Hauteur du header en pixels.
    /// </summary>
    [Parameter]
    public int Height { get; set; } = 56;

    /// <summary>
    /// Callback déclenché lors du clic sur le toggle sidebar.
    /// </summary>
    [Parameter]
    public EventCallback OnToggle { get; set; }

    /// <summary>
    /// Indique si le menu mobile est étendu.
    /// </summary>
    public bool NavbarExpanded
    {
        get => _navbarExpanded;
        set
        {
            if (_navbarExpanded != value)
            {
                _navbarExpanded = value;
                StateHasChanged();
            }
        }
    }

    internal string StickyClass => Sticky ? "super-header-sticky" : "";

    private async Task OnToggleClick()
    {
        ParentLayout.ToggleSidebar();

        if (OnToggle.HasDelegate)
        {
            await OnToggle.InvokeAsync();
        }
    }
}
