using System.Collections.Generic;

/// <summary>
/// JSON-based narrative provider that serves as a fallback when AI providers are unavailable.
/// Loads pre-authored narrative content from JSON files and matches it to conversation state.
/// </summary>
public class JsonNarrativeProvider : INarrativeProvider
{
    private readonly JsonNarrativeRepository repository;

    public JsonNarrativeProvider(JsonNarrativeRepository repository)
    {
        this.repository = repository;
    }

    /// <summary>
    /// Generates narrative content by finding the best matching template and mapping cards.
    /// Implements backwards construction by analyzing available cards and providing responses.
    /// </summary>
    public NarrativeOutput GenerateNarrativeContent(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards)
    {
        // Find the best matching template for current state
        NarrativeTemplate template = repository.FindBestMatch(state, npcData);
        
        if (template == null)
        {
            return CreateFallbackOutput(activeCards);
        }

        // Generate the output using the template
        NarrativeOutput output = new NarrativeOutput
        {
            NPCDialogue = ApplyVariableSubstitution(template.NPCDialogue, npcData),
            NarrativeText = template.NarrativeText != null 
                ? ApplyVariableSubstitution(template.NarrativeText, npcData) 
                : null
        };

        // Generate card narratives for all active cards
        foreach (CardInfo card in activeCards.Cards)
        {
            string cardNarrative = repository.GetCardNarrative(
                card.Id, 
                card.NarrativeCategory ?? "default", 
                template);
            
            output.CardNarratives[card.Id] = ApplyVariableSubstitution(cardNarrative, npcData);
        }

        // Generate progression hint based on current state
        output.ProgressionHint = GenerateProgressionHint(state, npcData, template);

        return output;
    }

    /// <summary>
    /// JSON fallback provider is always available.
    /// </summary>
    public bool IsAvailable()
    {
        return true;
    }

    /// <summary>
    /// Returns the provider name for debugging and logging.
    /// </summary>
    public string GetProviderName()
    {
        return "JSON Fallback";
    }

    /// <summary>
    /// Applies simple variable substitution to text templates.
    /// Replaces {npcName} and {playerName} with actual values.
    /// </summary>
    private string ApplyVariableSubstitution(string text, NPCData npcData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        string result = text;

        // Replace NPC name
        if (npcData != null && !string.IsNullOrEmpty(npcData.Name))
        {
            result = result.Replace("{npcName}", npcData.Name);
        }

        // Player name substitution would go here if available
        // result = result.Replace("{playerName}", playerName);

        return result;
    }

    /// <summary>
    /// Generates a progression hint based on current conversation state.
    /// Provides guidance to help the player understand their options.
    /// </summary>
    private string GenerateProgressionHint(ConversationState state, NPCData npcData, NarrativeTemplate template)
    {
        // Low rapport situations need trust building
        if (state.Rapport < 0)
        {
            return "Building trust through supportive responses may open new opportunities.";
        }

        // Low flow situations suggest the NPC is distant
        if (state.Flow < 5)
        {
            return "The conversation feels formal. Finding common ground might help establish a connection.";
        }

        // Medium rapport with good flow suggests progress is possible
        if (state.Rapport >= 6 && state.Rapport <= 15 && state.Flow >= 10)
        {
            return "There's growing trust here. Deeper topics might become accessible.";
        }

        // High rapport suggests the NPC might be ready for important requests
        if (state.Rapport > 15)
        {
            return "The conversation has reached a level of genuine trust and openness.";
        }

        // Low focus might indicate the player needs to LISTEN
        if (state.Focus < 3)
        {
            return "Taking time to listen carefully might provide more options.";
        }

        // Low patience suggests urgency
        if (state.Patience < 5)
        {
            return "Time is running short. Direct action may be necessary.";
        }

        // Default hint
        return "Consider how your response might affect the direction of this conversation.";
    }

    /// <summary>
    /// Creates a fallback narrative output when no templates match.
    /// Ensures the system always returns valid content.
    /// </summary>
    private NarrativeOutput CreateFallbackOutput(CardCollection activeCards)
    {
        NarrativeOutput output = new NarrativeOutput
        {
            NPCDialogue = "I'm not sure what to say about that.",
            NarrativeText = "The conversation has reached an awkward pause.",
            ProgressionHint = "Perhaps try a different approach to move the conversation forward."
        };

        // Provide basic card narratives
        foreach (CardInfo card in activeCards.Cards)
        {
            string category = card.NarrativeCategory ?? "default";
            string narrative = GetFallbackCardNarrative(category);
            output.CardNarratives[card.Id] = narrative;
        }

        return output;
    }

    /// <summary>
    /// Provides fallback card narratives when no template is available.
    /// </summary>
    private string GetFallbackCardNarrative(string category)
    {
        return category switch
        {
            "risk" => "You take a calculated risk.",
            "support" => "You offer your support.",
            "pressure" => "You press the issue.",
            "probe" => "You probe for more information.",
            "atmosphere" => "You shift the conversation's tone.",
            "utility" => "You gather your thoughts.",
            _ => "You consider your response carefully."
        };
    }
}