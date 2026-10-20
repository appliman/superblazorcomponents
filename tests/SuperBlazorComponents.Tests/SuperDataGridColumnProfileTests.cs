using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.Components.SuperDataGrid.Tools;

namespace SuperBlazorComponents.Tests;

[TestClass]
public sealed class SuperDataGridColumnProfileTests
{
    [TestMethod]
    public async Task SwitchRestoresAndAutomaticallySavesOrderVisibilityAndWidths()
    {
        using var context = CreateContext(new InMemorySettingsStorage());
        var grid = RenderGrid(context);
        await grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync("Initial"));
        var initial = grid.Instance.ActiveColumnProfileId!;
        await grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync("Commercial"));
        var commercial = grid.Instance.ActiveColumnProfileId!;
        await grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        await grid.InvokeAsync(() => grid.Instance.SetColumnVisibilityAsync(0, false));
        // No system columns in this fixture: visible index zero is A after hiding B.
        await grid.InvokeAsync(() => grid.Instance.OnResizeComplete(0, 245));
        await grid.InvokeAsync(() => grid.Instance.SelectColumnProfileAsync(initial));
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        Assert.IsTrue(grid.Instance.GetColumnSettings().All(c => c.IsVisible));
        Assert.AreEqual("100px", grid.Instance.GetColumnSettings().First().Width);
        await grid.InvokeAsync(() => grid.Instance.SelectColumnProfileAsync(commercial));
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
        Assert.IsFalse(grid.Instance.GetColumnSettings().First().IsVisible);
        Assert.AreEqual("245px", grid.Instance.GetColumnSettings().Single(c => c.PropertyName == "A").Width);
        Assert.AreEqual("100px", grid.Instance.ColumnProfiles.Single(p => p.Id == initial).Columns.First().Width);
    }

    [TestMethod]
    public async Task ReloadRenameDeleteAndResetPreserveIndependentProfiles()
    {
        var storage = new InMemorySettingsStorage();
        using var context = CreateContext(storage);
        var grid = RenderGrid(context);
        await grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync("Work"));
        var id = grid.Instance.ActiveColumnProfileId!;
        await grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        grid.Dispose();
        grid = RenderGrid(context);
        Assert.AreEqual(id, grid.Instance.ActiveColumnProfileId);
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
        await grid.InvokeAsync(() => grid.Instance.RenameColumnProfileAsync(id, "  Sales  "));
        Assert.AreEqual("Sales", grid.Instance.ColumnProfiles.Single().Name);
        await grid.InvokeAsync(() => grid.Instance.ResetColumnSettingsAsync());
        Assert.IsNull(grid.Instance.ActiveColumnProfileId);
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        Assert.AreEqual(1, grid.Instance.ColumnProfiles.Count);
        await grid.InvokeAsync(() => grid.Instance.SelectColumnProfileAsync(id));
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
        await grid.InvokeAsync(() => grid.Instance.DeleteColumnProfileAsync(id));
        Assert.IsNull(grid.Instance.ActiveColumnProfileId);
        Assert.AreEqual(0, grid.Instance.ColumnProfiles.Count);
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
        grid.Dispose();
        grid = RenderGrid(context);
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
    }

    [TestMethod]
    public async Task JsonRoundTripAndInvalidImportLeaveViewUntouched()
    {
        using var context = CreateContext(new InMemorySettingsStorage());
        var grid = RenderGrid(context);
        await grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync("Source"));
        await grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        var json = grid.Instance.ExportColumnProfileJson();
        Assert.AreEqual(1, JsonDocument.Parse(json).RootElement.GetProperty("version").GetInt32());
        await grid.InvokeAsync(() => grid.Instance.ResetColumnSettingsAsync());
        await grid.InvokeAsync(() => grid.Instance.ImportColumnProfileAsync(json, "Imported"));
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
        var before = grid.Instance.ExportColumnProfileJson();
        await Assert.ThrowsAsync<ArgumentException>(() => grid.InvokeAsync(() => grid.Instance.ImportColumnProfileAsync(json, " imported ")));
        await Assert.ThrowsAsync<JsonException>(() => grid.InvokeAsync(() => grid.Instance.ImportColumnProfileAsync("{", "Bad")));
        await Assert.ThrowsAsync<ArgumentException>(() => grid.InvokeAsync(() => grid.Instance.ImportColumnProfileAsync(json.Replace("\"version\": 1", "\"version\": 99"), "Future")));
        Assert.AreEqual(before, grid.Instance.ExportColumnProfileJson());
        Assert.AreEqual(2, grid.Instance.ColumnProfiles.Count);
    }

    [TestMethod]
    public async Task ImportHonorsConstraintsAndIgnoresRemovedColumns()
    {
        using var context = CreateContext(new InMemorySettingsStorage());
        var grid = RenderGrid(context, constrained: true);
        var document = new SuperDataGridColumnProfileDocument
        {
            Name = "External",
            Columns = new[] { "C", "B", "Removed", "A" }.Select((name, index) => new SuperDataGridColumnSettings
            {
                PropertyName = name, Order = index, IsVisible = false, Width = "250px"
            }).ToArray()
        };
        await grid.InvokeAsync(() => grid.Instance.ImportColumnProfileAsync(document.ToJson(), "Imported"));
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        var fixedColumn = grid.Instance.GetColumnSettings().Single(c => c.PropertyName == "B");
        Assert.IsTrue(fixedColumn.IsVisible);
        Assert.AreEqual("100px", fixedColumn.Width);
        Assert.AreEqual("250px", grid.Instance.GetColumnSettings().First().Width);
    }

    [TestMethod]
    public async Task MissingAndNewColumnsAndGridIsolationAreHandled()
    {
        var storage = new InMemorySettingsStorage();
        await storage.SaveProfilesAsync("profiles", new SuperDataGridColumnProfiles
        {
            ActiveProfileId = "old",
            Profiles = [new SuperDataGridColumnProfile
            {
                Id = "old", Name = "Old",
                Columns = [new() { PropertyName = "B", Order = 0, IsVisible = false }, new() { PropertyName = "Deleted", Order = 1 }]
            }]
        });
        using var context = CreateContext(storage);
        var grid = RenderGrid(context);
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
        Assert.IsFalse(grid.Instance.GetColumnSettings().First().IsVisible);
        var other = RenderGrid(context, "other");
        Assert.AreEqual(0, other.Instance.ColumnProfiles.Count);
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(other));
    }

    [TestMethod]
    public async Task LegacyCustomStorageKeepsWorkingWithoutProfileCommands()
    {
        var storage = new LegacyStorage();
        using var context = CreateContext(storage);
        var grid = RenderGrid(context);
        Assert.IsFalse(grid.Instance.SupportsColumnProfiles);
        Assert.AreEqual(0, grid.FindAll(".super-datagrid-profiles").Count);
        await grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, storage.Settings!.Select(s => s.PropertyName).ToArray());
    }

    [TestMethod]
    public async Task SaveFailureDoesNotClaimSuccessOrOverwriteSelectedProfile()
    {
        var storage = new FailingStorage();
        using var context = CreateContext(storage);
        var grid = RenderGrid(context);
        await grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync("Work"));
        storage.Fail = true;
        await grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        Assert.IsTrue(grid.Instance.ColumnProfilesStorageFailed);
        Assert.AreEqual("A", grid.Instance.ColumnProfiles.Single().Columns.First().PropertyName);
        Assert.AreEqual(1, grid.FindAll("[role=alert]").Count);
        storage.Fail = false;
        await grid.InvokeAsync(() => grid.Instance.SetColumnVisibilityAsync(0, false));
        Assert.IsFalse(grid.Instance.ColumnProfilesStorageFailed);
        Assert.AreEqual("B", grid.Instance.ColumnProfiles.Single().Columns.First().PropertyName);
    }

    [TestMethod]
    public void NameInputEnablesSaveImmediatelyAndShowsValidationErrors()
    {
        using var context = CreateContext(new InMemorySettingsStorage());
        var dialog = context.Render<SuperDataGridColumnProfileDialog>(p => p
            .Add(d => d.Save, _ => throw new ArgumentException("Duplicate")));
        Assert.IsTrue(dialog.Find("button[type=submit]").HasAttribute("disabled"));
        dialog.Find("input").Input("Sales");
        Assert.IsFalse(dialog.Find("button[type=submit]").HasAttribute("disabled"));
        dialog.Find("form").Submit();
        dialog.WaitForAssertion(() => Assert.AreEqual(1, dialog.FindAll("[role=alert]").Count));
        Assert.AreEqual("Sales", dialog.Find("input").GetAttribute("value"));
    }

    [TestMethod]
    public async Task SwitchingWaitsForPendingSaveWithoutOverwritingTheOtherProfile()
    {
        var storage = new DelayedStorage();
        using var context = CreateContext(storage);
        var grid = RenderGrid(context);
        await grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync("Initial"));
        var initial = grid.Instance.ActiveColumnProfileId!;
        await grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync("Work"));
        var work = grid.Instance.ActiveColumnProfileId!;
        storage.DelayNextSave = true;
        var moving = grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        await storage.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var switching = grid.InvokeAsync(() => grid.Instance.SelectColumnProfileAsync(initial));
        storage.Release.SetResult();
        await Task.WhenAll(moving, switching);
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        Assert.AreEqual("A", grid.Instance.ColumnProfiles.Single(p => p.Id == initial).Columns.First().PropertyName);
        Assert.AreEqual("B", grid.Instance.ColumnProfiles.Single(p => p.Id == work).Columns.First().PropertyName);
        Assert.AreEqual(initial, grid.Instance.ActiveColumnProfileId);
    }

    [TestMethod]
    public async Task LocalStorageUsesSeparateKeysAndKeepsLegacyPayload()
    {
        using var context = new BunitContext();
        var storage = new SuperDataGridSettingsLocalStorage(context.JSInterop.JSRuntime);
        var columns = new[] { new SuperDataGridColumnSettings { PropertyName = "A", Width = "150px" } };
        var writes = context.JSInterop.SetupVoid("localStorage.setItem", _ => true);
        writes.SetVoidResult();
        await storage.SaveSettingsAsync("grid", columns);
        await storage.SaveProfilesAsync("grid", new SuperDataGridColumnProfiles
        {
            ActiveProfileId = "p",
            Profiles = [new() { Id = "p", Name = "Sales", Columns = columns }]
        });
        var invocations = context.JSInterop.Invocations["localStorage.setItem"];
        Assert.AreEqual("vdg_settings_grid", invocations[0].Arguments[0]);
        Assert.AreEqual("vdg_profiles_grid", invocations[1].Arguments[0]);
        Assert.AreEqual(JsonValueKind.Array, JsonDocument.Parse((string)invocations[0].Arguments[1]!).RootElement.ValueKind);
        var payload = (string)invocations[1].Arguments[1]!;
        context.JSInterop.Setup<string?>("localStorage.getItem", "vdg_profiles_grid").SetResult(payload);
        var restored = await storage.GetProfilesAsync("grid");
        Assert.AreEqual("Sales", restored!.Profiles.Single().Name);
        Assert.AreEqual("150px", restored.Profiles.Single().Columns.Single().Width);
    }

    [TestMethod]
    public async Task ImportCannotHideEveryColumnOrBypassDisabledFeatures()
    {
        using var context = CreateContext(new InMemorySettingsStorage());
        var grid = RenderGrid(context);
        grid.Render(p => p.Add(g => g.AllowColumnReorder, false).Add(g => g.AllowColumnResize, false));
        var document = new SuperDataGridColumnProfileDocument
        {
            Name = "External",
            Columns = new[] { "C", "B", "A" }.Select((name, index) => new SuperDataGridColumnSettings
            {
                PropertyName = name, Order = index, IsVisible = false, Width = "250px"
            }).ToArray()
        };
        await grid.InvokeAsync(() => grid.Instance.ImportColumnProfileAsync(document.ToJson(), "Imported"));
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        Assert.IsTrue(grid.Instance.GetColumnSettings().Any(c => c.IsVisible));
        Assert.IsTrue(grid.Instance.GetColumnSettings().All(c => c.Width == "100px"));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("<script>alert(1)</script>")]
    [DataRow("&lt;img src=x onerror=alert(1)&gt;")]
    [DataRow("javascript:alert(1)")]
    [DataRow("../profile")]
    public async Task NamesRejectMarkupAndActiveContent(string name)
    {
        using var context = CreateContext(new InMemorySettingsStorage());
        var grid = RenderGrid(context);
        await Assert.ThrowsAsync<ArgumentException>(() => grid.InvokeAsync(() => grid.Instance.SaveColumnProfileAsync(name)));
        Assert.AreEqual(0, grid.Instance.ColumnProfiles.Count);
    }

    [TestMethod]
    [DataRow("10px; background:url(https://example.com)")]
    [DataRow("expression(alert(1))")]
    [DataRow("-10px")]
    public void ImportRejectsUnsafeWidths(string width)
    {
        var json = JsonSerializer.Serialize(new { version = 1, name = "Bad", columns = new[] { new { propertyName = "A", order = 0, width } } });
        Assert.Throws<ArgumentException>(() => SuperDataGridColumnProfileDocument.FromJson(json));
    }

    private static string[] Order(IRenderedComponent<SuperDataGrid<string>> grid) => grid.Instance.ColumnsCollection.Select(c => c.Property).ToArray();

    private static BunitContext CreateContext(ISuperDataGridSettingsStorage storage)
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddLogging();
        context.Services.AddSuperComponents();
        context.Services.AddSingleton(storage);
        return context;
    }

    private static IRenderedComponent<SuperDataGrid<string>> RenderGrid(BunitContext context, string gridId = "profiles", bool constrained = false)
    {
        return context.Render<SuperDataGrid<string>>(p => p
            .Add(g => g.GridId, gridId)
            .Add(g => g.DisplayRowNumberColumn, false)
            .Add(g => g.DisplaySelectionColumn, false)
            .Add(g => g.ItemsProvider, _ => ValueTask.FromResult(GridItemsProviderResult<string>.Empty()))
            .AddChildContent(builder =>
            {
                foreach (var name in new[] { "A", "B", "C" })
                {
                    builder.OpenComponent<DataGridColumn<string>>(0);
                    builder.SetKey(name);
                    builder.AddAttribute(1, "Property", name);
                    builder.AddAttribute(2, "Title", name);
                    builder.AddAttribute(3, "Width", "100px");
                    builder.AddAttribute(4, "AlwaysVisible", constrained && name == "B");
                    builder.AddAttribute(5, "Reorderable", !constrained || name != "B");
                    builder.AddAttribute(6, "Resizable", !constrained || name != "B");
                    builder.CloseComponent();
                }
            }));
    }

    private class LegacyStorage : ISuperDataGridSettingsStorage
    {
        public List<SuperDataGridColumnSettings>? Settings { get; private set; }
        public Task<IEnumerable<SuperDataGridColumnSettings>?> GetSettingsAsync(string gridId, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<SuperDataGridColumnSettings>?>(Settings);
        public Task SaveSettingsAsync(string gridId, IEnumerable<SuperDataGridColumnSettings> settings, CancellationToken cancellationToken = default)
        {
            Settings = settings.ToList();
            return Task.CompletedTask;
        }
        public Task ClearSettingsAsync(string gridId, CancellationToken cancellationToken = default)
        {
            Settings = null;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingStorage : LegacyStorage, ISuperDataGridColumnProfilesStorage
    {
        public bool Fail { get; set; }
        private SuperDataGridColumnProfiles? _profiles;
        public Task<SuperDataGridColumnProfiles?> GetProfilesAsync(string gridId, CancellationToken cancellationToken = default) => Task.FromResult(_profiles);
        public Task SaveProfilesAsync(string gridId, SuperDataGridColumnProfiles profiles, CancellationToken cancellationToken = default)
        {
            if (Fail)
            {
                throw new JSException("Storage unavailable");
            }
            _profiles = profiles;
            return Task.CompletedTask;
        }
    }

    private sealed class DelayedStorage : LegacyStorage, ISuperDataGridColumnProfilesStorage
    {
        public bool DelayNextSave { get; set; }
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private SuperDataGridColumnProfiles? _profiles;
        public Task<SuperDataGridColumnProfiles?> GetProfilesAsync(string gridId, CancellationToken cancellationToken = default) => Task.FromResult(_profiles);
        public async Task SaveProfilesAsync(string gridId, SuperDataGridColumnProfiles profiles, CancellationToken cancellationToken = default)
        {
            if (DelayNextSave)
            {
                DelayNextSave = false;
                Started.SetResult();
                await Release.Task;
            }
            _profiles = profiles;
        }
    }
}
