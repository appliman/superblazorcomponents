using System.ComponentModel;

using ModelContextProtocol.Server;

namespace DemoWebSite.Mcp;

[McpServerToolType]
public sealed class SuperComponentMcpTools(SuperComponentGuideCatalog catalog)
{
	[McpServerTool]
	[Description("Lists every SuperBlazorComponents component guide exposed by this demo site's MCP server.")]
	public string list_super_components()
	{
		return catalog.RenderIndex();
	}

	[McpServerTool]
	[Description("Gets setup guidance for one SuperBlazorComponents component by key or alias, for example 'super-data-grid', 'SuperButton', or 'ThemeToggle'.")]
	public string get_super_component_guide(
		[Description("Component key or alias. Use list_super_components to discover available keys.")]
		string component)
	{
		return catalog.RenderGuide(component);
	}

	[McpServerTool]
	[Description("Gets setup guidance for SuperDataGrid, including ItemsProvider and DataGridColumn usage.")]
	public string get_super_data_grid_guide() => catalog.RenderGuide("super-data-grid");

	[McpServerTool]
	[Description("Gets setup guidance for the SuperButtons family: SuperButton, SuperLinkButton, SuperSplitButton, SuperConfirmationButton, and SuperToggleButton.")]
	public string get_super_buttons_guide() => catalog.RenderGuide("super-buttons");

	[McpServerTool]
	[Description("Gets setup guidance for SuperBreadCrumb and breadcrumb items.")]
	public string get_super_breadcrumb_guide() => catalog.RenderGuide("super-breadcrumb");

	[McpServerTool]
	[Description("Gets setup guidance for SuperDateRangePicker and SuperDateRangeDialog.")]
	public string get_super_date_range_picker_guide() => catalog.RenderGuide("super-date-range-picker");

	[McpServerTool]
	[Description("Gets setup guidance for SuperDialog, SuperConfirmDialog, and dialog services.")]
	public string get_super_dialogs_guide() => catalog.RenderGuide("super-dialogs");

	[McpServerTool]
	[Description("Gets setup guidance for SuperLayout, SuperHeader, SuperSidebar, SuperBody, SuperFooter, and SuperChat.")]
	public string get_super_layout_guide() => catalog.RenderGuide("super-layout");

	[McpServerTool]
	[Description("Gets setup guidance for SuperMenuItem navigation entries.")]
	public string get_super_menu_item_guide() => catalog.RenderGuide("super-menu-item");

	[McpServerTool]
	[Description("Gets setup guidance for SuperNotification and SuperNotificationService.")]
	public string get_super_notifications_guide() => catalog.RenderGuide("super-notifications");

	[McpServerTool]
	[Description("Gets setup guidance for SuperTabs and TabItem.")]
	public string get_super_tabs_guide() => catalog.RenderGuide("super-tabs");

	[McpServerTool]
	[Description("Gets setup guidance for SuperDropDown and SuperSwitch form helpers.")]
	public string get_super_forms_guide() => catalog.RenderGuide("super-forms");

	[McpServerTool]
	[Description("Gets setup guidance for SuperSplitter and SplitPane.")]
	public string get_super_splitter_guide() => catalog.RenderGuide("super-splitter");

	[McpServerTool]
	[Description("Gets setup guidance for SuperTooltip.")]
	public string get_super_tooltip_guide() => catalog.RenderGuide("super-tooltip");

	[McpServerTool]
	[Description("Gets setup guidance for Google chart components included in SuperBlazorComponents.")]
	public string get_google_charts_guide() => catalog.RenderGuide("google-charts");

	[McpServerTool]
	[Description("Gets setup guidance for ThemeToggle.")]
	public string get_theme_toggle_guide() => catalog.RenderGuide("theme-toggle");

	[McpServerTool]
	[Description("Gets setup guidance for SuperIcon.")]
	public string get_super_icons_guide() => catalog.RenderGuide("super-icons");

	[McpServerTool]
	[Description("Gets setup guidance for SuperValidationSummary and SuperValidationMessage.")]
	public string get_super_validations_guide() => catalog.RenderGuide("super-validations");
}
