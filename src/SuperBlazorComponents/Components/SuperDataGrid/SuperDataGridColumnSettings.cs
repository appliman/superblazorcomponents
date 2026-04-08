namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Represents the persisted settings for a data grid column.
/// </summary>
public sealed record SuperDataGridColumnSettings
{
    /// <summary>
    /// The property name identifying the column.
    /// </summary>
    public string PropertyName { get; init; } = string.Empty;

    /// <summary>
    /// The width of the column.
    /// </summary>
    public string? Width { get; init; }

    /// <summary>
    /// The order/position of the column.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Whether the column is visible.
    /// </summary>
    public bool IsVisible { get; init; } = true;
}

