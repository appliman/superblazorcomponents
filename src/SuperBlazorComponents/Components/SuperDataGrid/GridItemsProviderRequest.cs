namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Represents a request for items from a <see cref="GridItemsProvider{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The type of data items.</typeparam>
public readonly record struct GridItemsProviderRequest<TItem>(
    int StartIndex,
    int? Count,
    string? SortColumn,
    SortDirection SortDirection,
    IEnumerable<SuperDataGridFilterInfo> Filters,
    CancellationToken CancellationToken
);
