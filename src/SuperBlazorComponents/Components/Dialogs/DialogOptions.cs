namespace SuperBlazorComponents.Components.Dialogs;

/// <summary>
/// Options de configuration pour les boîtes de dialogue modales.
/// </summary>
public class DialogOptions
{
    /// <summary>
    /// Largeur de la modale (ex: "500px", "80%").
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Hauteur de la modale (ex: "400px", "auto").
    /// </summary>
    public string? Height { get; set; }

    /// <summary>
    /// Indique si la modale peut être fermée en cliquant sur le fond.
    /// </summary>
    public bool CloseOnBackdropClick { get; set; } = false;

    /// <summary>
    /// Indique si la modale affiche un bouton de fermeture.
    /// </summary>
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Classe CSS supplémentaire pour la modale.
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Taille de la modale Bootstrap (sm, lg, xl).
    /// </summary>
    public DialogSize Size { get; set; } = DialogSize.Default;
}

/// <summary>
/// Tailles disponibles pour les modales Bootstrap.
/// </summary>
public enum DialogSize
{
    /// <summary>
    /// Taille par défaut.
    /// </summary>
    Default,

    /// <summary>
    /// Petite modale (modal-sm).
    /// </summary>
    Small,

    /// <summary>
    /// Grande modale (modal-lg).
    /// </summary>
    Large,

    /// <summary>
    /// Très grande modale (modal-xl).
    /// </summary>
    ExtraLarge
}
