using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBlazorComponents.Components.SuperDataGrid.Tools;

public class SelectedActionInfo<TItem>
{
	public string ActionName { get; set; } = null!;
	public SelectionInfo<TItem> DataGridSelectionInfo { get; set; } = default!;

}
