using DayZTypesHelper.Models;

namespace DayZTypesHelper.Tests;

public class TraderItemTests
{
    [Fact]
    public void CreateDefault_SetsClassNameAndMode1()
    {
        var item = TraderItem.CreateDefault("TestGun");
        Assert.Equal("TestGun", item.ClassName);
        Assert.Equal(1, item.BuySellMode); // default: Buy + Sell
        Assert.False(item.IsDirty);
    }

    [Fact]
    public void Clone_CreatesCopy()
    {
        var item = new TraderItem { ClassName = "Gun1", BuySellMode = 2, IsDirty = true };
        var clone = item.Clone();

        Assert.Equal("Gun1", clone.ClassName);
        Assert.Equal(2, clone.BuySellMode);
        Assert.False(clone.IsDirty); // clone resets dirty
    }

    [Fact]
    public void CopyFrom_CopiesMode()
    {
        var source = new TraderItem { ClassName = "Source", BuySellMode = 3 };
        var target = new TraderItem { ClassName = "Target", BuySellMode = 0 };

        target.CopyFrom(source);

        Assert.Equal("Target", target.ClassName); // name unchanged
        Assert.Equal(3, target.BuySellMode);
    }

    [Theory]
    [InlineData(0, "Buy only")]
    [InlineData(1, "Buy + Sell")]
    [InlineData(2, "Sell only")]
    [InlineData(3, "Hidden / Attachment")]
    [InlineData(99, "Unknown (99)")]
    public void ModeToString_ReturnsExpected(int mode, string expected)
    {
        Assert.Equal(expected, TraderItem.ModeToString(mode));
    }
}
