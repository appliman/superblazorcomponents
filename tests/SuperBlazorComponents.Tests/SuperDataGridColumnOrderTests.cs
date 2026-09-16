using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace SuperBlazorComponents.Tests;

#pragma warning disable BL0005 // Tests deliberately change the column parameter to verify guards.

[TestClass]
public sealed class SuperDataGridColumnOrderTests
{
    [TestMethod]
    public void MenuMovesColumnsAndResetRestoresDeclaredOrder()
    {
        using var context = CreateContext();
        var storage = new MemoryStorage();
        var grid = RenderGrid(context, storage);
        Assert.AreEqual(0, grid.FindAll("[draggable], .sdg-drag-handle").Count);
        Assert.IsTrue(grid.FindAll(".super-datagrid-column-move button")[0].HasAttribute("disabled"));
        Assert.IsTrue(grid.FindAll(".super-datagrid-column-move button")[5].HasAttribute("disabled"));

        grid.FindAll(".super-datagrid-column-move button")[1].Click();
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, Order(grid));
        CollectionAssert.AreEqual(new[] { "B", "A", "C" }, storage.Saved!.Select(c => c.PropertyName).ToArray());
        Assert.AreEqual("B", grid.Find(".super-datagrid-column-item span").TextContent);
        grid.FindAll(".super-datagrid-column-move button")[2].Click();
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        grid.FindAll(".super-datagrid-column-move button")[1].Click();
        grid.Find(".super-datagrid-columns-menu > li > button").Click();
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        Assert.IsTrue(storage.Cleared);
    }

    [TestMethod]
    public async Task ReorderRespectsDisabledSettingAndFixedColumns()
    {
        using var context = CreateContext();
        var grid = RenderGrid(context, new MemoryStorage(), allowReorder: false);
        Assert.AreEqual(0, grid.FindAll(".super-datagrid-column-move").Count);
        await grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
        grid.Render(p => p.Add(g => g.AllowColumnReorder, true));
        await grid.InvokeAsync(() => grid.Instance.ColumnsCollection[1].Reorderable = false);
        Assert.IsFalse(grid.Instance.CanMoveColumn(0, 1));
        Assert.IsFalse(grid.Instance.CanMoveColumn(1, -1));
        Assert.IsFalse(grid.Instance.CanMoveColumn(2, -1));
        Assert.IsFalse(grid.Instance.CanMoveColumn(0, 2));
        await grid.InvokeAsync(() => grid.Instance.MoveColumnAsync(0, 1));
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
    }

    [TestMethod]
    public void ResetRestoresDeclaredOrderAfterLoadingSavedPreferences()
    {
        using var context = CreateContext();
        var storage = new MemoryStorage
        {
            Saved = new[] { "C", "B", "A" }.Select((name, index) =>
                new SuperDataGridColumnSettings { PropertyName = name, Order = index, IsVisible = true }).ToList()
        };
        var grid = RenderGrid(context, storage);
        CollectionAssert.AreEqual(new[] { "C", "B", "A" }, Order(grid));
        grid.Find(".super-datagrid-columns-menu > li > button").Click();
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, Order(grid));
    }

    private static string[] Order(IRenderedComponent<SuperDataGrid<string>> grid)
        => grid.Instance.ColumnsCollection.Select(c => c.Property).ToArray();

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddLogging();
        context.Services.AddSuperComponents();
        return context;
    }

    private static IRenderedComponent<SuperDataGrid<string>> RenderGrid(
        BunitContext context, MemoryStorage storage, bool allowReorder = true)
    {
        context.Services.AddSingleton<ISuperDataGridSettingsStorage>(storage);
        return context.Render<SuperDataGrid<string>>(p => p
            .Add(g => g.GridId, "column-order")
            .Add(g => g.ItemsProvider, _ => ValueTask.FromResult(GridItemsProviderResult<string>.Empty()))
            .Add(g => g.AllowColumnReorder, allowReorder)
            .AddChildContent(builder =>
            {
                foreach (var name in new[] { "A", "B", "C" })
                {
                    builder.OpenComponent<DataGridColumn<string>>(0);
                    builder.SetKey(name);
                    builder.AddAttribute(1, "Property", name);
                    builder.AddAttribute(2, "Title", name);
                    builder.CloseComponent();
                }
            }));
    }

    private sealed class MemoryStorage : ISuperDataGridSettingsStorage
    {
        public List<SuperDataGridColumnSettings>? Saved { get; set; }
        public bool Cleared { get; private set; }

        public Task<IEnumerable<SuperDataGridColumnSettings>?> GetSettingsAsync(string gridId, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<SuperDataGridColumnSettings>?>(Saved);

        public Task SaveSettingsAsync(string gridId, IEnumerable<SuperDataGridColumnSettings> settings, CancellationToken cancellationToken = default)
        {
            Saved = settings.ToList();
            return Task.CompletedTask;
        }

        public Task ClearSettingsAsync(string gridId, CancellationToken cancellationToken = default)
        {
            Saved = null;
            Cleared = true;
            return Task.CompletedTask;
        }
    }
}
