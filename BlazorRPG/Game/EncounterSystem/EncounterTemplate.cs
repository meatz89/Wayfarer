public class EncounterTemplate
{
    public int Duration = 6;
    public int PartialThreshold = 12;
    public int StandardThreshold = 16;
    public int ExceptionalThreshold = 20;

    public EncounterInfo.HostilityLevels hostility = EncounterInfo.HostilityLevels.Neutral;

    public List<ApproachTags> favoredApproaches { get; set; } = new List<ApproachTags>();
    public List<ApproachTags> disfavoredApproaches { get; set; } = new List<ApproachTags>();

    // New properties
    public List<FocusTags> favoredFocuses { get; set; } = new List<FocusTags>();
    public List<FocusTags> disfavoredFocuses { get; set; } = new List<FocusTags>();

    // Existing tag lists
    public List<NarrativeTag> encounterNarrativeTags { get; set; } = new List<NarrativeTag>();
    public List<StrategicTag> encounterStrategicTags { get; set; } = new List<StrategicTag>();
}