public class ModifierConfiguration
{
    public string Description { get; set; }
    public string Source { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public TimeWindows TimeWindow { get; set; }
    public int EnergyReduction { get; set; }

    public ItemTypes RequiredResourceReward { get; set; }
    public ItemTypes AdditionalResource { get; set; }
    public int AdditionalResourceAmount { get; set; }
    public int AdditionalCoins { get; set; }
}