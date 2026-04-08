namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Arguments pour la suppression d'onglet
/// </summary>
public class SuperTabRemoveRequestEventArgs : EventArgs
{
    /// <summary>
    /// Identifiant de l'instance concernée
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Index de l'onglet à supprimer
    /// </summary>
    public int? Index { get; set; }
    
    /// <summary>
    /// ID de l'onglet à supprimer
    /// </summary>
    public string? TabId { get; set; }
}
