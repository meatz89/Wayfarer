using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Resolves card effects based on categorical properties and difficulty-based magnitude
/// </summary>
public class CategoricalEffectResolver
{
    private readonly TokenMechanicsManager tokenManager;
    private readonly AtmosphereManager atmosphereManager;
    private readonly FocusManager focusManager;
    private readonly CardDeckManager deckManager;
    private readonly Random random;

    public CategoricalEffectResolver(
        TokenMechanicsManager tokenManager,
        AtmosphereManager atmosphereManager,
        FocusManager focusManager,
        CardDeckManager deckManager)
    {
        this.tokenManager = tokenManager;
        this.atmosphereManager = atmosphereManager;
        this.focusManager = focusManager;
        this.deckManager = deckManager;
        this.random = new Random();
    }

    /// <summary>
    /// Process a card's success effect based on its categorical properties
    /// </summary>
    public CardEffectResult ProcessSuccessEffect(CardInstance card, ConversationSession session)
    {
        CardEffectResult result = new CardEffectResult
        {
            Card = card,
            RapportChange = 0,
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
                // Advance connection state by 1 (ignores magnitude)
                result.SpecialEffect = "Connection advanced";
                // State change handled by ConversationOrchestrator
                break;

            case SuccessEffectType.None:
            default:
                // No effect
                break;
        }

        // Handle Synchronized atmosphere (effect happens twice)
        if (session.CurrentAtmosphere == AtmosphereType.Synchronized && card.SuccessType != SuccessEffectType.None)
        {
            // Double the effect (except for Atmospheric and Advancing which don't make sense to double)
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
        }

        return result;
    }

    /// <summary>
    /// Process a card's failure effect based on its categorical properties
    /// </summary>
    public CardEffectResult ProcessFailureEffect(CardInstance card, ConversationSession session)
    {
        CardEffectResult result = new CardEffectResult
        {
            Card = card,
            RapportChange = 0,
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
            case FailureEffectType.Overreach:
                // Clear entire hand - catastrophic conversation breakdown
                int cardsCleared = session.ActiveCards.Count;
                session.ActiveCards.Clear();
                result.SpecialEffect = $"Overreach! All {cardsCleared} cards discarded";
                break;

            case FailureEffectType.Backfire:
                // Negative rapport based on magnitude
                result.RapportChange = -magnitude;
                result.SpecialEffect = $"-{magnitude} rapport";
                break;

            case FailureEffectType.Disrupting:
                // Discard all cards with focus 3+ from hand
                List<CardInstance> toDiscard = session.ActiveCards.Cards
                    .Where(c => c.Focus >= 3)
                    .ToList();
                foreach (var discarded in toDiscard)
                {
                    session.ActiveCards.Remove(discarded);
                    session.Deck.DiscardCard(discarded);
                }
                result.SpecialEffect = $"Disrupted {toDiscard.Count} high-focus cards";
                break;

            case FailureEffectType.None:
            default:
                // No additional effect beyond forced LISTEN
                break;
        }

        // Failure always clears atmosphere to Neutral
        result.AtmosphereTypeChange = AtmosphereType.Neutral;

        return result;
    }

    /// <summary>
    /// Process a card's exhaust effect (when removed unplayed)
    /// </summary>
    public CardEffectResult ProcessExhaustEffect(CardInstance card, ConversationSession session)
    {
        CardEffectResult result = new CardEffectResult
        {
            Card = card,
            RapportChange = 0,
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
                // Draw cards when discarded
                for (int i = 0; i < magnitude; i++)
                {
                    CardInstance drawn = session.Deck.DrawCard();
                    if (drawn != null)
                    {
                        result.CardsToAdd.Add(drawn);
                    }
                }
                result.SpecialEffect = $"Exhausted: Draw {magnitude} cards";
                break;

            case ExhaustEffectType.Focusing:
                // Restore focus when discarded
                result.FocusAdded = magnitude;
                result.SpecialEffect = $"Exhausted: +{magnitude} focus";
                break;

            case ExhaustEffectType.Regret:
                // Lose rapport when not played - the cost of silence
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
}

/// <summary>
/// Result of processing a card effect
/// </summary>
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int RapportChange { get; set; }
    public List<CardInstance> CardsToAdd { get; set; }
    public int FocusAdded { get; set; }
    public AtmosphereType? AtmosphereTypeChange { get; set; }
    public string SpecialEffect { get; set; }
    public bool EndsConversation { get; set; }
}