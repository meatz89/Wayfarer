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
        ConversationCard exchange,
        string npcName,
        string npcId,
        ExchangeMemory exchangeMemory,
        EmotionalState? currentEmotion = null)
    {
        var narrative = new ExchangeNarrativeText();
        
        // Build the opening based on relationship
        narrative.Opening = GenerateOpening(
            npcName, 
            currentEmotion
        );
        
        // Create the exchange description with emotional weight
        narrative.ExchangeDescription = GenerateExchangeDescription(
            exchange,
            exchangeMemory?.GetExchangeFlavor(npcId, exchange.Template.ToString()) ?? "for the first time",
            exchangeMemory?.HasCrisisHistory(npcId, exchange.Template.ToString()) ?? false
        );
        
        // Add contextual flavor based on history
        narrative.ContextualFlavor = GenerateContextualFlavor(
            exchange,
            exchangeMemory?.GetMostSignificantExchange(npcId)
        );
        
        // Create the closing that hints at relationship impact
        narrative.Closing = GenerateClosing(
            exchange,
            exchangeMemory?.HasGenerosityPattern(npcId) ?? false
        );
        
        return narrative;
    }
    
    private static string GenerateOpening(string npcName, EmotionalState? emotion)
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
        
        return emotionalContext;
    }
    
    private static string GenerateExchangeDescription(
        ConversationCard exchange, 
        string frequencyFlavor,
        bool hasCrisisHistory)
    {
        var templateString = exchange.Template.ToString().ToLower();
        var npcPersonality = exchange.Context?.NPCPersonality ?? PersonalityType.STEADFAST;
        
        var baseDescription = templateString switch
        {
            "food" => npcPersonality switch
            {
                PersonalityType.DEVOTED => $"They offer to share their meal {frequencyFlavor}",
                PersonalityType.MERCANTILE => $"They gesture to their provisions {frequencyFlavor}",
                PersonalityType.STEADFAST => $"They pull out simple rations {frequencyFlavor}",
                _ => $"They offer food {frequencyFlavor}"
            },
            "healing" => npcPersonality switch
            {
                PersonalityType.DEVOTED => $"They prepare to offer healing prayers {frequencyFlavor}",
                PersonalityType.MERCANTILE => $"They display their medical supplies {frequencyFlavor}",
                _ => $"They offer treatment {frequencyFlavor}"
            },
            "work" => $"They mention work that needs doing {frequencyFlavor}",
            "information" => $"They lean in conspiratorially {frequencyFlavor}",
            "lodging" => npcPersonality switch
            {
                PersonalityType.STEADFAST => $"They gesture to the rooms upstairs {frequencyFlavor}",
                _ => $"They offer you a place to rest {frequencyFlavor}"
            },
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
        ConversationCard exchange,
        MemorableExchange significantMemory)
    {
        var contextLines = new List<string>();
        
        // Reference significant past exchange
        if (significantMemory != null && significantMemory.EmotionalContext == EmotionalState.DESPERATE)
        {
            contextLines.Add("You both remember when desperation brought you together.");
        }
        
        // Add personality-specific observations
        var personality = exchange.Context?.NPCPersonality ?? PersonalityType.STEADFAST;
        var reward = exchange.Context?.Reward ?? new List<ResourceExchange>();
        var cost = exchange.Context?.Cost ?? new List<ResourceExchange>();
        
        switch (personality)
        {
            case PersonalityType.DEVOTED:
                if (reward.Exists(r => r.ResourceType == ResourceType.Health))
                    contextLines.Add("Their faith guides their healing touch.");
                break;
            case PersonalityType.MERCANTILE:
                if (cost.Exists(c => c.ResourceType == ResourceType.Coins))
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
        ConversationCard exchange,
        bool hasGenerosityPattern)
    {
        if (hasGenerosityPattern)
        {
            return "Your consistent generosity hasn't gone unnoticed.";
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