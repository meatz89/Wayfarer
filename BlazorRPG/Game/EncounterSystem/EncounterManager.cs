public class EncounterManager
{
    private readonly Encounter encounter;
    public ActionImplementation ActionImplementation;

    private ChoiceCardSelector cardSelectionAlgorithm;
    public EncounterState EncounterState;

    private IAIService _aiService;
    private NarrativeContext narrativeContext;

    private readonly WorldStateInputBuilder worldStateInputCreator;
    public List<EncounterOption> CurrentChoices = new List<EncounterOption>();

    public PlayerState playerState;
    public WorldState worldState;
    public Encounter Encounter;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;

    public EncounterResult EncounterResult;

    public bool IsInitialState { get; set; }

    private readonly ILogger<EncounterManager> _logger;

    public EncounterManager(
        Encounter encounter,
        ActionImplementation actionImplementation,
        ChoiceCardSelector cardSelector,
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
        // Use EncounterManager logger for this class
        _logger = logger as ILogger<EncounterManager> ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EncounterManager>();
    }

    public string GetCurrentAIProviderName()
    {
        _logger.LogInformation("GetCurrentAIProviderName called.");
        return _aiService.GetProviderName();
    }

    public async Task<NarrativeResult> SimulateChoiceForPreGeneration(
        string location,
        EncounterOption choice,
        ChoiceNarrative choiceDescription,
        int priority)
    {
        _logger.LogInformation("SimulateChoiceForPreGeneration called with location: {Location}, choice: {ChoiceId}, priority: {Priority}", location, choice?.Id, priority);
        PlayerState playerState = this.playerState;
        EncounterState encounterStateCopy = EncounterState.CreateDeepCopy(
            this.EncounterState,
            playerState);

        ChoiceOutcome outcome = ApplyChoiceProjection(playerState, encounterStateCopy, choice);

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
                    worldStateInput,
                    priority);
            }
        }

        NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);

        if (outcome.IsEncounterOver)
        {
            NarrativeResult result = new(
                narrative,
                string.Empty,
                new List<EncounterOption>(),
                new List<ChoiceProjection>(),
                new Dictionary<EncounterOption, ChoiceNarrative>(),
                narrativeEvent.ChoiceNarrative);

            result.SetOutcome(outcome.Outcome);
            result.SetIsEncounterOver(outcome.IsEncounterOver);

            _logger.LogInformation("SimulateChoiceForPreGeneration completed: Encounter is over.");
            return result;
        }
        else
        {
            List<EncounterOption> newChoices = cardSelectionAlgorithm.SelectChoices(
                encounterStateCopy, playerState);

            List<ChoiceProjection> newProjections = newChoices.Select(
                c => encounterStateCopy.CreateChoiceProjection(c, playerState)).ToList();

            Dictionary<EncounterOption, ChoiceNarrative> newChoiceDescriptions = null;

            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                newChoiceDescriptions = await _aiService.GenerateChoiceDescriptionsAsync(
                    narrativeContext,
                    newChoices,
                    newProjections,
                    worldStateInput,
                    priority);
            }

            NarrativeResult result = new(
                narrative,
                string.Empty,
                newChoices,
                newProjections,
                newChoiceDescriptions,
                narrativeEvent.ChoiceNarrative);

            _logger.LogInformation("SimulateChoiceForPreGeneration completed: Encounter continues.");
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
        _logger.LogInformation("StartEncounterWithNarrativeAsync called for location: {LocationId}, encounter: {EncounterId}", location?.Id, encounterInfo?.Id);
        this.playerState = playerState;
        this.Encounter = encounterInfo;

        StartEncounter(worldState, playerState, encounterInfo);

        narrativeContext =
            new NarrativeContext(
                encounterInfo.LocationName.ToString(),
                encounterInfo.LocationSpotName.ToString(),
                encounterInfo.EncounterType,
                playerState,
                actionImplementation);

        string introduction = "introduction";

        if (_useAiNarrative)
        {
            string memoryContent = string.Empty;

            if (_useMemory)
            {
                memoryContent = await MemoryFileAccess.ReadFromMemoryFile();
            }

            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location.Id);
            introduction = await _aiService.GenerateIntroductionAsync(
                narrativeContext,
                memoryContent,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE);
        }

        GenerateChoicesForPlayer(playerState);
        List<EncounterOption> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = choices.Select(ProjectChoice)
                                                    .ToList();

        NarrativeEvent firstNarrative = new NarrativeEvent(
            EncounterState.CurrentTurn,
            introduction);

        narrativeContext.AddEvent(firstNarrative);

        Dictionary<EncounterOption, ChoiceNarrative> choiceDescriptions = null;
        if (_useAiNarrative)
        {
            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location.Id);

            choiceDescriptions = await _aiService.GenerateChoiceDescriptionsAsync(
                narrativeContext,
                choices,
                projections,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE);

            firstNarrative.SetAvailableChoiceDescriptions(choiceDescriptions);
        }

        NarrativeResult result = new NarrativeResult(
            introduction,
            actionImplementation.Description,
            choices,
            projections,
            choiceDescriptions,
            firstNarrative.ChoiceNarrative);

        _logger.LogInformation("StartEncounterWithNarrativeAsync completed for encounter: {EncounterId}", encounterInfo?.Id);
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
        EncounterOption choice,
        ChoiceNarrative choiceDescription,
        int priority)
    {
        _logger.LogInformation("ApplyChoiceWithNarrativeAsync called with location: {Location}, choice: {ChoiceId}, priority: {Priority}", location, choice?.Id, priority);
        ChoiceOutcome outcome = ApplyChoiceProjection(playerState, EncounterState, choice);

        string narrative = "Continued Narrative";

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
                    worldStateInput,
                    priority);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            NarrativeResult currentResult = new(
                narrative,
                string.Empty,
                new List<EncounterOption>(),
                new List<ChoiceProjection>(),
                new Dictionary<EncounterOption, ChoiceNarrative>(),
                narrativeEvent.ChoiceNarrative);

            currentResult.SetOutcome(outcome.Outcome);
            currentResult.SetIsEncounterOver(outcome.IsEncounterOver);

            // Log NarrativeResult details
            _logger.LogInformation("NarrativeResult (Encounter Over): SceneNarrative='{SceneNarrative}', ChoicesCount={ChoicesCount}, ProjectionsCount={ProjectionsCount}, ChoiceDescriptionsCount={ChoiceDescriptionsCount}, LastChoiceNarrative='{LastChoiceNarrative}'",
                currentResult.SceneNarrative,
                currentResult.ChoiceDescriptions?.Count ?? 0,
                0,
                currentResult.ChoiceDescriptions?.Count ?? 0,
                currentResult.LastChoiceNarrative?.ToString() ?? "null");

            _logger.LogInformation("ApplyChoiceWithNarrativeAsync completed: Encounter is over.");
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
                    worldStateInput,
                    priority);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            GenerateChoicesForPlayer(playerState);
            List<EncounterOption> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = newChoices.Select(ProjectChoice).ToList();

            Dictionary<EncounterOption, ChoiceNarrative> newChoiceDescriptions = null;

            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                newChoiceDescriptions = await _aiService.GenerateChoiceDescriptionsAsync(
                    narrativeContext,
                    newChoices,
                    newProjections,
                    worldStateInput,
                    priority);
            }

            narrativeEvent.ChoiceDescriptions.Clear();
            if (newChoiceDescriptions != null)
            {
                foreach (KeyValuePair<EncounterOption, ChoiceNarrative> kvp in newChoiceDescriptions)
                {
                    narrativeEvent.ChoiceDescriptions[kvp.Key] = kvp.Value;
                }
            }

            NarrativeResult ongoingResult = new(
                narrative,
                string.Empty,
                newChoices,
                newProjections,
                newChoiceDescriptions,
                narrativeEvent.ChoiceNarrative);

            // Log NarrativeResult details
            _logger.LogInformation("NarrativeResult (Encounter Continues): SceneNarrative='{SceneNarrative}', ChoicesCount={ChoicesCount}, ProjectionsCount={ProjectionsCount}, ChoiceDescriptionsCount={ChoiceDescriptionsCount}, LastChoiceNarrative='{LastChoiceNarrative}'",
                ongoingResult.SceneNarrative,
                ongoingResult.ChoiceDescriptions?.Count ?? 0,
                newProjections?.Count ?? 0,
                ongoingResult.ChoiceDescriptions?.Count ?? 0,
                ongoingResult.LastChoiceNarrative?.ToString() ?? "null");

            _logger.LogInformation("ApplyChoiceWithNarrativeAsync completed: Encounter continues.");
            return ongoingResult;
        }
    }

    private ChoiceOutcome ApplyChoiceProjection(PlayerState playerState, EncounterState encounterState, EncounterOption choice)
    {
        this.playerState = playerState;

        ChoiceProjection projection = encounterState.ApplyChoice(playerState, Encounter, choice);

        // Log details of the projection
        _logger.LogInformation(
            "ApplyChoiceProjection: ChoiceId={ChoiceId}, ProgressGained={ProgressGained}, EncounterWillEnd={EncounterWillEnd}, ProjectedOutcome={ProjectedOutcome}, HealthChange={HealthChange}, ConcentrationChange={ConcentrationChange}, IsConversionChoice={IsConversionChoice}",
            choice?.Id,
            projection.ProgressGained,
            projection.EncounterWillEnd,
            projection.ProjectedOutcome,
            projection.HealthChange,
            projection.ConcentrationChange,
            projection.IsConversionChoice
        );

        ChoiceOutcome outcome = new ChoiceOutcome(
            projection.ProgressGained,
            projection.Description,
            projection.EncounterWillEnd,
            projection.ProjectedOutcome,
            projection.HealthChange,
            projection.ConcentrationChange);

        return outcome;
    }

    private NarrativeEvent GetNarrativeEvent(
        EncounterOption choice,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        string narrative)
    {
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            EncounterState.CurrentTurn - 1,
            narrative);

        narrativeEvent.SetChosenOption(choice);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);
        return narrativeEvent;
    }

    public NarrativeContext GetNarrativeContext()
    {
        return narrativeContext;
    }

    public ChoiceProjection ProjectChoice(EncounterOption choice)
    {
        ChoiceProjection projection = EncounterState.CreateChoiceProjection(choice, playerState);
        projection.Description = choice.Id;
        return projection;
    }

    public List<EncounterOption> GetCurrentChoices()
    {
        return CurrentChoices;
    }

    public void GenerateChoicesForPlayer(PlayerState playerState)
    {
        CurrentChoices = cardSelectionAlgorithm.SelectChoices(EncounterState, playerState);
    }

}