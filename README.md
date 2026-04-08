# 🚀 SuperBlazorComponents

[![NuGet](https://img.shields.io/nuget/v/SuperBlazorComponents)](https://www.nuget.org/packages/SuperBlazorComponents)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-purple)](https://dotnet.microsoft.com)

**High-performance, open-source Blazor component library** featuring a powerful virtual data grid, responsive layouts, tabs, charts, dialogs, notifications, and more — all built with Bootstrap 5.3 and zero third-party JS dependencies (except Google Charts).

---

## ✨ Features

- **Virtual Data Grid** — frozen columns/rows, column reordering & resizing, filtering, sorting, inline editing, row selection, and settings persistence
- **Responsive Layout System** — header, sidebar, body, footer, and chat panel with collapsible sidebar
- **Tabbed Interface** — dynamic tabs with badges, closable tabs, and service-driven tab management
- **Resizable Split Panels** — horizontal/vertical splitter with state persistence
- **Google Charts Integration** — combo charts, pie charts, and SVG time series charts
- **Dialog & Notification System** — modal dialogs, confirmation dialogs, and toast notifications
- **Date Range Picker** — calendar-based date range selection with presets
- **Rich Button Collection** — buttons, split buttons, toggle buttons, link buttons, and confirmation buttons
- **Breadcrumb Navigation** — customizable breadcrumbs with back navigation support
- **Font Awesome Icon Support** — configurable icon styles (Solid, Regular, Light, Thin, Duotone)

---

## 📦 Installation

```bash
dotnet add package SuperBlazorComponents
```

## 🔧 Setup

Register the services in your `Program.cs`:

```csharp
builder.Services.AddSuperComponents(options =>
{
    options.DataGridSettingsStorageMode = DataGridSettingsStorageMode.LocalStorage;
    options.DefaultSuperIconeStyle = SuperIconStyle.Solid;
});
```

---

## 📁 Component Reference

### 📊 SuperDataGrid (`Components/SuperDataGrid`)

A high-performance virtualized data grid component with enterprise-grade features.

> **📖 [Full SuperDataGrid Documentation](SUPERDATAGRID.md)** — Complete API reference with 20 detailed usage examples (sorting, filtering, editing, selection, frozen columns, custom templates, settings persistence, and more).

| Component / Class | Description |
|---|---|
| `SuperDataGrid<TItem>` | Main data grid component with virtualization, frozen columns/rows, and keyboard navigation |
| `DataGridColumn<TItem>` | Column definition supporting templates, sorting, filtering, resizing, and reordering |
| `GridItemsProvider<TItem>` | Delegate for providing data items to the grid (similar to QuickGrid's `ItemsProvider`) |
| `GridItemsProviderRequest<TItem>` | Request object sent to the items provider with paging, sorting, and filter info |
| `GridItemsProviderResult<TItem>` | Result returned by the items provider containing items and total count |
| `ISuperDataGridSettingsStorage` | Interface for persisting grid settings (column order, widths, visibility) |
| `SuperDataGridSettingsLocalStorage` | Built-in local storage implementation for settings persistence |
| `SuperDataGridSettings` | Serializable grid settings (column order, widths, sort state) |
| `SuperDataGridColumnSettings` | Per-column settings (width, visibility, position) |
| `SuperDataGridColumnVisibilityInfo` | Column visibility state information |
| `SuperDataGridFilterInfo` | Filter state information for a column |
| `SuperDataGridSelectionMode` | Selection mode enum: `None`, `Single`, `Multiple` |
| `SuperDataGridEditionMode` | Edition mode enum for inline editing support |
| `SuperDataGridOrientation` | Grid orientation: `Horizontal` or `Vertical` |
| `SuperTextAlignment` | Text alignment enum for column content |
| `SortDirection` | Sort direction enum for column sorting |
| `SelectionInfo` | Information about the current selection state |
| `SelectionChangedEventArgs` | Event args raised when row selection changes |
| `CellClickedEventArgs` | Event args raised when a cell is clicked |
| `SuperDataGridDataLoadedEventArgs` | Event args raised after data is loaded |
| `IDataItem` | Interface for data items with key support |
| `DataGridSettingsStorageMode` | Storage mode enum: `LocalStorage` or `InMemory` |

**Key features:**
- Frozen header, footer, left and right columns
- Column drag-and-drop reordering and resize handles
- Built-in virtualization for large datasets
- Single and multiple row selection with checkbox column
- Inline row editing with edit/save/cancel actions
- Custom header, footer, loading, and empty templates
- Settings persistence via local storage or custom storage

#### Filters (`Components/SuperDataGrid/Filters`)

| Component / Class | Description |
|---|---|
| `SuperDataGridNumberFilterDialog` | Dialog for numeric column filtering with operators (equals, greater than, less than, etc.) |
| `SuperDataGridEnumFilterDialog` | Dialog for filtering columns by enum values with multi-select |
| `SuperDataGridFilterComponent` | Base class for custom filter components |
| `SuperDataGridNumberFilterSelection` | Model representing the current numeric filter selection |
| `SuperDataGridEnumFilterSelection` | Model representing the current enum filter selection |
| `SuperDataGridNumberFilterOperatorHelper` | Helper for numeric filter operator labels and logic |
| `SuperDataGridEnumFilterHelper` | Helper for extracting display names from enums |
| `EnumDisplayExtensions` | Extension methods for reading `[Display]` attributes on enum values |

#### Tools (`Components/SuperDataGrid/Tools`)

| Component / Class | Description |
|---|---|
| `SuperDataGridRowSelectorItem` | Row selector item used in selection toolbar |
| `SelectedActionInfo` | Describes an action available for selected rows |

---

### 🖼️ SuperLayout (`Components/SuperLayout`)

A responsive application layout system built on Bootstrap 5.3 with collapsible sidebar and chat panel.

| Component / Class | Description |
|---|---|
| `SuperLayout` | Main layout container that orchestrates header, sidebar, body, footer, and chat panel |
| `SuperHeader` | Top header bar component |
| `SuperSidebar` | Collapsible side navigation panel |
| `SuperBody` | Main content area |
| `SuperFooter` | Bottom footer bar component |
| `SuperChat` | Slide-in chat panel |
| `SuperChatButton` | Floating button to toggle the chat panel |
| `SidebarState` | Sidebar state enum: `Expanded`, `Collapsed`, `Hidden` |
| `ChatState` | Chat panel state enum |
| `Device` | Device information model for responsive behavior |

---

### 🗂️ SuperTabs (`Components/SuperTabs`)

A full-featured tabbed interface with dynamic tab management and service-driven control.

| Component / Class | Description |
|---|---|
| `SuperTabs` | Tabbed container component with top/bottom/left/right positioning |
| `SuperTabItem` | Tab definition model (title, icon, badge, closable, disabled) |
| `SuperTabsService` | Service for programmatic tab management (add, remove, select, update badge) |
| `SuperTabsInstance` | Represents a registered tabs instance for the service |
| `TabItem` | Individual tab item renderer |
| `Tabs` | Inner tabs container |
| `SuperTabPosition` | Tab position enum: `Top`, `Bottom`, `Left`, `Right` |
| `SuperTabChangeEventArgs` | Event args for tab selection changes |
| `SuperTabCloseEventArgs` | Event args when a tab is closed |
| `SuperTabAddRequestEventArgs` | Event args for tab add requests via the service |
| `SuperTabRemoveRequestEventArgs` | Event args for tab remove requests via the service |
| `SuperTabSelectRequestEventArgs` | Event args for tab select requests via the service |
| `SuperTabBadgeUpdateEventArgs` | Event args for badge update requests |
| `SuperTabServiceEventArgs` | Base event args for service-driven tab operations |

---

### 🔘 Buttons (`Components/Buttons`)

A rich collection of button components with icons, badges, dropdown menus, and confirmation support.

| Component / Class | Description |
|---|---|
| `SuperButton` | Primary button component with icon, badge, popover tooltip, and multiple styles |
| `SuperButtonBase` | Base class shared by all button components |
| `SuperButtonGroup` | Groups multiple buttons into a single visual unit |
| `SuperSplitButton` | Button with a dropdown menu for secondary actions |
| `SuperSplitButtonItem` | Menu item inside a split button dropdown |
| `SuperSplitLinkItem` | Navigation link item inside a split button dropdown |
| `SuperSplitDivider` | Divider between split button dropdown items |
| `SuperLinkButton` | Button styled as a navigation link |
| `SuperConfirmationButton` | Button that shows a confirmation popover before executing the action |
| `SuperToggleButton` | Toggle (on/off) button component |
| `SuperButtonStyle` | Button style enum: `Primary`, `Secondary`, `Success`, `Danger`, `Warning`, `Info`, `Light`, `Dark` |
| `SuperButtonSize` | Button size enum: `Default`, `Small`, `Large` |
| `SuperDropdownMenuAlignment` | Dropdown alignment enum |
| `SuperSplitButtonActionEventArgs` | Event args for split button action clicks |

---

### 💬 Dialogs (`Components/Dialogs`)

Modal dialog system with dynamic component rendering and confirmation dialogs.

| Component / Class | Description |
|---|---|
| `SuperDialog` | Modal dialog component that renders any Blazor component dynamically |
| `SuperConfirmDialog` | Confirmation dialog with customizable buttons and messages |
| `DialogOptions` | Configuration for dialog size, width, height, and behavior |
| `ConfirmOptions` | Configuration for confirmation dialog button labels and styles |

---

### 🔔 Notifications (`Components/Notifications`)

Toast notification system with auto-dismiss, severity levels, and progress indicators.

| Component / Class | Description |
|---|---|
| `SuperNotification` | Toast notification container with configurable position |
| `NotificationMessage` | Notification model with severity, title, detail, duration, and callbacks |
| `NotificationSeverity` | Severity enum: `Info`, `Success`, `Warning`, `Error` |
| `NotificationPosition` | Position enum: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |

---

### 📅 SuperDateRange (`Components/SuperDateRange`)

Calendar-based date range picker with preset ranges and customizable options.

| Component / Class | Description |
|---|---|
| `SuperDateRangePicker` | Date range picker component with calendar UI and preset selection |
| `SuperDateRangeDialog` | Dialog wrapper for the date range picker |
| `SuperDateRangeSelection` | Model representing the selected date range (start, end, preset) |
| `SuperDateRangePreset` | Preset enum: `Today`, `Yesterday`, `ThisWeek`, `LastWeek`, `Last7Days`, `Last30Days`, `ThisMonth`, `LastMonth`, `ThisYear`, `AllTime`, and more |
| `SuperDateRangePresetCalculator` | Computes actual date ranges from preset values |
| `SuperDateRangeCalendarWeek` | Helper for calendar week calculations |

---

### 📊 Google Charts (`Components/GoogleCharts`)

Blazor wrappers for Google Charts (Combo Chart and Pie Chart) plus a pure SVG time series chart.

| Component / Class | Description |
|---|---|
| `GoogleComboChart` | Google Charts combo chart (bar, line, area, stepped area) |
| `GooglePieChart` | Google Charts pie/donut chart |
| `TimeSeriesChart` | Pure SVG time series line chart with tooltips (no JS dependency) |
| `GoogleChartColumn` | Column definition for chart data |
| `GoogleChartDataRow` | Data row for chart rendering |
| `GoogleChartSeries` | Series configuration (type, color, target axis) |
| `GoogleChartSeriesType` | Series type enum: `Bars`, `Line`, `Area`, `SteppedArea` |
| `GoogleChartOptions` | Configuration for combo chart (title, axes, legend, colors, trend lines) |
| `GooglePieChartOptions` | Configuration specific to pie charts |
| `ChartDataPoint` | Data point model for the time series chart |
| `ChartOptions` | Configuration for the time series chart (height, colors, culture, padding) |
| `ChartPadding` | Padding configuration for charts |
| `TrendLine` | Trend line configuration |
| `ValueFormat` | Value format configuration |

---

### ✂️ SuperSplitter (`Components/SuperSplitter`)

Resizable split panel component with state persistence.

| Component / Class | Description |
|---|---|
| `SuperSplitter` | Resizable split panel container (horizontal or vertical) |
| `SplitPane` | Individual pane within a splitter |
| `SuperSplitterOrientation` | Orientation enum: `Horizontal`, `Vertical` |

**Key features:**
- Drag-to-resize with min/max constraints
- Collapsible first pane
- Automatic state persistence via local storage
- Custom persistence key support

---

### 🧭 BreadCrumbs (`Components/BreadCrumbs`)

Navigation breadcrumb components with customizable separators.

| Component / Class | Description |
|---|---|
| `SuperBreadCrumb` | Breadcrumb navigation container |
| `SuperBreadCrumbItem` | Individual breadcrumb item with link support |
| `SuperBackBreadcrumbItem` | Back-navigation breadcrumb item |

---

### 🍔 Menus (`Components/Menus`)

Sidebar menu item component for use inside `SuperSidebar`.

| Component / Class | Description |
|---|---|
| `SuperMenuItem` | Navigation menu item with icon, badge, theme, and nested submenu support |

---

### 🖼️ Images (`Components/Images`)

Placeholder image components.

| Component / Class | Description |
|---|---|
| `EmptyImage` | SVG placeholder image with configurable size and colors |

---

### ⚙️ Services (`Services`)

Application-level services registered via `AddSuperComponents()`.

| Service | Description |
|---|---|
| `SuperDialogService` | Programmatic dialog management — open/close modals and confirmation dialogs |
| `SuperNotificationService` | Programmatic toast notifications — show, dismiss, and manage notification messages |

---

### 🔧 Configuration (`Configuration`)

| Class | Description |
|---|---|
| `SuperComponentsConfiguration` | Global configuration: icon style, data grid settings storage mode, filter components |

---

### 🎨 Shared Enums

| Enum | Description |
|---|---|
| `SuperIconStyle` | Font Awesome icon style: `Solid`, `Regular`, `Light`, `Thin`, `Duotone`, `SharpSolid` |
| `SuperIconSize` | Icon size enum: `Default`, `Fs1` through `Fs6` |

---

## 🚀 Quick Start

```razor
@using SuperBlazorComponents.Components.SuperDataGrid

<SuperDataGrid TItem="Product"
               ItemsProvider="LoadProducts"
               Height="500px"
               AllowColumnReorder="true"
               AllowColumnResize="true"
               AllowSorting="true"
               AllowFiltering="true"
               FreezeHeader="true"
               FreezeLeftColumns="1"
               SelectionMode="SuperDataGridSelectionMode.Multiple"
               GridId="products-grid">
    <DataGridColumn Title="Name" For="@(c => c.Name)" />
    <DataGridColumn Title="Price" For="@(c => c.Price)" Width="120" />
    <DataGridColumn Title="Category" For="@(c => c.Category)" Width="150" />
</SuperDataGrid>
```

---

## 🏗️ Project Structure

```
src/
├── SuperBlazorComponents/          # Component library
│   ├── Components/
│   │   ├── BreadCrumbs/            # Breadcrumb navigation
│   │   ├── Buttons/                # Button components
│   │   ├── Dialogs/                # Modal & confirmation dialogs
│   │   ├── GoogleCharts/           # Google Charts wrappers + SVG charts
│   │   ├── Images/                 # Placeholder image components
│   │   ├── Menus/                  # Sidebar menu items
│   │   ├── Notifications/          # Toast notification system
│   │   ├── SuperDataGrid/          # Virtual data grid
│   │   │   ├── Filters/            # Column filter components
│   │   │   └── Tools/              # Row selection tools
│   │   ├── SuperDateRange/         # Date range picker
│   │   ├── SuperLayout/            # Responsive app layout
│   │   ├── SuperSplitter/          # Resizable split panels
│   │   └── SuperTabs/              # Tabbed interface
│   ├── Configuration/              # Global configuration
│   └── Services/                   # Dialog & notification services
└── DemoWebSite/                    # Demo application
```

---

## 🤝 Contributing

Contributions are welcome! Feel free to open issues, suggest features, or submit pull requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is open source. See the [LICENSE](LICENSE) file for details.

---

## 🔗 Links

- **GitHub:** [https://github.com/appliman/superblazorcomponents](https://github.com/appliman/superblazorcomponents)
- **NuGet:** [https://www.nuget.org/packages/SuperBlazorComponents](https://www.nuget.org/packages/SuperBlazorComponents)
