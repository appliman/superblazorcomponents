using SuperBlazorComponents.Components;
using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.Components.SuperDataGrid.Filters;

namespace SuperBlazorComponents.Configuration;

public class SuperComponentsConfiguration
{
	public DataGridSettingsStorageMode DataGridSettingsStorageMode { get; set; } = DataGridSettingsStorageMode.LocalStorage;
	public SuperIconStyle DefaultSuperIconeStyle { get; set; } = SuperIconStyle.Solid;
	public List<SuperDataGridSettings> SuperDataGridSettingsList { get; set; } = new();
	public List<SuperDataGridFilterComponent>  SuperDataGridFilterComponentList { get; set; } = new();
}
