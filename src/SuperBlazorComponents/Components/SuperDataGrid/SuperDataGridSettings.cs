namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Represents all settings for a data grid instance.
/// Each property mirrors an identically named and typed <c>[Parameter]</c> of <see cref="SuperDataGrid{TItem}"/>.
/// Properties that depend on the generic type parameter <c>TItem</c> are intentionally omitted.
/// </summary>
public sealed record SuperDataGridSettings
{
	public string Name { get; set; } = null!;

	// --- Dimensions ---
	public float RowHeight { get; set; } = 40f;
	public int OverscanCount { get; set; } = 5;

	// --- Freeze ---
	public bool FreezeHeader { get; set; } = true;
	public bool FreezeFooter { get; set; } = true;

	// --- Features ---
	public bool AllowColumnReorder { get; set; } = true;
	public bool AllowColumnResize { get; set; } = true;
	public bool AllowSorting { get; set; } = true;
	public bool AllowFiltering { get; set; } = true;

	// --- Edition ---
	public SuperDataGridEditionMode EditionMode { get; set; } = SuperDataGridEditionMode.None;
	public bool EditOnDoubleClick { get; set; } = true;

	// --- Display toggles ---
	public bool DisplayRowNumberColumn { get; set; } = true;
	public bool DisplayRefreshButton { get; set; } = false;
	public bool DisplayColumnVisibilityToggle { get; set; } = true;
	public bool DisplayFooter { get; set; } = true;
	public bool DisplayDefaultFooterTemplate { get; set; } = true;

	// --- Appearance ---
	public string CurrentRowBackground { get; set; } = "#3b95c6";
	public string? ContainerCssClass { get; set; }
	public string TableCssClass { get; set; } = "table-striped table-hover table-bordered";
	public string HeaderCssClass { get; set; } = "";

	// --- Selection ---
	public SuperDataGridSelectionMode SelectionMode { get; set; } = SuperDataGridSelectionMode.Multiple;
	public bool DisplaySelectionColumn { get; set; } = true;
}
