using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Enums;

/// <summary>
/// Integration service that bridges the game's existing conversation system 
/// with the new narrative generation system. Converts between game models
/// and narrative models, manages provider selection, and handles fallback scenarios.
/// </summary>
public class ConversationNarrativeService
{
    private readonly NarrativeProviderFactory _providerFactory;

    public ConversationNarrativeService(
        NarrativeProviderFactory providerFactory)
    {
        _providerFactory = providerFactory;
    }

    /// <summary>
    /// Phase 1 only: Generates NPC dialogue without card narratives.
    /// For progressive UI updates - show NPC dialogue first.
    /// </summary>
    public async Task<NarrativeOutput> GenerateOnlyNPCDialogueAsync(
        ConversationSession session,
        NPC npc,
        List<CardInstance> activeCards)
    {
        Console.WriteLine("[ConversationNarrativeService] GenerateOnlyNPCDialogueAsync called");
        try
        {
            // Get provider from factory
            INarrativeProvider provider = await _providerFactory.GetProviderAsync();
            Console.WriteLine($"[ConversationNarrativeService] Got provider for phase 1: {provider?.GetProviderType().ToString() ?? "null"}");

            // Convert game models to narrative models
            ConversationState conversationState = BuildConversationState(session);
            NPCData npcData = BuildNPCData(npc);
            CardCollection cardCollection = BuildCardCollection(activeCards);

            // Generate NPC dialogue only (Phase 1)
            NarrativeOutput narrativeOutput = await provider.GenerateNPCDialogueAsync(
                conversationState,
                npcData,
                cardCollection);

            // Add provider source for UI styling
            narrativeOutput.ProviderSource = provider.GetProviderType();

            return narrativeOutput;
        }
        catch
        {
            // If narrative generation fails, provide minimal fallback
            return CreateMinimalFallbackNarrative(session, npc, activeCards);
        }
    }

    /// <summary>
    /// Phase 2 only: Generates card narratives based on existing NPC dialogue.
    /// For progressive UI updates - show card narratives after NPC dialogue.
    /// </summary>
    public async Task<List<CardNarrative>> GenerateOnlyCardNarrativesAsync(
        ConversationSession session,
        NPC npc,
        List<CardInstance> activeCards,
        string npcDialogue)
    {
        Console.WriteLine("[ConversationNarrativeService] GenerateOnlyCardNarrativesAsync called");
        try
        {
            // Get provider from factory
            INarrativeProvider provider = await _providerFactory.GetProviderAsync();
            Console.WriteLine($"[ConversationNarrativeService] Got provider for phase 2: {provider?.GetProviderType().ToString() ?? "null"}");

            // Convert game models to narrative models
            ConversationState conversationState = BuildConversationState(session);
            NPCData npcData = BuildNPCData(npc);
            CardCollection cardCollection = BuildCardCollection(activeCards);

            // Generate card narratives only (Phase 2)
            List<CardNarrative> cardNarratives = await provider.GenerateCardNarrativesAsync(
                conversationState,
                npcData,
                cardCollection,
                npcDialogue);

            return cardNarratives ?? new List<CardNarrative>();
        }
        catch
        {
            // If narrative generation fails, provide empty list
            Console.WriteLine("[ConversationNarrativeService] Failed to generate card narratives, returning empty list");
            return new List<CardNarrative>();
        }
    }

    /// <summary>
    /// Generates narrative content for current conversation state using available providers.
    /// Converts game models to narrative models, calls provider, and handles fallback.
    /// This does BOTH phases at once for backward compatibility.
    /// </summary>
    /// <param name="session">Current conversation session</param>
    /// <param name="npc">NPC being conversed with</param>
    /// <param name="activeCards">List of cards currently available to player</param>
    /// <returns>Generated narrative output or fallback content</returns>
    public async Task<NarrativeOutput> GenerateNarrativeAsync(
        ConversationSession session, 
        NPC npc, 
        List<CardInstance> activeCards)
    {
        Console.WriteLine("[ConversationNarrativeService] GenerateNarrativeAsync called");
        try
        {
            // Get provider from factory - properly await async operation
            Console.WriteLine("[ConversationNarrativeService] Getting provider from factory...");
            INarrativeProvider provider = await _providerFactory.GetProviderAsync();
            Console.WriteLine($"[ConversationNarrativeService] Got provider: {provider?.GetProviderType().ToString() ?? "null"}");

            // Convert game models to narrative models
            ConversationState conversationState = BuildConversationState(session);
            NPCData npcData = BuildNPCData(npc);
            CardCollection cardCollection = BuildCardCollection(activeCards);

            // Generate NPC dialogue first
            NarrativeOutput narrativeOutput = await provider.GenerateNPCDialogueAsync(
                conversationState, 
                npcData, 
                cardCollection);
            
            // Then generate card narratives using the NPC dialogue
            List<CardNarrative> cardNarratives = await provider.GenerateCardNarrativesAsync(
                conversationState,
                npcData,
                cardCollection,
                narrativeOutput.NPCDialogue);
            
            // Add card narratives to output
            narrativeOutput.CardNarratives = cardNarratives;
            
            // Add provider source for UI styling
            narrativeOutput.ProviderSource = provider.GetProviderType();

            return narrativeOutput;
        }
        catch
        {
            // If narrative generation fails completely, provide minimal fallback
            // This ensures the game continues functioning even if narrative systems fail
            return CreateMinimalFallbackNarrative(session, npc, activeCards);
        }
    }

    /// <summary>
    /// Creates minimal fallback narrative when all providers fail.
    /// Ensures game continues functioning with basic descriptive text.
    /// </summary>
    private NarrativeOutput CreateMinimalFallbackNarrative(
        ConversationSession session, 
        NPC npc, 
        List<CardInstance> activeCards)
    {
        NarrativeOutput fallback = new NarrativeOutput
        {
            NPCDialogue = $"{npc.Name} looks at you expectantly.",
            NarrativeText = $"The conversation continues with {npc.Name}.",
            CardNarratives = new List<CardNarrative>()
        };

        // Generate basic card narratives
        foreach (CardInstance card in activeCards)
        {
            string basicNarrative = card.Focus <= 1 ? "Say something carefully" :
                                   card.Focus >= 3 ? "Speak boldly" :
                                   "Continue the conversation";
            fallback.CardNarratives.Add(new CardNarrative
            {
                CardId = card.Id,
                NarrativeText = basicNarrative,
                ProviderSource = NarrativeProviderType.JsonFallback
            });
        }

        return fallback;
    }

    /// <summary>
    /// Converts ConversationSession to ConversationState for narrative generation.
    /// </summary>
    /// <param name="session">Game conversation session</param>
    /// <returns>Narrative conversation state</returns>
    private ConversationState BuildConversationState(ConversationSession session)
    {
        int rapport = session.RapportManager?.CurrentRapport ?? 0;
        TopicLayer currentLayer = DetermineTopicLayer(rapport);
        
        return new ConversationState
        {
            Flow = session.FlowBattery, // Internal 0-24 scale from FlowBattery
            Rapport = rapport,
            Atmosphere = session.CurrentAtmosphere,
            Focus = session.GetAvailableFocus(),
            Patience = session.CurrentPatience,
            CurrentState = session.CurrentState,
            CurrentTopicLayer = currentLayer,
            HighestTopicLayerReached = currentLayer, // Will need session tracking for persistence
            TotalTurns = session.TurnNumber,
            ConversationHistory = BuildConversationHistory(session.TurnHistory)
        };
    }
    
    /// <summary>
    /// Converts conversation turn history to strings for AI narrative generation.
    /// </summary>
    /// <param name="turnHistory">List of conversation turns</param>
    /// <returns>List of formatted conversation history strings</returns>
    private List<string> BuildConversationHistory(List<ConversationTurn> turnHistory)
    {
        List<string> history = new List<string>();
        
        foreach (ConversationTurn turn in turnHistory)
        {
            // Format each turn as a readable conversation entry
            if (turn.ActionType == ActionType.Listen)
            {
                // Listen turns - show NPC dialogue
                if (!string.IsNullOrEmpty(turn.Narrative?.NPCDialogue))
                {
                    history.Add($"NPC: \"{turn.Narrative.NPCDialogue}\"");
                }
            }
            else if (turn.ActionType == ActionType.Speak)
            {
                // Speak turns - show what card was played
                if (turn.CardPlayed != null)
                {
                    string cardDescription = turn.CardPlayed.Description ?? turn.CardPlayed.Id;
                    history.Add($"Player: {cardDescription}");
                    
                    // Also include NPC's response if available
                    if (!string.IsNullOrEmpty(turn.Narrative?.NPCDialogue))
                    {
                        history.Add($"NPC: \"{turn.Narrative.NPCDialogue}\"");
                    }
                }
            }
        }
        
        return history;
    }
    
    /// <summary>
    /// Determines the topic layer based on rapport level.
    /// </summary>
    /// <param name="rapport">Current rapport value</param>
    /// <returns>Appropriate topic layer</returns>
    private TopicLayer DetermineTopicLayer(int rapport)
    {
        if (rapport <= 5) return TopicLayer.Deflection;
        if (rapport <= 10) return TopicLayer.Gateway;
        return TopicLayer.Core;
    }

    /// <summary>
    /// Converts NPC to NPCData for narrative generation.
    /// </summary>
    /// <param name="npc">Game NPC instance</param>
    /// <returns>Narrative NPC data</returns>
    private NPCData BuildNPCData(NPC npc)
    {
        return new NPCData
        {
            NpcId = npc.ID,
            Name = npc.Name,
            Personality = npc.PersonalityType,
            CurrentCrisis = ExtractCurrentCrisis(npc),
            CurrentTopic = DetermineCurrentTopic(npc)
        };
    }

    /// <summary>
    /// Converts List<CardInstance> to CardCollection for narrative generation.
    /// </summary>
    /// <param name="cards">Game card instances</param>
    /// <returns>Narrative card collection</returns>
    private CardCollection BuildCardCollection(List<CardInstance> cards)
    {
        CardCollection collection = new CardCollection();

        foreach (CardInstance card in cards)
        {
            CardInfo cardInfo = new CardInfo
            {
                Id = card.Id,
                Focus = card.Focus,
                Difficulty = card.Difficulty,
                Effect = card.SuccessType.ToString() ?? card.Description ?? "",
                Persistence = DetermineCardPersistence(card),
                NarrativeCategory = DetermineNarrativeCategory(card)
            };

            collection.Cards.Add(cardInfo);
        }

        return collection;
    }

    /// <summary>
    /// Extracts current crisis information from NPC data.
    /// </summary>
    /// <param name="npc">NPC to analyze</param>
    /// <returns>Crisis description or null if no crisis</returns>
    private string ExtractCurrentCrisis(NPC npc)
    {
        // Look for crisis indicators in NPC state or description
        if (npc.CurrentState == ConnectionState.DISCONNECTED)
        {
            // Check for common crisis patterns in personality description
            string personality = npc.PersonalityDescription?.ToLower() ?? "";
            
            if (personality.Contains("forced") || personality.Contains("marriage"))
                return "forced_marriage";
            if (personality.Contains("debt") || personality.Contains("money"))
                return "financial_troubles";
            if (personality.Contains("family") || personality.Contains("children"))
                return "family_crisis";
                
            // Generic crisis for disconnected NPCs without specific patterns
            return "personal_troubles";
        }

        return null;
    }

    /// <summary>
    /// Determines current conversation topic based on rapport and NPC state.
    /// </summary>
    /// <param name="npc">NPC to analyze</param>
    /// <returns>Current topic string</returns>
    private string DetermineCurrentTopic(NPC npc)
    {
        // Start with general topics, escalate based on connection state
        return npc.CurrentState switch
        {
            ConnectionState.DISCONNECTED => "personal_troubles",
            ConnectionState.GUARDED => "local_matters",
            ConnectionState.NEUTRAL => "general",
            ConnectionState.RECEPTIVE => "personal_interests",
            ConnectionState.TRUSTING => "deep_concerns",
            _ => "general"
        };
    }

    /// <summary>
    /// Maps CardInstance properties to CardPersistence for narrative timing.
    /// </summary>
    /// <param name="card">Card instance to analyze</param>
    /// <returns>Persistence type for narrative generation</returns>
    private PersistenceType DetermineCardPersistence(CardInstance card)
    {
        return card.Persistence;
    }

    /// <summary>
    /// Determines narrative category for backwards construction based on card properties and effects.
    /// </summary>
    /// <param name="card">Card instance to categorize</param>
    /// <returns>Narrative category string</returns>
    private string DetermineNarrativeCategory(CardInstance card)
    {
        // Check for atmosphere effects (indicates risk/pressure cards)
        if (card.SuccessType == SuccessEffectType.Atmospheric)
        {
            return "atmosphere_change";
        }

        // Check for failure effects (indicates risk)
        if (card.FailureType != FailureEffectType.None)
        {
            if (card.FailureType == FailureEffectType.Overreach)
                return "risk_extreme";
            if (card.FailureType == FailureEffectType.Backfire)
                return "risk_with_consequence";
            if (card.FailureType == FailureEffectType.Disrupting)
                return "risk_disrupting";
        }
        
        // Difficulty-based risk assessment
        if (card.Difficulty == Difficulty.VeryHard) 
            return "risk_high";
        if (card.Difficulty == Difficulty.Hard) 
            return "risk_moderate";
        
        // Card persistence-based categories
        if (card.Persistence == PersistenceType.Opening)
            return "probe";
        if (card.Persistence == PersistenceType.Impulse)
            return "pressure";
        
        // Token type indicates support/connection building
        if (card.TokenType == ConnectionType.Trust) 
            return "support_trust";
        if (card.TokenType == ConnectionType.Commerce) 
            return "support_commerce";
        if (card.TokenType == ConnectionType.Status) 
            return "support_status";
        if (card.TokenType == ConnectionType.Shadow) 
            return "support_shadow";
        
        // Default
        return "standard";
    }
}