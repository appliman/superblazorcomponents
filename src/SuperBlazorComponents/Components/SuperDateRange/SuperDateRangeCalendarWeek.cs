namespace SuperBlazorComponents.Components.SuperDateRange;

internal sealed record SuperDateRangeCalendarWeek(
    int WeekNumber,
    DateTimeOffset WeekStart,
    DateTimeOffset WeekEnd,
    IReadOnlyList<DateTimeOffset?> Days);
