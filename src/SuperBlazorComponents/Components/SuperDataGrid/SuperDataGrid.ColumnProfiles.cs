using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.SuperDataGrid;

public partial class SuperDataGrid<TItem>
{
    private readonly SemaphoreSlim _settingsGate = new(1, 1);
    private SuperDataGridColumnProfiles _columnProfiles = new();

    public bool SupportsColumnProfiles => !string.IsNullOrWhiteSpace(GridId) && SettingsStorage is ISuperDataGridColumnProfilesStorage;
    public string? ActiveColumnProfileId => _columnProfiles.ActiveProfileId;
    public IReadOnlyList<SuperDataGridColumnProfile> ColumnProfiles => _columnProfiles.Profiles
        .Select(p => p with { Columns = p.Columns.ToArray() }).ToArray();
    public bool ColumnProfilesStorageFailed { get; private set; }

    public async Task SaveColumnProfileAsync(string name)
    {
        await _settingsGate.WaitAsync();
        try
        {
            name = GetAvailableProfileName(name);
            var profile = new SuperDataGridColumnProfile { Name = name, Columns = GetColumnSettings().ToArray() };
            new SuperDataGridColumnProfileDocument { Name = name, Columns = profile.Columns }.ToJson();
            await StoreProfilesAsync(_columnProfiles with
            {
                ActiveProfileId = profile.Id,
                Profiles = [.. _columnProfiles.Profiles, profile]
            });
        }
        finally
        {
            _settingsGate.Release();
        }
        NotifyColumnStateChanged();
    }

    public async Task SelectColumnProfileAsync(string? profileId)
    {
        await _settingsGate.WaitAsync();
        try
        {
            EnsureProfilesSupported();
            var profile = profileId is null ? null : FindProfile(profileId);
            await StoreProfilesAsync(_columnProfiles with { ActiveProfileId = profileId });
            if (profile is not null)
            {
                ApplyProfileColumns(profile.Columns);
                _loadedColumnSettings = NormalizeColumnSettings(GetColumnSettings());
                await SettingsStorage!.SaveSettingsAsync(GridId!, _loadedColumnSettings);
            }
        }
        finally
        {
            _settingsGate.Release();
        }
        await ColumnSettingsChanged.InvokeAsync(GetColumnSettings());
        NotifyColumnStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    public async Task RenameColumnProfileAsync(string profileId, string name)
    {
        await _settingsGate.WaitAsync();
        try
        {
            FindProfile(profileId);
            name = GetAvailableProfileName(name, profileId);
            await StoreProfilesAsync(_columnProfiles with
            {
                Profiles = _columnProfiles.Profiles.Select(p => p.Id == profileId ? p with { Name = name } : p).ToArray()
            });
        }
        finally
        {
            _settingsGate.Release();
        }
        NotifyColumnStateChanged();
    }

    public async Task DeleteColumnProfileAsync(string profileId)
    {
        await _settingsGate.WaitAsync();
        try
        {
            FindProfile(profileId);
            // Keep the displayed view in the legacy current-settings slot.
            await SettingsStorage!.SaveSettingsAsync(GridId!, GetColumnSettings());
            await StoreProfilesAsync(_columnProfiles with
            {
                ActiveProfileId = _columnProfiles.ActiveProfileId == profileId ? null : _columnProfiles.ActiveProfileId,
                Profiles = _columnProfiles.Profiles.Where(p => p.Id != profileId).ToArray()
            });
        }
        finally
        {
            _settingsGate.Release();
        }
        NotifyColumnStateChanged();
    }

    public string ExportColumnProfileJson()
    {
        return new SuperDataGridColumnProfileDocument
        {
            Name = _columnProfiles.Profiles.FirstOrDefault(p => p.Id == ActiveColumnProfileId)?.Name ?? "Configuration",
            Columns = GetColumnSettings().ToArray()
        }.ToJson();
    }

    public async Task ImportColumnProfileAsync(string json, string name)
    {
        var document = SuperDataGridColumnProfileDocument.FromJson(json);
        if (!document.Columns.Any(s => _columns.Any(c => c.Property == s.PropertyName)))
        {
            throw new ArgumentException("No matching columns.", nameof(json));
        }
        await _settingsGate.WaitAsync();
        try
        {
            name = GetAvailableProfileName(name);
            var profile = new SuperDataGridColumnProfile { Name = name, Columns = document.Columns.ToArray() };
            await StoreProfilesAsync(_columnProfiles with
            {
                ActiveProfileId = profile.Id,
                Profiles = [.. _columnProfiles.Profiles, profile]
            });
            ApplyProfileColumns(profile.Columns);
            _loadedColumnSettings = NormalizeColumnSettings(GetColumnSettings());
            await SettingsStorage!.SaveSettingsAsync(GridId!, _loadedColumnSettings);
        }
        finally
        {
            _settingsGate.Release();
        }
        await ColumnSettingsChanged.InvokeAsync(GetColumnSettings());
        NotifyColumnStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadColumnProfilesAsync()
    {
        if (!SupportsColumnProfiles)
        {
            return;
        }
        try
        {
            var profiles = await ((ISuperDataGridColumnProfilesStorage)SettingsStorage!).GetProfilesAsync(GridId!);
            if (_isDisposed)
            {
                return;
            }
            if (profiles is not null)
            {
                if (profiles.Profiles is null)
                {
                    throw new ArgumentException("Missing saved configurations.");
                }
                var ids = new HashSet<string>(StringComparer.Ordinal);
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var profile in profiles.Profiles)
                {
                    if (profile is null || string.IsNullOrWhiteSpace(profile.Id) || !ids.Add(profile.Id)
                        || !names.Add(SuperDataGridColumnProfileDocument.ValidateName(profile.Name)))
                    {
                        throw new ArgumentException("Invalid saved configurations.");
                    }
                    new SuperDataGridColumnProfileDocument { Name = profile.Name, Columns = profile.Columns }.ToJson();
                }
                _columnProfiles = profiles;
                var active = profiles.Profiles.FirstOrDefault(p => p.Id == profiles.ActiveProfileId);
                if (active is not null)
                {
                    ApplyProfileColumns(active.Columns);
                    _loadedColumnSettings = NormalizeColumnSettings(GetColumnSettings());
                }
                else
                {
                    _columnProfiles = profiles with { ActiveProfileId = null };
                }
            }
        }
        catch (Exception exception) when (exception is JSException or JsonException or ArgumentException or InvalidOperationException)
        {
            ColumnProfilesStorageFailed = true;
            Logger.LogWarning("Unable to load column configurations for grid {GridId}: {ErrorType}", GridId, exception.GetType().Name);
        }
        NotifyColumnStateChanged();
    }

    private string GetAvailableProfileName(string name, string? exceptId = null)
    {
        EnsureProfilesSupported();
        name = SuperDataGridColumnProfileDocument.ValidateName(name);
        if (_columnProfiles.Profiles.Any(p => p.Id != exceptId && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Configuration name already exists.", nameof(name));
        }
        return name;
    }

    private SuperDataGridColumnProfile FindProfile(string id)
    {
        EnsureProfilesSupported();
        return _columnProfiles.Profiles.FirstOrDefault(p => p.Id == id)
            ?? throw new ArgumentException("Unknown configuration.", nameof(id));
    }

    private void EnsureProfilesSupported()
    {
        if (!SupportsColumnProfiles)
        {
            throw new InvalidOperationException("This grid storage does not support named configurations.");
        }
    }

    private async Task StoreProfilesAsync(SuperDataGridColumnProfiles profiles)
    {
        EnsureProfilesSupported();
        await ((ISuperDataGridColumnProfilesStorage)SettingsStorage!).SaveProfilesAsync(GridId!, profiles);
        _columnProfiles = profiles;
        ColumnProfilesStorageFailed = false;
    }

    private void ApplyProfileColumns(IEnumerable<SuperDataGridColumnSettings> settings)
    {
        var columns = NormalizeColumnSettings(settings);
        foreach (var column in _columns)
        {
            var setting = columns.FirstOrDefault(s => s.PropertyName == column.Property);
            if (setting is not null)
            {
                column.SetVisible(column.AlwaysVisible || setting.IsVisible);
                if (AllowColumnResize && column.Resizable)
                {
                    column.SetWidth(setting.Width ?? column.Width);
                }
            }
        }
        if (AllowColumnReorder)
        {
            var positions = columns.Select((column, index) => (column.PropertyName, index))
                .ToDictionary(p => p.PropertyName, p => p.index, StringComparer.Ordinal);
            // Fixed columns form boundaries: no movable column crosses them.
            var start = 0;
            while (start < _columns.Count)
            {
                if (!_columns[start].Reorderable)
                {
                    start++;
                    continue;
                }
                var end = start;
                while (end < _columns.Count && _columns[end].Reorderable)
                {
                    end++;
                }
                var ordered = _columns.GetRange(start, end - start)
                    .OrderBy(c => positions.GetValueOrDefault(c.Property, int.MaxValue)).ToList();
                for (var i = 0; i < ordered.Count; i++)
                {
                    _columns[start + i] = ordered[i];
                }
                start = end;
            }
        }
        if (_columns.Count > 0 && !_columns.Any(c => c.CurrentVisible))
        {
            _columns[0].SetVisible(true);
        }
        InvalidateColumnStyleCache();
    }
}
