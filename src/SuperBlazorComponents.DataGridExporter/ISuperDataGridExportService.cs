using SuperBlazorComponents.Components.SuperDataGrid;

namespace SuperBlazorComponents.DataGridExporter;

public interface ISuperDataGridExportService
{
    Task<SuperDataGridExportResult> ExportAsync<TItem>(
        SuperDataGrid<TItem> grid,
        SuperDataGridExportFormat format,
        string fileName,
        CancellationToken cancellationToken = default);
}
