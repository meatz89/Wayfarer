public class PostEncounterEvolutionSystem
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionGenerator _actionGenerator;
    private readonly ActionRepository _actionRepository;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly GameState gameState;

    public PostEncounterEvolutionSystem(
        NarrativeService narrativeService,
        ActionGenerator actionGenerator,
        ActionRepository actionRepository,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        GameState gameState)
    {
        _narrativeService = narrativeService;
        this._actionGenerator = actionGenerator;
        this._actionRepository = actionRepository;
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.actionSystem = actionSystem;
        this.gameState = gameState;
    }

    public async Task<string> ConsolidateMemory(
        NarrativeContext context,
        MemoryConsolidationInput input)
    {
        return await _narrativeService.ProcessMemoryConsolidation(context, input);
    }

    public async Task<PostEncounterEvolutionResult> ProcessEncounterOutcome(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        EncounterResult encounterResult)
    {
        // Get world evolution response from narrative service
        PostEncounterEvolutionResult response = await _narrativeService.ProcessPostEncounterEvolution(context, input);
        return response;
    }

    public async Task IntegrateEncounterOutcome(
        PostEncounterEvolutionResult evolution,
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

        // First, process new locations so they exist before processing spots
        foreach (Location location in evolution.NewLocations)
        {
            // Verify location has spots collection (shouldn't be needed with proper prompting)
            if (location.LocationSpots == null)
            {
                location.LocationSpots = new List<LocationSpot>();
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
    }

    private async Task ProcessNewActions(PostEncounterEvolutionResult evolution, WorldState worldState)
    {
        foreach (NewAction newAction in evolution.NewActions)
        {
            Location targetLocation = worldState.GetLocation(newAction.LocationName);
            if (targetLocation != null && targetLocation.LocationSpots != null)
            {
                LocationSpot spotForAction = targetLocation.LocationSpots.FirstOrDefault(s =>
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
    private void ProcessInventoryChanges(PostEncounterEvolutionResult evolution, PlayerState playerState)
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

    private void ProcessRelationshipChanges(PostEncounterEvolutionResult evolution, PlayerState playerState)
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

    public PostEncounterEvolutionInput PreparePostEncounterEvolutionInput(
        string encounterNarrative,
        string encounterOutcome)
    {
        WorldState worldState = gameState.WorldState;
        PlayerState playerState = gameState.PlayerState;

        // Get current depth and hub depth
        int currentDepth = worldState.GetLocationDepth(worldState.CurrentLocation?.Name ?? "");

        // Get all locations
        List<Location> allLocations = worldState.GetLocations();

        return new PostEncounterEvolutionInput
        {
            EncounterNarrative = encounterNarrative,
            CharacterBackground = playerState.Archetype.ToString(),
            CurrentLocation = worldState.CurrentLocation?.Name ?? "Unknown",
            EncounterOutcome = encounterOutcome,

            KnownLocations = locationSystem.FormatKnownLocations(allLocations),
            KnownCharacters = characterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = opportunitySystem.FormatActiveOpportunities(worldState.GetOpportunities()),

            CurrentLocationSpots = locationSystem.FormatLocationSpots(worldState.CurrentLocation),
            AllKnownLocationSpots = locationSystem.FormatAllLocationSpots(allLocations),
            AllExistingActions = actionSystem.FormatExistingActions(allLocations),

            CurrentDepth = currentDepth,
            LastHubDepth = worldState.LastHubDepth,

            Health = playerState.Health,
            MaxHealth = playerState.MaxHealth,
            Energy = playerState.Energy,
            MaxEnergy = playerState.MaxEnergy
        };
    }

}
