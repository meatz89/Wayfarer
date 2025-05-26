public class ActionProcessor
{
    private readonly LocationRepository locationRepository;

    public GameState gameState { get; }
    public Player glayerState { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public LocationPropertyManager environmentalPropertyManager { get; }
    public MessageSystem messageSystem { get; }

    public ActionProcessor(
        GameState gameState,
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
        glayerState = gameState.PlayerState;
        worldState = gameState.WorldState;
    }

    public void ProcessTurnChange()
    {
        Player playerState = gameState.PlayerState;

        int energy = playerState.CurrentEnergy();
        int turnAp = playerState.MaxActionPoints;

        int newEnergy = energy - turnAp;
        if (newEnergy >= 0)
        {
            playerState.SetNewEnergy(newEnergy);
        }

        gameState.TimeManager.StartNewDay();
        gameState.PlayerState.ModifyActionPoints(gameState.PlayerState.MaxActionPoints);
    }

    public void ProcessAction(ActionImplementation action)
    {
        Player playerState = gameState.PlayerState;
        playerState.ApplyActionPointCost(action.ActionPointCost);
    }

    private int CalculateBasicActionSkillXP(ActionImplementation action)
    {
        return action.Difficulty * 5;
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

    public bool CanExecute(ActionImplementation action)
    {
        foreach (IRequirement requirement in action.Requirements)
        {
            if (!requirement.IsMet(gameState))
            {
                return false; // Requirement not met
            }
        }

        // Check if the action has been completed and is non-repeatable
        if (action.ActionType == ActionExecutionTypes.Encounter)
        {
            string encounterId = action.Id;
            if (gameState.WorldState.IsEncounterCompleted(encounterId))
            {
                return false; // Encounter already completed
            }
        }
        return true; // All requirements are met
    }

}