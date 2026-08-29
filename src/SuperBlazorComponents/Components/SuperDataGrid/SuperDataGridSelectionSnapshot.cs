using System.Collections.Immutable;

namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Immutable selection state captured from a <see cref="SuperDataGrid{TItem}"/>.
/// </summary>
public sealed record SuperDataGridSelectionSnapshot<TItem>(
    ImmutableArray<TItem> SelectedItems,
    ImmutableHashSet<object?> SelectedItemKeys,
    bool AllSelected,
    ImmutableHashSet<object?> ExcludedItemKeys,
    int SelectedCountTotal)
{
    /// <summary>Gets whether at least one row belongs to the captured selection.</summary>
    public bool HasSelection =>
        SelectedCountTotal > 0
        || SelectedItemKeys.Any(key => !ExcludedItemKeys.Contains(key));
}
