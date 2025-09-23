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
            MomentumChange = 0,
            DoubtChange = 0,
            FlowChange = 0,
            CardsToAdd = new List<CardInstance>(),
            FocusAdded = 0,
            AtmosphereTypeChange = null,
            EffectDescription = "",
            EndsConversation = false
        };

        int magnitude = GetMagnitude(card.Difficulty);
        magnitude = ApplyAtmosphereModifiers(magnitude, card.SuccessType, session);

        switch (card.SuccessType)
        {
            case SuccessEffectType.Strike:
                // Use card's momentum scaling formula with player stats for bonuses
                PlayerStats player = gameWorld.GetPlayer().Stats;
                int momentumGain = card.Template.GetMomentumEffect(session, player);
                result.MomentumChange = momentumGain;

                // Generate descriptive text based on scaling formula and card mechanics
                result.EffectDescription = GetStrikeEffectDescription(card, session, player, momentumGain);
                break;

            case SuccessEffectType.Soothe:
                // Check if this is a Realization card that needs momentum consumption
                if (card.Template.Category == CardCategory.Realization)
                {
                    var realizationEffect = GetRealizationEffect(card, session);
                    result.MomentumChange = -realizationEffect.MomentumCost;
                    result.DoubtChange = realizationEffect.DoubtReduction;
                    result.FlowChange = realizationEffect.FlowGain;
                    result.EffectDescription = realizationEffect.Description;
                }
                else if (card.Template.Category == CardCategory.Regulation)
                {
                    var regulationEffect = GetRegulationEffect(card, session);
                    result.FocusAdded = regulationEffect.FocusGain;
                    result.CardsToAdd = regulationEffect.CardsToAdd;
                    result.PreventNextDoubtIncrease = regulationEffect.PreventDoubtIncrease;
                    result.MomentumPerDiscard = regulationEffect.MomentumPerDiscard;
                    result.MaxDiscards = regulationEffect.MaxDiscards;
                    result.EffectDescription = regulationEffect.Description;
                }
                else
                {
                    // Regular doubt reduction without momentum cost
                    int doubtReduction = card.Template.GetDoubtEffect(session);
                    result.DoubtChange = -doubtReduction;
                    result.EffectDescription = $"-{doubtReduction} doubt";
                }
                break;

            case SuccessEffectType.Threading:
                // Check if this is a Regulation card with special effects
                if (card.Template.Category == CardCategory.Regulation)
                {
                    var regulationEffect = GetRegulationEffect(card, session);
                    result.FocusAdded = regulationEffect.FocusGain;
                    result.CardsToAdd = regulationEffect.CardsToAdd;
                    result.PreventNextDoubtIncrease = regulationEffect.PreventDoubtIncrease;
                    result.MomentumPerDiscard = regulationEffect.MomentumPerDiscard;
                    result.MaxDiscards = regulationEffect.MaxDiscards;
                    result.EffectDescription = regulationEffect.Description;
                }
                else
                {
                    // Regular card draw
                    for (int i = 0; i < magnitude; i++)
                    {
                        CardInstance drawn = session.Deck.DrawCard();
                        if (drawn != null)
                        {
                            result.CardsToAdd.Add(drawn);
                        }
                    }
                    result.EffectDescription = $"Draw {magnitude} cards";
                }
                break;

            case SuccessEffectType.Atmospheric:
                // Use magnitude to determine atmosphere tier
                AtmosphereType? newAtmosphere = GetAtmosphereFromMagnitude(magnitude);
                if (newAtmosphere.HasValue)
                {
                    result.AtmosphereTypeChange = newAtmosphere.Value;
                    result.EffectDescription = $"Set {newAtmosphere.Value} atmosphere";
                }
                break;

            case SuccessEffectType.Focusing:
                // Check if this is a Regulation card with special effects
                if (card.Template.Category == CardCategory.Regulation)
                {
                    var regulationEffect = GetRegulationEffect(card, session);
                    result.FocusAdded = regulationEffect.FocusGain;
                    result.CardsToAdd = regulationEffect.CardsToAdd;
                    result.PreventNextDoubtIncrease = regulationEffect.PreventDoubtIncrease;
                    result.MomentumPerDiscard = regulationEffect.MomentumPerDiscard;
                    result.MaxDiscards = regulationEffect.MaxDiscards;
                    result.EffectDescription = regulationEffect.Description;
                }
                else
                {
                    result.FocusAdded = magnitude;
                    result.EffectDescription = $"+{magnitude} focus";
                }
                break;

            case SuccessEffectType.DoubleMomentum:
                // Double current momentum (powerful realization effect)
                int currentMomentum = session.CurrentMomentum;
                result.MomentumChange = currentMomentum; // Double current momentum
                result.EffectDescription = $"Double momentum: {currentMomentum} → {currentMomentum * 2}";
                break;

            case SuccessEffectType.Promising:
                // Move obligation to position 1 and gain momentum based on magnitude
                result.MomentumChange = magnitude * 2; // Promising gives double momentum
                result.EffectDescription = $"Promise made, +{magnitude * 2} momentum";
                // Queue manipulation handled by ConversationFacade
                break;

            case SuccessEffectType.Advancing:
                // Advances flow battery by magnitude (which may trigger connection state change)
                // Flow battery changes are what actually drive connection state advancement
                result.FlowChange = magnitude; // Add flow equal to magnitude
                result.EffectDescription = $"+{magnitude} flow";
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
            if (card.SuccessType == SuccessEffectType.Strike)
            {
                result.MomentumChange *= 2;
                result.EffectDescription += " (synchronized)";
            }
            else if (card.SuccessType == SuccessEffectType.Soothe)
            {
                result.DoubtChange *= 2; // Double the doubt reduction
                result.EffectDescription += " (synchronized)";
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
                result.EffectDescription += " (synchronized)";
            }
            else if (card.SuccessType == SuccessEffectType.Focusing)
            {
                result.FocusAdded *= 2;
                result.EffectDescription += " (synchronized)";
            }
            else if (card.SuccessType == SuccessEffectType.Advancing)
            {
                result.FlowChange *= 2;
                result.EffectDescription += " (synchronized)";
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
            MomentumChange = 0,
            DoubtChange = 1, // Standard failure adds 1 doubt
            FlowChange = 0,
            CardsToAdd = new List<CardInstance>(),
            FocusAdded = 0,
            AtmosphereTypeChange = null,
            EffectDescription = "Failure: +1 doubt",
            EndsConversation = false
        };

        int magnitude = GetMagnitude(card.Difficulty);
        magnitude = ApplyAtmosphereModifiers(magnitude, SuccessEffectType.Strike, session); // Use Strike for failure magnitude

        switch (card.FailureType)
        {
            case FailureEffectType.Backfire:
                // Negative rapport based on magnitude
                result.RapportChange = -magnitude;
                result.EffectDescription = $"-{magnitude} rapport";
                break;

            case FailureEffectType.ForceListen:
                // PROJECTION: Would deplete focus to force LISTEN on next turn
                PlayerStats playerStats = gameWorld.GetPlayer().Stats;
                if (!card.IgnoresFailureListen(playerStats))
                {
                    // Indicate focus would be depleted
                    result.FocusAdded = -session.GetAvailableFocus();
                    result.EffectDescription = "Must LISTEN next turn";
                }
                else
                {
                    result.EffectDescription = "Immune to forced LISTEN (Level 5)";
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
                // Strike effects +1 (deterministic)
                if (effectType == SuccessEffectType.Strike)
                {
                    magnitude += 1;
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
    /// DETERMINISTIC: Check if card succeeds based on clear rules (no randomness)
    /// Cards succeed based on personality rules, resource requirements, and momentum thresholds
    /// </summary>
    public bool CheckCardSuccess(CardInstance card, ConversationSession session)
    {
        // Goal cards (Letters, Promises) always succeed if momentum threshold is met
        if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
        {
            return session.CurrentMomentum >= card.MomentumThreshold;
        }

        // Regular conversation cards succeed based on deterministic rules
        // For now, implement basic rules - can be expanded based on design needs

        // Very easy cards always succeed
        if (card.Difficulty == Difficulty.VeryEasy)
            return true;

        // Check if player has sufficient level in bound stat
        if (card.Template.BoundStat.HasValue)
        {
            PlayerStats playerStats = gameWorld.GetPlayer().Stats;
            int effectiveLevel = card.GetEffectiveLevel(playerStats);

            // Level requirements based on difficulty
            int requiredLevel = card.Difficulty switch
            {
                Difficulty.Easy => 1,
                Difficulty.Medium => 2,
                Difficulty.Hard => 3,
                Difficulty.VeryHard => 4,
                _ => 1
            };

            if (effectiveLevel < requiredLevel)
                return false;
        }

        // Apply doubt penalty - high doubt can cause failure
        int doubtPenalty = session.GetDoubtPenalty();
        if (doubtPenalty >= 50) // 10+ doubt = 50%+ penalty = automatic failure
            return false;

        // Otherwise succeed
        return true;
    }

    /// <summary>
    /// Get Realization effect based on card ID and stats
    /// </summary>
    private RealizationEffect GetRealizationEffect(CardInstance card, ConversationSession session)
    {
        // Calculate base effect from difficulty and bound stat
        int magnitude = GetMagnitude(card.Difficulty);
        PlayerStats playerStats = gameWorld.GetPlayer().Stats;

        if (card.Template.BoundStat.HasValue)
        {
            int statLevel = playerStats.GetLevel(card.Template.BoundStat.Value);
            magnitude += Math.Max(0, statLevel - 1); // +0 at level 1, +1 at level 2, etc.
        }

        return card.Id switch
        {
            "clear_confusion_1" or "clear_confusion_2" => new RealizationEffect
            {
                MomentumCost = 3,
                DoubtReduction = 2,
                Description = "Spend 3 momentum → Reduce doubt by 2"
            },
            "establish_trust" => new RealizationEffect
            {
                MomentumCost = 2,
                FlowGain = 1,
                Description = "Spend 2 momentum → +1 flow"
            },
            "deep_investment" => new RealizationEffect
            {
                MomentumCost = 4,
                Description = "Spend 4 momentum → Permanent +1 card on LISTEN"
            },
            "all_or_nothing" => new RealizationEffect
            {
                MomentumCost = 6,
                MomentumGain = 12,
                Description = "Spend 6 momentum → Gain 12 momentum"
            },
            "reset_stakes" => new RealizationEffect
            {
                MomentumCost = session.CurrentMomentum,
                DoubtSetTo = 0,
                Description = "Spend ALL momentum → Set doubt to 0"
            },
            "force_understanding" => new RealizationEffect
            {
                MomentumCost = 5,
                FlowGain = 3,
                Description = "Spend 5 momentum → +3 flow"
            },
            _ => new RealizationEffect { Description = "Unknown realization effect" }
        };
    }

    /// <summary>
    /// Get Regulation effect based on card ID and stats
    /// </summary>
    private RegulationEffect GetRegulationEffect(CardInstance card, ConversationSession session)
    {
        // Calculate base effect from difficulty and bound stat
        int magnitude = GetMagnitude(card.Difficulty);
        PlayerStats playerStats = gameWorld.GetPlayer().Stats;

        if (card.Template.BoundStat.HasValue)
        {
            int statLevel = playerStats.GetLevel(card.Template.BoundStat.Value);
            magnitude += Math.Max(0, statLevel - 1); // Stat bonus
        }

        return card.Id switch
        {
            "mental_reset" => new RegulationEffect
            {
                FocusGain = 2,
                Description = "Gain +2 focus this turn only"
            },
            "careful_words_1" or "careful_words_2" => new RegulationEffect
            {
                MomentumPerDiscard = 1,
                MaxDiscards = 2,
                Description = "Discard up to 2 cards → Gain 1 momentum per card"
            },
            "patience" => new RegulationEffect
            {
                PreventDoubtIncrease = true,
                Description = "Prevent next doubt increase"
            },
            "racing_mind_1" or "racing_mind_2" => new RegulationEffect
            {
                CardsToAdd = CreateDrawCards(session, 2),
                Description = "Draw 2 cards immediately"
            },
            _ => new RegulationEffect { Description = "Unknown regulation effect" }
        };
    }

    /// <summary>
    /// Create cards to be drawn immediately
    /// </summary>
    private List<CardInstance> CreateDrawCards(ConversationSession session, int count)
    {
        var cards = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            CardInstance drawn = session.Deck.DrawCard();
            if (drawn != null)
            {
                cards.Add(drawn);
            }
        }
        return cards;
    }

    /// <summary>
    /// Generate descriptive text for Strike effects based on scaling formulas and mechanics
    /// </summary>
    private string GetStrikeEffectDescription(CardInstance card, ConversationSession session, PlayerStats player, int momentumGain)
    {
        var template = card.Template;

        // Check for scaling formulas
        if (template.MomentumScaling != ScalingType.None)
        {
            switch (template.MomentumScaling)
            {
                case ScalingType.CardsInHand:
                    return $"Gain momentum = cards in hand ({session.Deck.HandSize} = {momentumGain})";

                case ScalingType.CardsInHandDivided:
                    return $"Gain momentum = cards in hand ÷ 2 ({session.Deck.HandSize} ÷ 2 = {momentumGain})";

                case ScalingType.DoubtReduction:
                    int doubtReduction = 10 - session.CurrentDoubt;
                    return $"Gain momentum = (10 - current doubt) = {doubtReduction}";

                case ScalingType.DoubtHalved:
                    int doubtHalved = (10 - session.CurrentDoubt) / 2;
                    return $"Gain momentum = (10 - doubt) ÷ 2 = {doubtHalved}";

                case ScalingType.DoubleCurrent:
                    return $"Gain momentum = current momentum × 2 ({session.CurrentMomentum} × 2 = {momentumGain})";

                case ScalingType.PatienteDivided:
                    int patience = session.GetAvailableFocus() / 3;
                    return $"Gain momentum = focus ÷ 3 ({session.GetAvailableFocus()} ÷ 3 = {patience})";
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
}

/// <summary>
/// Result of processing a card effect
/// </summary>
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int RapportChange { get; set; }
    public int FlowChange { get; set; } // For Advancing effect type
    public int MomentumChange { get; set; } // For momentum/doubt system
    public int DoubtChange { get; set; } // For momentum/doubt system
    public List<CardInstance> CardsToAdd { get; set; }
    public int FocusAdded { get; set; }
    public AtmosphereType? AtmosphereTypeChange { get; set; }
    public bool EndsConversation { get; set; }
    public bool PreventNextDoubtIncrease { get; set; }
    public int MomentumPerDiscard { get; set; }
    public int MaxDiscards { get; set; }
    public string EffectDescription { get; set; }
}

/// <summary>
/// Effect data for Realization cards
/// </summary>
public class RealizationEffect
{
    public int MomentumCost { get; set; }
    public int MomentumGain { get; set; }
    public int DoubtReduction { get; set; }
    public int DoubtSetTo { get; set; } = -1; // -1 means no change
    public int FlowGain { get; set; }
    public string Description { get; set; } = "";
}

/// <summary>
/// Effect data for Regulation cards
/// </summary>
public class RegulationEffect
{
    public int FocusGain { get; set; }
    public List<CardInstance> CardsToAdd { get; set; } = new();
    public bool PreventDoubtIncrease { get; set; }
    public int MomentumPerDiscard { get; set; }
    public int MaxDiscards { get; set; }
    public string Description { get; set; } = "";
}