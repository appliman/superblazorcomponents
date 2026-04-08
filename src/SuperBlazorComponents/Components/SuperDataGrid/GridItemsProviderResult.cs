namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Represents the result of a <see cref="GridItemsProvider{TItem}"/> request.
/// </summary>
/// <typeparam name="TItem">The type of data items.</typeparam>
public readonly record struct GridItemsProviderResult<TItem>(
    IEnumerable<TItem> Items,
    int TotalItemCount
)
{
    /// <summary>
    /// Creates a result from an enumerable with a known total count.
    /// </summary>
    public static GridItemsProviderResult<TItem> From(IEnumerable<TItem> items, int totalItemCount)
        => new(items, totalItemCount);

    public static GridItemsProviderResult<TItem> Empty() 
        => new(Array.Empty<TItem>(), 0);
}
