public class EncounterContext
{
    public BasicActionTypes ActionType { get; }
    public LocationTypes LocationType { get; }
    public LocationArchetypes LocationArchetype { get; }
    public TimeSlots TimeSlot { get; }
    public LocationProperties LocationProperties { get; }
    public PlayerState PlayerState { get; }
    public EncounterStateValues CurrentValues { get; set; } // Note: Now settable
    public int StageNumber { get; set; } // Note: Now settable
    public int LocationDifficulty { get; }
    public List<LocationPropertyChoiceEffect> LocationPropertyChoiceEffects { get; }

    // Constructor remains the same but without value initialization
    public EncounterContext(
        BasicActionTypes actionType,
        LocationTypes locationType,
        LocationArchetypes locationArchetype,
        TimeSlots timeSlot,
        LocationProperties locationProperties,
        PlayerState playerState,
        int locationDifficulty,
        List<LocationPropertyChoiceEffect> locationPropertyChoiceEffects,
        int playerLevel
        )
    {
        ActionType = actionType;
        LocationType = locationType;
        LocationArchetype = locationArchetype;
        TimeSlot = timeSlot;
        LocationProperties = locationProperties;
        PlayerState = playerState;
        LocationDifficulty = locationDifficulty;
        LocationPropertyChoiceEffects = locationPropertyChoiceEffects;

        // Initialize encounter values
        EncounterStateInitializer encounterStateInitializer = new EncounterStateInitializer();
        CurrentValues = EncounterStateInitializer.Generate(locationDifficulty, playerLevel);
        StageNumber = 0;
    }
}