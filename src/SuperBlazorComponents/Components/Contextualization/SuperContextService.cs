using System.Reflection;
using SuperBlazorComponents.Configuration;

namespace SuperBlazorComponents.Components.Contextualization;

public sealed class SuperContextService
{
	private readonly SuperContextConfiguration configuration;
	private IReadOnlyList<SuperContextDescriptor>? descriptors;

	public SuperContextService(SuperContextConfiguration configuration) => this.configuration = configuration;

	public event EventHandler<SuperContextItemChangeRequest>? ItemChangeRequested;
	public event EventHandler<SuperContextSelectRequest>? SelectRequested;
	public event EventHandler<string?>? ResetRequested;
	public event EventHandler<SuperContextSelectedEventArgs>? ContextSelected;

	public IReadOnlyList<SuperContextDescriptor> GetDescriptors() => descriptors ??= DiscoverDescriptors();

	public IEnumerable<SuperContextDescriptor> GetDescriptors(Type contextType, string zone)
	{
		ArgumentNullException.ThrowIfNull(contextType);
		return GetDescriptors().Where(item =>
			item.ContextType.IsAssignableFrom(contextType)
			&& string.Equals(item.Zone, zone, StringComparison.OrdinalIgnoreCase));
	}

	public SuperContextDescriptor? GetByTitle(string? title) =>
		string.IsNullOrWhiteSpace(title)
			? null
			: GetDescriptors().FirstOrDefault(item => string.Equals(item.Title, title, StringComparison.OrdinalIgnoreCase));

	public void ChangeItem(string title, bool visible, object? tag = null, string? hostId = null) =>
		ItemChangeRequested?.Invoke(this, new(hostId, title, visible, tag));

	public void Select(string title, string? hostId = null) =>
		SelectRequested?.Invoke(this, new(hostId, title));

	public void Reset(string? hostId = null) => ResetRequested?.Invoke(this, hostId);

	internal void NotifySelected(string hostId, SuperContextItem item) =>
		ContextSelected?.Invoke(this, new(hostId, item));

	private IReadOnlyList<SuperContextDescriptor> DiscoverDescriptors()
	{
		var assemblies = configuration.Assemblies.Count > 0
			? configuration.Assemblies
			: Assembly.GetEntryAssembly() is { } entryAssembly ? [entryAssembly] : [];
		var result = new List<SuperContextDescriptor>();

		foreach (var assembly in assemblies.Distinct())
		{
			foreach (var type in GetLoadableTypes(assembly))
			{
				foreach (var attribute in type.GetCustomAttributes<SuperContextAttribute>(false))
				{
					if (string.IsNullOrWhiteSpace(attribute.Title))
					{
						continue;
					}
					var descriptor = new SuperContextDescriptor(
						attribute.ContextType,
						type,
						attribute.Title,
						attribute.Zone,
						attribute.ExcludeRoutes,
						attribute.Icon,
						attribute.IconColor,
						attribute.Order,
						attribute.Visible,
						attribute.HelpFile ?? $"{type.Name}.md",
						attribute.PolicyName);
					if (!result.Contains(descriptor))
					{
						result.Add(descriptor);
					}
				}
			}
		}
		return result;
	}

	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException exception)
		{
			return exception.Types.OfType<Type>();
		}
	}
}

public sealed record SuperContextItemChangeRequest(string? HostId, string Title, bool Visible, object? Tag);
public sealed record SuperContextSelectRequest(string? HostId, string Title);
public sealed record SuperContextSelectedEventArgs(string HostId, SuperContextItem Item);
