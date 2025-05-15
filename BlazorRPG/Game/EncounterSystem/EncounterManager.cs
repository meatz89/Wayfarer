public class EncounterManager
{
    private readonly Encounter encounter;
    public ActionImplementation ActionImplementation;

    private CardSelectionAlgorithm cardSelectionAlgorithm;
    public EncounterState EncounterState;

    private IAIService _aiService;
    private NarrativeContext narrativeContext;

    private readonly WorldStateInputBuilder worldStateInputCreator;
    public List<CardDefinition> CurrentChoices = new List<CardDefinition>();

    public PlayerState playerState;
    public WorldState worldState;
    public Encounter Encounter;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;

    public EncounterResult EncounterResult;

    public bool IsInitialState { get; internal set; }

    public EncounterManager(
        Encounter encounter,
        ActionImplementation actionImplementation,
        CardSelectionAlgorithm cardSelector,
        IAIService aiService,
        WorldStateInputBuilder worldStateInputCreator,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.encounter = encounter;
        ActionImplementation = actionImplementation;
        cardSelectionAlgorithm = cardSelector;
        _aiService = aiService;
        this.worldStateInputCreator = worldStateInputCreator;
        _useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    public string GetCurrentAIProviderName()
    {
        return _aiService.GetProviderName();
    }

    public async Task<NarrativeResult> SimulateChoiceForPreGeneration(
    string location,
    CardDefinition choice,
    ChoiceNarrative choiceDescription,
    int priority)
    {
        // Create deep copies of state to avoid threading issues
        PlayerState playerStateCopy = this.playerState.Clone();
        EncounterState encounterStateCopy = EncounterState.CreateDeepCopy(
            this.EncounterState,
            playerStateCopy);

        // Proceed with simulation using the copied state
        ChoiceOutcome outcome = ApplyChoiceProjection(playerStateCopy, encounterStateCopy, choice);

        // Get status after the choice
        EncounterStatusModel newStatus = GetEncounterStatusModel(playerStateCopy, worldState);

        // Generate narrative based on the simulation
        string narrative = "Continued Narrative";

        if (_useAiNarrative)
        {
            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);

            if (outcome.IsEncounterOver)
            {
                narrative = await _aiService.GenerateEndingAsync(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus,
                    worldStateInput,
                    priority);
            }
            else
            {
                narrative = await _aiService.GenerateEncounterNarrative(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus,
                    worldStateInput,
                    priority);
            }
        }

        // Create a narrative event for the simulation
        NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);

        // Prepare the response based on whether the encounter ends
        if (outcome.IsEncounterOver)
        {
            NarrativeResult result = new(
                narrative,
                string.Empty,
                new List<CardDefinition>(),
                new List<ChoiceProjection>(),
                new Dictionary<CardDefinition, ChoiceNarrative>(),
                narrativeEvent.ChoiceNarrative);

            result.SetOutcome(outcome.Outcome);
            result.SetIsEncounterOver(outcome.IsEncounterOver);

            return result;
        }
        else
        {
            // Generate choices for the next step
            // This needs to be done on the copy to avoid interfering with the actual state
            List<CardDefinition> newChoices = cardSelectionAlgorithm.SelectChoices(
                encounterStateCopy, playerStateCopy);

            List<ChoiceProjection> newProjections = newChoices.Select(
                c => encounterStateCopy.CreateChoiceProjection(c)).ToList();

            // Generate choice descriptions
            Dictionary<CardDefinition, ChoiceNarrative> newChoiceDescriptions = null;

            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                newChoiceDescriptions = await _aiService.GenerateChoiceDescriptionsAsync(
                    narrativeContext,
                    newChoices,
                    newProjections,
                    newStatus,
                    worldStateInput,
                    priority);
            }

            // Create and return narrative result
            NarrativeResult result = new(
                narrative,
                string.Empty,
                newChoices,
                newProjections,
                newChoiceDescriptions,
                narrativeEvent.ChoiceNarrative);

            return result;
        }
    }

    public async Task<NarrativeResult> StartEncounterWithNarrativeAsync(
    Location location,
    Encounter encounterInfo,
    WorldState worldState,
    PlayerState playerState,
    ActionImplementation actionImplementation)
    {
        this.playerState = playerState;
        this.Encounter = encounterInfo;

        // Start the encounter mechanically
        StartEncounter(worldState, playerState, encounterInfo);

        // Create narrative context
        narrativeContext =
            new NarrativeContext(
                encounterInfo.LocationName.ToString(),
                encounterInfo.LocationSpotName.ToString(),
                encounterInfo.EncounterType,
                actionImplementation);

        // Generate introduction
        EncounterStatusModel status = GetEncounterStatusModel(playerState, worldState);

        string introduction = "introduction";

        if (_useAiNarrative)
        {
            string memoryContent = string.Empty;

            if (_useMemory)
            {
                memoryContent = await MemoryFileAccess.ReadFromMemoryFile();
            }

            // Use IMMEDIATE priority for initial generation to ensure it's fully synchronous
            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location.Id);
            introduction = await _aiService.GenerateIntroductionAsync(
                narrativeContext,
                status,
                memoryContent,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE);  // Ensure this uses immediate priority
        }

        // Get available choices
        GenerateChoicesForPlayer(playerState);
        List<CardDefinition> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = choices.Select(ProjectChoice)
                                                    .ToList();

        // Create first narrative event
        NarrativeEvent firstNarrative = new NarrativeEvent(
            EncounterState.CurrentTurn,
            introduction);

        narrativeContext.AddEvent(firstNarrative);

        // Generate choice descriptions - wait for this to complete
        Dictionary<CardDefinition, ChoiceNarrative> choiceDescriptions = null;
        if (_useAiNarrative)
        {
            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location.Id);

            // Use IMMEDIATE priority for initial choice descriptions
            choiceDescriptions = await _aiService.GenerateChoiceDescriptionsAsync(
                narrativeContext,
                choices,
                projections,
                status,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE);  // Ensure this uses immediate priority

            firstNarrative.SetAvailableChoiceDescriptions(choiceDescriptions);
        }

        NarrativeResult result = new NarrativeResult(
            introduction,
            actionImplementation.Description,
            choices,
            projections,
            choiceDescriptions,
            firstNarrative.ChoiceNarrative);

        // Return the narrative result
        return result;
    }

    private void StartEncounter(WorldState worldState, PlayerState playerState, Encounter encounterInfo)
    {
        this.worldState = worldState;
        this.playerState = playerState;

        EncounterState = new EncounterState(encounterInfo, playerState);
    }


    public async Task<NarrativeResult> ApplyChoiceWithNarrativeAsync(
        string location,
        CardDefinition choice,
        ChoiceNarrative choiceDescription,
        int priority = AIClient.PRIORITY_IMMEDIATE)
    {
        // Apply the choice
        ChoiceOutcome outcome = ApplyChoiceProjection(playerState, EncounterState, choice);

        // Get status after the choice
        EncounterStatusModel newStatus = GetEncounterStatusModel(playerState, worldState);

        // Generate narrative for the reaction and new scene
        string narrative = "Continued Narrative";

        // If the encounter is over, return the outcome
        if (outcome.IsEncounterOver)
        {
            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                narrative = await _aiService.GenerateEndingAsync(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus,
                    worldStateInput,
                    priority);  // Pass priority parameter
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            NarrativeResult currentResult = new(
                narrative,
                string.Empty,
                new List<CardDefinition>(),
                new List<ChoiceProjection>(),
                new Dictionary<CardDefinition, ChoiceNarrative>(),
                narrativeEvent.ChoiceNarrative);

            currentResult.SetOutcome(outcome.Outcome);
            currentResult.SetIsEncounterOver(outcome.IsEncounterOver);

            return currentResult;
        }
        else
        {
            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                narrative = await _aiService.GenerateEncounterNarrative(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus,
                    worldStateInput,
                    priority);  // Pass priority parameter
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            // Get the new choices and projections
            GenerateChoicesForPlayer(playerState);
            List<CardDefinition> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = newChoices.Select(ProjectChoice).ToList();

            // Generate descriptive narratives for each choice
            Dictionary<CardDefinition, ChoiceNarrative> newChoiceDescriptions = null;

            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                newChoiceDescriptions = await _aiService.GenerateChoiceDescriptionsAsync(
                    narrativeContext,
                    newChoices,
                    newProjections,
                    newStatus,
                    worldStateInput,
                    priority);  // Pass priority parameter
            }

            // Add the choice descriptions to the latest event
            narrativeEvent.ChoiceDescriptions.Clear();
            if (newChoiceDescriptions != null)
            {
                foreach (KeyValuePair<CardDefinition, ChoiceNarrative> kvp in newChoiceDescriptions)
                {
                    narrativeEvent.ChoiceDescriptions[kvp.Key] = kvp.Value;
                }
            }

            // Return the narrative result
            NarrativeResult ongoingResult = new(
                narrative,
                string.Empty,
                newChoices,
                newProjections,
                newChoiceDescriptions,
                narrativeEvent.ChoiceNarrative);

            return ongoingResult;
        }
    }

    private ChoiceOutcome ApplyChoiceProjection(PlayerState playerState, EncounterState encounterState, CardDefinition choice)
    {
        this.playerState = playerState;

        ChoiceProjection projection = encounterState.ApplyChoice(playerState, Encounter, choice);

        ChoiceOutcome outcome = new ChoiceOutcome(
            projection.MomentumGained,
            projection.PressureBuilt,
            projection.NarrativeDescription,
            projection.EncounterWillEnd,
            projection.ProjectedOutcome,
            projection.HealthChange,
            projection.ConcentrationChange);

        return outcome;
    }


    private NarrativeEvent GetNarrativeEvent(
        CardDefinition choice,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        string narrative)
    {
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            EncounterState.CurrentTurn - 1, // The turn counter increases after application
            narrative);

        narrativeEvent.SetChosenOption(choice);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);
        return narrativeEvent;
    }


    public EncounterStatusModel GetEncounterStatusModel(PlayerState playerState, WorldState worldState)
    {
        return new EncounterStatusModel(
            currentTurn: EncounterState.CurrentTurn,
            maxMomentum: EncounterState.EncounterInfo.ExceptionalThreshold,
            maxPressure: EncounterState.EncounterInfo.MaxPressure,
            successThreshold: EncounterState.EncounterInfo.StandardThreshold,
            maxTurns: EncounterState.EncounterInfo.MaxTurns,
            momentum: EncounterState.Momentum,
            pressure: EncounterState.Pressure,
            activeTagNames: EncounterState.GetActiveTagsNames(),
            playerState: playerState,
            worldState: worldState
        );
    }

    public NarrativeContext GetNarrativeContext()
    {
        return narrativeContext;
    }

    public ChoiceProjection ProjectChoice(CardDefinition choice)
    {
        ChoiceProjection projection = EncounterState.CreateChoiceProjection(choice);
        projection.NarrativeDescription = choice.Id + " " + choice.Description;
        return projection;
    }

    // Existing methods remain the same
    public List<CardDefinition> GetCurrentChoices()
    {
        return CurrentChoices;
    }

    public void GenerateChoicesForPlayer(PlayerState playerState)
    {
        CurrentChoices = cardSelectionAlgorithm.SelectChoices(EncounterState, playerState);
    }
}