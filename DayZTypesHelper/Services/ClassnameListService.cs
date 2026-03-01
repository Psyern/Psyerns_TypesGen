namespace DayZTypesHelper.Services;

public static class ClassnameListService
{
    public static List<string> Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required.", nameof(path));
        }

        var lines = File.ReadAllLines(path);
        var items = new List<string>();

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("#") || line.StartsWith("//"))
            {
                continue;
            }

            items.Add(line);
        }

        return items
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
