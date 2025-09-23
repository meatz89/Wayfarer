using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Stateless parser for converting conversation card DTOs to domain models
/// </summary>
public static class ConversationCardParser
{
    /// <summary>
    /// Get all cards for an NPC's personality from provided card data
    /// </summary>
    public static List<ConversationCard> GetCardsForNPC(NPC npc,
        Dictionary<string, ConversationCardDTO> cardTemplates,
        Dictionary<PersonalityType, PersonalityCardMapping> personalityMappings)
    {
        List<ConversationCard> cards = new List<ConversationCard>();

        // Add universal cards - all conversation cards that aren't special types
        IEnumerable<ConversationCardDTO> universalCards = cardTemplates.Values
            .Where(c => c.Type == null || c.Type.ToLower() == "normal" || c.Type.ToLower() == "conversation");

        foreach (ConversationCardDTO cardDto in universalCards)
        {
            cards.Add(ConvertDTOToCard(cardDto, npc));
        }

        // Add personality-specific cards
        if (personalityMappings.TryGetValue(npc.PersonalityType, out PersonalityCardMapping mapping))
        {
            foreach (string cardId in mapping.Cards)
            {
                if (cardTemplates.TryGetValue(cardId, out ConversationCardDTO cardDto))
                {
                    cards.Add(ConvertDTOToCard(cardDto, npc));
                }
            }
        }

        return cards;
    }

    /// <summary>
    /// Get request card for conversation type from provided card data
    /// </summary>
    public static ConversationCard GetRequestCard(string conversationTypeId, string npcId, string npcName,
        Dictionary<string, ConversationCardDTO> cardTemplates)
    {
        // Use conversation type ID directly to find request card template
        string cardId = $"request_{conversationTypeId}";
        if (!cardTemplates.TryGetValue(cardId, out ConversationCardDTO dto))
            return null;

        // Convert to ConversationCard with customized ID for this specific NPC
        string customId = $"{cardId}_{npcId}";
        return ConvertDTOToCard(dto, null, customId);
    }

    /// <summary>
    /// Convert a ConversationCardDTO to a ConversationCard domain model
    /// </summary>
    public static ConversationCard ConvertDTOToCard(ConversationCardDTO dto, NPC npc = null, string customId = null)
    {
        // Parse token type from connection type
        ConnectionType tokenType = ConnectionType.Trust;
        if (!string.IsNullOrEmpty(dto.ConnectionType))
        {
            Enum.TryParse<ConnectionType>(dto.ConnectionType, true, out tokenType);
        }

        // Parse difficulty
        Difficulty difficulty = Difficulty.Medium;
        if (!string.IsNullOrEmpty(dto.Difficulty))
        {
            Enum.TryParse<Difficulty>(dto.Difficulty, true, out difficulty);
        }

        // Parse categorical properties
        PersistenceType persistence = PersistenceType.Thought;
        if (!string.IsNullOrEmpty(dto.Persistence))
        {
            Enum.TryParse<PersistenceType>(dto.Persistence, true, out persistence);
        }

        SuccessEffectType successType = SuccessEffectType.None;
        if (!string.IsNullOrEmpty(dto.SuccessType))
        {
            Enum.TryParse<SuccessEffectType>(dto.SuccessType, true, out successType);
        }

        FailureEffectType failureType = FailureEffectType.None;
        if (!string.IsNullOrEmpty(dto.FailureType))
        {
            Enum.TryParse<FailureEffectType>(dto.FailureType, true, out failureType);
        }

        // Parse card type from DTO type field
        CardType cardType = ParseCardType(dto);

        // Parse or determine card category
        CardCategory category = CardCategory.Expression; // Default
        if (!string.IsNullOrEmpty(dto.Category))
        {
            Enum.TryParse<CardCategory>(dto.Category, true, out category);
        }
        else
        {
            // Auto-determine category from success effect type
            category = ConversationCard.DetermineCategoryFromEffect(successType);
        }

        // Validate momentum threshold for goal cards
        if (cardType == CardType.Letter || cardType == CardType.Promise || cardType == CardType.BurdenGoal)
        {
            if (!dto.MomentumThreshold.HasValue)
            {
                throw new InvalidOperationException($"Goal card '{dto.Id}' of type {cardType} MUST have a momentumThreshold defined in JSON!");
            }
        }

        // Parse level bonuses if specified
        List<CardLevelBonus> levelBonuses = new List<CardLevelBonus>();
        if (dto.LevelBonuses != null && dto.LevelBonuses.Any())
        {
            foreach (CardLevelBonusDTO bonusDto in dto.LevelBonuses)
            {
                CardLevelBonus bonus = new CardLevelBonus
                {
                    SuccessBonus = bonusDto.SuccessBonus,
                    AddDrawOnSuccess = bonusDto.AddDrawOnSuccess,
                    IgnoreFailureListen = bonusDto.IgnoreFailureListen
                };

                // Parse persistence type if specified
                if (!string.IsNullOrEmpty(bonusDto.AddPersistence))
                {
                    if (Enum.TryParse<PersistenceType>(bonusDto.AddPersistence, true, out PersistenceType persistenceType))
                    {
                        bonus.AddPersistence = persistenceType;
                    }
                }

                levelBonuses.Add(bonus);
            }
        }

        // Parse bound stat
        PlayerStatType? boundStat = null;
        if (!string.IsNullOrEmpty(dto.BoundStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.BoundStat, true, out PlayerStatType statType))
            {
                boundStat = statType;
            }
        }

        // Apply ForceListen as fallback for cards with None failure effect
        if (failureType == FailureEffectType.None)
        {
            failureType = FailureEffectType.ForceListen;
        }

        // Parse momentum/doubt scaling properties
        ScalingType momentumScaling = ScalingType.None;
        if (!string.IsNullOrEmpty(dto.MomentumScaling))
        {
            if (!Enum.TryParse<ScalingType>(dto.MomentumScaling, out momentumScaling))
            {
                throw new InvalidOperationException($"Invalid MomentumScaling value '{dto.MomentumScaling}' for card '{dto.Id}'");
            }
        }

        ScalingType doubtScaling = ScalingType.None;
        if (!string.IsNullOrEmpty(dto.DoubtScaling))
        {
            if (!Enum.TryParse<ScalingType>(dto.DoubtScaling, out doubtScaling))
            {
                throw new InvalidOperationException($"Invalid DoubtScaling value '{dto.DoubtScaling}' for card '{dto.Id}'");
            }
        }

        // Create ConversationCard with all properties in initializer
        return new ConversationCard
        {
            Id = customId ?? dto.Id,
            Description = dto.Description ?? "",
            CardType = cardType,
            Category = category,
            TokenType = tokenType,
            Focus = dto.Focus,
            Difficulty = difficulty,
            Persistence = persistence,
            SuccessType = successType,
            FailureType = failureType,
            PersonalityTypes = dto.PersonalityTypes != null ? new List<string>(dto.PersonalityTypes) : new List<string>(),
            DialogueFragment = dto.DialogueFragment,
            VerbPhrase = "",
            MinimumTokensRequired = dto.MinimumTokensRequired ?? 0,
            MomentumThreshold = dto.MomentumThreshold ?? 0,
            BoundStat = boundStat,
            LevelBonuses = levelBonuses,
            MomentumScaling = momentumScaling,
            DoubtScaling = doubtScaling
        };
    }


    /// <summary>
    /// Convert NPCGoalCardDTO to ConversationCard
    /// </summary>
    public static ConversationCard ConvertGoalCardDTO(NPCGoalCardDTO dto)
    {
        // Determine card type based on the goal type
        CardType cardType = dto.Type?.ToLower() switch
        {
            "letter" => CardType.Letter,
            "promise" => CardType.Promise,
            "burdengoal" => CardType.BurdenGoal,
            _ => CardType.Promise // Default for goal cards
        };

        // Parse categorical properties
        PersistenceType persistence = PersistenceType.Thought; // Goal cards are always Thoughts
        if (!string.IsNullOrEmpty(dto.Persistence))
        {
            Enum.TryParse<PersistenceType>(dto.Persistence, true, out persistence);
        }

        SuccessEffectType successType = cardType == CardType.Letter ?
            SuccessEffectType.Advancing : SuccessEffectType.Promising;
        if (!string.IsNullOrEmpty(dto.SuccessType))
        {
            Enum.TryParse<SuccessEffectType>(dto.SuccessType, true, out successType);
        }

        FailureEffectType failureType = FailureEffectType.None;
        if (!string.IsNullOrEmpty(dto.FailureType))
        {
            Enum.TryParse<FailureEffectType>(dto.FailureType, true, out failureType);
        }

        // Parse difficulty if specified, otherwise use default
        Difficulty difficulty = Difficulty.VeryEasy; // Goal cards typically always succeed
        if (!string.IsNullOrEmpty(dto.Difficulty))
        {
            Enum.TryParse<Difficulty>(dto.Difficulty, true, out difficulty);
        }

        // Parse token type
        ConnectionType tokenType = ConnectionType.Trust; // Default
        if (!string.IsNullOrEmpty(dto.ConnectionType))
        {
            Enum.TryParse<ConnectionType>(dto.ConnectionType, true, out tokenType);
        }

        // Parse or determine card category
        CardCategory category = CardCategory.Expression; // Default
        if (!string.IsNullOrEmpty(dto.Category))
        {
            Enum.TryParse<CardCategory>(dto.Category, true, out category);
        }
        else
        {
            // Auto-determine category from success effect type
            category = ConversationCard.DetermineCategoryFromEffect(successType);
        }

        return new ConversationCard
        {
            Id = dto.Id,
            Description = dto.Description,
            CardType = cardType,
            Category = category,
            Focus = dto.Focus,
            DialogueFragment = dto.DialogueFragment,
            Difficulty = difficulty,
            Persistence = persistence,
            SuccessType = successType,
            FailureType = failureType,
            MomentumThreshold = dto.MomentumThreshold,
            TokenType = tokenType,
            VerbPhrase = "",
            PersonalityTypes = new List<string>(),
            LevelBonuses = new List<CardLevelBonus>()
        };
    }

    /// <summary>
    /// Parse card type from DTO type field
    /// </summary>
    private static CardType ParseCardType(ConversationCardDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Type))
            return CardType.Conversation;

        return dto.Type.ToLower() switch
        {
            "letter" => CardType.Letter,
            "promise" => CardType.Promise,
            "burdengoal" => CardType.BurdenGoal,
            "observation" => CardType.Observation,
            "normal" => CardType.Conversation,
            _ => CardType.Conversation // Default to conversation
        };
    }


    /// <summary>
    /// Parse resource type from string
    /// </summary>
    private static ResourceType? ParseResourceType(string name)
    {
        return name.ToLower() switch
        {
            "coins" => ResourceType.Coins,
            "health" => ResourceType.Health,
            "food" => ResourceType.Hunger,
            "hunger" => ResourceType.Hunger,
            _ => null
        };
    }

}

/// <summary>
/// DTO for the entire conversations.json file
/// </summary>
public class ConversationDataDTO
{
    public List<ConversationCardDTO> ConversationCards { get; set; }
    public Dictionary<string, PersonalityCardMapping> PersonalityCardMappings { get; set; }
    public Dictionary<string, List<string>> TokenCardUnlocks { get; set; }
}

/// <summary>
/// DTO for individual conversation cards
/// </summary>
public class ConversationCardDTO
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string ConnectionType { get; set; }
    public int Focus { get; set; }
    public int? MomentumThreshold { get; set; } // For request cards
    public string Description { get; set; }
    public string DialogueFragment { get; set; }
    public int? MinimumTokensRequired { get; set; }

    // Categorical properties - define behavior through context
    public string Category { get; set; } // Expression/Realization/Regulation (optional - auto-determined from effect type if not specified)
    public string Persistence { get; set; } // Thought/Impulse/Opening
    public string SuccessType { get; set; } // Strike/Soothe/Threading/DoubleMomentum/Atmospheric/Focusing/Promising/Advancing/None
    public string FailureType { get; set; } // Backfire/None

    // Personality targeting - which NPCs can use this card
    public List<string> PersonalityTypes { get; set; }

    // Difficulty determines magnitude
    public string Difficulty { get; set; }

    // Player stats system - which stat this card is bound to
    public string BoundStat { get; set; } // insight/rapport/authority/commerce/cunning

    // Momentum/Doubt scaling properties
    public string MomentumScaling { get; set; } // "None", "CardsInHand", "DoubtReduction", etc.
    public string DoubtScaling { get; set; } // "None", "DoubtHalved", "DoubtReduction"

    // Level bonuses (optional, uses default progression if not specified)
    public List<CardLevelBonusDTO> LevelBonuses { get; set; }
}

/// <summary>
/// DTO for card level bonuses from JSON
/// </summary>
public class CardLevelBonusDTO
{
    public int Level { get; set; }
    public int? SuccessBonus { get; set; }
    public string AddPersistence { get; set; }
    public int? AddDrawOnSuccess { get; set; }
    public bool? IgnoreFailureListen { get; set; }
}

/// <summary>
/// Personality to card mappings
/// </summary>
public class PersonalityCardMapping
{
    public List<string> Cards { get; set; }
    public string StateBias { get; set; }
}