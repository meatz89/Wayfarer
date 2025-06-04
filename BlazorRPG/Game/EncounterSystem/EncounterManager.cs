public class EncounterManager
{
    private EncounterContext _context;
    private EncounterState _state;
    private AIGameMaster _aiGameMaster;
    private AIPromptBuilder _promptBuilder;
    private readonly WorldStateInputBuilder worldStateInputBuilder;
    private ChoiceProjectionService _projectionService;
    private StreamingContentState _streamingState;
    private EncounterChoiceProcessor _choiceProcessor;
    private bool _isAwaitingAIResponse = false;

    public LocationAction _locationAction { get; private set; }
    public List<EncounterChoice> Choices = new List<EncounterChoice>();
    private List<ChoiceProjection> _currentChoiceProjections = new List<ChoiceProjection>();

    public Player Player => _context.Player;
    public WorldState WorldState { get; private set; }
    public EncounterResult EncounterResult { get; private set; }
    public bool IsInitialState { get; set; }
    public bool IsEncounterComplete => _state?.IsEncounterComplete ?? false;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;
    private List<ChoiceTemplate> _allTemplates;

    public EncounterManager(
        EncounterContext encounterContext,
        EncounterState state,
        LocationAction locationAction,
        ChoiceProjectionService choiceProjectionService,
        AIGameMaster aiGameMaster,
        AIPromptBuilder promptBuilder,
        WorldStateInputBuilder worldStateInputBuilder,
        EncounterChoiceProcessor choiceProcessor)
    {
        _context = encounterContext;
        _state = state;
        _locationAction = locationAction;
        _projectionService = choiceProjectionService;
        _aiGameMaster = aiGameMaster;
        _promptBuilder = promptBuilder;
        this.worldStateInputBuilder = worldStateInputBuilder;
        _choiceProcessor = choiceProcessor;
        _streamingState = new StreamingContentState();
        _useAiNarrative = true;
        _useMemory = true;
        _allTemplates = ChoiceTemplateLibrary.GetAllTemplates();
    }

    public async Task InitializeEncounter()
    {
        if (_state == null)
        {
            _state = new EncounterState(
                _context.Player,
                DetermineFocusPoints(_context.SkillCategory),
                8,
                "You find yourself in a tense situation, where your skills will be put to the test.");
        }

        _state.MaxDuration = 8;
        _state.DurationCounter = 0;
        _state.IsEncounterComplete = false;

        IsInitialState = true;

        // Request initial choices from AI
        await RequestAIChoices();
    }

    public ChoiceProjection GetChoiceProjection(EncounterChoice choice)
    {
        return _projectionService.ProjectChoice(choice, _state);
    }

    public async Task<BeatOutcome> ProcessPlayerChoice(EncounterChoice selectedChoice)
    {
        if (selectedChoice == null || _isAwaitingAIResponse || _streamingState.IsStreaming)
        {
            return null;
        }

        // Create player selection
        PlayerChoiceSelection selection = new PlayerChoiceSelection
        {
            Choice = selectedChoice,
            SelectedOption = selectedChoice.SkillOption
        };

        // Process choice using the EncounterChoiceResponseProcessor
        BeatOutcome outcome = _choiceProcessor.ProcessChoice(
            selectedChoice,
            selectedChoice.SkillOption.SkillName,
            _state,
            _context.Player
        );

        // Begin streaming narrative based on outcome
        if (outcome.Outcome == BeatOutcomes.Success)
        {
            _streamingState.BeginStreaming(selectedChoice.SuccessNarrative);
        }
        else
        {
            _streamingState.BeginStreaming(selectedChoice.FailureNarrative);
        }

        // Clear choices while streaming
        _currentChoiceProjections.Clear();

        return outcome;
    }

    private async Task RequestAIChoices()
    {
        if (_isAwaitingAIResponse || _streamingState.IsStreaming)
        {
            return;
        }

        _isAwaitingAIResponse = true;

        // Build prompt with ALL templates
        AIPrompt prompt = _promptBuilder.BuildChoicesPrompt(_context, _state, _allTemplates);

        WorldStateInput worldStateInput = await worldStateInputBuilder
            .CreateWorldStateInput(_context.LocationName);

        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        // Request choices from AI
        List<EncounterChoice> encounterChoices = await _aiGameMaster.RequestChoices(
            "test", systemMessage, prompt, "test", AIClient.PRIORITY_IMMEDIATE);

        Choices = encounterChoices;
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

    public StreamingContentState GetStreamingState()
    {
        // Update streaming state
        _streamingState.Update();

        // Check if streaming just completed and we need new choices
        if (!_streamingState.IsStreaming &&
            !_isAwaitingAIResponse &&
            _currentChoiceProjections.Count == 0 &&
            !_state.IsEncounterComplete)
        {
            RequestAIChoices().ConfigureAwait(false);
        }

        return _streamingState;
    }

    public EncounterContext GetEncounterContext()
    {
        return _context;
    }

    public EncounterState GetEncounterState()
    {
        return _state;
    }
    public List<ChoiceProjection> GetCurrentChoiceProjections()
    {
        return _currentChoiceProjections;
    }

    public List<EncounterChoice> GetCurrentChoices()
    {
        return Choices;
    }
}