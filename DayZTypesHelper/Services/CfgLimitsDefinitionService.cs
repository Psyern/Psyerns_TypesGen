using System.Xml.Linq;

namespace DayZTypesHelper.Services;

/// <summary>
/// Parses a DayZ cfglimitsdefinition.xml file to extract the available
/// category, tag, usage-flag, and value-flag names.
/// </summary>
public sealed class CfgLimitsDefinitionResult
{
    public List<string> Categories { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<string> UsageFlags { get; init; } = [];
    public List<string> ValueFlags { get; init; } = [];
}

public static class CfgLimitsDefinitionService
{
    /// <summary>
    /// Loads a cfglimitsdefinition.xml and returns the four lists of names.
    /// </summary>
    public static CfgLimitsDefinitionResult Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));

        var doc = XDocument.Load(path);
        var root = doc.Root ?? throw new InvalidOperationException("Invalid cfglimitsdefinition.xml: missing root element.");

        return new CfgLimitsDefinitionResult
        {
            Categories = ReadNames(root, "categories", "category"),
            Tags = ReadNames(root, "tags", "tag"),
            UsageFlags = ReadNames(root, "usageflags", "usage"),
            ValueFlags = ReadNames(root, "valueflags", "value"),
        };
    }

    private static List<string> ReadNames(XElement root, string sectionName, string elementName)
    {
        var section = root.Element(sectionName);
        if (section is null) return [];

        return section
            .Elements(elementName)
            .Select(e => e.Attribute("name")?.Value)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
