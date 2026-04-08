using SuperBlazorComponents.Components;

namespace SuperBlazorComponents.Components.SuperDataGrid.Tools;

public class SuperDataGridRowSelectorItem
{
    public string ActionName { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public SuperIconStyle IconStyle { get; set; } = SuperIconStyle.Configuration;

    public bool Disabled { get; set; }
}
