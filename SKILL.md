# SuperBlazorComponents — Copilot Skill

> This file is intended to be used as a GitHub Copilot skill (or context drop) by applications consuming **SuperBlazorComponents**. It gives Copilot a precise, structured overview of every component, its key parameters, canonical usage patterns, and links to the full documentation.

---

## Library at a Glance

**SuperBlazorComponents** is a high-performance, open-source Blazor component library designed for admin and line-of-business applications. It is built on **Bootstrap 5.3** with zero third-party JS dependencies (except Google Charts).

| | |
|---|---|
| **NuGet** | `SuperBlazorComponents` |
| **Target framework** | .NET 10 / Blazor Server & WebAssembly |
| **Theme** | Bootstrap 5.3 — dark/light supported out of the box |

### Installation

```bash
dotnet add package SuperBlazorComponents
```

### Service registration (`Program.cs`)

```csharp
builder.Services.AddSuperComponents();
```

---

## Component Index

| Component | Category | Short description | Full docs |
|---|---|---|---|
| [SuperDataGrid](#superdatagrid) | Data | Virtualized grid — frozen cols/rows, hierarchical lazy-loading, inline editing, filtering, sorting, selection, persistence | [SUPERDATAGRID.md](SUPERDATAGRID.md) |
| [SuperLayout](#superlayout) | Layout | Responsive app shell — header, sidebar, body, footer, chat panel | [SUPERLAYOUT.md](SUPERLAYOUT.md) |
| [SuperTabs](#supertabs) | Navigation | Service-driven tabs — lazy-load, persistence, badges, closable | [SUPERTABS.md](SUPERTABS.md) |
| [SuperSplitter](#supersplitter) | Layout | Resizable split panes — horizontal/vertical, state persistence | [SuperSplitter.md](src/SuperBlazorComponents/Components/SuperSplitter/SuperSplitter.md) |
| [SuperDateRangePicker](#superdaterangepicker) | Forms | Calendar date range picker with presets | [SUPERDATERANGEPICKER.md](SUPERDATERANGEPICKER.md) |
| [SuperColorPicker](#supercolorpicker) | Forms | Inline HSV color picker with alpha channel | [SUPERCOLORPICKER.md](SUPERCOLORPICKER.md) |
| [SuperDropDownColorPicker](#supercolorpicker) | Forms | Dropdown variant of SuperColorPicker | [SUPERCOLORPICKER.md](SUPERCOLORPICKER.md) |
| [SuperHtmlEditor](#superhtmleditor) | Forms | WYSIWYG HTML editor with Monaco source view | [SUPERHTMLEDITOR.md](SUPERHTMLEDITOR.md) |
| [SuperButtons](#superbuttons) | Actions | Buttons, split buttons, toggle, confirmation | [SUPERBUTTONS.md](SUPERBUTTONS.md) |
| [SuperTriStateCheckbox](#supertristatecheckbox) | Forms | Nullable boolean checkbox (`true`/`false`/`null`) | [SuperTriStateCheckbox.md](src/SuperBlazorComponents/Components/SuperTriStateCheckbox/SuperTriStateCheckbox.md) |
| [SuperTooltip](#supertooltip) | UX | Tooltips — text, HTML, Markdown, positions, trigger | [SuperTooltip.md](src/SuperBlazorComponents/Components/Tooltips/SuperTooltip.md) |
| [SuperDialog / SuperConfirmDialog](#superdialog--superconfirmdialog) | Dialogs | Service-driven modal dialogs returning typed results | [SUPERDIALOGS.md](SUPERDIALOGS.md) |
| [SuperNotifications](#supernotifications) | UX | Toast notifications with severity levels and auto-dismiss | [SUPERNOTIFICATIONS.md](SUPERNOTIFICATIONS.md) |
| [SuperBreadCrumb](#superbreadcrumb) | Navigation | Bootstrap breadcrumb with back-navigation | [SUPERBREADCRUMB.md](SUPERBREADCRUMB.md) |
| [SuperMenuItem](#supermenuitem) | Navigation | Sidebar menu item with icons, badges, nested submenus | [SUPERMENUITEM.md](SUPERMENUITEM.md) |
| [ThemeToggle](#themetoggle) | Themes | Dark/light toggle with system preference detection | [THEMETOGGLE.md](THEMETOGGLE.md) |
| [Google Charts](#google-charts) | Charts | Combo charts, pie charts, pure-SVG time series | [GOOGLECHARTS.md](GOOGLECHARTS.md) |

---

## SuperDataGrid

> [Full docs → SUPERDATAGRID.md](SUPERDATAGRID.md)

Virtualized, feature-rich data grid for large datasets.

**Namespace:** `SuperBlazorComponents.Components.SuperDataGrid`

**Key features:** frozen columns & rows, hierarchical lazy-loading rows, column reordering & resizing, filtering, sorting, inline editing, row selection, settings persistence (localStorage or custom backend).

### Minimal example

```razor
@using SuperBlazorComponents.Components.SuperDataGrid

<SuperDataGrid TItem="Product"
               ItemsProvider="LoadProducts"
               Height="500px"
               AllowSorting="true"
               AllowFiltering="true"
               FreezeHeader="true"
               GridId="products-grid">
    <DataGridColumn Title="Name"     For="@(c => c.Name)" />
    <DataGridColumn Title="Price"    For="@(c => c.Price)"    Width="120" />
    <DataGridColumn Title="Category" For="@(c => c.Category)" Width="150" />
</SuperDataGrid>

@code {
    private async ValueTask<GridItemsProviderResult<Product>> LoadProducts(
        GridItemsProviderRequest<Product> request)
    {
        // apply request.SortColumn, request.SortDescending, request.Filters
        var result = await _service.GetPageAsync(request.StartIndex, request.Count);
        return GridItemsProviderResult.From(result.Items, result.TotalCount);
    }
}
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `TItem` | generic | Model type |
| `ItemsProvider` | `GridItemsProvider<TItem>` | Async data delegate |
| `Height` | `string` | CSS height of the grid |
| `GridId` | `string?` | Unique ID for settings persistence |
| `AllowSorting` | `bool` | Enable column sorting |
| `AllowFiltering` | `bool` | Enable column filters |
| `FreezeHeader` | `bool` | Sticky header row |
| `FrozenColumns` | `int` | Number of left-frozen columns |
| `FrozenRightColumns` | `int` | Number of right-frozen columns |
| `SelectionMode` | `SuperDataGridSelectionMode` | `None`, `Single`, `Multiple` |
| `AllowInlineEditing` | `bool` | Enable double-click inline row editing |
| `Hierarchical` | `bool` | Enable tree rows (expand/collapse children) |
| `DefaultSettingsName` | `string?` | Load a named preset settings profile |

### Hierarchical (tree) rows

```razor
<SuperDataGrid TItem="Category" Hierarchical="true" ItemsProvider="LoadNodes" ...>
    ...
</SuperDataGrid>

@code {
    private async ValueTask<GridItemsProviderResult<Category>> LoadNodes(
        GridItemsProviderRequest<Category> request)
    {
        if (request.IsHierarchyRequest)
        {
            // load children of request.HierarchyParent
            var children = await _service.GetChildrenAsync(request.HierarchyParent!.Id);
            return GridItemsProviderResult.From(children, children.Count);
        }
        // load root items
        var roots = await _service.GetRootsAsync();
        return GridItemsProviderResult.From(roots, roots.Count);
    }
}
```

---

## SuperLayout

> [Full docs → SUPERLAYOUT.md](SUPERLAYOUT.md)

Responsive application shell based on CSS Grid. Coordinates `SuperHeader`, `SuperSidebar`, `SuperBody`, `SuperFooter`, `SuperChat`, and `SuperChatButton` through a cascading parameter.

**Namespace:** `SuperBlazorComponents.Components.SuperLayout`

### Minimal example

```razor
@using SuperBlazorComponents.Components.SuperLayout

<SuperLayout>
    <HeaderContent>
        <span class="navbar-brand">My App</span>
    </HeaderContent>
    <SidebarContent>
        <SuperMenuItem Href="/" Icon="fa-house" Text="Home" Match="NavLinkMatch.All" />
        <SuperMenuItem Href="/orders" Icon="fa-receipt" Text="Orders" />
    </SidebarContent>
    <BodyContent>
        @Body
    </BodyContent>
    <FooterContent>
        <span>© 2026 My Company</span>
    </FooterContent>
</SuperLayout>
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `SidebarDefaultState` | `SidebarState` | `Expanded`, `Collapsed`, `Hidden` |
| `SidebarExpandedWidth` | `string` | CSS width when expanded (default `250px`) |
| `SidebarCollapsedWidth` | `string` | CSS width when collapsed (icon-only) |
| `StickyHeader` | `bool` | Whether the header sticks on scroll |
| `StickyFooter` | `bool` | Whether the footer sticks on scroll |
| `EnableChat` | `bool` | Show the chat panel |
| `ChatDefaultWidth` | `string` | CSS width of the chat panel |

### Programmatic sidebar control

```razor
@code {
    [CascadingParameter] SuperLayout Layout { get; set; } = default!;

    void CollapseMenu() => Layout.CollapseSidebar();
    void ExpandMenu()   => Layout.ExpandSidebar();
    void ToggleMenu()   => Layout.ToggleSidebar();
}
```

---

## SuperTabs

> [Full docs → SUPERTABS.md](SUPERTABS.md)

Service-driven tabbed interface supporting both declarative and programmatic tab definitions.

**Namespace:** `SuperBlazorComponents.Components.SuperTabs`

### Declarative tabs

```razor
@using SuperBlazorComponents.Components.SuperTabs

<SuperTabs>
    <Tabs>
        <TabItem Title="Overview" Icon="fa-chart-line">
            <OverviewPanel />
        </TabItem>
        <TabItem Title="Details" Icon="fa-list">
            <DetailsPanel />
        </TabItem>
    </Tabs>
</SuperTabs>
```

### Programmatic tabs (dynamic)

```razor
<SuperTabs Tabs="_tabs" @bind-SelectedIndex="_selectedIndex" />

@code {
    private List<SuperTabItem> _tabs =
    [
        new SuperTabItem { Title = "Home",    Icon = "fa-house",   ComponentType = typeof(HomePage) },
        new SuperTabItem { Title = "Reports", Icon = "fa-chart-bar", ComponentType = typeof(ReportsPage) },
    ];
    private int _selectedIndex;
}
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `Tabs` | `List<SuperTabItem>?` | Programmatic tab list |
| `SelectedIndex` | `int` | Two-way binding for active tab |
| `TabPosition` | `SuperTabPosition` | `Top`, `TopRight`, `Bottom`, `BottomRight`, `Left`, `Right` |
| `PersistenceKey` | `string?` | localStorage key for tab persistence |
| `PersistInUrl` | `bool` | Persist active tab in the URL query string |
| `AllowAddTab` | `bool` | Show an add-tab button in the toolbar |
| `AllowCloseTab` | `bool` | Show close buttons on tabs |

### Service API

```razor
@inject SuperTabsService TabsService

@code {
    void OpenCustomerTab(int id)
        => TabsService.AddTab("customers", new SuperTabItem
        {
            Title         = $"Customer #{id}",
            Icon          = "fa-user",
            ComponentType = typeof(CustomerDetail),
            Parameters    = new() { ["CustomerId"] = id },
            Closable      = true,
        });
}
```

---

## SuperSplitter

> [Full docs → SuperSplitter.md](src/SuperBlazorComponents/Components/SuperSplitter/SuperSplitter.md)

Two-pane resizable layout with drag handle. Supports horizontal and vertical orientations and localStorage-based size persistence.

**Namespace:** `SuperBlazorComponents.Components.SuperSplitter`

### Example

```razor
@using SuperBlazorComponents.Components.SuperSplitter

<SuperSplitter Orientation="SuperSplitterOrientation.Horizontal"
               FirstPaneSize="30"
               PersistenceKey="my-splitter">
    <SplitPane>
        <TreePanel />
    </SplitPane>
    <SplitPane>
        <DetailPanel />
    </SplitPane>
</SuperSplitter>
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `Orientation` | `SuperSplitterOrientation` | `Horizontal` or `Vertical` |
| `FirstPaneSize` | `double` | Initial size of the first pane in percent |
| `MinPaneSize` | `double` | Minimum pane size in percent |
| `PersistenceKey` | `string?` | localStorage key for size persistence |

---

## SuperDateRangePicker

> [Full docs → SUPERDATERANGEPICKER.md](SUPERDATERANGEPICKER.md)

Calendar-based date range picker with 18 built-in presets and a floating panel.

**Namespace:** `SuperBlazorComponents.Components.SuperDateRange`

### Example

```razor
@using SuperBlazorComponents.Components.SuperDateRange

<SuperDateRangePicker @bind-Value="_period" />

@code {
    private SuperDateRangeSelection _period = SuperDateRangeSelection.LastThirtyDays();
}
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `Value` | `SuperDateRangeSelection` | Two-way bound selection |
| `DisableFuture` | `bool` | Prevent selecting future dates |
| `ShowWeekNumbers` | `bool` | Display ISO week numbers |
| `Placeholder` | `string?` | Trigger button placeholder text |

### Open via dialog

```csharp
@inject SuperDialogService DialogService

var result = await DialogService.OpenDateRangeDialogAsync(_period);
if (result is not null)
    _period = result;
```

---

## SuperColorPicker

> [Full docs → SUPERCOLORPICKER.md](SUPERCOLORPICKER.md)

Inline HSV color picker (`SuperColorPicker`) and its compact dropdown variant (`SuperDropDownColorPicker`).

**Namespace:** `SuperBlazorComponents.Components.SuperColorPicker`

### Inline picker

```razor
@using SuperBlazorComponents.Components.SuperColorPicker

<SuperColorPicker @bind-Value="_color" ShowAlpha="true" />

@code {
    private string _color = "#3498db";
}
```

### Dropdown picker

```razor
<SuperDropDownColorPicker @bind-Value="_color" />
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `Value` | `string` | HEX color value (e.g. `#RRGGBB` or `#RRGGBBAA`) |
| `ShowAlpha` | `bool` | Show the alpha channel slider |
| `Disabled` | `bool` | Disable the picker |
| `ValueExpression` | `Expression<Func<string>>?` | For EditForm validation |

---

## SuperHtmlEditor

> [Full docs → SUPERHTMLEDITOR.md](SUPERHTMLEDITOR.md)

WYSIWYG HTML editor based on a native `contenteditable` div. Includes a full toolbar (font, size, bold/italic/underline, colors, alignment, lists) and a lazy-loaded Monaco Editor for HTML source editing.

**Namespace:** `SuperBlazorComponents.Components.SuperHtmlEditor`

### Example

```razor
@using SuperBlazorComponents.Components.SuperHtmlEditor

<SuperHtmlEditor @bind-Value="_html"
                 MinHeight="200px"
                 MaxHeight="600px" />

@code {
    private string _html = "<p>Hello <strong>world</strong></p>";
}
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `Value` | `string?` | HTML content (two-way bound) |
| `MinHeight` | `string` | Minimum editor height |
| `MaxHeight` | `string` | Maximum editor height |
| `MonacoHeight` | `string` | Height of the Monaco source panel |
| `Disabled` | `bool` | Disable all editing |
| `Placeholder` | `string?` | Placeholder text |
| `ValueExpression` | `Expression<Func<string?>>?` | For EditForm validation |

---

## SuperButtons

> [Full docs → SUPERBUTTONS.md](SUPERBUTTONS.md)

A complete family of Bootstrap-powered button components.

**Namespace:** `SuperBlazorComponents.Components.Buttons`

### Component family

| Component | Description |
|---|---|
| `SuperButton` | Standard button with busy state, badge, popover |
| `SuperLinkButton` | Navigation button rendered as `<a>` |
| `SuperToggleButton` | Stateful pressed/unpressed toggle |
| `SuperSplitButton` | Dropdown button with `SuperSplitButtonItem` / `SuperSplitLinkItem` / `SuperSplitDivider` |
| `SuperConfirmationButton` | Button that shows a confirmation dialog before firing the click |
| `SuperButtonGroup` | Groups multiple buttons in a Bootstrap `btn-group` |

### Examples

```razor
@using SuperBlazorComponents.Components.Buttons

<!-- Standard -->
<SuperButton Text="Save"
             Icon="fa-floppy-disk"
             Style="SuperButtonStyle.Primary"
             Click="OnSaveAsync" />

<!-- Busy state -->
<SuperButton Text="Export"
             BusyText="Exporting…"
             Click="OnExportAsync" />

<!-- Confirmation -->
<SuperConfirmationButton Text="Delete"
                         Icon="fa-trash"
                         Style="SuperButtonStyle.Danger"
                         ConfirmTitle="Delete record?"
                         ConfirmMessage="This action cannot be undone."
                         Click="OnDeleteAsync" />

<!-- Split button (dropdown) -->
<SuperSplitButton Text="Actions" Style="SuperButtonStyle.Secondary">
    <SuperSplitButtonItem Text="Edit"    Icon="fa-pen"   Click="OnEdit" />
    <SuperSplitDivider />
    <SuperSplitButtonItem Text="Archive" Icon="fa-box"   Click="OnArchive" />
</SuperSplitButton>
```

### Key shared parameters

| Parameter | Type | Description |
|---|---|---|
| `Text` | `string?` | Button label |
| `Icon` | `string?` | Font Awesome class (e.g. `fa-save`) |
| `Style` | `SuperButtonStyle` | `Primary`, `Secondary`, `Danger`, `Success`, `Warning`, `Info`, `Light`, `Dark`, `Link` |
| `Size` | `SuperButtonSize` | `Default`, `SuperSmall`, `Small`, `Large` |
| `Disabled` | `bool` | Disable the button |
| `BadgeText` | `string?` | Optional badge text |

---

## SuperTriStateCheckbox

> [Full docs → SuperTriStateCheckbox.md](src/SuperBlazorComponents/Components/SuperTriStateCheckbox/SuperTriStateCheckbox.md)

Bootstrap-friendly checkbox for `bool?` values. Cycles `null → true → false → null` on each click.

**Namespace:** `SuperBlazorComponents.Components.SuperTriStateCheckbox`

### Example

```razor
@using SuperBlazorComponents.Components.SuperTriStateCheckbox

<SuperTriStateCheckbox @bind-Value="_isValid"
                       Label="Validated"
                       HelpText="Click to cycle through null, true, and false." />

@code {
    private bool? _isValid;
}
```

---

## SuperTooltip

> [Full docs → SuperTooltip.md](src/SuperBlazorComponents/Components/Tooltips/SuperTooltip.md)

Bootstrap tooltip for any Blazor element or raw HTML element. Supports plain text, HTML, and Markdown content.

**Namespace:** `SuperBlazorComponents.Components.Tooltips`

**Prerequisite:** `bootstrap.bundle.min.js` must be loaded in the host project.

### Examples

```razor
@using SuperBlazorComponents.Components.Tooltips

<!-- Text tooltip -->
<SuperTooltip Text="Save changes">
    <button class="btn btn-primary">Save</button>
</SuperTooltip>

<!-- Markdown tooltip -->
<SuperTooltip Markdown="@_helpText" Position="SuperTooltipPosition.Right">
    <span class="fa fa-circle-info text-info" />
</SuperTooltip>

@code {
    private string _helpText = "**Important:** fill all required fields.\n\n- Field A\n- Field B";
}
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `Text` | `string?` | Plain text content |
| `HtmlContent` | `string?` | Raw HTML content (trusted source only) |
| `Markdown` | `string?` | Markdown content — highest priority |
| `Position` | `SuperTooltipPosition` | `Top`, `Bottom`, `Left`, `Right`, `Auto` |
| `Trigger` | `SuperTooltipTrigger` | `Hover`, `Click`, `Focus`, `Manual` |
| `Delay` | `int` | Open delay in milliseconds |
| `Duration` | `int` | Auto-close duration (`0` = stay open) |
| `CloseOnDocumentClick` | `bool` | Close on any click outside |
| `Disabled` | `bool` | Disable the tooltip |

---

## SuperDialog / SuperConfirmDialog

> [Full docs → SUPERDIALOGS.md](SUPERDIALOGS.md)

Service-driven modal dialog system. `SuperConfirmDialog` returns `bool`; `SuperDialog` hosts an arbitrary Blazor component and returns whatever the component passes back.

**Namespace:** `SuperBlazorComponents.Components.Dialogs` / `SuperBlazorComponents.Services`

### Setup — place hosts once in `MainLayout.razor`

```razor
<SuperDialog />
<SuperConfirmDialog />
```

### Confirmation dialog

```razor
@inject SuperDialogService DialogService

@code {
    private async Task DeleteAsync()
    {
        bool confirmed = await DialogService.ConfirmAsync(new ConfirmOptions
        {
            Title   = "Delete record?",
            Message = "This action cannot be undone.",
            ConfirmText = "Delete",
            ConfirmStyle = "btn-danger",
        });

        if (confirmed)
        {
            await _service.DeleteAsync(_id);
        }
    }
}
```

### Component dialog (returns typed result)

```razor
@code {
    private async Task EditAsync()
    {
        var result = await DialogService.OpenAsync<CustomerEditDialog>(
            new DialogOptions { Title = "Edit Customer", Size = "lg" },
            new() { ["CustomerId"] = _customerId });

        if (result is CustomerDto updated)
        {
            _customer = updated;
        }
    }
}
```

Inside `CustomerEditDialog.razor`, close and return a value with:

```csharp
[Parameter] public EventCallback<dynamic?> Close { get; set; }

await Close.InvokeAsync(_editedCustomer);
```

---

## SuperNotifications

> [Full docs → SUPERNOTIFICATIONS.md](SUPERNOTIFICATIONS.md)

Toast notification system driven by `SuperNotificationService`.

**Namespace:** `SuperBlazorComponents.Components.Notifications` / `SuperBlazorComponents.Services`

### Setup — place the host once in `MainLayout.razor`

```razor
<SuperNotification Position="NotificationPosition.TopRight" />
```

### Sending notifications

```razor
@inject SuperNotificationService NotificationService

@code {
    void NotifySuccess() => NotificationService.Notify(new NotificationMessage
    {
        Severity = NotificationSeverity.Success,
        Summary  = "Record saved",
        Detail   = "Changes have been persisted.",
        Duration = 4000,
    });

    void NotifyError(string message) => NotificationService.Notify(new NotificationMessage
    {
        Severity = NotificationSeverity.Error,
        Summary  = "Error",
        Detail   = message,
        Duration = 0,  // stays until closed manually
    });
}
```

### Severity levels

| Value | Description |
|---|---|
| `Info` | Informational (blue) |
| `Success` | Operation succeeded (green) |
| `Warning` | Non-blocking warning (yellow) |
| `Error` | Operation failed (red) |

---

## SuperBreadCrumb

> [Full docs → SUPERBREADCRUMB.md](SUPERBREADCRUMB.md)

Bootstrap 5 breadcrumb with optional Font Awesome icons and a back-navigation item.

**Namespace:** `SuperBlazorComponents.Components`

### Example

```razor
@using SuperBlazorComponents.Components

<SuperBreadCrumb>
    <SuperBreadCrumbItem Path="/"          Text="Home"      Icon="fa-solid fa-house" />
    <SuperBreadCrumbItem Path="/customers" Text="Customers" />
    <SuperBreadCrumbItem Text="Acme Corp"  IsActive="true" />
</SuperBreadCrumb>

<!-- With back navigation -->
<SuperBreadCrumb>
    <SuperBackBreadcrumbItem Text="Back" />
    <SuperBreadCrumbItem Text="Detail" IsActive="true" />
</SuperBreadCrumb>
```

---

## SuperMenuItem

> [Full docs → SUPERMENUITEM.md](SUPERMENUITEM.md)

Sidebar navigation item for `SuperLayout`. Automatically switches to icon-only mode when the sidebar collapses.

**Namespace:** `SuperBlazorComponents.Components`

### Example

```razor
@using SuperBlazorComponents.Components

<SuperMenuItem Href="/dashboard" Icon="fa-gauge"    Text="Dashboard" Match="NavLinkMatch.All" />
<SuperMenuItem Href="/customers" Icon="fa-users"    Text="Customers" />
<SuperMenuItem Href="/orders"    Icon="fa-receipt"  Text="Orders" BadgeText="5" BadgeCssClass="bg-danger" />

<!-- Nested submenu -->
<SuperMenuItem Icon="fa-cog" Text="Settings">
    <Items>
        <SuperMenuItem Href="/settings/users"   Icon="fa-user-gear" Text="Users" />
        <SuperMenuItem Href="/settings/company" Icon="fa-building"  Text="Company" />
    </Items>
</SuperMenuItem>
```

### Key parameters

| Parameter | Type | Description |
|---|---|---|
| `Href` | `string?` | Navigation URL |
| `Text` | `string?` | Label text |
| `Icon` | `string?` | Font Awesome class |
| `BadgeText` | `string?` | Badge content |
| `BadgeCssClass` | `string?` | Bootstrap badge class (e.g. `bg-danger`) |
| `Match` | `NavLinkMatch` | `Prefix` (default) or `All` |
| `PolicyName` | `string?` | Authorization policy — wraps in `AuthorizeView` |
| `Theme` | `string?` | Adds `super-theme-{Theme}` CSS class |

---

## ThemeToggle

> [Full docs → THEMETOGGLE.md](THEMETOGGLE.md)

Zero-parameter button that toggles between Bootstrap's `dark` and `light` themes. Persists the choice in `localStorage` and falls back to the OS `prefers-color-scheme` on first load.

**Namespace:** `SuperBlazorComponents.Components.Themes`

### Example

```razor
@using SuperBlazorComponents.Components.Themes

<SuperLayout>
    <HeaderRightContent>
        <ThemeToggle />
    </HeaderRightContent>
</SuperLayout>
```

---

## Google Charts

> [Full docs → GOOGLECHARTS.md](GOOGLECHARTS.md)

Three chart components accepting strongly-typed C# data — no JSON required.

**Namespace:** `SuperBlazorComponents.Components.GoogleCharts`

| Component | Renderer | Best for |
|---|---|---|
| `GoogleComboChart` | Google Charts JS | Multi-series line/area/bar/column/scatter with dual axes |
| `GooglePieChart` | Google Charts JS | Distributions — pie, donut, 3D |
| `TimeSeriesChart` | Pure SVG (zero JS) | Single-series time series with month markers and weekend bands |

### Setup for `GoogleComboChart` and `GooglePieChart`

Add to `App.razor` (or `_Host.cshtml`):

```html
<script src="https://www.gstatic.com/charts/loader.js"></script>
<script src="_content/SuperBlazorComponents/js/google-charts-interop.js"></script>
```

`TimeSeriesChart` has **no** JS dependency.

### GoogleComboChart example

```razor
@using SuperBlazorComponents.Components.GoogleCharts

<GoogleComboChart Title="Sales vs Target"
                  Data="_chartData"
                  Height="400px" />

@code {
    private List<ComboChartRow> _chartData =
    [
        new("Jan", 120, 100),
        new("Feb", 150, 130),
        new("Mar", 180, 160),
    ];
}
```

### TimeSeriesChart example (no JS)

```razor
<TimeSeriesChart Title="Daily visitors"
                 Data="_series"
                 Height="250px" />

@code {
    private List<TimeSeriesPoint> _series =
    [
        new(new DateOnly(2026, 1, 1), 420),
        new(new DateOnly(2026, 1, 2), 390),
        // ...
    ];
}
```

---

## Migration guides

| From | To | Guide |
|---|---|---|
| Radzen DataGrid | SuperDataGrid | [RadzenDatagridToSuperDataGridSkill.md](RadzenDatagridToSuperDataGridSkill.md) |
| Radzen Buttons | SuperButtons | [RadzenButtonsToSuperButtonsSkill.md](RadzenButtonsToSuperButtonsSkill.md) |

---

## Tips & best practices

- **Single registration** — call `builder.Services.AddSuperComponents()` once. It registers all services (`SuperDialogService`, `SuperNotificationService`, `SuperTabsService`, etc.).
- **Host placement** — `<SuperDialog />`, `<SuperConfirmDialog />`, and `<SuperNotification />` must each be placed exactly once, typically in `MainLayout.razor`.
- **Bootstrap dependency** — all components require Bootstrap 5.3 CSS. For `SuperTooltip` and standard Bootstrap JS features also include `bootstrap.bundle.min.js`.
- **Font Awesome** — `SuperMenuItem`, `SuperButtons`, and `SuperBreadCrumb` use Font Awesome icon classes. Add the Font Awesome CDN or package to your project.
- **Dark mode** — wrap the `<html>` element with `data-bs-theme="dark"` or use `<ThemeToggle />`. All components respond automatically.
- **Settings persistence** — `SuperDataGrid` and `SuperSplitter` use `localStorage` by default. Implement `ISuperDataGridSettingsStorage` to store grid settings server-side.
- **Localization** — all user-facing strings are localized via `IStringLocalizer`. Add resource files under your project's `Localization/` folder and register `AddLocalization()` to override defaults.
