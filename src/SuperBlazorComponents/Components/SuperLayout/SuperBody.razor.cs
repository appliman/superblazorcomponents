namespace SuperBlazorComponents.Components.SuperLayout;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Composant Body pour le contenu principal.
/// </summary>
public partial class SuperBody : ComponentBase
{
    /// <summary>
    /// Référence au SuperLayout parent.
    /// </summary>
    [CascadingParameter]
    public SuperLayout? ParentLayout { get; set; }

    /// <summary>
    /// Contenu principal.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

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
    /// Utilise un container fluid (100% largeur). Par défaut: true.
    /// </summary>
    [Parameter]
    public bool Fluid { get; set; } = true;

    /// <summary>
    /// Padding en pixels. Par défaut: 0.
    /// </summary>
    [Parameter]
    public int Padding { get; set; } = 0;

    /// <summary>
    /// Couleur de fond du body.
    /// </summary>
    [Parameter]
    public string? BackgroundColor { get; set; }

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

    internal string StyleString
    {
        get
        {
            var styles = new List<string>
            {
                $"padding: {Padding}px"
            };

            if (!string.IsNullOrEmpty(BackgroundColor))
            {
                styles.Add($"background-color: {BackgroundColor}");
            }

            if (!string.IsNullOrEmpty(Style))
            {
                styles.Add(Style);
            }

            return string.Join("; ", styles);
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
