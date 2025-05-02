public class PostEncounterEvolutionSystem
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionGenerator _actionGenerator;
    private readonly ActionRepository _actionRepository;
    private readonly GameState gameState;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly WorldStateInputBuilder worldStateInputCreator;
    private readonly LocationRepository locationRepository;

    public PostEncounterEvolutionSystem(
        NarrativeService narrativeService,
        ActionGenerator actionGenerator,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        WorldStateInputBuilder worldStateInputCreator,
        LocationRepository locationRepository,
        ActionRepository actionRepository,
        GameState gameState)
    {
        this._narrativeService = narrativeService;
        this._actionGenerator = actionGenerator;
        this._actionRepository = actionRepository;
        this.gameState = gameState;
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.actionSystem = actionSystem;
        this.worldStateInputCreator = worldStateInputCreator;
        this.locationRepository = locationRepository;
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

    // Update registry usage in IntegrateEncounterOutcome method
    public async Task IntegrateEncounterOutcome(
        PostEncounterEvolutionResult evolution,
        WorldState worldState,
        PlayerState playerState)
    {
        // Same logic but with updated registry calls
        foreach (Location loc in evolution.NewLocations)
        {
            string locationName = loc.Name;
            Location existingLoc = locationRepository.GetLocationById(locationName);
            if (existingLoc == null)
            {
                locationRepository.AddLocation(loc);
            }
            else
            {
                existingLoc.Description = loc.Description;
                existingLoc.DetailedDescription = loc.DetailedDescription;
                existingLoc.ConnectedTo = loc.ConnectedTo;
                existingLoc.Depth = loc.Depth;
            }
        }

        foreach (LocationSpot spot in evolution.NewLocationSpots)
        {
            string spotName = $"{spot.LocationId}:{spot.Name}";
            LocationSpot existingSpot = locationRepository.GetSpot(spot.LocationId, spot.Name);
            if (existingSpot == null)
            {
                locationRepository.AddLocationSpot(spot);
            }
            else
            {
                existingSpot.Description = spot.Description;
                existingSpot.PlayerKnowledge = spot.PlayerKnowledge;
            }
        }

        foreach (NewAction newAction in evolution.NewActions)
        {
            string actionId = await _actionGenerator.GenerateAction(
                newAction.Name,
                newAction.SpotName,
                newAction.LocationName);

            ActionDefinition actionDef = _actionRepository.GetAction(actionId);
            string spotId = $"{newAction.LocationName}:{newAction.SpotName}";
            LocationSpot spot = locationRepository.GetSpot(newAction.LocationName, newAction.SpotName);
            if (spot != null)
            {
                spot.RegisterActionDefinition(actionDef.Name);
            }
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
            Location targetLocation = locationRepository.GetLocationById(newAction.LocationName);
            List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(targetLocation.Name);
            if (targetLocation != null && locationSpots != null)
            {
                LocationSpot spotForAction = locationSpots.FirstOrDefault(s =>
                {
                    return s.Name.Equals(newAction.SpotName, StringComparison.OrdinalIgnoreCase);
                });

                if (spotForAction != null)
                {
                    // Create action template linked to the encounter
                    string actionId = await _actionGenerator.GenerateAction(
                        newAction.Name,
                        newAction.SpotName,
                        newAction.LocationName);

                    ActionDefinition actionTemplate = _actionRepository.GetAction(newAction.Name);
                    spotForAction.RegisterActionDefinition(actionId);

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
        return EncounterTypes.Persuasion;
    }

    public PostEncounterEvolutionInput PreparePostEncounterEvolutionInput(
        string encounterNarrative,
        string encounterOutcome)
    {
        WorldState worldState = gameState.WorldState;
        PlayerState playerState = gameState.PlayerState;

        // Get all locations
        List<Location> allLocations = locationRepository.GetAllLocations();

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

            CurrentDepth = worldState.CurrentLocation.Depth,

            Health = playerState.Health,
            MaxHealth = playerState.MaxHealth,
            Energy = playerState.Energy,
            MaxEnergy = playerState.MaxEnergy
        };
    }

}
