public class ActionProcessor
{
    public GameWorld gameState { get; }
    public Player player { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public LocationPropertyManager environmentalPropertyManager { get; }
    public MessageSystem messageSystem { get; }
    private LocationRepository locationRepository { get; }

    public ActionProcessor(
        GameWorld gameState,
        PlayerProgression playerProgression,
        LocationPropertyManager environmentalPropertyManager,
        LocationRepository locationRepository,
        MessageSystem messageSystem)
    {
        this.gameState = gameState;
        this.playerProgression = playerProgression;
        this.environmentalPropertyManager = environmentalPropertyManager;
        this.locationRepository = locationRepository;
        this.messageSystem = messageSystem;
        this.player = gameState.Player;
        this.worldState = gameState.WorldState;
    }

    public void ProcessTurnChange()
    {
        Player playerState = gameState.Player;

        int energy = playerState.CurrentEnergy();
        int turnAp = playerState.MaxActionPoints;

        int newEnergy = energy - turnAp;
        if (newEnergy >= 0)
        {
            playerState.SetNewEnergy(newEnergy);
        }

        gameState.TimeManager.StartNewDay();
        gameState.Player.ModifyActionPoints(gameState.Player.MaxActionPoints);
    }

    public void ProcessAction(LocationAction action)
    {
        Player playerState = gameState.Player;
        playerState.ApplyActionPointCost(action.ActionPointCost);
    }

    public void UpdateState()
    {
        Location currentLocation = worldState.CurrentLocation;
        List<Location> allLocs = locationRepository.GetAllLocations();

        foreach (Location loc in allLocs)
        {
            environmentalPropertyManager.UpdateLocationForTime(loc, worldState.CurrentTimeWindow);
        }
    }

    public bool CanExecute(LocationAction action)
    {
        foreach (IRequirement requirement in action.Requirements)
        {
            if (!requirement.IsMet(gameState))
            {
                return false; // Requirement not met
            }
        }

        if (action.ActionExecutionType == ActionExecutionTypes.Encounter)
        {
            string encounterId = action.ActionId;
            if (gameState.WorldState.IsEncounterCompleted(encounterId))
            {
                return false; // EncounterContext already completed
            }
        }
        return true; // All requirements are met
    }

}