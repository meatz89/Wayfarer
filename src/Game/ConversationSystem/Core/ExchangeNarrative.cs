using System;
using System.Collections.Generic;

/// <summary>
/// Generates rich narrative descriptions for exchanges that reflect
/// relationship depth, emotional context, and shared history.
/// Makes exchanges feel like human moments, not transactions.
/// </summary>
public static class ExchangeNarrative
{
    /// <summary>
    /// Generate the full narrative for an exchange including context and emotion
    /// </summary>
    public static ExchangeNarrativeText GenerateNarrative(
        ExchangeCard exchange,
        string npcName,
        NPCRelationshipTracker relationshipTracker,
        string npcId,
        ExchangeMemory exchangeMemory,
        EmotionalState? currentEmotion = null)
    {
        var narrative = new ExchangeNarrativeText();
        
        // Build the opening based on relationship
        narrative.Opening = GenerateOpening(
            npcName, 
            relationshipTracker.GetTrustPattern(npcId),
            currentEmotion
        );
        
        // Create the exchange description with emotional weight
        narrative.ExchangeDescription = GenerateExchangeDescription(
            exchange,
            exchangeMemory?.GetExchangeFlavor(npcId, exchange.TemplateType) ?? "for the first time",
            exchangeMemory?.HasCrisisHistory(npcId, exchange.TemplateType) ?? false
        );
        
        // Add contextual flavor based on history
        narrative.ContextualFlavor = GenerateContextualFlavor(
            exchange,
            relationshipTracker.GetLastInteractionOutcome(npcId),
            exchangeMemory?.GetMostSignificantExchange(npcId)
        );
        
        // Create the closing that hints at relationship impact
        narrative.Closing = GenerateClosing(
            exchange,
            relationshipTracker.GetSuccessfulDeliveries(npcId),
            exchangeMemory?.HasGenerosityPattern(npcId) ?? false
        );
        
        return narrative;
    }
    
    private static string GenerateOpening(string npcName, string trustPattern, EmotionalState? emotion)
    {
        var emotionalContext = emotion switch
        {
            EmotionalState.DESPERATE => $"{npcName}'s eyes are wild with desperation as they approach. ",
            EmotionalState.TENSE => $"{npcName} approaches with visible tension in their shoulders. ",
            EmotionalState.EAGER => $"{npcName} brightens at your approach. ",
            EmotionalState.OPEN => $"{npcName} greets you with genuine warmth. ",
            EmotionalState.NEUTRAL => $"{npcName} nods in acknowledgment. ",
            _ => $"{npcName} looks up as you approach. "
        };
        
        var relationshipContext = trustPattern switch
        {
            "reliable" => "There's trust in their eyes - you've proven yourself before.",
            "unreliable" => "Their expression is guarded, memories of past failures evident.",
            "new_relationship" => "They regard you with polite curiosity.",
            "mixed" => "Their expression is carefully neutral, unsure what to expect.",
            _ => ""
        };
        
        return emotionalContext + relationshipContext;
    }
    
    private static string GenerateExchangeDescription(
        ExchangeCard exchange, 
        string frequencyFlavor,
        bool hasCrisisHistory)
    {
        var baseDescription = exchange.TemplateType switch
        {
            "food" => exchange.NPCPersonality switch
            {
                PersonalityType.DEVOTED => $"They offer to share their meal {frequencyFlavor}",
                PersonalityType.MERCANTILE => $"They gesture to their provisions {frequencyFlavor}",
                PersonalityType.STEADFAST => $"They pull out simple rations {frequencyFlavor}",
                _ => $"They offer food {frequencyFlavor}"
            },
            "healing" => exchange.NPCPersonality switch
            {
                PersonalityType.DEVOTED => $"They prepare to offer healing prayers {frequencyFlavor}",
                PersonalityType.MERCANTILE => $"They display their medical supplies {frequencyFlavor}",
                _ => $"They offer treatment {frequencyFlavor}"
            },
            "work" => $"They mention work that needs doing {frequencyFlavor}",
            "information" => $"They lean in conspiratorially {frequencyFlavor}",
            "reconciliation" => "They extend a tentative offer of reconciliation",
            "trusted" => "They offer something special, reserved for trusted friends",
            _ => $"They propose an exchange {frequencyFlavor}"
        };
        
        if (hasCrisisHistory)
        {
            baseDescription += ", remembering when you helped during their darkest hour";
        }
        
        return baseDescription + ".";
    }
    
    private static string GenerateContextualFlavor(
        ExchangeCard exchange,
        string lastOutcome,
        MemorableExchange significantMemory)
    {
        var contextLines = new List<string>();
        
        // Add reaction to last interaction
        switch (lastOutcome)
        {
            case "failed_delivery":
                contextLines.Add("Despite your recent failure, they're willing to deal.");
                break;
            case "successful_delivery":
                contextLines.Add("Your recent success has earned this opportunity.");
                break;
            case "promise_broken":
                contextLines.Add("Trust must be rebuilt, one exchange at a time.");
                break;
            case "promise_fulfilled":
                contextLines.Add("Your kept promises have opened new doors.");
                break;
        }
        
        // Reference significant past exchange
        if (significantMemory != null && significantMemory.EmotionalContext == EmotionalState.DESPERATE)
        {
            contextLines.Add("You both remember when desperation brought you together.");
        }
        
        // Add personality-specific observations
        switch (exchange.NPCPersonality)
        {
            case PersonalityType.DEVOTED:
                if (exchange.Reward.Exists(r => r.Type == ResourceType.Health))
                    contextLines.Add("Their faith guides their healing touch.");
                break;
            case PersonalityType.MERCANTILE:
                if (exchange.Cost.Exists(c => c.Type == ResourceType.Coins))
                    contextLines.Add("Business is business, but there's warmth in the transaction.");
                break;
            case PersonalityType.CUNNING:
                contextLines.Add("Every exchange carries hidden meanings.");
                break;
            case PersonalityType.STEADFAST:
                contextLines.Add("Honest work for honest pay, as always.");
                break;
            case PersonalityType.PROUD:
                contextLines.Add("Even small exchanges maintain proper social order.");
                break;
        }
        
        return string.Join(" ", contextLines);
    }
    
    private static string GenerateClosing(
        ExchangeCard exchange,
        int successfulDeliveries,
        bool hasGenerosityPattern)
    {
        if (hasGenerosityPattern)
        {
            return "Your consistent generosity hasn't gone unnoticed.";
        }
        
        if (successfulDeliveries > 10)
        {
            return "Years of trust make this exchange feel like breathing.";
        }
        
        if (successfulDeliveries > 5)
        {
            return "A foundation of trust underlies this simple exchange.";
        }
        
        if (exchange.Reward.Exists(r => r.Type == ResourceType.TrustToken))
        {
            return "This exchange deepens the connection between you.";
        }
        
        return "The exchange completes with quiet understanding.";
    }
}

/// <summary>
/// Structured narrative text for an exchange
/// </summary>
public class ExchangeNarrativeText
{
    public string Opening { get; set; }
    public string ExchangeDescription { get; set; }
    public string ContextualFlavor { get; set; }
    public string Closing { get; set; }
    
    /// <summary>
    /// Get the full narrative as a single string
    /// </summary>
    public string GetFullNarrative()
    {
        return $"{Opening}\n\n{ExchangeDescription} {ContextualFlavor}\n\n{Closing}";
    }
    
    /// <summary>
    /// Get a brief version for UI display
    /// </summary>
    public string GetBriefNarrative()
    {
        return ExchangeDescription;
    }
}