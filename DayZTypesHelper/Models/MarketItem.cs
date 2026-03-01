namespace DayZTypesHelper.Models;

/// <summary>
/// Represents a single item in a DayZ Expansion Market JSON file.
/// </summary>
public sealed class MarketItem
{
    public string ClassName { get; set; } = string.Empty;

    public int MaxPriceThreshold { get; set; } = 0;
    public int MinPriceThreshold { get; set; } = 0;
    public int SellPricePercent { get; set; } = -1;
    public int MaxStockThreshold { get; set; } = 0;
    public int MinStockThreshold { get; set; } = 0;
    public int QuantityPercent { get; set; } = -1;

    public List<string> SpawnAttachments { get; set; } = new();
    public List<string> Variants { get; set; } = new();

    public bool IsDirty { get; set; } = false;

    public static MarketItem CreateDefault(string className) => new() { ClassName = className };

    public MarketItem Clone()
    {
        return new MarketItem
        {
            ClassName = ClassName,
            MaxPriceThreshold = MaxPriceThreshold,
            MinPriceThreshold = MinPriceThreshold,
            SellPricePercent = SellPricePercent,
            MaxStockThreshold = MaxStockThreshold,
            MinStockThreshold = MinStockThreshold,
            QuantityPercent = QuantityPercent,
            SpawnAttachments = new List<string>(SpawnAttachments),
            Variants = new List<string>(Variants),
            IsDirty = false
        };
    }

    public void CopyFrom(MarketItem source)
    {
        MaxPriceThreshold = source.MaxPriceThreshold;
        MinPriceThreshold = source.MinPriceThreshold;
        SellPricePercent = source.SellPricePercent;
        MaxStockThreshold = source.MaxStockThreshold;
        MinStockThreshold = source.MinStockThreshold;
        QuantityPercent = source.QuantityPercent;
        SpawnAttachments = new List<string>(source.SpawnAttachments);
        Variants = new List<string>(source.Variants);
    }
}
