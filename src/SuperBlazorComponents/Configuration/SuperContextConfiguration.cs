using System.Reflection;

namespace SuperBlazorComponents.Configuration;

public sealed class SuperContextConfiguration
{
	private readonly List<Assembly> assemblies = [];

	public IReadOnlyList<Assembly> Assemblies => assemblies;

	public SuperContextConfiguration AddAssembly(Assembly assembly)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		if (!assemblies.Contains(assembly))
		{
			assemblies.Add(assembly);
		}
		return this;
	}

	public SuperContextConfiguration AddAssembly<T>()
	{
		return AddAssembly(typeof(T).Assembly);
	}
}
