public class EncounterTemplate
{
    public string Name;

    public int MaxPressure = 13;
    public int Duration = 7;
    public int PartialThreshold = 12;
    public int StandardThreshold = 16;
    public int ExceptionalThreshold = 20;

    public EncounterInfo.HostilityLevels Hostility = EncounterInfo.HostilityLevels.Neutral;

    public List<ApproachTags> MomentumBoostApproaches { get; set; } = new List<ApproachTags>();
    
    public List<FocusTags> PressureReducingFocuses { get; set; } = new List<FocusTags>();
    public List<FocusTags> MomentumReducingFocuses { get; set; } = new List<FocusTags>();

    public List<NarrativeTag> encounterNarrativeTags { get; set; } = new List<NarrativeTag>();
    public List<StrategicTag> encounterStrategicTags { get; set; } = new List<StrategicTag>();
}