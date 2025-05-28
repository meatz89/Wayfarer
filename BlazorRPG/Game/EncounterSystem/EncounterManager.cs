public class EncounterManager
{
    public Encounter Encounter;
    private EncounterContext context;
    public EncounterState state;
    private IAIService aiGameMaster;
    private PayloadProcessor payloadProcessor;
    public LocationAction locationAction;
    private WorldStateInputBuilder worldStateInputCreator;
    private ChoiceProjectionService projectionService;
    public List<EncounterChoice> CurrentChoices = new List<EncounterChoice>();

    public Player player;
    public WorldState worldState;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;

    public EncounterResult EncounterResult;

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
        this.Encounter = encounter;
        this.locationAction = locationAction;
        this.aiGameMaster = aiGameMaster;
        this.worldStateInputCreator = worldStateInputCreator;
        this.projectionService = choiceProjectionService;
        _useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
        _useMemory = configuration.GetValue<bool>("useMemory");
        _logger = logger as ILogger<EncounterManager> ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EncounterManager>();
    }

    public async Task InitializeEncounter(EncounterContext encounterContext, Player player)
    {
        this.context = encounterContext;
        this.player = player;

        state = new EncounterState(player, 6, 8, "Narrative Begins");
        state.MaxFocusPoints = DetermineFocusPoints(encounterContext.SkillCategory);
        state.FocusPoints = state.MaxFocusPoints;
        state.MaxDuration = 8;
        state.DurationCounter = 0;
        state.IsEncounterComplete = false;
        state.GoalFlags = DetermineGoalFlags(encounterContext.SkillCategory);
        state.FlagManager = new EncounterFlagManager();

        await GenerateInitialChoices();

        IsInitialState = true;
    }

    public async Task GenerateInitialChoices()
    {
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(Encounter.LocationName);

        string narrative = await aiGameMaster.GenerateIntroduction(
            context,
            state,
            null, // No chosen option for initial state
            worldStateInput,
            AIClient.PRIORITY_IMMEDIATE
        );

        List<EncounterChoice> generatedChoices = await aiGameMaster.GenerateChoices(
            context,
            state,
            null, // No chosen option for initial state
            worldStateInput,
            AIClient.PRIORITY_IMMEDIATE
        );

        CurrentChoices = generatedChoices;

        EncounterResult = new EncounterResult
        {
            locationAction = locationAction,
            ActionResult = ActionResults.Ongoing,
            EncounterContext = context,
            AIResponse = new BeatResponse
            {
                SceneNarrative = narrative,
                AvailableChoices = generatedChoices
            }
        };

        _logger.LogInformation($"Generated {CurrentChoices.Count} initial AI choices for encounter");
    }

    public async Task<EncounterResult> ProcessPlayerChoice(EncounterChoice choice)
    {
        if (state.IsEncounterComplete)
        {
            return EncounterResult;
        }

        _logger.LogInformation($"Processing player choice: {choice.ChoiceID}");

        // Create chosen option from selected choice
        PlayerChoiceSelection chosenOption = new PlayerChoiceSelection
        {
            Choice = choice,
            SelectedOption = choice.SkillOption
        };

        // Apply choice effects
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(Encounter.LocationName);
        BeatOutcome outcome = await ApplyChoiceProjection(player, state, choice);

        // Check if encounter is complete
        if (outcome.IsEncounterComplete)
        {
            string concludingNarrative = "The encounter has concluded.";

            if (_useAiNarrative)
            {
                concludingNarrative = await aiGameMaster.GenerateConclusion(
                    context,
                    state,
                    choice,
                    outcome,
                    worldStateInput,
                    AIClient.PRIORITY_IMMEDIATE
                );
            }

            // Create final result
            EncounterResult = new EncounterResult
            {
                locationAction = locationAction,
                ActionResult = outcome.Outcome == BeatOutcomes.Success ? ActionResults.EncounterSuccess : ActionResults.EncounterFailure,
                EncounterEndMessage = concludingNarrative,
                EncounterContext = context,
                AIResponse = new BeatResponse
                {
                    SceneNarrative = concludingNarrative,
                    AvailableChoices = new List<EncounterChoice>(), // No choices after conclusion
                }
            };

            // Apply any persistent changes
            //await ProcessEncounterConclusion();

            return EncounterResult;
        }
        else
        {
            // Continue the encounter with new choices
            string reactionNarrative = "The situation continues to unfold.";

            reactionNarrative = await aiGameMaster.GenerateReaction(
                context,
                state,
                choice,
                outcome,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE
            );

            // Generate new choices
            List<EncounterChoice> newChoices = await aiGameMaster.GenerateChoices(
                context,
                state,
                chosenOption,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE
            );

            CurrentChoices = newChoices;
            
            // Update encounter result
            EncounterResult = new EncounterResult
            {
                locationAction = locationAction,
                ActionResult = ActionResults.Ongoing,
                EncounterContext = context,
                AIResponse = new BeatResponse
                {
                    SceneNarrative = reactionNarrative,
                    AvailableChoices = CurrentChoices
                }
            };

            return EncounterResult;
        }
    }

    public async Task<BeatOutcome> ApplyChoiceProjection(
        Player playerState, 
        EncounterState encounterState, 
        EncounterChoice choice)
    {
        this.player = playerState;

        // Get the projection for this choice
        ChoiceProjection projection = encounterState.ApplyChoice(projectionService, playerState, encounterState, choice);

        // Log details of the projection
        _logger.LogInformation(
            "ApplyChoiceProjection: ChoiceId={ChoiceId}, ProgressGained={ProgressGained}, EncounterWillEnd={EncounterWillEnd}, ProjectedOutcome={ProjectedOutcome}",
            choice?.ChoiceID,
            projection.ProgressGained,
            projection.WillEncounterEnd,
            projection.ProjectedOutcome
        );

        // Apply focus cost
        state.FocusPoints -= choice.FocusCost;

        // Apply payload effects
        if (projection.HasSkillCheck)
        {
            if (payloadProcessor != null && !string.IsNullOrEmpty(choice.SkillOption.SuccessPayload.ID))
            {
                payloadProcessor.ApplyPayload(choice.SkillOption.SuccessPayload.ID, state);
            }
        }
        else
        {
            if (payloadProcessor != null && !string.IsNullOrEmpty(choice.SkillOption.FailurePayload.ID))
            {
                payloadProcessor.ApplyPayload(choice.SkillOption.FailurePayload.ID, state);
            }
        }

        // Advance duration
        state.DurationCounter++;

        // Check for completion conditions
        CheckEncounterCompletion(state);

        return new BeatOutcome()
        {
            ProgressGained = projection.ProgressGained,
            NarrativeDescription = projection.NarrativeDescription,
            MechanicalDescription = projection.MechanicalDescription,
            IsEncounterComplete = projection.WillEncounterEnd || state.IsEncounterComplete,
            Outcome = projection.ProjectedOutcome
        };
    }

    private void CheckEncounterCompletion(EncounterState state)
    {
        // Check if all goal flags are active
        bool allGoalsComplete = true;
        foreach (FlagStates goalFlag in state.GoalFlags)
        {
            if (!state.FlagManager.IsActive(goalFlag))
            {
                allGoalsComplete = false;
                break;
            }
        }

        if (allGoalsComplete)
        {
            state.IsEncounterComplete = true;
            state.EncounterOutcome = BeatOutcomes.Success;
        }
        else if (state.FocusPoints <= 0 || state.DurationCounter >= state.MaxDuration)
        {
            state.IsEncounterComplete = true;
            state.EncounterOutcome = BeatOutcomes.Failure;
        }
    }

    private List<FlagStates> DetermineGoalFlags(ActionTypes actionType)
    {
        List<FlagStates> goalFlags = new List<FlagStates>();

        switch (actionType)
        {
            case ActionTypes.Physical:
                goalFlags.Add(FlagStates.PathCleared);
                break;
            case ActionTypes.Intellectual:
                goalFlags.Add(FlagStates.InsightGained);
                break;
            case ActionTypes.Social:
                goalFlags.Add(FlagStates.TrustEstablished);
                break;
            default:
                goalFlags.Add(FlagStates.PathCleared);
                break;
        }

        return goalFlags;
    }

    private int DetermineFocusPoints(ActionTypes actionType)
    {
        // Base focus points for different action types
        switch (actionType)
        {
            case ActionTypes.Physical:
                return 7;
            case ActionTypes.Intellectual:
                return 8;
            case ActionTypes.Social:
                return 6;
            default:
                return 6;
        }
    }

    public ChoiceProjection ProjectChoice(ChoiceProjectionService choiceProjectionService,
        EncounterState encounterState, EncounterChoice choice)
    {
        ChoiceProjection projection = this.state.CreateChoiceProjection(choiceProjectionService, choice, player);
        projection.MechanicalDescription = choice.ChoiceID;
        return projection;
    }

    public EncounterContext GetEncounterContext()
    {
        return context;
    }

    public List<EncounterChoice> GetCurrentChoices()
    {
        return CurrentChoices;
    }
}