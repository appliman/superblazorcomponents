using System.Globalization;

using SuperBlazorComponents.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents.Components.SuperDateRange;

public partial class SuperDateRangeDialog
{
    private IReadOnlyList<string> DayHeaders => Loc["DateRange.DayHeaders"].Value.Split(',');

    private static readonly IReadOnlyList<SuperDateRangePreset> OrderedPresets =
    [
        SuperDateRangePreset.Today,
        SuperDateRangePreset.Yesterday,
        SuperDateRangePreset.ThisWeek,
        SuperDateRangePreset.LastWeek,
        SuperDateRangePreset.Last7Days,
        SuperDateRangePreset.Last14Days,
        SuperDateRangePreset.Last30Days,
        SuperDateRangePreset.Last90Days,
        SuperDateRangePreset.ThisMonth,
        SuperDateRangePreset.LastMonth,
        SuperDateRangePreset.Last12Months,
        SuperDateRangePreset.Last13Months,
        SuperDateRangePreset.Last24Months,
        SuperDateRangePreset.ThisQuarter,
        SuperDateRangePreset.LastQuarter,
        SuperDateRangePreset.ThisYear,
        SuperDateRangePreset.LastYear,
        SuperDateRangePreset.AllTime
    ];

    private DateTime _visibleMonthStart;
    private SuperDateRangeSelection _draftValue = new(null, null, SuperDateRangePreset.AllTime);

    [Inject]
    private SuperDialogService DialogService { get; set; } = default!;

    [Inject]
    private IStringLocalizer Loc { get; set; } = default!;

    [Parameter]
    public SuperDateRangeSelection Value { get; set; } = new(null, null, SuperDateRangePreset.AllTime);

    [Parameter]
    public bool DisplayWeekNumbers { get; set; } = true;

    [Parameter]
    public bool DisableFutureDates { get; set; } = true;

    protected override void OnParametersSet()
    {
        var normalizedValue = NormalizeRange(Value);
        _draftValue = normalizedValue;
        ResetVisibleMonth(normalizedValue);
    }

    private Task CancelAsync()
    {
        return DialogService.Close(null);
    }

    private Task ApplyAsync()
    {
        var normalizedValue = NormalizeRange(_draftValue);
        return DialogService.Close(normalizedValue);
    }

    private void SelectPreset(SuperDateRangePreset preset)
    {
        _draftValue = SuperDateRangePresetCalculator.Resolve(preset, GetToday());
        ResetVisibleMonth(_draftValue);
    }

    private void ShowPreviousMonth()
    {
        _visibleMonthStart = _visibleMonthStart.AddMonths(-1);
    }

    private void ShowNextMonth()
    {
        _visibleMonthStart = _visibleMonthStart.AddMonths(1);
    }

    private void SelectDay(DateTime day)
    {
        if (IsDayDisabled(day))
        {
            return;
        }

        if (_draftValue.StartDate is null || _draftValue.EndDate is not null)
        {
            SetDraftRange(day, null, SuperDateRangePreset.Custom, resetVisibleMonth: false);
            return;
        }

        if (day < _draftValue.StartDate.Value)
        {
            SetDraftRange(day, _draftValue.StartDate, SuperDateRangePreset.Custom, resetVisibleMonth: false);
            return;
        }

        SetDraftRange(_draftValue.StartDate, day, SuperDateRangePreset.Custom, resetVisibleMonth: false);
    }

    private void SelectWeek(SuperDateRangeCalendarWeek week)
    {
        ArgumentNullException.ThrowIfNull(week);

        var selectableRange = GetSelectableWeekRange(week);
        if (selectableRange is null)
        {
            return;
        }

        SetDraftRange(selectableRange.Value.StartDate, selectableRange.Value.EndDate, SuperDateRangePreset.Custom, string.Format(CultureInfo.CurrentUICulture, Loc["DateRange.Week"], week.WeekNumber.ToString("D2", CultureInfo.InvariantCulture)), resetVisibleMonth: false);
    }

    private void OnStartDateChanged(ChangeEventArgs args)
    {
        var startDate = NormalizeSelectableDate(ParseDate(args.Value));
        SetDraftRange(startDate, _draftValue.EndDate, GetManualPreset(startDate, _draftValue.EndDate));
    }

    private void OnEndDateChanged(ChangeEventArgs args)
    {
        var endDate = NormalizeSelectableDate(ParseDate(args.Value));
        SetDraftRange(_draftValue.StartDate, endDate, GetManualPreset(_draftValue.StartDate, endDate));
    }

    private void ClearStartDate()
    {
        SetDraftRange(null, _draftValue.EndDate, GetManualPreset(null, _draftValue.EndDate));
    }

    private void ClearEndDate()
    {
        SetDraftRange(_draftValue.StartDate, null, GetManualPreset(_draftValue.StartDate, null));
    }

    private void SetDraftRange(DateTime? startDate, DateTime? endDate, SuperDateRangePreset preset, string? periodName = null, bool resetVisibleMonth = true)
    {
        _draftValue = NormalizeRange(new SuperDateRangeSelection(startDate, endDate, preset, periodName));
        if (resetVisibleMonth)
        {
            ResetVisibleMonth(_draftValue);
        }
    }

    private string GetSelectedDaysText()
    {
        if (_draftValue.StartDate is not null && _draftValue.EndDate is not null)
        {
            var dayCount = _draftValue.EndDate.Value.Day - _draftValue.StartDate.Value.Day + 1;
            return dayCount > 1
                ? string.Format(CultureInfo.CurrentUICulture, Loc["DateRange.DaysSelected"], dayCount)
                : Loc["DateRange.OneDaySelected"];
        }

        if (_draftValue.StartDate is not null)
        {
            return Loc["DateRange.StartSelected"];
        }

        if (_draftValue.EndDate is not null)
        {
            return Loc["DateRange.EndSelected"];
        }

        return Loc["DateRange.AllPeriod"];
    }

    private string? GetMaxSelectableDateValue()
    {
        return DisableFutureDates
            ? GetToday().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;
    }

    private string GetPresetLabel(SuperDateRangePreset preset)
    {
        return SuperDateRangePresetCalculator.GetLabel(preset, Loc);
    }

    private string GetPresetButtonClass(SuperDateRangePreset preset)
    {
        return _draftValue.Preset == preset
            ? "list-group-item list-group-item-action active"
            : "list-group-item list-group-item-action";
    }

    private static string GetInputDateValue(DateTime? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private IReadOnlyList<DateTime> GetVisibleMonths()
    {
        return [_visibleMonthStart, _visibleMonthStart.AddMonths(1)];
    }

    private IReadOnlyList<SuperDateRangeCalendarWeek> GetMonthWeeks(DateTime month)
    {
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var lastDayOfMonth = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
        var calendarStart = GetStartOfWeek(firstDayOfMonth);
        var calendarEnd = GetEndOfWeek(lastDayOfMonth);
        var weeks = new List<SuperDateRangeCalendarWeek>();

        for (var weekStart = calendarStart; weekStart <= calendarEnd; weekStart = weekStart.AddDays(7))
        {
            var days = new List<DateTime?>(7);
            for (var dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                var currentDay = weekStart.AddDays(dayOffset);
                days.Add(currentDay.Month == month.Month ? currentDay : null);
            }

            var weekNumber = ISOWeek.GetWeekOfYear(weekStart);
            weeks.Add(new SuperDateRangeCalendarWeek(weekNumber, weekStart, weekStart.AddDays(6), days));
        }

        return weeks;
    }

    private string GetWeekNumberButtonClass(SuperDateRangeCalendarWeek week)
    {
        ArgumentNullException.ThrowIfNull(week);

        var classes = new List<string> { "super-date-range-week-number" };

        if (IsWeekSelected(week))
        {
            classes.Add("is-selected");
        }

        if (IsWeekDisabled(week))
        {
            classes.Add("is-disabled");
        }

        return string.Join(" ", classes);
    }

    private static string GetMonthTitle(DateTime month)
    {
        return month.ToString("MMMM yyyy", CultureInfo.CurrentUICulture).ToUpper(CultureInfo.CurrentUICulture);
    }

    private string GetDayButtonClass(DateTime day)
    {
        var classes = new List<string> { "super-date-range-day" };

        if (IsDayDisabled(day))
        {
            classes.Add("is-disabled");
        }

        if (IsBoundaryDay(day))
        {
            classes.Add("is-boundary");
        }
        else if (IsInSelectedRange(day))
        {
            classes.Add("is-in-range");
        }

        if (day == GetToday())
        {
            classes.Add("is-today");
        }

        return string.Join(" ", classes);
    }

    private bool IsDayDisabled(DateTime day)
    {
        return DisableFutureDates && day > GetToday();
    }

    private bool IsWeekDisabled(SuperDateRangeCalendarWeek week)
    {
        ArgumentNullException.ThrowIfNull(week);

        return GetSelectableWeekRange(week) is null;
    }

    private bool IsWeekSelected(SuperDateRangeCalendarWeek week)
    {
        ArgumentNullException.ThrowIfNull(week);

        var selectableRange = GetSelectableWeekRange(week);
        if (selectableRange is null)
        {
            return false;
        }

        return _draftValue.StartDate == selectableRange.Value.StartDate
            && _draftValue.EndDate == selectableRange.Value.EndDate;
    }

    private bool IsBoundaryDay(DateTime day)
    {
        return _draftValue.StartDate == day || _draftValue.EndDate == day;
    }

    private bool IsInSelectedRange(DateTime day)
    {
        return _draftValue.StartDate is not null
            && _draftValue.EndDate is not null
            && day > _draftValue.StartDate.Value
            && day < _draftValue.EndDate.Value;
    }

    private void ResetVisibleMonth(SuperDateRangeSelection range)
    {
        var referenceDate = range.EndDate ?? range.StartDate ?? GetToday();
        _visibleMonthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(-1);
    }

    private SuperDateRangeSelection NormalizeRange(SuperDateRangeSelection range)
    {
        var preset = range.Preset;
        var startDate = NormalizeSelectableDate(range.StartDate);
        var endDate = NormalizeSelectableDate(range.EndDate);
        var periodName = range.PeriodName;

        if (startDate is not null && endDate is not null && startDate > endDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        if (startDate is null && endDate is null && preset == SuperDateRangePreset.Custom)
        {
            preset = SuperDateRangePreset.AllTime;
        }

        return new SuperDateRangeSelection(startDate, endDate, preset, periodName);
    }

    private static SuperDateRangePreset GetManualPreset(DateTime? startDate, DateTime? endDate)
    {
        return startDate is null && endDate is null
            ? SuperDateRangePreset.AllTime
            : SuperDateRangePreset.Custom;
    }

    private static DateTime? ParseDate(object? value)
    {
        var rawValue = value?.ToString();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        return DateTime.TryParseExact(rawValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
            ? parsedDate
            : null;
    }

    private DateTime? NormalizeSelectableDate(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        if (!DisableFutureDates)
        {
            return value;
        }

        var today = GetToday();
        return value > today ? today : value;
    }

    private (DateTime StartDate, DateTime EndDate)? GetSelectableWeekRange(SuperDateRangeCalendarWeek week)
    {
        ArgumentNullException.ThrowIfNull(week);

        var startDate = week.WeekStart;
        var endDate = week.WeekEnd;

        if (DisableFutureDates)
        {
            var today = GetToday();
            if (startDate > today)
            {
                return null;
            }

            if (endDate > today)
            {
                endDate = today;
            }
        }

        return (startDate, endDate);
    }

    private static DateTime GetToday()
    {
        return DateTime.Today;
    }

    private static DateTime GetEndOfWeek(DateTime date)
    {
        var offset = 6 - ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(offset);
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }
}
