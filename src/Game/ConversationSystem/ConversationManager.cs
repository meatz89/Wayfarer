/// <summary>
/// Manages AI-driven conversations with NPCs
/// </summary>
public class ConversationManager
{
    private SceneContext _context;
    private ConversationState _state;
    private INarrativeProvider _narrativeProvider;
    private GameWorld _gameWorld;
    private ConversationChoiceGenerator _choiceGenerator;

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
        GameWorld gameWorld,
        ConversationChoiceGenerator choiceGenerator = null)
    {
        _context = context;
        _state = state;
        _narrativeProvider = narrativeProvider;
        _gameWorld = gameWorld;
        _choiceGenerator = choiceGenerator;
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

        // Generate choices mechanically first
        if (_choiceGenerator != null)
        {
            Choices = _choiceGenerator.GenerateChoices(_context, _state);
        }
        else
        {
            // Fallback to narrative provider (temporary)
            List<ChoiceTemplate> choiceTemplates = new List<ChoiceTemplate>();
            if (_context is ActionConversationContext actionContext && actionContext.AvailableTemplates != null)
            {
                choiceTemplates = actionContext.AvailableTemplates;
            }
            
            Choices = await _narrativeProvider.GenerateChoices(
                _context,
                _state,
                choiceTemplates);
        }

        _isAwaitingAIResponse = false;
        return true;
    }

    public async Task<ConversationBeatOutcome> ProcessPlayerChoice(ConversationChoice selectedChoice)
    {
        // Spend attention if required
        if (_context?.AttentionManager != null && selectedChoice.AttentionCost > 0)
        {
            bool spent = _context.AttentionManager.TrySpend(selectedChoice.AttentionCost);
            if (!spent)
            {
                // Player doesn't have enough attention - this shouldn't happen if UI is correct
                Console.WriteLine($"[ConversationManager] WARNING: Player tried to select choice without enough attention");
            }
        }

        // Apply all mechanical effects from the choice
        if (selectedChoice.MechanicalEffects != null)
        {
            Console.WriteLine($"[ConversationManager] Applying {selectedChoice.MechanicalEffects.Count} mechanical effects");
            foreach (var effect in selectedChoice.MechanicalEffects)
            {
                try
                {
                    effect.Apply(_state);
                    Console.WriteLine($"[ConversationManager] Applied effect: {effect.GetDescriptionsForPlayer().FirstOrDefault()?.Text ?? ""}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ConversationManager] Error applying effect: {ex.Message}");
                }
            }
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
        _state.AdvanceDuration(1);

        // Check if conversation should complete
        // Conversation ends if: no attention left, or player chose to leave
        bool shouldComplete = _context?.AttentionManager?.Current == 0 || 
                             selectedChoice.ChoiceID == "exit" ||
                             selectedChoice.ChoiceID == "leave" ||
                             selectedChoice.NarrativeText.Contains("need to go") ||
                             selectedChoice.NarrativeText.Contains("be on my way");

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
    
    // UI display properties
    public bool IsLocked { get; set; }

    // Travel encounter properties
    public TravelChoiceEffect? TravelEffect { get; set; }
    public EquipmentType? RequiredEquipment { get; set; }
    public int? TimeModifierMinutes { get; set; }
    public int? StaminaCost { get; set; }
    public int? CoinReward { get; set; }
    
    // Card-based conversation system - no verb references needed
    
    // Mechanical description for UI display
    public string MechanicalDescription { get; set; }
    public bool IsAvailable { get; set; }
    
    // Mechanical effects to apply when choice is selected
    public List<IMechanicalEffect> MechanicalEffects { get; set; }
    
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
    LightSource,
    WeatherProtection,
    LoadDistribution
}