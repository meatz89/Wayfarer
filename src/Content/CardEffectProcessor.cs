using System;
using System.Collections.Generic;

// CardEffectProcessor handles all card effect processing for the new single-effect system
public class CardEffectProcessor
{
    private readonly TokenMechanicsManager tokenManager;
    private readonly AtmosphereManager atmosphereManager;
    private readonly FocusManager focusManager;

    public CardEffectProcessor(TokenMechanicsManager tokenManager, AtmosphereManager atmosphereManager, FocusManager focusManager)
    {
        this.tokenManager = tokenManager;
        this.atmosphereManager = atmosphereManager;
        this.focusManager = focusManager;
    }

    /// <summary>
    /// Process a card's success effect
    /// </summary>
    public CardEffectResult ProcessSuccessEffect(CardInstance card, ConversationSession session)
    {
        if (card.SuccessEffect == null || card.SuccessEffect.IsEmpty)
            return new CardEffectResult { Card = card };

        return ProcessEffect(card.SuccessEffect, card, session);
    }

    /// <summary>
    /// Process a card's failure effect
    /// </summary>
    public CardEffectResult ProcessFailureEffect(CardInstance card, ConversationSession session)
    {
        if (card.FailureEffect == null || card.FailureEffect.IsEmpty)
            return new CardEffectResult { Card = card };

        return ProcessEffect(card.FailureEffect, card, session);
    }

    /// <summary>
    /// Process a card's exhaust effect
    /// </summary>
    public CardEffectResult ProcessExhaustEffect(CardInstance card, ConversationSession session)
    {
        if (card.ExhaustEffect == null || card.ExhaustEffect.IsEmpty)
            return new CardEffectResult { Card = card };

        return ProcessEffect(card.ExhaustEffect, card, session);
    }

    /// <summary>
    /// Process any card effect and return the result
    /// </summary>
    private CardEffectResult ProcessEffect(CardEffect effect, CardInstance card, ConversationSession session)
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

        // Process the effect based on type
        switch (effect.Type)
        {
            case CardEffectType.AddRapport:
                result.RapportChange = ProcessFixedRapport(effect.Value);
                break;

            case CardEffectType.ScaleRapportByFlow:
            case CardEffectType.ScaleRapportByPatience:
            case CardEffectType.ScaleRapportByFocus:
                result.RapportChange = ProcessScaledRapport(effect, session);
                break;

            case CardEffectType.DrawCards:
                result.CardsToAdd = ProcessDrawCards(effect.Value, session);
                break;

            case CardEffectType.AddFocus:
                result.FocusAdded = ProcessAddFocus(effect.Value);
                break;

            case CardEffectType.SetAtmosphere:
                result.AtmosphereTypeChange = ProcessSetAtmosphereType(effect.Value);
                break;

            case CardEffectType.EndConversation:
                result.EndsConversation = true;
                result.SpecialEffect = "Conversation ends";
                break;

            case CardEffectType.RapportReset:
                result.RapportChange = -session.RapportManager.CurrentRapport; // Reset to starting value
                result.SpecialEffect = "Rapport reset to starting value";
                break;

            case CardEffectType.FocusRefresh:
                result.FocusAdded = session.GetEffectiveFocusCapacity() - session.CurrentFocus;
                result.SpecialEffect = "Focus refreshed";
                break;

            case CardEffectType.FreeNextAction:
                result.SpecialEffect = "Next action costs 0 patience";
                // Implementation would set a flag in session
                break;

            case CardEffectType.AdvanceConnectionState:
                result.SpecialEffect = ProcessAdvanceConnectionState(effect, session);
                break;

            case CardEffectType.UnlockExchange:
                result.SpecialEffect = ProcessUnlockExchange(effect, session);
                break;
                
            case CardEffectType.ForceQueuePosition:
                result.SpecialEffect = ProcessForceQueuePosition(effect, session);
                break;
        }

        // Apply atmosphere modifications to rapport changes
        if (result.RapportChange != 0)
        {
            result.RapportChange = atmosphereManager.ModifyRapportChange(result.RapportChange);
        }

        // Apply Synchronized effect (double the effect)
        if (atmosphereManager.ShouldDoubleNextEffect())
        {
            if (result.RapportChange != 0)
                result.RapportChange *= 2;
            if (result.FocusAdded > 0)
                result.FocusAdded *= 2;
            if (result.CardsToAdd.Count > 0)
            {
                // Double the cards drawn
                List<CardInstance> additionalCards = ProcessDrawCards(effect.Value, session);
                result.CardsToAdd.AddRange(additionalCards);
            }
        }

        return result;
    }

    // Process fixed rapport effect
    private int ProcessFixedRapport(string effectValue)
    {
        if (int.TryParse(effectValue, out int rapport))
        {
            return rapport;
        }
        return 0;
    }

    // Process scaled rapport based on effect type and formula
    private int ProcessScaledRapport(CardEffect effect, ConversationSession session)
    {
        // Handle specific scaling types
        if (effect.Type == CardEffectType.ScaleRapportByFlow)
        {
            // Parse formula like "4 - flow"
            if (effect.Value.Contains("-"))
            {
                string[] parts = effect.Value.Split('-');
                if (int.TryParse(parts[0].Trim(), out int baseValue))
                {
                    return baseValue - session.FlowManager.CurrentFlow;
                }
            }
            return session.RapportManager.ScaleRapportByFlow(session.FlowManager.CurrentFlow);
        }

        if (effect.Type == CardEffectType.ScaleRapportByPatience)
        {
            // Parse formula like "patience / 3"
            if (effect.Value.Contains("/"))
            {
                string[] parts = effect.Value.Split('/');
                if (int.TryParse(parts[1].Trim(), out int divisor) && divisor > 0)
                {
                    return session.CurrentPatience / divisor;
                }
            }
            return session.RapportManager.ScaleRapportByPatience(session.CurrentPatience);
        }

        if (effect.Type == CardEffectType.ScaleRapportByFocus)
        {
            return session.RapportManager.ScaleRapportByFocus(session.GetAvailableFocus());
        }

        return int.TryParse(effect.Value, out int value) ? value : 0;
    }

    // Process draw cards effect
    private List<CardInstance> ProcessDrawCards(string effectValue, ConversationSession session)
    {
        if (!int.TryParse(effectValue, out int cardCount))
            return new List<CardInstance>();

        List<CardInstance> drawnCards = session.Deck.DrawCards(cardCount);
        return drawnCards;
    }

    // Process add focus effect
    private int ProcessAddFocus(string effectValue)
    {
        if (int.TryParse(effectValue, out int focus))
        {
            return focus;
        }
        return 0;
    }

    // Process set atmosphere effect
    private AtmosphereType ProcessSetAtmosphereType(string effectValue)
    {
        if (Enum.TryParse<AtmosphereType>(effectValue, true, out AtmosphereType atmosphere))
        {
            atmosphereManager.SetAtmosphere(atmosphere);
            return atmosphere;
        }
        return AtmosphereType.Neutral;
    }

    // Process reset flow effect (observation only)
    private int ProcessResetFlow(ConversationSession session)
    {
        int currentFlow = session.FlowBattery;
        return -currentFlow; // Returns the change needed to reset to 0
    }

    // Process max focus effect (observation only)
    private void ProcessMaxFocus()
    {
        focusManager.SetToMaximum();
    }

    // Process free action effect (observation only)
    private void ProcessFreeAction()
    {
        // This would set a flag for the next action to cost 0 patience
        // Implementation depends on how patience is handled in the system
    }

    // Calculate success percentage for a card
    public int CalculateSuccessPercentage(CardInstance card, ConversationSession session)
    {
        // Exchange cards always succeed (they're trades, not skill checks)
        if (card.IsExchange)
        {
            return 100;
        }

        int baseSuccess = card.GetBaseSuccessPercentage();

        // Rapport modifier instead of token modifier
        int rapportBonus = session.RapportManager.GetSuccessModifier();

        // Add atmosphere bonus
        int atmosphereBonus = atmosphereManager.GetSuccessPercentageBonus();

        // Calculate final percentage (clamped to 5-95%)
        int finalPercentage = baseSuccess + rapportBonus + atmosphereBonus;
        return Math.Clamp(finalPercentage, 5, 95);
    }

    // Perform dice roll for success with hidden momentum system
    public bool RollForSuccess(int successPercentage, ConversationSession session = null)
    {
        // Auto-succeed if Informed atmosphere
        if (atmosphereManager.ShouldAutoSucceed())
            return true;

        // Apply hidden momentum (bad luck protection)
        // Each failure adds 3-5% invisible bonus, caps at 15%
        int momentum = session?.HiddenMomentum ?? 0;
        int momentumBonus = Math.Min(momentum * 4, 15); // 4% per failure, max 15%

        // Also apply a slight baseline player favor: 
        // Instead of pure random, we slightly weight the dice
        Random random = new Random();

        // Generate weighted roll that slightly favors success
        // We roll twice and take the better result 15% of the time
        int roll = random.Next(1, 101);
        if (random.Next(1, 101) <= 15) // 15% chance to roll twice
        {
            int secondRoll = random.Next(1, 101);
            roll = Math.Min(roll, secondRoll); // Lower roll is better for player
        }

        // Apply momentum bonus invisibly
        int adjustedSuccessChance = Math.Min(successPercentage + momentumBonus, 95);

        return roll <= adjustedSuccessChance;
    }

    // Check success using pre-rolled value with momentum
    public bool CheckSuccessWithPreRoll(int preRolledValue, int successPercentage, ConversationSession session = null)
    {
        // Auto-succeed if Informed atmosphere
        if (atmosphereManager.ShouldAutoSucceed())
            return true;

        // Apply hidden momentum (bad luck protection)
        int momentum = session?.HiddenMomentum ?? 0;
        int momentumBonus = Math.Min(momentum * 4, 15); // 4% per failure, max 15%

        // Apply momentum bonus invisibly to success chance
        int adjustedSuccessChance = Math.Min(successPercentage + momentumBonus, 95);

        Console.WriteLine($"[CardEffectProcessor] Using pre-rolled {preRolledValue} vs {adjustedSuccessChance}% (base {successPercentage}% + momentum {momentumBonus}%)");

        // Use the pre-rolled value
        return preRolledValue <= adjustedSuccessChance;
    }


    // Process advance connection state effect (observation cards only)
    private string ProcessAdvanceConnectionState(CardEffect effect, ConversationSession session)
    {
        if (effect.Value == null) return "No state specified";
        
        // Parse the target state from effect value
        if (Enum.TryParse<ConnectionState>(effect.Value, true, out ConnectionState targetState))
        {
            // Set the NPC's connection state directly
            session.CurrentState = targetState;
            
            // Reset flow to 0 when state changes
            session.FlowBattery = 0;
            
            return $"Conversation advanced to {targetState} state";
        }
        
        return $"Invalid state: {effect.Value}";
    }

    // Process unlock exchange effect (observation cards only)
    private string ProcessUnlockExchange(CardEffect effect, ConversationSession session)
    {
        if (effect.Value == null) return "No exchange specified";
        
        string exchangeId = effect.Value;
        
        // For now, we'll just set a flag or special effect
        // The actual exchange unlocking would need to be handled at a higher level
        // where we have access to the NPC's exchange deck
        
        return $"Unlocked exchange: {exchangeId}";
    }
    
    private string ProcessForceQueuePosition(CardEffect effect, ConversationSession session)
    {
        // Parse the queue position from the effect value (default to 1)
        int targetPosition = 1;
        if (!string.IsNullOrEmpty(effect.Value) && int.TryParse(effect.Value, out int parsed))
        {
            targetPosition = parsed;
        }
        
        // The actual queue manipulation needs to happen at a higher level
        // where we have access to the ObligationQueueManager
        // For now, just return the effect description
        // The Promise card's QueuePosition property should be used when processing the card
        
        return $"Force obligation to queue position {targetPosition} (burns tokens with displaced NPCs)";
    }
}

// Result of processing a card effect
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int RapportChange { get; set; }
    public List<CardInstance> CardsToAdd { get; set; } = new();
    public int FocusAdded { get; set; }
    public AtmosphereType? AtmosphereTypeChange { get; set; }
    public string SpecialEffect { get; set; } = "";
    public bool Success { get; set; } = true;
    public int Roll { get; set; }
    public int SuccessPercentage { get; set; }
    public bool EndsConversation { get; set; } = false;
    public EffectOutcomeData OutcomeData { get; set; }
}

/// <summary>
/// Strongly typed effect outcome data
/// </summary>
public class EffectOutcomeData
{
    public string EndReason { get; set; }
    public string ResolutionType { get; set; }
    public int FinalRapport { get; set; }
    public int TokensGained { get; set; }
    public string UnlockedExchange { get; set; }
    public ConnectionState? NewConnectionState { get; set; }
}