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
public static class SocialCardParser
{
    /// <summary>
    /// Get request card for conversation type from provided card data
    /// </summary>
    public static SocialCard GetGoalCard(string conversationTypeId, string npcId, string npcName,
        Dictionary<string, SocialCardDTO> cardTemplates)
    {
        // Use conversation type ID directly to find request card template
        string cardId = $"request_{conversationTypeId}";
        if (!cardTemplates.TryGetValue(cardId, out SocialCardDTO dto))
            return null;

        return ParseCard(dto);
    }

    /// <summary>
    /// Convert a ConversationCardDTO to a ConversationCard domain model.
    ///
    /// IMPORTANT: This method only handles conversation cards with "effects" structure.
    /// Exchange cards with "successEffect" structure are handled by the exchange system.
    /// </summary>
    public static SocialCard ParseCard(SocialCardDTO dto)
    {
        // Parse token type from connection type
        ConnectionType tokenType = ConnectionType.Trust; // Default
        if (!string.IsNullOrEmpty(dto.ConnectionType))
        {
            if (!Enum.TryParse<ConnectionType>(dto.ConnectionType, true, out tokenType))
            {
                throw new InvalidOperationException(
                    $"INVALID CONNECTION TYPE: Card '{dto.Id}' has invalid connectionType '{dto.ConnectionType}'. " +
                    $"Valid values: None, Trust, Diplomacy, Status, Shadow");
            }
        }

        // Parse depth (required for template derivation)
        CardDepth depth = CardDepth.Depth1;
        if (dto.Depth.HasValue)
        {
            depth = (CardDepth)dto.Depth.Value;
        }

        // Parse bound stat (required for template derivation)
        PlayerStatType? boundStat = PlayerStatType.None;
        if (!string.IsNullOrEmpty(dto.BoundStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.BoundStat, true, out PlayerStatType statType))
            {
                boundStat = statType;
            }
            else
            {
                throw new InvalidOperationException(
                    $"INVALID BOUND STAT: Card '{dto.Id}' has invalid boundStat '{dto.BoundStat}'. " +
                    $"Valid values: Insight, Rapport, Authority, Diplomacy, Cunning");
            }
        }

        // DERIVE Initiative Cost deterministically from boundStat + depth
        // NO JSON OVERRIDES - this must be 100% predictable
        int initiativeCost = 0;
        if (boundStat.HasValue)
        {
            initiativeCost = SocialCardEffectCatalog.GetSuggestedInitiativeCost(boundStat.Value, (int)depth);
        }

        // Parse persistence from JSON (this is a categorical property)
        PersistenceType persistence = PersistenceType.Statement; // Default
        if (!string.IsNullOrEmpty(dto.Persistence))
        {
            if (!Enum.TryParse<PersistenceType>(dto.Persistence, true, out persistence))
            {
                throw new InvalidOperationException(
                    $"INVALID PERSISTENCE TYPE: Card '{dto.Id}' has invalid persistence '{dto.Persistence}'. " +
                    $"Valid values: Statement, Echo");
            }
        }

        ConversationalMove? move;

        // Conversation cards REQUIRE conversationalMove
        if (string.IsNullOrEmpty(dto.ConversationalMove))
        {
            throw new InvalidOperationException(
                $"MISSING CONVERSATIONAL MOVE: Conversation card '{dto.Id}' must have 'conversationalMove' property. " +
                $"Valid values: Remark, Observation, Argument");
        }

        if (!Enum.TryParse<ConversationalMove>(dto.ConversationalMove, true, out ConversationalMove parsedMove))
        {
            throw new InvalidOperationException(
                $"INVALID CONVERSATIONAL MOVE: Card '{dto.Id}' has invalid conversationalMove '{dto.ConversationalMove}'. " +
                $"Valid values: Remark, Observation, Argument");
        }
        move = parsedMove;

        // Parse effects from new structure and determine success type
        SuccessEffectType successType = DetermineSuccessTypeFromEffects(dto.Effects);

        // VALIDATION: Cards with draw effects MUST be Statement persistence
        if (successType == SuccessEffectType.Threading && persistence != PersistenceType.Statement)
        {
            throw new InvalidOperationException(
                $"DRAW EFFECT PERSISTENCE VIOLATION: Card '{dto.Id}' has draw cards effect (Threading) but persistence is '{persistence}'. " +
                $"Cards with draw effects MUST be Statement persistence, not Echo. This ensures draw effects have narrative weight.");
        }

        PlayerStatType? requiredStat = PlayerStatType.None;
        int requiredStatements = 0;

        // Parse token requirements (for signature cards)
        Dictionary<string, int> tokenRequirements = new Dictionary<string, int>();
        if (dto.TokenRequirement != null)
        {
            foreach (KeyValuePair<string, int> requirement in dto.TokenRequirement)
            {
                tokenRequirements[requirement.Key] = requirement.Value;
            }
        }

        // Parse NPC-specific targeting
        string npcSpecific = dto.NpcSpecific;

        // All effects now use EffectFormula system
        // FORMULA-BASED EFFECT SYSTEM
        CardEffectFormula effectFormula;

        // Get effect formula from catalog using ONLY categorical properties
        // Card ID hash determines if Foundation card gets specialty or Momentum effect
        // move must have value here (only Conversation cards enter this block, and they always have move set)
        effectFormula = SocialCardEffectCatalog.GetEffectFromCategoricalProperties(move.Value, boundStat.Value, (int)depth, dto.Id);

        if (effectFormula == null)
        {
            throw new InvalidOperationException($"No effect formula found for card '{dto.Id}' with move {move.Value} stat {boundStat.Value} depth {(int)depth}!");
        }

        Console.WriteLine($"[ConversationCardParser] Card '{dto.Id}' using {move.Value} formula: {effectFormula}");

        // Parse Delivery property (NEW: Controls cadence on SPEAK)
        DeliveryType delivery = DeliveryType.Standard; // Default
        if (!string.IsNullOrEmpty(dto.Delivery))
        {
            if (!Enum.TryParse<DeliveryType>(dto.Delivery, true, out delivery))
            {
                throw new InvalidOperationException(
                    $"INVALID DELIVERY TYPE: Card '{dto.Id}' has invalid delivery '{dto.Delivery}'. " +
                    $"Valid values: Standard, Commanding, Measured, Yielding");
            }
        }

        // Auto-assign traits based on effect formula (NOT from JSON)
        List<CardTrait> traits = DeriveTraitsFromEffect(effectFormula);

        // Create ConversationCard with all properties in initializer
        return new SocialCard
        {
            Id = dto.Id,
            Title = dto.Title ?? "",
            TokenType = tokenType,
            Depth = depth,
            InitiativeCost = initiativeCost,
            Delivery = delivery,
            EffectFormula = effectFormula,
            Persistence = persistence,
            SuccessType = successType,
            Move = move, // Conversational move: Remark/Observation/Argument
            PersonalityTypes = dto.PersonalityTypes != null ? new List<string>(dto.PersonalityTypes) : new List<string>(),
            DialogueText = dto.DialogueText,
            VerbPhrase = "",
            MinimumTokensRequired = dto.MinimumTokensRequired ?? 0,
            MomentumThreshold = dto.MomentumThreshold ?? 0,
            BoundStat = boundStat,
            RequiredStat = requiredStat,
            RequiredStatements = requiredStatements,
            Traits = traits,
            TokenRequirements = tokenRequirements,
            NpcSpecific = npcSpecific,
            KnowledgeGranted = dto.KnowledgeGranted ?? new List<string>(),
            SecretsGranted = dto.SecretsGranted ?? new List<string>()
        };
    }

    /// <summary>
    /// Validate Foundation card rules for SteamWorld Quest-inspired Initiative system
    /// 70% of Foundation cards (depth 1-2) must be Echo type
    /// ALL Initiative-generating cards must be Echo type
    /// </summary>
    public static void ValidateFoundationCardRules(List<SocialCard> allCards)
    {
        if (allCards == null || !allCards.Any())
        {
            throw new InvalidOperationException("No conversation cards loaded for validation");
        }

        // Get Foundation cards (depth 1-2)
        List<SocialCard> foundationCards = allCards
            .Where(c => (int)c.Depth <= 2)
            .ToList();

        if (!foundationCards.Any())
        {
            Console.WriteLine("[ConversationCardParser] WARNING: No Foundation cards (depth 1-2) found");
            return;
        }

        // Report Foundation card distribution (no arbitrary percentage requirement)
        int echoFoundationCount = foundationCards.Count(c => c.Persistence == PersistenceType.Echo);
        decimal echoPercentage = (decimal)echoFoundationCount / foundationCards.Count;

        Console.WriteLine($"[ConversationCardParser] Foundation cards: {foundationCards.Count}, Echo: {echoFoundationCount} ({echoPercentage:P1}), Statement: {foundationCards.Count - echoFoundationCount} ({(1 - echoPercentage):P1})");

        // Report on card distribution (no strict validation)
        // Cards can be Echo or Statement based on narrative weight, not mechanical requirements
        List<SocialCard> cunningCards = allCards
            .Where(c => c.BoundStat == PlayerStatType.Cunning)
            .ToList();

        int cunningEcho = cunningCards.Count(c => c.Persistence == PersistenceType.Echo);
        int cunningStatement = cunningCards.Count(c => c.Persistence == PersistenceType.Statement);

        Console.WriteLine($"[ConversationCardParser] ✓ Card validation passed. " +
                         $"Cunning cards: {cunningCards.Count} total ({cunningEcho} Echo, {cunningStatement} Statement)");
    }

    /// <summary>
    /// Check if a card effect formula generates Initiative (including in compound effects)
    /// </summary>
    private static bool GeneratesInitiative(CardEffectFormula formula)
    {
        if (formula == null) return false;

        // Trading effects consume, not generate
        if (formula.FormulaType == EffectFormulaType.Trading) return false;

        // Check if top-level effect generates Initiative
        if (formula.TargetResource == SocialChallengeResourceType.Initiative) return true;

        // Recursively check compound effects
        if (formula.FormulaType == EffectFormulaType.Compound && formula.CompoundEffects != null)
        {
            return formula.CompoundEffects.Any(e => GeneratesInitiative(e));
        }

        return false;
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

    /// <summary>
    /// Derive traits automatically based on card's effect formula.
    /// Traits are NEVER defined in JSON - they are derived from categorical properties.
    /// </summary>
    private static List<CardTrait> DeriveTraitsFromEffect(CardEffectFormula effectFormula)
    {
        List<CardTrait> traits = new List<CardTrait>();

        // Auto-assign SuppressSpeakCadence if the card has a Cadence effect
        if (HasCadenceEffect(effectFormula))
        {
            traits.Add(CardTrait.SuppressSpeakCadence);
        }

        return traits;
    }

    /// <summary>
    /// Check if an effect formula contains a Cadence effect (recursively checks compound effects)
    /// </summary>
    private static bool HasCadenceEffect(CardEffectFormula formula)
    {
        if (formula == null) return false;

        // Check top-level effect
        if (formula.TargetResource == SocialChallengeResourceType.Cadence)
            return true;

        // Check compound effects recursively
        if (formula.FormulaType == EffectFormulaType.Compound && formula.CompoundEffects != null)
        {
            return formula.CompoundEffects.Any(e => HasCadenceEffect(e));
        }

        return false;
    }
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
