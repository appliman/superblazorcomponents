namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>Optional capability implemented alongside ISuperDataGridSettingsStorage.</summary>
public interface ISuperDataGridColumnProfilesStorage
{
    Task<SuperDataGridColumnProfiles?> GetProfilesAsync(string gridId, CancellationToken cancellationToken = default);
    Task SaveProfilesAsync(string gridId, SuperDataGridColumnProfiles profiles, CancellationToken cancellationToken = default);
}
