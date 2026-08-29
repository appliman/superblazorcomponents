namespace SuperBlazorComponents.DataGridExporter.Internal;

internal sealed record ExportColumn<TItem>(
    string Header,
    string? FormatString,
    Func<TItem, object?> ValueAccessor);
