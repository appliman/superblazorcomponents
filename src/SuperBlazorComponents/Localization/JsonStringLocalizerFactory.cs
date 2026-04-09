using System.Collections.Concurrent;

using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents.Localization;

/// <summary>
/// Factory that creates <see cref="JsonStringLocalizer"/> instances.
/// Shares a single localizer instance since all translations come from the same JSON sources.
/// </summary>
internal sealed class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly ConcurrentDictionary<string, IStringLocalizer> _cache = new(StringComparer.Ordinal);
    private readonly SuperLocalizationOptions _options;

    public JsonStringLocalizerFactory(SuperLocalizationOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Creates a localizer for the given resource type.
    /// All types share the same underlying JSON translations.
    /// </summary>
    public IStringLocalizer Create(Type resourceSource)
    {
        return _cache.GetOrAdd(resourceSource.FullName ?? resourceSource.Name, _ => new JsonStringLocalizer(_options));
    }

    /// <summary>
    /// Creates a localizer for the given base name and location.
    /// </summary>
    public IStringLocalizer Create(string baseName, string location)
    {
        var key = $"{location}.{baseName}";
        return _cache.GetOrAdd(key, _ => new JsonStringLocalizer(_options));
    }
}
