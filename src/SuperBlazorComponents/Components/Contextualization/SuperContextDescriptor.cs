namespace SuperBlazorComponents.Components.Contextualization;

public sealed record SuperContextDescriptor(
	Type ContextType,
	Type ComponentType,
	string Title,
	string Zone,
	string? ExcludeRoutes,
	string? Icon,
	string? IconColor,
	int Order,
	bool Visible,
	string? HelpFile,
	string? PolicyName);
