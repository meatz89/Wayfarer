public class ActionProcessor
{
    public GameWorld gameWorld { get; }
    public Player player { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public LocationPropertyManager environmentalPropertyManager { get; }
    public MessageSystem messageSystem { get; }
    private LocationRepository locationRepository { get; }
    private RouteUnlockManager routeUnlockManager { get; }
    private NPCLetterOfferService npcLetterOfferService { get; }

    public ActionProcessor(
        GameWorld gameWorld,
        PlayerProgression playerProgression,
        LocationPropertyManager environmentalPropertyManager,
        LocationRepository locationRepository,
        MessageSystem messageSystem,
        RouteUnlockManager routeUnlockManager,
        NPCLetterOfferService npcLetterOfferService)
    {
        this.gameWorld = gameWorld;
        this.playerProgression = playerProgression;
        this.environmentalPropertyManager = environmentalPropertyManager;
        this.locationRepository = locationRepository;
        this.messageSystem = messageSystem;
        this.routeUnlockManager = routeUnlockManager;
        this.npcLetterOfferService = npcLetterOfferService;
        this.player = gameWorld.GetPlayer();
        this.worldState = gameWorld.WorldState;
    }

    public void ProcessTurnChange()
    {
        Player player = gameWorld.GetPlayer();

        // Start new day through TimeManager
        gameWorld.TimeManager.StartNewDay();
    }

    public void ProcessAction(LocationAction action)
    {
        Player player = gameWorld.GetPlayer();

        // ActionPoints system removed - using time blocks and stamina instead

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

        // Time block consumption is handled by the action system based on action type

        // Apply refresh card effects
        if (action.RefreshCardType != SkillCategories.None)
        {
            ApplyCardRefresh(action.RefreshCardType);
        }

        // Apply categorical effects
        ApplyCategoricalEffects(action.Effects);
        
        // Handle route unlock actions
        if (action.ActionId.StartsWith("unlock_") && action.ActionId.Contains("route"))
        {
            HandleRouteUnlockAction(action);
        }
        

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

        // Time block validation is handled by the action system

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
        // Card refresh implementation: restore cards of the specified skill category
        // This provides strategic resource management through card availability
        if (cardType == SkillCategories.None)
        {
            return; // No cards to refresh
        }

        // Find cards of the specified category in player's collection
        List<SkillCard> playerCards = player.PlayerSkillCards?.Where(card => card.Category == cardType).ToList() ?? new List<SkillCard>();
        
        int refreshedCount = 0;
        foreach (SkillCard card in playerCards)
        {
            if (card.IsExhausted)
            {
                card.Refresh();
                refreshedCount++;
            }
        }

        if (refreshedCount > 0)
        {
            messageSystem.AddSystemMessage($"Refreshed {refreshedCount} {cardType} cards");
        }
        else
        {
            messageSystem.AddSystemMessage($"No {cardType} cards needed refreshing");
        }
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
    
    private void HandleRouteUnlockAction(LocationAction action)
    {
        // Get current location to find available route unlocks
        string currentLocationId = locationRepository.GetCurrentLocation()?.Id;
        if (string.IsNullOrEmpty(currentLocationId))
        {
            messageSystem.AddSystemMessage("Cannot unlock routes - location not found.", SystemMessageTypes.Danger);
            return;
        }
        
        // Get available route unlocks at this location
        var availableUnlocks = routeUnlockManager.GetAvailableRouteUnlocks(currentLocationId);
        
        if (!availableUnlocks.Any())
        {
            messageSystem.AddSystemMessage("No route unlocks available at this location.", SystemMessageTypes.Info);
            return;
        }
        
        // Show all available route unlocks with detailed information
        messageSystem.AddSystemMessage("🗺️ Available Route Unlocks:", SystemMessageTypes.Info);
        
        foreach (var unlock in availableUnlocks)
        {
            string affordabilityText = unlock.CanAfford ? "✓ Affordable" : "✗ Cannot afford";
            messageSystem.AddSystemMessage($"📍 {unlock.RouteName}: {unlock.TokenCost.Amount} {unlock.TokenCost.TokenType} tokens ({affordabilityText})", SystemMessageTypes.Info);
            messageSystem.AddSystemMessage($"   {unlock.RouteDescription}", SystemMessageTypes.Info);
        }
        
        // Auto-unlock the first affordable route for demonstration
        var firstAffordableUnlock = availableUnlocks.FirstOrDefault(u => u.CanAfford);
        if (firstAffordableUnlock != null)
        {
            bool success = routeUnlockManager.TryUnlockRoute(firstAffordableUnlock.RouteId, firstAffordableUnlock.UnlockingNPC.ID);
            if (success)
            {
                messageSystem.AddSystemMessage($"Successfully unlocked {firstAffordableUnlock.RouteName}!", SystemMessageTypes.Success);
            }
        }
        else
        {
            messageSystem.AddSystemMessage("💼 Build more connections to unlock routes.", SystemMessageTypes.Info);
            if (availableUnlocks.Any())
            {
                var cheapestUnlock = availableUnlocks.OrderBy(u => u.TokenCost.Amount).First();
                messageSystem.AddSystemMessage($"💡 Cheapest unlock: {cheapestUnlock.RouteName} ({cheapestUnlock.TokenCost.Amount} {cheapestUnlock.TokenCost.TokenType} tokens)", SystemMessageTypes.Info);
            }
        }
    }
    

}