using System.Collections.Immutable;

namespace SuperBlazorComponents.Components.SuperDataGrid;

public partial class SuperDataGrid<TItem>
{
    private long _rangeQueryVersion;
    private bool _rangeSelectionBusy;
    private bool _rangeDisposed;

    /// <summary>
    /// Replaces the checkbox selection with an inclusive, one-based range in the
    /// current sorted and filtered query. Hierarchical ranges refer to root rows.
    /// Virtualized providers must expose stable item keys.
    /// </summary>
    public async Task SelectRangeAsync(int fromRow, int toRow, CancellationToken cancellationToken = default)
    {
        if (SelectionMode != SuperDataGridSelectionMode.Multiple)
        {
            throw new InvalidOperationException("Range selection requires multiple selection mode.");
        }
        if (fromRow < 1 || toRow < fromRow || toRow > TotalRowCount)
        {
            throw new ArgumentOutOfRangeException(nameof(fromRow), "The inclusive range must be within the current query.");
        }
        if (ItemsProvider is null || _rangeSelectionBusy || _rangeDisposed)
        {
            throw new InvalidOperationException("Range selection is unavailable.");
        }

        _rangeSelectionBusy = true;
        var version = _rangeQueryVersion;
        var query = CaptureQuerySnapshot();
        var provider = ItemsProvider;
        var total = TotalRowCount;
        var prepared = new SelectionInfo<TItem>();
        try
        {
            for (var offset = fromRow - 1; offset < toRow;)
            {
                EnsureRangeQueryUnchanged();
                var count = Math.Min(200, toRow - offset);
                var request = new GridItemsProviderRequest<TItem>(offset, count,
                    query.SortColumn, query.SortDirection,
                    query.Filters.Select(filter => filter.ToFilterInfo()).ToArray(), cancellationToken);
                var result = await provider(request);
                EnsureRangeQueryUnchanged();
                var items = result.Items.Take(count + 1).ToList();
                if (result.TotalItemCount != total || items.Count == 0 || items.Count > count)
                {
                    throw new InvalidOperationException("The provider returned an inconsistent range. Retry the selection.");
                }
                foreach (var item in items)
                {
                    if (!IsRowDeleted(item))
                    {
                        prepared.AddSelected(item);
                    }
                }
                offset += items.Count;
            }

            EnsureRangeQueryUnchanged();
            foreach (var item in _selectionInfo.SelectedItems)
            {
                SetItemSelected(item, false);
            }
            _selectionInfo.ClearSelected();
            _selectionInfo.AllSelected = false;
            _selectionInfo.UnselectedItemKeys.Clear();
            CurrentItem = default;
            foreach (var item in prepared.SelectionOrder)
            {
                _selectionInfo.AddSelected(item);
                SetItemSelected(item, true);
            }
            SyncRenderedItemsSelectionState();
            if (IsHierarchicalRenderingEnabled())
            {
                foreach (var item in _hierarchicalRootItems.SelectMany(GetHierarchyRows).Select(row => row.Item))
                {
                    SetItemSelected(item, IsRowSelected(item));
                }
            }
            await NotifySelectionChangedAsync(default);
            StateHasChanged();
        }
        finally
        {
            _rangeSelectionBusy = false;
        }

        void EnsureRangeQueryUnchanged()
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_rangeDisposed || version != _rangeQueryVersion || provider != ItemsProvider
                || SelectionMode != SuperDataGridSelectionMode.Multiple || total != TotalRowCount
                || !HasSameRangeQuery(query, CaptureQuerySnapshot()))
            {
                throw new InvalidOperationException("The grid query changed. Retry the selection.");
            }
        }
    }

    private static bool HasSameRangeQuery(SuperDataGridQuerySnapshot left, SuperDataGridQuerySnapshot right)
    {
        if (left.SortColumn != right.SortColumn || left.SortDirection != right.SortDirection
            || left.Filters.Length != right.Filters.Length)
        {
            return false;
        }
        for (var i = 0; i < left.Filters.Length; i++)
        {
            var a = left.Filters[i];
            var b = right.Filters[i];
            if ((a with { SelectedValues = ImmutableArray<string>.Empty })
                != (b with { SelectedValues = ImmutableArray<string>.Empty })
                || !a.SelectedValues.SequenceEqual(b.SelectedValues))
            {
                return false;
            }
        }
        return true;
    }
}
