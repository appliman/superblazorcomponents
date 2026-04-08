namespace SuperBlazorComponents.Components.SuperDataGrid;

public sealed class SuperDataGridDataLoadedEventArgs<TItem>
{
    public SuperDataGridDataLoadedEventArgs(IReadOnlyList<TItem> items, int totalItemCount, int startIndex, int requestedCount)
    {
        Items = items;
        TotalItemCount = totalItemCount;
        StartIndex = startIndex;
        RequestedCount = requestedCount;
    }

    public IReadOnlyList<TItem> Items { get; }

    public int TotalItemCount { get; }

    public int StartIndex { get; }

    public int RequestedCount { get; }
}
