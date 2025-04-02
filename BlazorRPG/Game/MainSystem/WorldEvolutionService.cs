public class WorldEvolutionService
{
    public NarrativeService _narrativeService { get; }
    public ActionRepository _actionRepository { get; }
    public ActionFactory _actionFactory { get; }

    public WorldEvolutionService(
        NarrativeService narrativeService,
        ActionRepository actionRepository,
        ActionFactory actionFactory)
    {
        _narrativeService = narrativeService;
        _actionRepository = actionRepository;
        _actionFactory = actionFactory;
    }

    public async Task<WorldEvolutionResponse> ProcessWorldEvolution(NarrativeContext context, WorldEvolutionInput input)
    {
        WorldEvolutionResponse response = await _narrativeService.ProcessWorldEvolution(context, input);
        return response;
    }

    public async Task<string> ConsolidateMemory(NarrativeContext context, MemoryConsolidationInput input)
    {
        string response = await _narrativeService.ProcessMemoryConsolidation(context, input);
        return response;
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

        // Process relationship changes
        if (evolution.RelationshipChanges != null && evolution.RelationshipChanges.Any())
        {
            foreach (RelationshipChange relationshipChange in evolution.RelationshipChanges)
            {
                // Get current relationship level
                int currentLevel = playerState.Relationships.GetLevel(relationshipChange.CharacterName);

                // Apply the change
                int newLevel = currentLevel + relationshipChange.ChangeAmount;

                // Update relationship
                playerState.Relationships.SetLevel(relationshipChange.CharacterName, newLevel);

                // Optional: Log relationship changes for debugging or history tracking
                Console.WriteLine($"Relationship with {relationshipChange.CharacterName} changed from {currentLevel} to {newLevel}: {relationshipChange.Reason}");
            }
        }

        // Process new locations first (may be needed for player location change)
        foreach (Location location in evolution.NewLocations)
        {
            worldState.AddLocations(new List<Location> { location });
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

        // Add new location spots to current location
        string currentLocationName = worldState.CurrentLocation?.Name;
        if (currentLocationName != null)
        {
            foreach (LocationSpot spot in evolution.NewLocationSpots.Where(s =>
                string.IsNullOrEmpty(s.LocationName) ||
                s.LocationName.Equals(currentLocationName, StringComparison.OrdinalIgnoreCase)))
            {
                locationSystem.AddSpot(currentLocationName, spot);
            }

            // Handle spots for other existing locations
            IEnumerable<LocationSpot> spotsForOtherLocations = evolution.NewLocationSpots.Where(s =>
                !string.IsNullOrEmpty(s.LocationName) &&
                !s.LocationName.Equals(currentLocationName, StringComparison.OrdinalIgnoreCase));

            foreach (LocationSpot spot in spotsForOtherLocations)
            {
                Location? targetLocation = worldState.GetLocation(spot.LocationName);
                if (targetLocation != null)
                {
                    locationSystem.AddSpot(spot.LocationName, spot);
                }
            }
        }

        // Add new actions to existing spots
        foreach (NewAction newAction in evolution.NewActions)
        {
            Location? targetLocation = worldState.GetLocation(newAction.LocationName);
            if (targetLocation != null && targetLocation.Spots != null)
            {
                LocationSpot? spotToUpdate = targetLocation.Spots.FirstOrDefault(s =>
                    s.Name.Equals(newAction.SpotName, StringComparison.OrdinalIgnoreCase));

                if (spotToUpdate != null)
                {
                    if (spotToUpdate.ActionTemplates == null)
                        spotToUpdate.ActionTemplates = new List<string>();

                    // Process the action and get a proper implementation
                    string action = CreateSingleAction(
                        newAction.Name,
                        newAction.Description,
                        spotToUpdate.Name,
                        targetLocation,
                        newAction.Goal,
                        newAction.Complication,
                        newAction.ActionType);

                    spotToUpdate.ActionTemplates.Add(action);
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
            Location? newPlayerLocation = worldState.GetLocation(evolution.LocationUpdate.NewLocationName);
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
                    Spots = new List<LocationSpot> { },
                    ConnectedTo = new List<string> { worldState.CurrentLocation?.Name ?? string.Empty }
                };

                worldState.AddLocations(new List<Location> { newLocation });
                worldState.SetCurrentLocation(newLocation);
                playerState.AddLocationKnowledge(evolution.LocationUpdate.NewLocationName);
            }
        }
    }

    private string CreateSingleAction(
        string actionName,
        string description,
        string spotName,
        Location location,
        string goal = "",
        string complication = "",
        string actionTypeStr = "Discuss")
    {
        BasicActionTypes actionType = ParseActionType(actionTypeStr);

        // Check if this action already exists in the repository
        ActionTemplate existingTemplate = _actionRepository.GetAction(actionName);
        if (existingTemplate != null)
        {
            return existingTemplate.Name;

        }

        // If not, we need to create an encounter template for it
        string encounterName = $"{actionName}Encounter";

        // Generate an appropriate encounter template based on action type and spot
        EncounterTemplate encounterTemplate = GenerateEncounterTemplateForAction(
            actionName, actionType, spotName, location);

        // Register the encounter template
        _actionRepository.RegisterEncounterTemplate(encounterName, encounterTemplate);

        // Create and register the action template
        ActionTemplate template = _actionRepository.GetOrCreateAction(
            actionName, goal, complication, actionType, encounterTemplate.Name);

        // Create and return the action implementation
        return _actionFactory.CreateActionFromTemplate(template).Name;
    }

    private BasicActionTypes ParseActionType(string actionTypeStr)
    {
        if (string.IsNullOrEmpty(actionTypeStr) ||
            !Enum.TryParse<BasicActionTypes>(actionTypeStr, true, out BasicActionTypes actionType))
        {
            return BasicActionTypes.Discuss; // Default
        }

        return actionType;
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
