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
    public CardEffectResult ProcessSuccessEffect(ConversationCard card, ConversationSession session)
    {
        if (card.SuccessEffect == null || card.SuccessEffect.IsEmpty)
            return new CardEffectResult { Card = card };
        
        return ProcessEffect(card.SuccessEffect, card, session);
    }
    
    /// <summary>
    /// Process a card's failure effect
    /// </summary>
    public CardEffectResult ProcessFailureEffect(ConversationCard card, ConversationSession session)
    {
        if (card.FailureEffect == null || card.FailureEffect.IsEmpty)
            return new CardEffectResult { Card = card };
        
        return ProcessEffect(card.FailureEffect, card, session);
    }
    
    /// <summary>
    /// Process a card's exhaust effect
    /// </summary>
    public CardEffectResult ProcessExhaustEffect(ConversationCard card, ConversationSession session)
    {
        if (card.ExhaustEffect == null || card.ExhaustEffect.IsEmpty)
            return new CardEffectResult { Card = card };
        
        return ProcessEffect(card.ExhaustEffect, card, session);
    }

    /// <summary>
    /// Process any card effect and return the result
    /// </summary>
    private CardEffectResult ProcessEffect(CardEffect effect, ConversationCard card, ConversationSession session)
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
                if (effect.Data != null)
                {
                    result.ConversationOutcomeData = effect.Data;
                }
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
    public int CalculateSuccessPercentage(ConversationCard card, ConversationSession session)
    {
        int baseSuccess = card.GetBaseSuccessPercentage();

        // Rapport modifier instead of token modifier
        int rapportBonus = session.RapportManager.GetSuccessModifier();

        // Add atmosphere bonus
        int atmosphereBonus = atmosphereManager.GetSuccessPercentageBonus();

        // Calculate final percentage (clamped to 5-95%)
        int finalPercentage = baseSuccess + rapportBonus + atmosphereBonus;
        return Math.Clamp(finalPercentage, 5, 95);
    }

    // Perform dice roll for success
    public bool RollForSuccess(int successPercentage)
    {
        // Auto-succeed if Informed atmosphere
        if (atmosphereManager.ShouldAutoSucceed())
            return true;

        // Roll 1-100
        Random random = new Random();
        int roll = random.Next(1, 101);
        return roll <= successPercentage;
    }
}

// Result of processing a card effect
public class CardEffectResult
{
    public ConversationCard Card { get; set; }
    public int RapportChange { get; set; }
    public List<CardInstance> CardsToAdd { get; set; } = new();
    public int FocusAdded { get; set; }
    public AtmosphereType? AtmosphereTypeChange { get; set; }
    public string SpecialEffect { get; set; } = "";
    public bool Success { get; set; } = true;
    public int Roll { get; set; }
    public int SuccessPercentage { get; set; }
    public bool EndsConversation { get; set; } = false;
    public Dictionary<string, object> ConversationOutcomeData { get; set; }
}