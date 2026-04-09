namespace SuperBlazorComponents.Localization;

/// <summary>
/// Options for configuring localization in SuperBlazorComponents.
/// French and English are built-in by default. Additional cultures can be added via JSON files.
/// </summary>
public sealed class SuperLocalizationOptions
{
    private readonly List<ExternalCultureSource> _externalSources = [];

    /// <summary>
    /// Gets or sets the default culture code (e.g. "fr", "en").
    /// Defaults to French.
    /// </summary>
    public string DefaultCulture { get; set; } = "fr";

    /// <summary>
    /// Gets the registered external culture sources.
    /// </summary>
    internal IReadOnlyList<ExternalCultureSource> ExternalSources => _externalSources;

    /// <summary>
    /// Adds an external JSON file as a culture source.
    /// </summary>
    /// <param name="filePath">Absolute or relative path to the JSON file containing translations.</param>
    /// <param name="cultureCode">The culture code this file provides (e.g. "de", "es", "it").</param>
    public SuperLocalizationOptions AddJsonFile(string filePath, string cultureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureCode);

        _externalSources.Add(new ExternalCultureSource(filePath, cultureCode));
        return this;
    }

    /// <summary>
    /// Adds translations from a stream for a given culture.
    /// </summary>
    /// <param name="jsonStream">A stream containing JSON key/value translations.</param>
    /// <param name="cultureCode">The culture code this stream provides.</param>
    public SuperLocalizationOptions AddJsonStream(Stream jsonStream, string cultureCode)
    {
        ArgumentNullException.ThrowIfNull(jsonStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureCode);

        _externalSources.Add(new ExternalCultureSource(jsonStream, cultureCode));
        return this;
    }

    internal sealed record ExternalCultureSource
    {
        public string? FilePath { get; }
        public Stream? JsonStream { get; }
        public string CultureCode { get; }

        public ExternalCultureSource(string filePath, string cultureCode)
        {
            FilePath = filePath;
            CultureCode = cultureCode;
        }

        public ExternalCultureSource(Stream jsonStream, string cultureCode)
        {
            JsonStream = jsonStream;
            CultureCode = cultureCode;
        }
    }
}
