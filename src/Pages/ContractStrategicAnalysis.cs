// Supporting classes for contract strategic analysis
public class ContractStrategicAnalysis
{
    public List<ContractEquipmentInvestment> EquipmentInvestmentNeeded { get; set; } = new();
    public List<RouteEnablementBenefit> RouteEnablementOpportunities { get; set; } = new();
}

public class ContractEquipmentInvestment
{
    public EquipmentCategory Category { get; set; }
    public string Benefit { get; set; } = "";
    public List<Item> AvailableItems { get; set; } = new();
}

public class RouteEnablementBenefit
{
    public string RouteName { get; set; } = "";
    public string EnablementReason { get; set; } = "";
}

public class ContractTimePressure
{
    public string PressureLevel { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ContractSynergyAnalysis
{
    public List<ContractSynergy> SynergyContracts { get; set; } = new();
}

public class ContractSynergy
{
    public string ContractDescription { get; set; } = "";
    public string SharedBenefit { get; set; } = "";
}