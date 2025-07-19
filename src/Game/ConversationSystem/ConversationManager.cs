/// <summary>
/// Manages AI-driven conversations with NPCs
/// </summary>
public class ConversationManager
{
    private ConversationContext _context;
    private ConversationState _state;
    private AIGameMaster _aiGameMaster;
    private WorldStateInputBuilder _worldStateInputBuilder;
    private GameWorld _gameWorld;
    public List<ConversationChoice> Choices = new List<ConversationChoice>();
    public bool _isAwaitingAIResponse = false;
    public bool _isAvailable = true;
    
    public ConversationManager(
        ConversationContext context,
        ConversationState state,
        AIGameMaster aiGameMaster,
        WorldStateInputBuilder worldStateInputBuilder,
        GameWorld gameWorld)
    {
        _context = context;
        _state = state;
        _aiGameMaster = aiGameMaster;
        _worldStateInputBuilder = worldStateInputBuilder;
        _gameWorld = gameWorld;
    }
    
    public async Task InitializeConversation()
    {
        _state.DurationCounter = 0;
        _state.IsConversationComplete = false;
        
        // Generate initial AI narrative for the conversation
        var worldState = await _worldStateInputBuilder.CreateWorldStateInput(_gameWorld.WorldState.CurrentLocation?.Id ?? "unknown");
        string introduction = await _aiGameMaster.GenerateIntroduction(
            _context, 
            _state, 
            worldState, 
            1);
            
        _state.CurrentNarrative = introduction;
    }
    
    public async Task<bool> ProcessNextBeat()
    {
        if (!_isAvailable || !await _aiGameMaster.CanReceiveRequests())
        {
            return false;
        }
        
        _isAwaitingAIResponse = true;
        
        // Generate conversation choices
        var worldState = await _worldStateInputBuilder.CreateWorldStateInput(_gameWorld.WorldState.CurrentLocation?.Id ?? "unknown");
        
        var choiceTemplates = new List<ChoiceTemplate>(); // Empty for now
        Choices = await _aiGameMaster.RequestChoices(
            _context,
            _state,
            worldState,
            choiceTemplates,
            1);
            
        _isAwaitingAIResponse = false;
        return true;
    }
    
    public async Task<ConversationBeatOutcome> ProcessPlayerChoice(ConversationChoice selectedChoice)
    {
        // Process the player's dialogue choice
        var worldState = await _worldStateInputBuilder.CreateWorldStateInput(_gameWorld.WorldState.CurrentLocation?.Id ?? "unknown");
        
        // Generate reaction narrative
        var reactionNarrative = await _aiGameMaster.GenerateReaction(
            _context,
            _state,
            selectedChoice,
            true, // TODO: Determine success based on skill check
            worldState,
            1);
            
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
            var conclusion = await _aiGameMaster.GenerateConclusion(
                _context,
                _state,
                selectedChoice,
                worldState,
                1);
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