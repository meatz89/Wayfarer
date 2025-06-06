public class EncounterManager
{
    private EncounterContext _context;
    private EncounterState _state;
    private AIGameMaster _aiGameMaster;
    private readonly WorldStateInputBuilder _worldStateInputBuilder;
    private ChoiceProjectionService _projectionService;
    public bool _isAwaitingAIResponse = false;
    private GameWorld _gameWorld;

    public LocationAction _locationAction { get; private set; }
    public bool IsEncounterComplete
    {
        get
        {
            return _state?.IsEncounterComplete ?? false;
        }
    }

    public EncounterState EncounterState
    {
        get
        {
            return _state;
        }
    }

    public EncounterContext EncounterContext
    {
        get
        {
            return _context;
        }
    }

    public List<EncounterChoice> Choices = new List<EncounterChoice>();

    public EncounterManager(
        EncounterContext encounterContext,
        EncounterState state,
        LocationAction locationAction,
        ChoiceProjectionService choiceProjectionService,
        AIGameMaster aiGameMaster,
        WorldStateInputBuilder worldStateInputBuilder,
        GameWorld gameWorld)
    {
        _context = encounterContext;
        _state = state;
        _locationAction = locationAction;
        _projectionService = choiceProjectionService;
        _aiGameMaster = aiGameMaster;
        _worldStateInputBuilder = worldStateInputBuilder;
        _gameWorld = gameWorld;
    }

    public async Task InitializeEncounter()
    {
        if (_state == null)
        {
            _state = new EncounterState(
                _context.Player,
                DetermineFocusPoints(_context.SkillCategory),
                8,
                DetermineProgressThreshold(_context.LocationAction.Complexity)
            );
        }

        _state.MaxDuration = 8;
        _state.DurationCounter = 0;
        _state.IsEncounterComplete = false;
    }

    public async Task ProcessNextBeat()
    {
        if (!_isAwaitingAIResponse && !_gameWorld.StreamingContentState.IsStreaming)
        {
            if (string.IsNullOrEmpty(_gameWorld.StreamingContentState.CurrentText))
            {
                // If no introduction yet, generate introduction first
                await GenerateIntroduction();
            }
            else if (Choices.Count == 0)
            {
                // If introduction is done streaming and no choices, generate choices
                await GenerateChoices();
            }
        }
    }

    private async Task GenerateIntroduction()
    {
        if (_isAwaitingAIResponse || _gameWorld.StreamingContentState.IsStreaming)
        {
            return;
        }

        _isAwaitingAIResponse = true;

        WorldStateInput worldStateInput = await _worldStateInputBuilder
            .CreateWorldStateInput(_context.LocationName);

        // This will automatically begin streaming through StreamingContentState
        await _aiGameMaster.GenerateIntroduction(
            _context, _state, worldStateInput, AIClient.PRIORITY_IMMEDIATE);

        _isAwaitingAIResponse = false;
    }

    private async Task GenerateChoices()
    {
        if (_isAwaitingAIResponse || _gameWorld.StreamingContentState.IsStreaming)
        {
            return;
        }

        _isAwaitingAIResponse = true;

        // Store the current narrative before generating choices
        string currentNarrative = _gameWorld.StreamingContentState.CurrentText;

        WorldStateInput worldStateInput = await _worldStateInputBuilder
            .CreateWorldStateInput(_context.LocationName);

        List<ChoiceTemplate> allTemplates = ChoiceTemplateLibrary.GetAllTemplates();

        Choices = await _aiGameMaster.RequestChoices(
            _context, _state, worldStateInput, allTemplates, AIClient.PRIORITY_IMMEDIATE);

        // After choices are generated, ensure narrative is preserved
        if (string.IsNullOrEmpty(_gameWorld.StreamingContentState.CurrentText))
        {
            _gameWorld.StreamingContentState.SetFullText(currentNarrative);
        }

        _isAwaitingAIResponse = false;
    }

    public async Task<BeatOutcome> ProcessPlayerChoice(EncounterChoice selectedChoice)
    {
        if (selectedChoice == null || _isAwaitingAIResponse || _gameWorld.StreamingContentState.IsStreaming)
        {
            return null;
        }

        // Process the choice
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

            // Generate reaction - will automatically begin streaming
            _isAwaitingAIResponse = true;

            WorldStateInput worldStateInput = await _worldStateInputBuilder
                .CreateWorldStateInput(_context.LocationName);

            await _aiGameMaster.GenerateReaction(
                _context, _state, selectedChoice, success,
                worldStateInput, AIClient.PRIORITY_IMMEDIATE);

            _isAwaitingAIResponse = false;

            // Apply appropriate effect
            if (success)
            {
                ApplyEffect(selectedOption.SuccessEffectEntry.ID, _state);
                progressGained = 2; // Default success progress
            }
            else
            {
                ApplyEffect(selectedOption.FailureEffectEntry.ID, _state);
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

        // Check goal completion
        _state.CheckGoalCompletion();

        // Clear choices while streaming
        Choices.Clear();

        // Create outcome
        BeatOutcome outcome = new BeatOutcome
        {
            Outcome = success ? BeatOutcomes.Success : BeatOutcomes.Failure,
            ProgressGained = progressGained,
            IsEncounterComplete = _state.IsEncounterComplete
        };

        return outcome;
    }

    public EncounterState GetEncounterState()
    {
        return _state;
    }

    public List<EncounterChoice> GetCurrentChoices()
    {
        return Choices;
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

    private int DetermineProgressThreshold(int complexity)
    {
        // Base threshold on complexity
        return 5 + complexity * 2;
    }

    private void ApplyEffect(string effectID, EncounterState state)
    {
        if (ChoiceTemplateLibrary.HasEffect(effectID))
        {
            IMechanicalEffect effect = ChoiceTemplateLibrary.GetEffect(effectID).SuccessEffect;
            effect.Apply(state);
        }
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

    public ChoiceProjection GetChoiceProjection(EncounterChoice choice)
    {
        return _projectionService.ProjectChoice(choice, _state);
    }
}