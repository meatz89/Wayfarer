/// <summary>
/// Location personality types that apply universal modifier rules
/// </summary>
public enum LocationPersonalityType
{
    SequentialSite,    // Must play cards in order, methodical progression
    PublicLocation,    // Exposure threshold -3, OBSERVE +1 Exposure, speed critical
    LayeredMystery,    // Progress threshold +5, specialization bonuses
    DangerousSite,     // Same category twice = +10% danger, variety rewarded
    TimeCritical       // Hard Time cap, all costs/yields capped at 2
}
