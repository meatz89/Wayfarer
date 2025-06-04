public class EncounterManager
{
    private EncounterContext _context;
    private EncounterState _state;
    private AIGameMaster _aiGameMaster;
    private readonly WorldStateInputBuilder _worldStateInputBuilder;
    private ChoiceProjectionService _projectionService;
    private bool _isAwaitingAIResponse = false;

    public LocationAction _locationAction { get; private set; }

    public Player Player => _context.Player;
    public WorldState WorldState { get; private set; }
    public EncounterResult EncounterResult { get; private set; }
    public bool IsInitialState { get; set; }
    public bool IsEncounterComplete => _state?.IsEncounterComplete ?? false;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;
    private List<ChoiceTemplate> _allTemplates;

    private StreamingContentState currentNarrative;

    public List<EncounterChoice> Choices = new List<EncounterChoice>();
    private List<ChoiceProjection> _currentChoiceProjections = new List<ChoiceProjection>();

    public EncounterManager(
        EncounterContext encounterContext,
        EncounterState state,
        LocationAction locationAction,
        ChoiceProjectionService choiceProjectionService,
        AIGameMaster aiGameMaster,
        WorldStateInputBuilder worldStateInputBuilder)
    {
        _context = encounterContext;
        _state = state;
        _locationAction = locationAction;
        _projectionService = choiceProjectionService;
        _aiGameMaster = aiGameMaster;
        _worldStateInputBuilder = worldStateInputBuilder;
        currentNarrative = new StreamingContentState();
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
    }

    public async Task ProcessNextBeat()
    {
        await GenerateIntroduction();
        await GenerateChoices();
    }

    private async Task GenerateIntroduction()
    {
        if (_isAwaitingAIResponse || currentNarrative.IsStreaming)
        {
            return;
        }

        _isAwaitingAIResponse = true;

        WorldStateInput worldStateInput = await _worldStateInputBuilder
            .CreateWorldStateInput(_context.LocationName);

        Choices = await _aiGameMaster.RequestChoices(
            _context, _state, worldStateInput, _allTemplates, AIClient.PRIORITY_IMMEDIATE);

        _currentChoiceProjections = CreateChoiceProjections(Choices);

        _isAwaitingAIResponse = false;
    }

    private async Task GenerateChoices()
    {
        if (_isAwaitingAIResponse || currentNarrative.IsStreaming)
        {
            return;
        }

        _isAwaitingAIResponse = true;

        WorldStateInput worldStateInput = await _worldStateInputBuilder
            .CreateWorldStateInput(_context.LocationName);

        Choices = await _aiGameMaster.RequestChoices(
            _context, _state, worldStateInput, _allTemplates, AIClient.PRIORITY_IMMEDIATE);

        _currentChoiceProjections = CreateChoiceProjections(Choices);

        _isAwaitingAIResponse = false;
    }

    public async Task<BeatOutcome> ProcessPlayerChoice(EncounterChoice selectedChoice)
    {
        if (selectedChoice == null || _isAwaitingAIResponse || currentNarrative.IsStreaming)
        {
            return null;
        }

        // Create projection for UI (if needed)
        ChoiceProjection projection = _projectionService.ProjectChoice(selectedChoice, _state);

        // Process the choice directly on the state
        _state.SpendFocusPoints(selectedChoice.FocusCost);

        // Get the skill option
        SkillOption selectedOption = selectedChoice.SkillOption;

        // Perform skill check
        bool success = false;
        int progressGained = 0;

        if (selectedOption != null)
        {
            // Find matching skill card
            SkillCard card = FindCardByName(_context.Player.AvailableCards, selectedOption.SkillName);
            bool isUntrained = (card == null || card.IsExhausted);

            // Calculate effective level and difficulty
            int effectiveLevel = 0;
            int difficulty = selectedOption.SCD;

            if (!isUntrained && card != null)
            {
                effectiveLevel = card.GetEffectiveLevel(_state);
                card.Exhaust();
            }
            else
            {
                difficulty += 2; // +2 difficulty for untrained
            }

            // Apply modifier
            effectiveLevel += _state.GetNextCheckModifier();

            // Determine success
            success = effectiveLevel >= difficulty;

            // Apply appropriate effect
            if (success)
            {
                ApplyEffect(selectedOption.SuccessEffectEntry.ID, _state);
                currentNarrative.BeginStreaming(selectedChoice.SuccessNarrative);
                progressGained = 2; // Default success progress
            }
            else
            {
                ApplyEffect(selectedOption.FailureEffectEntry.ID, _state);
                currentNarrative.BeginStreaming(selectedChoice.FailureNarrative);
                progressGained = 1; // Default failure progress
            }
        }

        // Update recovery count
        if (selectedChoice.FocusCost == 0)
        {
            _state.ConsecutiveRecoveryCount++;
        }
        else
        {
            _state.ConsecutiveRecoveryCount = 0;
        }

        // Process state changes
        _state.ProcessModifiers();
        _state.AddProgress(progressGained);
        _state.AdvanceDuration(1);

        // Handle stage advancement (from EncounterState.ApplyChoice)
        if (_state.DurationCounter % 2 == 0 && _state.CurrentStageIndex < 4)
        {
            _state.AdvanceStage();
        }

        // Check goal completion
        _state.CheckGoalCompletion();

        // Clear choices while streaming
        _currentChoiceProjections.Clear();

        // Create outcome
        BeatOutcome outcome = new BeatOutcome
        {
            Outcome = success ? BeatOutcomes.Success : BeatOutcomes.Failure,
            ProgressGained = progressGained,
            IsEncounterComplete = _state.IsEncounterComplete
        };

        // Add outcome determination from EncounterState.ApplyChoice
        if (_state.IsEncounterComplete)
        {
            int successThreshold = 10; // Basic success threshold
            outcome.Outcome = _state.CurrentProgress >= successThreshold
                ? BeatOutcomes.Success
                : BeatOutcomes.Failure;
        }

        return outcome;
    }

    private List<ChoiceProjection> CreateChoiceProjections(List<EncounterChoice> choices)
    {
        List<ChoiceProjection> projections = new List<ChoiceProjection>();

        foreach (EncounterChoice choice in choices)
        {
            ChoiceProjection item = GetChoiceProjection(choice);
            projections.Add(item);
        }

        return projections;
    }

    public ChoiceProjection GetChoiceProjection(EncounterChoice choice)
    {
        return _projectionService.ProjectChoice(choice, _state);
    }

    private void ApplyEffect(string effectID, EncounterState state)
    {
        if (ChoiceTemplateLibrary.HasEffect(effectID))
        {
            IMechanicalEffect effect = ChoiceTemplateLibrary.GetEffect(effectID).SuccessEffect;
            effect.Apply(state);
        }
        else
        {
            // Handle missing effect - could log this in a real implementation
        }
    }

    private int CalculateProgressGained(EncounterChoice choice, SkillOption option, bool success)
    {
        // Base progress calculation - can be expanded based on effect types
        if (success)
        {
            return 2; // Default progress for successful action
        }
        return 1; // Minimal progress for failed action
    }

    private SkillCard FindCardByName(List<SkillCard> cards, string name)
    {
        foreach (SkillCard card in cards)
        {
            if (card.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !card.IsExhausted)
            {
                return card;
            }
        }
        return null;
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
        currentNarrative.Update();

        // Check if streaming just completed and we need new choices
        if (!currentNarrative.IsStreaming &&
            !_isAwaitingAIResponse &&
            _currentChoiceProjections.Count == 0 &&
            !_state.IsEncounterComplete)
        {
            GenerateChoices().ConfigureAwait(false);
        }

        return currentNarrative;
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