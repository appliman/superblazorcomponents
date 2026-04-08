namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// A delegate that provides items for the grid based on a request.
/// </summary>
/// <typeparam name="TItem">The type of data items.</typeparam>
/// <param name="request">The request containing pagination and sorting information.</param>
/// <returns>A task that resolves to the requested items and total count.</returns>
public delegate ValueTask<GridItemsProviderResult<TItem>> GridItemsProvider<TItem>(
    GridItemsProviderRequest<TItem> request
);
