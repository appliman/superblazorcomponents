using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperBlazorComponents.Components.SuperMarkdownEditor;
using SuperBlazorComponents.Services;

namespace SuperBlazorComponents.Tests;

[TestClass]
public sealed class SuperMarkdownEditorTests : BunitContext
{
    [TestInitialize]
    public void Setup()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<SuperDialogService>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    [TestMethod]
    public void Editor_RendersInitialMarkdownAsPreview()
    {
        var cut = Render<SuperMarkdownEditor>(parameters => parameters
            .Add(p => p.Label, "Notes")
            .Add(p => p.Value, "# Title\n\nA **bold** and *italic* sentence."));

        Assert.AreEqual("Notes", cut.Find("label").TextContent);
        Assert.AreEqual("Title", cut.Find(".sme-preview h1").TextContent);
        Assert.AreEqual("bold", cut.Find(".sme-preview strong").TextContent);
        Assert.AreEqual("italic", cut.Find(".sme-preview em").TextContent);
        Assert.IsTrue(cut.Find(".sme-preview").ClassList.Contains("sme-preview"));
        Assert.IsTrue(cut.Find("textarea").ClassList.Contains("sme-hidden"));
    }

    [TestMethod]
    public void Editor_WhenRenderedViewIsFalse_ShowsRawMarkdownTextarea()
    {
        var cut = Render<SuperMarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "## Raw")
            .Add(p => p.RenderedView, false));

        Assert.IsTrue(cut.Find(".sme-preview").ClassList.Contains("sme-hidden"));
        Assert.IsFalse(cut.Find("textarea").ClassList.Contains("sme-hidden"));
        Assert.IsFalse(cut.Find("textarea").HasAttribute("disabled"));
        Assert.AreEqual("Rendered", cut.Find(".sme-toggle-pill").TextContent);
    }

    [TestMethod]
    public void Toolbar_DoesNotRenderMarkdownActions()
    {
        var cut = Render<SuperMarkdownEditorToolbar>();

        Assert.AreEqual(2, cut.FindAll("button").Count);
        Assert.AreEqual(0, cut.FindAll("button[title='Bold']").Count);
        Assert.AreEqual(0, cut.FindAll("button[title='Italic']").Count);
        Assert.AreEqual(1, cut.FindAll("button[title='Markdown help']").Count);
        Assert.AreEqual(1, cut.FindAll("button[title='Toggle Markdown / rendered']").Count);
    }

    [TestMethod]
    public void Toolbar_ClickingMarkdownToggle_RaisesRenderedViewChanged()
    {
        bool? renderedView = null;

        var cut = Render<SuperMarkdownEditorToolbar>(parameters => parameters
            .Add(p => p.RenderedView, true)
            .Add(p => p.RenderedViewChanged, EventCallback.Factory.Create<bool>(
                this,
                value => renderedView = value)));

        cut.Find("button[title='Toggle Markdown / rendered']").Click();

        Assert.AreEqual(false, renderedView);
    }

    [TestMethod]
    public async Task Editor_OnContentChanged_UpdatesValueChanged()
    {
        string? changedValue = null;

        var cut = Render<SuperMarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "Initial")
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(
                this,
                value => changedValue = value)));

        await cut.InvokeAsync(() => cut.Instance.OnContentChanged("Updated **markdown**"));

        Assert.AreEqual("Updated **markdown**", changedValue);
    }
}
