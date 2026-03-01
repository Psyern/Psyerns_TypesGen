using System.Xml.Linq;

namespace DayZTypesHelper.Services;

public sealed class CfgLimitsData
{
    public List<string> Categories { get; init; } = new();
    public List<string> Tags { get; init; } = new();
    public List<string> UsageFlags { get; init; } = new();
    public List<string> ValueFlags { get; init; } = new();
}

public static class CfgLimitsService
{
    public static CfgLimitsData Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required.", nameof(path));
        }

        var doc = XDocument.Load(path);

        static List<string> Extract(XDocument document, string needle)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var el in document.Descendants())
            {
                var name = el.Name.LocalName;
                if (!name.Contains(needle, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = (string?)el.Attribute("name")
                            ?? (string?)el.Attribute("id")
                            ?? el.Value?.Trim();

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                set.Add(value.Trim());
            }

            return set.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        }

        return new CfgLimitsData
        {
            Categories = Extract(doc, "category"),
            Tags = Extract(doc, "tag"),
            UsageFlags = Extract(doc, "usage"),
            ValueFlags = Extract(doc, "value")
        };
    }
}
