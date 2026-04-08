namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Arguments pour les événements du service
/// </summary>
public class SuperTabServiceEventArgs : EventArgs
{
    /// <summary>
    /// Identifiant de l'instance concernée
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Titre de l'onglet
    /// </summary>
    public string TabTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// Index de l'onglet
    /// </summary>
    public int TabIndex { get; set; }
}
