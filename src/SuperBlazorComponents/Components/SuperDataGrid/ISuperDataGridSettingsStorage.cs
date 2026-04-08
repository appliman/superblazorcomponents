namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Interface for persisting data grid column settings.
/// Implement this interface to provide custom storage (e.g., database, user preferences).
/// </summary>
public interface ISuperDataGridSettingsStorage
{
    /// <summary>
    /// Retrieves the saved settings for a specific grid.
    /// </summary>
    /// <param name="gridId">The unique identifier of the grid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved column settings, or null if none exist.</returns>
    Task<IEnumerable<SuperDataGridColumnSettings>?> GetSettingsAsync(string gridId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the settings for a specific grid.
    /// </summary>
    /// <param name="gridId">The unique identifier of the grid.</param>
    /// <param name="settings">The column settings to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveSettingsAsync(string gridId, IEnumerable<SuperDataGridColumnSettings> settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the saved settings for a specific grid.
    /// </summary>
    /// <param name="gridId">The unique identifier of the grid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearSettingsAsync(string gridId, CancellationToken cancellationToken = default);
}
