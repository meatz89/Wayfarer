public class EncounterManager
{
    private EncounterContext _context;
    private EncounterState _state;
    private LocationAction _locationAction;
    private AIGameMaster _aiGameMaster;
    private WorldStateInputBuilder _worldStateInputBuilder;
    private ChoiceProjectionService _projectionService;
    private GameWorld _gameWorld;
    public List<EncounterChoice> Choices = new List<EncounterChoice>();
    public bool _isAwaitingAIResponse = false;
    
    public bool _isAvailable = true;

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
        this._locationAction = locationAction;
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

    public async Task<bool> ProcessNextBeat()
    {
        if (!_isAvailable || !await _aiGameMaster.CanReceiveRequests())
        {
            _isAvailable = false;
            return _isAvailable;
        }

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

        return _isAvailable;
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

    public async Task<BeatOutcome> ProcessPlayerChoice(PlayerChoiceSelection choiceSelection)
    {
        if (choiceSelection.Choice == null || _isAwaitingAIResponse || _gameWorld.StreamingContentState.IsStreaming)
        {
            return null;
        }

        EncounterChoice selectedChoice = choiceSelection.Choice;

        // Process the choice
        _state.SpendFocusPoints(selectedChoice.FocusCost);

        // Get the skill option
        SkillOption skillCheck = selectedChoice.SkillOption;

        // Perform skill check
        int progressGained = 0;

        // Determine success
        ChoiceProjection projection = _projectionService.ProjectChoice(selectedChoice, _state);
        bool success = projection.SkillOption.ChoiceSuccess;
            
        // Generate reaction - will automatically begin streaming
        _isAwaitingAIResponse = true;

        WorldStateInput worldStateInput = await _worldStateInputBuilder
            .CreateWorldStateInput(_context.LocationName);

        _isAwaitingAIResponse = false;

        // Apply appropriate effect
        IMechanicalEffect effect = ChoiceTemplateLibrary.GetEffect(selectedChoice.TemplateUsed, success);
        ApplyEffect(effect, _state);
        progressGained = success ? 2 : 1; // Default success progress

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
        
        _state.LastBeatOutcome = outcome.Outcome;
        _state.LastChoiceNarrative = selectedChoice.NarrativeText;

        await _aiGameMaster.GenerateReaction(
            _context, _state, selectedChoice, success,
            worldStateInput, AIClient.PRIORITY_IMMEDIATE);

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

    private void ApplyEffect(IMechanicalEffect effect, EncounterState state)
    {
        if (effect == null) return;

        effect.Apply(state);
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