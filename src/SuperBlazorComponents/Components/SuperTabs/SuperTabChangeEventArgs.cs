namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Arguments pour l'événement de changement d'onglet
/// </summary>
public class SuperTabChangeEventArgs
{
    /// <summary>
    /// Index de l'onglet précédent
    /// </summary>
    public int PreviousIndex { get; set; }

    /// <summary>
    /// Index du nouvel onglet
    /// </summary>
    public int NewIndex { get; set; }

    /// <summary>
    /// Onglet précédent
    /// </summary>
    public SuperTabItem? PreviousTab { get; set; }

    /// <summary>
    /// Nouvel onglet
    /// </summary>
    public SuperTabItem? NewTab { get; set; }

    /// <summary>
    /// Permet d'annuler le changement d'onglet
    /// </summary>
    public bool Cancel { get; set; }
}
