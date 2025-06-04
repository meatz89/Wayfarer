public class ActionProcessor
{
    public GameWorld gameWorld { get; }
    public Player player { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public LocationPropertyManager environmentalPropertyManager { get; }
    public MessageSystem messageSystem { get; }
    private LocationRepository locationRepository { get; }

    public ActionProcessor(
        GameWorld gameWorld,
        PlayerProgression playerProgression,
        LocationPropertyManager environmentalPropertyManager,
        LocationRepository locationRepository,
        MessageSystem messageSystem)
    {
        this.gameWorld = gameWorld;
        this.playerProgression = playerProgression;
        this.environmentalPropertyManager = environmentalPropertyManager;
        this.locationRepository = locationRepository;
        this.messageSystem = messageSystem;
        this.player = gameWorld.Player;
        this.worldState = gameWorld.WorldState;
    }

    public void ProcessTurnChange()
    {
        Player player = gameWorld.Player;

        int energy = player.CurrentEnergy();
        int turnAp = player.MaxActionPoints;

        int newEnergy = energy - turnAp;
        if (newEnergy >= 0)
        {
            player.SetNewEnergy(newEnergy);
        }

        gameWorld.TimeManager.StartNewDay();
        gameWorld.Player.ModifyActionPoints(gameWorld.Player.MaxActionPoints);
    }

    public void ProcessAction(LocationAction action)
    {
        Player player = gameWorld.Player;
        player.ApplyActionPointCost(action.ActionPointCost);
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
            if (!requirement.IsMet(gameWorld))
            {
                return false; // Requirement not met
            }
        }

        if (action.ActionExecutionType == ActionExecutionTypes.Encounter)
        {
            string encounterId = action.ActionId;
            if (gameWorld.WorldState.IsEncounterCompleted(encounterId))
            {
                return false; // EncounterContext already completed
            }
        }
        return true; // All requirements are met
    }

}