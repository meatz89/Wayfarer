using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;

public class ActionProcessor
{
    public GameWorld gameWorld { get; }
    public Player player { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public LocationPropertyManager environmentalPropertyManager { get; }
    public MessageSystem messageSystem { get; }
    private LocationRepository locationRepository { get; }
    private ContractProgressionService contractProgressionService { get; }

    public ActionProcessor(
        GameWorld gameWorld,
        PlayerProgression playerProgression,
        LocationPropertyManager environmentalPropertyManager,
        LocationRepository locationRepository,
        MessageSystem messageSystem,
        ContractProgressionService contractProgressionService)
    {
        this.gameWorld = gameWorld;
        this.playerProgression = playerProgression;
        this.environmentalPropertyManager = environmentalPropertyManager;
        this.locationRepository = locationRepository;
        this.messageSystem = messageSystem;
        this.contractProgressionService = contractProgressionService;
        this.player = gameWorld.GetPlayer();
        this.worldState = gameWorld.WorldState;
    }

    public void ProcessTurnChange()
    {
        Player player = gameWorld.GetPlayer();

        int stamina = player.Stamina;
        int turnAp = player.MaxActionPoints;

        int newStamina = stamina - turnAp;
        if (newStamina >= 0)
        {
            player.SetNewStamina(newStamina);
        }

        gameWorld.TimeManager.StartNewDay();
        gameWorld.GetPlayer().ModifyActionPoints(gameWorld.GetPlayer().MaxActionPoints);
    }

    public void ProcessAction(LocationAction action)
    {
        Player player = gameWorld.GetPlayer();

        // Apply action point cost
        player.ApplyActionPointCost(action.ActionPointCost);

        // Apply resource costs
        if (action.SilverCost > 0)
        {
            player.ModifyCoins(-action.SilverCost);
        }

        if (action.StaminaCost > 0)
        {
            player.SpendStamina(action.StaminaCost);
        }

        // Apply categorical stamina costs based on physical demand
        if (action.PhysicalDemand != PhysicalDemand.None)
        {
            player.ApplyCategoricalStaminaCost(action.PhysicalDemand);
        }

        if (action.ConcentrationCost > 0)
        {
            player.ModifyConcentration(-action.ConcentrationCost);
        }

        // Consume time blocks based on action point cost
        if (action.ActionPointCost > 0)
        {
            gameWorld.TimeManager.ConsumeTimeBlock(action.ActionPointCost);
        }

        // Apply refresh card effects
        if (action.RefreshCardType != SkillCategories.None)
        {
            ApplyCardRefresh(action.RefreshCardType);
        }

        // Apply categorical effects
        ApplyCategoricalEffects(action.Effects);

        // Check for contract progression
        contractProgressionService.CheckLocationActionProgression(action, player);
    }

    public void UpdateState()
    {
        Location currentLocation = worldState.CurrentLocation;
        List<Location> allLocs = locationRepository.GetAllLocations();

        foreach (Location loc in allLocs)
        {
            environmentalPropertyManager.UpdateLocationForTime(loc, worldState.CurrentTimeBlock);
        }
    }

    public bool CanExecute(LocationAction action)
    {
        Player player = gameWorld.GetPlayer();

        // Check basic requirements
        foreach (IRequirement requirement in action.Requirements)
        {
            if (!requirement.IsMet(gameWorld))
            {
                return false; // Requirement not met
            }
        }

        // Check resource requirements
        if (action.SilverCost > 0 && player.Coins < action.SilverCost)
        {
            return false; // Insufficient coins
        }

        if (action.StaminaCost > 0 && player.Stamina < action.StaminaCost)
        {
            return false; // Insufficient stamina
        }

        if (action.ConcentrationCost > 0 && player.Concentration < action.ConcentrationCost)
        {
            return false; // Insufficient concentration
        }

        // Check time block availability
        if (action.ActionPointCost > 0 && !gameWorld.TimeManager.ValidateTimeBlockAction(action.ActionPointCost))
        {
            return false; // Insufficient time blocks
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

    private void ApplyCardRefresh(SkillCategories cardType)
    {
        // TODO: Implement card refresh logic based on refresh card type
        // This would refresh cards of the specified skill category
        messageSystem.AddSystemMessage($"Refreshed {cardType} cards");
    }

    private void ApplyCategoricalEffects(List<IMechanicalEffect> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        // Create encounter state for effect application
        EncounterState encounterState = new EncounterState(player, 5, 8, 10);

        // Apply each categorical effect
        foreach (IMechanicalEffect effect in effects)
        {
            try
            {
                effect.Apply(encounterState);
            }
            catch (Exception ex)
            {
                // Log error but don't break action processing
                messageSystem.AddSystemMessage($"Error applying effect: {ex.Message}");
            }
        }
    }

}