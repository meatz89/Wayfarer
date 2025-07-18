/// <summary>
/// Lightweight system for AI-powered NPC conversations
/// </summary>
public class ConversationSystem
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly ConnectionTokenManager _tokenManager;
    
    public ConversationSystem(
        GameWorld gameWorld, 
        NPCRepository npcRepository,
        MessageSystem messageSystem,
        ConnectionTokenManager tokenManager)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _tokenManager = tokenManager;
    }
    
    /// <summary>
    /// Start a conversation with an NPC
    /// </summary>
    public ConversationContext StartConversation(string npcId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null) return null;
        
        var player = _gameWorld.GetPlayer();
        var tokens = _tokenManager.GetTokensWithNPC(npcId);
        var totalTokens = tokens.Values.Sum();
        
        return new ConversationContext
        {
            NPCId = npcId,
            NPCName = npc.Name,
            NPCDescription = npc.Description,
            NPCProfession = npc.Profession,
            PlayerName = player.Name,
            RelationshipLevel = totalTokens,
            TokensByType = tokens,
            ConversationHistory = GetConversationHistory(npcId)
        };
    }
    
    /// <summary>
    /// Process a player's dialogue choice
    /// </summary>
    public ConversationResult ProcessDialogue(string npcId, string playerChoice, string npcResponse)
    {
        var result = new ConversationResult
        {
            NPCResponse = npcResponse,
            Effects = new List<string>()
        };
        
        // Analyze the conversation for relationship effects
        if (IsPositiveInteraction(playerChoice, npcResponse))
        {
            var npc = _npcRepository.GetNPCById(npcId);
            if (npc != null && npc.LetterTokenTypes.Any())
            {
                var tokenType = npc.LetterTokenTypes.First();
                _tokenManager.AddTokensToNPC(tokenType, 1, npcId);
                result.Effects.Add($"+1 {tokenType} token with {npc.Name}");
                
                _messageSystem.AddSystemMessage(
                    $"Your relationship with {npc.Name} has improved",
                    SystemMessageTypes.Success
                );
            }
        }
        
        // Store conversation for context
        StoreConversation(npcId, playerChoice, npcResponse);
        
        return result;
    }
    
    private bool IsPositiveInteraction(string playerChoice, string npcResponse)
    {
        // Simple heuristic - can be enhanced with AI analysis
        var positiveWords = new[] { "thank", "help", "appreciate", "friend", "trust" };
        var combined = (playerChoice + " " + npcResponse).ToLower();
        return positiveWords.Any(word => combined.Contains(word));
    }
    
    private List<ConversationTurn> GetConversationHistory(string npcId)
    {
        // In a full implementation, this would retrieve from storage
        return new List<ConversationTurn>();
    }
    
    private void StoreConversation(string npcId, string playerChoice, string npcResponse)
    {
        // In a full implementation, this would persist the conversation
    }
}

/// <summary>
/// Context provided to AI for generating NPC responses
/// </summary>
public class ConversationContext
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public string NPCDescription { get; set; }
    public Professions NPCProfession { get; set; }
    public string PlayerName { get; set; }
    public int RelationshipLevel { get; set; }
    public Dictionary<ConnectionType, int> TokensByType { get; set; }
    public List<ConversationTurn> ConversationHistory { get; set; }
}

/// <summary>
/// Result of processing a conversation
/// </summary>
public class ConversationResult
{
    public string NPCResponse { get; set; }
    public List<string> Effects { get; set; }
}

/// <summary>
/// A single exchange in a conversation
/// </summary>
public class ConversationTurn
{
    public string PlayerText { get; set; }
    public string NPCText { get; set; }
    public DateTime Timestamp { get; set; }
}