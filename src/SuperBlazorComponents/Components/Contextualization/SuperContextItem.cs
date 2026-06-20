using Microsoft.AspNetCore.Components;

namespace SuperBlazorComponents.Components.Contextualization;

public sealed class SuperContextItem
{
	public required SuperContextDescriptor Descriptor { get; init; }
	public required object Context { get; init; }
	public object? Parent { get; init; }
	public object? Tag { get; set; }
	public bool Visible { get; set; }
	public EventCallback<object> ReloadHost { get; set; }
	public object? AdapterState { get; set; }

	public Type ContextType => Descriptor.ContextType;
	public Type ComponentType => Descriptor.ComponentType;
	public string Title => Descriptor.Title;
	public string Zone => Descriptor.Zone;
	public string? Icon => Descriptor.Icon;
	public string? IconColor => Descriptor.IconColor;
	public int Order => Descriptor.Order;
	public string? HelpFile => Descriptor.HelpFile;
	public string? PolicyName => Descriptor.PolicyName;
}
