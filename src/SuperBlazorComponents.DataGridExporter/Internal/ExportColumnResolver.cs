using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

using SuperBlazorComponents.Components.SuperDataGrid;

namespace SuperBlazorComponents.DataGridExporter.Internal;

internal static partial class ExportColumnResolver
{
    private static readonly ConcurrentDictionary<(Type Type, string Path), Func<object, object?>> Accessors = new();

    public static IReadOnlyList<ExportColumn<TItem>> Resolve<TItem>(SuperDataGrid<TItem> grid)
    {
        var result = new List<ExportColumn<TItem>>();

        int idx = 0;
        foreach (var column in grid.ColumnsCollection.Where(c => c.Exportable && c.IsCurrentlyVisible))
        {
            var header = FirstNotEmpty(
                column.ExportHeader,
                ExtractHeaderText(column.HeaderTemplate),
                column.Title,
                column.Property);

            if (string.IsNullOrWhiteSpace(header))
            {
                throw new InvalidOperationException(
                    $"An exportable grid column has no resolvable header. Set ExportHeader on the column index {idx}.");
            }

            idx++;

            Func<TItem, object?> accessor;
            if (column.ExportValue is not null)
            {
                accessor = column.ExportValue;
            }
            else if (column.Template is not null)
            {
                var cellTemplate = column.Template;
                accessor = item => ExtractCellText(cellTemplate, item);
            }
            else if (!string.IsNullOrWhiteSpace(column.Property))
            {
                var untypedAccessor = Accessors.GetOrAdd(
                    (typeof(TItem), column.Property),
                    static key => CreateAccessor(key.Type, key.Path));
                accessor = item => item is null ? null : untypedAccessor(item);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Column '{header}' has no resolvable value. Set a Template, Property, For, or ExportValue on the column.");
            }

            result.Add(new ExportColumn<TItem>(header, column.FormatString, accessor));
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException("The grid has no visible exportable columns.");
        }

        return result;
    }

    private static Func<object, object?> CreateAccessor(Type itemType, string propertyPath)
    {
        var members = new List<MemberInfo>();
        var currentType = itemType;

        foreach (var segment in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var member = (MemberInfo?)currentType.GetProperty(segment, BindingFlags.Instance | BindingFlags.Public)
                ?? currentType.GetField(segment, BindingFlags.Instance | BindingFlags.Public);

            if (member is null)
            {
                throw new InvalidOperationException($"Property path '{propertyPath}' was not found on '{itemType.Name}'.");
            }

            members.Add(member);
            currentType = member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new UnreachableException()
            };
        }

        return instance =>
        {
            object? value = instance;
            foreach (var member in members)
            {
                if (value is null)
                {
                    return null;
                }
                value = member switch
                {
                    PropertyInfo property => property.GetValue(value),
                    FieldInfo field => field.GetValue(value),
                    _ => null
                };
            }
            return value;
        };
    }

#pragma warning disable BL0006 // Deliberately inspect simple text frames; component output falls back to Title/Property.
    private static string? ExtractCellText<TItem>(RenderFragment<TItem> template, TItem item)
    {
        var builder = new RenderTreeBuilder();
        template(item)(builder);
        return ExtractRenderedText(builder.GetFrames());
    }

    private static string? ExtractHeaderText(RenderFragment? template)
    {
        if (template is null)
        {
            return null;
        }

        var builder = new RenderTreeBuilder();
        template(builder);
        return ExtractRenderedText(builder.GetFrames());
    }

    private static string? ExtractRenderedText(ArrayRange<RenderTreeFrame> frames)
    {
        var parts = new List<string>();

        for (var i = 0; i < frames.Count; i++)
        {
            var frame = frames.Array[i];
            if (frame.FrameType == RenderTreeFrameType.Text)
            {
                parts.Add(frame.TextContent);
            }
            else if (frame.FrameType == RenderTreeFrameType.Markup)
            {
                parts.Add(HtmlTagRegex().Replace(frame.MarkupContent, " "));
            }
            else if (frame.FrameType == RenderTreeFrameType.Attribute
                     && frame.AttributeValue is RenderFragment childContent)
            {
                var childBuilder = new RenderTreeBuilder();
                childContent(childBuilder);
                var childText = ExtractRenderedText(childBuilder.GetFrames());
                if (!string.IsNullOrWhiteSpace(childText))
                {
                    parts.Add(childText);
                }
            }
        }

        var text = WhitespaceRegex().Replace(WebUtility.HtmlDecode(string.Join(" ", parts)), " ").Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }
#pragma warning restore BL0006

    private static string? FirstNotEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();
}
