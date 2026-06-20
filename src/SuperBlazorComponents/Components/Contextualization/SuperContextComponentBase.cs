using Microsoft.AspNetCore.Components;

namespace SuperBlazorComponents.Components.Contextualization;

public abstract class SuperContextComponentBase : ComponentBase
{
	[Parameter]
	public SuperContextItem ContextInfo { get; set; } = default!;
}
