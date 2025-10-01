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
            UnderstandingChange = 0,
            CardsToAdd = new List<CardInstance>(),
            EffectDescription = "",
            EndsConversation = false
        };

        // ONLY use comprehensive effects from card model - NO LEGACY FALLBACKS
        // Parser ensures all cards have effects, so this should always succeed
        result = BuildComprehensiveEffectResult(card, session);

        return result;
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
    /// Build comprehensive effect result from EffectFormula system
    /// </summary>
    private CardEffectResult BuildComprehensiveEffectResult(CardInstance card, ConversationSession session)
    {
        var result = new CardEffectResult
        {
            Card = card,
            MomentumChange = 0,
            DoubtChange = 0,
            InitiativeChange = 0,
            CadenceChange = 0,
            UnderstandingChange = 0, // NEW: Understanding effects
            CardsToDraw = 0,
            CardsToAdd = new List<CardInstance>(),
            EffectDescription = "",
            EndsConversation = false
        };

        var effectsOnly = new List<string>();  // Card effects only (for card display)
        var allChanges = new List<string>();   // All resource changes (for SPEAK preview)

        // STEAMWORLD QUEST PATTERN: Derive Initiative generation from tier
        // Foundation tier (depth 1-2) generates Initiative (like Strike/Upgrade cards generate steam)
        // This is a CATEGORICAL EFFECT of the tier, not an explicit property
        int initiativeGen = card.ConversationCardTemplate?.GetInitiativeGeneration() ?? 0;
        if (initiativeGen > 0)
        {
            result.InitiativeChange += initiativeGen;
            // Add to allChanges for SPEAK preview, but NOT to effectsOnly (it's a tier effect, not card effect)
            allChanges.Add($"+{initiativeGen} Initiative");
        }

        // Use EffectFormula system for singular card effect
        CardEffectFormula formula = card.ConversationCardTemplate?.EffectFormula;
        if (formula == null)
        {
            result.EffectDescription = "No effect formula";
            return result;
        }

        // Execute the formula to get projected effects
        if (formula.FormulaType == EffectFormulaType.Compound)
        {
            if (formula.CompoundEffects != null)
            {
                foreach (var subFormula in formula.CompoundEffects)
                {
                    ApplyFormulaToResult(subFormula, session, result, effectsOnly);
                }
            }
        }
        else
        {
            ApplyFormulaToResult(formula, session, result, effectsOnly);
        }

        // Add effects to allChanges
        allChanges.AddRange(effectsOnly);

        // Build descriptions
        result.EffectOnlyDescription = effectsOnly.Any() ? string.Join(", ", effectsOnly) : "No effect";
        result.EffectDescription = allChanges.Any() ? string.Join(", ", allChanges) : "No effect";

        return result;
    }

    private void ApplyFormulaToResult(CardEffectFormula formula, ConversationSession session, CardEffectResult result, List<string> effects)
    {
        // Calculate the effect value
        int effectValue = formula.CalculateEffect(session);

        // Apply to appropriate resource
        switch (formula.TargetResource)
        {
            case ConversationResourceType.Initiative:
                result.InitiativeChange += effectValue;
                if (effectValue != 0)
                    effects.Add(effectValue > 0 ? $"+{effectValue} Initiative" : $"{effectValue} Initiative");
                break;

            case ConversationResourceType.Momentum:
                result.MomentumChange += effectValue;
                if (effectValue != 0)
                    effects.Add(effectValue > 0 ? $"+{effectValue} Momentum" : $"{effectValue} Momentum");
                break;

            case ConversationResourceType.Doubt:
                result.DoubtChange += effectValue;
                if (effectValue != 0)
                    effects.Add(effectValue > 0 ? $"+{effectValue} Doubt" : $"{-effectValue} Doubt");
                break;

            case ConversationResourceType.Cadence:
                result.CadenceChange += effectValue;
                if (effectValue != 0)
                    effects.Add(effectValue > 0 ? $"+{effectValue} Cadence" : $"{effectValue} Cadence");
                break;

            case ConversationResourceType.Cards:
                result.CardsToDraw += effectValue;
                if (effectValue > 0)
                    effects.Add($"Draw {effectValue} card{(effectValue == 1 ? "" : "s")}");
                break;

            case ConversationResourceType.Understanding:
                result.UnderstandingChange += effectValue;
                if (effectValue != 0)
                    effects.Add(effectValue > 0 ? $"+{effectValue} Understanding" : $"{effectValue} Understanding");
                break;
        }
    }
}

/// <summary>
/// Result of processing a card effect - COMPLETE projection of all resource changes
/// </summary>
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int InitiativeChange { get; set; }
    public int MomentumChange { get; set; }
    public int DoubtChange { get; set; }
    public int CadenceChange { get; set; } // Cadence resource tracking (rarely used - most cadence comes from Delivery)
    public int UnderstandingChange { get; set; } // NEW: Understanding resource - unlocks tiers, persists through LISTEN
    public int CardsToDraw { get; set; } // Number of cards to draw
    public List<CardInstance> CardsToAdd { get; set; }
    public bool EndsConversation { get; set; }
    public string EffectOnlyDescription { get; set; } // Card effect formula only (excludes Initiative generation property)
    public string EffectDescription { get; set; } // All resource changes (includes Initiative generation for SPEAK preview)
}

