public class EncounterTemplate
{
    public int Duration = 6;
    public int PartialThreshold = 12;
    public int StandardThreshold = 16;
    public int ExceptionalThreshold = 20;

    public EncounterInfo.HostilityLevels hostility = EncounterInfo.HostilityLevels.Neutral;

    public List<ApproachTags> favoredApproaches;
    public List<ApproachTags> disfavoredApproaches;
    public List<NarrativeTag> encounterNarrativeTags;
    public List<StrategicTag> encounterStrategicTags;
}