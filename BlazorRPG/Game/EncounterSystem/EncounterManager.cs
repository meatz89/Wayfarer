public class EncounterManager
{
    public ActionImplementation ActionImplementation;

    private CardSelectionAlgorithm cardSelectionAlgorithm;
    public EncounterState EncounterState;

    private NarrativeService narrativeService;
    private NarrativeContext narrativeContext;

    private ResourceManager resourceManager;

    public List<ChoiceCard> CurrentChoices = new List<ChoiceCard>();

    public PlayerState playerState;
    public WorldState worldState;
    public EncounterInfo encounterInfo;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;

    public EncounterManager(
        ActionImplementation actionImplementation,
        CardSelectionAlgorithm cardSelector,
        NarrativeService narrativeService,
        ResourceManager resourceManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        ActionImplementation = actionImplementation;
        cardSelectionAlgorithm = cardSelector;
        this.narrativeService = narrativeService;
        this.resourceManager = resourceManager;

        _useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    private void StartEncounter(WorldState worldState, PlayerState playerState, EncounterInfo encounterInfo)
    {
        this.worldState = worldState;
        this.playerState = playerState;

        EncounterState = new EncounterState(encounterInfo, playerState, this.resourceManager);
        EncounterState.UpdateActiveTags(encounterInfo.AvailableTags);
    }

    public async Task<NarrativeResult> StartEncounterWithNarrativeAsync(
        Location location,
        EncounterInfo encounterInfo,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation,
        AIProviderType providerType,
        WorldStateInput worldStateInput)
    {
        this.playerState = playerState;
        this.encounterInfo = encounterInfo;

        narrativeService.SwitchProvider(providerType);

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

            introduction = await narrativeService.GenerateIntroductionAsync(
                narrativeContext,
                status,
                memoryContent,
                worldStateInput);
        }

        // Get available choices
        GenerateChoicesForPlayer(playerState);
        List<ChoiceCard> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = choices.Select(ProjectChoice)
                                                    .ToList();

        // Create first narrative event
        NarrativeEvent firstNarrative = new NarrativeEvent(
            EncounterState.CurrentTurn,
            introduction);

        narrativeContext.AddEvent(firstNarrative);

        // Generate choice descriptions
        Dictionary<ChoiceCard, ChoiceNarrative> choiceDescriptions = null;
        if (_useAiNarrative)
        {
            choiceDescriptions = await narrativeService.GenerateChoiceDescriptionsAsync(
                narrativeContext,
                choices,
                projections,
                status,
                worldStateInput);

            firstNarrative.SetAvailableChoiceDescriptions(choiceDescriptions);
        }

        NarrativeResult result = new NarrativeResult(
            introduction,
            actionImplementation.Goal,
            choices,
            projections,
            choiceDescriptions,
            firstNarrative.ChoiceNarrative);

        // Return the narrative result
        return result;
    }

    public async Task<NarrativeResult> ApplyChoiceWithNarrativeAsync(
        ChoiceCard choice,
        PlayerState playerState,
        WorldState worldState,
        ChoiceNarrative choiceDescription,
        WorldStateInput worldStateInput)
    {
        // Apply the choice
        ChoiceOutcome outcome = ApplyChoiceProjection(playerState, encounterInfo, choice);

        // Get status after the choice
        EncounterStatusModel newStatus = GetEncounterStatusModel(playerState, worldState);

        // Generate narrative for the reaction and new scene
        string narrative = "Continued Narrative";

        // If the encounter is over, return the outcome
        if (outcome.IsEncounterOver)
        {
            if (_useAiNarrative)
            {
                narrative = await narrativeService.GenerateEndingAsync(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus,
                    worldStateInput);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            NarrativeResult currentResult = new(
                narrative,
                string.Empty,
                new List<ChoiceCard>(),
                new List<ChoiceProjection>(),
                new Dictionary<ChoiceCard, ChoiceNarrative>(),
                narrativeEvent.ChoiceNarrative);

            currentResult.SetOutcome(outcome.Outcome);
            currentResult.SetIsEncounterOver(outcome.IsEncounterOver);

            return currentResult;
        }
        else
        {
            if (_useAiNarrative)
            {
                narrative = await narrativeService.GenerateEncounterNarrative(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus,
                    worldStateInput);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            // Get the new choices and projections
            GenerateChoicesForPlayer(playerState);
            List<ChoiceCard> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = newChoices.Select(ProjectChoice).ToList();

            // Generate descriptive narratives for each choice
            Dictionary<ChoiceCard, ChoiceNarrative> newChoiceDescriptions = null;

            if (_useAiNarrative)
            {
                newChoiceDescriptions = await narrativeService.GenerateChoiceDescriptionsAsync(
                    narrativeContext,
                    newChoices,
                    newProjections,
                    newStatus,
                    worldStateInput);
            }

            // Add the choice descriptions to the latest event
            narrativeEvent.ChoiceDescriptions.Clear();
            if (newChoiceDescriptions != null)
            {
                foreach (KeyValuePair<ChoiceCard, ChoiceNarrative> kvp in newChoiceDescriptions)
                {
                    narrativeEvent.ChoiceDescriptions[kvp.Key] = kvp.Value;
                }
            }

            ChoiceNarrative lastChoiceNarrative = narrativeEvent.ChoiceNarrative;

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

    private ChoiceOutcome ApplyChoiceProjection(PlayerState playerState, EncounterInfo encounterInfo, ChoiceCard choice)
    {
        this.playerState = playerState;

        ChoiceProjection projection = EncounterState.ApplyChoice(playerState, encounterInfo, choice);

        ChoiceOutcome outcome = new ChoiceOutcome(
            projection.MomentumGained,
            projection.PressureBuilt,
            projection.NarrativeDescription,
            projection.EncounterWillEnd,
            projection.ProjectedOutcome,
            projection.HealthChange,
            projection.ConcentrationChange,
            projection.ConfidenceChange);

        foreach (KeyValuePair<FocusTags, int> kvp in projection.FocusTagChanges)
        {
            outcome.FocusTagChanges[kvp.Key] = kvp.Value;
        }

        foreach (KeyValuePair<ApproachTags, int> kvp in projection.ApproachTagChanges)
        {
            outcome.EncounterStateTagChanges[kvp.Key] = kvp.Value;
        }

        outcome.NewlyActivatedTags.AddRange(projection.NewlyActivatedTags);
        outcome.DeactivatedTags.AddRange(projection.DeactivatedTags);

        return outcome;
    }


    private NarrativeEvent GetNarrativeEvent(
        ChoiceCard choice,
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
            maxMomentum: EncounterState.Location.ExceptionalThreshold,
            maxPressure: EncounterState.Location.MaxPressure,
            successThreshold: EncounterState.Location.StandardThreshold,
            maxTurns: EncounterState.Location.TurnDuration,
            momentum: EncounterState.Momentum,
            pressure: EncounterState.Pressure,
            approachTags: EncounterState.TagSystem.GetAllApproachTags(),
            focusTags: EncounterState.TagSystem.GetAllFocusTags(),
            activeTagNames: EncounterState.GetActiveTagsNames(),
            playerState: playerState,
            worldState: worldState
        );
    }

    public NarrativeContext GetNarrativeContext()
    {
        return narrativeContext;
    }

    public ChoiceProjection ProjectChoice(ChoiceCard choice)
    {
        ChoiceProjection projection = EncounterState.CreateChoiceProjection(choice);
        projection.NarrativeDescription = choice.Name + " " + choice.Description;
        return projection;
    }

    public void SwitchAIProvider(AIProviderType providerType)
    {
        if (narrativeService != null)
        {
            narrativeService.SwitchProvider(providerType);
        }
    }

    public AIProviderType GetCurrentAIProvider()
    {
        return narrativeService?.CurrentProvider ?? AIProviderType.OpenAI;
    }

    public string GetCurrentAIProviderName()
    {
        return narrativeService?.GetCurrentProviderName() ?? "None";
    }

    // Existing methods remain the same
    public List<ChoiceCard> GetCurrentChoices()
    {
        return CurrentChoices;
    }

    public void GenerateChoicesForPlayer(PlayerState playerState)
    {
        CurrentChoices = cardSelectionAlgorithm.SelectChoices(EncounterState, playerState);
    }
}