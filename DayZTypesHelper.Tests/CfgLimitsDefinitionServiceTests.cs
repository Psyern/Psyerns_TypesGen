using DayZTypesHelper.Services;

namespace DayZTypesHelper.Tests;

public class CfgLimitsDefinitionServiceTests
{
    [Fact]
    public void Load_ParsesSampleFile_CorrectCounts()
    {
        // Arrange – use the Samples file in the repo
        var path = Path.Combine(TestHelpers.RepoRoot, "Samples", "cfglimitsdefinition.xml");

        // Act
        var result = CfgLimitsDefinitionService.Load(path);

        // Assert
        Assert.Equal(8, result.Categories.Count);
        Assert.Contains("tools", result.Categories);
        Assert.Contains("weapons", result.Categories);
        Assert.Contains("explosives", result.Categories);

        Assert.Equal(3, result.Tags.Count);
        Assert.Contains("floor", result.Tags);
        Assert.Contains("shelves", result.Tags);
        Assert.Contains("ground", result.Tags);

        Assert.Equal(17, result.UsageFlags.Count);
        Assert.Contains("Military", result.UsageFlags);
        Assert.Contains("Police", result.UsageFlags);
        Assert.Contains("ContaminatedArea", result.UsageFlags);

        Assert.Equal(5, result.ValueFlags.Count);
        Assert.Contains("Tier1", result.ValueFlags);
        Assert.Contains("Tier4", result.ValueFlags);
        Assert.Contains("Unique", result.ValueFlags);
    }

    [Fact]
    public void Load_FromXmlString_ParsesCorrectly()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<lists>
    <categories>
        <category name=""tools""/>
        <category name=""food""/>
    </categories>
    <tags>
        <tag name=""floor""/>
    </tags>
    <usageflags>
        <usage name=""Military""/>
        <usage name=""Farm""/>
        <usage name=""Coast""/>
    </usageflags>
    <valueflags>
        <value name=""Tier1""/>
        <value name=""Tier2""/>
    </valueflags>
</lists>";

        var tmp = Path.GetTempFileName();
        File.WriteAllText(tmp, xml);

        try
        {
            // Act
            var result = CfgLimitsDefinitionService.Load(tmp);

            // Assert
            Assert.Equal(2, result.Categories.Count);
            Assert.Single(result.Tags);
            Assert.Equal(3, result.UsageFlags.Count);
            Assert.Equal(2, result.ValueFlags.Count);
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void Load_MissingSections_ReturnsEmptyLists()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<lists>
    <categories>
        <category name=""weapons""/>
    </categories>
</lists>";

        var tmp = Path.GetTempFileName();
        File.WriteAllText(tmp, xml);

        try
        {
            var result = CfgLimitsDefinitionService.Load(tmp);

            Assert.Single(result.Categories);
            Assert.Empty(result.Tags);
            Assert.Empty(result.UsageFlags);
            Assert.Empty(result.ValueFlags);
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void Load_EmptyPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CfgLimitsDefinitionService.Load(""));
        Assert.Throws<ArgumentException>(() => CfgLimitsDefinitionService.Load("   "));
    }

    [Fact]
    public void Load_DuplicateNames_AreDeduplicated()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<lists>
    <categories>
        <category name=""tools""/>
        <category name=""tools""/>
        <category name=""food""/>
    </categories>
    <tags/>
    <usageflags/>
    <valueflags/>
</lists>";

        var tmp = Path.GetTempFileName();
        File.WriteAllText(tmp, xml);

        try
        {
            var result = CfgLimitsDefinitionService.Load(tmp);
            Assert.Equal(2, result.Categories.Count);
        }
        finally
        {
            File.Delete(tmp);
        }
    }
}

/// <summary>Helper to locate the repo root for test access to Samples/.</summary>
internal static class TestHelpers
{
    public static string RepoRoot
    {
        get
        {
            var dir = Directory.GetCurrentDirectory();
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "DayZTypesHelper.sln")))
                    return dir;
                dir = Directory.GetParent(dir)?.FullName;
            }
            throw new InvalidOperationException("Could not find repo root (DayZTypesHelper.sln).");
        }
    }
}
