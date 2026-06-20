using SuperBlazorComponents.Components;
using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.Components.SuperDataGrid.Filters;
using SuperBlazorComponents.Localization;

namespace SuperBlazorComponents.Configuration;

public class SuperComponentsConfiguration
{
	public DataGridSettingsStorageMode DataGridSettingsStorageMode { get; set; } = DataGridSettingsStorageMode.LocalStorage;
	public SuperIconStyle DefaultSuperIconeStyle { get; set; } = SuperIconStyle.Solid;
	public List<SuperDataGridSettings> SuperDataGridSettingsList { get; set; } = new();
	public List<SuperDataGridFilterComponent>  SuperDataGridFilterComponentList { get; set; } = new();
	public SuperContextConfiguration Contextualization { get; } = new();

	/// <summary>
	/// Gets the localization options for configuring built-in and external culture sources.
	/// </summary>
	public SuperLocalizationOptions Localization { get; } = new();
}
