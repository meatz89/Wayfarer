public class EncounterManager
{
    private readonly Encounter encounter;
    public ActionImplementation ActionImplementation;

    public EncounterState encounterState;

    private IAIService _aiService;
    private NarrativeContext narrativeContext;

    private readonly WorldStateInputBuilder worldStateInputCreator;
    private readonly ChoiceProjectionService choiceProjectionService;
    public List<AiChoice> CurrentChoices = new List<AiChoice>();

    public Player playerState;
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
        IAIService aiService,
        WorldStateInputBuilder worldStateInputCreator,
        ChoiceProjectionService choiceProjectionService,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.encounter = encounter;
        ActionImplementation = actionImplementation;
        _aiService = aiService;
        this.worldStateInputCreator = worldStateInputCreator;
        this.choiceProjectionService = choiceProjectionService;
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

    public async Task<NarrativeResult> StartEncounterWithNarrativeAsync(
    Location location,
    Encounter encounterInfo,
    WorldState worldState,
    Player playerState,
    ActionImplementation actionImplementation)
    {
        _logger.LogInformation("StartEncounterWithNarrativeAsync called for location: {LocationId}, encounter: {EncounterId}", location?.Id, encounterInfo?.Id);
        this.playerState = playerState;
        this.Encounter = encounterInfo;

        SkillCategories skillCategory = encounter.Approach.RequiredCardType;
        StartEncounter(worldState, playerState, skillCategory);

        narrativeContext =
            new NarrativeContext(
                encounterInfo.LocationName.ToString(),
                encounterInfo.LocationSpotName.ToString(),
                encounterInfo.SkillCategory,
                playerState,
                actionImplementation,
                encounter.Approach);

        string introduction = await CreateIntroduction(location);

        await GenerateChoicesForPlayer(playerState);

        NarrativeResult result = await CreateChoices(location, actionImplementation, introduction);

        _logger.LogInformation("StartEncounterWithNarrativeAsync completed for encounter: {EncounterId}", encounterInfo?.Id);
        return result;
    }


    private async Task GenerateChoicesForPlayer(Player playerState)
    {
        if (_useAiNarrative)
        {
            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(Encounter.LocationName);
            List<AiChoice> generatedChoices = await _aiService.GenerateEncounterChoicesAsync(
                narrativeContext,
                encounterState,
                worldStateInput,
                AIClient.PRIORITY_IMMEDIATE);

            // Update choice IDs to ensure uniqueness
            for (int i = 0; i < generatedChoices.Count; i++)
            {
                AiChoice choice = generatedChoices[i];
            }

            CurrentChoices = generatedChoices;
            _logger.LogInformation($"Generated {CurrentChoices.Count} AI choices for encounter stage {encounterState.DurationCounter}");
        }
        else
        {
            // Fallback to hardcoded choices if AI narrative is disabled
            GenerateFallbackChoices();
        }
    }

    private void GenerateFallbackChoices()
    {
        // Create basic fallback choices
        CurrentChoices = new List<AiChoice>
    {
        CreateFallbackChoice(0, "Proceed carefully", 1, "Observation"),
        CreateFallbackChoice(1, "Take aggressive action", 2, "Brute Force"),
        CreateFallbackChoice(2, "Try diplomatic approach", 1, "Negotiation"),
        CreateFallbackChoice(3, "Gather more information", 1, "Investigation"),
        CreateFallbackChoice(4, "Take a moment to recover", 0, "Perception")
    };
    }

    private AiChoice CreateFallbackChoice(int index, string narrativeText, int focusCost, string skillName)
    {
        return new AiChoice
        {
            ChoiceID = $"fallback_choice_{index}",
            NarrativeText = narrativeText,
            FocusCost = focusCost,
            SkillOptions = new List<SkillOption>
        {
            new SkillOption
            {
                SkillName = skillName,
                Difficulty = "Standard",
                SCD = 3,
                SuccessPayload = new Payload
                {
                    NarrativeEffect = "You succeed in your attempt.",
                    MechanicalEffectID = focusCost == 0 ? "GAIN_FOCUS_1" : "SET_FLAG_INSIGHT_GAINED"
                },
                FailurePayload = new Payload
                {
                    NarrativeEffect = "You encounter a setback.",
                    MechanicalEffectID = "ADVANCE_DURATION_1"
                }
            }
        }
        };
    }

    private async Task<NarrativeResult> CreateChoices(
        Location location,
        ActionImplementation actionImplementation,
        string introduction)
    {
        List<AiChoice> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = new List<ChoiceProjection>();
        foreach (AiChoice choice in choices)
        {
            projections.Add(ProjectChoice(choiceProjectionService, encounterState, choice));
        }

        NarrativeEvent firstNarrative = new NarrativeEvent(
            encounterState.DurationCounter,
            introduction);

        narrativeContext.AddEvent(firstNarrative);

        // Set the available choices on the narrative event
        firstNarrative.SetAvailableChoices(choices);

        NarrativeResult result = new NarrativeResult(
            introduction,
            actionImplementation.Description,
            choices,
            projections);

        return result;
    }

    private async Task<string> CreateIntroduction(Location location)
    {
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

        return introduction;
    }

    private void StartEncounter(WorldState worldState, Player player, SkillCategories skillCategory)
    {
        this.worldState = worldState;
        this.playerState = player;

        encounterState = new EncounterState(player, skillCategory);
    }

    public async Task<NarrativeResult> ApplyChoiceWithNarrativeAsync(
        string location,
        AiChoice choice,
        int priority)
    {
        _logger.LogInformation("ApplyChoiceWithNarrativeAsync called with location: {Location}, choice: {ChoiceId}, priority: {Priority}", location, choice?.ChoiceID, priority);
        ChoiceOutcome outcome = ApplyChoiceProjection(playerState, encounterState, choice);

        string narrative = "Continued Narrative";

        if (outcome.IsEncounterOver)
        {
            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                narrative = await _aiService.GenerateEndingAsync(
                    narrativeContext,
                    choice,
                    outcome,
                    worldStateInput,
                    priority);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            NarrativeResult currentResult = new(
                narrative,
                string.Empty,
                new List<AiChoice>(),
                new List<ChoiceProjection>());

            currentResult.SetOutcome(outcome.Outcome);
            currentResult.SetIsEncounterOver(outcome.IsEncounterOver);

            _logger.LogInformation("ApplyChoiceWithNarrativeAsync completed: Encounter is over.");
            return currentResult;
        }
        else
        {
            if (_useAiNarrative)
            {
                WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(location);
                narrative = await _aiService.GenerateReactionAsync(
                    narrativeContext,
                    encounterState,
                    choice,
                    outcome,
                    worldStateInput,
                    priority);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            await GenerateChoicesForPlayer(playerState);
            List<AiChoice> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = new List<ChoiceProjection>();
            foreach (var newChoice in newChoices)
            {
                newProjections.Add(ProjectChoice(choiceProjectionService, encounterState, newChoice));
            }

            // Set the available choices on the narrative event
            narrativeEvent.SetAvailableChoices(newChoices);

            NarrativeResult ongoingResult = new(
                narrative,
                string.Empty,
                newChoices,
                newProjections);

            _logger.LogInformation("ApplyChoiceWithNarrativeAsync completed: Encounter continues.");
            return ongoingResult;
        }
    }

    private ChoiceOutcome ApplyChoiceProjection(Player playerState, EncounterState encounterState, AiChoice choice)
    {
        this.playerState = playerState;

        ChoiceProjection projection = encounterState.ApplyChoice(choiceProjectionService, playerState, Encounter, choice);

        // Log details of the projection
        _logger.LogInformation(
            "ApplyChoiceProjection: ChoiceId={ChoiceId}, ProgressGained={ProgressGained}, EncounterWillEnd={EncounterWillEnd}, ProjectedOutcome={ProjectedOutcome}",
            choice?.ChoiceID,
            projection.ProgressGained,
            projection.WillEncounterEnd,
            projection.ProjectedOutcome
        );

        ChoiceOutcome outcome = new ChoiceOutcome(
            projection.ProgressGained,
            projection.NarrativeDescription,
            projection.MechanicalDescription,
            projection.WillEncounterEnd,
            projection.ProjectedOutcome);

        return outcome;
    }

    private NarrativeEvent GetNarrativeEvent(
        AiChoice choice,
        ChoiceOutcome outcome,
        string narrative)
    {
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            encounterState.DurationCounter - 1,
            narrative);

        narrativeEvent.SetChosenOption(choice);
        narrativeEvent.SetOutcome(outcome.MechanicalDescription);
        return narrativeEvent;
    }

    public NarrativeContext GetNarrativeContext()
    {
        return narrativeContext;
    }

    public ChoiceProjection ProjectChoice(ChoiceProjectionService choiceProjectionService,
        EncounterState encounterState, AiChoice choice)
    {
        ChoiceProjection projection = this.encounterState.CreateChoiceProjection(choiceProjectionService, choice, playerState);
        projection.MechanicalDescription = choice.ChoiceID;
        return projection;
    }

    public List<AiChoice> GetCurrentChoices()
    {
        return CurrentChoices;
    }

}