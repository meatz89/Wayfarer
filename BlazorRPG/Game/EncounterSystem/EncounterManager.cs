public class EncounterManager
{
    public ActionImplementation ActionImplementation;

    private CardSelectionAlgorithm cardSelectionAlgorithm;
    public EncounterState encounterState;

    private NarrativeService narrativeService;
    private NarrativeContext narrativeContext;

    private ResourceManager resourceManager;

    public List<IChoice> CurrentChoices = new List<IChoice>();

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

        encounterState = new EncounterState(encounterInfo, playerState, this.resourceManager);
        encounterState.UpdateActiveTags(encounterInfo.AvailableTags);
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
                encounterInfo.Type,
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
        GenerateChoices();
        List<IChoice> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = choices.Select(ProjectChoice).ToList();

        // Create first narrative event
        NarrativeEvent firstNarrative = new NarrativeEvent(
            encounterState.CurrentTurn,
            introduction);

        narrativeContext.AddEvent(firstNarrative);

        // Generate choice descriptions
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = null;
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
        IChoice choice,
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
                new List<IChoice>(),
                new List<ChoiceProjection>(),
                new Dictionary<IChoice, ChoiceNarrative>(),
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
            GenerateChoices();
            List<IChoice> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = newChoices.Select(ProjectChoice).ToList();

            // Generate descriptive narratives for each choice
            Dictionary<IChoice, ChoiceNarrative> newChoiceDescriptions = null;

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
                foreach (KeyValuePair<IChoice, ChoiceNarrative> kvp in newChoiceDescriptions)
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

    private ChoiceOutcome ApplyChoiceProjection(PlayerState playerState, EncounterInfo encounterInfo, IChoice choice)
    {
        this.playerState = playerState;

        ChoiceProjection projection = encounterState.ApplyChoice(playerState, encounterInfo, choice);

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

        foreach (KeyValuePair<ApproachTags, int> kvp in projection.EncounterStateTagChanges)
        {
            outcome.EncounterStateTagChanges[kvp.Key] = kvp.Value;
        }

        outcome.NewlyActivatedTags.AddRange(projection.NewlyActivatedTags);
        outcome.DeactivatedTags.AddRange(projection.DeactivatedTags);

        return outcome;
    }


    private NarrativeEvent GetNarrativeEvent(
        IChoice choice,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        string narrative)
    {
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            encounterState.CurrentTurn - 1, // The turn counter increases after application
            narrative);

        narrativeEvent.SetChosenOption(choice);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);
        return narrativeEvent;
    }


    public EncounterStatusModel GetEncounterStatusModel(PlayerState playerState, WorldState worldState)
    {
        return new EncounterStatusModel(
            currentTurn: encounterState.CurrentTurn,
            maxMomentum: encounterState.Location.ExceptionalThreshold,
            maxPressure: encounterState.Location.MaxPressure,
            successThreshold: encounterState.Location.StandardThreshold,
            maxTurns: encounterState.Location.TurnDuration,
            momentum: encounterState.Momentum,
            pressure: encounterState.Pressure,
            approachTags: encounterState.TagSystem.GetAllApproachTags(),
            focusTags: encounterState.TagSystem.GetAllFocusTags(),
            activeTagNames: encounterState.GetActiveTagsNames(),
            playerState: playerState,
            worldState: worldState
        );
    }

    public NarrativeContext GetNarrativeContext()
    {
        return narrativeContext;
    }

    public ChoiceProjection ProjectChoice(IChoice choice)
    {
        ChoiceProjection projection = encounterState.CreateChoiceProjection(choice);
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
    public List<IChoice> GetCurrentChoices()
    {
        return CurrentChoices;
    }

    public void GenerateChoices()
    {
        CurrentChoices = cardSelectionAlgorithm.SelectChoices(encounterState);
    }
}