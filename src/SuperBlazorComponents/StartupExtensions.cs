using SuperBlazorComponents.Components.Contextualization;
using SuperBlazorComponents.Components.SuperDataGrid;
using SuperBlazorComponents.Components.SuperTabs;
using SuperBlazorComponents.Localization;
using SuperBlazorComponents.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents;

/// <summary>
/// Extension methods for registering SuperBlazorComponents services.
/// </summary>
public static class StartupExtensions
{
	/// <summary>
	/// Adds the services required for SuperComponents to the specified service collection.
	/// </summary>
	/// <param name="services">The service collection to which the SuperComponents services will be added.</param>
	/// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
	public static IServiceCollection AddSuperComponents(this IServiceCollection services, Action<Configuration.SuperComponentsConfiguration>? options = null)
	{
		var configuration = new Configuration.SuperComponentsConfiguration();
		options?.Invoke(configuration);
		services.AddSingleton(configuration);
		services.AddSingleton(configuration.Localization);
		services.AddSingleton(configuration.Contextualization);
		services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
		services.AddSingleton<IStringLocalizer>(sp =>
			sp.GetRequiredService<IStringLocalizerFactory>().Create(typeof(StartupExtensions)));
		if (configuration.DataGridSettingsStorageMode == Components.SuperDataGrid.DataGridSettingsStorageMode.LocalStorage)
		{
			services.AddScoped<ISuperDataGridSettingsStorage, SuperDataGridSettingsLocalStorage>();
		}
		else if (configuration.DataGridSettingsStorageMode == Components.SuperDataGrid.DataGridSettingsStorageMode.InMemory)
		{
			services.AddSingleton<ISuperDataGridSettingsStorage, InMemorySettingsStorage>();
		}
		services.AddScoped<SuperTabsService>();
		services.AddScoped<SuperContextService>();
		services.AddScoped<SuperDialogService>();
		services.AddScoped<SuperNotificationService>();
		return services;
	}
}
