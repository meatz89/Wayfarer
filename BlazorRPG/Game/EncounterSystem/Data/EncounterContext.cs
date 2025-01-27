public class EncounterContext
{
    public GameState GameState { get; }
    public ActionImplementation ActionImplementation { get; }
    public Location Location { get; }
    public LocationSpot LocationSpot { get; }
    public BasicActionTypes ActionType { get; }
    public PlayerState PlayerState { get; }
    public EncounterValues CurrentValues { get; set; }
    public int StageNumber { get; set; }
    public LocationSpotProperties Properties { get; set; }
    public List<LocationPropertyChoiceEffect> LocationPropertyChoiceEffects { get; }

    // Constructor remains the same but without value initialization
    public EncounterContext(
        ActionImplementation actionImplementation,
        Location location,
        LocationSpot locationSpot,
        BasicActionTypes actionType,
        PlayerState playerState,
        List<LocationPropertyChoiceEffect> choiceEffects,
        int playerLevel,
        GameState gameState
        )
    {
        ActionImplementation = actionImplementation;
        Location = location;
        LocationSpot = locationSpot;
        ActionType = actionType;
        PlayerState = playerState;
        LocationPropertyChoiceEffects = choiceEffects;
        GameState = gameState;

        // Initialize encounter values
        EncounterStateInitializer encounterStateInitializer = new EncounterStateInitializer();
        StageNumber = 0;

        CurrentValues = EncounterStateInitializer.Generate(
                location,
                locationSpot,
                gameState,
                playerLevel
                );
    }
}