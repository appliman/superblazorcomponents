namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Représente une instance enregistrée de SuperTabs
/// </summary>
public class SuperTabsInstance
{
    /// <summary>
    /// Liste des onglets de l'instance
    /// </summary>
    public List<SuperTabItem> Tabs { get; set; } = new();
    
    /// <summary>
    /// Index de l'onglet actuellement sélectionné
    /// </summary>
    public int SelectedIndex { get; set; }
    
    /// <summary>
    /// Callback pour rafraîchir l'interface utilisateur
    /// </summary>
    public Action? RefreshCallback { get; set; }
}
