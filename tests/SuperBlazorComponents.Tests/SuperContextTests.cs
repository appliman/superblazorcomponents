using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using SuperBlazorComponents;
using SuperBlazorComponents.Components.Contextualization;
using SuperBlazorComponents.Configuration;

namespace SuperBlazorComponents.Tests;

[TestClass]
public sealed class SuperContextTests : BunitContext
{
	[TestMethod]
	public void DiscoversDescriptorsFromConfiguredAssemblies()
	{
		var configuration = new SuperContextConfiguration().AddAssembly<SuperContextTests>();
		var service = new SuperContextService(configuration);

		var descriptor = service.GetDescriptors(typeof(CustomerContext), SuperContextZones.Bottom).Single();

		Assert.AreEqual("Details", descriptor.Title);
		Assert.AreEqual(typeof(ContextDetailsComponent), descriptor.ComponentType);
		Assert.AreEqual("details.md", descriptor.HelpFile);
	}

	[TestMethod]
	public void UsesAssignableContextTypes()
	{
		var configuration = new SuperContextConfiguration().AddAssembly<SuperContextTests>();
		var service = new SuperContextService(configuration);

		var descriptors = service.GetDescriptors(typeof(SpecialCustomerContext), SuperContextZones.Bottom);

		Assert.AreEqual(1, descriptors.Count());
	}

	[TestMethod]
	public void FiltersDescriptorsByZone()
	{
		var configuration = new SuperContextConfiguration().AddAssembly<SuperContextTests>();
		var service = new SuperContextService(configuration);

		var descriptors = service.GetDescriptors(typeof(CustomerContext), SuperContextZones.Right);

		Assert.AreEqual("Right details", descriptors.Single().Title);
	}
	[TestMethod]
	public void HostsKeepIndependentRuntimeState()
	{
		Services.AddSuperComponents(options => options.Contextualization.AddAssembly<SuperContextTests>());
		JSInterop.Mode = JSRuntimeMode.Loose;

		var first = Render<SuperContextHost>(parameters => parameters
			.Add(component => component.InstanceId, "first-host")
			.Add(component => component.Zone, SuperContextZones.Bottom));
		var second = Render<SuperContextHost>(parameters => parameters
			.Add(component => component.InstanceId, "second-host")
			.Add(component => component.Zone, SuperContextZones.Bottom));

		first.InvokeAsync(() => first.Instance.ContextChanged(null, new CustomerContext("First")));
		second.InvokeAsync(() => second.Instance.ContextChanged(null, new CustomerContext("Second")));

		first.WaitForAssertion(() => StringAssert.Contains(first.Markup, "First"));
		second.WaitForAssertion(() => StringAssert.Contains(second.Markup, "Second"));

		var service = Services.GetRequiredService<SuperContextService>();
		service.ChangeItem("Details", false, hostId: "first-host");

		first.WaitForAssertion(() => Assert.IsFalse(first.Markup.Contains("First", StringComparison.Ordinal)));
		StringAssert.Contains(second.Markup, "Second");
	}

	public record CustomerContext(string Name);
	public sealed record SpecialCustomerContext(string Name) : CustomerContext(Name);

	[TestContext<CustomerContext>(
		Title = "Details",
		Zone = SuperContextZones.Bottom,
		HelpFile = "details.md")]
	[TestContext<CustomerContext>(Title = "Right details", Zone = SuperContextZones.Right)]
	public sealed class ContextDetailsComponent : ComponentBase
	{
		[Parameter]
		public SuperContextItem ContextInfo { get; set; } = default!;

		protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
		{
			builder.AddContent(0, ((CustomerContext)ContextInfo.Context).Name);
		}
	}

	private sealed class TestContextAttribute<T> : SuperContextAttribute<T>
	{
	}
}
