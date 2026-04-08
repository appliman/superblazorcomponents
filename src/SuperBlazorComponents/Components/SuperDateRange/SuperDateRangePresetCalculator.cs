using System.Globalization;

namespace SuperBlazorComponents.Components.SuperDateRange;

internal static class SuperDateRangePresetCalculator
{
    public static SuperDateRangeSelection Resolve(SuperDateRangePreset preset, DateTime today)
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
            SuperDateRangePreset.ThisMonth => new SuperDateRangeSelection(new DateTime(today.Year, today.Month, 1), today, preset),
            SuperDateRangePreset.LastMonth => GetLastMonth(today),
            SuperDateRangePreset.ThisQuarter => new SuperDateRangeSelection(GetQuarterStart(today), today, preset),
            SuperDateRangePreset.LastQuarter => GetLastQuarter(today),
            SuperDateRangePreset.ThisYear => new SuperDateRangeSelection(new DateTime(today.Year, 1, 1), today, preset),
            SuperDateRangePreset.LastYear => new SuperDateRangeSelection(new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year - 1, 12, 31), preset),
            SuperDateRangePreset.Last12Months => GetLastMonths(today, 12, preset),
            SuperDateRangePreset.Last13Months => GetLastMonths(today, 13, preset),
            SuperDateRangePreset.Last24Months => GetLastMonths(today, 24, preset),
            SuperDateRangePreset.AllTime => new SuperDateRangeSelection(null, null, preset),
            _ => new SuperDateRangeSelection(null, null, SuperDateRangePreset.Custom)
        };
    }

    public static string GetLabel(SuperDateRangePreset preset)
    {
        return preset switch
        {
            SuperDateRangePreset.Today => "Aujourd'hui",
            SuperDateRangePreset.Yesterday => "Hier",
            SuperDateRangePreset.ThisWeek => "Cette semaine (lun. - aujourd'hui)",
            SuperDateRangePreset.LastWeek => "La semaine dernière (lun. - dim.)",
            SuperDateRangePreset.Last7Days => "7 derniers jours",
            SuperDateRangePreset.Last14Days => "14 derniers jours",
            SuperDateRangePreset.Last30Days => "30 derniers jours",
            SuperDateRangePreset.Last90Days => "90 derniers jours",
            SuperDateRangePreset.ThisMonth => "Ce mois-ci",
            SuperDateRangePreset.LastMonth => "Le mois dernier",
            SuperDateRangePreset.ThisQuarter => "Ce trimestre",
            SuperDateRangePreset.LastQuarter => "Le trimestre dernier",
            SuperDateRangePreset.ThisYear => "Cette année",
            SuperDateRangePreset.LastYear => "L'année dernière",
            SuperDateRangePreset.Last12Months => "12 derniers mois",
            SuperDateRangePreset.Last13Months => "13 derniers mois",
            SuperDateRangePreset.Last24Months => "24 derniers mois",
            SuperDateRangePreset.AllTime => "Toute la période",
            _ => "Personnalisée"
        };
    }

    public static string GetSummary(SuperDateRangeSelection range, CultureInfo culture)
    {
        if (!string.IsNullOrWhiteSpace(range.PeriodName))
        {
            return range.PeriodName;
        }

        if (range.Preset != SuperDateRangePreset.Custom)
        {
            return GetLabel(range.Preset);
        }

        if (range.StartDate is null && range.EndDate is null)
        {
            return GetLabel(SuperDateRangePreset.AllTime);
        }

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
            return $"Depuis le {range.StartDate.Value.ToString("d MMM yyyy", culture)}";
        }

        return $"Jusqu'au {range.EndDate!.Value.ToString("d MMM yyyy", culture)}";
    }

    private static SuperDateRangeSelection GetLastWeek(DateTime today)
    {
        var startOfCurrentWeek = GetStartOfWeek(today);
        var startOfLastWeek = startOfCurrentWeek.AddDays(-7);
        return new SuperDateRangeSelection(startOfLastWeek, startOfLastWeek.AddDays(6), SuperDateRangePreset.LastWeek);
    }

    private static SuperDateRangeSelection GetLastMonth(DateTime today)
    {
        var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var lastDayOfLastMonth = firstDayOfCurrentMonth.AddDays(-1);
        var firstDayOfLastMonth = new DateTime(lastDayOfLastMonth.Year, lastDayOfLastMonth.Month, 1);
        return new SuperDateRangeSelection(firstDayOfLastMonth, lastDayOfLastMonth, SuperDateRangePreset.LastMonth);
    }

    private static SuperDateRangeSelection GetLastQuarter(DateTime today)
    {
        var currentQuarterStart = GetQuarterStart(today);
        var lastQuarterEnd = currentQuarterStart.AddDays(-1);
        var lastQuarterStart = GetQuarterStart(lastQuarterEnd);
        return new SuperDateRangeSelection(lastQuarterStart, lastQuarterEnd, SuperDateRangePreset.LastQuarter);
    }

    private static SuperDateRangeSelection GetLastMonths(DateTime today, int monthCount, SuperDateRangePreset preset)
    {
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);
        var rangeStart = currentMonthStart.AddMonths(-(monthCount - 1));
        return new SuperDateRangeSelection(rangeStart, today, preset);
    }

    private static DateTime GetQuarterStart(DateTime date)
    {
        var quarterMonth = ((date.Month - 1) / 3) * 3 + 1;
        return new DateTime(date.Year, quarterMonth, 1);
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }
}
