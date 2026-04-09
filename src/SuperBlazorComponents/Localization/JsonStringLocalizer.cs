using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

using Microsoft.Extensions.Localization;

namespace SuperBlazorComponents.Localization;

/// <summary>
/// A JSON-backed string localizer that loads translations from embedded resources
/// and optional external JSON files. Thread-safe with lazy loading per culture.
/// </summary>
internal sealed class JsonStringLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly SuperLocalizationOptions _options;
    private readonly string _defaultCulture;

    public JsonStringLocalizer(SuperLocalizationOptions options)
    {
        _options = options;
        _defaultCulture = options.DefaultCulture;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = GetString(name);
            var formatted = value is not null ? string.Format(CultureInfo.CurrentUICulture, value, arguments) : name;
            return new LocalizedString(name, formatted, resourceNotFound: value is null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var cultureName = CultureInfo.CurrentUICulture.Name;
        var translations = GetTranslationsForCulture(cultureName);

        foreach (var kvp in translations)
        {
            yield return new LocalizedString(kvp.Key, kvp.Value, resourceNotFound: false);
        }

        if (includeParentCultures)
        {
            var parentCulture = CultureInfo.CurrentUICulture.Parent;
            while (parentCulture != CultureInfo.InvariantCulture)
            {
                var parentTranslations = GetTranslationsForCulture(parentCulture.Name);
                foreach (var kvp in parentTranslations)
                {
                    if (!translations.ContainsKey(kvp.Key))
                    {
                        yield return new LocalizedString(kvp.Key, kvp.Value, resourceNotFound: false);
                    }
                }

                parentCulture = parentCulture.Parent;
            }
        }
    }

    private string? GetString(string name)
    {
        var cultureName = CultureInfo.CurrentUICulture.Name;

        // Try exact culture (e.g. "fr-CA")
        var translations = GetTranslationsForCulture(cultureName);
        if (translations.TryGetValue(name, out var value))
        {
            return value;
        }

        // Try parent culture (e.g. "fr")
        var parentCulture = CultureInfo.CurrentUICulture.Parent;
        if (parentCulture != CultureInfo.InvariantCulture)
        {
            translations = GetTranslationsForCulture(parentCulture.Name);
            if (translations.TryGetValue(name, out value))
            {
                return value;
            }
        }

        // Fallback to default culture
        if (!string.Equals(cultureName, _defaultCulture, StringComparison.OrdinalIgnoreCase))
        {
            translations = GetTranslationsForCulture(_defaultCulture);
            if (translations.TryGetValue(name, out value))
            {
                return value;
            }
        }

        return null;
    }

    private Dictionary<string, string> GetTranslationsForCulture(string cultureName)
    {
        return _cache.GetOrAdd(cultureName, LoadTranslations);
    }

    private Dictionary<string, string> LoadTranslations(string cultureName)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        // 1. Load from embedded resources (fr and en are built-in)
        LoadEmbeddedResource(cultureName, result);

        // 2. Overlay with external sources (can override built-in keys)
        LoadExternalSources(cultureName, result);

        return result;
    }

    private static void LoadEmbeddedResource(string cultureName, Dictionary<string, string> target)
    {
        // Normalize: "fr-FR" -> try "fr-FR" first, but embedded files use short codes "fr", "en"
        var assembly = typeof(JsonStringLocalizer).Assembly;
        var resourcePrefix = "SuperBlazorComponents.Localization.Resources.SuperBlazorComponents";

        // Try exact match first (e.g., "fr-FR"), then short code (e.g., "fr")
        string[] candidates = [cultureName, GetLanguageCode(cultureName)];

        foreach (var candidate in candidates)
        {
            var resourceName = $"{resourcePrefix}.{candidate}.json";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is not null)
            {
                MergeJsonStream(stream, target);
                return;
            }
        }
    }

    private void LoadExternalSources(string cultureName, Dictionary<string, string> target)
    {
        foreach (var source in _options.ExternalSources)
        {
            if (!string.Equals(source.CultureCode, cultureName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(source.CultureCode, GetLanguageCode(cultureName), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (source.FilePath is not null && File.Exists(source.FilePath))
            {
                using var stream = File.OpenRead(source.FilePath);
                MergeJsonStream(stream, target);
            }
            else if (source.JsonStream is not null)
            {
                MergeJsonStream(source.JsonStream, target);
            }
        }
    }

    private static void MergeJsonStream(Stream stream, Dictionary<string, string> target)
    {
        using var doc = JsonDocument.Parse(stream);
        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
            {
                target[property.Name] = property.Value.GetString()!;
            }
        }
    }

    private static string GetLanguageCode(string cultureName)
    {
        var dashIndex = cultureName.IndexOf('-');
        return dashIndex > 0 ? cultureName[..dashIndex] : cultureName;
    }
}
