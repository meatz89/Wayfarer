public class EncounterTemplate
{
    public string ActionId;
    public string Name;

    public int MaxPressure = 13;
    public int Duration = 7;
    public int PartialThreshold = 12;
    public int StandardThreshold = 16;
    public int ExceptionalThreshold = 20;

    public EncounterInfo.HostilityLevels Hostility = EncounterInfo.HostilityLevels.Neutral;

    public List<NarrativeTag> EncounterNarrativeTags { get; set; } = new List<NarrativeTag>();
    public List<EnvironmentPropertyTag> encounterStrategicTags { get; set; } = new List<EnvironmentPropertyTag>();
}