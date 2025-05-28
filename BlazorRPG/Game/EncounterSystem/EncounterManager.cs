public class EncounterManager
{
    private IAIService aiGameMaster;
    private EncounterUIController uiController;
    private IUISystem uiSystem;
    private PayloadRegistry payloadRegistry;
    private SkillCheckResolver skillResolver;
    private ChoiceConverter choiceConverter;
    private PayloadProcessor payloadProcessor;

    private Encounter encounter;
    public LocationAction locationAction;

    public EncounterState state;

    private EncounterContext context;

    private WorldStateInputBuilder worldStateInputCreator;
    private ChoiceProjectionService projectionService;
    public List<EncounterChoice> CurrentChoices = new List<EncounterChoice>();

    public Player playerState;
    public WorldState worldState;
    public Encounter Encounter;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;

    public EncounterResult EncounterResult;
    private UIService uiService;

    public bool IsInitialState { get; set; }

    private ILogger<EncounterManager> _logger;

    public EncounterManager(
        Encounter encounter,
        LocationAction locationAction,
        WorldStateInputBuilder worldStateInputCreator,
        ChoiceProjectionService choiceProjectionService,
        IAIService aiGameMaster,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.encounter = encounter;
        this.encounter = encounter;
        this.aiGameMaster = aiGameMaster;
        this.worldStateInputCreator = worldStateInputCreator;
        this.projectionService = choiceProjectionService;
        _useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
        _useMemory = configuration.GetValue<bool>("useMemory");
        _logger = logger as ILogger<EncounterManager> ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EncounterManager>();
    }

    public async Task<EncounterResult> StartEncounter(EncounterContext context, Player player)
    {
        EncounterState state = new EncounterState(player, 6, 8);

        uiSystem.ShowMessage("Beginning encounter: " + context.ActionName);

        if (!state.IsEncounterComplete && state.FocusPoints > 0 && state.DurationCounter < state.MaxDuration)
        {
            var beatResult = await ProcessEncounterBeat(
                state, 
                context,
                chosenOption,
                worldStateInput);

            return null;
        }
        else
        {
            EncounterResult result = await ProcessEncounterConclusion(state, context);
            return result;
        }
    }

    public void StartEncounter(EncounterContext context, Player player)
    {
        // Initialize state
        EncounterState state = new EncounterState();
        state.Player = player;
        state.MaxFocusPoints = DetermineFocusPoints(context.ActionType);
        state.FocusPoints = state.MaxFocusPoints;
        state.MaxDuration = 8;
        state.DurationCounter = 0;
        state.IsEncounterComplete = false;
        state.GoalFlags = DetermineGoalFlags(context.ActionType);
        state.FlagManager = new EncounterFlagManager();

        // Initialize AI
        aiGameMaster = new AIGameMaster();

        // Run encounter loop
        while (!state.IsEncounterComplete && state.FocusPoints >= 0)
        {
            ProcessEncounterBeat(state, context);
        }

        ProcessEncounterConclusion(state, context);
    }

    public async Task<BeatResult> ProcessEncounterBeat(
        EncounterState state, 
        EncounterContext context,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput)
    {
        // Step 1: Get AI response
        AIGameMasterResponse aiResponse = new AIGameMasterResponse();
        string narration = await aiGameMaster.GenerateIntroduction(
            context,
            state,
            chosenOption,
            worldStateInput,
            AIClient.PRIORITY_IMMEDIATE
        );
        aiResponse.Narration = narration;

        List<EncounterChoice> choices = await aiGameMaster.GenerateChoices(
            context,
            state,
            chosenOption,
            worldStateInput,
            AIClient.PRIORITY_IMMEDIATE
        );
        aiResponse.AvailableChoices = choices;

        // Step 2: Project choices for UI
        List<ChoiceProjection> projections = new List<ChoiceProjection>();
        foreach (EncounterChoice choice in aiResponse.AvailableChoices)
        {
            projections.Add(projectionService.ProjectChoice(choice, state));
        }

        // Step 3: Present to player via UI
        PlayerChoiceSelection selection = await uiService.PresentChoices(projections);

        // Step 4: Check for encounter completion
        if (state.IsEncounterComplete)
        {
            return new BeatResult { IsComplete = true };
        }

        // Step 8: Continue to next beat
        return new BeatResult { IsComplete = false };
    }


    private void ProcessEncounterConclusion(EncounterState state, EncounterContext context)
    {
        // Step 1: Get AI to generate narrative conclusion
        EncounterResult conclusion = aiGameMaster.GenerateConclusion(
            context,
            state,
            chosenOption,
            worldStateInput,
            AIClient.PRIORITY_IMMEDIATE
        ); 

        // Step 2: Show conclusion to player
        uiController.ShowEncounterConclusion(conclusion);

        // Step 3: Apply persistent changes
        PersistentChangeProcessor persistentChangeProcessor = new PersistentChangeProcessor();
        persistentChangeProcessor.ApplyChanges(conclusion, state);
    }


    // TODO
    private void ProcessPlayerSelection(PlayerChoiceSelection selection, EncounterState state)
    {
        // Deduct focus cost
        state.FocusPoints -= selection.Choice.FocusCost;

        // Resolve skill check
        SkillCheckResult result = skillResolver.ResolveCheck(selection.SelectedOption, state);
        uiSystem.DisplaySkillCheckResult(result);

        // Apply payload
        string payloadID = result.IsSuccess ?
            selection.SelectedOption.SuccessPayload.MechanicalEffectID :
            selection.SelectedOption.FailurePayload.MechanicalEffectID;

        IMechanicalEffect effect = payloadRegistry.GetEffect(payloadID);
        effect.Apply(state);

        // Advance duration
        state.DurationCounter++;

        // Check for completion
        CheckEncounterCompletion(state);
    }

    public async Task<AIGameMasterResponse> ProcessPlayerSelection(
        string location,
        EncounterChoice choice,
        int priority)
    {
        _logger.LogInformation("ApplyChoiceWithNarrativeAsync called with location: {Location}, choice: {ChoiceId}, priority: {Priority}", location, choice?.ChoiceID, priority);
        BeatOutcome outcome = ApplyChoiceProjection(playerState, state, choice);

        string narrative = "Continued Narrative";

        if (outcome.IsEncounterComplete)
        {
            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                narrative = await _aiGameMaster.GenerateEndingAsync(
                    context,
                    choice,
                    outcome,
                    worldStateInput,
                    priority);
            }

            AIGameMasterResponse currentResult = new(
                narrative,
                string.Empty,
                new List<EncounterChoice>(),
                new List<ChoiceProjection>());

            currentResult.SetOutcome(outcome.Outcome);
            currentResult.SetIsEncounterOver(outcome.IsEncounterComplete);

            _logger.LogInformation("ApplyChoiceWithNarrativeAsync completed: Encounter is over.");
            return currentResult;
        }
        else
        {
            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                narrative = await _aiGameMaster.GenerateReactionAsync(
                    context,
                    state,
                    choice,
                    outcome,
                    worldStateInput,
                    priority);
            }

            await GenerateChoicesForPlayer(playerState);
            List<EncounterChoice> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = new List<ChoiceProjection>();
            foreach (EncounterChoice newChoice in newChoices)
            {
                newProjections.Add(ProjectChoice(projectionService, state, newChoice));
            }

            AIGameMasterResponse ongoingResult = new(
                narrative,
                string.Empty,
                newChoices,
                newProjections);

            _logger.LogInformation("ApplyChoiceWithNarrativeAsync completed: Encounter continues.");
            return ongoingResult;
        }
    }


    private int DetermineFocusPoints(ActionTypes actionType)
    {
        throw new NotImplementedException();
    }

    private async Task GenerateChoicesForPlayer(Player playerState, PlayerChoiceSelection chosenOption)
    {
        if (_useAiNarrative)
        {
            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(Encounter.LocationName);
            List<EncounterChoice> generatedChoices = await aiGameMaster.GenerateChoices(
                context,
                state,
                chosenOption,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE);

            // Update choice IDs to ensure uniqueness
            for (int i = 0; i < generatedChoices.Count; i++)
            {
                EncounterChoice choice = generatedChoices[i];
            }

            CurrentChoices = generatedChoices;
            _logger.LogInformation($"Generated {CurrentChoices.Count} AI choices for encounter stage {state.DurationCounter}");
        }
        else
        {
            // Fallback to hardcoded choices if AI narrative is disabled
            GenerateFallbackChoices(state, context);
        }
    }

    private AIGameMasterResponse GenerateFallbackChoices(EncounterState encounterState, EncounterContext encounterContext)
    {
        CurrentChoices = new List<EncounterChoice>
        {
            CreateFallbackChoice(0, "Proceed carefully", 1, "Observation"),
            CreateFallbackChoice(1, "Take aggressive action", 2, "Brute Force"),
            CreateFallbackChoice(2, "Try diplomatic approach", 1, "Negotiation"),
            CreateFallbackChoice(3, "Gather more information", 1, "Investigation"),
            CreateFallbackChoice(4, "Take a moment to recover", 0, "Perception")
        };

        return new AIGameMasterResponse
        {
            Narration = "Fallback choices generated due to AI narrative being disabled.",
            AvailableChoices = CurrentChoices,
        };
    }

    public void StartEncounter(EncounterContext encounterContext, EncounterParameters encounterParameters, Player player)
    {
        // TODO
        state = new EncounterState(player, skillCategory);
    }


    private BeatOutcome ApplyChoiceProjection(Player playerState, EncounterState encounterState, EncounterChoice choice)
    {
        this.playerState = playerState;

        ChoiceProjection projection = encounterState.ApplyChoice(projectionService, playerState, Encounter, choice);

        // Log details of the projection
        _logger.LogInformation(
            "ApplyChoiceProjection: ChoiceId={ChoiceId}, ProgressGained={ProgressGained}, EncounterWillEnd={EncounterWillEnd}, ProjectedOutcome={ProjectedOutcome}",
            choice?.ChoiceID,
            projection.ProgressGained,
            projection.WillEncounterEnd,
            projection.ProjectedOutcome
        );

        BeatOutcome outcome = new BeatOutcome(
            projection.ProgressGained,
            projection.NarrativeDescription,
            projection.MechanicalDescription,
            projection.WillEncounterEnd,
            projection.ProjectedOutcome);

        return outcome;
    }

    public EncounterContext GetEncounterContext()
    {
        return context;
    }

    public ChoiceProjection ProjectChoice(ChoiceProjectionService choiceProjectionService,
        EncounterState encounterState, EncounterChoice choice)
    {
        ChoiceProjection projection = this.state.CreateChoiceProjection(choiceProjectionService, choice, playerState);
        projection.MechanicalDescription = choice.ChoiceID;
        return projection;
    }

    public List<EncounterChoice> GetCurrentChoices()
    {
        return CurrentChoices;
    }
}