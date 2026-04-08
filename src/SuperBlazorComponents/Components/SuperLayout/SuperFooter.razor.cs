namespace SuperBlazorComponents.Components.SuperLayout;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Composant Footer sticky responsive.
/// </summary>
public partial class SuperFooter : ComponentBase
{
    /// <summary>
    /// Référence au SuperLayout parent.
    /// </summary>
    [CascadingParameter]
    public SuperLayout? ParentLayout { get; set; }

    /// <summary>
    /// Contenu du footer.
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
    /// Indique si le footer est sticky (fixé en bas). Par défaut: true.
    /// </summary>
    [Parameter]
    public bool Sticky { get; set; } = true;

    /// <summary>
    /// Utilise un container fluid (100% largeur). Par défaut: true.
    /// </summary>
    [Parameter]
    public bool Fluid { get; set; } = true;

    /// <summary>
    /// Hauteur du footer en pixels.
    /// </summary>
    [Parameter]
    public int Height { get; set; } = 48;

    /// <summary>
    /// Couleur de fond du footer.
    /// </summary>
    [Parameter]
    public string? BackgroundColor { get; set; }

    internal string StickyClass => Sticky ? "super-footer-sticky" : "";

    internal string StyleString
    {
        get
        {
            var styles = new List<string>
            {
                $"min-height: {Height}px"
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
}
