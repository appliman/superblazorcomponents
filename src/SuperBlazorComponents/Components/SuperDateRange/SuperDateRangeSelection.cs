namespace SuperBlazorComponents.Components.SuperDateRange;

public sealed record SuperDateRangeSelection(
    DateTime? StartDate,
    DateTime? EndDate,
    SuperDateRangePreset Preset = SuperDateRangePreset.Custom,
    string? PeriodName = null);
