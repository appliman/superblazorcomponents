using System.Text.Json;

using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>
/// Default implementation of IDataGridSettingsStorage using browser localStorage.
/// Suitable for Blazor WebAssembly and Blazor Server applications.
/// </summary>
public sealed class SuperDataGridSettingsLocalStorage : ISuperDataGridSettingsStorage
{
    private const string KEY_PREFIX = "vdg_settings_";
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the LocalStorageSettingsStorage class using the specified JavaScript runtime for
    /// browser interop.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance used to interact with the browser's local storage.</param>
    /// <exception cref="ArgumentNullException">Thrown if jsRuntime is null.</exception>
    public SuperDataGridSettingsLocalStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SuperDataGridColumnSettings>?> GetSettingsAsync(string gridId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gridId);

        try
        {
            var key = GetStorageKey(gridId);
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<List<SuperDataGridColumnSettings>>(json, _jsonOptions);
        }
        catch (JSDisconnectedException)
        {
            // Circuit is disconnected, return null
            return null;
        }
        catch (JsonException)
        {
            // Invalid JSON, return null
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SaveSettingsAsync(string gridId, IEnumerable<SuperDataGridColumnSettings> settings, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gridId);
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var key = GetStorageKey(gridId);
            var json = JsonSerializer.Serialize(settings.ToList(), _jsonOptions);

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, json);
        }
        catch (JSDisconnectedException)
        {
            // Circuit is disconnected, silently fail
        }
    }

    /// <inheritdoc />
    public async Task ClearSettingsAsync(string gridId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gridId);

        try
        {
            var key = GetStorageKey(gridId);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, key);
        }
        catch (JSDisconnectedException)
        {
            // Circuit is disconnected, silently fail
        }
    }

    private static string GetStorageKey(string gridId)
    {
        return $"{KEY_PREFIX}{gridId}";
    }
}

/// <summary>
/// In-memory implementation of IDataGridSettingsStorage.
/// Useful for testing or when persistence is not required.
/// Settings are lost when the application restarts.
/// </summary>
public sealed class InMemorySettingsStorage : ISuperDataGridSettingsStorage
{
    private readonly Dictionary<string, List<SuperDataGridColumnSettings>> _storage = [];

    /// <inheritdoc />
    public Task<IEnumerable<SuperDataGridColumnSettings>?> GetSettingsAsync(string gridId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gridId);

        _storage.TryGetValue(gridId, out var settings);
        return Task.FromResult<IEnumerable<SuperDataGridColumnSettings>?>(settings);
    }

    /// <inheritdoc />
    public Task SaveSettingsAsync(string gridId, IEnumerable<SuperDataGridColumnSettings> settings, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gridId);
        ArgumentNullException.ThrowIfNull(settings);

        _storage[gridId] = settings.ToList();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearSettingsAsync(string gridId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gridId);

        _storage.Remove(gridId);
        return Task.CompletedTask;
    }
}
