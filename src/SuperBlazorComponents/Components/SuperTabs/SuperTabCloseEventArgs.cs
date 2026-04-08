namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Arguments pour l'événement de fermeture d'onglet
/// </summary>
public class SuperTabCloseEventArgs
{
    /// <summary>
    /// Onglet à fermer
    /// </summary>
    public SuperTabItem Tab { get; set; } = default!;

    /// <summary>
    /// Index de l'onglet
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Permet d'annuler la fermeture
    /// </summary>
    public bool Cancel { get; set; }
}
