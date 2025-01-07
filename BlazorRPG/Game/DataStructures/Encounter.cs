public class Encounter
{
    public BasicActionTypes ActionType { get; set; }
    public LocationTypes LocationType { get; set; }
    public LocationArchetype LocationArchetype { get; set; }
    public LocationNames LocationName { get; set; }
    public CharacterNames EncounterCharacter { get; set; }
    public CharacterRoleTypes EncounterCharacterRole { get; set; }
    public TimeSlots? TimeSlot { get; set; }
    public string Situation { get; set; }
    public List<EncounterStage> Stages { get; set; } = new();
    public EncounterStateValues InitialState { get; set; } = new(5, 0, 5, 0);
    public int EncounterDifficulty { get; set; }
    public int currentStage = 0;
    public int numberOfStages => Stages.Count();
}