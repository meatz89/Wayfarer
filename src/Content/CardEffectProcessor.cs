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

    // Process a single card effect and return the result
    public CardEffectResult ProcessCardEffect(ConversationCard card, ConversationSession session)
    {
        CardEffectResult result = new CardEffectResult
        {
            Card = card,
            ComfortChange = 0,
            CardsToAdd = new List<CardInstance>(),
            WeightAdded = 0,
            AtmosphereTypeChange = null,
            SpecialEffect = ""
        };

        // Process the primary effect
        switch (card.EffectType)
        {
            case CardEffectType.FixedComfort:
                result.ComfortChange = ProcessFixedComfort(card.GetEffectValueOrFormula());
                break;

            case CardEffectType.ScaledComfort:
                result.ComfortChange = ProcessScaledComfort(card.GetEffectValueOrFormula(), session);
                break;

            case CardEffectType.DrawCards:
                result.CardsToAdd = ProcessDrawCards(card.GetEffectValueOrFormula(), session);
                break;

            case CardEffectType.AddWeight:
                result.WeightAdded = ProcessAddWeight(card.GetEffectValueOrFormula());
                break;

            case CardEffectType.SetAtmosphereType:
                result.AtmosphereTypeChange = ProcessSetAtmosphereType(card.GetEffectValueOrFormula());
                break;

            case CardEffectType.ResetComfort:
                result.ComfortChange = ProcessResetComfort(session);
                result.SpecialEffect = "Comfort reset to 0";
                break;

            case CardEffectType.MaxWeight:
                ProcessMaxWeight();
                result.SpecialEffect = "Weight pool refreshed to maximum";
                break;

            case CardEffectType.FreeAction:
                ProcessFreeAction();
                result.SpecialEffect = "Next action costs 0 patience";
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
                List<CardInstance> additionalCards = ProcessDrawCards(card.GetEffectValueOrFormula(), session);
                result.CardsToAdd.AddRange(additionalCards);
            }
        }

        // Handle atmosphere change from card (separate from effect)
        if (card.AtmosphereTypeChange.HasValue)
        {
            result.AtmosphereTypeChange = card.AtmosphereTypeChange.Value;
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

    // Process scaled comfort based on formulas
    private int ProcessScaledComfort(string formula, ConversationSession session)
    {
        return formula switch
        {
            "trust_tokens" => tokenManager.GetTokenCount(ConnectionType.Trust, session.NPC.ID),
            "commerce_tokens" => tokenManager.GetTokenCount(ConnectionType.Commerce, session.NPC.ID),
            "status_tokens" => tokenManager.GetTokenCount(ConnectionType.Status, session.NPC.ID),
            "shadow_tokens" => tokenManager.GetTokenCount(ConnectionType.Shadow, session.NPC.ID),
            "inverse_comfort" => 4 - Math.Abs(session.ComfortBattery),
            "patience_third" => session.CurrentPatience / 3,
            "weight_remaining" => weightPoolManager.AvailableWeight,
            _ => int.TryParse(formula, out int value) ? value : 0
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
        ConnectionType cardTokenType = card.TokenType;

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
}