public class Narrative
{
    public ActionTypes ActionType { get; set; }
    public LocationTypes LocationType { get; set; }
    public LocationNames LocationName { get; set; }
    public LocationSpotNames LocationSpot { get; set; }
    public CharacterNames NarrativeCharacter { get; set; }
    public CharacterRoles NarrativeCharacterRole { get; set; }
    public TimeSlots TimeSlot { get; set; }
    public string Situation { get; set; }

    public NarrativeState InitialState { get; set; }
    public List<NarrativeStage> Stages { get; set; } = new();

    public int currentStage = 0;
    public int numberOfStages => Stages.Count();
}