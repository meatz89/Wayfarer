using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Public facade for all token-related operations.
/// Manages relationship tokens, effects, unlocks, and NPC relationships.
/// </summary>
public class TokenFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly TokenEffectProcessor _tokenEffectProcessor;
    private readonly TokenUnlockManager _tokenUnlockManager;
    private readonly RelationshipTracker _relationshipTracker;
    private readonly MessageSystem _messageSystem;

    public TokenFacade(
        GameWorld gameWorld,
        ConnectionTokenManager connectionTokenManager,
        TokenEffectProcessor tokenEffectProcessor,
        TokenUnlockManager tokenUnlockManager,
        RelationshipTracker relationshipTracker,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _connectionTokenManager = connectionTokenManager;
        _tokenEffectProcessor = tokenEffectProcessor;
        _tokenUnlockManager = tokenUnlockManager;
        _relationshipTracker = relationshipTracker;
        _messageSystem = messageSystem;
    }

    // ========== TOKEN BALANCE OPERATIONS ==========

    /// <summary>
    /// Get all tokens with a specific NPC
    /// </summary>
    public Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId)
    {
        return _connectionTokenManager.GetTokensWithNPC(npcId);
    }

    /// <summary>
    /// Get total tokens of a type across all NPCs (only positive values count)
    /// </summary>
    public int GetTotalTokensOfType(ConnectionType type)
    {
        return _connectionTokenManager.GetTotalTokensOfType(type);
    }

    /// <summary>
    /// Check if player has enough tokens of a type (aggregated across all NPCs)
    /// </summary>
    public bool HasTokens(ConnectionType type, int amount)
    {
        return _connectionTokenManager.HasTokens(type, amount);
    }

    /// <summary>
    /// Get specific token count with an NPC
    /// </summary>
    public int GetTokenCount(string npcId, ConnectionType type)
    {
        return _connectionTokenManager.GetTokenCount(npcId, type);
    }

    // ========== TOKEN GENERATION OPERATIONS ==========

    /// <summary>
    /// Add tokens to a specific NPC relationship (applies equipment modifiers)
    /// </summary>
    public void AddTokensToNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        // Apply equipment modifiers
        int modifiedCount = _tokenEffectProcessor.ApplyGenerationModifiers(type, count);

        // Add tokens
        _connectionTokenManager.AddTokensToNPC(type, modifiedCount, npcId);

        // Check for relationship milestones
        int totalWithNPC = GetTotalTokensWithNPC(npcId);
        _relationshipTracker.CheckRelationshipMilestone(npcId, totalWithNPC);

        // Check for unlocks
        _tokenUnlockManager.CheckAndProcessUnlocks(npcId, type, GetTokenCount(npcId, type));

        // Notify relationship change
        _relationshipTracker.UpdateRelationshipState(npcId);
    }

    // ========== TOKEN SPENDING OPERATIONS ==========

    /// <summary>
    /// Spend tokens with a specific NPC (can go negative to represent debt)
    /// </summary>
    public bool SpendTokensWithNPC(ConnectionType type, int amount, string npcId)
    {
        if (amount <= 0) return true;

        bool result = _connectionTokenManager.SpendTokensWithNPC(type, amount, npcId);

        if (result)
        {
            // Update relationship state after spending
            _relationshipTracker.UpdateRelationshipState(npcId);

            // Check if we now owe the NPC
            int currentTokens = GetTokenCount(npcId, type);
            if (currentTokens < 0)
            {
                _relationshipTracker.RecordDebt(npcId, type, Math.Abs(currentTokens));
            }
        }

        return result;
    }

    /// <summary>
    /// Spend tokens of a type from any NPCs that have them
    /// </summary>
    public bool SpendTokensOfType(ConnectionType type, int amount)
    {
        if (amount <= 0) return true;

        return _connectionTokenManager.SpendTokensOfType(type, amount);
    }

    // ========== TOKEN REMOVAL OPERATIONS ==========

    /// <summary>
    /// Remove tokens from NPC relationship (for expired letters or relationship damage)
    /// </summary>
    public void RemoveTokensFromNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        _connectionTokenManager.RemoveTokensFromNPC(type, count, npcId);

        // Update relationship state after removal
        _relationshipTracker.UpdateRelationshipState(npcId);

        // Check if this created debt
        int currentTokens = GetTokenCount(npcId, type);
        if (currentTokens < 0)
        {
            _relationshipTracker.RecordDebt(npcId, type, Math.Abs(currentTokens));
        }
    }

    // ========== LEVERAGE OPERATIONS ==========

    /// <summary>
    /// Get leverage an NPC has over the player (negative tokens)
    /// </summary>
    public int GetLeverage(string npcId, ConnectionType type)
    {
        return _connectionTokenManager.GetLeverage(npcId, type);
    }

    /// <summary>
    /// Get total leverage an NPC has across all token types
    /// </summary>
    public int GetTotalLeverage(string npcId)
    {
        int totalLeverage = 0;
        foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
        {
            if (type != ConnectionType.None)
            {
                totalLeverage += GetLeverage(npcId, type);
            }
        }
        return totalLeverage;
    }

    // ========== RELATIONSHIP OPERATIONS ==========

    /// <summary>
    /// Get total tokens with an NPC across all types (only positive values)
    /// </summary>
    public int GetTotalTokensWithNPC(string npcId)
    {
        Dictionary<ConnectionType, int> tokens = GetTokensWithNPC(npcId);
        return tokens.Values.Where(v => v > 0).Sum();
    }

    /// <summary>
    /// Get the primary connection type with an NPC (highest token count)
    /// </summary>
    public ConnectionType GetPrimaryConnection(string npcId)
    {
        return _relationshipTracker.GetPrimaryConnection(npcId);
    }

    /// <summary>
    /// Get relationship tier with an NPC based on total tokens
    /// </summary>
    public RelationshipTier GetRelationshipTier(string npcId)
    {
        return _relationshipTracker.GetRelationshipTier(npcId);
    }

    /// <summary>
    /// Check if player has any relationship with an NPC
    /// </summary>
    public bool HasRelationship(string npcId)
    {
        return GetTotalTokensWithNPC(npcId) > 0;
    }

    // ========== EFFECT CALCULATIONS ==========

    /// <summary>
    /// Calculate success bonus from tokens for a specific action
    /// </summary>
    public int CalculateTokenBonus(ConnectionType type, int baseChance)
    {
        return _tokenEffectProcessor.CalculateSuccessBonus(type, baseChance);
    }

    /// <summary>
    /// Get all active token modifiers from equipment
    /// </summary>
    public Dictionary<ConnectionType, float> GetActiveModifiers()
    {
        return _tokenEffectProcessor.GetActiveModifiers();
    }

    /// <summary>
    /// Check if a token type is enabled for generation (via equipment)
    /// </summary>
    public bool IsTokenTypeEnabled(ConnectionType type)
    {
        return _tokenEffectProcessor.IsTokenTypeEnabled(type);
    }

    // ========== UNLOCK OPERATIONS ==========

    /// <summary>
    /// Get all available unlocks for an NPC based on current tokens
    /// </summary>
    public List<TokenUnlock> GetAvailableUnlocks(string npcId)
    {
        return _tokenUnlockManager.GetAvailableUnlocks(npcId);
    }

    /// <summary>
    /// Check if a specific unlock is available
    /// </summary>
    public bool IsUnlockAvailable(string npcId, string unlockId)
    {
        return _tokenUnlockManager.IsUnlockAvailable(npcId, unlockId);
    }

    /// <summary>
    /// Get unlock requirements for an NPC
    /// </summary>
    public Dictionary<string, TokenRequirement> GetUnlockRequirements(string npcId)
    {
        return _tokenUnlockManager.GetUnlockRequirements(npcId);
    }

    // ========== DEBT OPERATIONS ==========

    /// <summary>
    /// Get all NPCs the player owes tokens to
    /// </summary>
    public List<DebtInfo> GetAllDebts()
    {
        return _relationshipTracker.GetAllDebts();
    }

    /// <summary>
    /// Check if player has any debts
    /// </summary>
    public bool HasAnyDebt()
    {
        return _relationshipTracker.HasAnyDebt();
    }

    /// <summary>
    /// Get debt to a specific NPC
    /// </summary>
    public Dictionary<ConnectionType, int> GetDebtToNPC(string npcId)
    {
        Dictionary<ConnectionType, int> debt = new Dictionary<ConnectionType, int>();
        foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
        {
            if (type != ConnectionType.None)
            {
                int leverage = GetLeverage(npcId, type);
                if (leverage > 0)
                {
                    debt[type] = leverage;
                }
            }
        }
        return debt;
    }

    // ========== QUERY OPERATIONS ==========

    /// <summary>
    /// Get all NPCs with whom the player has tokens
    /// </summary>
    public List<string> GetNPCsWithTokens()
    {
        return _connectionTokenManager.GetNPCsWithTokens();
    }

    /// <summary>
    /// Get summary of all token relationships
    /// </summary>
    public TokenSummary GetTokenSummary()
    {
        return new TokenSummary
        {
            TotalTrust = GetTotalTokensOfType(ConnectionType.Trust),
            TotalCommerce = GetTotalTokensOfType(ConnectionType.Diplomacy),
            TotalStatus = GetTotalTokensOfType(ConnectionType.Status),
            TotalShadow = GetTotalTokensOfType(ConnectionType.Shadow),
            NPCsWithRelationships = GetNPCsWithTokens().Count,
            TotalDebts = GetAllDebts().Count,
            ActiveModifiers = GetActiveModifiers()
        };
    }
}

// ========== SUPPORTING TYPES ==========

public class TokenSummary
{
    public int TotalTrust { get; set; }
    public int TotalCommerce { get; set; }
    public int TotalStatus { get; set; }
    public int TotalShadow { get; set; }
    public int NPCsWithRelationships { get; set; }
    public int TotalDebts { get; set; }
    public Dictionary<ConnectionType, float> ActiveModifiers { get; set; }
}

public class DebtInfo
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public Dictionary<ConnectionType, int> Debts { get; set; }
    public int TotalDebt { get; set; }
}

public class TokenUnlock
{
    public string UnlockId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TokenRequirement Requirement { get; set; }
    public bool IsAvailable { get; set; }
}

public enum RelationshipTier
{
    None = 0,
    Acquaintance = 1,  // 1-2 tokens
    Friend = 2,         // 3-5 tokens
    CloseFriend = 3,    // 6-8 tokens
    Confidant = 4,      // 9-12 tokens
    InnerCircle = 5     // 13+ tokens
}
