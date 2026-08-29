using System.Globalization;
using System.Text;

namespace SuperBlazorComponents.DataGridExporter;

public sealed class SuperDataGridExporterOptions
{
    public string TemporaryDirectory { get; set; } = Path.Combine(
        Path.GetTempPath(), "SuperBlazorComponents.DataGridExporter");

    public TimeSpan FileLifetime { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromDays(1);
    public string DownloadRoute { get; set; } = "/_super-datagrid-export";
    public int BatchSize { get; set; } = 200;
    public string CsvDelimiter { get; set; } = ",";
    public Encoding CsvEncoding { get; set; } = new UTF8Encoding(false);
    public CultureInfo CsvCulture { get; set; } = CultureInfo.InvariantCulture;
    public bool ProtectCsvFormulas { get; set; } = true;

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(TemporaryDirectory))
            throw new InvalidOperationException("TemporaryDirectory must be configured.");
        if (FileLifetime <= TimeSpan.Zero)
            throw new InvalidOperationException("FileLifetime must be greater than zero.");
        if (CleanupInterval <= TimeSpan.Zero)
            throw new InvalidOperationException("CleanupInterval must be greater than zero.");
        if (BatchSize <= 0)
            throw new InvalidOperationException("BatchSize must be greater than zero.");
        if (string.IsNullOrEmpty(CsvDelimiter))
            throw new InvalidOperationException("CsvDelimiter must not be empty.");
        if (string.IsNullOrWhiteSpace(DownloadRoute))
            throw new InvalidOperationException("DownloadRoute must be configured.");
    }

    internal string NormalizedDownloadRoute => "/" + DownloadRoute.Trim('/');
}
