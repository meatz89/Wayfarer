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
            FlowChange = 0,
            CardsToAdd = new List<CardInstance>(),
            FocusAdded = 0,
            EffectDescription = "",
            EndsConversation = false
        };

        int magnitude = GetMagnitude(card.Difficulty);

        switch (card.SuccessType)
        {
            case SuccessEffectType.Strike:
                // Use card's momentum scaling formula with player stats for bonuses
                PlayerStats player = gameWorld.GetPlayer().Stats;
                int momentumGain = card.ConversationCardTemplate.GetMomentumEffect(session, player);

                // Handle resource conversion cards (negative momentum = spend momentum for other effects)
                if (momentumGain < 0 && card.ConversationCardTemplate.MomentumScaling != ScalingType.None)
                {
                    int momentumCost = Math.Abs(momentumGain);
                    if (session.CurrentMomentum >= momentumCost)
                    {
                        result.MomentumChange = -momentumCost;

                        // Apply conversion effect based on scaling type
                        switch (card.ConversationCardTemplate.MomentumScaling)
                        {
                            case ScalingType.SpendForDoubt:
                                result.DoubtChange = -3; // Reduce doubt by 3
                                result.EffectDescription = $"Spend {momentumCost} momentum → -3 doubt";
                                break;
                            case ScalingType.SpendForFlow:
                                result.FlowChange = 1; // Gain 1 flow
                                result.EffectDescription = $"Spend {momentumCost} momentum → +1 flow";
                                break;
                            case ScalingType.SpendForFlowMajor:
                                result.FlowChange = 2; // Gain 2 flow
                                result.EffectDescription = $"Spend {momentumCost} momentum → +2 flow";
                                break;
                            default:
                                result.EffectDescription = $"Spend {momentumCost} momentum";
                                break;
                        }
                    }
                    else
                    {
                        // Not enough momentum to afford the conversion
                        result.MomentumChange = 0;
                        result.EffectDescription = $"Need {momentumCost} momentum (have {session.CurrentMomentum})";
                    }
                }
                else
                {
                    // Regular momentum generation
                    result.MomentumChange = momentumGain;
                    result.EffectDescription = GetStrikeEffectDescription(card, session, player, momentumGain);
                }
                break;

            case SuccessEffectType.Soothe:
                // Handle resource conversion for Soothe cards
                if (card.ConversationCardTemplate.MomentumScaling == ScalingType.SpendForDoubt)
                {
                    int momentumCost = 2; // Fixed cost for clarity
                    if (session.CurrentMomentum >= momentumCost)
                    {
                        result.MomentumChange = -momentumCost;
                        result.DoubtChange = -3; // Reduce doubt by 3
                        result.EffectDescription = $"Spend {momentumCost} momentum → -3 doubt";
                    }
                    else
                    {
                        result.MomentumChange = 0;
                        result.EffectDescription = $"Need {momentumCost} momentum (have {session.CurrentMomentum})";
                    }
                }
                else if (card.ConversationCardTemplate.MomentumScaling == ScalingType.PreventDoubt)
                {
                    // Special effect: prevent next doubt increase
                    result.DoubtChange = 0; // No immediate change
                    result.EffectDescription = "Prevent next doubt increase";
                    // Note: session.PreventNextDoubtIncrease will be set by ConversationFacade when effect is applied
                }
                else
                {
                    // Regular doubt reduction
                    int doubtReduction = magnitude;
                    result.DoubtChange = -doubtReduction;
                    result.EffectDescription = $"-{doubtReduction} doubt";
                }
                break;

            case SuccessEffectType.Threading:
                // Threading = card draw (regardless of card category)
                for (int i = 0; i < magnitude; i++)
                {
                    CardInstance drawn = session.Deck.DrawCard();
                    if (drawn != null)
                    {
                        result.CardsToAdd.Add(drawn);
                    }
                }
                result.EffectDescription = $"Draw {magnitude} cards";
                break;

            case SuccessEffectType.Atmospheric:
                // Use magnitude to determine atmosphere tier
                AtmosphereType? newAtmosphere = GetAtmosphereFromMagnitude(magnitude);
                if (newAtmosphere.HasValue)
                {
                    result.EffectDescription = $"Set {newAtmosphere.Value} atmosphere";
                }
                break;

            case SuccessEffectType.Focusing:
                // Focusing = focus gain (regardless of card category)
                result.FocusAdded = magnitude;
                result.EffectDescription = $"+{magnitude} focus";
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
                // Handle resource conversion for Advancing cards
                if (card.ConversationCardTemplate.MomentumScaling == ScalingType.SpendForFlow)
                {
                    int momentumCost = 3; // Fixed cost for clarity
                    if (session.CurrentMomentum >= momentumCost)
                    {
                        result.MomentumChange = -momentumCost;
                        result.FlowChange = 1; // Gain 1 flow
                        result.EffectDescription = $"Spend {momentumCost} momentum → +1 flow";
                    }
                    else
                    {
                        result.MomentumChange = 0;
                        result.EffectDescription = $"Need {momentumCost} momentum (have {session.CurrentMomentum})";
                    }
                }
                else if (card.ConversationCardTemplate.MomentumScaling == ScalingType.SpendForFlowMajor)
                {
                    int momentumCost = 4; // Fixed cost for major flow gain
                    if (session.CurrentMomentum >= momentumCost)
                    {
                        result.MomentumChange = -momentumCost;
                        result.FlowChange = 2; // Gain 2 flow
                        result.EffectDescription = $"Spend {momentumCost} momentum → +2 flow";
                    }
                    else
                    {
                        result.MomentumChange = 0;
                        result.EffectDescription = $"Need {momentumCost} momentum (have {session.CurrentMomentum})";
                    }
                }
                else
                {
                    // Regular flow gain
                    result.FlowChange = magnitude;
                    result.EffectDescription = $"+{magnitude} flow";
                }
                break;

            case SuccessEffectType.None:
            default:
                // No effect
                break;
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
            MomentumChange = 0,
            DoubtChange = 1, // Standard failure adds 1 doubt
            FlowChange = 0,
            CardsToAdd = new List<CardInstance>(),
            FocusAdded = 0,
            EffectDescription = "Failure: +1 doubt",
            EndsConversation = false
        };

        int magnitude = GetMagnitude(card.Difficulty);

        switch (card.FailureType)
        {
            case FailureEffectType.Backfire:
                // Negative rapport based on magnitude
                result.MomentumChange = -magnitude;
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
        if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.Letter)
        {
            return session.CurrentMomentum >= card.MomentumThreshold;
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
    public int FlowChange { get; set; }
    public int MomentumChange { get; set; }
    public int DoubtChange { get; set; }
    public List<CardInstance> CardsToAdd { get; set; }
    public int FocusAdded { get; set; }
    public bool EndsConversation { get; set; }
    public string EffectDescription { get; set; }
}

