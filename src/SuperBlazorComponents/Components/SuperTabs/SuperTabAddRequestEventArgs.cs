namespace SuperBlazorComponents.Components.SuperTabs;

/// <summary>
/// Arguments pour l'ajout d'onglet
/// </summary>
public class SuperTabAddRequestEventArgs : EventArgs
{
    public string InstanceId { get; set; } = string.Empty;
    public SuperTabItem Tab { get; set; } = new();
    public bool SelectAfterAdd { get; set; } = true;
}
