namespace Wayfarer.GameState.Actions.Requirements;

/// <summary>
/// Requirement that checks if player has sufficient relationship with an NPC
/// </summary>
public class RelationshipRequirement : IActionRequirement
{
    private readonly string _npcId;
    private readonly int _tokensRequired;
    private readonly ConnectionType? _specificTokenType;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    
    public RelationshipRequirement(string npcId, int tokensRequired, ConnectionType? specificTokenType, NPCRepository npcRepository, ConnectionTokenManager tokenManager)
    {
        _npcId = npcId;
        _tokensRequired = tokensRequired;
        _specificTokenType = specificTokenType;
        _npcRepository = npcRepository;
        _tokenManager = tokenManager;
    }
    
    public bool IsSatisfied(Player player, GameWorld world)
    {
        var tokens = _tokenManager.GetTokensWithNPC(_npcId);
        
        if (_specificTokenType.HasValue)
        {
            return tokens.GetValueOrDefault(_specificTokenType.Value, 0) >= _tokensRequired;
        }
        
        return tokens.Values.Sum() >= _tokensRequired;
    }
    
    public string GetDescription()
    {
        var npc = _npcRepository.GetNPCById(_npcId);
        var npcName = npc?.Name ?? "NPC";
        
        if (_specificTokenType.HasValue)
        {
            return $"{_tokensRequired} {_specificTokenType} token{(_tokensRequired > 1 ? "s" : "")} with {npcName}";
        }
        
        return $"{_tokensRequired} token{(_tokensRequired > 1 ? "s" : "")} with {npcName}";
    }
    
    public string GetFailureReason(Player player, GameWorld world)
    {
        var tokens = _tokenManager.GetTokensWithNPC(_npcId);
        var npc = _npcRepository.GetNPCById(_npcId);
        var npcName = npc?.Name ?? "NPC";
        
        if (_specificTokenType.HasValue)
        {
            var current = tokens.GetValueOrDefault(_specificTokenType.Value, 0);
            return $"Insufficient relationship with {npcName}! Need {_tokensRequired} {_specificTokenType} tokens, have {current}";
        }
        
        var totalTokens = tokens.Values.Sum();
        return $"Insufficient relationship with {npcName}! Need {_tokensRequired} tokens, have {totalTokens}";
    }
    
    public bool CanBeRemedied => true;
    
    public string GetRemediationHint()
    {
        var npc = _npcRepository.GetNPCById(_npcId);
        var npcName = npc?.Name ?? "the NPC";
        
        return $"Work for {npcName} or socialize to build relationship";
    }
    
    public double GetProgress(Player player, GameWorld world)
    {
        var tokens = _tokenManager.GetTokensWithNPC(_npcId);
        
        int current;
        if (_specificTokenType.HasValue)
        {
            current = tokens.GetValueOrDefault(_specificTokenType.Value, 0);
        }
        else
        {
            current = tokens.Values.Sum();
        }
        
        if (current >= _tokensRequired) return 1.0;
        return (double)current / _tokensRequired;
    }
}