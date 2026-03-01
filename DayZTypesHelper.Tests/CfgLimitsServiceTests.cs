using DayZTypesHelper.Services;

namespace DayZTypesHelper.Tests;

public class CfgLimitsServiceTests
{
    [Fact]
    public void Load_ExtractsAllSections()
    {
        var xml = @"<?xml version=""1.0""?>
<lists>
  <categories>
    <category name=""weapons""/>
    <category name=""tools""/>
  </categories>
  <tags>
    <tag name=""shelves""/>
  </tags>
  <usageflags>
    <usage name=""Military""/>
    <usage name=""Town""/>
  </usageflags>
  <valueflags>
    <value name=""Tier1""/>
    <value name=""Tier2""/>
    <value name=""Tier3""/>
  </valueflags>
</lists>";

        var path = WriteTempXml(xml);
        try
        {
            var data = CfgLimitsService.Load(path);
            Assert.Equal(2, data.Categories.Count);
            Assert.Contains("weapons", data.Categories);
            Assert.Contains("tools", data.Categories);
            Assert.Single(data.Tags);
            Assert.Equal(2, data.UsageFlags.Count);
            Assert.Equal(3, data.ValueFlags.Count);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Load_DeduplicatesAndSorts()
    {
        var xml = @"<lists>
  <categories><category name=""Zulu""/><category name=""Alpha""/><category name=""alpha""/></categories>
</lists>";

        var path = WriteTempXml(xml);
        try
        {
            var data = CfgLimitsService.Load(path);
            Assert.Equal(2, data.Categories.Count);
            Assert.Equal("Alpha", data.Categories[0]);
            Assert.Equal("Zulu", data.Categories[1]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Load_ThrowsOnNullPath()
    {
        Assert.Throws<ArgumentException>(() => CfgLimitsService.Load(""));
    }

    private static string WriteTempXml(string xml)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        File.WriteAllText(path, xml);
        return path;
    }
}
