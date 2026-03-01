using DayZTypesHelper.Models;

namespace DayZTypesHelper.Tests;

public class TypeEntryTests
{
    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new TypeEntry
        {
            Name = "M4A1",
            Nominal = 10,
            Lifetime = 3600,
            CountInCargo = true,
            IsDirty = true
        };
        original.Categories.Add("weapons");
        original.UsageFlags.Add("Military");

        var clone = original.Clone();

        Assert.Equal("M4A1", clone.Name);
        Assert.Equal(10, clone.Nominal);
        Assert.Equal(3600, clone.Lifetime);
        Assert.True(clone.CountInCargo);
        Assert.False(clone.IsDirty); // clone always starts clean
        Assert.Contains("weapons", clone.Categories);
        Assert.Contains("Military", clone.UsageFlags);

        // Mutating clone doesn't affect original
        clone.Nominal = 99;
        clone.Categories.Add("tools");
        Assert.Equal(10, original.Nominal);
        Assert.DoesNotContain("tools", original.Categories);
    }

    [Fact]
    public void CopyFrom_CopiesAllValuesExceptName()
    {
        var source = new TypeEntry
        {
            Name = "Source",
            Nominal = 42,
            Lifetime = 9999,
            Restock = 5,
            Min = 3,
            QuantMin = 10,
            QuantMax = 50,
            Cost = 200,
            CountInCargo = true,
            CountInHoarder = true,
            Crafted = true,
            Deloot = true
        };
        source.Categories.Add("food");
        source.Tags.Add("floor");
        source.UsageFlags.Add("Town");
        source.ValueFlags.Add("Tier3");

        var target = TypeEntry.CreateDefault("Target");
        target.CopyFrom(source);

        Assert.Equal("Target", target.Name); // name preserved
        Assert.Equal(42, target.Nominal);
        Assert.Equal(9999, target.Lifetime);
        Assert.Equal(5, target.Restock);
        Assert.True(target.CountInCargo);
        Assert.True(target.Crafted);
        Assert.Contains("food", target.Categories);
        Assert.Contains("floor", target.Tags);
        Assert.Contains("Town", target.UsageFlags);
        Assert.Contains("Tier3", target.ValueFlags);
    }

    [Fact]
    public void CreateDefault_SetsNameAndDefaults()
    {
        var entry = TypeEntry.CreateDefault("TestClass");
        Assert.Equal("TestClass", entry.Name);
        Assert.Equal(0, entry.Nominal);
        Assert.Equal(-1, entry.QuantMin);
        Assert.Equal(-1, entry.QuantMax);
        Assert.False(entry.IsDirty);
        Assert.Empty(entry.Categories);
    }
}
