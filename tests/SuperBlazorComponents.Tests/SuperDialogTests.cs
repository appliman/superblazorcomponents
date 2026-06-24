using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperBlazorComponents.Components.Dialogs;
using SuperBlazorComponents.Services;

namespace SuperBlazorComponents.Tests;

[TestClass]
public sealed class SuperDialogTests : BunitContext
{
    [TestInitialize]
    public void Setup()
    {
        Services.AddLocalization();
        Services.AddScoped<SuperDialogService>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    [TestMethod]
    public async Task CloseButton_CompletesOpenAsync()
    {
        var cut = Render<SuperDialog>();
        var dialogService = Services.GetRequiredService<SuperDialogService>();

        var openTask = cut.InvokeAsync(() => dialogService.OpenAsync<TestDialog>("Title"));

        cut.WaitForAssertion(() => Assert.AreEqual("Title", cut.Find(".modal-title").TextContent));

        cut.Find(".btn-close").Click();

        var result = await openTask.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.IsNull(result);
        Assert.AreEqual(0, cut.FindAll(".modal").Count);
    }

    [TestMethod]
    public async Task BackdropClick_Default_KeepsDialogOpen()
    {
        var cut = Render<SuperDialog>();
        var dialogService = Services.GetRequiredService<SuperDialogService>();

        var openTask = cut.InvokeAsync(() => dialogService.OpenAsync<TestDialog>("Title"));

        cut.WaitForAssertion(() => Assert.AreEqual("Title", cut.Find(".modal-title").TextContent));

        cut.Find(".modal").Click();

        Assert.IsFalse(openTask.IsCompleted);
        Assert.AreEqual("Title", cut.Find(".modal-title").TextContent);

        await cut.InvokeAsync(() => dialogService.Close(null));
        await openTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public async Task BackdropClick_WhenEnabled_CompletesOpenAsync()
    {
        var cut = Render<SuperDialog>();
        var dialogService = Services.GetRequiredService<SuperDialogService>();

        var openTask = cut.InvokeAsync(() => dialogService.OpenAsync<TestDialog>(
            "Title",
            options: new DialogOptions { CloseOnBackdropClick = true }));

        cut.WaitForAssertion(() => Assert.AreEqual("Title", cut.Find(".modal-title").TextContent));

        cut.Find(".modal").Click();

        var result = await openTask.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.IsNull(result);
        Assert.AreEqual(0, cut.FindAll(".modal").Count);
    }

    [TestMethod]
    public async Task OpenAsync_WhenDialogIsAlreadyOpen_ClosesPreviousDialog()
    {
        var cut = Render<SuperDialog>();
        var dialogService = Services.GetRequiredService<SuperDialogService>();

        var firstOpenTask = cut.InvokeAsync(() => dialogService.OpenAsync<TestDialog>("First"));

        cut.WaitForAssertion(() => Assert.AreEqual("First", cut.Find(".modal-title").TextContent));

        var secondOpenTask = cut.InvokeAsync(() => dialogService.OpenAsync<TestDialog>("Second"));

        var firstResult = await firstOpenTask.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.IsNull(firstResult);
        cut.WaitForAssertion(() => Assert.AreEqual("Second", cut.Find(".modal-title").TextContent));

        await cut.InvokeAsync(() => dialogService.Close("done"));

        var secondResult = await secondOpenTask.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.AreEqual("done", secondResult);
    }

    private sealed class TestDialog : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, "Test dialog");
        }
    }
}
