namespace SuperBlazorComponents.Components.SuperDataGrid;

public sealed class SelectionInfo<TItem>
{
    public HashSet<TItem> SelectedItems { get; } = [];

    // HashSet is kept for fast membership checks and backwards compatibility. The
    // list preserves the order in which individual rows were checked for exports.
    internal List<TItem> SelectionOrder { get; } = [];

    internal HashSet<object?> UnselectedItemKeys { get; } = [];

    public int TotalCount { get; set; }

    public int SelectedCount { get; set; }

    public bool AllSelected { get; set; }

    public int ExcludedCount { get; set; }

    public int SelectedCountTotal => Math.Max(0, SelectedCount - ExcludedCount);

    internal void AddSelected(TItem item)
    {
        if (SelectedItems.Add(item))
            SelectionOrder.Add(item);
    }

    internal bool RemoveSelected(TItem item)
    {
        var removed = SelectedItems.Remove(item);
        if (removed)
            SelectionOrder.Remove(item);
        return removed;
    }

    internal void ClearSelected()
    {
        SelectedItems.Clear();
        SelectionOrder.Clear();
    }
}
