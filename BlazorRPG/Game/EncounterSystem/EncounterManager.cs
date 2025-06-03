public class EncounterManager
{
    private EncounterContext context;
    private EncounterState state;
    private AIGameMaster aiGameMaster;
    private ChoiceProjectionService choiceProjectionService;
    private StreamingContentState streamingState;
    private bool isAwaitingAIResponse = false;

    public LocationAction locationAction;
    public List<EncounterChoice> Choices = new List<EncounterChoice>();
    private List<ChoiceProjection> currentChoiceProjections = new List<ChoiceProjection>();

    public Player player;
    public WorldState worldState;
    public EncounterResult EncounterResult;
    public bool IsInitialState { get; set; }
    private bool _useAiNarrative = false;
    private bool _useMemory = false;
    private ILogger<EncounterManager> _logger;
    private List<ChoiceTemplate> allTemplates;

    public EncounterManager(
        EncounterContext encounterContext,
        EncounterState encounterState,
        LocationAction locationAction,
        ChoiceProjectionService choiceProjectionService,
        AIGameMaster aiGameMaster)
    {
        this.context = encounterContext;
        this.state = encounterState;
        this.locationAction = locationAction;
        this.choiceProjectionService = choiceProjectionService;
        this.aiGameMaster = aiGameMaster;
        _useAiNarrative = true;
        _useMemory = true;
        allTemplates = ChoiceTemplateLibrary.GetAllTemplates();
    }

    public async Task InitializeEncounter()
    {
        state = new EncounterState(player, 6, 8, "Narrative Begins");
        state.MaxFocusPoints = DetermineFocusPoints(context.SkillCategory);
        state.FocusPoints = state.MaxFocusPoints;
        state.MaxDuration = 8;
        state.DurationCounter = 0;
        state.IsEncounterComplete = false;
        state.GoalFlags = DetermineGoalFlags(context.SkillCategory);
        state.FlagManager = new EncounterFlagManager();

        IsInitialState = true;
    }

    public async Task<EncounterResult> ProcessPlayerChoice(GameWorld gameWorld, EncounterChoice choice)
    {
        if (gameWorld.CurrentEncounterManager == null || gameWorld.CurrentAIResponse == null)
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
        gameWorld.CurrentEncounterManager.FocusPoints -= selectedChoice.FocusCost;

        // Perform skill check
        bool success = PerformSkillCheck(selectedChoice.SkillCheck);

        // Apply mechanical effect directly from template
        if (success)
        {
            // Begin streaming success narrative
            gameWorld.StreamingContentState.BeginStreaming(selectedChoice.SuccessNarrative);

            // Create and apply the success effect (direct instantiation)
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.SuccessEffectClass);
            effect.Apply(gameWorld.CurrentEncounterManager);
        }
        else
        {
            // Begin streaming failure narrative
            gameWorld.StreamingContentState.BeginStreaming(selectedChoice.FailureNarrative);

            // Create and apply the failure effect (direct instantiation)
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.FailureEffectClass);
            effect.Apply(gameWorld.CurrentEncounterManager);
        }

        // Update encounter state
        gameWorld.CurrentEncounterManager.AdvanceDuration(1);
        gameWorld.CurrentEncounterManager.ProcessModifiers();
        gameWorld.CurrentEncounterManager.CheckGoalCompletion();

        // Clear AI response while streaming occurs
        gameWorld.CurrentAIResponse = null;

    }

    private bool PerformSkillCheck(object skillCheck)
    {
        return true; // Placeholder for actual skill check logic
    }

    private ChoiceTemplate FindTemplateByName(string templateName)
    {
        foreach (ChoiceTemplate template in allTemplates)
        {
            if (template.TemplateName == templateName)
            {
                return template;
            }
        }

        return null;
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

    public ChoiceProjection GetChoiceProjection(EncounterState encounterState, EncounterChoice choice)
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
        return Choices;
    }

    public EncounterState GetEncounterState()
    {
        return state;
    }

    internal StreamingContentState GetStreamingState()
    {
        throw new NotImplementedException();
    }
}