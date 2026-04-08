namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Arguments pour la mise à jour de badge
/// </summary>
public class SuperTabBadgeUpdateEventArgs : EventArgs
{
    /// <summary>
    /// Identifiant de l'instance concernée
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID de l'onglet dont le badge doit être mis à jour
    /// </summary>
    public string? TabId { get; set; }
    
    /// <summary>
    /// Index de l'onglet dont le badge doit être mis à jour
    /// </summary>
    public int? Index { get; set; }
    
    /// <summary>
    /// Nouveau texte du badge
    /// </summary>
    public string? BadgeText { get; set; }
    
    /// <summary>
    /// Nouvelle classe CSS du badge
    /// </summary>
    public string? BadgeClass { get; set; }
    
    /// <summary>
    /// Nouvelle icône du badge
    /// </summary>
    public string? BadgeIcon { get; set; }
}
