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

        // Parse depth (required for template derivation)
        CardDepth depth = CardDepth.Depth1;
        if (dto.Depth.HasValue)
        {
            depth = (CardDepth)dto.Depth.Value;
        }

        // Parse bound stat (required for template derivation)
        PlayerStatType? boundStat = null;
        if (!string.IsNullOrEmpty(dto.BoundStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.BoundStat, true, out PlayerStatType statType))
            {
                boundStat = statType;
            }
        }

        // DERIVE properties from catalog (for standard cards with boundStat)
        int initiativeCost = 0;
        PersistenceType persistence = PersistenceType.Statement;

        if (boundStat.HasValue)
        {
            // Derive Initiative cost from catalog
            initiativeCost = CardEffectCatalog.GetSuggestedInitiativeCost(boundStat.Value, (int)depth);

            // Derive persistence: Foundation (1-2) = Echo, higher = Statement
            persistence = (int)depth <= 2 ? PersistenceType.Echo : PersistenceType.Statement;
        }

        // Allow JSON overrides (only if explicitly specified)
        if (dto.InitiativeCost.HasValue)
        {
            initiativeCost = dto.InitiativeCost.Value;
        }
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

        // boundStat already parsed above for template derivation

        // DERIVE Statement requirements from depth
        // Standard (3-4): Require depth-1 statements
        // Advanced (5-6): Require depth-1 statements
        // Master (7-8): Require depth-1 statements
        // Statement requirements: Get from JSON ONLY (no auto-derivation)
        PlayerStatType? requiredStat = null;
        int requiredStatements = 0;

        // Parse requiredStat from JSON
        if (!string.IsNullOrEmpty(dto.RequiredStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.RequiredStat, true, out PlayerStatType reqStatType))
            {
                requiredStat = reqStatType;
            }
        }

        // Parse requiredStatements from JSON
        if (dto.RequiredStatements.HasValue)
        {
            requiredStatements = dto.RequiredStatements.Value;
        }

        // NO AUTO-DERIVATION: Statement requirements must be explicit in JSON
        // Specification requires exact thresholds: 3, 4, 5, 8 (not depth-based formulas)

        // VALIDATION: Foundation tier must have NO signature variants
        if ((int)depth <= 2 && requiredStatements > 0)
        {
            throw new InvalidOperationException(
                $"FOUNDATION SIGNATURE VIOLATION: Card '{dto.Id}' (depth {(int)depth}) cannot have Statement requirements. " +
                $"Foundation tier (depths 1-2) must be 100% base cards accessible to all players. " +
                $"Signature variants must start at Standard tier (depth 3+).");
        }

        // VALIDATION: Signature cards must start at Standard tier (depth 3+)
        if (requiredStatements > 0 && (int)depth < 3)
        {
            throw new InvalidOperationException(
                $"SIGNATURE DEPTH VIOLATION: Card '{dto.Id}' has Statement requirements but is depth {(int)depth}. " +
                $"Signature variants must be depth 3 or higher.");
        }

        // VALIDATION: Statement requirements must match valid thresholds
        if (requiredStatements > 0)
        {
            var validThresholds = new[] { 3, 4, 5, 8 };
            if (!validThresholds.Contains(requiredStatements))
            {
                throw new InvalidOperationException(
                    $"INVALID STATEMENT REQUIREMENT: Card '{dto.Id}' requires {requiredStatements} statements. " +
                    $"Valid thresholds are: 3 (Standard), 4 (Standard), 5 (Advanced), 8 (Master).");
            }
        }

        // VALIDATION: RequiredStat must match BoundStat for signature variants
        if (requiredStatements > 0 && requiredStat.HasValue && boundStat.HasValue && requiredStat.Value != boundStat.Value)
        {
            throw new InvalidOperationException(
                $"SIGNATURE STAT MISMATCH: Card '{dto.Id}' requires {requiredStat.Value} statements but is bound to {boundStat.Value}. " +
                $"Signature cards must require statements of their own stat.");
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

        // DELETED: Legacy MomentumScaling, DoubtScaling, ScalingFormula parsing
        // All effects now use EffectFormula system

        // FORMULA-BASED EFFECT SYSTEM
        CardEffectFormula effectFormula = null;

        if (cardType == CardType.Conversation && boundStat.HasValue)
        {
            // Get effect formula from catalog based on boundStat + depth + variant
            string variantName = dto.EffectVariant ?? "Base"; // Default to first variant if not specified

            if (variantName == "Base")
            {
                // Get first (default) variant
                var variants = CardEffectCatalog.GetEffectVariants(boundStat.Value, (int)depth);
                effectFormula = variants.FirstOrDefault();
            }
            else
            {
                // Get specific variant by name
                effectFormula = CardEffectCatalog.GetEffectByVariantName(boundStat.Value, (int)depth, variantName);
            }

            if (effectFormula == null)
            {
                throw new InvalidOperationException($"No effect formula found for card '{dto.Id}' with stat {boundStat.Value} depth {(int)depth} variant '{variantName}'!");
            }

            Console.WriteLine($"[ConversationCardParser] Card '{dto.Id}' using formula: {effectFormula}");
        }

        // Create ConversationCard with all properties in initializer
        return new ConversationCard
        {
            Id = customId ?? dto.Id,
            Title = dto.Title ?? "",
            CardType = cardType,
            TokenType = tokenType,
            Depth = depth,
            InitiativeCost = initiativeCost,
            // Formula-based effect system
            EffectFormula = effectFormula,
            // OLD: ScalingEffect, EffectInitiative, etc. (deprecated, removed)
            Persistence = persistence,
            SuccessType = successType,
            PersonalityTypes = dto.PersonalityTypes != null ? new List<string>(dto.PersonalityTypes) : new List<string>(),
            DialogueText = dto.DialogueText,
            VerbPhrase = "",
            MinimumTokensRequired = dto.MinimumTokensRequired ?? 0,
            MomentumThreshold = dto.MomentumThreshold ?? 0,
            BoundStat = boundStat,
            RequiredStat = requiredStat,
            RequiredStatements = requiredStatements,
            LevelBonuses = levelBonuses,
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
            .Where(c => c.EffectFormula != null &&
                       c.EffectFormula.TargetResource == ConversationResourceType.Initiative &&
                       c.EffectFormula.FormulaType != EffectFormulaType.Trading) // Trading effects consume, not generate
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
    public string Persistence { get; set; } // Echo/Statement
    public string SuccessType { get; set; } // Strike/Soothe/Threading/DoubleMomentum/Atmospheric/Focusing/Promising/Advancing/None

    // Personality targeting - which NPCs can use this card
    public List<string> PersonalityTypes { get; set; }

    // Difficulty determines magnitude
    public string Difficulty { get; set; }

    // Player stats system - which stat this card is bound to
    public string BoundStat { get; set; } // insight/rapport/authority/commerce/cunning

    // Effect variant system - which formula variant to use from catalog
    public string EffectVariant { get; set; } // "Base", "Compound", "Scaling_Doubt", etc. (optional, defaults to "Base")

    // Statement requirement system - cards may require prior Statement cards to be playable
    public string RequiredStat { get; set; } // Which stat's Statement count to check
    public int? RequiredStatements { get; set; } // How many Statements of that stat are required

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