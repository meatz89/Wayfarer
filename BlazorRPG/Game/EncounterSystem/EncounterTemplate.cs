public class EncounterTemplate
{
    public string ActionId;
    public string Name;

    public int MaxPressure = 13;
    public int Duration = 7;
    public int PartialThreshold = 12;
    public int StandardThreshold = 16;
    public int ExceptionalThreshold = 20;

    public Encounter.HostilityLevels Hostility = Encounter.HostilityLevels.Neutral;

    public List<NarrativeTag> EncounterNarrativeTags { get; set; } = new List<NarrativeTag>();
    public List<StrategicTag> EncounterStrategicTags { get; set; } = new List<StrategicTag>();
}