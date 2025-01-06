public class Narrative
{
    public BasicActionTypes ActionType { get; set; }
    public LocationTypes LocationType { get; set; }
    public LocationNames LocationName { get; set; }
    public CharacterNames NarrativeCharacter { get; set; }
    public CharacterRoles NarrativeCharacterRole { get; set; }
    public TimeSlots TimeSlot { get; set; }
    public string Situation { get; set; }
    public List<NarrativeStage> Stages { get; set; } = new();

    public NarrativeStateValues InitialState { get; set; }
    public int ActionCompletion = 0;

    public int currentStage = 0;
    public int numberOfStages => Stages.Count();
}