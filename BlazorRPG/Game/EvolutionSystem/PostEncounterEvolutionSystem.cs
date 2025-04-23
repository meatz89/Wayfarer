public class PostEncounterEvolutionSystem
{
    private readonly ContentRegistry contentRegistry;
    private readonly NarrativeService _narrativeService;
    private readonly ActionGenerator _actionGenerator;
    private readonly ActionRepository _actionRepository;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly WorldStateInputBuilder worldStateInputCreator;
    private readonly GameState gameState;

    public PostEncounterEvolutionSystem(
        ContentRegistry contentRegistry,
        NarrativeService narrativeService,
        ActionGenerator actionGenerator,
        ActionRepository actionRepository,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        WorldStateInputBuilder worldStateInputCreator,
        GameState gameState)
    {
        this.contentRegistry = contentRegistry;
        _narrativeService = narrativeService;
        this._actionGenerator = actionGenerator;
        this._actionRepository = actionRepository;
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.actionSystem = actionSystem;
        this.worldStateInputCreator = worldStateInputCreator;
        this.gameState = gameState;
    }

    public async Task<string> ConsolidateMemory(
        NarrativeContext context,
        MemoryConsolidationInput input)
    {
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(context.LocationName);
        return await _narrativeService.ProcessMemoryConsolidation(context, input, worldStateInput);
    }

    public async Task<PostEncounterEvolutionResult> ProcessEncounterOutcome(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        EncounterResult encounterResult)
    {
        // Get world evolution response from narrative service
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(context.LocationName);
        PostEncounterEvolutionResult response = await _narrativeService.ProcessPostEncounterEvolution(context, input, worldStateInput);
        return response;
    }

    public async Task IntegrateEncounterOutcome(
        PostEncounterEvolutionResult evolution,
        WorldState worldState,
        PlayerState playerState)
    {
        if (evolution.CoinChange != 0)
            playerState.AddCoins(evolution.CoinChange);

        foreach (object invChange in evolution.InventoryChanges)
            playerState.Inventory.Apply(invChange);

        foreach (RelationshipChange relChange in evolution.RelationshipChanges)
            playerState.UpdateRelationship(relChange.CharacterName, relChange.ChangeAmount);

        foreach (Location loc in evolution.NewLocations)
        {
            string locId = loc.Name;
            if (!contentRegistry.TryResolve<Location>(locId, out Location? existingLoc))
            {
                contentRegistry.Register<Location>(locId, loc);
                worldState.AddLocation(loc);
            }
            else
            {
                existingLoc.Description = loc.Description;
                existingLoc.DetailedDescription = loc.DetailedDescription;
                existingLoc.ConnectedTo = loc.ConnectedTo;
                existingLoc.Depth = loc.Depth;
            }
            worldState.SetLocationDepth(locId, loc.Depth);
            worldState.UpdateHubTracking(loc);
        }

        foreach (LocationSpot spot in evolution.NewLocationSpots)
        {
            string spotId = $"{spot.LocationName}:{spot.Name}";
            if (!contentRegistry.TryResolve<LocationSpot>(spotId, out LocationSpot? existingSpot))
            {
                contentRegistry.Register<LocationSpot>(spotId, spot);
            }
            else
            {
                existingSpot.Description = spot.Description;
                existingSpot.BaseActionIds = spot.BaseActionIds;
                existingSpot.PlayerKnowledge = spot.PlayerKnowledge;
            }
        }

        foreach (NewAction newAction in evolution.NewActions)
        {
            string actionId = await _actionGenerator.GenerateActionAndEncounter(
                newAction.Name,
                newAction.SpotName,
                newAction.LocationName,
                newAction.Goal,
                newAction.Complication,
                newAction.ActionType);
            ActionDefinition actionDef = _actionRepository.GetAction(actionId);
            string spotId = $"{newAction.LocationName}:{newAction.SpotName}";
            if (contentRegistry.TryResolve<LocationSpot>(spotId, out LocationSpot? spot))
                spot.BaseActionIds.Add(actionDef.Id);
        }

        foreach (Character character in evolution.NewCharacters)
            worldState.AddCharacter(character);

        foreach (Opportunity opp in evolution.NewOpportunities)
            worldState.AddOpportunity(opp);
    }

    private async Task ProcessNewActions(
        PostEncounterEvolutionResult evolution,
        WorldState worldState)
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
                    if (spotForAction.BaseActionIds == null)
                    {
                        spotForAction.BaseActionIds = new List<string>();
                    }

                    // Create action template linked to the encounter
                    string actionId = await _actionGenerator.GenerateActionAndEncounter(
                        newAction.Name,
                        newAction.SpotName,
                        newAction.LocationName,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType).ToString());

                    ActionDefinition actionTemplate = _actionRepository.GetAction(newAction.Name);
                    spotForAction.BaseActionIds.Add(actionId);

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
                    playerState.Inventory.AddItem(itemType.ToString());
                }
                else
                {
                    playerState.Inventory.AddItem(itemName);
                }
            }

            // Remove items from inventory
            foreach (string itemName in evolution.ResourceChanges.ItemsRemoved)
            {
                if (Enum.TryParse<ItemTypes>(itemName.Replace(" ", ""), true, out ItemTypes itemType))
                {
                    playerState.Inventory.RemoveItem(itemType.ToString());
                }
                else
                {
                    playerState.Inventory.RemoveItem(itemName);

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

    private EncounterTypes ParseActionType(string actionTypeStr)
    {
        if (Enum.TryParse<EncounterTypes>(actionTypeStr, true, out EncounterTypes actionType))
        {
            return actionType;
        }

        // Default fallback
        return EncounterTypes.Exploration;
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

            KnownLocations = locationSystem.FormatLocations(allLocations),
            KnownCharacters = characterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = opportunitySystem.FormatActiveOpportunities(worldState.GetOpportunities()),

            CurrentLocationSpots = locationSystem.FormatLocationSpots(worldState.CurrentLocation),
            ConnectedLocations = locationSystem.FormatLocations(locationSystem.GetConnectedLocations(worldState.CurrentLocation.Name)),
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
