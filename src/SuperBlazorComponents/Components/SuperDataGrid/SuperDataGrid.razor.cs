using SuperBlazorComponents.Configuration;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// A high-performance virtual data grid component with frozen columns, headers, and footers.
/// Supports column reordering, resizing, and persistence of settings.
/// </summary>
/// <typeparam name="TItem">The type of data items displayed in the grid.</typeparam>
public partial class SuperDataGrid<TItem> : IAsyncDisposable
{
	internal static event Action<SuperDataGrid<TItem>>? GridInstanceReady;
	public event EventHandler? ColumnStateChanged;

	private const int ROW_NUMBER_WIDTH = 50;
	private const int SELECTION_WIDTH = 40;
	private const int ACTIONS_WIDTH = 50;

	private ElementReference _containerRef;
	private ElementReference _tableRef;
	private Virtualize<TItem>? _virtualizeRef;
	private IJSObjectReference? _jsModule;
	private DotNetObjectReference<SuperDataGrid<TItem>>? _dotNetRef;
	private bool _isLoading;
	private bool _defaultSettingsApplied;
	private List<DataGridColumn<TItem>> _columns = [];
	private DataGridColumn<TItem>? _draggedColumn;
	private string? _sortColumn;
	private SortDirection _sortDirection = SortDirection.None;
	private bool _settingsLoaded;
	private List<SuperDataGridColumnSettings>? _loadedColumnSettings;
	private int _totalItemCount;
	private List<TItem> _renderedItems = [];
	private List<TItem> _hierarchicalRootItems = [];
	private bool _hierarchicalRootItemsLoaded;
	private Dictionary<object, int> _rowNumberLookup = [];
	private readonly Dictionary<object, HierarchyRowState> _hierarchyState = [];
	private object? _currentRowKey;
	private readonly HashSet<object> _editingItemKeys = [];

	// Cache pour les styles de colonnes
	private Dictionary<string, string> _columnStyleCache = [];
	private List<DataGridColumn<TItem>>? _cachedVisibleColumns;
	private int _cacheVersion;

	// Cache pour les items avec clé basée sur les paramètres de la requête
	// (désactivé) - Virtualize annule et relance fréquemment les requêtes; un cache côté composant
	// peut renvoyer des pages incohérentes et provoquer des boucles.

	[Inject]
	private IJSRuntime JSRuntime { get; set; } = default!;

	[Inject]
	private ISuperDataGridSettingsStorage? SettingsStorage { get; set; }

	[Inject]
	ILogger<SuperDataGrid<TItem>> Logger { get; set; } = default!;

	[Inject]
	private SuperComponentsConfiguration? SuperConfiguration { get; set; }

	/// <summary>
	/// The content of the grid, typically DataGridColumn components.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	/// <summary>
	/// Template displayed in the header section above the table.
	/// </summary>
	[Parameter]
	public RenderFragment? HeaderTemplate { get; set; }

	/// <summary>
	/// Template displayed in the footer section below the table.
	/// </summary>
	[Parameter]
	public RenderFragment? FooterTemplate { get; set; }

	/// <summary>
	/// Template displayed while loading data.
	/// </summary>
	[Parameter]
	public RenderFragment? LoadingTemplate { get; set; }

	/// <summary>
	/// Template displayed when no data is available.
	/// </summary>
	[Parameter]
	public RenderFragment? EmptyTemplate { get; set; }

	/// <summary>
	/// Template rendered for each item to display action buttons (edit, save, cancel…).
	/// In vertical orientation, it is stacked below the row number inside the row-number cell.
	/// In horizontal orientation, it is rendered as a dedicated column after the row-number and selection columns.
	/// </summary>
	[Parameter]
	public RenderFragment<TItem>? ActionsTemplate { get; set; }

	/// <summary>
	/// Width in pixels of the actions column when <see cref="ActionsTemplate"/> is set.
	/// </summary>
	[Parameter]
	public int ActionsWidth { get; set; } = ACTIONS_WIDTH;

	/// <summary>
	/// A callback that provides data items for virtualization.
	/// This is the preferred way to provide data for large datasets.
	/// Similar to QuickGrid's ItemsProvider pattern.
	/// </summary>
	/// <remarks>
	/// The callback receives a <see cref="GridItemsProviderRequest{TItem}"/> and should return
	/// a <see cref="GridItemsProviderResult{TItem}"/> containing the requested items and total count.
	/// </remarks>
	[Parameter]
	public GridItemsProvider<TItem> ItemsProvider { get; set; } = default!;

	/// <summary>
	/// Callback invoked when items are loaded through <see cref="ItemsProvider"/>.
	/// </summary>
	[Parameter]
	public EventCallback<SuperDataGridDataLoadedEventArgs<TItem>> DataLoaded { get; set; }

	/// <summary>
	/// Returns the items currently rendered in the grid.
	/// Returns <c>null</c> when no data has been loaded yet.
	/// </summary>
	public IEnumerable<TItem>? Items => _renderedItems.Count > 0 ? _renderedItems : null;

	/// <summary>
	/// Unique identifier for persisting grid settings.
	/// </summary>
	[Parameter]
	public string? GridId { get; set; }

	/// <summary>
	/// Height of the grid container (e.g., "400px", "100%").
	/// </summary>
	[Parameter]
	public string Height { get; set; } = "400px";

	/// <summary>
	/// Estimated height of each row in pixels for virtualization.
	/// </summary>
	[Parameter]
	public float RowHeight { get; set; } = 40f;

	/// <summary>
	/// Keeps body rows at <see cref="RowHeight"/> and makes overflowing cell content vertically scrollable.
	/// </summary>
	[Parameter]
	public bool FixedRowHeight { get; set; } = true;

	/// <summary>
	/// Number of items to render outside the visible area.
	/// </summary>
	[Parameter]
	public int OverscanCount { get; set; } = 5;

	/// <summary>
	/// Whether to freeze the header row.
	/// </summary]
	[Parameter]
	public bool FreezeHeader { get; set; } = true;

	/// <summary>
	/// Whether to freeze the footer row.
	/// </summary>
	[Parameter]
	public bool FreezeFooter { get; set; } = true;

	/// <summary>
	/// Number of columns to freeze on the left.
	/// </summary>
	[Parameter]
	public int FreezeLeftColumns { get; set; }

	/// <summary>
	/// Number of columns to freeze on the right.
	/// </summary>
	[Parameter]
	public int FreezeRightColumns { get; set; }

	/// <summary>
	/// Whether columns can be reordered by drag and drop.
	/// </summary>
	[Parameter]
	public bool AllowColumnReorder { get; set; } = true;

	/// <summary>
	/// Whether columns can be resized.
	/// </summary>
	[Parameter]
	public bool AllowColumnResize { get; set; } = true;

	/// <summary>
	/// Whether sorting is enabled.
	/// </summary>
	[Parameter]
	public bool AllowSorting { get; set; } = true;

	/// <summary>
	/// Whether filter controls are displayed in the table header.
	/// </summary>
	[Parameter]
	public bool AllowFiltering { get; set; } = true;

	/// <summary>
	/// Indicates whether the grid renders cells in display mode or edit mode.
	/// </summary>
	[Parameter]
	public SuperDataGridEditionMode EditionMode { get; set; } = SuperDataGridEditionMode.None;

	/// <summary>
	/// When true, double-clicking a row toggles its individual edit mode.
	/// </summary>
	[Parameter]
	public bool EditOnDoubleClick { get; set; }

	/// <summary>
	/// Callback invoked when a row enters individual edit mode via <see cref="BeginEditAsync"/>.
	/// </summary>
	[Parameter]
	public EventCallback<TItem> RowEditStarted { get; set; }

	/// <summary>
	/// Callback invoked when a row leaves individual edit mode via <see cref="EndEditAsync"/> or <see cref="CancelEditAsync"/>.
	/// </summary>
	[Parameter]
	public EventCallback<TItem> RowEditEnded { get; set; }

	[Parameter]
	public bool DisplayRowNumberColumn { get; set; } = true;

	[Parameter]
	public bool Hierarchical { get; set; }

	[Parameter]
	public Func<TItem, object?>? HierarchyKeySelector { get; set; }

	[Parameter]
	public bool DisplayRefreshButton { get; set; } = false;

	/// <summary>
	/// CSS background color for the current row.
	/// </summary>
	[Parameter]
	public string CurrentRowBackground { get; set; } = "#3b95c6";

	/// <summary>
	/// CSS class for the container.
	/// </summary>
	[Parameter]
	public string? ContainerCssClass { get; set; }

	/// <summary>
	/// CSS class for the table element.
	/// </summary>
	[Parameter]
	public string TableCssClass { get; set; } = "table-striped table-hover table-bordered";

	/// <summary>
	/// Gets or sets the CSS class applied to the header element of the component.
	/// </summary>
	[Parameter]
	public string HeaderCssClass { get; set; } = "";

	/// <summary>
	/// Function to determine the CSS class for a row.
	/// </summary>
	[Parameter]
	public Func<TItem, string?>? RowClass { get; set; }

	/// <summary>
	/// Callback invoked when a row is clicked.
	/// </summary>
	[Parameter]
	public EventCallback<TItem> RowClicked { get; set; }

	/// <summary>
	/// Callback invoked when a row is double-clicked.
	/// </summary>
	[Parameter]
	public EventCallback<TItem> RowDoubleClicked { get; set; }


	/// <summary>
	/// Callback invoked when column settings change.
	/// </summary>
	[Parameter]
	public EventCallback<IEnumerable<SuperDataGridColumnSettings>> ColumnSettingsChanged { get; set; }

	/// <summary>
	/// Callback invoked when a cell is clicked.
	/// Returns the item and the property name of the clicked cell.
	/// </summary>
	[Parameter]
	public EventCallback<CellClickedEventArgs<TItem>> CellClicked { get; set; }

	/// <summary>
	/// The currently selected item.
	/// </summary>
	[Parameter]
	public TItem? CurrentItem { get; set; }

	/// <summary>
	/// Callback for two-way binding of CurrentItem.
	/// </summary>
	[Parameter]
	public EventCallback<TItem?> CurrentItemChanged { get; set; }

	[Parameter]
	public bool DisplayColumnVisibilityToggle { get; set; } = true;

	[Parameter]
	public bool DisplayFooter { get; set; } = true;

	[Parameter]
	public bool DisplayDefaultFooterTemplate { get; set; } = true;

	[Parameter]
	public string? DefaultSettingsName { get; set; }

	[Parameter]
	public SuperDataGridOrientation GridOrientation { get; set; } = SuperDataGridOrientation.Horizontal;

	/// <summary>
	/// Occurs when the data has been reloaded.
	/// </summary>
	/// <remarks>Subscribe to this event to be notified when a data reload operation completes. Handlers can be used
	/// to update UI elements or perform additional processing after the data is refreshed.</remarks>
	public event Action? DataReloaded;

	/// <summary>
	/// Gets the number of rows currently rendered in the table.
	/// This represents the number of visible items (not the total count).
	/// </summary>
	public int RowCount => _renderedItems.Count;

	public int TotalRowCount => _totalItemCount;

	/// <summary>
	/// Gets the current collection of grid columns with their configured properties.
	/// </summary>
	public IReadOnlyList<DataGridColumn<TItem>> ColumnsCollection => _columns;

	public string FooterText
	{
		get
		{
			var plurial = "";
			if (_totalItemCount > 1)
			{
				plurial = "s";
			}
			var result = $"Nombre de ligne{plurial}: {_totalItemCount}";
			if (_selectionInfo.SelectedCountTotal > 0)
			{
				plurial = "";
				if (_selectionInfo.SelectedCountTotal > 1)
				{
					plurial = "s";
				}
				result = result + $" - {_selectionInfo.SelectedCountTotal} sélectionnée{plurial}";
			}
			return result;
		}
	}

	List<SuperDataGridFilterInfo> _filterInfoList = new();

	/// <summary>
	/// Returns whether the given item is currently in individual row-edit mode.
	/// </summary>
	public bool IsRowInEditMode(TItem item)
	{
		ArgumentNullException.ThrowIfNull(item);
		var key = TryGetItemKey(item);
		return key is not null && _editingItemKeys.Contains(key);
	}

	/// <summary>
	/// Puts the given row into individual edit mode.
	/// </summary>
	public async Task BeginEditAsync(TItem item)
	{
		ArgumentNullException.ThrowIfNull(item);
		var key = TryGetItemKey(item);
		if (key is null)
		{
			return;
		}

		_editingItemKeys.Add(key);

		if (RowEditStarted.HasDelegate)
		{
			await RowEditStarted.InvokeAsync(item);
		}

		StateHasChanged();
	}

	/// <summary>
	/// Confirms and closes individual edit mode for the given row.
	/// </summary>
	public async Task EndEditAsync(TItem item)
	{
		ArgumentNullException.ThrowIfNull(item);
		var key = TryGetItemKey(item);
		if (key is null)
		{
			return;
		}

		_editingItemKeys.Remove(key);

		if (RowEditEnded.HasDelegate)
		{
			await RowEditEnded.InvokeAsync(item);
		}

		StateHasChanged();
	}

	/// <summary>
	/// Cancels individual edit mode for the given row without raising <see cref="RowEditEnded"/>.
	/// </summary>
	public Task CancelEditAsync(TItem item)
	{
		ArgumentNullException.ThrowIfNull(item);
		var key = TryGetItemKey(item);
		if (key is null)
		{
			return Task.CompletedTask;
		}

		_editingItemKeys.Remove(key);
		StateHasChanged();
		return Task.CompletedTask;
	}

	/// <summary>
	/// Reloads the data in the grid.
	/// </summary>
	public async Task ReloadAsync()
	{
		ResetHierarchyState();

		if (IsHierarchicalRenderingEnabled())
		{
			await LoadHierarchicalRootItemsAsync(CancellationToken.None);
			StateHasChanged();
			return;
		}

		if (_virtualizeRef is not null)
		{
			await _virtualizeRef.RefreshDataAsync();
			StateHasChanged();
		}
	}

	/// <summary>
	/// Forces a re-render of the UI.
	/// </summary>
	public Task RefreshAsync()
	{
		return InvokeAsync(StateHasChanged);
	}

	/// <summary>
	/// Expands every loaded root row and recursively expands its descendants.
	/// In hierarchical mode, root rows are rendered without virtualization to keep row heights stable.
	/// </summary>
	public async Task ExpandAllAsync(CancellationToken cancellationToken = default)
	{
		if (!Hierarchical || _renderedItems.Count == 0)
		{
			return;
		}

		var visitedKeys = new HashSet<object>();
		foreach (var item in _renderedItems.ToList())
		{
			await ExpandHierarchyBranchAsync(item, 0, visitedKeys, cancellationToken);
		}

		await InvokeAsync(StateHasChanged);
	}

	/// <summary>
	/// Collapses all expanded hierarchy rows and removes their loaded descendants from the grid state.
	/// </summary>
	public Task CollapseAllAsync()
	{
		ResetHierarchyState();
		return InvokeAsync(StateHasChanged);
	}

	/// <summary>
	/// Resets column settings to defaults.
	/// </summary>
	public async Task ResetColumnSettingsAsync()
	{
		foreach (var column in _columns)
		{
			column.ResetToDefaults();
		}

		if (!string.IsNullOrEmpty(GridId) && SettingsStorage is not null)
		{
			await SettingsStorage.ClearSettingsAsync(GridId);
		}

		InvalidateColumnStyleCache();
		NotifyColumnStateChanged();
		StateHasChanged();
	}

	/// <summary>
	/// Gets the current column settings.
	/// </summary>
	public IEnumerable<SuperDataGridColumnSettings> GetColumnSettings()
	{
		var result = _columns.Select((c, i) => new SuperDataGridColumnSettings
		{
			PropertyName = c.Property,
			Width = c.CurrentWidth,
			Order = i,
			IsVisible = c.CurrentVisible
		});

		return NormalizeColumnSettings(result);
	}

	/// <summary>
	/// Gets the current column visibility metadata for the grid.
	/// </summary>
	public IReadOnlyList<SuperDataGridColumnVisibilityInfo> GetColumnVisibilityInfo()
	{
		return _columns
			.Select((column, index) => new SuperDataGridColumnVisibilityInfo(
				index,
				column.Property,
				string.IsNullOrWhiteSpace(column.Title) ? column.Property : column.Title,
             column.CurrentVisible,
				column.AlwaysVisible))
			.ToList();
	}

	/// <summary>
	/// Sets the visibility of a column by index.
	/// </summary>
	/// <param name="columnIndex">The target column index in the current grid order.</param>
	/// <param name="isVisible">The new visibility state.</param>
	public async Task SetColumnVisibilityAsync(int columnIndex, bool isVisible)
	{
		if (columnIndex < 0 || columnIndex >= _columns.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(columnIndex));
		}

		var column = _columns[columnIndex];
		if (column.AlwaysVisible && !isVisible)
		{
			return;
		}

		if (column.CurrentVisible == isVisible)
		{
			return;
		}

		if (!isVisible && _columns.Count(c => c.CurrentVisible) <= 1)
		{
			return;
		}

		column.SetVisible(isVisible);
		InvalidateColumnStyleCache();
		await SaveSettingsAsync();
		await ColumnSettingsChanged.InvokeAsync(GetColumnSettings());
		NotifyColumnStateChanged();
		await InvokeAsync(StateHasChanged);
	}


	/// <summary>
	/// Inserts a column at the requested position or moves it there if it is already attached to the grid.
	/// </summary>
	/// <param name="position">Zero-based target index in the logical column collection.</param>
	/// <param name="column">The column instance to insert.</param>
	public void AddColumn(int position, DataGridColumn<TItem> column)
	{
		ArgumentNullException.ThrowIfNull(column);

		column.AttachToGrid(this);

		var existingIndex = _columns.IndexOf(column);
		var targetIndex = Math.Clamp(position, 0, _columns.Count);

		if (existingIndex >= 0)
		{
			if (existingIndex == targetIndex)
			{
				ApplyLoadedColumnSettingsIfAvailable();
				InvalidateColumnStyleCache();
				StateHasChanged();
				return;
			}

			_columns.RemoveAt(existingIndex);
			targetIndex = Math.Clamp(position, 0, _columns.Count);
		}

		_columns.Insert(targetIndex, column);
		column.MarkRegistrationState(true);
		ApplyLoadedColumnSettingsIfAvailable();
		InvalidateColumnStyleCache();
		NotifyColumnStateChanged();
		StateHasChanged();
	}

	internal void AddColumn(DataGridColumn<TItem> column)
	{
		ArgumentNullException.ThrowIfNull(column);
		column.AttachToGrid(this);
	}

	internal void AddColumnCore(DataGridColumn<TItem> column)
	{
		if (_columns.Contains(column))
		{
			return;
		}

		_columns.Add(column);
		column.MarkRegistrationState(true);
		ApplyLoadedColumnSettingsIfAvailable();
		InvalidateColumnStyleCache();
		StateHasChanged();
	}

	internal void RemoveColumn(DataGridColumn<TItem> column)
	{
		if (_columns.Remove(column))
		{
			column.MarkRegistrationState(false);
		}
		InvalidateColumnStyleCache();
		StateHasChanged();
	}

	public override async Task SetParametersAsync(ParameterView parameters)
	{
		if (!_defaultSettingsApplied)
		{
			_defaultSettingsApplied = true;

			if (parameters.TryGetValue<string?>(nameof(DefaultSettingsName), out var settingsName)
				&& !string.IsNullOrEmpty(settingsName)
				&& SuperConfiguration is not null)
			{
				var preset = SuperConfiguration.SuperDataGridSettingsList
					.FirstOrDefault(s => s.Name == settingsName);

				if (preset is not null)
				{
					ApplyPresetDefaults(parameters, preset);
				}
			}
		}

		await base.SetParametersAsync(parameters);
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			_dotNetRef = DotNetObjectReference.Create(this);
			_jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
				"import", "./_content/SuperBlazorComponents/Components/SuperDataGrid/SuperDataGrid.razor.js");

			await _jsModule.InvokeVoidAsync("initialize", _containerRef, _dotNetRef);
			await LoadSettingsAsync();
			GridInstanceReady?.Invoke(this);
		}
	}

	protected override async Task OnParametersSetAsync()
	{
		if (IsHierarchicalRenderingEnabled() && !_hierarchicalRootItemsLoaded && ItemsProvider is not null)
		{
			await LoadHierarchicalRootItemsAsync(CancellationToken.None);
		}
	}

	private void ApplyPresetDefaults(ParameterView parameters, SuperDataGridSettings preset)
	{
		if (!parameters.TryGetValue<float>(nameof(RowHeight), out _)) { RowHeight = preset.RowHeight; }
		if (!parameters.TryGetValue<bool>(nameof(FixedRowHeight), out _)) { FixedRowHeight = preset.FixedRowHeight; }
		if (!parameters.TryGetValue<int>(nameof(OverscanCount), out _)) { OverscanCount = preset.OverscanCount; }
		if (!parameters.TryGetValue<bool>(nameof(FreezeHeader), out _)) { FreezeHeader = preset.FreezeHeader; }
		if (!parameters.TryGetValue<bool>(nameof(FreezeFooter), out _)) { FreezeFooter = preset.FreezeFooter; }
		if (!parameters.TryGetValue<bool>(nameof(AllowColumnReorder), out _)) { AllowColumnReorder = preset.AllowColumnReorder; }
		if (!parameters.TryGetValue<bool>(nameof(AllowColumnResize), out _)) { AllowColumnResize = preset.AllowColumnResize; }
		if (!parameters.TryGetValue<bool>(nameof(AllowSorting), out _)) { AllowSorting = preset.AllowSorting; }
		if (!parameters.TryGetValue<bool>(nameof(AllowFiltering), out _)) { AllowFiltering = preset.AllowFiltering; }
		if (!parameters.TryGetValue<SuperDataGridEditionMode>(nameof(EditionMode), out _)) { EditionMode = preset.EditionMode; }
		if (!parameters.TryGetValue<bool>(nameof(EditOnDoubleClick), out _)) { EditOnDoubleClick = preset.EditOnDoubleClick; }
		if (!parameters.TryGetValue<bool>(nameof(DisplayRowNumberColumn), out _)) { DisplayRowNumberColumn = preset.DisplayRowNumberColumn; }
		if (!parameters.TryGetValue<bool>(nameof(DisplayRefreshButton), out _)) { DisplayRefreshButton = preset.DisplayRefreshButton; }
		if (!parameters.TryGetValue<bool>(nameof(DisplayColumnVisibilityToggle), out _)) { DisplayColumnVisibilityToggle = preset.DisplayColumnVisibilityToggle; }
		if (!parameters.TryGetValue<bool>(nameof(DisplayFooter), out _)) { DisplayFooter = preset.DisplayFooter; }
		if (!parameters.TryGetValue<bool>(nameof(DisplayDefaultFooterTemplate), out _)) { DisplayDefaultFooterTemplate = preset.DisplayDefaultFooterTemplate; }
		if (!parameters.TryGetValue<string>(nameof(CurrentRowBackground), out _)) { CurrentRowBackground = preset.CurrentRowBackground; }
		if (!parameters.TryGetValue<string?>(nameof(ContainerCssClass), out _)) { ContainerCssClass = preset.ContainerCssClass; }
		if (!parameters.TryGetValue<string>(nameof(TableCssClass), out _)) { TableCssClass = preset.TableCssClass; }
		if (!parameters.TryGetValue<string>(nameof(HeaderCssClass), out _)) { HeaderCssClass = preset.HeaderCssClass; }
		if (!parameters.TryGetValue<SuperDataGridSelectionMode>(nameof(SelectionMode), out _)) { SelectionMode = preset.SelectionMode; }
		if (!parameters.TryGetValue<bool>(nameof(DisplaySelectionColumn), out _)) { DisplaySelectionColumn = preset.DisplaySelectionColumn; }
	}

	private async Task LoadSettingsAsync()
	{
		if (string.IsNullOrEmpty(GridId) || SettingsStorage is null || _settingsLoaded)
		{
			return;
		}

		var settings = await SettingsStorage.GetSettingsAsync(GridId);
		if (settings is not null && settings.Any())
		{
			_loadedColumnSettings = NormalizeColumnSettings(settings);
			ApplyColumnSettings(_loadedColumnSettings);
			await InvokeAsync(StateHasChanged);
		}

		_settingsLoaded = true;
	}

	private void ApplyColumnSettings(IEnumerable<SuperDataGridColumnSettings> settings)
	{
		var settingsList = NormalizeColumnSettings(settings);
		var reorderedColumns = new List<DataGridColumn<TItem>>();

		foreach (var setting in settingsList.OrderBy(s => s.Order))
		{
			var column = _columns.FirstOrDefault(c => c.Property == setting.PropertyName);
			if (column is not null)
			{
				if (!string.IsNullOrEmpty(setting.Width))
				{
					column.SetWidth(setting.Width);
				}
				column.SetVisible(column.AlwaysVisible || setting.IsVisible);
				reorderedColumns.Add(column);
			}
		}

		// Add any columns not in settings at the end
		foreach (var column in _columns)
		{
			if (!reorderedColumns.Contains(column))
			{
				reorderedColumns.Add(column);
			}
		}

		_columns = reorderedColumns;
		InvalidateColumnStyleCache();
		NotifyColumnStateChanged();
	}

	private void ApplyLoadedColumnSettingsIfAvailable()
	{
		if (_loadedColumnSettings is { Count: > 0 })
		{
			ApplyColumnSettings(_loadedColumnSettings);
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_jsModule is not null)
		{
			try
			{
				await _jsModule.InvokeVoidAsync("dispose", _containerRef);
				await _jsModule.DisposeAsync();
			}
			catch (JSDisconnectedException)
			{
				// Circuit is disconnected, ignore
			}
		}

		_dotNetRef?.Dispose();
	}

	internal async Task ApplyFilter(SuperDataGridFilterInfo filterInfo)
	{
		if (filterInfo is null)
		{
			ResetHierarchyState();
			await ReloadAsync();
			return;
		}
		_filterInfoList.RemoveAll(f => f.PropertyName == filterInfo.PropertyName);

		if (HasFilterValue(filterInfo))
		{
			_filterInfoList.Add(filterInfo);
		}

		// On relance le chargement des données avec les nouveaux filtres
		ResetHierarchyState();
		await ReloadAsync();
	}

	private static bool HasFilterValue(SuperDataGridFilterInfo filterInfo)
	{
		ArgumentNullException.ThrowIfNull(filterInfo);

		return !string.IsNullOrWhiteSpace(filterInfo.PropertyValue)
         || filterInfo.SelectedValues.Count > 0
			|| filterInfo.StartDate is not null
          || filterInfo.EndDate is not null
			|| filterInfo.FromNumericValue is not null
			|| filterInfo.ToNumericValue is not null;
	}

	private async ValueTask<ItemsProviderResult<TItem>> LoadItemsAsync(ItemsProviderRequest request)
	{
		var count = request.Count;
		//if (_isLoading)
		//{
		//    // Prevent overlapping requests
		//    return new ItemsProviderResult<TItem>(Array.Empty<TItem>(), 0);
		//}
		_isLoading = true;

		try
		{
			ItemsProviderResult<TItem> result;

			var providerRequest = new GridItemsProviderRequest<TItem>(
				StartIndex: request.StartIndex,
				Count: count,
				SortColumn: _sortColumn,
				SortDirection: _sortDirection,
				Filters: _filterInfoList,
				CancellationToken: request.CancellationToken
			);

			var providerResult = await ItemsProvider(providerRequest);
			_totalItemCount = providerResult.TotalItemCount;
			ResetHierarchyState();

			_renderedItems = providerResult.Items.ToList();
			for (var i = 0; i < _renderedItems.Count; i++)
			{
				var rowNumber = request.StartIndex + i + 1;
				SetRowNumber(_renderedItems[i], rowNumber);
				_rowNumberLookup[_renderedItems[i]!] = rowNumber;
			}

			if (DataLoaded.HasDelegate)
			{
				await DataLoaded.InvokeAsync(new SuperDataGridDataLoadedEventArgs<TItem>(
					 _renderedItems,
					 providerResult.TotalItemCount,
					 request.StartIndex,
					 count));
			}

			SyncRenderedItemsSelectionState();

			result = new ItemsProviderResult<TItem>(_renderedItems, providerResult.TotalItemCount);
			return result;
		}
		catch (OperationCanceledException) when (request.CancellationToken.IsCancellationRequested)
		{
			// Cancellation is expected when the user scrolls quickly and Virtualize cancels in-flight requests.
			// Do not log as error and do not return stale cached data for a different request.
			throw;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Error loading items in SuperDataGrid");
			throw;
		}
		finally
		{
			_isLoading = false;
			if (DataReloaded is not null)
			{
				DataReloaded.Invoke();
			}
			// Le footer (FooterText) est rendu par le composant parent, en dehors de Virtualize.
			// Il faut notifier le composant que _totalItemCount a changé.
			_ = InvokeAsync(StateHasChanged);
		}
	}

	private List<DataGridColumn<TItem>> GetVisibleColumns()
	{
		if (_cachedVisibleColumns is not null)
		{
			return _cachedVisibleColumns;
		}

		_cachedVisibleColumns = _columns.Where(c => c.CurrentVisible).ToList();
		return _cachedVisibleColumns;
	}

	private void InvalidateColumnStyleCache()
	{
		_cacheVersion++;
		_columnStyleCache.Clear();
		_cachedVisibleColumns = null;
	}

	private void InvalidateItemsCache()
	{
		// Cache items désactivé
	}

	private string? GetContainerStyle()
	{
		var styles = new List<string>();

		styles.Add($"--sdg-row-number-width: {GetRowNumberWidth()}px");
		styles.Add($"--sdg-selection-width: {SELECTION_WIDTH}px");
		styles.Add($"--sdg-actions-width: {ActionsWidth}px");

		if (!string.IsNullOrWhiteSpace(CurrentRowBackground))
		{
			styles.Add($"--sdg-current-row-bg: {CurrentRowBackground}");
		}

		var rowHeight = Math.Max(1, (int)MathF.Round(RowHeight));
		styles.Add($"--sdg-row-height: {rowHeight}px");

		return styles.Count == 0 ? null : string.Join("; ", styles) + ";";
	}

	private string GetContainerClass()
	{
		var classes = new List<string> { "sdg-container" };

		if (IsFixedRowHeightEnabled())
		{
			classes.Add("sdg-fixed-row-height");
		}

		if (!string.IsNullOrWhiteSpace(ContainerCssClass))
		{
			classes.Add(ContainerCssClass);
		}

		return string.Join(" ", classes);
	}

	private bool IsFixedRowHeightEnabled()
	{
		return FixedRowHeight && GridOrientation == SuperDataGridOrientation.Horizontal;
	}

	private string GetTableWrapperStyle()
	{
		// Si Height est 100%, on laisse le flex gérer la hauteur
		// sinon on applique la hauteur explicite
		if (Height == "100%")
		{
			return string.Empty;
		}
		return $"height: {Height};";
	}

	/// <summary>
	/// Calculates the virtualization item size for vertical orientation.
	/// Each item spans multiple rows (one per visible column).
	/// </summary>
	private float GetVerticalItemSize()
	{
		var columnCount = GetVisibleColumns().Count;
		return RowHeight * Math.Max(1, columnCount);
	}

	private string GetColumnStyle(DataGridColumn<TItem> column)
	{
		var cacheKey = $"{column.Property}_style_{_cacheVersion}";
		if (_columnStyleCache.TryGetValue(cacheKey, out var cachedStyle))
		{
			return cachedStyle;
		}

		var styles = new List<string>();

		if (!string.IsNullOrEmpty(column.CurrentWidth))
		{
			styles.Add($"width: {column.CurrentWidth}");
			styles.Add($"min-width: {column.CurrentWidth}");
		}

		if (!string.IsNullOrEmpty(column.MinWidth))
		{
			styles.Add($"min-width: {column.MinWidth}");
		}

		if (!string.IsNullOrEmpty(column.MaxWidth))
		{
			styles.Add($"max-width: {column.MaxWidth}");
		}

		var visibleColumns = GetVisibleColumns();
		var index = visibleColumns.IndexOf(column);

		// Frozen left columns - utiliser inset-inline-start comme Radzen
		if (index < FreezeLeftColumns && index >= 0)
		{
			var leftOffsetParts = new List<string>();

			if (DisplayRowNumberColumn)
			{
				leftOffsetParts.Add($"{GetRowNumberWidth()}px");
			}

			if (DisplaySelectionColumn)
			{
				leftOffsetParts.Add($"{SELECTION_WIDTH}px");
			}

			if (ActionsTemplate is not null)
			{
				leftOffsetParts.Add($"{ActionsWidth}px");
			}

			if (index > 0)
			{
				leftOffsetParts.AddRange(visibleColumns.Take(index).Select(c => c.CurrentWidth ?? "100px"));
			}

			if (leftOffsetParts.Count == 0)
			{
				styles.Add("inset-inline-start: 0");
			}
			else if (leftOffsetParts.Count == 1)
			{
				styles.Add($"inset-inline-start: {leftOffsetParts[0]}");
			}
			else
			{
				var calcExpression = string.Join(" + ", leftOffsetParts);
				styles.Add($"inset-inline-start: calc({calcExpression})");
			}
			styles.Add("z-index: 40 !important");
			styles.Add("position: sticky");
		}

		// Frozen right columns - utiliser inset-inline-end
		if (FreezeRightColumns > 0 && index >= visibleColumns.Count - FreezeRightColumns && index >= 0)
		{
			var rightIndex = visibleColumns.Count - index - 1;

			if (rightIndex == 0)
			{
				styles.Add("inset-inline-end: 0");
			}
			else
			{
				// Calculer le décalage cumulé avec calc()
				var calcParts = visibleColumns.Skip(index + 1)
					.Select(c => c.CurrentWidth ?? "100px")
					.ToList();

				if (calcParts.Count > 0)
				{
					var calcExpression = string.Join(" + ", calcParts);
					styles.Add($"inset-inline-end: calc({calcExpression})");
				}
			}
			styles.Add("z-index: 40 !important");
			styles.Add("position: sticky");
		}

		var styleString = string.Join("; ", styles);
		_columnStyleCache[cacheKey] = styleString;
		return styleString;
	}

	private static int ParseWidth(string? width)
	{
		if (string.IsNullOrEmpty(width))
		{
			return 100; // Default width
		}

		var numericPart = new string(width.TakeWhile(c => char.IsDigit(c)).ToArray());
		return int.TryParse(numericPart, out var result) ? result : 100;
	}

	private string GetColumnHeaderClass(DataGridColumn<TItem> column)
	{
		var classes = new List<string> { "sdg-column-header" };

		if (column.Sortable && AllowSorting)
		{
			classes.Add("sdg-sortable");
		}

		if (_draggedColumn == column)
		{
			classes.Add("sdg-dragging");
		}

		if (!string.IsNullOrEmpty(column.HeaderCssClass))
		{
			classes.Add(column.HeaderCssClass);
		}

		var visibleColumns = GetVisibleColumns();
		var visibleIndex = visibleColumns.IndexOf(column);
		var isFrozenLeft = visibleIndex >= 0 && visibleIndex < FreezeLeftColumns;
		var isFrozenRight = FreezeRightColumns > 0 && visibleIndex >= 0 && visibleIndex >= visibleColumns.Count - FreezeRightColumns;

		if (isFrozenLeft && visibleIndex >= 0)
		{
			classes.Add("sdg-frozen-left");
			if (visibleIndex == (FreezeLeftColumns - 1))
			{
				classes.Add("sdg-frozen-left-last");
			}
		}

		if (isFrozenRight && visibleIndex >= 0)
		{
			classes.Add("sdg-frozen-right");
		}

		classes.Add(GetTextAlignmentClass(column.TextAlign));

		return string.Join(" ", classes);
	}

	private string? GetColumnHeaderStyle(DataGridColumn<TItem> column)
	{
		return GetColumnStyle(column);
	}

	private string? GetColumnFooterStyle(DataGridColumn<TItem> column)
	{
		return GetColumnStyle(column);
	}

	private string GetSortIndicatorClass(DataGridColumn<TItem> column)
	{
		if (column.Property != _sortColumn)
		{
			return "";
		}

		return _sortDirection switch
		{
			SortDirection.Ascending => "sdg-sort-asc",
			SortDirection.Descending => "sdg-sort-desc",
			_ => ""
		};
	}

	private string GetRowClass(TItem item)
	{
		var classes = new List<string> { "sdg-row" };

		if (IsCurrentRow(item))
		{
			classes.Add("sdg-row-current");
		}

        if (EqualityComparer<TItem>.Default.Equals(item, CurrentItem))
		{
			classes.Add("sdg-row-selected");
		}

      if (_selectionInfo.SelectedItems.Contains(item))
		{
			classes.Add("sdg-row-selected");
		}

		if (IsRowInEditMode(item))
		{
			classes.Add("sdg-row-editing");
		}

		var customClass = RowClass?.Invoke(item);
		if (!string.IsNullOrEmpty(customClass))
		{
			classes.Add(customClass);
		}

		return string.Join(" ", classes);
	}

	private bool IsCurrentRow(TItem item)
	{
		ArgumentNullException.ThrowIfNull(item);

		if (_currentRowKey is null)
		{
			return false;
		}

		var itemKey = TryGetItemKey(item);
		return itemKey is not null && Equals(itemKey, _currentRowKey);
	}

	private static object? TryGetItemKey(TItem item)
	{
		ArgumentNullException.ThrowIfNull(item);

		var keyProperty = typeof(TItem).GetProperty(nameof(IDataItem.KeyValue));
		if (keyProperty is not null)
		{
			return keyProperty.GetValue(item);
		}

		return item;
	}

	private string GetCellClass(DataGridColumn<TItem> column, TItem item, string? cellTitle = null)
	{
		var classes = new List<string> { "sdg-cell" };

		if (IsRowSelected(item))
		{
			classes.Add("sdg-cell-selected");
		}

		if (!string.IsNullOrEmpty(column.CssClass))
		{
			classes.Add(column.CssClass);
		}

		var customClass = column.CellClass?.Invoke(item);
		if (!string.IsNullOrEmpty(customClass))
		{
			classes.Add(customClass);
		}

		// Mark cells that render multiline plain text so the CSS hover rule can expand them
		if ((cellTitle ?? GetCellTitle(column, item)) is not null)
		{
			classes.Add("sdg-cell-multiline");
		}

		var visibleColumns = GetVisibleColumns();
		var index = visibleColumns.IndexOf(column);

		if (index >= 0 && index < FreezeLeftColumns)
		{
			classes.Add("sdg-frozen-left");
		}

		if (index >= 0 && index == (FreezeLeftColumns - 1))
		{
			classes.Add("sdg-frozen-left-last");
		}

		if (FreezeRightColumns > 0 && index >= visibleColumns.Count - FreezeRightColumns)
		{
			classes.Add("sdg-frozen-right");
		}

		classes.Add(GetTextAlignmentClass(column.TextAlign));

		return string.Join(" ", classes);
	}

	private string GetCombinedCellStyle(DataGridColumn<TItem> column)
	{
		var result = GetColumnStyle(column);
		return result;
	}

	private object? GetCellValue(DataGridColumn<TItem> column, TItem item)
	{
		if (item is null || string.IsNullOrEmpty(column.Property))
		{
			return null;
		}

		try
		{
			var property = typeof(TItem).GetProperty(column.Property);
			var value = property?.GetValue(item);

			if (value is not null && !string.IsNullOrEmpty(column.FormatString))
			{
				return string.Format(column.FormatString, value);
			}

			return value;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>Returns true when the cell renders plain text (no custom template active for this item).</summary>
	private bool IsTextCell(DataGridColumn<TItem> column, TItem item)
	{
		if ((EditionMode == SuperDataGridEditionMode.Edit || IsRowInEditMode(item)) && column.EditTemplate is not null)
		{
			return false;
		}

		return column.Template is null;
	}

	/// <summary>Returns the cell value as a string for use as a native title tooltip, only when the value contains line breaks.</summary>
	private string? GetCellTitle(DataGridColumn<TItem> column, TItem item)
	{
		if (!IsTextCell(column, item))
		{
			return null;
		}

		var value = GetCellValue(column, item)?.ToString();
		if (string.IsNullOrEmpty(value) || (!value.Contains('\n') && !value.Contains('\r')))
		{
			return null;
		}

		return value;
	}

	private async Task OnHeaderClick(DataGridColumn<TItem> column)
	{
		if (!AllowSorting || !column.Sortable)
		{
			return;
		}

		if (_sortColumn == column.Property)
		{
			// Cycle through: Ascending -> Descending -> None
			_sortDirection = _sortDirection switch
			{
				SortDirection.None => SortDirection.Ascending,
				SortDirection.Ascending => SortDirection.Descending,
				SortDirection.Descending => SortDirection.None,
				_ => SortDirection.Ascending
			};

			if (_sortDirection == SortDirection.None)
			{
				_sortColumn = null;
			}
		}
		else
		{
			_sortColumn = column.Property;
			_sortDirection = SortDirection.Ascending;
		}

		// Invalider le cache car l'ordre des items change
		InvalidateItemsCache();
		ResetHierarchyState();

		// Refresh the data with new sort
		if (_virtualizeRef is not null)
		{
			await _virtualizeRef.RefreshDataAsync();
		}
		else if (IsHierarchicalRenderingEnabled())
		{
			await LoadHierarchicalRootItemsAsync(CancellationToken.None);
		}

		StateHasChanged();
	}

	private async Task OnRowDoubleClick(TItem item)
	{
		if (EditOnDoubleClick)
		{
			if (IsRowInEditMode(item))
			{
				await EndEditAsync(item);
			}
			else
			{
				await BeginEditAsync(item);
			}

			await RowDoubleClicked.InvokeAsync(item);
			return;
		}

		if (IsRowSelected(item))
		{
			_selectionInfo.SelectedItems.Remove(item);
			SetItemSelected(item, false);
		   _selectionInfo.AllSelected = false;
		}
		else
		{
		   _selectionInfo.SelectedItems.Add(item);
			SetItemSelected(item, true);
		}

	   await NotifySelectionChangedAsync(CurrentItem);
		await RowDoubleClicked.InvokeAsync(item);
		StateHasChanged();
	}

	private void OnColumnDragStart(DragEventArgs e, DataGridColumn<TItem> column)
	{
		if (!AllowColumnReorder)
		{
			return;
		}

		_draggedColumn = column;
	}

	private void OnColumnDragEnd(DragEventArgs e)
	{
		_draggedColumn = null;
	}

	private void OnColumnDragOver(DragEventArgs e, DataGridColumn<TItem> column)
	{
		// Allow drop
	}

	private async Task OnColumnDrop(DragEventArgs e, DataGridColumn<TItem> targetColumn)
	{
		if (_draggedColumn is null || _draggedColumn == targetColumn || !AllowColumnReorder)
		{
			return;
		}

		var draggedIndex = _columns.IndexOf(_draggedColumn);
		var targetIndex = _columns.IndexOf(targetColumn);

		_columns.RemoveAt(draggedIndex);
		_columns.Insert(targetIndex, _draggedColumn);

		_draggedColumn = null;

		InvalidateColumnStyleCache();
		await SaveSettingsAsync();
		await ColumnSettingsChanged.InvokeAsync(GetColumnSettings());
		NotifyColumnStateChanged();
		StateHasChanged();
	}

	private async Task OnResizeStart(MouseEventArgs e, DataGridColumn<TItem> column)
	{
		if (!AllowColumnResize || _jsModule is null)
		{
			return;
		}

		var visibleColumnIndex = GetVisibleColumns().IndexOf(column);
		if (visibleColumnIndex < 0)
		{
			return;
		}

		var systemOffset = 0;
		if (DisplayRowNumberColumn)
		{
			systemOffset++;
		}

		if (DisplaySelectionColumn)
		{
			systemOffset++;
		}

		var tableColumnIndex = visibleColumnIndex + systemOffset;

		await _jsModule.InvokeVoidAsync("startResize", _tableRef, tableColumnIndex, e.ClientX);
	}

	/// <summary>
	/// Called from JavaScript when column resize is completed.
	/// </summary>
	[JSInvokable]
	public async Task OnResizeComplete(int columnIndex, double newWidth)
	{
		var systemOffset = 0;
		if (DisplayRowNumberColumn)
		{
			systemOffset++;
		}

		if (DisplaySelectionColumn)
		{
			systemOffset++;
		}

		var visibleColumnIndex = columnIndex - systemOffset;
		var visibleColumns = GetVisibleColumns();
		if (visibleColumnIndex >= 0 && visibleColumnIndex < visibleColumns.Count)
		{
			visibleColumns[visibleColumnIndex].SetWidth($"{Math.Max(50, (int)newWidth)}px");
			InvalidateColumnStyleCache();
			await SaveSettingsAsync();
			await ColumnSettingsChanged.InvokeAsync(GetColumnSettings());
			NotifyColumnStateChanged();
			StateHasChanged();
		}
	}

	private async Task SaveSettingsAsync()
	{
		if (string.IsNullOrEmpty(GridId) || SettingsStorage is null)
		{
			return;
		}

       var deduplicatedSettings = NormalizeColumnSettings(GetColumnSettings());
		await SettingsStorage.SaveSettingsAsync(GridId, deduplicatedSettings);
	}

	private static List<SuperDataGridColumnSettings> NormalizeColumnSettings(IEnumerable<SuperDataGridColumnSettings> settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		var uniqueSettings = new List<SuperDataGridColumnSettings>();
		var seenProperties = new HashSet<string>(StringComparer.Ordinal);

		foreach (var setting in settings.OrderBy(s => s.Order))
		{
			if (string.IsNullOrWhiteSpace(setting.PropertyName))
			{
				continue;
			}

			if (!seenProperties.Add(setting.PropertyName))
			{
				continue;
			}

			uniqueSettings.Add(setting);
		}

		for (var i = 0; i < uniqueSettings.Count; i++)
		{
			uniqueSettings[i] = uniqueSettings[i] with { Order = i };
		}

		return uniqueSettings;
	}


	private int GetRowNumber(TItem item)
	{
		var rowNumberProperty = typeof(TItem).GetProperty(nameof(IDataItem.RowNumber));
		if (rowNumberProperty?.PropertyType == typeof(int))
		{
			return (int?)rowNumberProperty.GetValue(item) ?? 0;
		}

		if (_rowNumberLookup.TryGetValue(item!, out var rowNumber))
		{
			return rowNumber;
		}

		return 0;
	}

	private int GetRowNumberWidth()
	{
		return Hierarchical ? 78 : ROW_NUMBER_WIDTH;
	}

	private bool IsHierarchicalRenderingEnabled()
	{
		return Hierarchical && GridOrientation == SuperDataGridOrientation.Horizontal;
	}

	private async Task LoadHierarchicalRootItemsAsync(CancellationToken cancellationToken)
	{
		_isLoading = true;
		_hierarchicalRootItemsLoaded = true;

		try
		{
			var providerRequest = new GridItemsProviderRequest<TItem>(
				StartIndex: 0,
				Count: null,
				SortColumn: _sortColumn,
				SortDirection: _sortDirection,
				Filters: _filterInfoList,
				CancellationToken: cancellationToken);

			var providerResult = await ItemsProvider(providerRequest);
			_totalItemCount = providerResult.TotalItemCount;
			ResetHierarchyState();

			_hierarchicalRootItems = providerResult.Items.ToList();
			_renderedItems = _hierarchicalRootItems;
			for (var i = 0; i < _renderedItems.Count; i++)
			{
				var rowNumber = i + 1;
				SetRowNumber(_renderedItems[i], rowNumber);
				_rowNumberLookup[_renderedItems[i]!] = rowNumber;
			}

			if (DataLoaded.HasDelegate)
			{
				await DataLoaded.InvokeAsync(new SuperDataGridDataLoadedEventArgs<TItem>(
					_renderedItems,
					providerResult.TotalItemCount,
					0,
					_renderedItems.Count));
			}

			SyncRenderedItemsSelectionState();
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Error loading hierarchical root items in SuperDataGrid");
			throw;
		}
		finally
		{
			_isLoading = false;
			DataReloaded?.Invoke();
			_ = InvokeAsync(StateHasChanged);
		}
	}

	private IReadOnlyList<HierarchyGridRow> GetHierarchyRows(TItem item)
	{
		var rows = new List<HierarchyGridRow>
		{
			new(item, 0, GetRowNumber(item))
		};

		if (Hierarchical)
		{
			AppendExpandedChildren(item, 1, rows);
		}

		return rows;
	}

	private void AppendExpandedChildren(TItem parent, int level, List<HierarchyGridRow> rows)
	{
		var state = GetHierarchyState(parent, create: false);
		if (state?.IsExpanded != true || state.Children.Count == 0)
		{
			return;
		}

		for (var i = 0; i < state.Children.Count; i++)
		{
			var child = state.Children[i];
			var rowNumber = i + 1;
			SetRowNumber(child, rowNumber);
			rows.Add(new HierarchyGridRow(child, level, rowNumber));
			AppendExpandedChildren(child, level + 1, rows);
		}
	}

	private bool ShouldDisplayHierarchyToggle(TItem item)
	{
		if (!Hierarchical)
		{
			return false;
		}

		var state = GetHierarchyState(item, create: false);
		return state?.HasNoChildren != true;
	}

	private bool IsHierarchyExpanded(TItem item)
	{
		var state = GetHierarchyState(item, create: false);
		return state?.IsExpanded == true;
	}

	private bool IsHierarchyLoading(TItem item)
	{
		var state = GetHierarchyState(item, create: false);
		return state?.IsLoading == true;
	}

	private async Task ToggleHierarchyAsync(TItem item, int level)
	{
		var state = GetHierarchyState(item, create: true);
		if (state is null || state.IsLoading)
		{
			return;
		}

		if (state.IsExpanded)
		{
			RemoveDescendantHierarchyState(state.Children);
			state.Children.Clear();
			state.IsExpanded = false;
			await InvokeAsync(StateHasChanged);
			return;
		}

		state.IsLoading = true;
		await InvokeAsync(StateHasChanged);

		try
		{
			await LoadHierarchyChildrenAsync(item, level, state, CancellationToken.None);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Error loading child items in SuperDataGrid");
			throw;
		}
		finally
		{
			state.IsLoading = false;
			await InvokeAsync(StateHasChanged);
		}
	}

	private async Task ExpandHierarchyBranchAsync(
		TItem item,
		int level,
		HashSet<object> visitedKeys,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var key = GetHierarchyKey(item);
		if (key is not null && !visitedKeys.Add(key))
		{
			return;
		}

		var state = GetHierarchyState(item, create: true);
		if (state is null || state.HasNoChildren)
		{
			return;
		}

		state.IsLoading = true;
		await InvokeAsync(StateHasChanged);

		try
		{
			await LoadHierarchyChildrenAsync(item, level, state, cancellationToken);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Error expanding hierarchy branch in SuperDataGrid");
			throw;
		}
		finally
		{
			state.IsLoading = false;
			await InvokeAsync(StateHasChanged);
		}

		foreach (var child in state.Children.ToList())
		{
			await ExpandHierarchyBranchAsync(child, level + 1, visitedKeys, cancellationToken);
		}
	}

	private async Task LoadHierarchyChildrenAsync(
		TItem item,
		int level,
		HierarchyRowState state,
		CancellationToken cancellationToken)
	{
		RemoveDescendantHierarchyState(state.Children);
		state.Children.Clear();
		state.IsExpanded = false;

		var parentKey = GetHierarchyKey(item);
		var providerRequest = new GridItemsProviderRequest<TItem>(
			StartIndex: 0,
			Count: null,
			SortColumn: _sortColumn,
			SortDirection: _sortDirection,
			Filters: _filterInfoList,
			CancellationToken: cancellationToken,
			ParentItem: item,
			ParentKey: parentKey,
			HierarchyLevel: level + 1);

		var providerResult = await ItemsProvider(providerRequest);
		state.Children = providerResult.Items.ToList();
		state.HasNoChildren = state.Children.Count == 0;
		state.IsExpanded = state.Children.Count > 0;

		for (var i = 0; i < state.Children.Count; i++)
		{
			SetRowNumber(state.Children[i], i + 1);
		}
	}

	private string GetHierarchyRowNumberStyle(int level)
	{
		var baseStyle = GetRowNumberCellStyle();
		if (!Hierarchical || level <= 0)
		{
			return baseStyle;
		}

		return $"{baseStyle} --sdg-hierarchy-level: {level};";
	}

	private object? GetHierarchyKey(TItem item)
	{
		ArgumentNullException.ThrowIfNull(item);
		return HierarchyKeySelector?.Invoke(item) ?? TryGetItemKey(item) ?? item;
	}

	private HierarchyRowState? GetHierarchyState(TItem item, bool create)
	{
		var key = GetHierarchyKey(item);
		if (key is null)
		{
			return null;
		}

		if (_hierarchyState.TryGetValue(key, out var state))
		{
			return state;
		}

		if (!create)
		{
			return null;
		}

		state = new HierarchyRowState();
		_hierarchyState[key] = state;
		return state;
	}

	private void ResetHierarchyState()
	{
		_hierarchyState.Clear();
	}

	private void RemoveDescendantHierarchyState(IEnumerable<TItem> items)
	{
		foreach (var item in items)
		{
			var state = GetHierarchyState(item, create: false);
			if (state is not null)
			{
				RemoveDescendantHierarchyState(state.Children);
			}

			var key = GetHierarchyKey(item);
			if (key is not null)
			{
				_hierarchyState.Remove(key);
			}
		}
	}

	private sealed class HierarchyRowState
	{
		public bool IsExpanded { get; set; }

		public bool IsLoading { get; set; }

		public bool HasNoChildren { get; set; }

		public List<TItem> Children { get; set; } = [];
	}

	private readonly record struct HierarchyGridRow(TItem Item, int Level, int RowNumber);

	private static void SetRowNumber(TItem item, int rowNumber)
	{
		var rowNumberProperty = typeof(TItem).GetProperty(nameof(IDataItem.RowNumber));
		if (rowNumberProperty?.CanWrite == true && rowNumberProperty.PropertyType == typeof(int))
		{
			rowNumberProperty.SetValue(item, rowNumber);
		}
	}

	private static string GetTextAlignmentClass(SuperTextAlignment alignment)
	{
		return alignment switch
		{
			SuperTextAlignment.Center => "sdg-text-center",
			SuperTextAlignment.Right => "sdg-text-right",
			SuperTextAlignment.Left => "sdg-text-left",
			_ => "sdg-text-left"
		};
	}

	private void NotifyColumnStateChanged()
	{
		ColumnStateChanged?.Invoke(this, EventArgs.Empty);
	}
}
