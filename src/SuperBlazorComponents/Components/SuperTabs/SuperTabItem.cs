using Microsoft.AspNetCore.Components;
using SuperBlazorComponents.Components;

namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Représente un onglet dans le composant SuperTabs
/// </summary>
public class SuperTabItem
{
    /// <summary>
    /// Identifiant unique de l'onglet
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Titre affiché sur l'onglet
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Icône FontAwesome (ex: "fa-home")
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Style d'icône Font Awesome.
    /// </summary>
    public SuperIconStyle SuperIconStyle { get; set; } = SuperIconStyle.Configuration;

    /// <summary>
    /// Couleur de l'icône (ex: "#FF5733" ou "var(--bs-primary)")
    /// </summary>
    public string? IconColor { get; set; }

    /// <summary>
    /// Texte du badge (peut être un nombre ou du texte)
    /// </summary>
    public string? BadgeText { get; set; }

    /// <summary>
    /// Classe CSS du badge (ex: "badge-primary", "badge-danger")
    /// </summary>
    public string BadgeClass { get; set; } = "badge-primary";

    /// <summary>
    /// Icône FontAwesome pour le badge (optionnel)
    /// </summary>
    public string? BadgeIcon { get; set; }

    /// <summary>
    /// Indique si l'onglet est visible
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Indique si l'onglet est désactivé (grisé mais visible)
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Indique si le contenu doit être chargé en lazy loading
    /// </summary>
    public bool LazyLoad { get; set; }

    /// <summary>
    /// Indique si le contenu a déjà été chargé (pour le lazy loading)
    /// </summary>
    public bool HasBeenLoaded { get; set; }

    /// <summary>
    /// Indique si l'onglet peut être fermé
    /// </summary>
    public bool Closable { get; set; }

    /// <summary>
    /// Ordre d'affichage de l'onglet
    /// </summary>
    public int Order { get; set; } = int.MaxValue;

    /// <summary>
    /// Tooltip affiché au survol de l'onglet
    /// </summary>
    public string? Tooltip { get; set; }

    /// <summary>
    /// Type du composant à afficher dans l'onglet (pour le rendu dynamique)
    /// </summary>
    public Type? ComponentType { get; set; }

    /// <summary>
    /// Paramètres à passer au composant dynamique
    /// </summary>
    public Dictionary<string, object>? ComponentParameters { get; set; }

    /// <summary>
    /// Contenu personnalisé de l'onglet (RenderFragment)
    /// </summary>
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Données associées à l'onglet (pour un usage personnalisé)
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Clé pour la persistance de l'état (localStorage/URL)
    /// </summary>
    public string? PersistenceKey { get; set; }

    /// <summary>
    /// Indique si des modifications non sauvegardées existent
    /// </summary>
    public bool HasUnsavedChanges { get; set; }

    /// <summary>
    /// Message de confirmation avant de quitter l'onglet
    /// </summary>
    public string? ConfirmLeaveMessage { get; set; }

    /// <summary>
    /// Gets or sets the name of the policy associated with this instance.
    /// </summary>
	public string? PolicyName { get; set; }
}
