public class EncounterManager
{
    private EncounterContext context;
    public EncounterState state;

    public LocationAction locationAction;
    public List<EncounterChoice> CurrentChoices = new List<EncounterChoice>();

    public Player player;
    public WorldState worldState;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;

    public EncounterResult EncounterResult;

    public bool IsInitialState { get; set; }

    private ILogger<EncounterManager> _logger;

    public EncounterManager(
        EncounterContext encounterContext,
        EncounterState encounterState,
        LocationAction locationAction)
    {
        this.context = encounterContext;
        this.state = encounterState;
        this.locationAction = locationAction;

        _useAiNarrative = true;
        _useMemory = true;
    }

    public async Task Initialize()
    {
        state = new EncounterState(player, 6, 8, "Narrative Begins");
        state.MaxFocusPoints = DetermineFocusPoints(context.SkillCategory);
        state.FocusPoints = state.MaxFocusPoints;
        state.MaxDuration = 8;
        state.DurationCounter = 0;
        state.IsEncounterComplete = false;
        state.GoalFlags = DetermineGoalFlags(context.SkillCategory);
        state.FlagManager = new EncounterFlagManager();

        await GenerateInitialChoices();

        IsInitialState = true;
    }

    public async Task GenerateInitialChoices()
    {
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(context.LocationName);

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
            AIResponse = new AIResponse
            {
                BeatNarration = narrative,
                Choices = generatedChoices
            }
        };

        _logger.LogInformation($"Generated {CurrentChoices.Count} initial AI choices for encounter");
    }

    public async Task<EncounterResult> ProcessPlayerChoice(
        EncounterChoice choice)
    {
        logger.LogInformation("ExecuteChoice called for choice: {ChoiceId}", choice?.ChoiceID);
        AIResponse currentNarrative = AIResponse;
        AIResponse cachedResult = null;

        EncounterManager encounterManager = GetEncounterManager();
        bool isInitialChoice = encounterManager.IsInitialState;

        _preGenerationManager.CancelAllPendingGenerations();

        List<EncounterChoice> choices = currentNarrative.Choices;

        currentNarrative = await encounterManager.ProcessPlayerChoice(
            encounterManager.context.LocationName,
            choice,
            AIClient.PRIORITY_IMMEDIATE);

        encounterManager.IsInitialState = false;
        encounterManager.EncounterResult = CreateEncounterResult(encounterManager, currentNarrative);

        logger.LogInformation("Choice executed: {ChoiceId}, EncounterOver: {IsEncounterOver}", choice?.ChoiceID, currentNarrative.IsEncounterOver);
        return encounterManager.EncounterResult;
    }


    public async Task<EncounterResult> ProcessPlayerChoice(GameWorld gameWorld, EncounterChoice choice)
    {

        if (gameWorld.CurrentEncounter == null || gameWorld.CurrentAIResponse == null)
        {
            return; // No active encounter or no AI response
        }

        // Find selected choice
        EncounterChoice selectedChoice = null;
        foreach (EncounterChoice choice in gameWorld.CurrentAIResponse.Choices)
        {
            if (choice.ChoiceID == choiceId)
            {
                selectedChoice = choice;
                break;
            }
        }

        if (selectedChoice == null)
        {
            return; // Choice not found
        }

        // Find the template used by the AI
        ChoiceTemplate template = FindTemplateByName(selectedChoice.TemplateUsed);
        if (template == null)
        {
            return; // Template not found
        }

        // Process focus cost
        gameWorld.CurrentEncounter.FocusPoints -= selectedChoice.FocusCost;

        // Perform skill check
        bool success = PerformSkillCheck(selectedChoice.SkillCheck);

        // Apply mechanical effect directly from template
        if (success)
        {
            // Begin streaming success narrative
            gameWorld.StreamingContentState.BeginStreaming(selectedChoice.SuccessNarrative);

            // Create and apply the success effect (direct instantiation)
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.SuccessEffectClass);
            effect.Apply(gameWorld.CurrentEncounter);
        }
        else
        {
            // Begin streaming failure narrative
            gameWorld.StreamingContentState.BeginStreaming(selectedChoice.FailureNarrative);

            // Create and apply the failure effect (direct instantiation)
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.FailureEffectClass);
            effect.Apply(gameWorld.CurrentEncounter);
        }

        // Update encounter state
        gameWorld.CurrentEncounter.AdvanceDuration(1);
        gameWorld.CurrentEncounter.ProcessModifiers();
        gameWorld.CurrentEncounter.CheckGoalCompletion();

        // Clear AI response while streaming occurs
        gameWorld.CurrentAIResponse = null;

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
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(context.LocationName);
        BeatOutcome outcome = await ApplyChoiceProjection(player, state, choice);

        // Check if encounterContext is complete
        if (outcome.IsEncounterComplete)
        {
            string concludingNarrative = "The encounterContext has concluded.";

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
                ActionResult = outcome.Outcome == EncounterStageOutcomes.Success ? ActionResults.EncounterSuccess : ActionResults.EncounterFailure,
                EncounterEndMessage = concludingNarrative,
                EncounterContext = context,
                AIResponse = new AIResponse
                {
                    BeatNarration = concludingNarrative,
                    Choices = new List<EncounterChoice>(), // No choices after conclusion
                }
            };

            // Apply any persistent changes
            //await ProcessEncounterConclusion();

            return EncounterResult;
        }
        else
        {
            // Continue the encounterContext with new choices
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

            // Update encounterContext result
            EncounterResult = new EncounterResult
            {
                locationAction = locationAction,
                ActionResult = ActionResults.Ongoing,
                EncounterContext = context,
                AIResponse = new AIResponse
                {
                    BeatNarration = reactionNarrative,
                    Choices = CurrentChoices
                }
            };

            return EncounterResult;
        }
    }

    private EncounterResult CreateEncounterResult(EncounterManager encounterContext, AIResponse aiResponse)
    {
        if (aiResponse.IsEncounterOver)
        {
            if (aiResponse.Outcome == EncounterStageOutcomes.Failure)
            {
                EncounterResult failureResult = new EncounterResult()
                {
                    locationAction = encounterContext.locationAction,
                    ActionResult = ActionResults.EncounterFailure,
                    EncounterEndMessage = $"=== EncounterContext Over: {aiResponse.Outcome} ===",
                    AIResponse = aiResponse,
                    EncounterContext = encounterContext.GetEncounterContext()
                };
                return failureResult;
            }
            else
            {
                EncounterResult successResult = new EncounterResult()
                {
                    locationAction = encounterContext.locationAction,
                    ActionResult = ActionResults.EncounterSuccess,
                    EncounterEndMessage = $"=== EncounterContext Over: {aiResponse.Outcome} ===",
                    AIResponse = aiResponse,
                    EncounterContext = encounterContext.GetEncounterContext()
                };
                return successResult;
            }
        }

        EncounterResult ongoingResult = new EncounterResult()
        {
            locationAction = encounterContext.locationAction,
            ActionResult = ActionResults.Ongoing,
            EncounterEndMessage = "",
            AIResponse = aiResponse,
            EncounterContext = encounterContext.GetEncounterContext()
        };

        return ongoingResult;
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

        // Apply effect effects
        if (projection.HasSkillCheck)
        {
            if (effectProcessor != null && !string.IsNullOrEmpty(choice.SkillOption.SuccessEffect.ID))
            {
                effectProcessor.ApplyEffect(choice.SkillOption.SuccessEffect.ID, state);
            }
        }
        else
        {
            if (effectProcessor != null && !string.IsNullOrEmpty(choice.SkillOption.FailureEffect.ID))
            {
                effectProcessor.ApplyEffect(choice.SkillOption.FailureEffect.ID, state);
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
            state.EncounterOutcome = EncounterStageOutcomes.Success;
        }
        else if (state.FocusPoints <= 0 || state.DurationCounter >= state.MaxDuration)
        {
            state.IsEncounterComplete = true;
            state.EncounterOutcome = EncounterStageOutcomes.Failure;
        }
    }

    private List<FlagStates> DetermineGoalFlags(SkillCategories actionType)
    {
        List<FlagStates> goalFlags = new List<FlagStates>();

        switch (actionType)
        {
            case SkillCategories.Physical:
                goalFlags.Add(FlagStates.PathCleared);
                break;
            case SkillCategories.Intellectual:
                goalFlags.Add(FlagStates.InsightGained);
                break;
            case SkillCategories.Social:
                goalFlags.Add(FlagStates.TrustEstablished);
                break;
            default:
                goalFlags.Add(FlagStates.PathCleared);
                break;
        }

        return goalFlags;
    }

    private int DetermineFocusPoints(SkillCategories actionType)
    {
        // Base focus points for different action types
        switch (actionType)
        {
            case SkillCategories.Physical:
                return 7;
            case SkillCategories.Intellectual:
                return 8;
            case SkillCategories.Social:
                return 6;
            default:
                return 6;
        }
    }

    public ChoiceProjection GetChoiceProjection(ChoiceProjectionService choiceProjectionService,
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