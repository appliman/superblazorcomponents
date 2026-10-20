namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>Named configurations and the last selected configuration for one GridId.</summary>
public sealed record SuperDataGridColumnProfiles
{
    public string? ActiveProfileId { get; init; }
    public IReadOnlyList<SuperDataGridColumnProfile> Profiles { get; init; } = [];
}
