public class PostEncounterEvolutionSystem
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionGenerator _actionGenerator;
    private readonly ActionRepository _actionRepository;

    public PostEncounterEvolutionSystem(
        NarrativeService narrativeService,
        ActionGenerator actionGenerator,
        ActionRepository actionRepository)
    {
        _narrativeService = narrativeService;
        this._actionGenerator = actionGenerator;
        this._actionRepository = actionRepository;
    }

    public async Task<string> ConsolidateMemory(
        NarrativeContext context,
        MemoryConsolidationInput input)
    {
        return await _narrativeService.ProcessMemoryConsolidation(context, input);
    }

    public async Task<EvolutionResult> ProcessEncounterOutcome(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        EncounterResult encounterResult)
    {
        // Get world evolution response from narrative service
        EvolutionResult response = await _narrativeService.ProcessPostEncounterEvolution(context, input);
        return response;
    }

    public async Task<Location> IntegrateEncounterOutcome(
    EvolutionResult evolution,
    WorldState worldState,
    LocationSystem locationSystem,
    PlayerState playerState)
    {
        // Process coin change
        if (evolution.CoinChange != 0)
        {
            playerState.AddCoins(evolution.CoinChange);
        }

        // Process inventory changes
        ProcessInventoryChanges(evolution, playerState);

        // Process relationship changes
        ProcessRelationshipChanges(evolution, playerState);

        Location travelLocation = null;

        // First, process new locations so they exist before processing spots
        foreach (Location location in evolution.NewLocations)
        {
            // Check if this location might be a travel destination
            bool isTravelDestination = evolution.LocationUpdate?.LocationChanged == true &&
                                      evolution.LocationUpdate?.NewLocationName == location.Name;

            // Store reference if this is a travel destination
            if (isTravelDestination)
            {
                travelLocation = location;
            }

            // Verify location has spots collection (shouldn't be needed with proper prompting)
            if (location.Spots == null)
            {
                location.Spots = new List<LocationSpot>();
            }

            // Add location to world state
            worldState.AddLocation(location);

            // Set location depth
            worldState.SetLocationDepth(location.Name, location.Depth);

            // Update hub tracking if applicable
            worldState.UpdateHubTracking(location);
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

                // If this spot belongs to our travel destination, update reference
                if (travelLocation != null && targetLocationName == travelLocation.Name)
                {
                    // No need to add the spot again as it's already added above,
                    // but we ensure our travelLocation reference is up to date
                }
            }
        }

        // Process new actions and associate them with the appropriate spots
        await ProcessNewActions(evolution, worldState);

        // Add new characters
        foreach (Character character in evolution.NewCharacters)
        {
            worldState.AddCharacter(character);
        }

        // Add new opportunities
        foreach (Opportunity opportunity in evolution.NewOpportunities)
        {
            worldState.AddOpportunity(opportunity);
        }

        // Process player location change
        if (evolution.LocationUpdate?.LocationChanged == true &&
            !string.IsNullOrEmpty(evolution.LocationUpdate.NewLocationName))
        {
            // Get the location to move to (either new or existing)
            string targetLocationName = evolution.LocationUpdate.NewLocationName;
            Location newPlayerLocation = worldState.GetLocation(targetLocationName);

            if (newPlayerLocation != null)
            {
                travelLocation = newPlayerLocation;

                // Log for debugging purposes
                Console.WriteLine($"Player moved to location: {newPlayerLocation.Name}");

                // Verify this location has spots (shouldn't be needed with proper prompting)
                if (newPlayerLocation.Spots == null || !newPlayerLocation.Spots.Any())
                {
                    // This should never happen with proper prompting
                    Console.WriteLine($"WARNING: Travel location {newPlayerLocation.Name} has no spots! Check world evolution prompt.");
                }
            }
            else
            {
                // This should never happen with proper prompting
                Console.WriteLine($"ERROR: Travel location {targetLocationName} was not created or doesn't exist!");
            }
        }

        return travelLocation;
    }

    private async Task ProcessNewActions(EvolutionResult evolution, WorldState worldState)
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
                    {
                        spotForAction.ActionTemplates = new List<string>();
                    }

                    // Create action template linked to the encounter
                    string actionTemplateName = await _actionGenerator.GenerateActionAndEncounter(
                        newAction.Name,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType).ToString(),
                        newAction.SpotName,
                        newAction.LocationName);

                    SpotAction actionTemplate = _actionRepository.GetAction(newAction.Name);
                    string encounterTemplateName = actionTemplate.EncounterTemplateName;

                    EncounterTemplate encounterTemplate = _actionRepository.GetEncounterTemplate(encounterTemplateName);
                    spotForAction.ActionTemplates.Add(actionTemplate.Name);

                    Console.WriteLine($"Created new action {newAction.Name} at {newAction.LocationName}/{newAction.SpotName}");
                }
                else
                {
                    Console.WriteLine($"WARNING: Could not find spot {newAction.SpotName} at location {newAction.LocationName} for action {newAction.Name}");
                }
            }
            else
            {
                Console.WriteLine($"WARNING: Could not find location {newAction.LocationName} for action {newAction.Name}");
            }
        }
    }

    // Helper methods for handling player state changes
    private void ProcessInventoryChanges(EvolutionResult evolution, PlayerState playerState)
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

    private void ProcessRelationshipChanges(EvolutionResult evolution, PlayerState playerState)
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

                Console.WriteLine($"Updated relationship with {relationshipChange.CharacterName}: {currentLevel} -> {newLevel} ({relationshipChange.ChangeAmount:+0;-0})");
            }
        }
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
