using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ViewModel for the Market UI - contains only display data, no game logic
/// </summary>
public class MarketViewModel
{
    public string LocationName { get; init; }
    public string MarketStatus { get; init; }
    public bool IsOpen { get; init; }
    public int TraderCount { get; init; }

    // Player status for display
    public int PlayerCoins { get; init; }
    public int InventoryUsed { get; init; }
    public int InventoryCapacity { get; init; }
    public int FreeSlots => InventoryCapacity - InventoryUsed;

    public bool IsInventoryFull => FreeSlots == 0;

    // Market items with display-ready data
    public List<MarketItemViewModel> Items { get; init; } = new();

    // Available categories for filtering
    public List<string> AvailableCategories { get; init; } = new();
    public string SelectedCategory { get; set; } = "All";
}

/// <summary>
/// ViewModel for individual market items
/// </summary>
public class MarketItemViewModel
{
    public string ItemId { get; init; }
    public string Name { get; init; }
    public int BuyPrice { get; init; }
    public int SellPrice { get; init; }
    public bool CanBuy { get; init; }
    public bool CanSell { get; init; }
    public List<string> Categories { get; init; } = new();
    public Item Item { get; init; } // Reference to the full item for description and token effects
}