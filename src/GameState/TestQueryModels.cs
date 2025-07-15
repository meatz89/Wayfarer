namespace Wayfarer.Game.MainSystem;

/// <summary>
/// Result model for contract completion status queries.
/// Provides comprehensive information about contract progress for testing.
/// </summary>
public class ContractCompletionResult
{
    public string ContractId { get; set; } = "";
    public ContractStatus Status { get; set; }
    public float ProgressPercentage { get; set; }
    public List<ContractStep> CompletedSteps { get; set; } = new List<ContractStep>();
    public List<ContractTransaction> CompletedTransactions { get; set; } = new List<ContractTransaction>();
    public List<string> CompletedDestinations { get; set; } = new List<string>();
    public List<string> CompletedNPCConversations { get; set; } = new List<string>();
    public List<string> CompletedLocationActions { get; set; } = new List<string>();
}

/// <summary>
/// Enum representing the various states a contract can be in.
/// </summary>
public enum ContractStatus
{
    NotFound,       // Contract doesn't exist
    NotActive,      // Contract exists but not accepted
    Active,         // Contract is active and in progress
    Completed,      // Contract has been completed successfully
    Failed          // Contract has failed (missed deadline, etc.)
}

/// <summary>
/// Snapshot of current player state for testing verification.
/// Provides complete view of player status without exposing internal objects.
/// </summary>
public class PlayerStateSnapshot
{
    public int Coins { get; set; }
    public int Stamina { get; set; }
    public int Concentration { get; set; }
    public string CurrentLocationId { get; set; } = "";
    public string CurrentLocationSpotId { get; set; } = "";
    public List<string> InventoryItems { get; set; } = new List<string>();
    public int Reputation { get; set; }
    public List<string> KnownContracts { get; set; } = new List<string>();
    public List<Information> KnownInformation { get; set; } = new List<Information>();
}

/// <summary>
/// Snapshot of current game time state for testing verification.
/// </summary>
public class GameTimeSnapshot
{
    public int CurrentDay { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }
    public int UsedTimeBlocks { get; set; }
    public bool CanPerformTimeBlockAction { get; set; }
}

/// <summary>
/// Market price information for an item at a specific location.
/// Used for testing market price queries across multiple locations.
/// </summary>
public class MarketPriceInfo
{
    public string LocationId { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string ItemId { get; set; } = "";
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public bool CanBuy { get; set; }
    public float SupplyLevel { get; set; }
}

/// <summary>
/// Result of executing a trade action, with detailed before/after state.
/// Provides comprehensive information for testing trade outcomes.
/// </summary>
public class TradeActionResult
{
    public bool Success { get; set; }
    public string Action { get; set; } = "";  // "buy" or "sell"
    public string ItemId { get; set; } = "";
    public string LocationId { get; set; } = "";
    public int CoinsBefore { get; set; }
    public int CoinsAfter { get; set; }
    public int CoinsChanged { get; set; }
    public bool HadItemBefore { get; set; }
    public bool HasItemAfter { get; set; }
    public string? ErrorMessage { get; set; }
    public int TransactionPrice { get; set; }
}

/// <summary>
/// Result of executing a travel action, with detailed before/after state.
/// Provides comprehensive information for testing travel outcomes.
/// </summary>
public class TravelActionResult
{
    public bool Success { get; set; }
    public string DestinationId { get; set; } = "";
    public string? RouteId { get; set; }
    public string LocationBefore { get; set; } = "";
    public string LocationAfter { get; set; } = "";
    public int TimeBlocksBefore { get; set; }
    public int TimeBlocksAfter { get; set; }
    public int TimeBlocksUsed { get; set; }
    public int StaminaBefore { get; set; }
    public int StaminaAfter { get; set; }
    public int StaminaUsed { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of executing a contract action, with detailed before/after state.
/// Provides comprehensive information for testing contract action outcomes.
/// </summary>
public class ContractActionResult
{
    public bool Success { get; set; }
    public string ContractId { get; set; } = "";
    public string Action { get; set; } = "";
    public ContractStatus StatusBefore { get; set; }
    public ContractStatus StatusAfter { get; set; }
    public float ProgressBefore { get; set; }
    public float ProgressAfter { get; set; }
    public int CoinsBefore { get; set; }
    public int CoinsAfter { get; set; }
    public int CoinsChanged { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Market arbitrage analysis for an item across all locations.
/// Shows profit potential and best buy/sell opportunities.
/// </summary>
public class MarketArbitrageInfo
{
    public string ItemId { get; set; } = "";
    public MarketPriceInfo? BestBuyLocation { get; set; }
    public MarketPriceInfo? BestSellLocation { get; set; }
    public int MaxProfit { get; set; }
    public List<MarketPriceInfo> AllPrices { get; set; } = new List<MarketPriceInfo>();
}