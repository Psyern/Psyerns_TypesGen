using DayZTypesHelper.Models;
using DayZTypesHelper.Services;

namespace DayZTypesHelper.Tests;

public class TraderJsonServiceTests
{
    private string CreateTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"trader_test_{Guid.NewGuid()}.json");
        File.WriteAllText(path, content);
        return path;
    }

    private const string SampleTraderJson = """
    {
        "m_Version": 13,
        "DisplayName": "TestTrader",
        "MinRequiredReputation": 0,
        "MaxRequiredReputation": 2147483647,
        "RequiredFaction": "",
        "RequiredCompletedQuestID": -1,
        "TraderIcon": "Shotgun",
        "Currencies": ["expansionbanknotehryvnia", "expansiongoldbar"],
        "DisplayCurrencyValue": 1,
        "DisplayCurrencyName": "",
        "UseCategoryOrder": 0,
        "Categories": ["Weapons:1", "Ammo:0"],
        "Items": {
            "AKM": 1,
            "M4A1": 0,
            "Glock19": 2,
            "HiddenPart": 3
        }
    }
    """;

    [Fact]
    public void Load_ParsesAllItems()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            var items = svc.Load(path);

            Assert.Equal(4, items.Count);
            Assert.Equal("AKM", items[0].ClassName);
            Assert.Equal(1, items[0].BuySellMode);
            Assert.Equal("M4A1", items[1].ClassName);
            Assert.Equal(0, items[1].BuySellMode);
            Assert.Equal("Glock19", items[2].ClassName);
            Assert.Equal(2, items[2].BuySellMode);
            Assert.Equal("HiddenPart", items[3].ClassName);
            Assert.Equal(3, items[3].BuySellMode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_PreservesDisplayName()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);
            Assert.Equal("TestTrader", svc.DisplayName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ImportFromFile_ReturnsItems()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var items = TraderJsonService.ImportFromFile(path);
            Assert.Equal(4, items.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void GetCategories_ReturnsCategories()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);
            var cats = svc.GetCategories();
            Assert.Equal(2, cats.Count);
            Assert.Equal("Weapons:1", cats[0]);
            Assert.Equal("Ammo:0", cats[1]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void GetCurrencies_ReturnsCurrencies()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);
            var currencies = svc.GetCurrencies();
            Assert.Equal(2, currencies.Count);
            Assert.Contains("expansionbanknotehryvnia", currencies);
            Assert.Contains("expansiongoldbar", currencies);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Upsert_AddsNewItem()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);

            svc.Upsert(new TraderItem { ClassName = "NewGun", BuySellMode = 2 });
            var item = svc.TryRead("NewGun");
            Assert.NotNull(item);
            Assert.Equal(2, item.BuySellMode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Upsert_UpdatesExistingItem()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);

            svc.Upsert(new TraderItem { ClassName = "AKM", BuySellMode = 3 });
            var item = svc.TryRead("AKM");
            Assert.NotNull(item);
            Assert.Equal(3, item.BuySellMode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TryRead_ReturnsNullForMissing()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);
            Assert.Null(svc.TryRead("DoesNotExist"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Remove_RemovesItem()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);

            Assert.NotNull(svc.TryRead("AKM"));
            svc.Remove("AKM");
            Assert.Null(svc.TryRead("AKM"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SetDestination_CreatesNewFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"new_trader_{Guid.NewGuid()}.json");
        try
        {
            var svc = new TraderJsonService();
            svc.SetDestination(path);

            Assert.True(svc.HasFile);
            Assert.Equal("", svc.DisplayName); // default empty display name

            svc.SetDisplayName("MyTrader");
            Assert.Equal("MyTrader", svc.DisplayName);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_WritesAndRoundTrips()
    {
        var path = Path.Combine(Path.GetTempPath(), $"roundtrip_trader_{Guid.NewGuid()}.json");
        try
        {
            var svc = new TraderJsonService();
            svc.SetDestination(path);
            svc.SetDisplayName("RoundTrip");

            svc.Upsert(new TraderItem { ClassName = "AK74", BuySellMode = 1 });
            svc.Upsert(new TraderItem { ClassName = "M4A1", BuySellMode = 0 });
            svc.Save();

            // Re-load and verify
            var svc2 = new TraderJsonService();
            var items = svc2.Load(path);

            Assert.Equal("RoundTrip", svc2.DisplayName);
            Assert.Equal(2, items.Count);

            var ak = items.First(i => i.ClassName == "AK74");
            Assert.Equal(1, ak.BuySellMode);

            var m4 = items.First(i => i.ClassName == "M4A1");
            Assert.Equal(0, m4.BuySellMode);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void SetDestination_LoadsExistingFile()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.SetDestination(path);

            Assert.True(svc.HasFile);
            Assert.Equal("TestTrader", svc.DisplayName);

            var item = svc.TryRead("AKM");
            Assert.NotNull(item);
            Assert.Equal(1, item.BuySellMode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_EmptyItems_ReturnsEmptyList()
    {
        var json = """
        {
            "m_Version": 13,
            "DisplayName": "Empty",
            "Items": {}
        }
        """;
        var path = CreateTempFile(json);
        try
        {
            var svc = new TraderJsonService();
            var items = svc.Load(path);
            Assert.Empty(items);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Save_PreservesTopLevelFields()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.Load(path);

            // Modify an item
            svc.Upsert(new TraderItem { ClassName = "AKM", BuySellMode = 0 });
            svc.Save();

            // Re-read and verify top-level fields are preserved
            var json = File.ReadAllText(path);
            Assert.Contains("\"TraderIcon\"", json);
            Assert.Contains("Shotgun", json);
            Assert.Contains("\"MinRequiredReputation\"", json);
            Assert.Contains("\"Currencies\"", json);
            Assert.Contains("expansionbanknotehryvnia", json);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SetCategories_Save_PreservesItems()
    {
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            // Simulate: ImportFromFile (static, temp service) then SetDestination
            var importedItems = TraderJsonService.ImportFromFile(path);
            Assert.Equal(4, importedItems.Count);

            // Now use main service to set destination (re-reads file)
            var svc = new TraderJsonService();
            svc.SetDestination(path);

            // Add a new category (simulates drag & drop)
            var cats = svc.GetCategories();
            Assert.Equal(2, cats.Count); // "Weapons:1", "Ammo:0"

            cats.Add("Helmets:1");
            svc.SetCategories(cats);
            svc.Save();

            // Re-read the file and verify items are preserved
            var json = File.ReadAllText(path);
            Assert.Contains("\"AKM\"", json);
            Assert.Contains("\"M4A1\"", json);
            Assert.Contains("\"Glock19\"", json);
            Assert.Contains("\"HiddenPart\"", json);
            Assert.Contains("Helmets:1", json);

            // Verify via service: items AND categories are preserved
            var svc2 = new TraderJsonService();
            var items2 = svc2.Load(path);
            Assert.Equal(4, items2.Count); // all 4 items must survive
            var cats2 = svc2.GetCategories();
            Assert.Equal(3, cats2.Count); // original 2 + new Helmets
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PersistTrader_ThenDropCategory_PreservesItems()
    {
        // Simulates the full UI flow:
        // 1. Import Trader JSON (static)
        // 2. SetDestination on main service
        // 3. Populate traderCache from imported items
        // 4. PersistTrader (autosave) — upserts only dirty items, saves
        // 5. Drop a new category → SetCategories + Save
        // 6. Verify all items still in file

        var path = CreateTempFile(SampleTraderJson);
        try
        {
            // Step 1: Import (static)
            var importedItems = TraderJsonService.ImportFromFile(path);
            Assert.Equal(4, importedItems.Count);

            // Step 2: SetDestination
            var svc = new TraderJsonService();
            svc.SetDestination(path);

            // Step 3: Simulate traderCache population (items NOT dirty, as in ImportTraderJsonFromPath)
            var traderCache = new Dictionary<string, TraderItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in importedItems)
                traderCache[item.ClassName] = item; // IsDirty = false from ReadItems

            // Step 4: PersistTrader(force: false) — no dirty items → should NOT save,
            //         but old code DID always call Save(). Let's verify items survive either way.
            foreach (var kvp in traderCache)
            {
                if (kvp.Value.IsDirty)
                    svc.Upsert(kvp.Value);
            }
            svc.Save(); // simulates old behavior (always saves)

            // Step 5: Drop a new category
            var cats = svc.GetCategories();
            cats.Add("Helmets:1");
            svc.SetCategories(cats);
            svc.Save();

            // Step 6: Verify
            var svc2 = new TraderJsonService();
            var items2 = svc2.Load(path);
            Assert.Equal(4, items2.Count);
            Assert.Contains(items2, i => i.ClassName == "AKM");
            Assert.Contains(items2, i => i.ClassName == "M4A1");
            Assert.Contains(items2, i => i.ClassName == "Glock19");
            Assert.Contains(items2, i => i.ClassName == "HiddenPart");

            var cats2 = svc2.GetCategories();
            Assert.Equal(3, cats2.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PersistTrader_ForceTrue_WithDirtyItems_PreservesAllOriginalItems()
    {
        // Simulates: import, mark ONE item dirty, PersistTrader(force:true), verify ALL items survive
        var path = CreateTempFile(SampleTraderJson);
        try
        {
            var svc = new TraderJsonService();
            svc.SetDestination(path);

            // Only upsert ONE item (simulating force:true with partial cache)
            var oneItem = new TraderItem { ClassName = "AKM", BuySellMode = 2, IsDirty = true };
            svc.Upsert(oneItem);
            svc.Save();

            // Verify ALL 4 original items survive (not just the one we upserted)
            var svc2 = new TraderJsonService();
            var items = svc2.Load(path);
            Assert.Equal(4, items.Count);
            Assert.Equal(2, items.First(i => i.ClassName == "AKM").BuySellMode); // updated
            Assert.Equal(0, items.First(i => i.ClassName == "M4A1").BuySellMode); // original
            Assert.Equal(2, items.First(i => i.ClassName == "Glock19").BuySellMode); // original
            Assert.Equal(3, items.First(i => i.ClassName == "HiddenPart").BuySellMode); // original
        }
        finally
        {
            File.Delete(path);
        }
    }
}
