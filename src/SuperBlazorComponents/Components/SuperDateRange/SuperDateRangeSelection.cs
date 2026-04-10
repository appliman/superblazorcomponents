namespace SuperBlazorComponents.Components.SuperDateRange;

public sealed record SuperDateRangeSelection(
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    SuperDateRangePreset Preset = SuperDateRangePreset.Custom,
    string? PeriodName = null);
