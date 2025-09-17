using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

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
    private readonly AtmosphereManager atmosphereManager;
    private readonly FocusManager focusManager;
    private readonly GameWorld gameWorld;
    private readonly Random random;

    public CategoricalEffectResolver(
        TokenMechanicsManager tokenManager,
        AtmosphereManager atmosphereManager,
        FocusManager focusManager,
        GameWorld gameWorld)
    {
        this.tokenManager = tokenManager;
        this.atmosphereManager = atmosphereManager;
        this.focusManager = focusManager;
        this.gameWorld = gameWorld;
        this.random = new Random();
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
            RapportChange = 0,
            FlowChange = 0,
            CardsToAdd = new List<CardInstance>(),
            FocusAdded = 0,
            AtmosphereTypeChange = null,
            SpecialEffect = "",
            EndsConversation = false
        };

        int magnitude = GetMagnitude(card.Difficulty);
        magnitude = ApplyAtmosphereModifiers(magnitude, card.SuccessType, session);

        switch (card.SuccessType)
        {
            case SuccessEffectType.Rapport:
                result.RapportChange = magnitude;
                result.SpecialEffect = $"+{magnitude} rapport";
                break;

            case SuccessEffectType.Threading:
                // Draw cards equal to magnitude
                for (int i = 0; i < magnitude; i++)
                {
                    CardInstance drawn = session.Deck.DrawCard();
                    if (drawn != null)
                    {
                        result.CardsToAdd.Add(drawn);
                    }
                }
                result.SpecialEffect = $"Draw {magnitude} cards";
                break;

            case SuccessEffectType.Atmospheric:
                // Use magnitude to determine atmosphere tier
                AtmosphereType? newAtmosphere = GetAtmosphereFromMagnitude(magnitude);
                if (newAtmosphere.HasValue)
                {
                    result.AtmosphereTypeChange = newAtmosphere.Value;
                    result.SpecialEffect = $"Set {newAtmosphere.Value} atmosphere";
                }
                break;

            case SuccessEffectType.Focusing:
                result.FocusAdded = magnitude;
                result.SpecialEffect = $"+{magnitude} focus";
                break;

            case SuccessEffectType.Promising:
                // Move obligation to position 1 and gain rapport based on magnitude
                result.RapportChange = magnitude * 2; // Promising gives double rapport
                result.SpecialEffect = $"Promise made, +{magnitude * 2} rapport";
                // Queue manipulation handled by ConversationOrchestrator
                break;

            case SuccessEffectType.Advancing:
                // Advances flow battery by magnitude (which may trigger connection state change)
                // Flow battery changes are what actually drive connection state advancement
                result.FlowChange = magnitude; // Add flow equal to magnitude
                result.SpecialEffect = $"+{magnitude} flow";
                break;

            case SuccessEffectType.None:
            default:
                // No effect
                break;
        }

        // Handle Synchronized atmosphere (effect happens twice)
        if (session.CurrentAtmosphere == AtmosphereType.Synchronized && card.SuccessType != SuccessEffectType.None)
        {
            // Double the effect (except for Atmospheric which doesn't make sense to double)
            if (card.SuccessType == SuccessEffectType.Rapport)
            {
                result.RapportChange *= 2;
                result.SpecialEffect += " (synchronized)";
            }
            else if (card.SuccessType == SuccessEffectType.Threading)
            {
                // Draw again
                for (int i = 0; i < magnitude; i++)
                {
                    CardInstance drawn = session.Deck.DrawCard();
                    if (drawn != null)
                    {
                        result.CardsToAdd.Add(drawn);
                    }
                }
                result.SpecialEffect += " (synchronized)";
            }
            else if (card.SuccessType == SuccessEffectType.Focusing)
            {
                result.FocusAdded *= 2;
                result.SpecialEffect += " (synchronized)";
            }
            else if (card.SuccessType == SuccessEffectType.Advancing)
            {
                result.FlowChange *= 2;
                result.SpecialEffect += " (synchronized)";
            }
        }

        return result;
    }

    /// <summary>
    /// PROJECTION: Returns what WOULD happen on card failure without modifying state.
    /// Calculates hand clearing, rapport loss, card discards, etc.
    /// The caller decides whether to apply these projected changes.
    /// </summary>
    public CardEffectResult ProcessFailureEffect(CardInstance card, ConversationSession session)
    {
        CardEffectResult result = new CardEffectResult
        {
            Card = card,
            RapportChange = 0,
            FlowChange = 0,
            CardsToAdd = new List<CardInstance>(),
            FocusAdded = 0,
            AtmosphereTypeChange = null,
            SpecialEffect = "",
            EndsConversation = false
        };

        int magnitude = GetMagnitude(card.Difficulty);
        magnitude = ApplyAtmosphereModifiers(magnitude, SuccessEffectType.Rapport, session); // Use Rapport for failure magnitude

        switch (card.FailureType)
        { 
            case FailureEffectType.Backfire:
                // Negative rapport based on magnitude
                result.RapportChange = -magnitude;
                result.SpecialEffect = $"-{magnitude} rapport";
                break;

            case FailureEffectType.ForceListen:
                // PROJECTION: Would deplete focus to force LISTEN on next turn
                PlayerStats playerStats = gameWorld.GetPlayer().Stats;
                if (!card.IgnoresFailureListen(playerStats))
                {
                    // Indicate focus would be depleted
                    result.FocusAdded = -session.GetAvailableFocus();
                    result.SpecialEffect = "Must LISTEN next turn";
                }
                else
                {
                    result.SpecialEffect = "Immune to forced LISTEN (Level 5)";
                }
                break;

            case FailureEffectType.None:
            default:
                // No additional effect
                break;
        }

        // Failure always clears atmosphere to Neutral
        result.AtmosphereTypeChange = AtmosphereType.Neutral;

        return result;
    }

    /// <summary>
    /// PROJECTION: Returns what WOULD happen when card exhausts without modifying state.
    /// Exhaust effects are ALWAYS penalties (negative rapport, focus loss, etc.)
    /// The caller decides whether to apply these projected penalties.
    /// </summary>
    public CardEffectResult ProcessExhaustEffect(CardInstance card, ConversationSession session)
    {
        CardEffectResult result = new CardEffectResult
        {
            Card = card,
            RapportChange = 0,
            FlowChange = 0,
            CardsToAdd = new List<CardInstance>(),
            FocusAdded = 0,
            AtmosphereTypeChange = null,
            SpecialEffect = "",
            EndsConversation = false
        };

        // Only Impulse and Opening cards can have exhaust effects
        if (card.Persistence == PersistenceType.Thought)
        {
            return result;
        }

        int magnitude = GetMagnitude(card.Difficulty);

        switch (card.ExhaustType)
        {
            case ExhaustEffectType.Threading:
                // PENALTY: Lose cards from hand when exhausted
                // Note: CardsToAdd with negative semantics means cards to remove
                // The UI interprets this as "lose X cards" based on exhaust context
                for (int i = 0; i < magnitude; i++)
                {
                    // We populate CardsToAdd to indicate how many cards would be lost
                    // The actual removal is handled by the caller
                    result.CardsToAdd.Add(new CardInstance { Template = new ConversationCard() });
                }
                result.SpecialEffect = $"Exhausted: Lose {magnitude} cards";
                break;

            case ExhaustEffectType.Focusing:
                // PENALTY: Lose focus when exhausted
                result.FocusAdded = -magnitude; // Negative focus = penalty
                result.SpecialEffect = $"Exhausted: -{magnitude} focus";
                break;

            case ExhaustEffectType.Regret:
                // PENALTY: Lose rapport when not played - the cost of silence
                result.RapportChange = -magnitude;
                result.SpecialEffect = $"Regret: -{magnitude} rapport";
                break;

            case ExhaustEffectType.None:
            default:
                // No effect when exhausted
                break;
        }

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
    /// Apply atmosphere modifiers to magnitude
    /// </summary>
    private int ApplyAtmosphereModifiers(int baseMagnitude, SuccessEffectType effectType, ConversationSession session)
    {
        int magnitude = baseMagnitude;

        switch (session.CurrentAtmosphere)
        {
            case AtmosphereType.Volatile:
                // Rapport effects ±1
                if (effectType == SuccessEffectType.Rapport)
                {
                    magnitude += random.Next(0, 2) == 0 ? -1 : 1;
                }
                break;

            case AtmosphereType.Focused:
                // All success magnitudes +1
                magnitude += 1;
                break;

            case AtmosphereType.Exposed:
                // All magnitudes doubled
                magnitude *= 2;
                break;

            // Synchronized handled separately in effect processing
        }

        return Math.Max(1, magnitude); // Never go below 1
    }

    /// <summary>
    /// Get atmosphere type from magnitude (for Atmospheric success type)
    /// Progression: Patient → Receptive → Focused → Synchronized
    /// </summary>
    private AtmosphereType? GetAtmosphereFromMagnitude(int magnitude)
    {
        return magnitude switch
        {
            1 => AtmosphereType.Patient,      // 0 patience cost - minor benefit
            2 => AtmosphereType.Receptive,    // +1 card on LISTEN - moderate benefit
            3 => AtmosphereType.Focused,      // +20% success - strong benefit
            4 => AtmosphereType.Synchronized, // Next effect twice - powerful benefit
            _ => AtmosphereType.Patient
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
    /// Calculate success percentage based on card difficulty and atmosphere
    /// </summary>
    public int CalculateSuccessPercentage(CardInstance card, ConversationSession session)
    {
        int baseChance = card.Difficulty switch
        {
            Difficulty.VeryEasy => 85,
            Difficulty.Easy => 70,
            Difficulty.Medium => 50,
            Difficulty.Hard => 30,
            Difficulty.VeryHard => 15,
            _ => 50
        };

        // Apply atmosphere modifiers
        if (session.CurrentAtmosphere == AtmosphereType.Focused)
        {
            baseChance += 20; // +20% success for focused atmosphere
        }

        // Apply level bonuses
        // Standard progression: +10% at level 2, +10% at level 4, then +10% every even level
        int levelBonus = card.Level switch
        {
            1 => 0,
            2 => 10,
            3 => 10,
            4 => 20,
            5 => 20,
            _ => 20 + ((card.Level - 5) / 2) * 10  // +10% every even level after 5
        };

        baseChance += levelBonus;

        return Math.Clamp(baseChance, 0, 100);
    }

    /// <summary>
    /// Check if action succeeds using pre-rolled value
    /// </summary>
    public bool CheckSuccessWithPreRoll(int preRoll, int successPercentage, ConversationSession session)
    {
        // Pre-roll is 1-100, success if roll <= successPercentage
        return preRoll <= successPercentage;
    }
}

/// <summary>
/// Result of processing a card effect
/// </summary>
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int RapportChange { get; set; }
    public int FlowChange { get; set; } // For Advancing effect type
    public List<CardInstance> CardsToAdd { get; set; }
    public int FocusAdded { get; set; }
    public AtmosphereType? AtmosphereTypeChange { get; set; }
    public bool EndsConversation { get; set; }
    public string SpecialEffect { get; set; }
}