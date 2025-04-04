public class WorldEvolutionService
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionGenerator _actionGenerator;
    private readonly ActionRepository _actionRepository;

    public WorldEvolutionService(
        NarrativeService narrativeService,
        ActionGenerator actionGenerator,
        ActionRepository actionRepository)
    {
        _narrativeService = narrativeService;
        this._actionGenerator = actionGenerator;
        this._actionRepository = actionRepository;
    }

    public async Task<WorldEvolutionResponse> ProcessWorldEvolution(
        NarrativeContext context,
        WorldEvolutionInput input)
    {
        // Get world evolution response from narrative service
        WorldEvolutionResponse response = await _narrativeService.ProcessWorldEvolution(context, input);
        return response;
    }

    public async Task<string> ConsolidateMemory(
        NarrativeContext context,
        MemoryConsolidationInput input)
    {
        return await _narrativeService.ProcessMemoryConsolidation(context, input);
    }

    public async Task<Location> IntegrateWorldEvolution(
        WorldEvolutionResponse evolution,
        WorldState worldState,
        LocationSystem locationSystem,
        PlayerState playerState)
    {
        // Process coin change
        if (evolution.CoinChange != 0)
        {
            playerState.ModifyCoins(evolution.CoinChange);
        }

        // Process inventory changes
        ProcessInventoryChanges(evolution, playerState);

        // Process relationship changes
        ProcessRelationshipChanges(evolution, playerState);

        // Process new locations first (may be needed for player location change)
        foreach (Location location in evolution.NewLocations)
        {
            // Ensure each location has at least one spot
            if (location.Spots == null || !location.Spots.Any())
            {
                location.Spots = new List<LocationSpot>();
            }

            worldState.AddLocations(new List<Location> { location });
        }

        // Add new location spots to appropriate locations
        foreach (LocationSpot spot in evolution.NewLocationSpots)
        {
            string targetLocationName = !string.IsNullOrEmpty(spot.LocationName)
                ? spot.LocationName
                : worldState.CurrentLocation?.Name;

            if (!string.IsNullOrEmpty(targetLocationName))
            {
                locationSystem.AddSpot(targetLocationName, spot);
            }
        }

        // Process new actions
        await ProcessNewActions(evolution, worldState);

        // Add new characters
        worldState.AddCharacters(evolution.NewCharacters);

        // Add new opportunities
        worldState.AddOpportunities(evolution.NewOpportunities);

        // Process player location change (must be done last after all locations are set up)
        return ProcessPlayerLocationChange(evolution, worldState, playerState);
    }

    private async Task ProcessNewActions(WorldEvolutionResponse evolution, WorldState worldState)
    {
        foreach (NewAction newAction in evolution.NewActions)
        {
            Location targetLocation = worldState.GetLocation(newAction.LocationName);
            if (targetLocation != null && targetLocation.Spots != null)
            {
                LocationSpot spotForAction = targetLocation.Spots.FirstOrDefault(s =>
                    s.Name.Equals(newAction.SpotName, StringComparison.OrdinalIgnoreCase));

                if (spotForAction != null)
                {
                    if (spotForAction.ActionTemplates == null)
                        spotForAction.ActionTemplates = new List<string>();

                    // Create action template linked to the encounter
                    string actionTemplateName = await _actionGenerator.GenerateActionAndEncounter(
                        newAction.Name,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType).ToString(),
                        newAction.SpotName,
                        newAction.LocationName);

                    ActionTemplate actionTemplate = _actionRepository.GetAction(newAction.Name);
                    string encounterTemplateName = actionTemplate.EncounterTemplateName;

                    EncounterTemplate encounterTemplate = _actionRepository.GetEncounterTemplate(encounterTemplateName);
                    spotForAction.ActionTemplates.Add(actionTemplate.Name);
                }
            }
        }
    }

    // Helper methods for handling player state changes
    private void ProcessInventoryChanges(WorldEvolutionResponse evolution, PlayerState playerState)
    {
        if (evolution.ResourceChanges != null)
        {
            // Add items to inventory
            foreach (string itemName in evolution.ResourceChanges.ItemsAdded)
            {
                if (Enum.TryParse<ItemTypes>(itemName.Replace(" ", ""), true, out ItemTypes itemType))
                {
                    playerState.Inventory.AddItem(itemType);
                }
            }

            // Remove items from inventory
            foreach (string itemName in evolution.ResourceChanges.ItemsRemoved)
            {
                if (Enum.TryParse<ItemTypes>(itemName.Replace(" ", ""), true, out ItemTypes itemType))
                {
                    playerState.Inventory.RemoveItem(itemType);
                }
            }
        }
    }

    private void ProcessRelationshipChanges(WorldEvolutionResponse evolution, PlayerState playerState)
    {
        if (evolution.RelationshipChanges != null && evolution.RelationshipChanges.Any())
        {
            foreach (RelationshipChange relationshipChange in evolution.RelationshipChanges)
            {
                // Skip if character name is empty
                if (string.IsNullOrEmpty(relationshipChange.CharacterName))
                    continue;

                // Get current relationship level
                int currentLevel = playerState.Relationships.GetLevel(relationshipChange.CharacterName);

                // Apply the change
                int newLevel = currentLevel + relationshipChange.ChangeAmount;

                // Update relationship
                playerState.Relationships.SetLevel(relationshipChange.CharacterName, newLevel);
            }
        }
    }

    private Location ProcessPlayerLocationChange(
        WorldEvolutionResponse evolution,
        WorldState worldState,
        PlayerState playerState)
    {
        Location travelLocation = null;

        if (evolution.LocationUpdate.LocationChanged && !string.IsNullOrEmpty(evolution.LocationUpdate.NewLocationName))
        {
            // Find the location (should exist now after processing new locations)
            Location newPlayerLocation = worldState.GetLocation(evolution.LocationUpdate.NewLocationName);
            if (newPlayerLocation != null)
            {
                travelLocation = newPlayerLocation;
            }
            else
            {
                // If location doesn't exist, create a minimal one with a default spot and action
                Location newLocation = new Location
                {
                    Name = evolution.LocationUpdate.NewLocationName,
                    Description = $"A location the player traveled to during an encounter.",
                    Spots = new List<LocationSpot>(),
                    ConnectedTo = new List<string> { worldState.CurrentLocation?.Name ?? string.Empty }
                };
                worldState.AddLocations(new List<Location> { newLocation });
                travelLocation = newLocation;
            }
        }

        return travelLocation;
    }

    private BasicActionTypes ParseActionType(string actionTypeStr)
    {
        if (Enum.TryParse<BasicActionTypes>(actionTypeStr, true, out BasicActionTypes actionType))
        {
            return actionType;
        }

        // Default fallback
        return BasicActionTypes.Discuss;
    }
}
