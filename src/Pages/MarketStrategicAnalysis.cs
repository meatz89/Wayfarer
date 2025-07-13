// Supporting classes for market strategic analysis
public class MarketStrategicAnalysis
{
    public List<Item> EquipmentItems { get; set; } = new();
    public List<Item> RouteEnablingItems { get; set; } = new();
    public List<Item> ContractRelevantItems { get; set; } = new();
    public List<MarketEquipmentOpportunity> EquipmentInvestmentOpportunities { get; set; } = new();
    public List<TradeOpportunityIndicator> TradeOpportunityIndicators { get; set; } = new();
}

public class MarketEquipmentOpportunity
{
    public EquipmentCategory Category { get; set; }
    public List<Item> AvailableItems { get; set; } = new();
    public int RoutesUnlocked { get; set; }
    public int ContractsEnabled { get; set; }
    public string StrategicValue { get; set; } = "";
}

public class TradeOpportunityIndicator
{
    public string ItemName { get; set; } = "";
    public string OpportunityType { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ItemStrategicValue
{
    public bool IsHighValue { get; set; }
    public List<StrategicValueIndicator> ValueIndicators { get; set; } = new();
}

public class StrategicValueIndicator
{
    public string Type { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
}