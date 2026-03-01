namespace DayZTypesHelper.Models;

public sealed class TypeEntry
{
    public string Name { get; set; } = string.Empty;

    public int Nominal { get; set; } = 0;
    public int Lifetime { get; set; } = 0;
    public int Restock { get; set; } = 0;
    public int Min { get; set; } = 0;
    public int QuantMin { get; set; } = -1;
    public int QuantMax { get; set; } = -1;
    public int Cost { get; set; } = 0;

    public bool CountInCargo { get; set; } = false;
    public bool CountInHoarder { get; set; } = false;
    public bool CountInMap { get; set; } = false;
    public bool CountInPlayer { get; set; } = false;
    public bool Crafted { get; set; } = false;
    public bool Deloot { get; set; } = false;

    public HashSet<string> Categories { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Tags { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> UsageFlags { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ValueFlags { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsDirty { get; set; } = false;

    public static TypeEntry CreateDefault(string name) => new() { Name = name };

    /// <summary>Deep-clone this entry (sets IsDirty = false on clone).</summary>
    public TypeEntry Clone()
    {
        var copy = new TypeEntry
        {
            Name = Name,
            Nominal = Nominal,
            Lifetime = Lifetime,
            Restock = Restock,
            Min = Min,
            QuantMin = QuantMin,
            QuantMax = QuantMax,
            Cost = Cost,
            CountInCargo = CountInCargo,
            CountInHoarder = CountInHoarder,
            CountInMap = CountInMap,
            CountInPlayer = CountInPlayer,
            Crafted = Crafted,
            Deloot = Deloot,
            IsDirty = false
        };

        foreach (var c in Categories) copy.Categories.Add(c);
        foreach (var t in Tags) copy.Tags.Add(t);
        foreach (var u in UsageFlags) copy.UsageFlags.Add(u);
        foreach (var v in ValueFlags) copy.ValueFlags.Add(v);

        return copy;
    }

    /// <summary>Copy all values (except Name) from another entry into this one.</summary>
    public void CopyFrom(TypeEntry source)
    {
        Nominal = source.Nominal;
        Lifetime = source.Lifetime;
        Restock = source.Restock;
        Min = source.Min;
        QuantMin = source.QuantMin;
        QuantMax = source.QuantMax;
        Cost = source.Cost;

        CountInCargo = source.CountInCargo;
        CountInHoarder = source.CountInHoarder;
        CountInMap = source.CountInMap;
        CountInPlayer = source.CountInPlayer;
        Crafted = source.Crafted;
        Deloot = source.Deloot;

        Categories.Clear();
        Tags.Clear();
        UsageFlags.Clear();
        ValueFlags.Clear();

        foreach (var c in source.Categories) Categories.Add(c);
        foreach (var t in source.Tags) Tags.Add(t);
        foreach (var u in source.UsageFlags) UsageFlags.Add(u);
        foreach (var v in source.ValueFlags) ValueFlags.Add(v);
    }
}
