namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Arguments pour la demande de sélection d'onglet
/// </summary>
public class SuperTabSelectRequestEventArgs : EventArgs
{
    /// <summary>
    /// Identifiant de l'instance (null pour toutes les instances)
    /// </summary>
    public string? InstanceId { get; set; }
    
    /// <summary>
    /// Index de l'onglet à sélectionner
    /// </summary>
    public int? Index { get; set; }
    
    /// <summary>
    /// Titre de l'onglet à sélectionner
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// ID de l'onglet à sélectionner
    /// </summary>
    public string? TabId { get; set; }
}
