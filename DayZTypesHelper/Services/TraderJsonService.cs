using System.Text.Json;
using System.Text.Json.Nodes;
using DayZTypesHelper.Models;

namespace DayZTypesHelper.Services;

/// <summary>
/// Reads and writes DayZ Expansion Trader JSON files.
/// Preserves all top-level fields (m_Version, DisplayName, TraderIcon, Currencies, Categories, etc.)
/// while allowing item-level edits.
///
/// Trader JSON format:
///   Items is a JSON object (Map&lt;string, int&gt;) where key=classname, value=buy/sell mode:
///     0 = Buy only, 1 = Buy + Sell, 2 = Sell only, 3 = Hidden / Attachment
/// </summary>
public sealed class TraderJsonService
{
    private string? _filePath;
    private JsonNode? _rootNode;

    public bool HasFile => _filePath != null && _rootNode != null;
    public string? FilePath => _filePath;

    /// <summary>Display name from the JSON (e.g. "Marina Wodka-Queen").</summary>
    public string DisplayName => _rootNode?["DisplayName"]?.GetValue<string>() ?? "(unknown)";

    /// <summary>Trader icon from the JSON (e.g. "Shotgun").</summary>
    public string TraderIcon => _rootNode?["TraderIcon"]?.GetValue<string>() ?? "";

    /// <summary>Set the display name on the root JSON object.</summary>
    public void SetDisplayName(string name)
    {
        if (_rootNode != null)
            _rootNode["DisplayName"] = name;
    }

    /// <summary>Set the trader icon on the root JSON object.</summary>
    public void SetTraderIcon(string icon)
    {
        if (_rootNode != null)
            _rootNode["TraderIcon"] = icon;
    }

    /// <summary>Set the currencies list on the root JSON object.</summary>
    public void SetCurrencies(List<string> currencies)
    {
        if (_rootNode == null) return;
        var arr = new JsonArray();
        foreach (var c in currencies) arr.Add(c);
        _rootNode["Currencies"] = arr;
    }

    /// <summary>Set the categories list on the root JSON object.</summary>
    public void SetCategories(List<string> categories)
    {
        if (_rootNode == null) return;
        var arr = new JsonArray();
        foreach (var c in categories) arr.Add(c);
        _rootNode["Categories"] = arr;
    }

    /// <summary>Load a trader JSON file and return all items.</summary>
    public List<TraderItem> Load(string path)
    {
        var json = File.ReadAllText(path);
        _rootNode = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        }) ?? throw new InvalidOperationException("Failed to parse Trader JSON.");
        _filePath = path;

        return ReadItems();
    }

    /// <summary>Import items from a file without setting it as destination.</summary>
    public static List<TraderItem> ImportFromFile(string path)
    {
        var svc = new TraderJsonService();
        return svc.Load(path);
    }

    /// <summary>Set this file as the destination for saving.</summary>
    public void SetDestination(string path)
    {
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            _rootNode = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            }) ?? throw new InvalidOperationException("Failed to parse Trader JSON.");
        }
        else
        {
            // Create a new trader file structure with sensible defaults
            _rootNode = JsonNode.Parse("""
            {
                "m_Version": 13,
                "DisplayName": "",
                "MinRequiredReputation": 0,
                "MaxRequiredReputation": 2147483647,
                "RequiredFaction": "",
                "RequiredCompletedQuestID": -1,
                "TraderIcon": "Deliver",
                "Currencies": [],
                "DisplayCurrencyValue": 1,
                "DisplayCurrencyName": "",
                "UseCategoryOrder": 0,
                "Categories": [],
                "Items": {}
            }
            """)!;
        }
        _filePath = path;
    }

    /// <summary>Update or insert an item in the Items object.</summary>
    public void Upsert(TraderItem item)
    {
        if (_rootNode == null) return;

        var itemsObj = _rootNode["Items"];
        if (itemsObj == null || itemsObj is not JsonObject)
        {
            var newObj = new JsonObject();
            _rootNode["Items"] = newObj;
            itemsObj = newObj;
        }

        // Set or overwrite the classname → mode entry
        itemsObj[item.ClassName] = item.BuySellMode;
    }

    /// <summary>Remove an item from the Items object.</summary>
    public void Remove(string className)
    {
        if (_rootNode == null) return;
        var itemsObj = _rootNode["Items"]?.AsObject();
        if (itemsObj == null) return;

        // Items keys are case-sensitive in JSON; find exact match
        string? keyToRemove = null;
        foreach (var kvp in itemsObj)
        {
            if (string.Equals(kvp.Key, className, StringComparison.OrdinalIgnoreCase))
            {
                keyToRemove = kvp.Key;
                break;
            }
        }
        if (keyToRemove != null)
            itemsObj.Remove(keyToRemove);
    }

    /// <summary>Save the JSON to file.</summary>
    public void Save()
    {
        if (_filePath == null || _rootNode == null) return;

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = _rootNode.ToJsonString(options);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>Try to read an existing item by classname.</summary>
    public TraderItem? TryRead(string className)
    {
        if (_rootNode == null) return null;

        var itemsObj = _rootNode["Items"]?.AsObject();
        if (itemsObj == null) return null;

        foreach (var kvp in itemsObj)
        {
            if (string.Equals(kvp.Key, className, StringComparison.OrdinalIgnoreCase))
            {
                return new TraderItem
                {
                    ClassName = kvp.Key,
                    BuySellMode = kvp.Value?.GetValue<int>() ?? 1,
                    IsDirty = false
                };
            }
        }

        return null;
    }

    /// <summary>Read the Categories list from the root JSON.</summary>
    public List<string> GetCategories()
    {
        var list = new List<string>();
        if (_rootNode?["Categories"] is JsonArray arr)
        {
            foreach (var item in arr)
            {
                var s = item?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
            }
        }
        return list;
    }

    /// <summary>Read the Currencies list from the root JSON.</summary>
    public List<string> GetCurrencies()
    {
        var list = new List<string>();
        if (_rootNode?["Currencies"] is JsonArray arr)
        {
            foreach (var item in arr)
            {
                var s = item?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
            }
        }
        return list;
    }

    // ── Private helpers ──

    private List<TraderItem> ReadItems()
    {
        var items = new List<TraderItem>();
        var itemsObj = _rootNode?["Items"]?.AsObject();
        if (itemsObj == null) return items;

        foreach (var kvp in itemsObj)
        {
            items.Add(new TraderItem
            {
                ClassName = kvp.Key,
                BuySellMode = kvp.Value?.GetValue<int>() ?? 1,
                IsDirty = false
            });
        }

        return items;
    }
}
