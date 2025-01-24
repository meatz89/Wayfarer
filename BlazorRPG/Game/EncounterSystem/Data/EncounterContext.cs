public class EncounterContext
{
    public ActionImplementation ActionImplementation { get; }
    public LocationNames LocationName { get; }
    public string LocationSpotName { get; }
    public BasicActionTypes ActionType { get; }
    public LocationTypes LocationType { get; }
    public LocationArchetypes LocationArchetype { get; }
    public TimeSlots TimeSlot { get; }
    public LocationSpotProperties LocationProperties { get; }
    public PlayerState PlayerState { get; }
    public EncounterValues CurrentValues { get; set; }
    public int StageNumber { get; set; }
    public int LocationDifficulty { get; }
    public List<LocationPropertyChoiceEffect> LocationPropertyChoiceEffects { get; }

    // Constructor remains the same but without value initialization
    public EncounterContext(
        ActionImplementation actionImplementation,
        LocationNames locationName,
        string locationSpotName,
        BasicActionTypes actionType,
        LocationTypes locationType,
        LocationArchetypes locationArchetype,
        TimeSlots timeSlot,
        LocationSpotProperties locationProperties,
        PlayerState playerState,
        int locationDifficulty,
        List<LocationPropertyChoiceEffect> choiceEffects,
        int playerLevel,
        List<PlayerStatusTypes> playerStatusTypes
        )
    {
        ActionImplementation = actionImplementation;
        LocationName = locationName;
        LocationSpotName = locationSpotName;
        ActionType = actionType;
        LocationType = locationType;
        LocationArchetype = locationArchetype;
        TimeSlot = timeSlot;
        LocationProperties = locationProperties;
        PlayerState = playerState;
        LocationDifficulty = locationDifficulty;
        LocationPropertyChoiceEffects = choiceEffects;

        // Initialize encounter values
        EncounterStateInitializer encounterStateInitializer = new EncounterStateInitializer();
        StageNumber = 0;

        CurrentValues = EncounterStateInitializer.Generate(
                locationDifficulty,
                playerLevel,
                locationProperties,
                playerStatusTypes
                );
    }
}