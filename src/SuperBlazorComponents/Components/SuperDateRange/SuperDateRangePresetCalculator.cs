using System.Globalization;

using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents.Components.SuperDateRange;

internal static class SuperDateRangePresetCalculator
{
  public static SuperDateRangeSelection Resolve(SuperDateRangePreset preset, DateTimeOffset today)
    {
        return preset switch
        {
            SuperDateRangePreset.Today => new SuperDateRangeSelection(today, today, preset),
            SuperDateRangePreset.Yesterday => new SuperDateRangeSelection(today.AddDays(-1), today.AddDays(-1), preset),
            SuperDateRangePreset.ThisWeek => new SuperDateRangeSelection(GetStartOfWeek(today), today, preset),
            SuperDateRangePreset.LastWeek => GetLastWeek(today),
            SuperDateRangePreset.Last7Days => new SuperDateRangeSelection(today.AddDays(-6), today, preset),
            SuperDateRangePreset.Last14Days => new SuperDateRangeSelection(today.AddDays(-13), today, preset),
            SuperDateRangePreset.Last30Days => new SuperDateRangeSelection(today.AddDays(-29), today, preset),
            SuperDateRangePreset.Last90Days => new SuperDateRangeSelection(today.AddDays(-89), today, preset),
         SuperDateRangePreset.ThisMonth => new SuperDateRangeSelection(CreateDate(today.Year, today.Month, 1), today, preset),
            SuperDateRangePreset.LastMonth => GetLastMonth(today),
            SuperDateRangePreset.ThisQuarter => new SuperDateRangeSelection(GetQuarterStart(today), today, preset),
            SuperDateRangePreset.LastQuarter => GetLastQuarter(today),
            SuperDateRangePreset.ThisYear => new SuperDateRangeSelection(CreateDate(today.Year, 1, 1), today, preset),
            SuperDateRangePreset.LastYear => new SuperDateRangeSelection(CreateDate(today.Year - 1, 1, 1), CreateDate(today.Year - 1, 12, 31), preset),
            SuperDateRangePreset.Last12Months => GetLastMonths(today, 12, preset),
            SuperDateRangePreset.Last13Months => GetLastMonths(today, 13, preset),
            SuperDateRangePreset.Last24Months => GetLastMonths(today, 24, preset),
            SuperDateRangePreset.AllTime => new SuperDateRangeSelection(null, null, preset),
            _ => new SuperDateRangeSelection(null, null, SuperDateRangePreset.Custom)
        };
    }

    public static string GetLabel(SuperDateRangePreset preset, IStringLocalizer loc)
    {
        return preset switch
        {
            SuperDateRangePreset.Custom => loc["DateRange.Custom"],
            _ => loc[$"DateRange.Preset.{preset}"]
        };
    }

    public static string GetSummary(SuperDateRangeSelection range, IStringLocalizer loc)
    {
        if (!string.IsNullOrWhiteSpace(range.PeriodName))
        {
            return range.PeriodName;
        }

        if (range.Preset != SuperDateRangePreset.Custom)
        {
            return GetLabel(range.Preset, loc);
        }

        if (range.StartDate is null && range.EndDate is null)
        {
            return GetLabel(SuperDateRangePreset.AllTime, loc);
        }

        var culture = CultureInfo.CurrentUICulture;

        if (range.StartDate is not null && range.EndDate is not null)
        {
            if (range.StartDate == range.EndDate)
            {
                return range.StartDate.Value.ToString("d MMM yyyy", culture);
            }

            return $"{range.StartDate.Value.ToString("d MMM yyyy", culture)} - {range.EndDate.Value.ToString("d MMM yyyy", culture)}";
        }

        if (range.StartDate is not null)
        {
            return string.Format(culture, loc["DateRange.Since"], range.StartDate.Value.ToString("d MMM yyyy", culture));
        }

        return string.Format(culture, loc["DateRange.Until"], range.EndDate!.Value.ToString("d MMM yyyy", culture));
    }

  private static SuperDateRangeSelection GetLastWeek(DateTimeOffset today)
    {
        var startOfCurrentWeek = GetStartOfWeek(today);
        var startOfLastWeek = startOfCurrentWeek.AddDays(-7);
        return new SuperDateRangeSelection(startOfLastWeek, startOfLastWeek.AddDays(6), SuperDateRangePreset.LastWeek);
    }

 private static SuperDateRangeSelection GetLastMonth(DateTimeOffset today)
    {
      var firstDayOfCurrentMonth = CreateDate(today.Year, today.Month, 1);
        var lastDayOfLastMonth = firstDayOfCurrentMonth.AddDays(-1);
       var firstDayOfLastMonth = CreateDate(lastDayOfLastMonth.Year, lastDayOfLastMonth.Month, 1);
        return new SuperDateRangeSelection(firstDayOfLastMonth, lastDayOfLastMonth, SuperDateRangePreset.LastMonth);
    }

   private static SuperDateRangeSelection GetLastQuarter(DateTimeOffset today)
    {
        var currentQuarterStart = GetQuarterStart(today);
        var lastQuarterEnd = currentQuarterStart.AddDays(-1);
        var lastQuarterStart = GetQuarterStart(lastQuarterEnd);
        return new SuperDateRangeSelection(lastQuarterStart, lastQuarterEnd, SuperDateRangePreset.LastQuarter);
    }

   private static SuperDateRangeSelection GetLastMonths(DateTimeOffset today, int monthCount, SuperDateRangePreset preset)
    {
       var currentMonthStart = CreateDate(today.Year, today.Month, 1);
        var rangeStart = currentMonthStart.AddMonths(-(monthCount - 1));
        return new SuperDateRangeSelection(rangeStart, today, preset);
    }

  private static DateTimeOffset GetQuarterStart(DateTimeOffset date)
    {
        var quarterMonth = ((date.Month - 1) / 3) * 3 + 1;
        return CreateDate(date.Year, quarterMonth, 1);
    }

   private static DateTimeOffset GetStartOfWeek(DateTimeOffset date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }

    private static DateTimeOffset CreateDate(int year, int month, int day)
    {
        var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        return new DateTimeOffset(date, TimeZoneInfo.Local.GetUtcOffset(date));
    }
}
