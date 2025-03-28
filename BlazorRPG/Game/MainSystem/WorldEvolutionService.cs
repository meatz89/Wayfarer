public class WorldEvolutionService
{
    public NarrativeService narrativeService { get; }
    public ActionRepository _actionRepository { get; }

    public WorldEvolutionService(NarrativeService narrativeService, ActionRepository actionRepository)
    {
        this.narrativeService = narrativeService;
        this._actionRepository = actionRepository;
    }

    public async Task<WorldEvolutionResponse> ProcessWorldEvolution(NarrativeContext context, WorldEvolutionInput input)
    {
        WorldEvolutionResponse response = await narrativeService.ProcessWorldEvolution(context, input);
        return response;
    }

    public async Task<string> ConsolidateMemory(NarrativeContext context, MemoryConsolidationInput input)
    {
        string response = await narrativeService.ProcessMemoryConsolidation(context, input);
        return response;
    }
    public void IntegrateWorldEvolution(WorldEvolutionResponse evolution, WorldState worldState, LocationSystem locationSystem)
    {
        // Add new location spots to current location
        string locationName = worldState.CurrentLocation?.Name;
        if (locationName != null)
        {
            foreach (LocationSpot spot in evolution.NewLocationSpots)
            {
                // Ensure the spot has at least one action
                EnsureSpotHasActions(spot, spot.Name, worldState.CurrentLocation);

                // Process actions before adding spot
                ProcessActionsForSpot(spot, spot.Name, worldState.CurrentLocation);

                locationSystem.AddSpot(locationName, spot);
            }
        }

        // Add new actions to existing spots
        foreach (NewAction newAction in evolution.NewActions)
        {
            if (worldState.CurrentLocation != null && worldState.CurrentLocation.Spots != null)
            {
                LocationSpot? spotToUpdate = worldState.CurrentLocation.Spots.FirstOrDefault(s => s.Name == newAction.SpotName);
                if (spotToUpdate != null)
                {
                    if (spotToUpdate.Actions == null)
                        spotToUpdate.Actions = new List<ActionImplementation>();

                    // Process the action and get a proper implementation
                    ActionImplementation action = ProcessSingleAction(
                        newAction.Name,
                        newAction.Description,
                        spotToUpdate.Name,
                        worldState.CurrentLocation,
                        newAction.Goal,
                        newAction.Complication,
                        newAction.ActionType);

                    spotToUpdate.Actions.Add(action);
                }
            }
        }

        // Add new locations
        foreach (Location location in evolution.NewLocations)
        {
            // Ensure each location has at least one spot
            if (location.Spots == null || !location.Spots.Any())
            {
                location.Spots = new List<LocationSpot>
            {
                CreateDefaultSpot(location.Name)
            };
            }

            // Process actions in each spot of the new location
            if (location.Spots != null)
            {
                foreach (LocationSpot spot in location.Spots)
                {
                    // Ensure the spot has at least one action
                    EnsureSpotHasActions(spot, spot.Name, location);

                    // Process actions
                    ProcessActionsForSpot(spot, spot.Name, location);
                }
            }

            worldState.AddLocations(new List<Location> { location });
        }

        // Add new characters
        worldState.AddCharacters(evolution.NewCharacters);

        // Add new opportunities
        worldState.AddOpportunities(evolution.NewOpportunities);
    }

    private void EnsureSpotHasActions(LocationSpot spot, string spotName, Location location)
    {
        // Initialize actions list if null
        if (spot.Actions == null)
            spot.Actions = new List<ActionImplementation>();

        // If no actions exist, create a default one based on spot type
        if (!spot.Actions.Any())
        {
            string actionName;
            string description;
            string goal;
            string complication;
            BasicActionTypes actionType;

            // Determine appropriate default action based on interaction type
            switch (spot.InteractionType?.ToLower())
            {
                case "shop":
                    actionName = "TradeGoods";
                    description = $"Trade at {spotName}";
                    goal = $"Acquire or sell goods at {spotName}";
                    complication = "Getting fair prices requires negotiation";
                    actionType = BasicActionTypes.Persuade;
                    break;

                case "character":
                    actionName = "VillageGathering";
                    description = $"Speak with locals at {spotName}";
                    goal = $"Gather information from locals at {spotName}";
                    complication = "People may not readily share what they know";
                    actionType = BasicActionTypes.Discuss;
                    break;

                case "feature":
                    actionName = "Investigate";
                    description = $"Examine {spotName}";
                    goal = $"Discover what {spotName} has to offer";
                    complication = "Careful observation is required";
                    actionType = BasicActionTypes.Investigate;
                    break;

                case "service":
                    actionName = "RentRoom";
                    description = $"Use services at {spotName}";
                    goal = $"Benefit from the services at {spotName}";
                    complication = "Service quality may vary";
                    actionType = BasicActionTypes.Rest;
                    break;

                case "travel":
                    actionName = "ForestTravel";
                    description = $"Travel through {spotName}";
                    goal = $"Navigate safely through {spotName}";
                    complication = "The journey may present unexpected challenges";
                    actionType = BasicActionTypes.Travel;
                    break;

                default:
                    actionName = "VillageGathering";
                    description = $"Explore {spotName}";
                    goal = $"Discover what this area has to offer";
                    complication = "The unfamiliar environment presents challenges";
                    actionType = BasicActionTypes.Investigate;
                    break;
            }

            // Create and add the default action
            ActionImplementation defaultAction = ProcessSingleAction(
                actionName, description, spotName, location, goal, complication, actionType);

            spot.Actions.Add(defaultAction);
        }
    }

    private LocationSpot CreateDefaultSpot(string locationName)
    {
        // Create a default spot for locations that don't have any
        string spotName = $"Main Area";

        LocationSpot defaultSpot = new LocationSpot
        {
            Name = spotName,
            Description = $"The main area of {locationName}",
            InteractionType = "Feature",
            InteractionDescription = $"Explore this area to see what {locationName} has to offer",
            Position = "Center",
            LocationName = locationName,
            Actions = new List<ActionImplementation>()
        };

        // Add a default action to the spot
        ActionImplementation defaultAction = ProcessSingleAction(
            "VillageGathering",
            $"Explore {spotName}",
            spotName,
            new Location { Name = locationName },
            $"Discover what this area has to offer",
            "The unfamiliar environment presents challenges",
            BasicActionTypes.Investigate.ToString());

        defaultSpot.Actions.Add(defaultAction);

        return defaultSpot;
    }

    private void ProcessActionsForSpot(LocationSpot spot, string spotName, Location location)
    {
        if (spot.Actions == null)
            return;

        // Create a new list to hold the processed actions
        List<ActionImplementation> processedActions = new List<ActionImplementation>();

        foreach (ActionImplementation action in spot.Actions)
        {
            // Extract values from the action
            string actionName = action.Name;
            string description = action.Description;
            string goal = action.Goal ?? description;
            string complication = action.Complication ?? "Unexpected complications may arise";
            BasicActionTypes actionType = action.ActionType;

            // Process the action
            ActionImplementation processedAction = ProcessSingleAction(
                actionName, description, spotName, location, goal, complication, actionType.ToString());

            // Add to the processed list
            processedActions.Add(processedAction);
        }

        // Replace the original actions with processed ones
        spot.Actions = processedActions;
    }

    private ActionImplementation ProcessSingleAction(
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
        if (_actionRepository.TryGetActionTemplate(actionName, out ActionTemplate? existingTemplate))
        {
            // If it exists, use the existing template
            return ActionFactory.CreateAction(existingTemplate);
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
            actionName, goal, complication, actionType, encounterTemplate);

        // Create and return the action implementation
        return ActionFactory.CreateAction(template);
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
            Duration = 5,
            MaxPressure = 10,
            PartialThreshold = 10,
            StandardThreshold = 14,
            ExceptionalThreshold = 18,
            Hostility = EncounterInfo.HostilityLevels.Neutral,
            MomentumBoostApproaches = new List<ApproachTags>(),
            DangerousApproaches = new List<ApproachTags>(),
            PressureReducingFocuses = new List<FocusTags>(),
            MomentumReducingFocuses = new List<FocusTags>(),
            encounterNarrativeTags = new List<NarrativeTag>(),
            encounterStrategicTags = new List<StrategicTag>()
        };

        // Configure approaches based on action type
        switch (actionType)
        {
            case BasicActionTypes.Discuss:
                template.MomentumBoostApproaches.AddRange(new[] { ApproachTags.Rapport, ApproachTags.Analysis });
                template.DangerousApproaches.Add(ApproachTags.Dominance);
                template.PressureReducingFocuses.AddRange(new[] { FocusTags.Relationship, FocusTags.Information });
                template.MomentumReducingFocuses.Add(FocusTags.Physical);
                template.encounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.ColdCalculation, NarrativeTagRepository.IntimidatingPresence });
                break;

            case BasicActionTypes.Persuade:
                template.MomentumBoostApproaches.AddRange(new[] { ApproachTags.Rapport, ApproachTags.Dominance });
                template.DangerousApproaches.Add(ApproachTags.Analysis);
                template.PressureReducingFocuses.AddRange(new[] { FocusTags.Relationship, FocusTags.Resource });
                template.MomentumReducingFocuses.Add(FocusTags.Environment);
                template.encounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.SuperficialCharm, NarrativeTagRepository.DetailFixation });
                break;

            case BasicActionTypes.Travel:
                template.MomentumBoostApproaches.AddRange(new[] { ApproachTags.Analysis, ApproachTags.Precision });
                template.DangerousApproaches.Add(ApproachTags.Dominance);
                template.PressureReducingFocuses.AddRange(new[] { FocusTags.Environment, FocusTags.Physical });
                template.MomentumReducingFocuses.Add(FocusTags.Relationship);
                template.encounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.TunnelVision, NarrativeTagRepository.ParanoidMindset });
                break;

            case BasicActionTypes.Rest:
                template.MomentumBoostApproaches.Add(ApproachTags.Analysis);
                template.DangerousApproaches.AddRange(new[] { ApproachTags.Rapport, ApproachTags.Dominance });
                template.PressureReducingFocuses.Add(FocusTags.Environment);
                template.MomentumReducingFocuses.Add(FocusTags.Relationship);
                template.encounterNarrativeTags.AddRange(new[]
                    { NarrativeTagRepository.ParanoidMindset, NarrativeTagRepository.DetailFixation });
                break;

            case BasicActionTypes.Investigate:
                template.MomentumBoostApproaches.Add(ApproachTags.Analysis);
                template.DangerousApproaches.Add(ApproachTags.Dominance);
                template.PressureReducingFocuses.Add(FocusTags.Information);
                template.MomentumReducingFocuses.AddRange(new[] { FocusTags.Relationship, FocusTags.Physical });
                template.encounterNarrativeTags.AddRange(new[]
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
