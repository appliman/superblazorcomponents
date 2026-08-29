using System.Security.Cryptography;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

namespace SuperBlazorComponents.DataGridExporter.Internal;

internal sealed partial class ExportFileStore
{
    private readonly SuperDataGridExporterOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ExportFileStore> _logger;

    public ExportFileStore(
        SuperDataGridExporterOptions options,
        TimeProvider timeProvider,
        ILogger<ExportFileStore> logger)
    {
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<SuperDataGridExportResult> CreateAsync(
        SuperDataGridExportFormat format,
        string requestedFileName,
        Func<string, CancellationToken, Task<int>> writer,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.TemporaryDirectory);

        var extension = GetExtension(format);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var finalPath = Path.Combine(_options.TemporaryDirectory, $"{token}.{extension}");
        var partialPath = Path.Combine(_options.TemporaryDirectory, $"{token}.partial.{extension}");
        var downloadName = SanitizeFileName(requestedFileName, extension);

        try
        {
            var rowCount = await writer(partialPath, cancellationToken);
            File.Move(partialPath, finalPath);
            File.SetLastWriteTimeUtc(finalPath, _timeProvider.GetUtcNow().UtcDateTime);

            var url = $"{_options.NormalizedDownloadRoute}/{token}/{extension}?fileName={Uri.EscapeDataString(downloadName)}";
            return new SuperDataGridExportResult(downloadName, url, rowCount);
        }
        catch
        {
            TryDelete(partialPath);
            TryDelete(finalPath);
            throw;
        }
    }

    public StoredExportFile? TryResolve(string token, string format, string? requestedFileName)
    {
        if (!TokenRegex().IsMatch(token) || !TryParseFormat(format, out var exportFormat))
            return null;

        var extension = GetExtension(exportFormat);
        var path = Path.Combine(_options.TemporaryDirectory, $"{token}.{extension}");
        if (!File.Exists(path))
            return null;

        var expiresAt = File.GetLastWriteTimeUtc(path) + _options.FileLifetime;
        if (expiresAt <= _timeProvider.GetUtcNow().UtcDateTime)
        {
            TryDelete(path);
            return null;
        }

        return new StoredExportFile(
            path,
            exportFormat == SuperDataGridExportFormat.Csv
                ? "text/csv; charset=utf-8"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            SanitizeFileName(requestedFileName, extension));
    }

    public Task CleanupExpiredAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_options.TemporaryDirectory))
            return Task.CompletedTask;

        var threshold = _timeProvider.GetUtcNow().UtcDateTime - _options.FileLifetime;
        foreach (var path in Directory.EnumerateFiles(_options.TemporaryDirectory, "*", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ManagedFileRegex().IsMatch(Path.GetFileName(path)))
                continue;

            try
            {
                if (File.GetLastWriteTimeUtc(path) <= threshold)
                    File.Delete(path);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(exception, "Unable to delete expired data-grid export file {FilePath}", path);
            }
        }

        return Task.CompletedTask;
    }

    internal static string SanitizeFileName(string? requestedFileName, string extension)
    {
        var name = Path.GetFileName(requestedFileName ?? string.Empty).Trim();
        if (name.EndsWith($".{extension}", StringComparison.OrdinalIgnoreCase))
            name = name[..^(extension.Length + 1)];
        else if (Path.HasExtension(name))
            name = Path.GetFileNameWithoutExtension(name);

        name = InvalidFileNameRegex().Replace(name, "_").Trim(' ', '.');
        if (string.IsNullOrWhiteSpace(name))
            name = "export";
        if (name.Length > 120)
            name = name[..120];

        return $"{name}.{extension}";
    }

    internal static string GetExtension(SuperDataGridExportFormat format)
        => format == SuperDataGridExportFormat.Csv ? "csv" : "xlsx";

    private static bool TryParseFormat(string value, out SuperDataGridExportFormat format)
    {
        if (string.Equals(value, "csv", StringComparison.OrdinalIgnoreCase))
        {
            format = SuperDataGridExportFormat.Csv;
            return true;
        }
        if (string.Equals(value, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            format = SuperDataGridExportFormat.Excel;
            return true;
        }

        format = default;
        return false;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }

    [GeneratedRegex("^[a-f0-9]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();

    [GeneratedRegex("^[a-f0-9]{64}(?:\\.partial)?\\.(?:csv|xlsx)$", RegexOptions.CultureInvariant)]
    private static partial Regex ManagedFileRegex();

    [GeneratedRegex("[<>:\"/\\\\|?*\\x00-\\x1F]", RegexOptions.CultureInvariant)]
    private static partial Regex InvalidFileNameRegex();
}
