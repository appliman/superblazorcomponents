# Changelog

All notable changes to SuperBlazorComponents are documented in this file.

## 1.5.41.0

### Added

- Added `SuperDataGrid` hierarchical lazy-loading mode with the `Hierarchical` parameter.
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
- Root rows remain virtualized; child rows are rendered inline under the currently rendered root rows.
- Child rows are expected to be returned without paging.
