/// <summary>
/// Manages AI-driven conversations with NPCs
/// </summary>
public class ConversationManager
{
    private SceneContext _context;
    private ConversationState _state;
    private INarrativeProvider _narrativeProvider;
    private GameWorld _gameWorld;

    public ConversationState State => _state;

    public SceneContext Context => _context;

    public bool IsAwaitingResponse => _isAwaitingAIResponse;

    public List<ConversationChoice> Choices = new List<ConversationChoice>();
    public bool _isAwaitingAIResponse = false;
    public bool _isAvailable = true;

    public ConversationManager(
        SceneContext context,
        ConversationState state,
        INarrativeProvider narrativeProvider,
        GameWorld gameWorld)
    {
        _context = context;
        _state = state;
        _narrativeProvider = narrativeProvider;
        _gameWorld = gameWorld;
    }

    public async Task InitializeConversation()
    {
        _state.DurationCounter = 0;
        _state.IsConversationComplete = false;

        // Generate initial narrative for the conversation
        string introduction = await _narrativeProvider.GenerateIntroduction(_context, _state);
        _state.CurrentNarrative = introduction;
    }

    public async Task<bool> ProcessNextBeat()
    {
        if (!_isAvailable || !await _narrativeProvider.IsAvailable())
        {
            return false;
        }

        _isAwaitingAIResponse = true;

        // Generate conversation choices
        List<ChoiceTemplate> choiceTemplates = new List<ChoiceTemplate>(); // Empty for now, can be populated from context
        if (_context is ActionConversationContext actionContext && actionContext.AvailableTemplates != null)
        {
            choiceTemplates = actionContext.AvailableTemplates;
        }

        Choices = await _narrativeProvider.GenerateChoices(
            _context,
            _state,
            choiceTemplates);

        _isAwaitingAIResponse = false;
        return true;
    }

    public async Task<ConversationBeatOutcome> ProcessPlayerChoice(ConversationChoice selectedChoice)
    {
        // Spend attention if required
        if (_context?.AttentionManager != null && selectedChoice.AttentionCost > 0)
        {
            _context.AttentionManager.TrySpend(selectedChoice.AttentionCost);
        }

        // Process the player's dialogue choice
        bool success = true; // TODO: Determine success based on skill check

        // Generate reaction narrative
        string reactionNarrative = await _narrativeProvider.GenerateReaction(
            _context,
            _state,
            selectedChoice,
            success);

        _state.LastChoiceNarrative = selectedChoice.NarrativeText;
        _state.CurrentNarrative = reactionNarrative;
        _state.AdvanceDuration();

        // Check if conversation should complete
        // For now, ALL conversations complete after one choice
        bool shouldComplete = true;

        // Create outcome
        ConversationBeatOutcome outcome = new ConversationBeatOutcome
        {
            NarrativeDescription = reactionNarrative,
            IsConversationComplete = shouldComplete
        };

        // Check for conversation completion
        if (outcome.IsConversationComplete)
        {
            _state.IsConversationComplete = true;
            // Generate conclusion
            string conclusion = await _narrativeProvider.GenerateConclusion(
                _context,
                _state,
                selectedChoice);
            outcome.NarrativeDescription = conclusion;
        }

        return outcome;
    }
}

public class ConversationChoice
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int AttentionCost { get; set; }
    public bool IsAffordable { get; set; }
    public string TemplatePurpose { get; set; }
    public ConversationChoiceType ChoiceType { get; set; } = ConversationChoiceType.Default;
    public string SuccessNarrative { get; internal set; }
    public string FailureNarrative { get; internal set; }
    public SkillOption SkillOption { get; set; }
    public bool RequiresSkillCheck { get; internal set; }

    // Category-based properties for letter offers
    public ConnectionType? OfferTokenType { get; set; }
    public LetterCategory? OfferCategory { get; set; }

    // Delivery-specific properties
    public DeliveryOutcome DeliveryOutcome { get; set; }
    public int Priority { get; set; } = 0;

    // Travel encounter properties
    public TravelChoiceEffect? TravelEffect { get; set; }
    public EquipmentType? RequiredEquipment { get; set; }
    public int? TimeModifierMinutes { get; set; }
    public int? StaminaCost { get; set; }
    public int? CoinReward { get; set; }
    
    // Literary UI properties - hidden verb system
    public BaseVerb BaseVerb { get; set; }
    public bool IsAvailable { get; set; }
    
    // Mechanical effect to apply when choice is selected
    public IMechanicalEffect MechanicalEffect { get; set; }
    
    // Multiple system effects - choices should touch 2-3 systems minimum
    public string BodyLanguageHint { get; set; } // "Relief floods their features"
}

/// <summary>
/// Effects of travel encounter choices
/// </summary>
public enum TravelChoiceEffect
{
    None,
    SaveTime,        // Reduce travel time
    LoseTime,        // Increase travel time
    SpendStamina,    // Cost stamina
    EarnCoins,       // Gain coins
    GainInformation  // Learn something useful
}

/// <summary>
/// Equipment types for travel encounters
/// </summary>
public enum EquipmentType
{
    None,
    ClimbingGear,
    LightSource,
    WeatherProtection,
    LoadDistribution
}