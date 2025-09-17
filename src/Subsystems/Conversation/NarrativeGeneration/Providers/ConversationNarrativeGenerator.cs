using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Core backwards construction algorithm for generating conversation narratives.
/// Implements the principle of analyzing cards first, then generating NPC dialogue 
/// that all cards can respond to meaningfully.
/// </summary>
public class ConversationNarrativeGenerator
{
    /// <summary>
    /// Generates narrative content using backwards construction.
    /// Analyzes active cards to determine narrative constraints, then generates
    /// NPC dialogue that all cards can respond to appropriately.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npc">NPC data for personality and context</param>
    /// <param name="cards">Active cards available to player</param>
    /// <returns>Generated narrative output with NPC dialogue and card responses</returns>
    public NarrativeOutput GenerateNarrative(ConversationState state, NPCData npc, CardCollection cards)
    {
        // Phase 1: Analyze what cards player has available
        CardAnalysis analysis = AnalyzeActiveCards(cards);

        // Phase 2: Determine what kind of NPC dialogue is needed
        NarrativeConstraints constraints = DetermineNarrativeConstraints(analysis);

        // Phase 3: Generate NPC dialogue that works for all cards
        string npcDialogue = GenerateNPCDialogue(constraints, npc, state);

        // Phase 4: Map each card to appropriate response narrative
        List<CardNarrative> cardNarratives = MapCardNarratives(cards, npcDialogue, state.Rapport);

        return new NarrativeOutput
        {
            NPCDialogue = npcDialogue,
            NarrativeText = GenerateEnvironmentalNarrative(state, npc),
            CardNarratives = cardNarratives,
            ProgressionHint = GenerateProgressionHint(state, npc, analysis)
        };
    }

    /// <summary>
    /// Analyzes active cards to understand player options and constraints.
    /// Identifies persistence requirements, focus patterns, and dominant categories.
    /// </summary>
    /// <param name="cards">Active cards to analyze</param>
    /// <returns>Analysis results for narrative generation</returns>
    public CardAnalysis AnalyzeActiveCards(CardCollection cards)
    {
        CardAnalysis analysis = new CardAnalysis
        {
            HasImpulse = cards.Cards.Any(c => c.Persistence == PersistenceType.Impulse),
            HasOpening = cards.Cards.Any(c => c.Persistence == PersistenceType.Opening)
        };

        // Categorize each card
        foreach (CardInfo card in cards.Cards)
        {
            analysis.CategoryBreakdown[card.Id] = card.NarrativeCategory;

            // Identify risk cards
            if (card.NarrativeCategory.StartsWith("risk"))
            {
                analysis.RiskCards.Add(card.Id);
            }

            // Identify atmosphere setters
            if (card.NarrativeCategory.Contains("atmosphere") ||
                card.NarrativeCategory.Contains("volatile") ||
                card.NarrativeCategory.Contains("pressured"))
            {
                analysis.AtmosphereSetters.Add(card.Id);
            }
        }

        // Determine focus pattern and dominant category
        analysis.FocusPattern = DetermineFocusPattern(cards);
        analysis.DominantCategory = DetermineDominantCategory(cards);

        // Set urgency and invitation requirements
        analysis.RequiresUrgency = analysis.HasImpulse || analysis.RiskCards.Any();
        analysis.RequiresInvitation = analysis.HasOpening || analysis.DominantCategory == "probe";

        return analysis;
    }

    /// <summary>
    /// Determines narrative constraints based on card analysis.
    /// Creates requirements for NPC dialogue generation.
    /// </summary>
    /// <param name="analysis">Card analysis results</param>
    /// <returns>Narrative constraints for dialogue generation</returns>
    public NarrativeConstraints DetermineNarrativeConstraints(CardAnalysis analysis)
    {
        return new NarrativeConstraints
        {
            MustIncludeUrgency = analysis.RequiresUrgency,
            MustIncludeInvitation = analysis.RequiresInvitation,
            IntensityLevel = GetIntensityFromFocusPattern(analysis.FocusPattern),
            NarrativeStyle = analysis.DominantCategory
        };
    }

    /// <summary>
    /// Generates NPC dialogue that all cards can respond to appropriately.
    /// Uses backwards construction principle to ensure narrative coherence.
    /// </summary>
    /// <param name="constraints">Narrative requirements from card analysis</param>
    /// <param name="npc">NPC personality and context data</param>
    /// <param name="state">Current conversation state</param>
    /// <returns>NPC dialogue text</returns>
    public string GenerateNPCDialogue(NarrativeConstraints constraints, NPCData npc, ConversationState state)
    {
        RapportStage rapportStage = GetRapportStage(state.Rapport);
        TopicLayer topicLayer = GetTopicLayer(state.Rapport, npc.CurrentCrisis);

        // Build base statement appropriate for rapport level and topic
        string baseStatement = GenerateBaseStatement(npc, rapportStage, topicLayer);

        // Add emotional coloring from atmosphere and flow
        string emotionalModifier = GenerateEmotionalModifier(state.Atmosphere, state.Flow);

        // Add persistence hooks if needed
        string persistenceHook = GeneratePersistenceHook(constraints);

        return CombineDialogueElements(baseStatement, emotionalModifier, persistenceHook, npc.Personality);
    }

    /// <summary>
    /// Maps each card to appropriate response narrative based on NPC dialogue.
    /// Ensures each card's response makes sense in context.
    /// </summary>
    /// <param name="cards">Active cards to map</param>
    /// <param name="npcDialogue">Generated NPC dialogue to respond to</param>
    /// <param name="rapport">Current rapport level for response scaling</param>
    /// <returns>List of card narratives with provider source</returns>
    public List<CardNarrative> MapCardNarratives(CardCollection cards, string npcDialogue, int rapport)
    {
        List<CardNarrative> narratives = new List<CardNarrative>();
        RapportStage rapportStage = GetRapportStage(rapport);

        foreach (CardInfo card in cards.Cards)
        {
            string narrative = GenerateCardResponse(card, npcDialogue, rapportStage);
            narratives.Add(new CardNarrative
            {
                CardId = card.Id,
                NarrativeText = narrative,
                ProviderSource = NarrativeProviderType.JsonFallback // This generator is used as fallback
            });
        }

        return narratives;
    }

    /// <summary>
    /// Determines rapport stage based on current rapport value.
    /// Used to gate narrative depth and topic accessibility.
    /// </summary>
    /// <param name="rapport">Current rapport value (-50 to +50)</param>
    /// <returns>Rapport stage for narrative generation</returns>
    public RapportStage GetRapportStage(int rapport)
    {
        if (rapport <= 5) return RapportStage.Surface;
        if (rapport <= 10) return RapportStage.Gateway;
        if (rapport <= 15) return RapportStage.Personal;
        return RapportStage.Intimate;
    }

    /// <summary>
    /// Determines topic layer based on rapport and crisis presence.
    /// Controls how directly NPC addresses their core problem.
    /// </summary>
    /// <param name="rapport">Current rapport level</param>
    /// <param name="crisis">NPC's current crisis description</param>
    /// <returns>Topic layer for dialogue generation</returns>
    public TopicLayer GetTopicLayer(int rapport, string crisis)
    {
        if (string.IsNullOrEmpty(crisis)) return TopicLayer.Deflection;

        if (rapport <= 5) return TopicLayer.Deflection;
        if (rapport <= 10) return TopicLayer.Gateway;
        return TopicLayer.Core;
    }

    private FocusPattern DetermineFocusPattern(CardCollection cards)
    {
        int[] focusCosts = cards.Cards.Select(c => c.Focus).ToArray();

        if (focusCosts.All(f => f <= 2)) return FocusPattern.AllLow;
        if (focusCosts.All(f => f >= 3)) return FocusPattern.AllHigh;
        return FocusPattern.Mixed;
    }

    private string DetermineDominantCategory(CardCollection cards)
    {
        Dictionary<string, int> categoryCounts = new Dictionary<string, int>();

        foreach (CardInfo card in cards.Cards)
        {
            string category = card.NarrativeCategory ?? "utility";
            categoryCounts[category] = categoryCounts.GetValueOrDefault(category, 0) + 1;
        }

        return categoryCounts.OrderByDescending(kv => kv.Value).First().Key;
    }

    private int GetIntensityFromFocusPattern(FocusPattern pattern)
    {
        return pattern switch
        {
            FocusPattern.AllLow => 1,
            FocusPattern.Mixed => 2,
            FocusPattern.AllHigh => 3,
            _ => 2
        };
    }

    private string GenerateBaseStatement(NPCData npc, RapportStage rapportStage, TopicLayer topicLayer)
    {
        // Generate context-appropriate base statement
        // This would typically use templates or a more sophisticated system
        string topic = DetermineCurrentTopic(npc, topicLayer);
        return FormatStatementForRapport(topic, rapportStage, npc.Personality);
    }

    private string DetermineCurrentTopic(NPCData npc, TopicLayer layer)
    {
        return layer switch
        {
            TopicLayer.Deflection => "general_conversation",
            TopicLayer.Gateway => "related_concerns",
            TopicLayer.Core => npc.CurrentCrisis ?? "personal_matters",
            _ => "general_conversation"
        };
    }

    private string FormatStatementForRapport(string topic, RapportStage stage, PersonalityType personality)
    {
        // Placeholder implementation - in real system this would use templates
        return stage switch
        {
            RapportStage.Surface => "Making polite observations about the situation",
            RapportStage.Gateway => "Sharing related experiences and showing understanding",
            RapportStage.Personal => "Opening up about personal concerns and challenges",
            RapportStage.Intimate => "Being vulnerable about deep fears and needs",
            _ => "Having a conversation"
        };
    }

    private string GenerateEmotionalModifier(AtmosphereType atmosphere, int flow)
    {
        // Generate emotional coloring based on current atmosphere and flow
        return atmosphere switch
        {
            AtmosphereType.Volatile => "with heightened emotion",
            AtmosphereType.Patient => "thoughtfully and carefully",
            AtmosphereType.Focused => "with clear directness",
            _ => "naturally"
        };
    }

    private string GeneratePersistenceHook(NarrativeConstraints constraints)
    {
        if (constraints.MustIncludeUrgency && constraints.MustIncludeInvitation)
            return "requiring both immediate response and follow-up";
        if (constraints.MustIncludeUrgency)
            return "demanding immediate attention";
        if (constraints.MustIncludeInvitation)
            return "inviting further discussion";
        return "";
    }

    private string CombineDialogueElements(string baseStatement, string emotionalModifier,
        string persistenceHook, PersonalityType personality)
    {
        // Combine elements into coherent NPC dialogue
        string combined = baseStatement;
        if (!string.IsNullOrEmpty(emotionalModifier))
            combined += $" {emotionalModifier}";
        if (!string.IsNullOrEmpty(persistenceHook))
            combined += $", {persistenceHook}";

        return FormatForPersonality(combined, personality);
    }

    private string FormatForPersonality(string dialogue, PersonalityType personality)
    {
        // Apply personality-specific formatting
        return personality switch
        {
            PersonalityType.DEVOTED => $"Speaking with emotional sincerity: \"{dialogue}\"",
            PersonalityType.MERCANTILE => $"In a business-like tone: \"{dialogue}\"",
            PersonalityType.PROUD => $"With dignity and composure: \"{dialogue}\"",
            _ => $"\"{dialogue}\""
        };
    }

    private string GenerateCardResponse(CardInfo card, string npcDialogue, RapportStage rapportStage)
    {
        // Generate response narrative based on card type and context
        string baseResponse = GenerateResponseByCategory(card.NarrativeCategory, card.Effect);
        return ScaleResponseToRapport(baseResponse, rapportStage, card.Focus);
    }

    private string GenerateResponseByCategory(string category, string effect)
    {
        return category switch
        {
            "risk" => "Taking a bold stance on the matter",
            "support" => "Offering understanding and assistance",
            "atmosphere" => "Shifting the emotional tone",
            "utility" => "Gathering more information",
            _ => "Responding thoughtfully"
        };
    }

    private string ScaleResponseToRapport(string response, RapportStage stage, int focus)
    {
        string intensity = focus switch
        {
            1 => "gently",
            2 => "directly",
            >= 3 => "boldly",
            _ => "carefully"
        };

        return $"{intensity.Substring(0, 1).ToUpper()}{intensity.Substring(1)} {response.ToLower()}";
    }

    private string GenerateEnvironmentalNarrative(ConversationState state, NPCData npc)
    {
        // Generate environmental description based on current state
        return "The conversation continues with growing understanding";
    }

    private string GenerateProgressionHint(ConversationState state, NPCData npc, CardAnalysis analysis)
    {
        // Generate hint about conversation progression
        if (state.Rapport >= 15 && !string.IsNullOrEmpty(npc.CurrentCrisis))
            return "The conversation has reached a point where deeper trust might unlock new possibilities";

        if (analysis.HasImpulse)
            return "Some opportunities require immediate action";

        if (analysis.HasOpening)
            return "There are opportunities to learn more about the situation";

        return null;
    }
}

/// <summary>
/// Narrative constraints determined from card analysis.
/// Used to guide NPC dialogue generation.
/// </summary>
public class NarrativeConstraints
{
    public bool MustIncludeUrgency { get; set; }
    public bool MustIncludeInvitation { get; set; }
    public int IntensityLevel { get; set; }
    public string NarrativeStyle { get; set; }
}

/// <summary>
/// Rapport stages for narrative depth control.
/// </summary>
public enum RapportStage
{
    Surface,   // 0-5: Observations, deflections
    Gateway,   // 6-10: Understanding, sharing  
    Personal,  // 11-15: Emotional support
    Intimate   // 16+: Vulnerability, deep support
}

/// <summary>
/// Topic layers for crisis discussion control.
/// </summary>
public enum TopicLayer
{
    Deflection, // Avoiding real issues
    Gateway,    // Related but indirect topics
    Core        // Direct crisis discussion
}

/// <summary>
/// Conversation progression beats for tracking narrative milestones.
/// </summary>
public enum ConversationBeat
{
    Opening,      // Initial greeting/acknowledgment
    Deflection,   // Avoiding the real topic
    Probing,      // Player asking questions
    Gateway,      // Starting to open up
    Revelation,   // Core crisis revealed
    Support,      // Player offering help
    Request,      // NPC makes request (letter/promise)
    Resolution    // Request accepted/declined
}