using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stateless parser for converting conversation card DTOs to domain models
/// </summary>
public static class ConversationCardParser
{

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

        // Parse 4-resource system properties
        CardDepth depth = CardDepth.Depth1;
        if (dto.Depth.HasValue)
        {
            depth = (CardDepth)dto.Depth.Value;
        }

        int initiativeCost = dto.InitiativeCost ?? 0;

        // Parse categorical properties
        PersistenceType persistence = PersistenceType.Statement;
        if (!string.IsNullOrEmpty(dto.Persistence))
        {
            Enum.TryParse<PersistenceType>(dto.Persistence, true, out persistence);
        }

        // Parse effects from new structure and determine success type
        SuccessEffectType successType = DetermineSuccessTypeFromEffects(dto.Effects);
        if (successType == SuccessEffectType.None && !string.IsNullOrEmpty(dto.SuccessType))
        {
            // Fall back to legacy SuccessType if new effects don't specify
            Enum.TryParse<SuccessEffectType>(dto.SuccessType, true, out successType);
        }

        FailureEffectType failureType = FailureEffectType.None;
        if (!string.IsNullOrEmpty(dto.FailureType))
        {
            Enum.TryParse<FailureEffectType>(dto.FailureType, true, out failureType);
        }

        // Parse card type from DTO type field
        CardType cardType = string.IsNullOrEmpty(dto.Type) ? CardType.Conversation : dto.Type.ToLower() switch
        {
            "letter" => CardType.Letter,
            "promise" => CardType.Promise,
            "burdengoal" => CardType.Letter,
            "observation" => CardType.Observation,
            "normal" => CardType.Conversation,
            _ => CardType.Conversation
        };

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
        if (cardType == CardType.Letter || cardType == CardType.Promise || cardType == CardType.Letter)
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

        // Parse alternative costs
        List<AlternativeCost> alternativeCosts = new List<AlternativeCost>();
        if (dto.AlternativeCosts != null && dto.AlternativeCosts.Any())
        {
            foreach (var altCostDto in dto.AlternativeCosts)
            {
                alternativeCosts.Add(new AlternativeCost
                {
                    Condition = altCostDto.Condition ?? "always",
                    ReducedInitiativeCost = altCostDto.ReducedInitiativeCost,
                    MomentumCost = altCostDto.MomentumCost,
                    Description = altCostDto.Description ?? ""
                });
            }
        }

        // Parse scaling formula
        ScalingFormula scalingFormula = null;
        if (dto.ScalingEffect != null)
        {
            scalingFormula = new ScalingFormula
            {
                ScalingType = dto.ScalingEffect.ScalingType ?? "None",
                BaseEffect = dto.ScalingEffect.BaseEffect,
                Multiplier = dto.ScalingEffect.Multiplier,
                Formula = dto.ScalingEffect.Formula ?? ""
            };
        }

        // Parse effects from new structure
        int? effectInitiative = dto.Effects?.Success?.Initiative;
        int? effectMomentum = dto.Effects?.Success?.Momentum;
        int? effectDoubt = dto.Effects?.Success?.Doubt;
        int? effectDrawCards = dto.Effects?.Success?.DrawCards;
        decimal? effectMomentumMultiplier = dto.Effects?.Success?.MomentumMultiplier;

        // Create ConversationCard with all properties in initializer
        return new ConversationCard
        {
            Id = customId ?? dto.Id,
            Description = dto.Description ?? "",
            CardType = cardType,
            Category = category,
            TokenType = tokenType,
            Depth = depth,
            InitiativeCost = initiativeCost,
            AlternativeCosts = alternativeCosts,
            ScalingEffect = scalingFormula,
            // New 4-resource effects
            EffectInitiative = effectInitiative,
            EffectMomentum = effectMomentum,
            EffectDoubt = effectDoubt,
            EffectDrawCards = effectDrawCards,
            EffectMomentumMultiplier = effectMomentumMultiplier,
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
    /// Determine success type from new effects structure
    /// </summary>
    private static SuccessEffectType DetermineSuccessTypeFromEffects(CardEffectsDTO effects)
    {
        if (effects?.Success == null) return SuccessEffectType.None;

        // Check for momentum effects (Strike)
        if (effects.Success.Momentum.HasValue && effects.Success.Momentum.Value > 0)
        {
            return SuccessEffectType.Strike;
        }

        // Check for momentum multiplier (DoubleMomentum)
        if (effects.Success.MomentumMultiplier.HasValue && effects.Success.MomentumMultiplier.Value > 1)
        {
            return SuccessEffectType.DoubleMomentum;
        }

        // Check for doubt reduction (Soothe)
        if (effects.Success.Doubt.HasValue && effects.Success.Doubt.Value < 0)
        {
            return SuccessEffectType.Soothe;
        }

        // Check for card drawing (Threading)
        if (effects.Success.DrawCards.HasValue && effects.Success.DrawCards.Value > 0)
        {
            return SuccessEffectType.Threading;
        }

        return SuccessEffectType.None;
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

    // 4-Resource System Properties
    public int? Depth { get; set; } // 1-10 depth system
    public int? InitiativeCost { get; set; } // Replaces Focus
    public CardEffectsDTO Effects { get; set; } // New effects structure
    public List<AlternativeCostDTO> AlternativeCosts { get; set; }
    public ScalingEffectDTO ScalingEffect { get; set; }

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

/// <summary>
/// DTO for card effects in 4-resource system
/// </summary>
public class CardEffectsDTO
{
    public CardSuccessEffectsDTO Success { get; set; }
}

/// <summary>
/// DTO for success effects in 4-resource system
/// </summary>
public class CardSuccessEffectsDTO
{
    public int? Initiative { get; set; }
    public int? Momentum { get; set; }
    public int? Doubt { get; set; }
    public int? DrawCards { get; set; }
    public decimal? MomentumMultiplier { get; set; }
}

/// <summary>
/// DTO for alternative costs in 4-resource system
/// </summary>
public class AlternativeCostDTO
{
    public string Condition { get; set; }
    public int ReducedInitiativeCost { get; set; }
    public int MomentumCost { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// DTO for scaling effects in 4-resource system
/// </summary>
public class ScalingEffectDTO
{
    public string ScalingType { get; set; }
    public int BaseEffect { get; set; }
    public decimal Multiplier { get; set; }
    public string Formula { get; set; }
}