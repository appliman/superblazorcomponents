namespace SuperBlazorComponents.Components.SuperDataGrid;

public sealed class SelectionInfo<TItem>
{
    public HashSet<TItem> SelectedItems { get; } = [];

    internal HashSet<object?> UnselectedItemKeys { get; } = [];

    public int TotalCount { get; set; }

    public int SelectedCount { get; set; }

    public bool AllSelected { get; set; }

    public int ExcludedCount { get; set; }

    public int SelectedCountTotal => SelectedCount - ExcludedCount;
}
