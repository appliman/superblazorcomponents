using System.Globalization;
using System.Text;

using Bunit;

using ClosedXML.Excel;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.DataGridExporter;
using SuperBlazorComponents.DataGridExporter.Components;
using SuperBlazorComponents.DataGridExporter.Internal;

namespace SuperBlazorComponents.Tests;

#pragma warning disable BL0005 // Tests intentionally construct component instances to exercise the exporter directly.

[TestClass]
public sealed class SuperDataGridExporterTests
{
    private string _temporaryDirectory = null!;
    private readonly List<BunitContext> _contexts = [];
    private readonly Dictionary<SuperDataGrid<TestRow>, IRenderedComponent<SuperDataGrid<TestRow>>> _renderedGrids = [];

    [TestInitialize]
    public void Setup()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "SuperBlazorComponents.Tests",
            Guid.NewGuid().ToString("N"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var context in _contexts)
            context.Dispose();
        _contexts.Clear();
        _renderedGrids.Clear();

        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
    }

    [TestMethod]
    public void ColumnResolver_UsesConfiguredPriorityAndExportValue()
    {
        var grid = CreateGrid((request) =>
            ValueTask.FromResult(GridItemsProviderResult<TestRow>.Empty()));
        var column = AddColumn(grid, new TestColumn
        {
            Property = nameof(TestRow.Name),
            Title = "Title",
            ExportHeader = "Explicit",
            HeaderTemplate = builder => builder.AddContent(0, "Template"),
            ExportValue = row => row.Name.ToUpperInvariant()
        });
        AddColumn(grid, new TestColumn
        {
            Property = nameof(TestRow.Id),
            Title = "Hidden",
            Visible = false
        });

        var resolved = ExportColumnResolver.Resolve(grid);

        Assert.HasCount(1, resolved);
        Assert.AreEqual("Explicit", resolved[0].Header);
        Assert.AreEqual("ALPHA", resolved[0].ValueAccessor(new TestRow(1, "Alpha", 12.5m, true)));
        Assert.IsTrue(column.IsCurrentlyVisible);
    }

    [TestMethod]
    public void ColumnResolver_ExtractsSimpleHeaderTemplateAndRejectsMissingValue()
    {
        var grid = CreateGrid((request) =>
            ValueTask.FromResult(GridItemsProviderResult<TestRow>.Empty()));
        AddColumn(grid, new TestColumn
        {
            HeaderTemplate = builder =>
            {
                builder.OpenElement(0, "strong");
                builder.AddContent(1, "Custom header");
                builder.CloseElement();
            }
        });

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => ExportColumnResolver.Resolve(grid));

        StringAssert.Contains(exception.Message, "Custom header");
        StringAssert.Contains(exception.Message, "ExportValue");
    }

    [TestMethod]
    public async Task CsvExport_ReadsEveryBatchAndProtectsFormulaStrings()
    {
        var rows = Enumerable.Range(1, 5)
            .Select(index => new TestRow(index, index == 2 ? "=SUM(A1:A2)" : $"Name {index}", index + .25m, index % 2 == 0))
            .ToArray();
        var starts = new List<int>();
        var grid = CreateGrid(request =>
        {
            starts.Add(request.StartIndex);
            var page = rows.Skip(request.StartIndex).Take(request.Count ?? rows.Length).ToArray();
            return ValueTask.FromResult(GridItemsProviderResult<TestRow>.From(page, rows.Length));
        });
        AddColumn(grid, new TestColumn { Property = nameof(TestRow.Name), Title = "Name" });
        AddColumn(grid, new TestColumn { Property = nameof(TestRow.Amount), Title = "Amount", FormatString = "{0:F2}" });
        starts.Clear();

        var service = CreateService(batchSize: 2);
        var result = await service.ExportAsync(grid, SuperDataGridExportFormat.Csv, "../unsafe:report.csv");
        var file = Directory.GetFiles(_temporaryDirectory, "*.csv").Single();
        var content = await File.ReadAllTextAsync(file, Encoding.UTF8);

        Assert.AreEqual(5, result.RowCount);
        Assert.AreEqual("unsafe_report.csv", result.FileName);
        CollectionAssert.AreEqual(new[] { 0, 2, 4 }, starts);
        StringAssert.StartsWith(content, "Name,Amount");
        StringAssert.Contains(content, "'=SUM(A1:A2)");
        StringAssert.Contains(content, "1.25");
        StringAssert.Matches(result.DownloadUrl, new System.Text.RegularExpressions.Regex(
            @"/[a-f0-9]{64}/csv\?fileName="));
    }

    [TestMethod]
    public async Task ExcelExport_PreservesNativeCellTypes()
    {
        var rows = new[] { new TestRow(1, "Alpha", 12.5m, true) };
        var grid = CreateGrid(request => ValueTask.FromResult(
            GridItemsProviderResult<TestRow>.From(rows.Skip(request.StartIndex), rows.Length)));
        grid.FreezeLeftColumns = 2;
        AddColumn(grid, new TestColumn { Property = nameof(TestRow.Id), Title = "ID" });
        AddColumn(grid, new TestColumn { Property = nameof(TestRow.Amount), Title = "Amount" });
        AddColumn(grid, new TestColumn { Property = nameof(TestRow.Enabled), Title = "Enabled" });

        var result = await CreateService().ExportAsync(
            grid, SuperDataGridExportFormat.Excel, "typed");
        var file = Directory.GetFiles(_temporaryDirectory, "*.xlsx").Single();
        using var workbook = new XLWorkbook(file);
        var worksheet = workbook.Worksheet("Export");

        Assert.AreEqual(1, result.RowCount);
        Assert.AreEqual(XLDataType.Number, worksheet.Cell(2, 1).DataType);
        Assert.AreEqual(XLDataType.Number, worksheet.Cell(2, 2).DataType);
        Assert.AreEqual(XLDataType.Boolean, worksheet.Cell(2, 3).DataType);
        Assert.IsTrue(worksheet.SheetView.SplitRow > 0);
        Assert.AreEqual(2, worksheet.SheetView.SplitColumn);
    }

    [TestMethod]
    public async Task Export_FailsWhenProviderStopsBeforeAnnouncedTotal()
    {
        var grid = CreateGrid(request => ValueTask.FromResult(
            request.StartIndex == 0
                ? GridItemsProviderResult<TestRow>.From(
                    new[] { new TestRow(1, "One", 1, true) }, 2)
                : GridItemsProviderResult<TestRow>.From([], 2)));
        AddColumn(grid, new TestColumn { Property = nameof(TestRow.Id), Title = "ID" });

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => CreateService(batchSize: 1).ExportAsync(
                grid, SuperDataGridExportFormat.Csv, "broken"));

        StringAssert.Contains(exception.Message, "announced total");
        Assert.HasCount(0, Directory.Exists(_temporaryDirectory)
            ? Directory.GetFiles(_temporaryDirectory)
            : []);
    }

    [TestMethod]
    public async Task Export_HonorsCancellationWithoutPublishingAFile()
    {
        var grid = CreateGrid(request => ValueTask.FromResult(
            GridItemsProviderResult<TestRow>.From(
                new[] { new TestRow(1, "One", 1, true) }, 1)));
        AddColumn(grid, new TestColumn { Property = nameof(TestRow.Id), Title = "ID" });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => CreateService().ExportAsync(
                grid,
                SuperDataGridExportFormat.Csv,
                "cancelled",
                cancellation.Token));

        Assert.HasCount(0, Directory.Exists(_temporaryDirectory)
            ? Directory.GetFiles(_temporaryDirectory)
            : []);
    }

    [TestMethod]
    public async Task FileStore_RejectsExpiredTokenAndCleansOldPartialFiles()
    {
        var clock = new ManualTimeProvider(new DateTimeOffset(2026, 8, 29, 10, 0, 0, TimeSpan.Zero));
        var options = CreateOptions();
        options.FileLifetime = TimeSpan.FromHours(1);
        var store = new ExportFileStore(options, clock, NullLogger<ExportFileStore>.Instance);
        var result = await store.CreateAsync(
            SuperDataGridExportFormat.Csv,
            "test",
            async (path, cancellationToken) =>
            {
                await File.WriteAllTextAsync(path, "value", cancellationToken);
                return 1;
            },
            CancellationToken.None);
        var token = result.DownloadUrl.Split('/', StringSplitOptions.RemoveEmptyEntries)[1];
        var partial = Path.Combine(_temporaryDirectory, $"{new string('a', 64)}.partial.csv");
        await File.WriteAllTextAsync(partial, "partial");
        File.SetLastWriteTimeUtc(partial, clock.GetUtcNow().UtcDateTime - TimeSpan.FromHours(2));

        clock.Advance(TimeSpan.FromHours(2));
        Assert.IsNull(store.TryResolve(token, "csv", "test.csv"));
        await store.CleanupExpiredAsync(CancellationToken.None);

        Assert.IsFalse(File.Exists(partial));
        Assert.HasCount(0, Directory.GetFiles(_temporaryDirectory));
    }

    [TestMethod]
    public void ExportButtons_RenderSuperButtonIconsAndDisableWithoutGrid()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSuperComponents();

        var csv = context.Render<SuperDataGridCsvExportButton<TestRow>>();
        var excel = context.Render<SuperDataGridExcelExportButton<TestRow>>();

        Assert.HasCount(1, csv.FindAll(".fa-file-csv"));
        var excelLogo = excel.Find("img.super-button-image");
        Assert.AreEqual(
            "_content/SuperBlazorComponents.DataGridExporter/icons/microsoft-excel.svg",
            excelLogo.GetAttribute("src"));
        Assert.IsTrue(csv.Find("button").HasAttribute("disabled"));
        Assert.IsTrue(excel.Find("button").HasAttribute("disabled"));
    }

    [TestMethod]
    public void ExportButtons_IconOnlyHidesTextAndKeepsAccessibleLabel()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSuperComponents();

        var csv = context.Render<SuperDataGridCsvExportButton<TestRow>>(parameters => parameters
            .Add(component => component.IconOnly, true));
        var button = csv.Find("button");

        Assert.AreEqual("Exporter CSV", button.GetAttribute("title"));
        Assert.AreEqual("Exporter CSV", button.GetAttribute("aria-label"));
        Assert.IsFalse(button.TextContent.Contains("Exporter CSV", StringComparison.Ordinal));
        Assert.HasCount(1, button.QuerySelectorAll(".fa-file-csv"));
    }

    [TestMethod]
    public void ExportDialog_AfterGenerationDisplaysDownloadLink()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSuperComponents();
        context.Services.AddSingleton<ISuperDataGridExportService>(new StubExportService());
        var grid = new SuperDataGrid<TestRow>
        {
            ItemsProvider = request => ValueTask.FromResult(GridItemsProviderResult<TestRow>.Empty())
        };

        var dialog = context.Render<SuperDataGridExportDialog<TestRow>>(parameters => parameters
            .Add(component => component.Grid, grid)
            .Add(component => component.Format, SuperDataGridExportFormat.Csv)
            .Add(component => component.DefaultFileName, "products"));

        dialog.Find("button.btn-primary").Click();

        dialog.WaitForAssertion(() =>
        {
            var link = dialog.Find("a.btn-primary");
            Assert.AreEqual("/exports/token/csv", link.GetAttribute("href"));
            StringAssert.Contains(dialog.Markup, "products.csv");
        });
    }

    private SuperDataGridExportService CreateService(int batchSize = 200)
    {
        var options = CreateOptions();
        options.BatchSize = batchSize;
        var store = new ExportFileStore(options, TimeProvider.System, NullLogger<ExportFileStore>.Instance);
        return new SuperDataGridExportService(options, store);
    }

    private SuperDataGridExporterOptions CreateOptions() => new()
    {
        TemporaryDirectory = _temporaryDirectory,
        FileLifetime = TimeSpan.FromHours(24),
        CleanupInterval = TimeSpan.FromDays(1),
        CsvCulture = CultureInfo.InvariantCulture
    };

    private SuperDataGrid<TestRow> CreateGrid(GridItemsProvider<TestRow> provider)
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddLogging();
        context.Services.AddSuperComponents();
        _contexts.Add(context);

        var renderedGrid = context.Render<SuperDataGrid<TestRow>>(parameters => parameters
            .Add(component => component.ItemsProvider, provider));
        _renderedGrids.Add(renderedGrid.Instance, renderedGrid);
        return renderedGrid.Instance;
    }

    private TestColumn AddColumn(SuperDataGrid<TestRow> grid, TestColumn column)
    {
        column.Initialize();
        _renderedGrids[grid].InvokeAsync(
            () => grid.AddColumn(grid.ColumnsCollection.Count, column)).GetAwaiter().GetResult();
        return column;
    }

    private sealed class TestColumn : DataGridColumn<TestRow>
    {
        public void Initialize()
        {
            base.OnInitialized();
            base.OnParametersSet();
        }
    }

    private sealed record TestRow(int Id, string Name, decimal Amount, bool Enabled);

    private sealed class StubExportService : ISuperDataGridExportService
    {
        public Task<SuperDataGridExportResult> ExportAsync<TItem>(
            SuperDataGrid<TItem> grid,
            SuperDataGridExportFormat format,
            string fileName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new SuperDataGridExportResult(
                "products.csv", "/exports/token/csv", 42));
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;
        public override DateTimeOffset GetUtcNow() => _utcNow;
        public void Advance(TimeSpan duration) => _utcNow += duration;
    }
}
#pragma warning restore BL0005
