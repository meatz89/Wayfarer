public class EncounterManager
{
    public ActionImplementation ActionImplementation;
    private readonly CardSelectionAlgorithm _cardSelector;
    public EncounterState State;

    private NarrativeService _narrativeService;
    private NarrativeContext _narrativeContext;

    public List<IChoice> CurrentChoices = new List<IChoice>();

    private bool _useAiNarrative = false;
    private bool _useMemory = false;
    public EncounterManager(
        ActionImplementation actionImplementation,
        CardSelectionAlgorithm cardSelector,
        NarrativeService narrativeService,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        ActionImplementation = actionImplementation;
        _cardSelector = cardSelector;
        _narrativeService = narrativeService;

        _useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    // Add methods to control AI provider selection
    public void SwitchAIProvider(AIProviderType providerType)
    {
        if (_narrativeService != null)
        {
            _narrativeService.SwitchProvider(providerType);
        }
    }

    public AIProviderType GetCurrentAIProvider()
    {
        return _narrativeService?.CurrentProvider ?? AIProviderType.OpenAI;
    }

    public string GetCurrentAIProviderName()
    {
        return _narrativeService?.GetCurrentProviderName() ?? "None";
    }

    // Existing methods remain the same
    public List<IChoice> GetCurrentChoices()
    {
        return CurrentChoices;
    }

    public void GenerateChoices()
    {
        CurrentChoices = _cardSelector.SelectChoices(State);
    }

    private ChoiceOutcome ApplyChoiceProjection(IChoice choice)
    {
        ChoiceProjection projection = State.ApplyChoice(choice);

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

    public EncounterStatusModel GetEncounterStatusModel()
    {
        return new EncounterStatusModel(
            currentTurn: State.CurrentTurn,
            maxMomentum: State.Location.ExceptionalThreshold,
            maxPressure: State.Location.MaxPressure,
            successThreshold: State.Location.StandardThreshold,
            maxTurns: State.Location.TurnDuration,
            momentum: State.Momentum,
            pressure: State.Pressure,
            health: State.PlayerState.Health,
            maxHealth: State.PlayerState.MaxHealth,
            concentration: State.PlayerState.Concentration,
            maxConcentration: State.PlayerState.MaxConcentration,
            confidence: State.PlayerState.Confidence,
            maxConfidence: State.PlayerState.MaxConfidence,
            approachTags: State.TagSystem.GetAllApproachTags(),
            focusTags: State.TagSystem.GetAllFocusTags(),
            activeTagNames: State.GetActiveTagsNames()
        );
    }

    public NarrativeContext GetNarrativeContext()
    {
        return _narrativeContext;
    }

    public ChoiceProjection ProjectChoice(IChoice choice)
    {
        ChoiceProjection projection = State.CreateChoiceProjection(choice);
        projection.NarrativeDescription = choice.Name + " " + choice.Description;
        return projection;
    }

    private void StartEncounter(EncounterInfo encounterInfo, PlayerState playerState)
    {
        State = new EncounterState(encounterInfo, playerState);
        State.UpdateActiveTags(encounterInfo.AvailableTags);
    }

    public async Task<NarrativeResult> StartEncounterWithNarrativeAsync(
        Location location,
        EncounterInfo encounterInfo,
        PlayerState playerState,
        ActionImplementation incitingAction,
        AIProviderType providerType)
    {
        if (_narrativeService != null)
        {
            _narrativeService.SwitchProvider(providerType);
        }

        // Start the encounter mechanically
        StartEncounter(encounterInfo, playerState);

        // Create narrative context
        _narrativeContext =
            new NarrativeContext(
                encounterInfo.LocationName.ToString(),
                encounterInfo.LocationSpotName.ToString(),
                encounterInfo.EncounterType,
                incitingAction);

        // Generate introduction
        EncounterStatusModel status = GetEncounterStatusModel();

        string introduction = "introduction";
        string memoryContent = string.Empty;
        if (_useMemory)
        {
            memoryContent = await MemoryFileAccess.ReadFromMemoryFile();
        }

        if (_useAiNarrative && _narrativeService != null)
        {

            introduction = await _narrativeService.GenerateIntroductionAsync(
                _narrativeContext,
                status,
                memoryContent);
        }

        // Get available choices
        GenerateChoices();
        List<IChoice> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = choices.Select(ProjectChoice).ToList();

        // Create first narrative event
        NarrativeEvent firstNarrative = new NarrativeEvent(
            State.CurrentTurn,
            introduction);

        _narrativeContext.AddEvent(firstNarrative);

        // Generate choice descriptions
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = null;
        if (_useAiNarrative && _narrativeService != null)
        {
            choiceDescriptions = await _narrativeService.GenerateChoiceDescriptionsAsync(
                _narrativeContext,
                choices,
                projections,
                status);

            firstNarrative.SetAvailableChoiceDescriptions(choiceDescriptions);
        }

        // Return the narrative result
        return new NarrativeResult(
            introduction,
            choices,
            projections,
            choiceDescriptions,
            firstNarrative.ChoiceNarrative);
    }

    public async Task<NarrativeResult> ApplyChoiceWithNarrativeAsync(
        IChoice choice,
        ChoiceNarrative choiceDescription)
    {
        // Apply the choice
        ChoiceOutcome outcome = ApplyChoiceProjection(choice);

        // Get status after the choice
        EncounterStatusModel newStatus = GetEncounterStatusModel();

        // Generate narrative for the reaction and new scene
        string narrative = "Continued Narrative";

        // If the encounter is over, return the outcome
        if (outcome.IsEncounterOver)
        {
            narrative = await _narrativeService.GenerateEndingAsync(
                _narrativeContext,
                choice,
                choiceDescription,
                outcome,
                newStatus);

            bool _processStateChanges = true;
            if (_processStateChanges)
            {
                EncounterSummaryResult stateChanges = await GenerateStateChanges(outcome, newStatus);
            }

            if (_useMemory)
            {
                await UpdateMemoryFile(outcome, newStatus);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            _narrativeContext.AddEvent(narrativeEvent);

            NarrativeResult narrativeResultFinished = new(
                narrative,
                new List<IChoice>(),
                new List<ChoiceProjection>(),
                new Dictionary<IChoice, ChoiceNarrative>(),
                narrativeEvent.ChoiceNarrative);

            narrativeResultFinished.SetOutcome(outcome.Outcome);
            narrativeResultFinished.SetIsEncounterOver(outcome.IsEncounterOver);

            return narrativeResultFinished;
        }
        else
        {
            if (_useAiNarrative && _narrativeService != null)
            {
                narrative = await _narrativeService.GenerateReactionAndSceneAsync(
                    _narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            _narrativeContext.AddEvent(narrativeEvent);

            // Get the new choices and projections
            GenerateChoices();
            List<IChoice> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = newChoices.Select(ProjectChoice).ToList();

            // Generate descriptive narratives for each choice
            Dictionary<IChoice, ChoiceNarrative> newChoiceDescriptions = null;
            if (_useAiNarrative && _narrativeService != null)
            {
                newChoiceDescriptions = await _narrativeService.GenerateChoiceDescriptionsAsync(
                    _narrativeContext,
                    newChoices,
                    newProjections,
                    newStatus);
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
            NarrativeResult narrativeResultOngoing = new(
                narrative,
                newChoices,
                newProjections,
                newChoiceDescriptions,
                narrativeEvent.ChoiceNarrative);

            return narrativeResultOngoing;
        }
    }

    private NarrativeEvent GetNarrativeEvent(IChoice choice, ChoiceNarrative choiceDescription, ChoiceOutcome outcome, string narrative)
    {
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            State.CurrentTurn - 1, // The turn counter increases after application
            narrative);

        narrativeEvent.SetChosenOption(choice);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);
        return narrativeEvent;
    }

    private async Task UpdateMemoryFile(ChoiceOutcome outcome, EncounterStatusModel newStatus)
    {
        string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

        string memoryContent = await _narrativeService.GenerateMemoryFileAsync(
            _narrativeContext,
            outcome,
            newStatus,
            oldMemory
            );

        await MemoryFileAccess.WriteToMemoryFile(outcome, newStatus, memoryContent);
    }

    private async Task<EncounterSummaryResult> GenerateStateChanges(ChoiceOutcome outcome, EncounterStatusModel newStatus)
    {
        string stateChangesJson = await _narrativeService.GenerateStateChangesAsync(
            _narrativeContext,
            outcome,
            newStatus
            );

        EncounterSummaryParser parser = new EncounterSummaryParser();
        EncounterSummaryResult summary = parser.ParseSummary(stateChangesJson);

        return summary;
    }
}