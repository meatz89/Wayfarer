public class PostEncounterEvolutionSystem
{
    private readonly ActionGenerator _actionGenerator;
    private readonly ActionRepository _actionRepository;
    private readonly GameState gameState;
    private readonly IAIService aiService;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly WorldStateInputBuilder worldStateInputCreator;
    private readonly LocationRepository locationRepository;

    public PostEncounterEvolutionSystem(
        ActionGenerator actionGenerator,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        WorldStateInputBuilder worldStateInputCreator,
        LocationRepository locationRepository,
        ActionRepository actionRepository,
        GameState gameState,
        IAIService aiService
        )
    {
        this._actionGenerator = actionGenerator;
        this._actionRepository = actionRepository;
        this.gameState = gameState;
        this.aiService = aiService;
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
        return await aiService.ProcessMemoryConsolidation(context, input, worldStateInput);
    }

    public async Task<PostEncounterEvolutionResult> ProcessEncounterOutcome(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        EncounterResult encounterResult)
    {
        // Get world evolution response from narrative service
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(context.LocationName);
        PostEncounterEvolutionResult response = await aiService.ProcessPostEncounterEvolution(context, input, worldStateInput);
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
            string locationName = loc.Id;
            Location existingLoc = locationRepository.GetLocationById(locationName);
            if (existingLoc == null)
            {
                locationRepository.AddLocation(loc);
            }
            else
            {
                existingLoc.Description = loc.Description;
                existingLoc.ConnectedTo = loc.ConnectedTo;
                existingLoc.Depth = loc.Depth;
            }
        }

        foreach (LocationSpot spot in evolution.NewLocationSpots)
        {
            string spotName = $"{spot.LocationId}:{spot.Id}";
            string locationId = spot.LocationId;
            if(locationId == null)
            {
                locationId = worldState.CurrentLocation.Id;
                spot.LocationId = locationId;
            }

            LocationSpot existingSpot = locationRepository.GetSpot(locationId, spot.Id);
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
        }

        foreach (Character character in evolution.NewCharacters)
            worldState.AddCharacter(character);

        foreach (Opportunity opp in evolution.NewOpportunities)
            worldState.AddOpportunity(opp);
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
            CurrentLocation = worldState.CurrentLocation?.Id ?? "Unknown",
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
            MaxEnergy = playerState.MaxEnergyPoints
        };
    }

}
