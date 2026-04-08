namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Describes a grid column and its current visibility state.
/// </summary>
/// <param name="ColumnIndex">The zero-based column index in the current grid order.</param>
/// <param name="PropertyName">The property name bound to the column.</param>
/// <param name="DisplayName">The display text shown to users for the column.</param>
/// <param name="IsVisible">Indicates whether the column is currently visible.</param>
/// <param name="IsAlwaysVisible">Indicates whether the column must stay visible.</param>
public sealed record SuperDataGridColumnVisibilityInfo(
    int ColumnIndex,
    string PropertyName,
    string DisplayName,
    bool IsVisible,
    bool IsAlwaysVisible);
