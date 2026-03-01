using System.Xml;
using System.Xml.Linq;
using DayZTypesHelper.Models;

namespace DayZTypesHelper.Services;

public sealed class TypesXmlService
{
    private XDocument _doc = new(new XElement("types"));
    private string? _path;

    public bool HasDestination => !string.IsNullOrWhiteSpace(_path);

    public void SetDestination(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required.", nameof(path));
        }

        _path = path;

        if (File.Exists(path))
        {
            try
            {
                _doc = XDocument.Load(path);
                if (_doc.Root == null || !string.Equals(_doc.Root.Name.LocalName, "types", StringComparison.OrdinalIgnoreCase))
                {
                    _doc = new XDocument(new XElement("types"));
                }
            }
            catch (System.Xml.XmlException ex)
            {
                throw new InvalidOperationException(
                    $"The file '{Path.GetFileName(path)}' is not valid XML: {ex.Message}", ex);
            }
        }
        else
        {
            _doc = new XDocument(new XElement("types"));
        }
    }

    /// <summary>Returns all classnames found in the loaded types.xml.</summary>
    public List<string> ReadAllClassnames()
    {
        var root = _doc.Root;
        if (root == null) return new List<string>();

        return root.Elements("type")
            .Select(t => (string?)t.Attribute("name"))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Load a separate types.xml file and return all entries (for import, not destination).</summary>
    public static List<TypeEntry> ImportFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));

        XDocument doc;
        try
        {
            doc = XDocument.Load(path);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidOperationException(
                $"The file '{Path.GetFileName(path)}' is not valid XML: {ex.Message}", ex);
        }

        var root = doc.Root;
        if (root == null || !string.Equals(root.Name.LocalName, "types", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Root element must be <types>.");

        var service = new TypesXmlService { _doc = doc };
        var result = new List<TypeEntry>();

        foreach (var typeEl in root.Elements("type"))
        {
            var name = (string?)typeEl.Attribute("name");
            if (string.IsNullOrWhiteSpace(name)) continue;

            var entry = service.TryRead(name.Trim());
            if (entry != null) result.Add(entry);
        }

        return result;
    }

    public TypeEntry? TryRead(string classname)
    {
        var root = _doc.Root;
        if (root == null)
        {
            return null;
        }

        var typeEl = root.Elements("type")
            .FirstOrDefault(t => string.Equals((string?)t.Attribute("name"), classname, StringComparison.OrdinalIgnoreCase));

        if (typeEl == null)
        {
            return null;
        }

        int ReadInt(string name, int fallback)
        {
            var el = typeEl.Element(name);
            return el != null && int.TryParse(el.Value.Trim(), out var value) ? value : fallback;
        }

        bool ReadFlag(string attr)
        {
            var flags = typeEl.Element("flags");
            var s = (string?)flags?.Attribute(attr);
            return s == "1" || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        }

        var entry = new TypeEntry
        {
            Name = classname,
            Nominal = ReadInt("nominal", 0),
            Lifetime = ReadInt("lifetime", 0),
            Restock = ReadInt("restock", 0),
            Min = ReadInt("min", 0),
            QuantMin = ReadInt("quantmin", -1),
            QuantMax = ReadInt("quantmax", -1),
            Cost = ReadInt("cost", 0),
            CountInCargo = ReadFlag("count_in_cargo"),
            CountInHoarder = ReadFlag("count_in_hoarder"),
            CountInMap = ReadFlag("count_in_map"),
            CountInPlayer = ReadFlag("count_in_player"),
            Crafted = ReadFlag("crafted"),
            Deloot = ReadFlag("deloot"),
            IsDirty = false
        };

        foreach (var c in typeEl.Elements("category"))
        {
            var v = (string?)c.Attribute("name");
            if (!string.IsNullOrWhiteSpace(v)) entry.Categories.Add(v.Trim());
        }

        foreach (var t in typeEl.Elements("tag"))
        {
            var v = (string?)t.Attribute("name");
            if (!string.IsNullOrWhiteSpace(v)) entry.Tags.Add(v.Trim());
        }

        foreach (var u in typeEl.Elements("usage"))
        {
            var v = (string?)u.Attribute("name");
            if (!string.IsNullOrWhiteSpace(v)) entry.UsageFlags.Add(v.Trim());
        }

        foreach (var vEl in typeEl.Elements("value"))
        {
            var v = (string?)vEl.Attribute("name");
            if (!string.IsNullOrWhiteSpace(v)) entry.ValueFlags.Add(v.Trim());
        }

        return entry;
    }

    public void Upsert(TypeEntry entry)
    {
        var root = _doc.Root;
        if (root == null)
        {
            _doc = new XDocument(new XElement("types"));
            root = _doc.Root!;
        }

        var typeEl = root.Elements("type")
            .FirstOrDefault(t => string.Equals((string?)t.Attribute("name"), entry.Name, StringComparison.OrdinalIgnoreCase));

        if (typeEl == null)
        {
            typeEl = new XElement("type", new XAttribute("name", entry.Name));
            root.Add(typeEl);
        }

        typeEl.Elements("category").Remove();
        typeEl.Elements("tag").Remove();
        typeEl.Elements("usage").Remove();
        typeEl.Elements("value").Remove();

        static void SetInt(XElement parent, string name, int value)
        {
            var el = parent.Element(name);
            if (el == null)
            {
                parent.Add(new XElement(name, value));
            }
            else
            {
                el.Value = value.ToString();
            }
        }

        SetInt(typeEl, "nominal", entry.Nominal);
        SetInt(typeEl, "lifetime", entry.Lifetime);
        SetInt(typeEl, "restock", entry.Restock);
        SetInt(typeEl, "min", entry.Min);
        SetInt(typeEl, "quantmin", entry.QuantMin);
        SetInt(typeEl, "quantmax", entry.QuantMax);
        SetInt(typeEl, "cost", entry.Cost);

        var flags = typeEl.Element("flags") ?? new XElement("flags");
        if (flags.Parent == null)
        {
            typeEl.Add(flags);
        }

        static string B(bool b) => b ? "1" : "0";
        flags.SetAttributeValue("count_in_cargo", B(entry.CountInCargo));
        flags.SetAttributeValue("count_in_hoarder", B(entry.CountInHoarder));
        flags.SetAttributeValue("count_in_map", B(entry.CountInMap));
        flags.SetAttributeValue("count_in_player", B(entry.CountInPlayer));
        flags.SetAttributeValue("crafted", B(entry.Crafted));
        flags.SetAttributeValue("deloot", B(entry.Deloot));

        foreach (var c in entry.Categories.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            typeEl.Add(new XElement("category", new XAttribute("name", c)));
        foreach (var t in entry.Tags.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            typeEl.Add(new XElement("tag", new XAttribute("name", t)));
        foreach (var u in entry.UsageFlags.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            typeEl.Add(new XElement("usage", new XAttribute("name", u)));
        foreach (var v in entry.ValueFlags.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            typeEl.Add(new XElement("value", new XAttribute("name", v)));
    }

    public void Save()
    {
        if (string.IsNullOrWhiteSpace(_path))
        {
            throw new InvalidOperationException("Destination not set.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");

        using var writer = XmlWriter.Create(_path, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true,
            NewLineOnAttributes = false
        });
        _doc.Save(writer);
    }
}
