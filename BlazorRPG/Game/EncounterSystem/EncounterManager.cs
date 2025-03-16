public class EncounterManager
{
    public bool _useAiNarrative = false;

    public ActionImplementation ActionImplementation;
    private readonly CardSelectionAlgorithm _cardSelector;
    public EncounterState State;

    private SwitchableNarrativeService _narrativeService;
    private NarrativeContext _narrativeContext;

    public List<IChoice> CurrentChoices = new List<IChoice>();

    public EncounterManager(
        ActionImplementation actionImplementation,
        CardSelectionAlgorithm cardSelector,
        bool useAiNarrative,
        IConfiguration configuration,
        ILogger logger = null)
    {
        ActionImplementation = actionImplementation;
        _cardSelector = cardSelector;
        _useAiNarrative = useAiNarrative;

        if (_useAiNarrative)
        {
            _narrativeService = new SwitchableNarrativeService(configuration, logger);
        }
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

    private ChoiceOutcome ApplyChoiceProjection(ChoiceProjection projection)
    {
        State.ApplyChoiceProjection(projection);

        ChoiceOutcome outcome = new ChoiceOutcome(
            projection.MomentumGained,
            projection.PressureBuilt,
            projection.NarrativeDescription,
            projection.EncounterWillEnd,
            projection.ProjectedOutcome,
            projection.HealthChange,
            projection.FocusChange,
            projection.ConfidenceChange);

        foreach (KeyValuePair<ApproachTags, int> kvp in projection.ApproachTagChanges)
        {
            outcome.ApproachTagChanges[kvp.Key] = kvp.Value;
        }

        foreach (KeyValuePair<FocusTags, int> kvp in projection.FocusTagChanges)
        {
            outcome.FocusTagChanges[kvp.Key] = kvp.Value;
        }

        foreach (KeyValuePair<EncounterStateTags, int> kvp in projection.EncounterStateTagChanges)
        {
            outcome.EncounterStateTagChanges[kvp.Key] = kvp.Value;
        }

        outcome.NewlyActivatedTags.AddRange(projection.NewlyActivatedTags);
        outcome.DeactivatedTags.AddRange(projection.DeactivatedTags);

        return outcome;
    }

    public EncounterStatus GetEncounterStatus()
    {
        return new EncounterStatus(
            State.CurrentTurn,
            State.Location.TurnDuration,
            State.Momentum,
            State.Pressure,
            State.TagSystem.GetAllEncounterStateTags(),
            State.TagSystem.GetAllApproachTags(),
            State.TagSystem.GetAllFocusTags(),
            State.ActiveTags.Select(t => t.Name).ToList()
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

    private void StartEncounter(LocationInfo location, PlayerState playerState)
    {
        State = new EncounterState(location, playerState);
        State.UpdateActiveTags(location.AvailableTags);
    }

    public async Task<NarrativeResult> StartEncounterWithNarrativeAsync(
        LocationInfo location,
        PlayerState playerState,
        string incitingAction,
        AIProviderType providerType)
    {
        if (_narrativeService != null)
        {
            _narrativeService.SwitchProvider(providerType);
        }

        // Start the encounter mechanically
        StartEncounter(location, playerState);

        // Create narrative context
        _narrativeContext = new NarrativeContext(location.Name, incitingAction, location.Style);

        // Generate introduction
        EncounterStatus status = GetEncounterStatus();

        string introduction = "introduction";
        if (_useAiNarrative && _narrativeService != null)
        {
            introduction = await _narrativeService.GenerateIntroductionAsync(
                location.Name,
                incitingAction,
                status);
        }

        // Get available choices
        GenerateChoices();
        List<IChoice> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = choices.Select(ProjectChoice).ToList();

        // Create first narrative event
        NarrativeEvent firstEvent = new NarrativeEvent(
            State.CurrentTurn,
            introduction);

        _narrativeContext.AddEvent(firstEvent);

        // Generate choice descriptions
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = null;
        if (_useAiNarrative && _narrativeService != null)
        {
            choiceDescriptions = await _narrativeService.GenerateChoiceDescriptionsAsync(
                _narrativeContext,
                choices,
                projections,
                status);

            firstEvent.SetAvailableChoiceDescriptions(choiceDescriptions);
        }

        // Return the narrative result
        return new NarrativeResult(
            introduction,
            choices,
            projections,
            choiceDescriptions);
    }

    public async Task<NarrativeResult> ApplyChoiceWithNarrativeAsync(
        IChoice choice,
        ChoiceNarrative choiceDescription)
    {
        // Get projection
        ChoiceProjection projection = ProjectChoice(choice);

        // Apply the choice
        ChoiceOutcome outcome = ApplyChoiceProjection(projection);

        // Get status after the choice
        EncounterStatus newStatus = GetEncounterStatus();

        // Generate narrative for the reaction and new scene
        string narrative = "Continued Narrative";

        if (_useAiNarrative && _narrativeService != null)
        {
            narrative = await _narrativeService.GenerateReactionAndSceneAsync(
                _narrativeContext,
                choice,
                choiceDescription,
                outcome,
                newStatus);
        }

        NarrativeEvent narrativeEvent = new NarrativeEvent(
            State.CurrentTurn - 1, // The turn counter increases after application
            narrative);

        narrativeEvent.SetChosenOption(choice);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);

        _narrativeContext.AddEvent(narrativeEvent);

        // If the encounter is over, return the outcome
        if (outcome.IsEncounterOver)
        {
            return new NarrativeResult(
                narrative,
                new List<IChoice>(),
                new List<ChoiceProjection>(),
                new Dictionary<IChoice, ChoiceNarrative>(),
                outcome.IsEncounterOver,
                outcome.Outcome);
        }

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

        // Return the narrative result
        return new NarrativeResult(
            narrative,
            newChoices,
            newProjections,
            newChoiceDescriptions);
    }
}