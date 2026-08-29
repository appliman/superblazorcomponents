using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SuperBlazorComponents.DataGridExporter.Internal;

internal sealed class ExportFileCleanupService : BackgroundService
{
    private readonly ExportFileStore _store;
    private readonly SuperDataGridExporterOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ExportFileCleanupService> _logger;

    public ExportFileCleanupService(
        ExportFileStore store,
        SuperDataGridExporterOptions options,
        TimeProvider timeProvider,
        ILogger<ExportFileCleanupService> logger)
    {
        _store = store;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanupAsync(stoppingToken);
        using var timer = new PeriodicTimer(_options.CleanupInterval, _timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            await CleanupAsync(stoppingToken);
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _store.CleanupExpiredAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Data-grid export cleanup failed.");
        }
    }
}
