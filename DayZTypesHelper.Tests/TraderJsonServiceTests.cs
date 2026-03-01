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
}
