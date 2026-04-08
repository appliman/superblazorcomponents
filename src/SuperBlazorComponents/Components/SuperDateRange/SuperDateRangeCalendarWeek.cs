namespace SuperBlazorComponents.Components.SuperDateRange;

internal sealed record SuperDateRangeCalendarWeek(
    int WeekNumber,
    DateTime WeekStart,
	DateTime WeekEnd,
    IReadOnlyList<DateTime?> Days);
