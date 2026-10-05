using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperBlazorComponents.Components.Dialogs;
using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.Components.SuperDataGrid.Tools;
using SuperBlazorComponents.Services;

namespace SuperBlazorComponents.Tests;

[TestClass]
public sealed class SuperDataGridRangeSelectionTests
{
    [TestMethod]
    public async Task InclusiveRange_LoadsOnlyRequestedBatchesAndNotifiesOnce()
    {
        using var context = CreateContext();
        var requests = new List<GridItemsProviderRequest<Row>>();
        var cut = RenderGrid(context, request =>
        {
            requests.Add(request);
            return Page(request, 600);
        });
        SetField(cut.Instance, "_sortColumn", nameof(Row.Id));
        SetField(cut.Instance, "_sortDirection", SortDirection.Descending);
        SetField(cut.Instance, "_filterInfoList", new List<SuperDataGridFilterInfo>
        {
            new() { PropertyName = "Name", PropertyValue = "test", SelectedValues = new[] { "A", "B" } }
        });
        var notifications = 0;
        cut.Instance.SelectedRowsChanged += (_, args) =>
        {
            notifications++;
            Assert.AreEqual(100, args.SelectionInfo.FromRow);
            Assert.AreEqual(550, args.SelectionInfo.ToRow);
        };
        requests.Clear();

        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(100, 550));

        CollectionAssert.AreEqual(new[] { 99, 299, 499 }, requests.Select(r => r.StartIndex).ToArray());
        CollectionAssert.AreEqual(new int?[] { 200, 200, 51 }, requests.Select(r => r.Count).ToArray());
        Assert.IsTrue(requests.All(r => r.SortColumn == "Id" && r.SortDirection == SortDirection.Descending));
        Assert.IsTrue(requests.All(r => r.Filters.Single().PropertyValue == "test"));
        Assert.IsTrue(requests.All(r => r.ParentKey is null && r.HierarchyLevel == 0));
        Assert.AreEqual(451, cut.Instance.SelectedCountTotal);
        Assert.AreEqual(1, notifications);
        var snapshot = cut.Instance.CaptureSelectionSnapshot();
        CollectionAssert.AreEqual(Enumerable.Range(100, 451).ToArray(), snapshot.SelectedItems.Select(r => r.Id).ToArray());
        Assert.IsFalse(snapshot.AllSelected);
    }

    [TestMethod]
    public async Task FirstRows_ReplaceIndividualAndAllSelection()
    {
        using var context = CreateContext();
        var cut = RenderGrid(context);
        var original = new Row { Id = 9 };
        await cut.InvokeAsync(() => cut.Instance.SelectRow(original));
        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 3));
        Assert.AreEqual(1, cut.Instance.GetSelectionInfo().FromRow);
        Assert.AreEqual(3, cut.Instance.GetSelectionInfo().ToRow);
        Assert.IsFalse(original.IsSelected);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, cut.Instance.SelectedItems.Select(r => r.Id).ToArray());
        await cut.InvokeAsync(() => cut.Instance.SelectAllAsync());
        Assert.IsNull(cut.Instance.GetSelectionInfo().FromRow);
        Assert.IsNull(cut.Instance.GetSelectionInfo().ToRow);
        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(5, 5));
        Assert.AreEqual(1, cut.Instance.SelectedCountTotal);
        Assert.IsFalse(cut.Instance.GetSelectionInfo().AllSelected);
        Assert.AreEqual(5, cut.Instance.SelectedItems.Single().Id);
        Assert.AreEqual(5, cut.Instance.GetSelectionInfo().FromRow);
        Assert.AreEqual(5, cut.Instance.GetSelectionInfo().ToRow);
    }

    [TestMethod]
    public async Task ReloadedInstances_StayCheckedAndCanBeUncheckedWithoutDuplicates()
    {
        using var context = CreateContext();
        var cut = RenderGrid(context);
        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(3, 6));
        var stored = cut.Instance.SelectedItems.Single(r => r.Id == 4);
        // Exercise the same loading path used by Virtualize, with fresh instances.
        await cut.InvokeAsync(async () =>
        {
            var request = new ItemsProviderRequest(2, 6, CancellationToken.None);
            var result = (ValueTask<ItemsProviderResult<Row>>)Method("LoadItemsAsync").Invoke(cut.Instance, new object[] { request })!;
            await result;
        });
        var rendered = (List<Row>)Field("_renderedItems").GetValue(cut.Instance)!;
        Assert.IsTrue(rendered.Where(r => r.Id >= 3 && r.Id <= 6).All(r => r.IsSelected));
        Assert.IsFalse(rendered.Single(r => r.Id == 7).IsSelected);
        Assert.AreEqual(4, cut.Instance.CaptureSelectionSnapshot().SelectedItems.Length);
        var replacement = rendered.Single(r => r.Id == 4);
        await cut.InvokeAsync(() => cut.Instance.SelectRow(replacement, clearOthers: false));
        Assert.AreEqual(4, cut.Instance.SelectedCountTotal);
        Assert.AreEqual(3, cut.Instance.GetSelectionInfo().FromRow);
        Assert.AreEqual(6, cut.Instance.GetSelectionInfo().ToRow);
        await cut.InvokeAsync(() => cut.Instance.DeselectRowAsync(replacement));
        Assert.IsNull(cut.Instance.GetSelectionInfo().FromRow);
        Assert.IsNull(cut.Instance.GetSelectionInfo().ToRow);
        Assert.IsFalse(replacement.IsSelected);
        Assert.IsFalse(stored.IsSelected);
        Assert.AreEqual(3, cut.Instance.SelectedCountTotal);
        Assert.IsFalse(cut.Instance.CaptureSelectionSnapshot().SelectedItemKeys.Contains(4));
        await cut.InvokeAsync(() => cut.Instance.ClearSelectionAsync());
        Assert.IsTrue(rendered.All(r => !r.IsSelected));
    }

    [TestMethod]
    public async Task FailureOrCancellation_DoesNotPartiallyReplaceSelection()
    {
        using var context = CreateContext();
        var fail = false;
        using var cancellation = new CancellationTokenSource();
        var cut = RenderGrid(context, request =>
        {
            if (request.StartIndex == 200)
            {
                if (fail)
                {
                    throw new InvalidOperationException("Provider failure");
                }
                cancellation.Cancel();
            }
            return Page(request, 600);
        });
        var original = new Row { Id = 600 };
        await cut.InvokeAsync(() => cut.Instance.SelectRow(original));
        fail = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 450)));
        Assert.AreSame(original, cut.Instance.SelectedItems.Single());
        Assert.IsTrue(original.IsSelected);
        fail = false;
        await Assert.ThrowsAsync<OperationCanceledException>(() => cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 450, cancellation.Token)));
        Assert.AreSame(original, cut.Instance.SelectedItems.Single());
        Assert.IsTrue(original.IsSelected);
    }

    [TestMethod]
    public async Task QueryChangeDuringLoading_PreservesSelection()
    {
        using var context = CreateContext();
        var pending = new TaskCompletionSource<GridItemsProviderResult<Row>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var defer = false;
        var cut = RenderGrid(context, request => defer ? new(pending.Task) : Page(request, 10));
        await cut.InvokeAsync(() => cut.Instance.SelectRow(new Row { Id = 9 }));
        defer = true;
        var selection = cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 3));
        await cut.InvokeAsync(() => SetField(cut.Instance, "_sortColumn", nameof(Row.Id)));
        pending.SetResult(GridItemsProviderResult<Row>.From(Enumerable.Range(1, 3).Select(id => new Row { Id = id }), 10));
        await Assert.ThrowsAsync<InvalidOperationException>(() => selection);
        Assert.AreEqual(9, cut.Instance.SelectedItems.Single().Id);
    }

    [TestMethod]
    public async Task InvalidBoundsAndDeletedRows_AreHandled()
    {
        using var context = CreateContext();
        var cut = RenderGrid(context);
        foreach (var (from, to) in new[] { (0, 3), (3, 2), (1, 11), (-1, 2) })
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(from, to)));
        }
        cut.Render(p => p.Add(g => g.DisplayRowDeleted, row => row.Id == 2));
        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 3));
        CollectionAssert.AreEqual(new[] { 1, 3 }, cut.Instance.SelectedItems.Select(r => r.Id).ToArray());
    }

    [TestMethod]
    public async Task Dialog_ValidatesBothModesAndAppliesInclusiveRange()
    {
        using var context = CreateContext();
        var cut = RenderGrid(context);
        var dialog = context.Render<SuperDataGridRangeSelectionDialog<Row>>(p => p.Add(d => d.SuperDataGrid, cut.Instance));
        Assert.IsTrue(dialog.Find("button.btn-primary").HasAttribute("disabled"));
        foreach (var invalid in new[] { "", "0", "-1", "11", "1.5", "2147483648" })
        {
            dialog.Find("input").Input(invalid);
            Assert.IsTrue(dialog.Find("button.btn-primary").HasAttribute("disabled"), invalid);
        }
        dialog.Find("input").Input("3");
        Assert.IsFalse(dialog.Find("button.btn-primary").HasAttribute("disabled"));
        dialog.Find("select").Change("false");
        Assert.IsTrue(dialog.Find("button.btn-primary").HasAttribute("disabled"));
        dialog.FindAll("input")[0].Input("4");
        Assert.IsTrue(dialog.Find("button.btn-primary").HasAttribute("disabled"));
        dialog.FindAll("input")[1].Input("7");
        await dialog.Find("button.btn-primary").ClickAsync(new());
        CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, cut.Instance.SelectedItems.Select(r => r.Id).ToArray());
    }

    [TestMethod]
    public async Task Dialog_ShowsFailureAndAllowsRetry()
    {
        using var context = CreateContext();
        var fail = false;
        var cut = RenderGrid(context, request => fail ? throw new InvalidOperationException("offline") : Page(request, 10));
        await cut.InvokeAsync(() => cut.Instance.SelectRow(new Row { Id = 8 }));
        var dialog = context.Render<SuperDataGridRangeSelectionDialog<Row>>(p => p.Add(d => d.SuperDataGrid, cut.Instance));
        dialog.Find("input").Input("3");
        fail = true;
        await dialog.Find("button.btn-primary").ClickAsync(new());
        Assert.AreEqual(1, dialog.FindAll("[role=alert]").Count);
        Assert.AreEqual(8, cut.Instance.SelectedItems.Single().Id);
        Assert.IsFalse(dialog.Find("button.btn-primary").HasAttribute("disabled"));
        fail = false;
        await dialog.Find("button.btn-primary").ClickAsync(new());
        Assert.AreEqual(3, cut.Instance.SelectedCountTotal);
        Assert.AreEqual(0, dialog.FindAll("[role=alert]").Count);
    }

    [TestMethod]
    public async Task Selector_OpensDialogAndCancellationLeavesSelectionUntouched()
    {
        using var context = CreateContext();
        var cut = RenderGrid(context);
        await cut.InvokeAsync(() => cut.Instance.SelectRow(new Row { Id = 8 }));
        var host = context.Render<SuperDialog>();
        var selector = context.Render<SuperDataGridRowSelector<Row>>(p => p.Add(s => s.SuperDataGrid, cut.Instance));
        var click = selector.FindComponents<SuperBlazorComponents.Components.Buttons.SuperSplitButtonItem>()
            .Single(i => i.Instance.ActionName == "selectRange").Find("button").ClickAsync(new());
        host.WaitForAssertion(() => Assert.AreEqual(1, host.FindAll("input[type=number]").Count));
        Assert.AreEqual("true", host.Find("select").GetAttribute("value") ?? host.Find("option[selected]").GetAttribute("value"));
        Assert.IsTrue(host.Find("button.btn-primary").HasAttribute("disabled"));
        await host.Find("button.btn-outline-secondary").ClickAsync(new());
        await click.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.AreEqual(8, cut.Instance.SelectedItems.Single().Id);
    }

    [TestMethod]
    public void Selector_DisablesEmptyListAndHidesRangeInSingleMode()
    {
        using var context = CreateContext();
        var cut = RenderGrid(context, r => Page(r, 0));
        var selector = context.Render<SuperDataGridRowSelector<Row>>(p => p.Add(s => s.SuperDataGrid, cut.Instance));
        Assert.IsTrue(selector.FindComponents<SuperBlazorComponents.Components.Buttons.SuperSplitButtonItem>()
            .Single(i => i.Instance.ActionName == "selectRange").Find("button").HasAttribute("disabled"));
        cut.Render(p => p.Add(g => g.SelectionMode, SuperDataGridSelectionMode.Single));
        selector.Render();
        Assert.IsFalse(selector.FindComponents<SuperBlazorComponents.Components.Buttons.SuperSplitButtonItem>()
            .Any(i => i.Instance.ActionName == "selectRange"));
    }

    [TestMethod]
    public async Task Dialog_LoadingDisablesInputsAndCancelAbortsWithoutReplacingSelection()
    {
        using var context = CreateContext();
        var pending = new TaskCompletionSource<GridItemsProviderResult<Row>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var defer = false;
        var requests = 0;
        var cut = RenderGrid(context, request =>
        {
            requests++;
            return defer ? new(pending.Task) : Page(request, 10);
        });
        await cut.InvokeAsync(() => cut.Instance.SelectRow(new Row { Id = 8 }));
        var host = context.Render<SuperDialog>();
        var service = context.Services.GetRequiredService<SuperDialogService>();
        var opening = host.InvokeAsync(() => service.OpenAsync<SuperDataGridRangeSelectionDialog<Row>>("Range",
            new() { [nameof(SuperDataGridRangeSelectionDialog<Row>.SuperDataGrid)] = cut.Instance }));
        host.WaitForAssertion(() => Assert.AreEqual(1, host.FindAll("input").Count));
        host.Find("input").Input("3");
        defer = true;
        requests = 0;
        var applying = host.Find("button.btn-primary").ClickAsync(new());
        host.WaitForAssertion(() => Assert.IsTrue(host.Find("fieldset").HasAttribute("disabled")));
        Assert.IsTrue(host.Find("button.btn-primary").HasAttribute("disabled"));
        Assert.AreEqual(1, host.FindAll("[role=status]").Count);
        // A second callback cannot start another provider operation.
        await host.Find("button.btn-primary").ClickAsync(new());
        Assert.AreEqual(1, requests);
        await host.Find("button.btn-outline-secondary").ClickAsync(new());
        await opening.WaitAsync(TimeSpan.FromSeconds(3));
        pending.SetResult(GridItemsProviderResult<Row>.From(Enumerable.Range(1, 3).Select(id => new Row { Id = id }), 10));
        await applying.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.AreEqual(8, cut.Instance.SelectedItems.Single().Id);
    }

    [TestMethod]
    public async Task ShortPagesAreContinued_AndInconsistentResultsPreserveSelection()
    {
        using var context = CreateContext();
        var inconsistent = false;
        var cut = RenderGrid(context, request =>
        {
            if (request.Count is null)
            {
                return Page(request, 10);
            }
            if (inconsistent)
            {
                return ValueTask.FromResult(GridItemsProviderResult<Row>.Empty());
            }
            return Page(request with { Count = Math.Min(2, request.Count.Value) }, 10);
        });
        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(3, 8));
        CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, 8 }, cut.Instance.SelectedItems.Select(r => r.Id).ToArray());
        inconsistent = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 3)));
        Assert.AreEqual(6, cut.Instance.SelectedCountTotal);
    }

    [TestMethod]
    public async Task RangeBounds_ResetOnClearAndManualAddition_AndSurviveFailedOperations()
    {
        using var context = CreateContext();
        var fail = false;
        var cut = RenderGrid(context, request => fail ? throw new InvalidOperationException("offline") : Page(request, 10));
        Assert.IsNull(cut.Instance.GetSelectionInfo().FromRow);
        Assert.IsNull(cut.Instance.GetSelectionInfo().ToRow);
        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(3, 6));
        fail = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 2)));
        Assert.AreEqual(3, cut.Instance.GetSelectionInfo().FromRow);
        Assert.AreEqual(6, cut.Instance.GetSelectionInfo().ToRow);
        fail = false;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 2, cancellation.Token)));
        Assert.AreEqual(3, cut.Instance.GetSelectionInfo().FromRow);
        Assert.AreEqual(6, cut.Instance.GetSelectionInfo().ToRow);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(0, 2)));
        Assert.AreEqual(3, cut.Instance.GetSelectionInfo().FromRow);
        Assert.AreEqual(6, cut.Instance.GetSelectionInfo().ToRow);
        await cut.InvokeAsync(() => cut.Instance.SelectRow(new Row { Id = 8 }, clearOthers: false));
        Assert.IsNull(cut.Instance.GetSelectionInfo().FromRow);
        Assert.IsNull(cut.Instance.GetSelectionInfo().ToRow);
        await cut.InvokeAsync(() => cut.Instance.SelectRangeAsync(1, 2));
        await cut.InvokeAsync(() => cut.Instance.ClearSelectionAsync());
        Assert.IsNull(cut.Instance.GetSelectionInfo().FromRow);
        Assert.IsNull(cut.Instance.GetSelectionInfo().ToRow);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddLogging();
        context.Services.AddSuperComponents();
        return context;
    }

    private static IRenderedComponent<SuperDataGrid<Row>> RenderGrid(BunitContext context, GridItemsProvider<Row>? provider = null)
    {
        return context.Render<SuperDataGrid<Row>>(p => p
            .Add(g => g.Hierarchical, true)
            .Add(g => g.ItemsProvider, provider ?? (request => Page(request, 10)))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<DataGridColumn<Row>>(0);
                builder.AddAttribute(1, "Property", nameof(Row.Id));
                builder.AddAttribute(2, "Title", "Id");
                builder.CloseComponent();
            }));
    }

    private static ValueTask<GridItemsProviderResult<Row>> Page(GridItemsProviderRequest<Row> request, int total)
    {
        return ValueTask.FromResult(GridItemsProviderResult<Row>.From(
            Enumerable.Range(1, total).Skip(request.StartIndex).Take(request.Count ?? total).Select(id => new Row { Id = id }).ToArray(), total));
    }

    private static FieldInfo Field(string name) => typeof(SuperDataGrid<Row>).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static MethodInfo Method(string name) => typeof(SuperDataGrid<Row>).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static void SetField(SuperDataGrid<Row> grid, string name, object value) => Field(name).SetValue(grid, value);

    public sealed class Row : IDataItem
    {
        public int Id { get; set; }
        public object KeyValue => Id;
        public bool IsSelected { get; set; }
        public int RowNumber { get; set; }
    }
}
