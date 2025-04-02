public class WorldEvolutionService
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionRepository _actionRepository;

    public WorldEvolutionService(
        NarrativeService narrativeService,
        ActionRepository actionRepository)
    {
        _narrativeService = narrativeService;
        _actionRepository = actionRepository;
    }

    public async Task<WorldEvolutionResponse> ProcessWorldEvolution(
        NarrativeContext context,
        WorldEvolutionInput input)
    {
        // Get world evolution response from narrative service
        WorldEvolutionResponse response = await _narrativeService.ProcessWorldEvolution(context, input);

        // Register encounter templates for any new actions
        foreach (NewAction newAction in response.NewActions)
        {
            // Create a basic encounter template if one doesn't exist
            string encounterTemplateName = $"{newAction.Name}Encounter";
            if (_actionRepository.GetEncounterTemplate(encounterTemplateName) == null)
            {
                EncounterTemplate template = CreateBasicEncounterTemplate(encounterTemplateName);
                _actionRepository.RegisterEncounterTemplate(encounterTemplateName, template);
            }
        }

        return response;
    }

    public async Task<string> ConsolidateMemory(
        NarrativeContext context,
        MemoryConsolidationInput input)
    {
        return await _narrativeService.ProcessMemoryConsolidation(context, input);
    }

    private EncounterTemplate CreateBasicEncounterTemplate(string name)
    {
        return new EncounterTemplate
        {
            Name = name,
            Duration = 5,
            MaxPressure = 13,
            PartialThreshold = 12,
            StandardThreshold = 16,
            ExceptionalThreshold = 20,
            Hostility = EncounterInfo.HostilityLevels.Neutral,
            PressureReducingFocuses = new List<FocusTags>(),
            MomentumReducingFocuses = new List<FocusTags>(),
            EncounterNarrativeTags = new List<NarrativeTag>(),
            encounterStrategicTags = new List<StrategicTag>()
        };
    }

    public void IntegrateWorldEvolution(
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

        // Process relationship changes
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

        // Add new actions to existing spots - CORRECTED TO USE TEMPLATES ONLY
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

                    // Create and store an action template name - NOT an implementation
                    string actionTemplateName = _actionRepository.GetOrCreateActionTemplate(
                        newAction.Name,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType),
                        $"{newAction.Name}Encounter" // Default encounter template name
                    );

                    // Add the template name to the spot's action templates
                    spotToUpdate.ActionTemplates.Add(actionTemplateName);
                }
            }
        }

        // Add new characters
        worldState.AddCharacters(evolution.NewCharacters);

        // Add new opportunities
        worldState.AddOpportunities(evolution.NewOpportunities);

        // Process player location change (must be done last after all locations are set up)
        if (evolution.LocationUpdate.LocationChanged && !string.IsNullOrEmpty(evolution.LocationUpdate.NewLocationName))
        {
            // Find the location (should exist now after processing new locations)
            Location newPlayerLocation = worldState.GetLocation(evolution.LocationUpdate.NewLocationName);
            if (newPlayerLocation != null)
            {
                worldState.SetCurrentLocation(newPlayerLocation);
                // Also update player's knowledge of this location
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


    private EncounterTemplate GenerateEncounterTemplateForAction(
    string actionName,
    BasicActionTypes actionType,
    string spotName,
    Location location)
    {
        // Create a basic template with reasonable defaults
        EncounterTemplate template = new EncounterTemplate
        {
            Name = actionName,
            Duration = 5,
            MaxPressure = 10,
            PartialThreshold = 10,
            StandardThreshold = 14,
            ExceptionalThreshold = 18,
            Hostility = EncounterInfo.HostilityLevels.Neutral,
            PressureReducingFocuses = new List<FocusTags>(),
            MomentumReducingFocuses = new List<FocusTags>(),
            EncounterNarrativeTags = new List<NarrativeTag>(),
            encounterStrategicTags = new List<StrategicTag>()
        };

        // Configure approaches based on action type
        switch (actionType)
        {
            case BasicActionTypes.Discuss:
                template.PressureReducingFocuses.AddRange(new[] { FocusTags.Relationship, FocusTags.Information });
                template.MomentumReducingFocuses.Add(FocusTags.Physical);
                template.EncounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.ColdCalculation, NarrativeTagRepository.IntimidatingPresence });
                break;

            case BasicActionTypes.Persuade:
                template.PressureReducingFocuses.AddRange(new[] { FocusTags.Relationship, FocusTags.Resource });
                template.MomentumReducingFocuses.Add(FocusTags.Environment);
                template.EncounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.SuperficialCharm, NarrativeTagRepository.DetailFixation });
                break;

            case BasicActionTypes.Travel:
                template.PressureReducingFocuses.AddRange(new[] { FocusTags.Environment, FocusTags.Physical });
                template.MomentumReducingFocuses.Add(FocusTags.Relationship);
                template.EncounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.TunnelVision, NarrativeTagRepository.ParanoidMindset });
                break;

            case BasicActionTypes.Rest:
                template.PressureReducingFocuses.Add(FocusTags.Environment);
                template.MomentumReducingFocuses.Add(FocusTags.Relationship);
                template.EncounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.ParanoidMindset, NarrativeTagRepository.DetailFixation });
                break;

            case BasicActionTypes.Investigate:
                template.PressureReducingFocuses.Add(FocusTags.Information);
                template.MomentumReducingFocuses.AddRange(new[] { FocusTags.Relationship, FocusTags.Physical });
                template.EncounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.Overthinking, NarrativeTagRepository.DetailFixation });
                break;
        }

        // Add strategic tags based on spot type and location properties
        template.encounterStrategicTags.AddRange(GenerateStrategicTagsFromLocation(spotName, location));

        return template;
    }

    private List<StrategicTag> GenerateStrategicTagsFromLocation(string spotName, Location location)
    {
        List<StrategicTag> tags = new List<StrategicTag>();

        // Get existing environmental properties from the location
        if (location.EnvironmentalProperties != null && location.EnvironmentalProperties.Count > 0)
        {
            // Use existing location properties to create strategic tags
            foreach (IEnvironmentalProperty prop in location.EnvironmentalProperties)
            {
                string tagName = $"{spotName} {prop.GetPropertyType()}";
                tags.Add(new StrategicTag(tagName, prop));
            }
        }
        else
        {
            // Create some default tags if no properties exist
            tags.Add(new StrategicTag("Natural Light", Illumination.Bright));
            tags.Add(new StrategicTag("Open Space", Physical.Expansive));
            tags.Add(new StrategicTag("Common Area", Population.Quiet));
            tags.Add(new StrategicTag("Neutral Setting", Atmosphere.Formal));
        }

        return tags;
    }

}
