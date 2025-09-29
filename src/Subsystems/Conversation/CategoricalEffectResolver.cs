using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// PROJECTION PRINCIPLE: This resolver is a pure projection function that returns
/// what WOULD happen without modifying any game state. Both UI (for preview display)
/// and game logic (for actual execution) call these methods to get projections.
///
/// The resolver NEVER modifies state directly - it only returns CardEffectResult
/// projections that describe what changes would occur. The caller is responsible
/// for actually applying these changes if/when appropriate.
///
/// This ensures:
/// - UI can accurately preview effects before they happen
/// - Game logic gets consistent effect calculations
/// - No side effects occur during preview operations
/// - Single source of truth for effect calculations
/// </summary>
public class CategoricalEffectResolver
{
    private readonly TokenMechanicsManager tokenManager;
    private readonly GameWorld gameWorld;

    public CategoricalEffectResolver(
        TokenMechanicsManager tokenManager,
        GameWorld gameWorld)
    {
        this.tokenManager = tokenManager;
        this.gameWorld = gameWorld;
    }

    /// <summary>
    /// PROJECTION: Returns what WOULD happen on card success without modifying state.
    /// Calculates rapport changes, cards to draw, focus to add, atmosphere to set, etc.
    /// The caller decides whether to apply these projected changes.
    /// </summary>
    public CardEffectResult ProcessSuccessEffect(CardInstance card, ConversationSession session)
    {
        CardEffectResult result = new CardEffectResult
        {
            Card = card,
            MomentumChange = 0,
            DoubtChange = 0,
            InitiativeChange = 0,
            CardsToAdd = new List<CardInstance>(),
            EffectDescription = "",
            EndsConversation = false
        };

        int magnitude = GetMagnitude(card.ConversationCardTemplate.Difficulty);

        // ONLY use comprehensive effects from card model - NO LEGACY FALLBACKS
        // Parser ensures all cards have effects, so this should always succeed
        result = BuildComprehensiveEffectResult(card, session);

        return result;
    }

    /// <summary>
    /// Get magnitude based on difficulty tier
    /// </summary>
    private int GetMagnitude(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.VeryEasy => 1,
            Difficulty.Easy => 1,
            Difficulty.Medium => 2,
            Difficulty.Hard => 3,
            Difficulty.VeryHard => 4,
            _ => 1
        };
    }


    /// <summary>
    /// Calculate magnitude from difficulty
    /// </summary>
    public int GetMagnitudeFromDifficulty(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.VeryEasy => 1,
            Difficulty.Easy => 1,
            Difficulty.Medium => 2,
            Difficulty.Hard => 3,
            Difficulty.VeryHard => 4,
            _ => 1
        };
    }

    /// <summary>
    /// DETERMINISTIC: Check if card succeeds based on clear rules (no randomness)
    /// Cards succeed based on personality rules, resource requirements, and momentum thresholds
    /// </summary>
    public bool CheckCardSuccess(CardInstance card, ConversationSession session)
    {
        // Goal cards (Letters, Promises) always succeed if momentum threshold is met
        if (card.ConversationCardTemplate.CardType == CardType.Letter || card.ConversationCardTemplate.CardType == CardType.Promise || card.ConversationCardTemplate.CardType == CardType.Letter)
        {
            return session.CurrentMomentum >= card.ConversationCardTemplate.MomentumThreshold;
        }

        // Regular conversation cards ALWAYS succeed - no failure possible
        // The design explicitly states cards have deterministic effects, not failure chances
        return true;
    }


    /// <summary>
    /// Generate descriptive text for Strike effects based on scaling formulas and mechanics
    /// </summary>
    private string GetStrikeEffectDescription(CardInstance card, ConversationSession session, PlayerStats player, int momentumGain)
    {
        ConversationCard template = card.ConversationCardTemplate;

        // Check for scaling formulas
        if (template.MomentumScaling != ScalingType.None)
        {
            switch (template.MomentumScaling)
            {
                case ScalingType.CardsInMind:
                    return $"Gain momentum = cards in hand ({session.Deck.HandSize} = {momentumGain})";

                case ScalingType.CurrentDoubt:
                    int doubtReduction = 10 - session.CurrentDoubt;
                    return $"Gain momentum = (10 - current doubt) = {doubtReduction}";

                case ScalingType.DoubleMomentum:
                    return $"Gain momentum = current momentum × 2 ({session.CurrentMomentum} × 2 = {momentumGain})";

                case ScalingType.CurrentInitiative:
                    return $"Gain momentum = current initiative ÷ 3 ({session.CurrentInitiative} ÷ 3 = {momentumGain})";
            }
        }

        // Check for stat bonuses on Expression cards
        if (template.Category == CardCategory.Expression && template.BoundStat.HasValue)
        {
            int statLevel = player.GetLevel(template.BoundStat.Value);
            if (statLevel >= 2)
            {
                int baseEffect = template.GetBaseMomentumFromProperties();
                int statBonus = statLevel - 1;
                return $"Gain {baseEffect} momentum (+{statBonus} from {template.BoundStat.Value} Lv{statLevel} = {momentumGain} total)";
            }
        }

        // Default basic momentum gain
        return $"Gain {momentumGain} momentum";
    }

    /// <summary>
    /// Get effective momentum gain from card, checking new effects structure first
    /// </summary>
    private int GetEffectiveMomentumGain(CardInstance card, ConversationSession session)
    {
        // Try to get from card's stored effect data first (from JSON parsing)
        if (TryGetStoredEffect(card, "Momentum", out int storedMomentum))
        {
            return storedMomentum;
        }

        // Use template calculation
        PlayerStats player = gameWorld.GetPlayer().Stats;
        return card.ConversationCardTemplate.GetMomentumEffect(session, player);
    }

    /// <summary>
    /// Get effective doubt change from card, checking new effects structure first
    /// </summary>
    private int GetEffectiveDoubtChange(CardInstance card, ConversationSession session)
    {
        // Try to get from card's stored effect data first
        if (TryGetStoredEffect(card, "Doubt", out int storedDoubt))
        {
            return storedDoubt;
        }

        // Use template calculation
        return card.ConversationCardTemplate.GetDoubtEffect(session);
    }

    /// <summary>
    /// Get effective card draw from card, checking new effects structure first
    /// </summary>
    private int GetEffectiveCardDraw(CardInstance card, ConversationSession session)
    {
        // Try to get from card's stored effect data first
        if (TryGetStoredEffect(card, "DrawCards", out int storedDraw))
        {
            return storedDraw;
        }

        // Use difficulty-based draw calculation
        return GetMagnitude(card.ConversationCardTemplate.Difficulty);
    }

    /// <summary>
    /// Get effective initiative change from card, checking new effects structure first
    /// </summary>
    private int GetEffectiveInitiativeChange(CardInstance card, ConversationSession session)
    {
        // Try to get from card's stored effect data first
        if (TryGetStoredEffect(card, "Initiative", out int storedInitiative))
        {
            return storedInitiative;
        }

        // No stored initiative effect - return 0
        return 0;
    }

    /// <summary>
    /// Try to get stored effect value from card's parsed effects from JSON
    /// </summary>
    private bool TryGetStoredEffect(CardInstance card, string effectName, out int effectValue)
    {
        effectValue = 0;

        if (card?.ConversationCardTemplate == null) return false;

        var template = card.ConversationCardTemplate;

        return effectName switch
        {
            "Momentum" => TryGetNullableInt(template.EffectMomentum, out effectValue),
            "Initiative" => TryGetNullableInt(template.EffectInitiative, out effectValue),
            "Doubt" => TryGetNullableInt(template.EffectDoubt, out effectValue),
            "DrawCards" => TryGetNullableInt(template.EffectDrawCards, out effectValue),
            _ => false
        };
    }

    /// <summary>
    /// Helper to extract int value from nullable
    /// </summary>
    private bool TryGetNullableInt(int? nullableValue, out int value)
    {
        if (nullableValue.HasValue)
        {
            value = nullableValue.Value;
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// Build comprehensive effect result from all available 4-resource effects
    /// </summary>
    private CardEffectResult BuildComprehensiveEffectResult(CardInstance card, ConversationSession session)
    {
        var result = new CardEffectResult
        {
            Card = card,
            MomentumChange = 0,
            DoubtChange = 0,
            InitiativeChange = 0,
            CardsToAdd = new List<CardInstance>(),
            EffectDescription = "",
            EndsConversation = false
        };

        var effects = new List<string>();

        // Check each effect type and accumulate
        if (TryGetStoredEffect(card, "Initiative", out int initiative) && initiative != 0)
        {
            result.InitiativeChange = initiative;
            effects.Add(initiative > 0 ? $"+{initiative} Initiative" : $"{initiative} Initiative");
        }

        if (TryGetStoredEffect(card, "Momentum", out int momentum) && momentum != 0)
        {
            result.MomentumChange = momentum;
            effects.Add(momentum > 0 ? $"+{momentum} Momentum" : $"{momentum} Momentum");
        }

        if (TryGetStoredEffect(card, "Doubt", out int doubt) && doubt != 0)
        {
            result.DoubtChange = doubt;
            effects.Add(doubt > 0 ? $"+{doubt} Doubt" : $"{-doubt} Doubt");
        }

        if (TryGetStoredEffect(card, "Cadence", out int cadence) && cadence != 0)
        {
            effects.Add(cadence > 0 ? $"+{cadence} Cadence" : $"{cadence} Cadence");
        }

        if (TryGetStoredEffect(card, "DrawCards", out int drawCards) && drawCards > 0)
        {
            effects.Add($"Draw {drawCards} card{(drawCards == 1 ? "" : "s")}");
        }

        // Handle momentum multiplier
        if (card.ConversationCardTemplate?.EffectMomentumMultiplier.HasValue == true)
        {
            decimal multiplier = card.ConversationCardTemplate.EffectMomentumMultiplier.Value;
            if (multiplier > 1)
            {
                result.MomentumChange = (int)(session.CurrentMomentum * multiplier) - session.CurrentMomentum;
                effects.Clear(); // Clear other effects for multiplier
                effects.Add($"Double Momentum ({session.CurrentMomentum} → {session.CurrentMomentum * (int)multiplier})");
            }
        }

        // Build final description
        result.EffectDescription = effects.Any() ? string.Join(", ", effects) : "No effect";

        return result;
    }
}

/// <summary>
/// Result of processing a card effect
/// </summary>
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int InitiativeChange { get; set; } // Replaces FlowChange in 4-resource system
    public int MomentumChange { get; set; }
    public int DoubtChange { get; set; }
    public List<CardInstance> CardsToAdd { get; set; }
    public bool EndsConversation { get; set; }
    public string EffectDescription { get; set; }
}

