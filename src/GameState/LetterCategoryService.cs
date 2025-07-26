using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service responsible for managing letter categories based on connection token thresholds.
/// Implements the core mechanic where building relationships unlocks better letter opportunities.
/// </summary>
public class LetterCategoryService
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly GameConfiguration _config;

    public LetterCategoryService(GameWorld gameWorld, ConnectionTokenManager connectionTokenManager,
        NPCRepository npcRepository, MessageSystem messageSystem, GameConfiguration config)
    {
        _gameWorld = gameWorld;
        _connectionTokenManager = connectionTokenManager;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _config = config;
    }

    /// <summary>
    /// Get the highest letter category available for an NPC based on player's token count with them.
    /// </summary>
    public LetterCategory GetAvailableCategory(string npcId, ConnectionType tokenType)
    {
        Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int tokenCount = npcTokens.GetValueOrDefault(tokenType, 0);

        // Check thresholds from highest to lowest
        if (tokenCount >= GameRules.TOKENS_PREMIUM_THRESHOLD) // 5+ tokens
            return LetterCategory.Premium;
        if (tokenCount >= GameRules.TOKENS_QUALITY_THRESHOLD) // 3-4 tokens
            return LetterCategory.Quality;
        if (tokenCount >= GameRules.TOKENS_BASIC_THRESHOLD) // 1-2 tokens
            return LetterCategory.Basic;

        // Not enough tokens for any category
        return LetterCategory.Basic; // Default, but should check CanOfferLetters first
    }

    /// <summary>
    /// Check if an NPC can offer letters based on token thresholds.
    /// </summary>
    public bool CanNPCOfferLetters(string npcId)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null || !npc.LetterTokenTypes.Any())
            return false;

        // Check if we have enough tokens with this NPC in ANY of their token types
        foreach (ConnectionType tokenType in npc.LetterTokenTypes)
        {
            Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
            if (npcTokens.GetValueOrDefault(tokenType, 0) >= GameRules.TOKENS_BASIC_THRESHOLD)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Get all available letter categories for an NPC across all their token types.
    /// </summary>
    public Dictionary<ConnectionType, LetterCategory> GetAvailableCategories(string npcId)
    {
        Dictionary<ConnectionType, LetterCategory> result = new Dictionary<ConnectionType, LetterCategory>();
        NPC npc = _npcRepository.GetById(npcId);

        if (npc == null) return result;

        foreach (ConnectionType tokenType in npc.LetterTokenTypes)
        {
            Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
            int tokenCount = npcTokens.GetValueOrDefault(tokenType, 0);

            if (tokenCount >= GameRules.TOKENS_BASIC_THRESHOLD)
            {
                result[tokenType] = GetAvailableCategory(npcId, tokenType);
            }
        }

        return result;
    }

    /// <summary>
    /// Get letter templates that match the available category for an NPC.
    /// </summary>
    public List<LetterTemplate> GetAvailableTemplates(string npcId, ConnectionType tokenType)
    {
        LetterCategory category = GetAvailableCategory(npcId, tokenType);
        Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int tokenCount = npcTokens.GetValueOrDefault(tokenType, 0);

        // If not enough tokens for basic category, return empty list
        if (tokenCount < GameRules.TOKENS_BASIC_THRESHOLD)
            return new List<LetterTemplate>();

        // Get all templates of the appropriate type and category or lower
        return _gameWorld.WorldState.LetterTemplates
            .Where(t => t.TokenType == tokenType &&
                       t.Category <= category &&
                       t.MinTokensRequired <= tokenCount)
            .ToList();
    }

    /// <summary>
    /// Check if a specific letter template is available based on token thresholds.
    /// </summary>
    public bool IsTemplateAvailable(LetterTemplate template, string npcId)
    {
        if (template == null || string.IsNullOrEmpty(npcId)) return false;

        Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int tokenCount = npcTokens.GetValueOrDefault(template.TokenType, 0);

        return tokenCount >= template.MinTokensRequired;
    }

    /// <summary>
    /// Get the payment range for a letter category.
    /// </summary>
    public (int min, int max) GetCategoryPaymentRange(LetterCategory category)
    {
        return category switch
        {
            LetterCategory.Basic => (_config.LetterPayment.BasicMin, _config.LetterPayment.BasicMax),      // 1-2 tokens
            LetterCategory.Quality => (_config.LetterPayment.QualityMin, _config.LetterPayment.QualityMax),   // 3-4 tokens
            LetterCategory.Premium => (_config.LetterPayment.PremiumMin, _config.LetterPayment.PremiumMax),  // 5+ tokens
            _ => (3, 5)
        };
    }

    /// <summary>
    /// Check and announce if crossing a category threshold.
    /// </summary>
    public void CheckCategoryUnlock(string npcId, ConnectionType tokenType, int oldTokenCount, int newTokenCount)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return;

        // Check each threshold
        if (oldTokenCount < GameRules.TOKENS_BASIC_THRESHOLD && newTokenCount >= GameRules.TOKENS_BASIC_THRESHOLD)
        {
            _messageSystem.AddSystemMessage(
                $"🔓 {npc.Name} now trusts you enough to offer {tokenType} letters!",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • Basic letters ({_config.LetterPayment.BasicMin}-{_config.LetterPayment.BasicMax} coins) are now available",
                SystemMessageTypes.Info
            );
        }
        else if (oldTokenCount < GameRules.TOKENS_QUALITY_THRESHOLD && newTokenCount >= GameRules.TOKENS_QUALITY_THRESHOLD)
        {
            _messageSystem.AddSystemMessage(
                $"⭐ Your relationship with {npc.Name} has grown stronger!",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • Quality {tokenType} letters ({_config.LetterPayment.QualityMin}-{_config.LetterPayment.QualityMax} coins) are now available",
                SystemMessageTypes.Info
            );
        }
        else if (oldTokenCount < GameRules.TOKENS_PREMIUM_THRESHOLD && newTokenCount >= GameRules.TOKENS_PREMIUM_THRESHOLD)
        {
            _messageSystem.AddSystemMessage(
                $"🌟 {npc.Name} considers you among their most trusted associates!",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • Premium {tokenType} letters ({_config.LetterPayment.PremiumMin}-{_config.LetterPayment.PremiumMax} coins) are now available",
                SystemMessageTypes.Info
            );
        }
    }

    /// <summary>
    /// Get a description of category requirements for UI display.
    /// </summary>
    public string GetCategoryRequirementDescription(LetterCategory category)
    {
        return category switch
        {
            LetterCategory.Basic => $"{GameRules.TOKENS_BASIC_THRESHOLD}+ tokens",
            LetterCategory.Quality => $"{GameRules.TOKENS_QUALITY_THRESHOLD}+ tokens",
            LetterCategory.Premium => $"{GameRules.TOKENS_PREMIUM_THRESHOLD}+ tokens",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get tokens needed to reach next category threshold.
    /// </summary>
    public int GetTokensToNextCategory(string npcId, ConnectionType tokenType)
    {
        Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int currentTokens = npcTokens.GetValueOrDefault(tokenType, 0);

        if (currentTokens < GameRules.TOKENS_BASIC_THRESHOLD)
            return GameRules.TOKENS_BASIC_THRESHOLD - currentTokens;
        if (currentTokens < GameRules.TOKENS_QUALITY_THRESHOLD)
            return GameRules.TOKENS_QUALITY_THRESHOLD - currentTokens;
        if (currentTokens < GameRules.TOKENS_PREMIUM_THRESHOLD)
            return GameRules.TOKENS_PREMIUM_THRESHOLD - currentTokens;

        return 0; // Already at maximum category
    }
}