using DayZTypesHelper.Services;

namespace DayZTypesHelper.Tests;

public class ClassnameListServiceTests
{
    [Fact]
    public void Load_ReturnsDistinctSortedNames()
    {
        var path = CreateTempFile("Alpha\nBravo\nalpha\nCharlie\n");
        try
        {
            var result = ClassnameListService.Load(path);
            Assert.Equal(3, result.Count);
            Assert.Equal("Alpha", result[0]);
            Assert.Equal("Bravo", result[1]);
            Assert.Equal("Charlie", result[2]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Load_IgnoresCommentsAndEmptyLines()
    {
        var path = CreateTempFile("# comment\n// another\n\nActual\n  \nItem2\n");
        try
        {
            var result = ClassnameListService.Load(path);
            Assert.Equal(2, result.Count);
            Assert.Contains("Actual", result);
            Assert.Contains("Item2", result);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Load_EmptyFile_ReturnsEmpty()
    {
        var path = CreateTempFile("");
        try
        {
            var result = ClassnameListService.Load(path);
            Assert.Empty(result);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Load_ThrowsOnNullPath()
    {
        Assert.Throws<ArgumentException>(() => ClassnameListService.Load(null!));
        Assert.Throws<ArgumentException>(() => ClassnameListService.Load(""));
        Assert.Throws<ArgumentException>(() => ClassnameListService.Load("   "));
    }

    private static string CreateTempFile(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }
}
