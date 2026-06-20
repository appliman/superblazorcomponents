namespace SuperBlazorComponents.Components.Contextualization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class SuperContextAttribute : Attribute
{
	protected SuperContextAttribute(Type contextType) => ContextType = contextType;

	public Type ContextType { get; }
	public string? ExcludeRoutes { get; set; }
	public string Title { get; set; } = null!;
	public string? Icon { get; set; }
	public string? IconColor { get; set; }
	public int Order { get; set; } = int.MaxValue;
	public string Zone { get; set; } = SuperContextZones.Default;
	public bool Visible { get; set; } = true;
	public string? HelpFile { get; set; }
	public string? PolicyName { get; set; }
}

public class SuperContextAttribute<TContext> : SuperContextAttribute
{
	public SuperContextAttribute() : base(typeof(TContext))
	{
	}
}

public static class SuperContextZones
{
	public const string Default = "default";
	public const string Bottom = "bottom";
	public const string Right = "right";
}
