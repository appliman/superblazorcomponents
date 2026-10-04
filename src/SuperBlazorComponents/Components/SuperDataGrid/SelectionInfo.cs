namespace SuperBlazorComponents.Components.SuperDataGrid;

public sealed class SelectionInfo<TItem>
{
    public HashSet<TItem> SelectedItems { get; } = [];

    // HashSet is kept for fast membership checks and backwards compatibility. The
    // list preserves the order in which individual rows were checked for exports.
    internal List<TItem> SelectionOrder { get; } = [];

    private readonly Dictionary<object, TItem> _selectedByKey = [];

    private static object SelectionKey(TItem item)
    {
        return (item is IDataItem dataItem
            ? dataItem.KeyValue
            : typeof(TItem).GetProperty(nameof(IDataItem.KeyValue))?.GetValue(item)) ?? item!;
    }

    internal bool ContainsSelected(TItem item)
    {
        return SelectedItems.Contains(item) || (_selectedByKey.TryGetValue(SelectionKey(item), out var selected) && SelectedItems.Contains(selected));
    }

    internal HashSet<object?> UnselectedItemKeys { get; } = [];

    public int TotalCount { get; set; }

    public int SelectedCount { get; set; }

    public bool AllSelected { get; set; }

    public int ExcludedCount { get; set; }

    public int SelectedCountTotal => Math.Max(0, SelectedCount - ExcludedCount);

    internal TItem GetSelectedInstance(TItem item)
    {
        return _selectedByKey.TryGetValue(SelectionKey(item), out var selected) ? selected : item;
    }

    internal void AddSelected(TItem item)
    {
        if (ContainsSelected(item))
        {
            return;
        }
        if (SelectedItems.Add(item))
        {
            _selectedByKey[SelectionKey(item)] = item;
            SelectionOrder.Add(item);
        }
    }

    internal bool RemoveSelected(TItem item)
    {
        var key = SelectionKey(item);
        var storedItem = _selectedByKey.Remove(key, out var selectedItem) ? selectedItem : item;
        var removed = SelectedItems.Remove(storedItem);
        if (removed)
        {
            SelectionOrder.Remove(storedItem);
        }
        return removed;
    }

    internal void ClearSelected()
    {
        SelectedItems.Clear();
        SelectionOrder.Clear();
        _selectedByKey.Clear();
    }
}
