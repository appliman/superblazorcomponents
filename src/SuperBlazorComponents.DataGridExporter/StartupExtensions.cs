using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuperBlazorComponents.DataGridExporter.Internal;

namespace SuperBlazorComponents.DataGridExporter;

public static class StartupExtensions
{
    public static IServiceCollection AddSuperDataGridExporter(
        this IServiceCollection services,
        Action<SuperDataGridExporterOptions>? configure = null)
    {
        var options = new SuperDataGridExporterOptions();
        configure?.Invoke(options);
        options.Validate();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(options);
        services.AddSingleton<ExportFileStore>();
        services.AddScoped<ISuperDataGridExportService, SuperDataGridExportService>();
        services.AddHostedService<ExportFileCleanupService>();
        return services;
    }

    public static IEndpointConventionBuilder MapSuperDataGridExporter(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<SuperDataGridExporterOptions>();
        var pattern = $"{options.NormalizedDownloadRoute}/{{token}}/{{format}}";

        return endpoints.MapGet(pattern, (
            string token,
            string format,
            string? fileName,
            ExportFileStore store) =>
        {
            var file = store.TryResolve(token, format, fileName);
            return file is null
                ? Results.NotFound()
                : Results.File(file.Path, file.ContentType, file.DownloadName, enableRangeProcessing: true);
        }).AllowAnonymous();
    }
}
