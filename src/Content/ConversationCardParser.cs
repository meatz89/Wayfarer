using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stateless parser for converting conversation card DTOs to domain models.
///
/// CRITICAL ARCHITECTURAL PRINCIPLE: Conversation cards and exchange cards are PARALLEL SYSTEMS.
/// - Conversation cards use "effects" structure and are parsed by this parser
/// - Exchange cards use "successEffect" structure and are handled by the exchange system
/// - They must NEVER be mixed or cross-parsed between systems
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
    /// Convert a ConversationCardDTO to a ConversationCard domain model.
    ///
    /// IMPORTANT: This method only handles conversation cards with "effects" structure.
    /// Exchange cards with "successEffect" structure are handled by the exchange system.
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

        // Parse card type from DTO type field - MUST be defined early as it's used in validation
        CardType cardType = string.IsNullOrEmpty(dto.Type) ? CardType.Conversation : dto.Type.ToLower() switch
        {
            "letter" => CardType.Letter,
            "promise" => CardType.Promise,
            "burdengoal" => CardType.Letter,
            "observation" => CardType.Observation,
            "request" => CardType.Conversation, // Request cards are conversation cards
            "exchange" => CardType.Conversation, // Exchange cards are conversation cards
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
                    AddDrawOnSuccess = bonusDto.AddDrawOnSuccess
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

        // Parse token requirements (for signature cards)
        Dictionary<string, int> tokenRequirements = new Dictionary<string, int>();
        if (dto.TokenRequirement != null)
        {
            foreach (var requirement in dto.TokenRequirement)
            {
                tokenRequirements[requirement.Key] = requirement.Value;
            }
        }

        // Parse NPC-specific targeting
        string npcSpecific = dto.NpcSpecific;

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

        // VALIDATE effects based on card type
        if (cardType == CardType.Conversation)
        {
            // Regular conversation cards MUST have new effects structure
            if (dto.Effects?.Success == null)
            {
                throw new InvalidOperationException($"Conversation card '{dto.Id}' is missing effects.success definition in JSON! All conversation cards MUST have effects defined.");
            }

            var successEffects = dto.Effects.Success;

            // At least ONE effect must be defined for conversation cards
            if (!successEffects.Initiative.HasValue &&
                !successEffects.Momentum.HasValue &&
                !successEffects.Doubt.HasValue &&
                !successEffects.DrawCards.HasValue &&
                !successEffects.MomentumMultiplier.HasValue &&
                string.IsNullOrEmpty(successEffects.OfferLetter) &&
                string.IsNullOrEmpty(successEffects.AddObligation) &&
                !successEffects.GainCoins.HasValue &&
                string.IsNullOrEmpty(successEffects.GainCard))
            {
                throw new InvalidOperationException($"Conversation card '{dto.Id}' has no effects defined! At least one effect (Initiative, Momentum, Doubt, DrawCards, MomentumMultiplier, OfferLetter, AddObligation, GainCoins, or GainCard) MUST be specified.");
            }
        }

        // Parse effects from new structure (if present)
        int? effectInitiative = dto.Effects?.Success?.Initiative;
        int? effectMomentum = dto.Effects?.Success?.Momentum;
        int? effectDoubt = dto.Effects?.Success?.Doubt;
        int? effectDrawCards = dto.Effects?.Success?.DrawCards;
        decimal? effectMomentumMultiplier = dto.Effects?.Success?.MomentumMultiplier;

        // Create ConversationCard with all properties in initializer
        return new ConversationCard
        {
            Id = customId ?? dto.Id,
            Title = dto.Title ?? "",
            CardType = cardType,
            Category = category,
            TokenType = tokenType,
            Depth = depth,
            InitiativeCost = initiativeCost,
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
            PersonalityTypes = dto.PersonalityTypes != null ? new List<string>(dto.PersonalityTypes) : new List<string>(),
            DialogueText = dto.DialogueText,
            VerbPhrase = "",
            MinimumTokensRequired = dto.MinimumTokensRequired ?? 0,
            MomentumThreshold = dto.MomentumThreshold ?? 0,
            BoundStat = boundStat,
            LevelBonuses = levelBonuses,
            MomentumScaling = momentumScaling,
            DoubtScaling = doubtScaling,
            TokenRequirements = tokenRequirements,
            NpcSpecific = npcSpecific
        };
    }

    /// <summary>
    /// Validate Foundation card rules for SteamWorld Quest-inspired Initiative system
    /// 70% of Foundation cards (depth 1-2) must be Echo type
    /// ALL Initiative-generating cards must be Echo type
    /// </summary>
    public static void ValidateFoundationCardRules(List<ConversationCard> allCards)
    {
        if (allCards == null || !allCards.Any())
        {
            throw new InvalidOperationException("No conversation cards loaded for validation");
        }

        // Get Foundation cards (depth 1-2)
        List<ConversationCard> foundationCards = allCards
            .Where(c => (int)c.Depth <= 2)
            .ToList();

        if (!foundationCards.Any())
        {
            Console.WriteLine("[ConversationCardParser] WARNING: No Foundation cards (depth 1-2) found");
            return;
        }

        // Check 70% Echo rule for Foundation cards
        int echoFoundationCount = foundationCards.Count(c => c.Persistence == PersistenceType.Echo);
        decimal echoPercentage = (decimal)echoFoundationCount / foundationCards.Count;

        Console.WriteLine($"[ConversationCardParser] Foundation cards: {foundationCards.Count}, Echo: {echoFoundationCount} ({echoPercentage:P1})");

        if (echoPercentage < 0.70m)
        {
            throw new InvalidOperationException(
                $"Foundation card sustainability validation FAILED: Only {echoPercentage:P1} of Foundation cards are Echo type. " +
                $"Minimum 70% required for sustainable Initiative generation. " +
                $"Echo Foundation cards: {echoFoundationCount}/{foundationCards.Count}");
        }

        // Check that ALL Initiative-generating cards are Echo type
        List<ConversationCard> initiativeGenerators = allCards
            .Where(c => c.EffectInitiative.HasValue && c.EffectInitiative.Value > 0)
            .ToList();

        List<ConversationCard> nonEchoInitiativeCards = initiativeGenerators
            .Where(c => c.Persistence != PersistenceType.Echo)
            .ToList();

        if (nonEchoInitiativeCards.Any())
        {
            string violatingCards = string.Join(", ", nonEchoInitiativeCards.Select(c => c.Id));
            throw new InvalidOperationException(
                $"Initiative generation validation FAILED: Cards that generate Initiative must be Echo type for repeatability. " +
                $"Violating cards: {violatingCards}");
        }

        Console.WriteLine($"[ConversationCardParser] ✓ Foundation card validation passed: " +
                         $"{echoPercentage:P1} Echo Foundation cards, " +
                         $"{initiativeGenerators.Count} Initiative generators (all Echo)");
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
    public string Title { get; set; }
    public string DialogueText { get; set; }
    public int? MinimumTokensRequired { get; set; }

    // Categorical properties - define behavior through context
    public string Category { get; set; } // Expression/Realization/Regulation (optional - auto-determined from effect type if not specified)
    public string Persistence { get; set; } // Thought/Impulse/Opening
    public string SuccessType { get; set; } // Strike/Soothe/Threading/DoubleMomentum/Atmospheric/Focusing/Promising/Advancing/None

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
    public ScalingEffectDTO ScalingEffect { get; set; }

    // Level bonuses (optional, uses default progression if not specified)
    public List<CardLevelBonusDTO> LevelBonuses { get; set; }

    // Token requirements for signature cards
    public Dictionary<string, int> TokenRequirement { get; set; }

    // NPC-specific targeting for signature cards
    public string NpcSpecific { get; set; }
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

    // Custom game effects for request/goal cards
    public string OfferLetter { get; set; }
    public string AddObligation { get; set; }
    public int? GainCoins { get; set; }
    public string GainCard { get; set; }
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