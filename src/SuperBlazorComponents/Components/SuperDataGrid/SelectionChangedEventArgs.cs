namespace SuperBlazorComponents.Components.SuperDataGrid;

public sealed class SelectionChangedEventArgs<TItem> : EventArgs
{
    public SelectionChangedEventArgs(IReadOnlyCollection<TItem> selectedItems, SelectionInfo<TItem> selectionInfo)
    {
        SelectedItems = selectedItems;
        SelectionInfo = selectionInfo;
    }

    public IReadOnlyCollection<TItem> SelectedItems { get; }

    public SelectionInfo<TItem> SelectionInfo { get; }
}
