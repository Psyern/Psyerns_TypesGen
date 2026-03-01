namespace DayZTypesHelper.Models;

/// <summary>
/// Represents a single item entry inside a DayZ Expansion Trader JSON file.
/// The mode controls how the item can be bought/sold:
///   0 = Buy only
///   1 = Buy + Sell
///   2 = Sell only
///   3 = Hidden / attachment only
/// </summary>
public sealed class TraderItem
{
    public string ClassName { get; set; } = string.Empty;
    public int BuySellMode { get; set; } = 1;
    public bool IsDirty { get; set; } = false;

    public static TraderItem CreateDefault(string className) => new() { ClassName = className, BuySellMode = 1 };

    public TraderItem Clone()
    {
        return new TraderItem
        {
            ClassName = ClassName,
            BuySellMode = BuySellMode,
            IsDirty = false
        };
    }

    public void CopyFrom(TraderItem source)
    {
        BuySellMode = source.BuySellMode;
    }

    /// <summary>Human-readable label for the buy/sell mode.</summary>
    public static string ModeToString(int mode) => mode switch
    {
        0 => "Buy only",
        1 => "Buy + Sell",
        2 => "Sell only",
        3 => "Hidden / Attachment",
        _ => $"Unknown ({mode})"
    };
}
