using System.Globalization;
using System.Net;
using System.Text.Json;

namespace SuperBlazorComponents.Components.SuperDataGrid;

/// <summary>Versioned, row-data-free interchange format for one column configuration.</summary>
public sealed record SuperDataGridColumnProfileDocument
{
    public int Version { get; init; } = 1;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<SuperDataGridColumnSettings> Columns { get; init; } = [];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public string ToJson()
    {
        Validate();
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public static SuperDataGridColumnProfileDocument FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        if (json.Length > 1_048_576)
        {
            throw new ArgumentException("Configuration exceeds the size limit.");
        }
        var document = JsonSerializer.Deserialize<SuperDataGridColumnProfileDocument>(json, JsonOptions)
            ?? throw new ArgumentException("Missing configuration.");
        document.Validate();
        return document with { Name = ValidateName(document.Name) };
    }

    internal static string ValidateName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        name = name.Trim();
        // Names are plain text, never markup, script, CSS or file paths.
        var decoded = name;
        for (var i = 0; i < 4; i++)
        {
            decoded = WebUtility.HtmlDecode(decoded);
        }
        if (name.Length is < 1 or > 100 || decoded.Any(c => char.IsControl(c) || "<>/\\:".Contains(c)))
        {
            throw new ArgumentException("Invalid configuration name.", nameof(name));
        }
        return name;
    }

    private void Validate()
    {
        ValidateName(Name);
        if (Version != 1 || Columns is null || Columns.Count is < 1 or > 1000)
        {
            throw new ArgumentException("Unsupported or empty configuration.");
        }
        var properties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var column in Columns)
        {
            if (column is null || string.IsNullOrWhiteSpace(column.PropertyName)
                || column.PropertyName.Length > 512 || !properties.Add(column.PropertyName)
                || column.Order < 0 || !IsValidWidth(column.Width))
            {
                throw new ArgumentException("Invalid column configuration.");
            }
        }
    }

    private static bool IsValidWidth(string? width)
    {
        if (string.IsNullOrEmpty(width) || width == "auto")
        {
            return true;
        }
        foreach (var unit in new[] { "px", "%", "rem", "em", "ch", "vw" })
        {
            if (width.EndsWith(unit, StringComparison.Ordinal)
                && double.TryParse(width[..^unit.Length], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number)
                && double.IsFinite(number) && number >= 0 && number <= 100000)
            {
                return true;
            }
        }
        return false;
    }
}
