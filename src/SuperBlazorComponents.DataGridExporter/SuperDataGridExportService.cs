using System.Globalization;
using System.Runtime.CompilerServices;

using ClosedXML.Excel;

using CsvHelper;
using CsvHelper.Configuration;

using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.DataGridExporter.Internal;

namespace SuperBlazorComponents.DataGridExporter;

internal sealed class SuperDataGridExportService : ISuperDataGridExportService
{
    private const int ExcelMaximumDataRows = 1_048_575;

    private readonly SuperDataGridExporterOptions _options;
    private readonly ExportFileStore _fileStore;

    public SuperDataGridExportService(
        SuperDataGridExporterOptions options,
        ExportFileStore fileStore)
    {
        _options = options;
        _fileStore = fileStore;
    }

    public Task<SuperDataGridExportResult> ExportAsync<TItem>(
        SuperDataGrid<TItem> grid,
        SuperDataGridExportFormat format,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grid);
        if (grid.ItemsProvider is null)
            throw new InvalidOperationException("The grid must define an ItemsProvider to export all rows.");

        var columns = ExportColumnResolver.Resolve(grid);
        var query = grid.CaptureQuerySnapshot();
        var frozenColumnCount = grid.ColumnsCollection
            .Where(column => column.IsCurrentlyVisible)
            .Take(Math.Max(0, grid.FreezeLeftColumns))
            .Count(column => column.Exportable);

        return _fileStore.CreateAsync(
            format,
            fileName,
            (path, token) => format == SuperDataGridExportFormat.Csv
                ? WriteCsvAsync(path, grid.ItemsProvider, query, columns, token)
                : WriteExcelAsync(path, grid.ItemsProvider, query, columns, frozenColumnCount, token),
            cancellationToken);
    }

    private async Task<int> WriteCsvAsync<TItem>(
        string path,
        GridItemsProvider<TItem> provider,
        SuperDataGridQuerySnapshot query,
        IReadOnlyList<ExportColumn<TItem>> columns,
        CancellationToken cancellationToken)
    {
        var configuration = new CsvConfiguration(_options.CsvCulture)
        {
            Delimiter = _options.CsvDelimiter,
            HasHeaderRecord = true
        };

        await using var stream = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true);
        await using var textWriter = new StreamWriter(stream, _options.CsvEncoding);
        using var csv = new CsvWriter(textWriter, configuration);

        foreach (var column in columns)
            csv.WriteField(column.Header);
        await csv.NextRecordAsync();

        var rowCount = 0;
        await foreach (var item in ReadAllAsync(provider, query, cancellationToken))
        {
            foreach (var column in columns)
            {
                var value = column.ValueAccessor(item);
                var formatted = FormatCsvValue(value, column.FormatString);
                csv.WriteField(ProtectCsvValue(value, formatted));
            }

            await csv.NextRecordAsync();
            rowCount++;
        }

        await textWriter.FlushAsync(cancellationToken);
        return rowCount;
    }

    private async Task<int> WriteExcelAsync<TItem>(
        string path,
        GridItemsProvider<TItem> provider,
        SuperDataGridQuerySnapshot query,
        IReadOnlyList<ExportColumn<TItem>> columns,
        int frozenColumnCount,
        CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Export");

        for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            var cell = worksheet.Cell(1, columnIndex + 1);
            cell.SetValue(columns[columnIndex].Header);
            cell.Style.Font.Bold = true;
        }

        var rowCount = 0;
        await foreach (var item in ReadAllAsync(provider, query, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rowCount >= ExcelMaximumDataRows)
            {
                throw new InvalidOperationException(
                    $"Excel supports at most {ExcelMaximumDataRows:N0} data rows per worksheet.");
            }

            var rowNumber = rowCount + 2;
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                SetExcelValue(worksheet.Cell(rowNumber, columnIndex + 1), columns[columnIndex].ValueAccessor(item));

            rowCount++;
        }

        worksheet.SheetView.FreezeRows(1);
        if (frozenColumnCount > 0)
            worksheet.SheetView.FreezeColumns(Math.Min(frozenColumnCount, columns.Count));
        worksheet.Range(1, 1, rowCount + 1, columns.Count).SetAutoFilter();
        worksheet.Columns(1, columns.Count).AdjustToContents(
            1,
            Math.Min(rowCount + 1, 10_000));
        workbook.SaveAs(path);
        return rowCount;
    }

    private async IAsyncEnumerable<TItem> ReadAllAsync<TItem>(
        GridItemsProvider<TItem> provider,
        SuperDataGridQuerySnapshot query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var startIndex = 0;
        int? expectedTotal = null;
        var filters = query.Filters.Select(filter => filter.ToFilterInfo()).ToArray();

        while (expectedTotal is null || startIndex < expectedTotal.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new GridItemsProviderRequest<TItem>(
                startIndex,
                _options.BatchSize,
                query.SortColumn,
                query.SortDirection,
                filters,
                cancellationToken);
            var result = await provider(request);
            expectedTotal ??= Math.Max(0, result.TotalItemCount);
            var items = result.Items.Take(_options.BatchSize).ToArray();

            if (items.Length == 0)
            {
                if (startIndex < expectedTotal.Value)
                {
                    throw new InvalidOperationException(
                        $"The grid ItemsProvider returned no rows at index {startIndex} before the announced total of {expectedTotal.Value} rows.");
                }
                yield break;
            }

            foreach (var item in items)
                yield return item;

            startIndex += items.Length;
        }
    }

    private string FormatCsvValue(object? value, string? formatString)
    {
        if (value is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(formatString))
            return string.Format(_options.CsvCulture, formatString, value);

        return value switch
        {
            DateTime dateTime => dateTime.ToString("O", _options.CsvCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", _options.CsvCulture),
            IFormattable formattable => formattable.ToString(null, _options.CsvCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private string ProtectCsvValue(object? source, string formatted)
    {
        if (!_options.ProtectCsvFormulas || source is not string || string.IsNullOrEmpty(formatted))
            return formatted;

        var firstMeaningful = formatted.FirstOrDefault(character => !char.IsWhiteSpace(character));
        return firstMeaningful is '=' or '+' or '-' or '@' ? "'" + formatted : formatted;
    }

    private static void SetExcelValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null: cell.Clear(XLClearOptions.Contents); break;
            case string text: cell.SetValue(text); break;
            case bool boolean: cell.SetValue(boolean); break;
            case byte number: cell.SetValue(number); break;
            case sbyte number: cell.SetValue(number); break;
            case short number: cell.SetValue(number); break;
            case ushort number: cell.SetValue(number); break;
            case int number: cell.SetValue(number); break;
            case uint number: cell.SetValue(number); break;
            case long number: cell.SetValue(number); break;
            case ulong number when number <= long.MaxValue: cell.SetValue((long)number); break;
            case float number: cell.SetValue(number); break;
            case double number: cell.SetValue(number); break;
            case decimal number: cell.SetValue(number); break;
            case DateTime dateTime: cell.SetValue(dateTime); break;
            case DateTimeOffset dateTimeOffset: cell.SetValue(dateTimeOffset.DateTime); break;
            case DateOnly date: cell.SetValue(date.ToDateTime(TimeOnly.MinValue)); break;
            case TimeSpan timeSpan: cell.SetValue(timeSpan); break;
            case Enum enumValue: cell.SetValue(enumValue.ToString()); break;
            default: cell.SetValue(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty); break;
        }
    }
}
