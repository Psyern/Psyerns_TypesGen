using DayZTypesHelper.Models;
using DayZTypesHelper.Services;

namespace DayZTypesHelper.Tests;

public class TypesXmlServiceTests
{
    [Fact]
    public void Upsert_And_TryRead_Roundtrip()
    {
        var service = new TypesXmlService();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        try
        {
            service.SetDestination(path);

            var entry = new TypeEntry
            {
                Name = "TestItem",
                Nominal = 10,
                Lifetime = 3600,
                Restock = 0,
                Min = 5,
                QuantMin = -1,
                QuantMax = -1,
                Cost = 100,
                CountInCargo = true,
                CountInHoarder = false,
                CountInMap = true,
                CountInPlayer = false,
                Crafted = false,
                Deloot = true
            };
            entry.Categories.Add("weapons");
            entry.Tags.Add("shelves");
            entry.UsageFlags.Add("Military");
            entry.ValueFlags.Add("Tier1");
            entry.ValueFlags.Add("Tier2");

            service.Upsert(entry);
            service.Save();

            // Reload from file
            var service2 = new TypesXmlService();
            service2.SetDestination(path);
            var read = service2.TryRead("TestItem");

            Assert.NotNull(read);
            Assert.Equal("TestItem", read.Name);
            Assert.Equal(10, read.Nominal);
            Assert.Equal(3600, read.Lifetime);
            Assert.Equal(100, read.Cost);
            Assert.True(read.CountInCargo);
            Assert.False(read.CountInHoarder);
            Assert.True(read.CountInMap);
            Assert.True(read.Deloot);
            Assert.Contains("weapons", read.Categories);
            Assert.Contains("shelves", read.Tags);
            Assert.Contains("Military", read.UsageFlags);
            Assert.Contains("Tier1", read.ValueFlags);
            Assert.Contains("Tier2", read.ValueFlags);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Upsert_OverwritesExistingEntry()
    {
        var service = new TypesXmlService();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        try
        {
            service.SetDestination(path);

            var entry1 = new TypeEntry { Name = "M4A1", Nominal = 5 };
            service.Upsert(entry1);

            var entry2 = new TypeEntry { Name = "M4A1", Nominal = 99 };
            service.Upsert(entry2);

            service.Save();

            var service2 = new TypesXmlService();
            service2.SetDestination(path);
            var read = service2.TryRead("M4A1");

            Assert.NotNull(read);
            Assert.Equal(99, read.Nominal);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void TryRead_ReturnsNullForMissing()
    {
        var service = new TypesXmlService();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        try
        {
            service.SetDestination(path);
            Assert.Null(service.TryRead("NonExistent"));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void SetDestination_ThrowsOnInvalidXml()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        File.WriteAllText(path, "<<<not valid xml>>>");
        try
        {
            var service = new TypesXmlService();
            Assert.Throws<InvalidOperationException>(() => service.SetDestination(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ReadAllClassnames_ReturnsAllNames()
    {
        var service = new TypesXmlService();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        try
        {
            service.SetDestination(path);
            service.Upsert(new TypeEntry { Name = "Zulu" });
            service.Upsert(new TypeEntry { Name = "Alpha" });
            service.Upsert(new TypeEntry { Name = "Bravo" });
            service.Save();

            var service2 = new TypesXmlService();
            service2.SetDestination(path);
            var names = service2.ReadAllClassnames();

            Assert.Equal(3, names.Count);
            Assert.Equal("Alpha", names[0]);
            Assert.Equal("Bravo", names[1]);
            Assert.Equal("Zulu", names[2]);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void ImportFromFile_ParsesValidFile()
    {
        var xml = @"<types>
  <type name=""AK74"">
    <nominal>10</nominal>
    <lifetime>7200</lifetime>
    <restock>0</restock>
    <min>5</min>
    <quantmin>-1</quantmin>
    <quantmax>-1</quantmax>
    <cost>100</cost>
    <flags count_in_cargo=""1"" count_in_hoarder=""0"" count_in_map=""1"" count_in_player=""0"" crafted=""0"" deloot=""0""/>
    <category name=""weapons""/>
    <usage name=""Military""/>
  </type>
  <type name=""Canteen"">
    <nominal>20</nominal>
    <lifetime>3600</lifetime>
    <restock>0</restock>
    <min>10</min>
    <quantmin>1</quantmin>
    <quantmax>100</quantmax>
    <cost>50</cost>
    <flags count_in_cargo=""0"" count_in_hoarder=""0"" count_in_map=""0"" count_in_player=""0"" crafted=""0"" deloot=""0""/>
  </type>
</types>";

        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        File.WriteAllText(path, xml);
        try
        {
            var entries = TypesXmlService.ImportFromFile(path);
            Assert.Equal(2, entries.Count);

            var ak = entries.First(e => e.Name == "AK74");
            Assert.Equal(10, ak.Nominal);
            Assert.Equal(7200, ak.Lifetime);
            Assert.True(ak.CountInCargo);
            Assert.Contains("weapons", ak.Categories);
            Assert.Contains("Military", ak.UsageFlags);

            var can = entries.First(e => e.Name == "Canteen");
            Assert.Equal(20, can.Nominal);
            Assert.Equal(1, can.QuantMin);
            Assert.Equal(100, can.QuantMax);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ImportFromFile_ThrowsOnCorruptXml()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        File.WriteAllText(path, "not xml at all");
        try
        {
            Assert.Throws<InvalidOperationException>(() => TypesXmlService.ImportFromFile(path));
        }
        finally { File.Delete(path); }
    }
}
