using System.Text.Json;
using System.Text.Json.Nodes;
using DayZTypesHelper.Models;

namespace DayZTypesHelper.Services;

/// <summary>
/// Reads and writes DayZ Expansion Market JSON files.
/// Preserves all top-level fields (m_Version, DisplayName, Icon, Color, etc.)
/// while allowing item-level edits.
/// </summary>
public sealed class MarketJsonService
{
    private string? _filePath;
    private JsonNode? _rootNode;

    public bool HasFile => _filePath != null && _rootNode != null;
    public string? FilePath => _filePath;

    /// <summary>Display name from the JSON (e.g. "#STR_EXPANSION_MARKET_CATEGORY_AMMO").</summary>
    public string DisplayName => _rootNode?["DisplayName"]?.GetValue<string>() ?? "(unknown)";

    /// <summary>Set the display name on the root JSON object.</summary>
    public void SetDisplayName(string name)
    {
        if (_rootNode != null)
            _rootNode["DisplayName"] = name;
    }

    /// <summary>Load a market JSON file and return all items.</summary>
    public List<MarketItem> Load(string path)
    {
        var json = File.ReadAllText(path);
        _rootNode = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })
            ?? throw new InvalidOperationException("Failed to parse JSON.");
        _filePath = path;

        var items = new List<MarketItem>();
        var itemsArray = _rootNode["Items"]?.AsArray();
        if (itemsArray == null) return items;

        foreach (var node in itemsArray)
        {
            if (node == null) continue;

            items.Add(new MarketItem
            {
                ClassName = node["ClassName"]?.GetValue<string>() ?? string.Empty,
                MaxPriceThreshold = node["MaxPriceThreshold"]?.GetValue<int>() ?? 0,
                MinPriceThreshold = node["MinPriceThreshold"]?.GetValue<int>() ?? 0,
                SellPricePercent = node["SellPricePercent"]?.GetValue<int>() ?? -1,
                MaxStockThreshold = node["MaxStockThreshold"]?.GetValue<int>() ?? 0,
                MinStockThreshold = node["MinStockThreshold"]?.GetValue<int>() ?? 0,
                QuantityPercent = node["QuantityPercent"]?.GetValue<int>() ?? -1,
                SpawnAttachments = ReadStringArray(node["SpawnAttachments"]),
                Variants = ReadStringArray(node["Variants"]),
                IsDirty = false
            });
        }

        return items;
    }

    /// <summary>Import items from a file without setting it as destination.</summary>
    public static List<MarketItem> ImportFromFile(string path)
    {
        var svc = new MarketJsonService();
        return svc.Load(path);
    }

    /// <summary>Set this file as the destination for saving.</summary>
    public void SetDestination(string path)
    {
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            _rootNode = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })
                ?? throw new InvalidOperationException("Failed to parse JSON.");
        }
        else
        {
            // Create a new market file structure
            _rootNode = JsonNode.Parse("""
            {
                "m_Version": 12,
                "DisplayName": "",
                "Icon": "Deliver",
                "Color": "FBFCFEFF",
                "IsExchange": 0,
                "InitStockPercent": 75.0,
                "Items": []
            }
            """)!;
        }
        _filePath = path;
    }

    /// <summary>Update or insert an item in the JSON.</summary>
    public void Upsert(MarketItem item)
    {
        if (_rootNode == null) return;

        var itemsArray = _rootNode["Items"]?.AsArray();
        if (itemsArray == null)
        {
            itemsArray = new JsonArray();
            _rootNode["Items"] = itemsArray;
        }

        // Find existing
        JsonNode? existing = null;
        int existingIndex = -1;
        for (int i = 0; i < itemsArray.Count; i++)
        {
            var cn = itemsArray[i]?["ClassName"]?.GetValue<string>();
            if (string.Equals(cn, item.ClassName, StringComparison.OrdinalIgnoreCase))
            {
                existing = itemsArray[i];
                existingIndex = i;
                break;
            }
        }

        var node = new JsonObject
        {
            ["ClassName"] = item.ClassName,
            ["MaxPriceThreshold"] = item.MaxPriceThreshold,
            ["MinPriceThreshold"] = item.MinPriceThreshold,
            ["SellPricePercent"] = item.SellPricePercent,
            ["MaxStockThreshold"] = item.MaxStockThreshold,
            ["MinStockThreshold"] = item.MinStockThreshold,
            ["QuantityPercent"] = item.QuantityPercent,
            ["SpawnAttachments"] = ToJsonArray(item.SpawnAttachments),
            ["Variants"] = ToJsonArray(item.Variants)
        };

        if (existingIndex >= 0)
        {
            itemsArray[existingIndex] = node;
        }
        else
        {
            itemsArray.Add(node);
        }
    }

    /// <summary>Save the JSON to file.</summary>
    public void Save()
    {
        if (_filePath == null || _rootNode == null) return;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = _rootNode.ToJsonString(options);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>Try to read an existing item by classname.</summary>
    public MarketItem? TryRead(string className)
    {
        if (_rootNode == null) return null;

        var itemsArray = _rootNode["Items"]?.AsArray();
        if (itemsArray == null) return null;

        foreach (var node in itemsArray)
        {
            if (node == null) continue;
            var cn = node["ClassName"]?.GetValue<string>();
            if (string.Equals(cn, className, StringComparison.OrdinalIgnoreCase))
            {
                return new MarketItem
                {
                    ClassName = cn ?? string.Empty,
                    MaxPriceThreshold = node["MaxPriceThreshold"]?.GetValue<int>() ?? 0,
                    MinPriceThreshold = node["MinPriceThreshold"]?.GetValue<int>() ?? 0,
                    SellPricePercent = node["SellPricePercent"]?.GetValue<int>() ?? -1,
                    MaxStockThreshold = node["MaxStockThreshold"]?.GetValue<int>() ?? 0,
                    MinStockThreshold = node["MinStockThreshold"]?.GetValue<int>() ?? 0,
                    QuantityPercent = node["QuantityPercent"]?.GetValue<int>() ?? -1,
                    SpawnAttachments = ReadStringArray(node["SpawnAttachments"]),
                    Variants = ReadStringArray(node["Variants"]),
                    IsDirty = false
                };
            }
        }

        return null;
    }

    private static List<string> ReadStringArray(JsonNode? node)
    {
        var list = new List<string>();
        if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                var s = item?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
            }
        }
        return list;
    }

    private static JsonArray ToJsonArray(List<string> items)
    {
        var arr = new JsonArray();
        foreach (var s in items) arr.Add(s);
        return arr;
    }
}
