namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>A named snapshot of column order, visibility and widths.</summary>
public sealed record SuperDataGridColumnProfile
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<SuperDataGridColumnSettings> Columns { get; init; } = [];
}
