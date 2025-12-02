/// <summary>
/// Calculates and applies token effects from equipment, relationships, and game state.
/// </summary>
public class TokenEffectProcessor
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly ConnectionTokenManager _tokenManager;

    // Base success bonus per token (flat integer additions)
    private const int BASE_TRUST_BONUS = 1;      // +1 per Trust token
    private const int BASE_COMMERCE_BONUS = 1;   // +1 per Diplomacy token
    private const int BASE_STATUS_BONUS = 2;     // +2 per Status token
    private const int BASE_SHADOW_BONUS = 1;     // +1 per Shadow token

    public TokenEffectProcessor(
        GameWorld gameWorld,
        ItemRepository itemRepository,
        ConnectionTokenManager tokenManager)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Apply equipment bonuses to token generation (additive, not multiplicative)
    /// </summary>
    public int ApplyGenerationModifiers(ConnectionType tokenType, int baseAmount)
    {
        if (baseAmount <= 0) return baseAmount;

        int equipmentBonus = GetEquipmentTokenBonus(tokenType);
        int modifiedAmount = baseAmount + equipmentBonus;

        return Math.Max(0, modifiedAmount);
    }

    /// <summary>
    /// Calculate success bonus from tokens for a specific action
    /// </summary>
    public int CalculateSuccessBonus(ConnectionType type, int baseChance)
    {
        int tokenCount = _tokenManager.GetTotalTokensOfType(type);
        if (tokenCount <= 0) return 0;

        int bonusPerToken = GetBonusPerToken(type);
        int totalBonus = tokenCount * bonusPerToken;

        // Cap bonus at 50% to prevent trivializing challenges
        int maxBonus = baseChance / 2;
        return Math.Min(totalBonus, maxBonus);
    }

    /// <summary>
    /// Calculate relationship effect on action success
    /// HIGHLANDER: Accept NPC object
    /// </summary>
    public int CalculateRelationshipBonus(NPC npc, ConnectionType actionType)
    {
        if (npc == null) return 0;

        // DOMAIN COLLECTION: Use List with LINQ queries
        List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npc);
        int relevantTokens = tokens.FirstOrDefault(t => t.Type == actionType)?.Count ?? 0;

        // Negative tokens (debt) apply penalties
        if (relevantTokens < 0)
        {
            int penalty = Math.Abs(relevantTokens) * GetBonusPerToken(actionType);
            return -penalty; // Return negative for penalty
        }

        // Positive tokens apply bonuses
        int bonus = relevantTokens * GetBonusPerToken(actionType);

        // Additional synergy bonus if multiple token types are high
        int synergyBonus = CalculateSynergyBonus(tokens);

        return bonus + synergyBonus;
    }

    /// <summary>
    /// Get all active token bonuses from equipment (flat integer additions)
    /// DOMAIN COLLECTION PRINCIPLE: Return explicit properties, not Dictionary
    /// </summary>
    public TokenBonuses GetActiveBonuses()
    {
        Player player = _gameWorld.GetPlayer();
        int trustBonus = 0;
        int diplomacyBonus = 0;
        int statusBonus = 0;
        int shadowBonus = 0;

        // Apply equipment bonuses (additive stacking)
        // DOMAIN COLLECTION PRINCIPLE: Use explicit properties on Item
        foreach (Item item in player.Inventory.GetAllItems())
        {
            if (item != null)
            {
                // Add bonuses together (e.g., +1 from item A + +2 from item B = +3 total)
                trustBonus = trustBonus + item.TrustGenerationBonus;
                diplomacyBonus = diplomacyBonus + item.DiplomacyGenerationBonus;
                statusBonus = statusBonus + item.StatusGenerationBonus;
                shadowBonus = shadowBonus + item.ShadowGenerationBonus;
            }
        }

        return new TokenBonuses
        {
            TrustBonus = trustBonus,
            DiplomacyBonus = diplomacyBonus,
            StatusBonus = statusBonus,
            ShadowBonus = shadowBonus
        };
    }

    /// <summary>
    /// Check if a token type is enabled for generation (via equipment)
    /// </summary>
    public bool IsTokenTypeEnabled(ConnectionType type)
    {
        // Base token types are always enabled
        if (type == ConnectionType.Trust || type == ConnectionType.Diplomacy)
        {
            return true;
        }

        // Status and Shadow might require special equipment
        Player player = _gameWorld.GetPlayer();

        foreach (Item item in player.Inventory.GetAllItems())
        {
            // HIGHLANDER: GetAllItems() returns List<Item>, not List<string>
            if (item != null && item.EnablesTokenGeneration != null)
            {
                if (item.EnablesTokenGeneration.Contains(type))
                {
                    return true;
                }
            }
        }

        // Check if player has any tokens of this type already (grandfathered in)
        return _tokenManager.GetTotalTokensOfType(type) > 0;
    }

    /// <summary>
    /// Calculate token decay over time (flat integer decay, not percentage-based)
    /// </summary>
    public int CalculateTokenDecay(ConnectionType type, int currentTokens, int daysSinceInteraction)
    {
        if (currentTokens <= 0 || daysSinceInteraction < 7) return 0;

        // Different token types decay at different base rates
        int baseDecayPerWeek = GetBaseDecayPerWeek(type);

        // Decay accelerates with time (additive escalation)
        int weeksWithoutContact = daysSinceInteraction / 7;
        int additionalDecay = weeksWithoutContact - 1; // First week uses base rate, subsequent weeks add +1 each

        // Calculate total decay: base rate + time escalation
        int decay = baseDecayPerWeek + Math.Max(0, additionalDecay);

        // Never decay more than half of current tokens in one go
        return Math.Min(decay, currentTokens / 2);
    }

    /// <summary>
    /// Get equipment-based token generation bonus (flat integer addition)
    /// </summary>
    private int GetEquipmentTokenBonus(ConnectionType tokenType)
    {
        Player player = _gameWorld.GetPlayer();
        int totalBonus = 0;

        // Check all items in inventory for token bonuses
        // DOMAIN COLLECTION PRINCIPLE: Use explicit properties on Item
        foreach (Item item in player.Inventory.GetAllItems())
        {
            if (item != null)
            {
                // Add bonuses together
                totalBonus = totalBonus + item.GetTokenGenerationBonus(tokenType);
            }
        }

        return totalBonus;
    }

    /// <summary>
    /// Get base bonus per token based on type
    /// </summary>
    private int GetBonusPerToken(ConnectionType type)
    {
        switch (type)
        {
            case ConnectionType.Trust:
                return BASE_TRUST_BONUS;
            case ConnectionType.Diplomacy:
                return BASE_COMMERCE_BONUS;
            case ConnectionType.Status:
                return BASE_STATUS_BONUS;
            case ConnectionType.Shadow:
                return BASE_SHADOW_BONUS;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Calculate synergy bonus from having multiple token types
    /// DOMAIN COLLECTION: Query List with LINQ
    /// </summary>
    private int CalculateSynergyBonus(List<TokenCount> tokens)
    {
        int typesWithTokens = 0;
        int totalTokens = 0;

        foreach (TokenCount tokenCount in tokens)
        {
            if (tokenCount.Type == ConnectionType.None) continue;

            if (tokenCount.Count > 0)
            {
                typesWithTokens++;
                totalTokens += tokenCount.Count;
            }
        }

        // No synergy with only one type
        if (typesWithTokens <= 1) return 0;

        // Synergy bonus: 2% per additional type, multiplied by average tokens
        int averageTokens = totalTokens / typesWithTokens;
        int synergyBonus = (typesWithTokens - 1) * 2 * averageTokens;

        return Math.Min(synergyBonus, 20); // Cap at 20% bonus
    }

    /// <summary>
    /// Get base decay per week for token type (flat integers)
    /// </summary>
    private int GetBaseDecayPerWeek(ConnectionType type)
    {
        switch (type)
        {
            case ConnectionType.Trust:
                return 1; // Trust decays slowly (1 token per week)
            case ConnectionType.Diplomacy:
                return 1; // Diplomacy is most stable (1 token per week)
            case ConnectionType.Status:
                return 2; // Status decays faster (2 tokens per week)
            case ConnectionType.Shadow:
                return 2; // Shadow decays fastest (2 tokens per week)
            default:
                return 1;
        }
    }
}

/// <summary>
/// Token bonuses from equipment (flat integer additions).
/// DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum (ConnectionType)
/// </summary>
public class TokenBonuses
{
    public int TrustBonus { get; set; }
    public int DiplomacyBonus { get; set; }
    public int StatusBonus { get; set; }
    public int ShadowBonus { get; set; }

    public int GetBonus(ConnectionType type) => type switch
    {
        ConnectionType.Trust => TrustBonus,
        ConnectionType.Diplomacy => DiplomacyBonus,
        ConnectionType.Status => StatusBonus,
        ConnectionType.Shadow => ShadowBonus,
        _ => 0
    };
}
