using System.Globalization;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace SuperBlazorComponents.Components.SuperDateRange;

public partial class SuperDateRangePicker : ComponentBase, IAsyncDisposable
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

    private DateTimeOffset _visibleMonthStart;
    private bool _isOpen;
    private SuperDateRangeSelection _committedValue = new(null, null, SuperDateRangePreset.AllTime);
    private SuperDateRangeSelection _draftValue = new(null, null, SuperDateRangePreset.AllTime);
    private string? _committedSummaryOverride;
    private string? _draftSummaryOverride;
    private ElementReference _pickerRef;
    private ElementReference _buttonRef;
    private ElementReference _panelRef;
    private IJSObjectReference? _jsModule;
    private bool _floatingPanelAttached;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IStringLocalizer Loc { get; set; } = default!;

    [Parameter]
    public SuperDateRangeSelection Value { get; set; } = new(null, null, SuperDateRangePreset.AllTime);

    [Parameter]
    public EventCallback<SuperDateRangeSelection> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<SuperDateRangeSelection> Applied { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string ButtonCssClass { get; set; } = "btn btn-outline-secondary d-inline-flex align-items-center justify-content-between gap-2";

    [Parameter]
    public string? EmptyText { get; set; }

    [Parameter]
    public string MinWidth { get; set; } = "18rem";

    [Parameter]
    public bool DisplayWeekNumbers { get; set; } = true;

    /// <summary>
    /// Indique si les dates futures doivent être désactivées dans le calendrier et dans les champs de saisie.
    /// </summary>
    [Parameter]
    public bool DisableFutureDates { get; set; } = true;

    protected override void OnParametersSet()
    {
        var normalizedValue = NormalizeRange(Value);
        if (_committedValue == normalizedValue)
        {
            return;
        }

        _committedValue = normalizedValue;
        _committedSummaryOverride = normalizedValue.PeriodName;
        if (_isOpen)
        {
            return;
        }

        _draftValue = normalizedValue;
        _draftSummaryOverride = _committedSummaryOverride;
        ResetVisibleMonth(normalizedValue);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/SuperBlazorComponents/Components/SuperDateRange/SuperDateRangePicker.razor.js");
        }

        if (_jsModule is null)
        {
            return;
        }

        if (_isOpen)
        {
            await _jsModule.InvokeVoidAsync("attachFloatingPanel", _pickerRef, _buttonRef, _panelRef);
            _floatingPanelAttached = true;
            return;
        }

        if (_floatingPanelAttached)
        {
            await _jsModule.InvokeVoidAsync("detachFloatingPanel", _pickerRef);
            _floatingPanelAttached = false;
        }
    }

    private async Task ApplySelectionAsync()
    {
        var normalizedValue = NormalizeRange(_draftValue);
        var hasChanged = normalizedValue != _committedValue;

        await DetachFloatingPanelAsync();

        _committedValue = normalizedValue;
        _draftValue = normalizedValue;
        _committedSummaryOverride = _draftSummaryOverride;
        _isOpen = false;

        if (hasChanged)
        {
            await ValueChanged.InvokeAsync(normalizedValue);
        }

        if (Applied.HasDelegate)
        {
            await Applied.InvokeAsync(normalizedValue);
        }
    }

    private async Task CancelSelectionAsync()
    {
        await DetachFloatingPanelAsync();

        _draftValue = _committedValue;
        _draftSummaryOverride = _committedSummaryOverride;
        ResetVisibleMonth(_committedValue);
        _isOpen = false;
    }

    private async Task TogglePickerAsync()
    {
        if (Disabled)
        {
            return;
        }

        if (_isOpen)
        {
            await CancelSelectionAsync();
            return;
        }

        _draftValue = _committedValue;
        _draftSummaryOverride = _committedSummaryOverride;
        ResetVisibleMonth(_draftValue);
        _isOpen = true;
    }

    private void SelectPreset(SuperDateRangePreset preset)
    {
        _draftValue = SuperDateRangePresetCalculator.Resolve(preset, GetToday());
        _draftSummaryOverride = _draftValue.PeriodName;
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

    private void SelectDay(DateTimeOffset day)
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

   private void SetDraftRange(DateTimeOffset? startDate, DateTimeOffset? endDate, SuperDateRangePreset preset, string? periodName = null, bool resetVisibleMonth = true)
    {
        _draftValue = NormalizeRange(new SuperDateRangeSelection(startDate, endDate, preset, periodName));
        _draftSummaryOverride = periodName;
        if (resetVisibleMonth)
        {
            ResetVisibleMonth(_draftValue);
        }
    }

    private string GetSummaryText()
    {
        if (!string.IsNullOrWhiteSpace(_committedSummaryOverride))
        {
            return _committedSummaryOverride;
        }

        var summary = SuperDateRangePresetCalculator.GetSummary(_committedValue, Loc);
        return string.IsNullOrWhiteSpace(summary) ? EmptyText ?? Loc["DateRange.AllPeriod"] : summary;
    }

    private string GetToggleButtonCssClass()
    {
        return string.IsNullOrWhiteSpace(ButtonCssClass)
            ? "btn btn-outline-secondary d-inline-flex align-items-center justify-content-between gap-2"
            : ButtonCssClass;
    }

    private string? GetContainerStyle()
    {
        return string.IsNullOrWhiteSpace(MinWidth)
            ? null
            : $"--sdrp-min-width: {MinWidth};";
    }

    private async Task DetachFloatingPanelAsync()
    {
        if (_jsModule is null || !_floatingPanelAttached)
        {
            return;
        }

        try
        {
            await _jsModule.InvokeVoidAsync("detachFloatingPanel", _pickerRef);
            _floatingPanelAttached = false;
        }
        catch (JSDisconnectedException)
        {
        }
    }

    private string GetSelectedDaysText()
    {
        if (_draftValue.StartDate is not null && _draftValue.EndDate is not null)
        {
         var dayCount = (_draftValue.EndDate.Value - _draftValue.StartDate.Value).Days + 1;
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
        var isActive = _draftValue.Preset == preset;
        return isActive
            ? "list-group-item list-group-item-action active"
            : "list-group-item list-group-item-action";
    }

   private string GetInputDateValue(DateTimeOffset? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
    }

  private IReadOnlyList<DateTimeOffset> GetVisibleMonths()
    {
        return [_visibleMonthStart, _visibleMonthStart.AddMonths(1)];
    }

 private IReadOnlyList<SuperDateRangeCalendarWeek> GetMonthWeeks(DateTimeOffset month)
    {
     var firstDayOfMonth = CreateDate(month.Year, month.Month, 1);
        var lastDayOfMonth = CreateDate(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
        var calendarStart = GetStartOfWeek(firstDayOfMonth);
        var calendarEnd = GetEndOfWeek(lastDayOfMonth);
        var weeks = new List<SuperDateRangeCalendarWeek>();

        for (var weekStart = calendarStart; weekStart <= calendarEnd; weekStart = weekStart.AddDays(7))
        {
          var days = new List<DateTimeOffset?>(7);
            for (var dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                var currentDay = weekStart.AddDays(dayOffset);
                days.Add(currentDay.Month == month.Month ? currentDay : null);
            }

          var weekNumber = ISOWeek.GetWeekOfYear(weekStart.Date);
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

 private static string GetMonthTitle(DateTimeOffset month)
    {
        return month.ToString("MMMM yyyy", CultureInfo.CurrentUICulture).ToUpper(CultureInfo.CurrentUICulture);
    }

  private string GetDayButtonClass(DateTimeOffset day)
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

    private bool IsDayDisabled(DateTimeOffset day)
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

    private bool IsBoundaryDay(DateTimeOffset day)
    {
        return _draftValue.StartDate == day || _draftValue.EndDate == day;
    }

    private bool IsInSelectedRange(DateTimeOffset day)
    {
        return _draftValue.StartDate is not null
            && _draftValue.EndDate is not null
            && day > _draftValue.StartDate.Value
            && day < _draftValue.EndDate.Value;
    }

    private void ResetVisibleMonth(SuperDateRangeSelection range)
    {
        var referenceDate = range.StartDate ?? range.EndDate ?? GetToday();
        _visibleMonthStart = CreateDate(referenceDate.Year, referenceDate.Month, 1).AddMonths(-1);
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

 private static SuperDateRangePreset GetManualPreset(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        return startDate is null && endDate is null
            ? SuperDateRangePreset.AllTime
            : SuperDateRangePreset.Custom;
    }

   private static DateTimeOffset? ParseDate(object? value)
    {
        var rawValue = value?.ToString();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        return DateTime.TryParseExact(rawValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
            ? CreateDate(parsedDate.Year, parsedDate.Month, parsedDate.Day)
            : null;
    }

  private DateTimeOffset? NormalizeSelectableDate(DateTimeOffset? value)
    {
        if (value is null)
        {
            return null;
        }

        if (!DisableFutureDates)
        {
           return NormalizeDate(value.Value);
        }

        var today = GetToday();
       var normalizedValue = NormalizeDate(value.Value);
        return normalizedValue > today ? today : normalizedValue;
    }

 private (DateTimeOffset StartDate, DateTimeOffset EndDate)? GetSelectableWeekRange(SuperDateRangeCalendarWeek week)
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

  private static DateTimeOffset GetToday()
    {
      return NormalizeDate(DateTimeOffset.Now);
    }

 private static DateTimeOffset GetEndOfWeek(DateTimeOffset date)
    {
        var offset = 6 - ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(offset);
    }

   private static DateTimeOffset GetStartOfWeek(DateTimeOffset date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }

    private static DateTimeOffset NormalizeDate(DateTimeOffset value)
    {
        return CreateDate(value.Year, value.Month, value.Day);
    }

    private static DateTimeOffset CreateDate(int year, int month, int day)
    {
        var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        return new DateTimeOffset(date, TimeZoneInfo.Local.GetUtcOffset(date));
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is null)
        {
            return;
        }

        try
        {
            if (_floatingPanelAttached)
            {
                await DetachFloatingPanelAsync();
            }

            await _jsModule.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
