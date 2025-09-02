using System;
using System.Collections.Generic;

// CardEffectProcessor handles all card effect processing for the new single-effect system
public class CardEffectProcessor
{
    private readonly TokenMechanicsManager tokenManager;
    private readonly AtmosphereManager atmosphereManager;
    private readonly WeightPoolManager weightPoolManager;

    public CardEffectProcessor(TokenMechanicsManager tokenManager, AtmosphereManager atmosphereManager, WeightPoolManager weightPoolManager)
    {
        this.tokenManager = tokenManager;
        this.atmosphereManager = atmosphereManager;
        this.weightPoolManager = weightPoolManager;
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
            ComfortChange = 0,
            CardsToAdd = new List<CardInstance>(),
            WeightAdded = 0,
            AtmosphereTypeChange = null,
            SpecialEffect = "",
            EndsConversation = false
        };

        // Process the effect based on type
        switch (effect.Type)
        {
            case CardEffectType.AddComfort:
            case CardEffectType.FixedComfort: // Legacy compatibility
                result.ComfortChange = ProcessFixedComfort(effect.Value);
                break;

            case CardEffectType.ScaleByTokens:
            case CardEffectType.ScaleByComfort:
            case CardEffectType.ScaleByPatience:
            case CardEffectType.ScaleByWeight:
            case CardEffectType.ScaledComfort: // Legacy compatibility
                result.ComfortChange = ProcessScaledComfort(effect, session);
                break;

            case CardEffectType.DrawCards:
                result.CardsToAdd = ProcessDrawCards(effect.Value, session);
                break;

            case CardEffectType.AddWeight:
                result.WeightAdded = ProcessAddWeight(effect.Value);
                break;

            case CardEffectType.SetAtmosphere:
                result.AtmosphereTypeChange = ProcessSetAtmosphereType(effect.Value);
                break;
                
            case CardEffectType.EndConversation:
            case CardEffectType.GoalEffect: // Legacy compatibility
                result.EndsConversation = true;
                result.SpecialEffect = "Conversation ends";
                if (effect.Data != null)
                {
                    result.ConversationOutcomeData = effect.Data;
                }
                break;
                
            case CardEffectType.ComfortReset:
                result.ComfortChange = -session.CurrentComfort; // Reset to 0
                result.SpecialEffect = "Comfort reset to 0";
                break;
                
            case CardEffectType.WeightRefresh:
                result.WeightAdded = weightPoolManager.CurrentCapacity - weightPoolManager.CurrentSpentWeight;
                result.SpecialEffect = "Weight pool refreshed";
                break;
                
            case CardEffectType.FreeNextAction:
                result.SpecialEffect = "Next action costs 0 patience";
                // Implementation would set a flag in session
                break;
        }

        // Apply atmosphere modifications to comfort changes
        if (result.ComfortChange != 0)
        {
            result.ComfortChange = atmosphereManager.ModifyComfortChange(result.ComfortChange);
        }

        // Apply Synchronized effect (double the effect)
        if (atmosphereManager.ShouldDoubleNextEffect())
        {
            if (result.ComfortChange != 0)
                result.ComfortChange *= 2;
            if (result.WeightAdded > 0)
                result.WeightAdded *= 2;
            if (result.CardsToAdd.Count > 0)
            {
                // Double the cards drawn
                List<CardInstance> additionalCards = ProcessDrawCards(effect.Value, session);
                result.CardsToAdd.AddRange(additionalCards);
            }
        }

        return result;
    }

    // Process fixed comfort effect
    private int ProcessFixedComfort(string effectValue)
    {
        if (int.TryParse(effectValue, out int comfort))
        {
            return comfort;
        }
        return 0;
    }

    // Process scaled comfort based on effect type and formula
    private int ProcessScaledComfort(CardEffect effect, ConversationSession session)
    {
        // Handle specific scaling types
        if (effect.Type == CardEffectType.ScaleByTokens)
        {
            TokenType tokenType = Enum.Parse<TokenType>(effect.Value);
            return tokenManager.GetTokenCount(ConversationCard.ConvertTokenToConnection(tokenType), session.NPC.ID);
        }
        
        if (effect.Type == CardEffectType.ScaleByComfort)
        {
            // Parse formula like "4 - comfort"
            if (effect.Value.Contains("-"))
            {
                string[] parts = effect.Value.Split('-');
                if (int.TryParse(parts[0].Trim(), out int baseValue))
                {
                    return baseValue - session.CurrentComfort;
                }
            }
            return session.CurrentComfort;
        }
        
        if (effect.Type == CardEffectType.ScaleByPatience)
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
            return session.CurrentPatience;
        }
        
        if (effect.Type == CardEffectType.ScaleByWeight)
        {
            return weightPoolManager.AvailableWeight;
        }
        
        // Legacy formula handling
        return effect.Value switch
        {
            "trust_tokens" => tokenManager.GetTokenCount(ConnectionType.Trust, session.NPC.ID),
            "commerce_tokens" => tokenManager.GetTokenCount(ConnectionType.Commerce, session.NPC.ID),
            "status_tokens" => tokenManager.GetTokenCount(ConnectionType.Status, session.NPC.ID),
            "shadow_tokens" => tokenManager.GetTokenCount(ConnectionType.Shadow, session.NPC.ID),
            "inverse_comfort" => 4 - Math.Abs(session.ComfortBattery),
            "patience_third" => session.CurrentPatience / 3,
            "weight_remaining" => weightPoolManager.AvailableWeight,
            _ => int.TryParse(effect.Value, out int value) ? value : 0
        };
    }

    // Process draw cards effect
    private List<CardInstance> ProcessDrawCards(string effectValue, ConversationSession session)
    {
        if (!int.TryParse(effectValue, out int cardCount))
            return new List<CardInstance>();

        List<CardInstance> drawnCards = session.Deck.DrawCards(cardCount);
        return drawnCards;
    }

    // Process add weight effect
    private int ProcessAddWeight(string effectValue)
    {
        if (int.TryParse(effectValue, out int weight))
        {
            weightPoolManager.AddWeight(weight);
            return weight;
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

    // Process reset comfort effect (observation only)
    private int ProcessResetComfort(ConversationSession session)
    {
        int currentComfort = session.ComfortBattery;
        return -currentComfort; // Returns the change needed to reset to 0
    }

    // Process max weight effect (observation only)
    private void ProcessMaxWeight()
    {
        weightPoolManager.SetToMaximum();
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

        // Get the card's token type (Trust, Commerce, Status, or Shadow)
        ConnectionType cardTokenType = ConversationCard.ConvertTokenToConnection(card.TokenType);

        // Add token bonus (5% per MATCHING token only)
        int matchingTokens = tokenManager.GetTokenCount(cardTokenType, session.NPC.ID);
        int tokenBonus = matchingTokens * 5;

        // Add atmosphere bonus
        int atmosphereBonus = atmosphereManager.GetSuccessPercentageBonus();

        // Calculate final percentage (clamped to 5-95%)
        int finalPercentage = baseSuccess + tokenBonus + atmosphereBonus;
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
    public int ComfortChange { get; set; }
    public List<CardInstance> CardsToAdd { get; set; } = new();
    public int WeightAdded { get; set; }
    public AtmosphereType? AtmosphereTypeChange { get; set; }
    public string SpecialEffect { get; set; } = "";
    public bool Success { get; set; } = true;
    public int Roll { get; set; }
    public int SuccessPercentage { get; set; }
    public bool EndsConversation { get; set; } = false;
    public Dictionary<string, object> ConversationOutcomeData { get; set; }
}