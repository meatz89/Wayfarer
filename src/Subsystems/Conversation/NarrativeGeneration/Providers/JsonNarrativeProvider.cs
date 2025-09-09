using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// JSON-based narrative provider that serves as a fallback when AI providers are unavailable.
/// Provides smart mechanical fallbacks based on game state using flow, rapport, and card properties.
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
    /// If no template found, creates smart mechanical fallbacks based on conversation state.
    /// </summary>
    public Task<NarrativeOutput> GenerateNarrativeContentAsync(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards)
    {
        // Find the best matching template for current state
        NarrativeTemplate template = repository.FindBestMatch(state, npcData);
        
        if (template == null)
        {
            return Task.FromResult(CreateSmartFallbackOutput(state, npcData, activeCards));
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

        return Task.FromResult(output);
    }

    /// <summary>
    /// JSON fallback provider is always available.
    /// Returns Task for interface compatibility but executes synchronously.
    /// </summary>
    public Task<bool> IsAvailableAsync()
    {
        // JSON provider has no external dependencies, always available
        return Task.FromResult(true);
    }

    /// <summary>
    /// Gets the provider type for identifying this provider.
    /// </summary>
    public NarrativeProviderType GetProviderType()
    {
        return NarrativeProviderType.JsonFallback;
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
    /// Creates smart mechanical fallbacks based on conversation state when no templates match.
    /// Uses flow, rapport, atmosphere, and card properties to generate appropriate responses.
    /// </summary>
    private NarrativeOutput CreateSmartFallbackOutput(ConversationState state, NPCData npcData, CardCollection activeCards)
    {
        NarrativeOutput output = new NarrativeOutput
        {
            NPCDialogue = GenerateFlowBasedDialogue(state, npcData, activeCards),
            NarrativeText = GenerateConnectionStateBodyLanguage(state.CurrentState),
            ProgressionHint = GenerateSmartProgressionHint(state, activeCards)
        };

        // Generate smart card narratives based on mechanical properties
        foreach (CardInfo card in activeCards.Cards)
        {
            output.CardNarratives[card.Id] = GenerateSmartCardNarrative(card, state, npcData);
        }

        return output;
    }

    /// <summary>
    /// Generates NPC dialogue based on flow value (0-24) and card presence.
    /// </summary>
    private string GenerateFlowBasedDialogue(ConversationState state, NPCData npcData, CardCollection activeCards)
    {
        // Check for special card requirements first
        bool hasImpulse = activeCards.Cards.Any(c => c.Persistence == CardPersistence.Impulse);
        bool hasOpening = activeCards.Cards.Any(c => c.Persistence == CardPersistence.Opening);
        bool hasHighFocusCards = activeCards.Cards.Any(c => c.Focus >= 3);

        // Rapport-based dialogue depth
        if (state.Rapport >= 16)
        {
            return "I trust you. Here's the truth.";
        }
        
        if (state.Rapport >= 11 && state.Rapport <= 15 && hasHighFocusCards)
        {
            return "You're pushing hard. Why?";
        }
        
        if (state.Rapport >= 6 && state.Rapport <= 10 && activeCards.Cards.Any(c => c.NarrativeCategory == "risk"))
        {
            return "This is difficult to discuss.";
        }
        
        if (state.Rapport >= 0 && state.Rapport <= 5)
        {
            if (hasImpulse)
            {
                return "I need an answer. Now.";
            }
            if (hasOpening)
            {
                return "Unless there's something else?";
            }
        }

        // Flow-based greetings for initial conversation
        if (state.TotalTurns <= 1)
        {
            return state.Flow switch
            {
                >= 20 and <= 24 => SelectRandomGreeting(new[] { "My friend!", "You came!", "I knew you would come." }),
                >= 15 and <= 19 => SelectRandomGreeting(new[] { "Thank goodness you're here.", "I've been waiting for you." }),
                >= 10 and <= 14 => SelectRandomGreeting(new[] { "Welcome.", "Please, come in.", "I was hoping to see you." }),
                >= 5 and <= 9 => SelectRandomGreeting(new[] { "Hello again.", "You've returned.", "Good to see you." }),
                _ => SelectRandomGreeting(new[] { "Oh. You're here.", "Yes?", "Can I help you?" })
            };
        }

        // Default based on atmosphere and personality
        return GenerateAtmosphereBasedDialogue(state.Atmosphere, npcData.Personality);
    }

    /// <summary>
    /// Selects a random greeting from the provided options to add variety.
    /// </summary>
    private string SelectRandomGreeting(string[] options)
    {
        // Use a simple deterministic selection based on system time to avoid needing random seed management
        int index = System.DateTime.Now.Millisecond % options.Length;
        return options[index];
    }

    /// <summary>
    /// Generates dialogue based on current atmosphere and NPC personality.
    /// </summary>
    private string GenerateAtmosphereBasedDialogue(AtmosphereType atmosphere, PersonalityType personality)
    {
        return atmosphere switch
        {
            AtmosphereType.Volatile => "Things are getting heated here.",
            AtmosphereType.Patient => "Take your time. We have all day.",
            AtmosphereType.Focused => "Let's get straight to the point.",
            AtmosphereType.Prepared => "I've been thinking about what you said.",
            AtmosphereType.Receptive => "Please, tell me more.",
            _ => personality switch
            {
                PersonalityType.DEVOTED => "Family comes first in all things.",
                PersonalityType.MERCANTILE => "Time is money, as they say.",
                PersonalityType.PROUD => "I have my reputation to consider.",
                PersonalityType.CUNNING => "There's more here than meets the eye.",
                _ => "What brings you here today?"
            }
        };
    }

    /// <summary>
    /// Generates body language descriptions based on connection state.
    /// </summary>
    private string GenerateConnectionStateBodyLanguage(ConnectionState connectionState)
    {
        return connectionState switch
        {
            ConnectionState.DISCONNECTED => "arms crossed, avoiding eye contact",
            ConnectionState.GUARDED => "cautious stance, measuring words",
            ConnectionState.NEUTRAL => "relaxed posture, professional demeanor",
            ConnectionState.RECEPTIVE => "leaning forward, engaged expression",
            ConnectionState.TRUSTING => "open gestures, warm smile",
            _ => "neutral stance"
        };
    }

    /// <summary>
    /// Generates card narratives based on card properties and conversation state.
    /// </summary>
    private string GenerateSmartCardNarrative(CardInfo card, ConversationState state, NPCData npcData)
    {
        // Handle by narrative category first
        string baseNarrative = card.NarrativeCategory switch
        {
            "risk" => GenerateRiskCardNarrative(card.Focus),
            "atmosphere" => GenerateAtmosphereCardNarrative(state.Atmosphere),
            "utility" => GenerateUtilityCardNarrative(card),
            "support" => "You offer understanding and support",
            "pressure" => "You press for more information",
            "probe" => "You carefully explore deeper",
            _ => "You consider your response carefully"
        };

        // Modify based on difficulty and focus cost
        if (card.Focus >= 3)
        {
            return $"{baseNarrative} with bold conviction";
        }
        
        if (card.Difficulty == Difficulty.VeryHard || card.Difficulty == Difficulty.Hard)
        {
            return $"{baseNarrative}, risking their reaction";
        }

        return baseNarrative;
    }

    /// <summary>
    /// Generates risk card narratives based on focus cost.
    /// </summary>
    private string GenerateRiskCardNarrative(int focus)
    {
        return focus switch
        {
            1 => "Show understanding",
            2 => "Press the issue",
            >= 3 => "Take bold action",
            _ => "Take a calculated approach"
        };
    }

    /// <summary>
    /// Generates atmosphere card narratives based on current atmosphere.
    /// </summary>
    private string GenerateAtmosphereCardNarrative(AtmosphereType currentAtmosphere)
    {
        return currentAtmosphere switch
        {
            AtmosphereType.Volatile => "Raise emotional stakes",
            AtmosphereType.Patient => "Slow things down",
            AtmosphereType.Focused => "Cut through confusion",
            AtmosphereType.Prepared => "Build on the foundation",
            _ => "Shift the conversation's tone"
        };
    }

    /// <summary>
    /// Generates utility card narratives based on card effect.
    /// </summary>
    private string GenerateUtilityCardNarrative(CardInfo card)
    {
        if (card.Effect?.Contains("draw") == true || card.Id.Contains("draw"))
        {
            return "Gather thoughts";
        }
        
        if (card.Effect?.Contains("focus") == true || card.Id.Contains("focus"))
        {
            return "Center yourself";
        }
        
        return "Take a moment to think";
    }

    /// <summary>
    /// Generates smart progression hints based on current state and available cards.
    /// </summary>
    private string GenerateSmartProgressionHint(ConversationState state, CardCollection activeCards)
    {
        // Check card-specific hints first
        if (activeCards.Cards.Any(c => c.Persistence == CardPersistence.Impulse))
        {
            return "An urgent matter requires immediate attention.";
        }
        
        if (activeCards.Cards.Any(c => c.Persistence == CardPersistence.Opening))
        {
            return "There's an opportunity to invite deeper discussion.";
        }

        // State-based hints
        if (state.Patience < 3)
        {
            return "Time is running out. Direct action may be necessary.";
        }
        
        if (state.Focus < 2)
        {
            return "Consider gathering focus before attempting difficult conversations.";
        }
        
        if (state.Rapport < 0)
        {
            return "Building trust through supportive responses may open new opportunities.";
        }
        
        if (state.Flow < 5)
        {
            return "The conversation feels distant. Finding common ground might help.";
        }
        
        if (state.Rapport >= 15 && state.Flow >= 15)
        {
            return "High trust and connection suggest deeper topics may be accessible.";
        }

        return "Consider how your response might affect the direction of this conversation.";
    }
}