# Changelog

All notable changes to SuperBlazorComponents are documented in this file.

## SuperBlazorComponents 2.0.12 / DataGridExporter 2.0.15 — 2026-10-05

### Added

- Added nullable `FromRow` and `ToRow` properties to `SelectionInfo<TItem>` with one-based, inclusive range bounds available before selection notifications and in selector action handlers.
- Added tests for range metadata, first-X and single-row intervals, bounds reset, stable-key reloads, and preservation after failed or cancelled operations.

### Changed

- Clearing selection, selecting all, or manually changing selected membership clears both range bounds; reloading equivalent instances preserves them.
- Documented the range metadata lifecycle, event usage, requested positions versus item identity, and asynchronous capture of the live selection state.
- Updated the exporter package dependency to SuperBlazorComponents 2.0.12.

## SuperBlazorComponents 2.0.11 / DataGridExporter 2.0.14 — 2026-10-04

### Added

- Added a localized selection-range dialog to the SuperDataGrid selector: select the first X rows or rows X through Y, inclusive, across the active filtered and sorted query.
- Added `SelectRangeAsync(int fromRow, int toRow, CancellationToken cancellationToken = default)` with bounded provider requests, cancellation, and replacement only after successful loading.
- Added tests for interval bounds, batching, selection replacement, stable keys, errors, query changes, dialog cancellation, and CSV export after deselection.

### Changed

- Checkbox selection now matches stable row keys when virtualized providers recreate item instances, preventing duplicate selections and allowing deselection with a new instance.
- Expanded the SuperDataGrid guide with interactive dialog setup, selection semantics, range API examples, provider requirements, error handling, and memory/consistency limits.
- Updated the exporter documentation and package version for compatibility with SuperBlazorComponents 2.0.11.

## 1.6.43.0

### Added

- Added `SuperDataGrid` hierarchical lazy-loading mode with the `Hierarchical` parameter.
- Added `SuperTriStateCheckbox` for nullable boolean values with isolated CSS and JavaScript.
- Added `HierarchyKeySelector` to customize row identity for hierarchy state.
- Extended `GridItemsProviderRequest<TItem>` with `ParentItem`, `ParentKey`, `HierarchyLevel`, and `IsHierarchyRequest` so the existing `ItemsProvider` can load child rows.
- Added `ExpandAllAsync(CancellationToken)` and `CollapseAllAsync()` public methods for external hierarchy control through `@ref`.
- Added a hierarchical demo to `SuperGridDemo.razor`, including external expand/collapse buttons and a 200 child-row safety cap for the demo.

### Changed

- The row-number column can now render hierarchy expand/collapse controls when `Hierarchical` is enabled.
- Hierarchical child rows are reloaded on every expansion and discarded on collapse, keeping child data fresh.
- Child hierarchy requests reuse the active sort and filter state from the root grid.

### Notes

- Parent and child rows must use the same `TItem` type.
- Hierarchical mode renders root rows without `Virtualize` so expanded child rows do not break Blazor's fixed item-size virtualization assumptions.
- Child rows are expected to be returned without paging.
