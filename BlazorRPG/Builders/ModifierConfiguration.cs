public class ModifierConfiguration
{
    public string Description { get; set; }
    public string Source { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public TimeWindows TimeWindow { get; set; }
    public EnergyTypes EnergyType { get; set; }
    public int EnergyReduction { get; set; }

    public ItemTypes RequiredResourceReward { get; set; } // Condition: action must reward this
    public ItemTypes AdditionalResource { get; set; }     // The extra resource to give
    public int AdditionalResourceAmount { get; set; }         // How much extra to give
    public int AdditionalCoins { get; set; }
}