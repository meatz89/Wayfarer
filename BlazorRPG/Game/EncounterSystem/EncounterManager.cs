using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        SwitchableNarrativeService narrativeService,
        bool useAiNarrative,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        ActionImplementation = actionImplementation;
        _cardSelector = cardSelector;
        _useAiNarrative = useAiNarrative;
        _narrativeService = narrativeService;
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

    public EncounterStatus GetEncounterStatus()
    {
        return new EncounterStatus(
            State.CurrentTurn,
            State.Location.TurnDuration,
            State.Momentum,
            State.Pressure,
            State.TagSystem.GetAllApproachTags(),
            State.TagSystem.GetAllFocusTags(),
            State.GetActiveTagsNames()
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
        EncounterStatus status = GetEncounterStatus();

        string introduction = "introduction";
        if (_useAiNarrative && _narrativeService != null)
        {
            string memoryContent = await MemoryFileAccess.ReadFromMemoryFile();

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
        // Apply the choice
        ChoiceOutcome outcome = ApplyChoiceProjection(choice);

        // Get status after the choice
        EncounterStatus newStatus = GetEncounterStatus();

        // Generate narrative for the reaction and new scene
        string narrative = "Continued Narrative";

        // If the encounter is over, return the outcome
        if (outcome.IsEncounterOver)
        {
            narrative = await _narrativeService.GenerateEndingAsync(
                _narrativeContext,
                choice,
                outcome,
                newStatus);

            NarrativeResult narrativeResultFinished = new(
                narrative,
                new List<IChoice>(),
                new List<ChoiceProjection>(),
                new Dictionary<IChoice, ChoiceNarrative>());

            narrativeResultFinished.SetOutcome(outcome.Outcome);
            narrativeResultFinished.SetIsEncounterOver(outcome.IsEncounterOver);
            
            await UpdateMemoryFile(outcome, newStatus);

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            _narrativeContext.AddEvent(narrativeEvent);

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

            // Return the narrative result
            NarrativeResult narrativeResultOngoing = new(
                narrative,
                newChoices,
                newProjections,
                newChoiceDescriptions);

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

    private async Task UpdateMemoryFile(ChoiceOutcome outcome, EncounterStatus newStatus)
    {
        var oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

        string memoryContent = await _narrativeService.GenerateMemoryFileAsync(
            _narrativeContext,
            outcome,
            newStatus,
            oldMemory
            );

        await MemoryFileAccess.WriteToMemoryFile(outcome, newStatus, memoryContent);
    }
}