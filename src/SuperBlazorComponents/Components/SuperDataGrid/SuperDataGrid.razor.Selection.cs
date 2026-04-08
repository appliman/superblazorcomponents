using Microsoft.AspNetCore.Components;

namespace SuperBlazorComponents.Components.SuperDataGrid;

public partial class SuperDataGrid<TItem>
{
	private SelectionInfo<TItem> _selectionInfo = new();
	private readonly List<Tools.SuperDataGridRowSelectorItem> _selectorMenuItems = [];

	/// <summary>
	/// Defines the row selection behavior.
	/// </summary>
	[Parameter]
	public SuperDataGridSelectionMode SelectionMode { get; set; } = SuperDataGridSelectionMode.Multiple;

	[Parameter]
	public bool DisplaySelectionColumn { get; set; } = true;

	/// <summary>
	/// Callback invoked when the selection changes.
	/// </summary>
	[Parameter]
	public EventCallback<IEnumerable<TItem>> SelectionChanged { get; set; }

	public event EventHandler<SelectionChangedEventArgs<TItem>> SelectedRowsChanged = default!;

	[Parameter]
	public EventCallback<SelectionChangedEventArgs<TItem>> SelectionStateChanged { get; set; }

	[Parameter]
	public RenderFragment? SelectorMenuItemsContent { get; set; }

	[Parameter]
	public IEnumerable<Tools.SuperDataGridRowSelectorItem>? SelectorMenuItems { get; set; }

	internal IEnumerable<Tools.SuperDataGridRowSelectorItem> SelectorMenuItemsSource
	{
		get
		{
			if (_selectorMenuItems.Count == 0)
			{
				return SelectorMenuItems ?? [];
			}

			if (SelectorMenuItems is null)
			{
				return _selectorMenuItems;
			}

			return SelectorMenuItems.Concat(_selectorMenuItems);
		}
	}

	[Parameter]
    public EventCallback<Tools.SelectedActionInfo<TItem>> SelectorMenuItemSelected { get; set; }

	public event Action<Tools.SelectedActionInfo<TItem>> SelectorMenuItemClicked = default!;

	public IReadOnlyCollection<TItem> SelectedItems => _selectionInfo.SelectedItems;

	public int SelectedCountTotal => _selectionInfo.SelectedCountTotal;

	/// <summary>
	/// Adds one item to the row selector menu at runtime.
	/// </summary>
	/// <param name="item">The row selector item to add.</param>
	public Task AddSelectorMenuItemAsync(Tools.SuperDataGridRowSelectorItem item)
	{
		ArgumentNullException.ThrowIfNull(item);
		ArgumentException.ThrowIfNullOrWhiteSpace(item.ActionName);

		_selectorMenuItems.Add(item);

		return InvokeAsync(StateHasChanged);
	}

	/// <summary>
	/// Adds multiple items to the row selector menu at runtime.
	/// </summary>
	/// <param name="items">The row selector items to add.</param>
	public Task AddSelectorMenuItemsAsync(IEnumerable<Tools.SuperDataGridRowSelectorItem> items)
	{
		ArgumentNullException.ThrowIfNull(items);

		foreach (var item in items)
		{
			ArgumentNullException.ThrowIfNull(item);
			ArgumentException.ThrowIfNullOrWhiteSpace(item.ActionName);
			_selectorMenuItems.Add(item);
		}

		return InvokeAsync(StateHasChanged);
	}

	/// <summary>
	/// Clears menu items that were added at runtime.
	/// </summary>
	public Task ClearSelectorMenuItemsAsync()
	{
		_selectorMenuItems.Clear();
		return InvokeAsync(StateHasChanged);
	}

	/// <summary>
	/// Selects a specific item.
	/// </summary>
	public async Task SelectItemAsync(TItem item)
	{
		if (SelectionMode != SuperDataGridSelectionMode.Multiple)
		{
			_selectionInfo.SelectedItems.Clear();
		}

		_selectionInfo.SelectedItems.Add(item);

		if (_selectionInfo.AllSelected)
		{
			_selectionInfo.UnselectedItemKeys.Remove(TryGetItemKey(item));
		}

		SetItemSelected(item, false);

		await NotifySelectionChangedAsync(item);
		StateHasChanged();
	}

	/// <summary>
	/// Selects a specific row (item) in the grid.
	/// </summary>
	/// <param name="item">The item to select.</param>
	/// <param name="clearOthers">If true, clears other selections (default: true for single selection mode).</param>
	public async Task SelectRow(TItem item, bool clearOthers = true)
	{
		if (item is null)
		{
			return;
		}

		if (clearOthers || SelectionMode != SuperDataGridSelectionMode.Multiple)
		{
			foreach (var selectedItem in _selectionInfo.SelectedItems)
			{
				SetItemSelected(selectedItem, false);
			}

			_selectionInfo.SelectedItems.Clear();
		}

		if (!_selectionInfo.SelectedItems.Contains(item))
		{
			_selectionInfo.SelectedItems.Add(item);
			SetItemSelected(item, true);
		}

		if (_selectionInfo.AllSelected)
		{
			_selectionInfo.UnselectedItemKeys.Remove(TryGetItemKey(item));
		}

		await NotifySelectionChangedAsync(item);
		StateHasChanged();
	}

	/// <summary>
	/// Tries to select the first item from the currently rendered list.
	/// </summary>
	/// <returns><c>true</c> if the first row was selected; otherwise <c>false</c>.</returns>
	public async Task<bool> TrySelectFirstRow()
	{
		var firstItem = _renderedItems.FirstOrDefault();
		if (firstItem is null)
		{
			return false;
		}

		_currentRowKey = TryGetItemKey(firstItem);
		await InvokeAsync(StateHasChanged);

		return true;
	}

	/// <summary>
	/// Sets the current row highlight without changing checkbox selection state.
	/// </summary>
	public Task SetCurrentRowAsync(TItem item)
	{
		if (item is null)
		{
			return Task.CompletedTask;
		}

		CurrentItem = item;
		_currentRowKey = TryGetItemKey(item);
		StateHasChanged();
		return Task.CompletedTask;
	}

	/// <summary>
	/// Clears the selection.
	/// </summary>
	public async Task ClearSelectionAsync()
	{
		foreach (var selectedItem in _selectionInfo.SelectedItems)
		{
			SetItemSelected(selectedItem, false);
		}

		CurrentItem = default;
		_selectionInfo.SelectedItems.Clear();
		_selectionInfo.UnselectedItemKeys.Clear();
		_selectionInfo.AllSelected = false;

		await NotifySelectionChangedAsync(default);
		StateHasChanged();
	}

	/// <summary>
	/// Selects all currently rendered rows.
	/// </summary>
	public async Task SelectAllRenderedAsync()
	{
		await SelectAllAsync();
	}

	/// <summary>
	/// Selects all rows in the grid.
	/// </summary>
	public async Task SelectAllAsync()
	{
		if (SelectionMode != SuperDataGridSelectionMode.Multiple)
		{
			return;
		}

		_selectionInfo.AllSelected = true;
		_selectionInfo.UnselectedItemKeys.Clear();

		foreach (var renderedItem in _renderedItems)
		{
			_selectionInfo.SelectedItems.Add(renderedItem);
			SetItemSelected(renderedItem, true);
		}

        await NotifySelectionChangedAsync(CurrentItem);
		StateHasChanged();
	}

	/// <summary>
	/// Returns the current selection summary.
	/// </summary>
	public SelectionInfo<TItem> GetSelectionInfo()
	{
        UpdateSelectionInfo();

		return _selectionInfo;
	}

	public void OnSelectorMenuItemClicked(Tools.SelectedActionInfo<TItem> actionInfo)
	{
		SelectorMenuItemClicked?.Invoke(actionInfo);
	}

	private bool IsAllSelected()
	{
		return _selectionInfo.AllSelected;
	}

	private bool IsRowSelected(TItem item)
	{
		if (_selectionInfo.AllSelected)
		{
			return !IsExcludedFromAllSelected(item);
		}

		return _selectionInfo.SelectedItems.Contains(item);
	}

	private async Task OnRowClick(TItem item)
	{
		_currentRowKey = TryGetItemKey(item);
		await RowClicked.InvokeAsync(item);

		StateHasChanged();
	}

	private async Task OnCellClick(TItem item, DataGridColumn<TItem> column)
	{
		_currentRowKey = TryGetItemKey(item);

		if (!CellClicked.HasDelegate)
		{
			StateHasChanged();
			return;
		}

		var value = GetCellValue(column, item);
		var args = new CellClickedEventArgs<TItem>(item, column.Property, value);
		await CellClicked.InvokeAsync(args);
		StateHasChanged();
	}

	private async Task ToggleSelectAllAsync(ChangeEventArgs args)
	{
		if (SelectionMode != SuperDataGridSelectionMode.Multiple)
		{
			return;
		}

		var isChecked = args.Value is bool value && value;
		_selectionInfo.AllSelected = isChecked;

		if (!isChecked)
		{
			await ClearSelectionAsync();
			return;
		}

		await SelectAllAsync();
	}

	private async Task OnSelectionCheckboxChangeAsync(TItem item, ChangeEventArgs args)
	{
		var isChecked = args.Value is bool value && value;
        CurrentItem = item;

		if (SelectionMode == SuperDataGridSelectionMode.Single)
		{
			foreach (var selectedItem in _selectionInfo.SelectedItems)
			{
				SetItemSelected(selectedItem, false);
			}

			_selectionInfo.SelectedItems.Clear();
			_selectionInfo.AllSelected = false;
			_selectionInfo.UnselectedItemKeys.Clear();
		}

		if (_selectionInfo.AllSelected)
		{
			var itemKey = TryGetItemKey(item);

			if (isChecked)
			{
				_selectionInfo.SelectedItems.Add(item);
				_selectionInfo.UnselectedItemKeys.Remove(itemKey);
				SetItemSelected(item, true);
			}
			else
			{
				_selectionInfo.SelectedItems.Remove(item);
				_selectionInfo.UnselectedItemKeys.Add(itemKey);
				SetItemSelected(item, false);
			}

			await NotifySelectionChangedAsync(item);
			StateHasChanged();
			return;
		}

		if (isChecked)
		{
			_selectionInfo.SelectedItems.Add(item);
			SetItemSelected(item, true);
		}
		else
		{
			_selectionInfo.SelectedItems.Remove(item);
			SetItemSelected(item, false);
			_selectionInfo.AllSelected = false;
		}

		await NotifySelectionChangedAsync(item);
		StateHasChanged();
	}

	private void SyncRenderedItemsSelectionState()
	{
		foreach (var renderedItem in _renderedItems)
		{
			if (_selectionInfo.AllSelected)
			{
				var isSelected = !IsExcludedFromAllSelected(renderedItem);
				SetItemSelected(renderedItem, isSelected);

				if (isSelected)
				{
					_selectionInfo.SelectedItems.Add(renderedItem);
				}
				else
				{
					_selectionInfo.SelectedItems.Remove(renderedItem);
				}
			}
			else
			{
				SetItemSelected(renderedItem, _selectionInfo.SelectedItems.Contains(renderedItem));
			}
		}
	}

	private async Task NotifySelectionChangedAsync(TItem? selectedItem)
	{
		await CurrentItemChanged.InvokeAsync(selectedItem);
		await SelectionChanged.InvokeAsync(_selectionInfo.SelectedItems);

       var selectionInfo = GetSelectionInfo();

		if (SelectionStateChanged.HasDelegate)
		{
			await SelectionStateChanged.InvokeAsync(new SelectionChangedEventArgs<TItem>(_selectionInfo.SelectedItems.ToList(), selectionInfo));
		}
		if (SelectedRowsChanged != null)
		{
			SelectedRowsChanged.Invoke(this, new SelectionChangedEventArgs<TItem>(_selectionInfo.SelectedItems.ToList(), selectionInfo));
		}
	}

    private void SetItemSelected(TItem item, bool isSelected)
	{
     ArgumentNullException.ThrowIfNull(item);

		var selectedProperty = typeof(TItem).GetProperty(nameof(IDataItem.IsSelected));
		if (selectedProperty?.CanWrite == true && selectedProperty.PropertyType == typeof(bool))
		{
			selectedProperty.SetValue(item, isSelected);
		}

		UpdateSelectionInfo();
	}

	private string GetSelectionColumnStyle()
	{
		if (DisplayRowNumberColumn)
		{
			return $"inset-inline-start: {ROW_NUMBER_WIDTH}px;";
		}

		return "inset-inline-start: 0;";
	}

	private string GetActionsColumnStyle()
	{
		var left = 0;
		if (DisplayRowNumberColumn)
		{
			left += ROW_NUMBER_WIDTH;
		}

		if (DisplaySelectionColumn)
		{
			left += SELECTION_WIDTH;
		}

		return $"inset-inline-start: {left}px;";
	}

	private static string GetRowNumberHeaderStyle()
	{
		return "font-size: 0.8rem; color: var(--bs-secondary-color, #adb5bd); font-weight: 400;";
	}

	private static string GetRowNumberCellStyle()
	{
		return "font-size: 1rem; color: var(--bs-secondary-color, #adb5bd); font-weight: 400; line-height: 1;";
	}

	private int GetSelectedCount()
	{
		if (_selectionInfo.AllSelected)
		{
			return Math.Max(0, _totalItemCount - _selectionInfo.UnselectedItemKeys.Count);
		}

		return _selectionInfo.SelectedItems.Count;
	}

	private void UpdateSelectionInfo()
	{
		_selectionInfo.TotalCount = _totalItemCount;
		_selectionInfo.SelectedCount = GetSelectedCount();
		_selectionInfo.ExcludedCount = GetExcludedCount();
	}

	private int GetExcludedCount()
	{
		if (!_selectionInfo.AllSelected)
		{
			return 0;
		}

		return _selectionInfo.UnselectedItemKeys.Count;
	}

	private bool IsExcludedFromAllSelected(TItem item)
	{
		return _selectionInfo.UnselectedItemKeys.Contains(TryGetItemKey(item));
	}
}
