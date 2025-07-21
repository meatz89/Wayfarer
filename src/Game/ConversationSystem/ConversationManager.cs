/// <summary>
/// Manages AI-driven conversations with NPCs
/// </summary>
public class ConversationManager
{
    private ConversationContext _context;
    private ConversationState _state;
    private INarrativeProvider _narrativeProvider;
    private GameWorld _gameWorld;
    
    public ConversationState State => _state;
    public ConversationContext Context => _context;
    public bool IsAwaitingResponse => _isAwaitingAIResponse;
    public List<ConversationChoice> Choices = new List<ConversationChoice>();
    public bool _isAwaitingAIResponse = false;
    public bool _isAvailable = true;
    
    public ConversationManager(
        ConversationContext context,
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
        var choiceTemplates = new List<ChoiceTemplate>(); // Empty for now, can be populated from context
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
        // Process the player's dialogue choice
        bool success = true; // TODO: Determine success based on skill check
        
        // Generate reaction narrative
        var reactionNarrative = await _narrativeProvider.GenerateReaction(
            _context,
            _state,
            selectedChoice,
            success);
            
        _state.LastChoiceNarrative = selectedChoice.NarrativeText;
        _state.CurrentNarrative = reactionNarrative;
        _state.AdvanceDuration();
        
        // Create outcome
        var outcome = new ConversationBeatOutcome
        {
            NarrativeDescription = reactionNarrative,
            IsConversationComplete = _state.DurationCounter >= _state.MaxDuration
        };
        
        // Check for conversation completion
        if (outcome.IsConversationComplete)
        {
            _state.IsConversationComplete = true;
            // Generate conclusion
            var conclusion = await _narrativeProvider.GenerateConclusion(
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
    public int FocusCost { get; set; }
    public bool IsAffordable { get; set; }
    public string TemplateUsed { get; set; }
    public string TemplatePurpose { get; set; }
    public string SuccessNarrative { get; internal set; }
    public string FailureNarrative { get; internal set; }
    public SkillOption SkillOption { get; set; }
    public bool RequiresSkillCheck { get; internal set; }
}