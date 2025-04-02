public class WorldEvolutionService
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionRepository _actionRepository;
    private readonly ActionGenerator _actionGenerator; 

    public WorldEvolutionService(
        NarrativeService narrativeService,
        ActionRepository actionRepository,
        ActionGenerator actionGenerator) 
    {
        _narrativeService = narrativeService;
        _actionRepository = actionRepository;
        _actionGenerator = actionGenerator;
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

    public async Task IntegrateWorldEvolution(
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

        // Process new actions using ActionGenerator
        await ProcessNewActions(evolution, worldState);

        // Add new characters
        worldState.AddCharacters(evolution.NewCharacters);

        // Add new opportunities
        worldState.AddOpportunities(evolution.NewOpportunities);

        // Process player location change (must be done last after all locations are set up)
        ProcessPlayerLocationChange(evolution, worldState, playerState);
    }

    private async Task ProcessNewActions(WorldEvolutionResponse evolution, WorldState worldState)
    {
        foreach (NewAction newAction in evolution.NewActions)
        {
            Location targetLocation = worldState.GetLocation(newAction.LocationName);
            if (targetLocation != null && targetLocation.Spots != null)
            {
                LocationSpot spotToUpdate = targetLocation.Spots.FirstOrDefault(s =>
                    s.Name.Equals(newAction.SpotName, StringComparison.OrdinalIgnoreCase));

                if (spotToUpdate != null)
                {
                    if (spotToUpdate.ActionTemplates == null)
                        spotToUpdate.ActionTemplates = new List<string>();

                    // Create a context for ActionGenerator
                    ActionGenerationContext context = new ActionGenerationContext
                    {
                        LocationName = targetLocation.Name,
                        LocationDescription = targetLocation.Description,
                        SpotName = spotToUpdate.Name,
                        SpotDescription = spotToUpdate.Description,
                        InteractionType = spotToUpdate.InteractionType,
                        EnvironmentalProperties = targetLocation.EnvironmentalProperties
                            .Select(p => p.GetPropertyValue())
                            .ToList()
                    };

                    // Create a mock action creation result to pass to ActionGenerator
                    // This ensures proper creation of both action and encounter templates
                    EncounterTemplateModel encounterModel = CreateDefaultEncounterModel(newAction);

                    // Create and register the encounter template
                    EncounterTemplate encounterTemplate = _actionGenerator.CreateEncounterTemplate(encounterModel);
                    string encounterName = $"{newAction.Name}Encounter";
                    _actionRepository.RegisterEncounterTemplate(encounterName, encounterTemplate);

                    // Create action template linked to the encounter
                    string actionTemplateName = _actionRepository.GetOrCreateActionTemplate(
                        newAction.Name,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType),
                        encounterName
                    );

                    // Add the template name to the spot's action templates
                    spotToUpdate.ActionTemplates.Add(actionTemplateName);
                }
            }
        }
    }

    private EncounterTemplateModel CreateDefaultEncounterModel(NewAction newAction)
    {
        // Create a default encounter template model based on the action type
        return new EncounterTemplateModel
        {
            Name = $"{newAction.Name}Encounter",
            Duration = 5,
            MaxPressure = 12,
            PartialThreshold = 10,
            StandardThreshold = 14,
            ExceptionalThreshold = 18,
            Hostility = "Neutral",
            PressureReducingFocuses = GetDefaultPressureReducingFocuses(newAction.ActionType),
            MomentumReducingFocuses = GetDefaultMomentumReducingFocuses(newAction.ActionType),
            StrategicTags = GetDefaultStrategicTags(newAction.ActionType),
            NarrativeTags = GetDefaultNarrativeTags(newAction.ActionType)
        };
    }

    private List<string> GetDefaultPressureReducingFocuses(string actionType)
    {
        switch (ParseActionType(actionType))
        {
            case BasicActionTypes.Discuss:
                return new List<string> { "Relationship", "Information" };
            case BasicActionTypes.Persuade:
                return new List<string> { "Relationship", "Resource" };
            case BasicActionTypes.Travel:
                return new List<string> { "Environment", "Physical" };
            case BasicActionTypes.Rest:
                return new List<string> { "Physical", "Resource" };
            case BasicActionTypes.Investigate:
                return new List<string> { "Information", "Environment" };
            default:
                return new List<string> { "Information", "Relationship" };
        }
    }

    private List<string> GetDefaultMomentumReducingFocuses(string actionType)
    {
        switch (ParseActionType(actionType))
        {
            case BasicActionTypes.Discuss:
                return new List<string> { "Physical", "Environment" };
            case BasicActionTypes.Persuade:
                return new List<string> { "Physical", "Environment" };
            case BasicActionTypes.Travel:
                return new List<string> { "Relationship", "Information" };
            case BasicActionTypes.Rest:
                return new List<string> { "Relationship", "Information" };
            case BasicActionTypes.Investigate:
                return new List<string> { "Relationship", "Physical" };
            default:
                return new List<string> { "Physical", "Environment" };
        }
    }

    private List<StrategicTagModel> GetDefaultStrategicTags(string actionType)
    {
        List<StrategicTagModel> tags = new List<StrategicTagModel>();

        switch (ParseActionType(actionType))
        {
            case BasicActionTypes.Discuss:
                tags.Add(new StrategicTagModel { Name = "Social Currency", EnvironmentalProperty = "Crowded" });
                tags.Add(new StrategicTagModel { Name = "Cold Calculation", EnvironmentalProperty = "Formal" });
                tags.Add(new StrategicTagModel { Name = "Social Distraction", EnvironmentalProperty = "Chaotic" });
                tags.Add(new StrategicTagModel { Name = "Tactical Advantage", EnvironmentalProperty = "Quiet" });
                break;
            case BasicActionTypes.Persuade:
                tags.Add(new StrategicTagModel { Name = "Social Currency", EnvironmentalProperty = "Crowded" });
                tags.Add(new StrategicTagModel { Name = "Calming Influence", EnvironmentalProperty = "Tense" });
                tags.Add(new StrategicTagModel { Name = "Trading Post", EnvironmentalProperty = "Commercial" });
                tags.Add(new StrategicTagModel { Name = "Overthinking", EnvironmentalProperty = "Formal" });
                break;
            // Add other action types with appropriate tags
            default:
                tags.Add(new StrategicTagModel { Name = "Insightful Approach", EnvironmentalProperty = "Bright" });
                tags.Add(new StrategicTagModel { Name = "Calculated Response", EnvironmentalProperty = "Quiet" });
                tags.Add(new StrategicTagModel { Name = "Tactical Advantage", EnvironmentalProperty = "Isolated" });
                tags.Add(new StrategicTagModel { Name = "Overthinking", EnvironmentalProperty = "Tense" });
                break;
        }

        return tags;
    }

    private List<string> GetDefaultNarrativeTags(string actionType)
    {
        switch (ParseActionType(actionType))
        {
            case BasicActionTypes.Discuss:
                return new List<string> { "SuperficialCharm", "ColdCalculation" };
            case BasicActionTypes.Persuade:
                return new List<string> { "SuperficialCharm", "GenerousSpirit" };
            case BasicActionTypes.Travel:
                return new List<string> { "DetailFixation", "TunnelVision" };
            case BasicActionTypes.Rest:
                return new List<string> { "DetailFixation", "HesitantPoliteness" };
            case BasicActionTypes.Investigate:
                return new List<string> { "DetailFixation", "AnalysisParalysis" };
            default:
                return new List<string> { "DetailFixation", "Overthinking" };
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

    private void ProcessPlayerLocationChange(
        WorldEvolutionResponse evolution,
        WorldState worldState,
        PlayerState playerState)
    {
        if (evolution.LocationUpdate.LocationChanged && !string.IsNullOrEmpty(evolution.LocationUpdate.NewLocationName))
        {
            // Find the location (should exist now after processing new locations)
            Location newPlayerLocation = worldState.GetLocation(evolution.LocationUpdate.NewLocationName);
            if (newPlayerLocation != null)
            {
                worldState.SetCurrentLocation(newPlayerLocation);
                playerState.AddLocationKnowledge(evolution.LocationUpdate.NewLocationName);
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
                worldState.SetCurrentLocation(newLocation);
                playerState.AddLocationKnowledge(evolution.LocationUpdate.NewLocationName);
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
