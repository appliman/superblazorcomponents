---
name: RadzenDatagridToSuperDataGridSkill
description: >
  Migration guide from Radzen Blazor RadzenDataGrid to SuperBlazorComponents SuperDataGrid.
  Use this skill when migrating, converting, or replacing a RadzenDataGrid with a SuperDataGrid,
  or when a user asks how to map RadzenDataGrid parameters, events, columns, or patterns
  to their SuperDataGrid equivalents.
applyTo: "**/*.razor"
---

# Skill: Migrate RadzenDataGrid → SuperDataGrid

## Overview

`SuperDataGrid<TItem>` is a high-performance, virtualized Blazor data grid built into
`SuperBlazorComponents`. It uses a **delegate-based `ItemsProvider`** pattern (similar to
QuickGrid) rather than the `Data`/`LoadData` pattern used by Radzen.

This skill provides exact mapping rules, before/after code examples, and a list of
Radzen features that have no direct equivalent (with recommended workarounds).

---

## 1. Required Namespace & Registration

### Radzen
```razor
@using Radzen.Blazor
```
```csharp
// Program.cs
builder.Services.AddRadzenComponents();
```

### SuperDataGrid
```razor
@using SuperBlazorComponents.Components.SuperDataGrid
```
```csharp
// Program.cs
builder.Services.AddSuperComponents(options =>
{
    options.DataGridSettingsStorageMode = DataGridSettingsStorageMode.LocalStorage;
    options.DefaultSuperIconStyle = SuperIconStyle.Solid;
});
```

---

## 2. Grid-Level Parameter Mapping

### Direct equivalents

| Radzen (`RadzenDataGrid`) | SuperDataGrid | Notes |
|---|---|---|
| `TItem="Order"` | `TItem="Order"` | Identical |
| `AllowSorting="true"` | `AllowSorting="true"` | Identical |
| `AllowFiltering="true"` | `AllowFiltering="true"` | Identical |
| `AllowColumnReorder="true"` | `AllowColumnReorder="true"` | Identical |
| `AllowColumnResize="true"` | `AllowColumnResize="true"` | Identical |
| `Style="height:500px"` | `Height="500px"` | Use dedicated `Height` parameter |
| `EmptyTemplate` | `EmptyTemplate` | Identical usage |
| `HeaderTemplate` | `HeaderTemplate` | Identical usage |
| `FooterTemplate` | `FooterTemplate` | Identical usage |
| `IsLoading="@isLoading"` | *(built-in)* | SuperDataGrid manages loading state internally |
| `RowClick="@OnRowClick"` | `RowClicked="@OnRowClicked"` | Renamed; args differ (see §5) |
| `RowDoubleClick="@OnDblClick"` | `RowDoubleClicked="@OnDblClick"` | Renamed; args differ |
| `CellClick="@OnCellClick"` | `CellClicked="@OnCellClicked"` | Renamed; args differ (see §5) |
| `SelectionMode="DataGridSelectionMode.Single"` | `SelectionMode="SuperDataGridSelectionMode.Single"` | Enum renamed |
| `SelectionMode="DataGridSelectionMode.Multiple"` | `SelectionMode="SuperDataGridSelectionMode.Multiple"` | Default in Super |
| `Value="@selectedItems"` / `ValueChanged` | `SelectionChanged="@OnSelectionChanged"` | See §6 |
| `RowSelect="@OnSelect"` | `SelectionChanged` or `SelectionStateChanged` | See §6 |
| `RowDeselect="@OnDeselect"` | `SelectionStateChanged` | See §6 |
| `EditMode="DataGridEditMode.Single"` | `EditionMode="SuperDataGridEditionMode.Edit"` + `EditOnDoubleClick="true"` | See §7 |
| `RowRender="@OnRowRender"` | `RowClass="@(item => GetRowClass(item))"` | Func-based CSS class only |
| `AllowVirtualization="true"` | *(always on)* | SuperDataGrid is always virtualized |
| `VirtualizationOverscanCount="5"` | `OverscanCount="5"` | Renamed |
| `ColumnWidth="150px"` | *(per-column only)* | Set `Width` on each `DataGridColumn` |

### Frozen columns

| Radzen | SuperDataGrid |
|---|---|
| `Frozen="true"` on `RadzenDataGridColumn` (left freeze) | `FreezeLeftColumns="N"` on `SuperDataGrid` |
| `Frozen="true"` + `FrozenPosition="FrozenColumnPosition.Right"` on column | `FreezeRightColumns="N"` on `SuperDataGrid` |

SuperDataGrid freezes columns by **count** at grid level, not per-column.

### No direct equivalent (Radzen-only features)

| Radzen feature | Recommended workaround in SuperDataGrid |
|---|---|
| `AllowPaging` / `AllowGrouping` / `AllowMultiColumnSorting` | Not supported. Use server-side filtering/paging in `ItemsProvider` |
| `Groups` / `GroupHeaderTemplate` / `GroupFooterTemplate` | Not supported |
| `AllowColumnPicking` | Use `DisplayColumnVisibilityToggle="true"` (built-in toggle button) |
| `Settings` / `SettingsChanged` | Use `GridId` + `DataGridSettingsStorageMode.LocalStorage` or implement `ISuperDataGridSettingsStorage` |
| `LoadSettings` / `SaveSettings` | Implement `ISuperDataGridSettingsStorage` |
| `ShowExpandColumn` / `LoadChildData` / master-detail expand | Not supported natively. Embed child content in `Template` |
| `AllowAlternatingRows` | Use `TableCssClass="table-striped ..."` |
| `GridLines` | Use custom CSS via `TableCssClass` |
| `Density` (Compact/Default) | Use custom CSS via `TableCssClass` / `ContainerCssClass` |
| `Responsive` | Use `ContainerCssClass` with responsive Bootstrap classes |
| `KeyProperty` | Implement `IDataItem` on the model (provides `KeyValue`, `IsSelected`, `RowNumber`) |
| `ExportData` (Excel/CSV) | Not built-in. Handle manually in `HeaderTemplate` with a button |

---

## 3. Data Loading Pattern

This is the **most important difference** between the two grids.

### Radzen — in-memory + server-side with `LoadData`

```razor
<!-- In-memory -->
<RadzenDataGrid Data="@orders" TItem="Order" AllowSorting="true" AllowPaging="true" PageSize="20">
    <Columns>
        <RadzenDataGridColumn TItem="Order" Property="OrderId" Title="ID" />
    </Columns>
</RadzenDataGrid>

<!-- Server-side -->
<RadzenDataGrid Data="@orders" TItem="Order" Count="@count" IsLoading="@isLoading"
                LoadData="@LoadData" AllowSorting="true" AllowFiltering="true" AllowPaging="true" PageSize="20">
    <Columns>
        <RadzenDataGridColumn TItem="Order" Property="OrderId" Title="ID" />
    </Columns>
</RadzenDataGrid>

@code {
    IEnumerable<Order> orders;
    int count;
    bool isLoading;

    async Task LoadData(LoadDataArgs args)
    {
        isLoading = true;
        var result = await orderService.GetOrders(
            skip: args.Skip ?? 0,
            top: args.Top ?? 20,
            orderBy: args.OrderBy,
            filter: args.Filter);
        orders = result.Data;
        count = result.Count;
        isLoading = false;
    }
}
```

### SuperDataGrid — always delegate-based `ItemsProvider`

SuperDataGrid **only** supports delegate-based loading. There is no `Data` binding.
The provider receives paging, sorting, and filter state in one request object.

```razor
<SuperDataGrid TItem="Order" ItemsProvider="LoadOrders" Height="500px">
    <DataGridColumn For="@(o => o.OrderId)" Title="ID" Width="80px" />
    <DataGridColumn For="@(o => o.OrderDate)" Title="Date" FormatString="{0:d}" Width="120px" />
</SuperDataGrid>

@code {
    private async ValueTask<GridItemsProviderResult<Order>> LoadOrders(
        GridItemsProviderRequest<Order> request)
    {
        // request.StartIndex  — replaces args.Skip
        // request.Count       — replaces args.Top
        // request.SortColumn  — replaces args.OrderBy (property name)
        // request.SortDirection — SortDirection.None / Ascending / Descending
        // request.Filters     — replaces args.Filter (list of SuperDataGridFilterInfo)
        // request.CancellationToken

        var result = await orderService.GetOrders(
            skip: request.StartIndex,
            top: request.Count ?? 20,
            sortColumn: request.SortColumn,
            sortDesc: request.SortDirection == SortDirection.Descending,
            filters: request.Filters);

        return GridItemsProviderResult<Order>.From(result.Data, result.TotalCount);
    }
}
```

#### Mapping `LoadDataArgs` → `GridItemsProviderRequest`

| `LoadDataArgs` (Radzen) | `GridItemsProviderRequest<TItem>` (Super) |
|---|---|
| `args.Skip` | `request.StartIndex` |
| `args.Top` | `request.Count` |
| `args.OrderBy` (string, OData) | `request.SortColumn` (property name) + `request.SortDirection` |
| `args.Filter` (OData string) | `request.Filters` (`IEnumerable<SuperDataGridFilterInfo>`) |
| — | `request.CancellationToken` |

#### Reloading the grid

| Radzen | SuperDataGrid |
|---|---|
| `grid.Reload()` | `await grid.ReloadAsync()` |
| `grid.RefreshDataAsync()` | `await grid.ReloadAsync()` |

---

## 4. Column-Level Parameter Mapping

Replace `<RadzenDataGridColumn TItem="Order" ... />` with `<DataGridColumn TItem="Order" ... />`.

> When using `For="@(o => o.Property)"`, the `TItem` type parameter is inferred — you can omit it.

### Direct equivalents

| Radzen (`RadzenDataGridColumn`) | SuperDataGrid (`DataGridColumn`) | Notes |
|---|---|---|
| `Property="OrderId"` | `Property="OrderId"` or `For="@(o => o.OrderId)"` | Prefer `For` |
| `Title="Order ID"` | `Title="Order ID"` | Identical |
| `Width="150px"` | `Width="150px"` | Identical |
| `MinWidth="80px"` | `MinWidth="80px"` | Identical |
| `MaxWidth="300px"` | `MaxWidth="300px"` | Identical |
| `Visible="false"` | `Visible="false"` | Identical |
| `Sortable="false"` | `Sortable="false"` | Identical |
| `Filterable="false"` | `Filterable="false"` | Identical |
| `Resizable="false"` | `Resizable="false"` | Identical |
| `Reorderable="false"` | `Reorderable="false"` | Identical |
| `FormatString="{0:C}"` | `FormatString="{0:C}"` | Identical |
| `TextAlign="TextAlign.Right"` | `TextAlign="SuperTextAlignment.Right"` | Enum renamed |
| `HeaderCssClass="my-header"` | `HeaderCssClass="my-header"` | Identical |
| `CssClass="my-cell"` | `CssClass="my-cell"` | Identical |
| `FilterProperty="SearchName"` | `FilterProperty="SearchName"` | Identical |
| `Template` | `Template` | Context is `TItem` in both |
| `EditTemplate` | `EditTemplate` | Context is `TItem` in both |
| `HeaderTemplate` | `HeaderTemplate` | Identical |
| `FooterTemplate` | `FooterTemplate` | Identical |
| `FilterTemplate` | `FilterTemplate` | Context differs — see §9 |

### No direct equivalent (column-level)

| Radzen feature | SuperDataGrid workaround |
|---|---|
| `Frozen="true"` per column | Use `FreezeLeftColumns` / `FreezeRightColumns` on the grid |
| `CalculatedCssClass="@((col, item) => ...)"` | Use `CellClass="@(item => ...)"` — simpler Func<TItem, string?> |
| `GroupProperty` / `Groupable` | Not supported |
| `FooterTemplate` with `@column.GetSumForColumn()` aggregation | Compute aggregates manually; render in `FooterTemplate` |
| `OrderIndex` (programmatic column order) | Drag-and-drop reorder only; settings persisted via `GridId` |
| `AlwaysVisible` | `AlwaysVisible="true"` — both have this, identical |

---

## 5. Event / Callback Mapping

### Row & cell events

| Radzen | SuperDataGrid | Arg type difference |
|---|---|---|
| `RowClick="@OnRowClick"` | `RowClicked="@OnRowClicked"` | Radzen: `DataGridRowMouseEventArgs<TItem>`; Super: `TItem` directly |
| `RowDoubleClick="@OnDblClick"` | `RowDoubleClicked="@OnDblClick"` | Same as above |
| `CellClick="@OnCellClick"` | `CellClicked="@OnCellClicked"` | Radzen: `DataGridCellMouseEventArgs<TItem>`; Super: `CellClickedEventArgs<TItem>` (`Item` + `Property`) |

**Radzen row click handler:**
```csharp
void OnRowClick(DataGridRowMouseEventArgs<Order> args)
{
    var order = args.Data;
}
```

**SuperDataGrid row click handler:**
```csharp
void OnRowClicked(Order order)
{
    // order is the item directly
}
```

**SuperDataGrid cell click handler:**
```csharp
void OnCellClicked(CellClickedEventArgs<Order> args)
{
    var order = args.Item;
    var property = args.Property; // "OrderId", "OrderDate", etc.
}
```

### Column & data events

| Radzen | SuperDataGrid |
|---|---|
| `Sort="@OnSort"` | *(handled inside `ItemsProvider` via `request.SortColumn` / `request.SortDirection`)* |
| `Filter="@OnFilter"` | *(handled inside `ItemsProvider` via `request.Filters`)* |
| `ColumnReordered` | `ColumnSettingsChanged` |
| `ColumnResized` | `ColumnSettingsChanged` |
| `DataLoaded` / `Render` | `DataLoaded` (`EventCallback<SuperDataGridDataLoadedEventArgs<TItem>>`) |

---

## 6. Selection Mapping

### Radzen selection

```razor
<RadzenDataGrid @ref="grid" Data="@orders" TItem="Order"
                SelectionMode="DataGridSelectionMode.Multiple"
                @bind-Value="@selectedOrders"
                RowSelect="@OnSelect"
                RowDeselect="@OnDeselect">
```
```csharp
IList<Order> selectedOrders = new List<Order>();
void OnSelect(Order order) { }
void OnDeselect(Order order) { }
```

### SuperDataGrid selection

```razor
<SuperDataGrid TItem="Order" ItemsProvider="LoadOrders"
               SelectionMode="SuperDataGridSelectionMode.Multiple"
               SelectionChanged="@OnSelectionChanged"
               @bind-CurrentItem="@currentOrder">
```
```csharp
Order? currentOrder;

void OnSelectionChanged(IEnumerable<Order> selected)
{
    // Full collection of selected items
}

// Programmatic access:
// grid.SelectedItems          — IReadOnlyCollection<TItem>
// grid.SelectedCountTotal     — int
// await grid.SelectItemAsync(item)
// await grid.SelectAllAsync()
// await grid.ClearSelectionAsync()
```

> **Key difference:** Radzen uses `@bind-Value` (IList). SuperDataGrid uses `SelectionChanged`
> callback + `grid.SelectedItems` property. The currently highlighted row is `@bind-CurrentItem`.

---

## 7. Inline Editing Mapping

### Radzen edit pattern (Single row edit mode)

```razor
<RadzenDataGrid @ref="grid" Data="@orders" TItem="Order"
                EditMode="DataGridEditMode.Single"
                RowEdit="@OnRowEdit"
                RowUpdate="@OnRowUpdate"
                RowCreate="@OnRowCreate">
    <Columns>
        <RadzenDataGridColumn TItem="Order" Property="OrderDate" Title="Date">
            <EditTemplate Context="order">
                <RadzenDatePicker @bind-Value="order.OrderDate" />
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Sortable="false" Filterable="false">
            <Template Context="order">
                <RadzenButton Icon="edit" Click="@(() => grid.EditRow(order))" />
            </Template>
            <EditTemplate Context="order">
                <RadzenButton Icon="check" Click="@(() => grid.UpdateRow(order))" />
                <RadzenButton Icon="close" Click="@(() => grid.CancelEditRow(order))" />
            </EditTemplate>
        </RadzenDataGridColumn>
    </Columns>
</RadzenDataGrid>

@code {
    RadzenDataGrid<Order> grid;
    void OnRowEdit(Order order) { }
    void OnRowUpdate(Order order) { /* save */ }
    void OnRowCreate(Order order) { /* insert */ }
}
```

### SuperDataGrid edit pattern

```razor
<SuperDataGrid @ref="grid" TItem="Order" ItemsProvider="LoadOrders"
               EditOnDoubleClick="true"
               RowEditStarted="@OnEditStarted"
               RowEditEnded="@OnEditEnded">
    <DataGridColumn For="@(o => o.OrderDate)" Title="Date" Width="150px">
        <Template>
            @context.OrderDate.ToString("d")
        </Template>
        <EditTemplate>
            <input type="date" class="form-control form-control-sm"
                   value="@context.OrderDate.ToString("yyyy-MM-dd")"
                   @onchange="@(e => context.OrderDate = DateTime.Parse(e.Value?.ToString()!))" />
        </EditTemplate>
    </DataGridColumn>
    <ActionsTemplate>
        @if (grid.IsRowInEditMode(context))
        {
            <button class="btn btn-sm btn-success" @onclick="@(() => SaveRow(context))">✓</button>
            <button class="btn btn-sm btn-secondary" @onclick="@(() => grid.CancelEditAsync(context))">✗</button>
        }
        else
        {
            <button class="btn btn-sm btn-light" @onclick="@(() => grid.BeginEditAsync(context))">✎</button>
        }
    </ActionsTemplate>
</SuperDataGrid>

@code {
    SuperDataGrid<Order> grid = default!;

    async Task SaveRow(Order order)
    {
        // persist order...
        await grid.EndEditAsync(order);
        await grid.ReloadAsync();
    }

    void OnEditStarted(Order order) { }
    void OnEditEnded(Order order) { }
}
```

### Edit method mapping

| Radzen method | SuperDataGrid method |
|---|---|
| `grid.EditRow(item)` | `await grid.BeginEditAsync(item)` |
| `grid.UpdateRow(item)` | `await grid.EndEditAsync(item)` |
| `grid.CancelEditRow(item)` | `await grid.CancelEditAsync(item)` |
| `grid.IsRowInEditMode(item)` | `grid.IsRowInEditMode(item)` |
| `grid.InsertRow(item)` | Not built-in — add item to source, call `ReloadAsync()` |

---

## 8. Full Before / After Example

### Before (Radzen)

```razor
@using Radzen.Blazor

<RadzenDataGrid @ref="grid" Data="@orders" TItem="Order" Count="@count"
                IsLoading="@isLoading" LoadData="@LoadData"
                AllowSorting="true" AllowFiltering="true" AllowPaging="true" PageSize="20"
                AllowColumnReorder="true" AllowColumnResize="true"
                SelectionMode="DataGridSelectionMode.Multiple"
                @bind-Value="@selectedOrders"
                RowClick="@OnRowClick"
                Style="height:500px">
    <Columns>
        <RadzenDataGridColumn TItem="Order" Property="OrderId" Title="ID" Width="80px" />
        <RadzenDataGridColumn TItem="Order" Property="OrderDate" Title="Date"
                              FormatString="{0:d}" Width="120px" />
        <RadzenDataGridColumn TItem="Order" Property="Total" Title="Total"
                              FormatString="{0:C}" TextAlign="TextAlign.Right" Width="120px" />
        <RadzenDataGridColumn TItem="Order" Property="Status" Title="Status" Width="100px">
            <Template Context="order">
                <span class="badge bg-@GetBadgeColor(order.Status)">@order.Status</span>
            </Template>
        </RadzenDataGridColumn>
    </Columns>
</RadzenDataGrid>

@code {
    RadzenDataGrid<Order> grid;
    IEnumerable<Order> orders;
    IList<Order> selectedOrders = new List<Order>();
    int count;
    bool isLoading;

    async Task LoadData(LoadDataArgs args)
    {
        isLoading = true;
        var result = await orderService.GetOrders(
            args.Skip ?? 0, args.Top ?? 20, args.OrderBy, args.Filter);
        orders = result.Data;
        count = result.Count;
        isLoading = false;
    }

    void OnRowClick(DataGridRowMouseEventArgs<Order> args)
    {
        Console.WriteLine($"Clicked: {args.Data.OrderId}");
    }
}
```

### After (SuperDataGrid)

```razor
@using SuperBlazorComponents.Components.SuperDataGrid

<SuperDataGrid @ref="grid" TItem="Order" ItemsProvider="LoadOrders"
               Height="500px"
               AllowSorting="true" AllowFiltering="true"
               AllowColumnReorder="true" AllowColumnResize="true"
               SelectionMode="SuperDataGridSelectionMode.Multiple"
               SelectionChanged="@OnSelectionChanged"
               RowClicked="@OnRowClicked">
    <DataGridColumn For="@(o => o.OrderId)" Title="ID" Width="80px" />
    <DataGridColumn For="@(o => o.OrderDate)" Title="Date"
                    FormatString="{0:d}" Width="120px" />
    <DataGridColumn For="@(o => o.Total)" Title="Total"
                    FormatString="{0:C}" TextAlign="SuperTextAlignment.Right" Width="120px" />
    <DataGridColumn For="@(o => o.Status)" Title="Status" Width="100px">
        <Template>
            <span class="badge bg-@GetBadgeColor(context.Status)">@context.Status</span>
        </Template>
    </DataGridColumn>
</SuperDataGrid>

@code {
    SuperDataGrid<Order> grid = default!;

    async ValueTask<GridItemsProviderResult<Order>> LoadOrders(
        GridItemsProviderRequest<Order> request)
    {
        var result = await orderService.GetOrders(
            skip: request.StartIndex,
            top: request.Count ?? 20,
            sortColumn: request.SortColumn,
            sortDesc: request.SortDirection == SortDirection.Descending,
            filters: request.Filters);

        return GridItemsProviderResult<Order>.From(result.Data, result.TotalCount);
    }

    void OnSelectionChanged(IEnumerable<Order> selected)
    {
        Console.WriteLine($"Selected: {selected.Count()} rows");
    }

    void OnRowClicked(Order order)
    {
        Console.WriteLine($"Clicked: {order.OrderId}");
    }
}
```

---

## 9. Filter System

### Radzen filter approach

Radzen generates OData filter strings automatically and passes them to `LoadData`.
For server-side filtering, you parse `args.Filter` (OData).

### SuperDataGrid filter approach

SuperDataGrid generates strongly-typed `SuperDataGridFilterInfo` objects, one per active filter:

```csharp
public class SuperDataGridFilterInfo
{
    public string PropertyName { get; set; }       // e.g. "CustomerName"
    public string? PropertyValue { get; set; }     // text filter value
    public IReadOnlyList<string> SelectedValues { get; set; }  // enum/multi-select
    public DateTimeOffset? StartDate { get; set; }  // date range start
    public DateTimeOffset? EndDate { get; set; }    // date range end
    public long? FromNumericValue { get; set; }
    public long? ToNumericValue { get; set; }
    public Type PropertyType { get; set; }
    public SuperDataGridFilterOperator Operator { get; set; }
    // Operators: Equals, NotEquals, Contains, StartsWith, EndsWith,
    //            GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual,
    //            Between, NotBetween
}
```

**Applying filters server-side:**
```csharp
async ValueTask<GridItemsProviderResult<Order>> LoadOrders(GridItemsProviderRequest<Order> request)
{
    var query = dbContext.Orders.AsQueryable();

    foreach (var filter in request.Filters)
    {
        if (filter.PropertyName == "CustomerName" && filter.PropertyValue is not null)
            query = query.Where(o => o.CustomerName.Contains(filter.PropertyValue));

        if (filter.PropertyName == "Total" && filter.FromNumericValue.HasValue)
            query = query.Where(o => (long)o.Total >= filter.FromNumericValue.Value);
    }

    if (request.SortColumn is not null)
    {
        query = request.SortDirection == SortDirection.Descending
            ? query.OrderByDescending(request.SortColumn)
            : query.OrderBy(request.SortColumn);
    }

    var total = await query.CountAsync();
    var items = await query.Skip(request.StartIndex).Take(request.Count ?? 20).ToListAsync();
    return GridItemsProviderResult<Order>.From(items, total);
}
```

### Built-in filter components (auto-assigned by property type)

| Type | Auto-assigned filter component |
|---|---|
| `string` | `SuperDataGridStringFilter` (Contains by default) |
| `bool` / `bool?` | `SuperDataGridBooleanFilter` |
| `Enum` | `SuperDataGridEnumFilter` (dialog with checkbox list) |
| `int`, `long`, `decimal`, etc. | `SuperDataGridNumberFilter` (range dialog) |
| `DateTime` / `DateTimeOffset` | `SuperDataGridPeriodFilter` (date range) |

### Custom filter template (per-column)

```razor
<DataGridColumn For="@(o => o.Status)" Title="Status">
    <FilterTemplate>
        <select class="form-select form-select-sm" @onchange="@OnStatusFilterChange">
            <option value="">All</option>
            <option value="Pending">Pending</option>
            <option value="Shipped">Shipped</option>
        </select>
    </FilterTemplate>
</DataGridColumn>
```

> **Note:** In Radzen, `FilterTemplate` receives `RadzenDataGridColumn<TItem>` as context.
> In SuperDataGrid, `FilterTemplate` is a plain `RenderFragment` — use component-level state.

---

## 10. Settings Persistence (Column Width / Order / Visibility)

### Radzen approach
```razor
<RadzenDataGrid Settings="@settings" SettingsChanged="@OnSettingsChanged" ...>
```
```csharp
DataGridSettings settings;
void OnSettingsChanged(DataGridSettings s) { settings = s; /* save */ }
```

### SuperDataGrid approach

**Option A — LocalStorage (automatic):**
```csharp
// Program.cs
builder.Services.AddSuperComponents(options =>
{
    options.DataGridSettingsStorageMode = DataGridSettingsStorageMode.LocalStorage;
});
```
```razor
<SuperDataGrid GridId="orders-grid" ...>
```

**Option B — Custom storage (database, user-scoped):**
```csharp
public class MyGridSettingsStorage : ISuperDataGridSettingsStorage
{
    public async Task<SuperDataGridSettings?> LoadAsync(string gridId)
        => await db.GridSettings.FindAsync(gridId) is { } s
            ? JsonSerializer.Deserialize<SuperDataGridSettings>(s.Json)
            : null;

    public async Task SaveAsync(string gridId, SuperDataGridSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        await db.GridSettings.UpsertAsync(gridId, json);
    }
}

// Program.cs
builder.Services.AddSingleton<ISuperDataGridSettingsStorage, MyGridSettingsStorage>();
```

---

## 11. IDataItem Interface (Optional but Recommended)

Radzen uses `KeyProperty` to track selection across pages. SuperDataGrid uses `IDataItem`:

```csharp
public class Order : IDataItem
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal Total { get; set; }

    // IDataItem — required for cross-page selection tracking
    public object KeyValue => OrderId;
    public bool IsSelected { get; set; }
    public int RowNumber { get; set; }
}
```

When `IDataItem` is implemented:
- `KeyValue` is used to identify rows across virtualization pages
- `IsSelected` is automatically toggled by the grid
- `RowNumber` is automatically set (used when `DisplayRowNumberColumn="true"`)

---

## 12. Public API Method Mapping

| Radzen method | SuperDataGrid method |
|---|---|
| `grid.Reload()` | `await grid.ReloadAsync()` |
| `grid.RefreshDataAsync()` | `await grid.ReloadAsync()` |
| `grid.EditRow(item)` | `await grid.BeginEditAsync(item)` |
| `grid.UpdateRow(item)` | `await grid.EndEditAsync(item)` |
| `grid.CancelEditRow(item)` | `await grid.CancelEditAsync(item)` |
| `grid.SelectRow(item, true)` | `await grid.SelectRow(item, true)` |
| `grid.OrderBy("Property")` | *(not exposed; sort handled in `ItemsProvider`)* |
| `grid.IsRowInEditMode(item)` | `grid.IsRowInEditMode(item)` |
| `grid.ColumnsCollection` | `grid.ColumnsCollection` |
| `grid.Reset(true, true)` | `await grid.ResetColumnSettingsAsync()` |

---

## 13. Step-by-Step Migration Checklist

1. **Replace the using** `@using Radzen.Blazor` → `@using SuperBlazorComponents.Components.SuperDataGrid`
2. **Replace the component tag** `<RadzenDataGrid` → `<SuperDataGrid`
3. **Replace columns container**: `<Columns>` wrapper is removed — `DataGridColumn` components go directly as `ChildContent`
4. **Replace column tags** `<RadzenDataGridColumn` → `<DataGridColumn` and add `For="@(x => x.Property)"` where possible
5. **Migrate data loading** from `Data="@list" + LoadData="@Handler"` → `ItemsProvider="@ProviderMethod"` using `GridItemsProviderRequest`
6. **Remove paging parameters** (`AllowPaging`, `PageSize`, etc.) — paging is implicit via virtualization
7. **Rename events**: `RowClick` → `RowClicked`, `RowDoubleClick` → `RowDoubleClicked`, `CellClick` → `CellClicked`
8. **Update event handler signatures** (Radzen passes event args; SuperDataGrid passes `TItem` directly for row events)
9. **Replace selection binding** `@bind-Value` → `SelectionChanged` callback + `@bind-CurrentItem`
10. **Replace frozen columns** (per-column `Frozen`) → grid-level `FreezeLeftColumns` / `FreezeRightColumns`
11. **Replace `RowRender`** → `RowClass="@(item => ...)"` Func
12. **Replace `CalculatedCssClass`** → `CellClass="@(item => ...)"`
13. **Replace `TextAlign.Right`** → `SuperTextAlignment.Right`
14. **Add `GridId`** for settings persistence if needed
15. **Implement `IDataItem`** on the model if cross-page selection is required
16. **Register services** in `Program.cs` using `AddSuperComponents(...)`
