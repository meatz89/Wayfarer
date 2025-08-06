using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The four core social navigation verbs - NEVER shown directly to players
/// </summary>
public enum BaseVerb
{
    PLACATE,  // Reduce tension, buy time
    EXTRACT,  // Get information or favors
    DEFLECT,  // Redirect pressure elsewhere
    COMMIT    // Make binding promises
}

/// <summary>
/// Transforms hidden mechanical verbs into contextual narrative choices.
/// The player never sees "PLACATE" - they see "Take her trembling hand in comfort"
/// </summary>
public class VerbContextualizer
{
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly AttentionManager _attentionManager;

    public VerbContextualizer(
        ConnectionTokenManager tokenManager,
        NPCEmotionalStateCalculator stateCalculator,
        AttentionManager attentionManager)
    {
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _attentionManager = attentionManager;
    }

    /// <summary>
    /// Get available verbs based on game state
    /// </summary>
    public List<BaseVerb> GetAvailableVerbs(NPC npc, NPCEmotionalState state, Player player)
    {
        var verbs = new List<BaseVerb>();

        // Different states unlock different verbs
        switch (state)
        {
            case NPCEmotionalState.DESPERATE:
                verbs.Add(BaseVerb.PLACATE);  // They need comfort
                verbs.Add(BaseVerb.COMMIT);   // They need promises
                verbs.Add(BaseVerb.EXTRACT);  // They'll share information
                break;
                
            case NPCEmotionalState.HOSTILE:
                verbs.Add(BaseVerb.PLACATE);  // Try to calm them
                verbs.Add(BaseVerb.DEFLECT);  // Redirect their anger
                // COMMIT and EXTRACT locked when hostile
                break;
                
            case NPCEmotionalState.CALCULATING:
                verbs.Add(BaseVerb.EXTRACT);  // Trade information
                verbs.Add(BaseVerb.DEFLECT);  // Negotiate
                verbs.Add(BaseVerb.COMMIT);   // Make deals
                // PLACATE less useful when calculating
                break;
                
            case NPCEmotionalState.WITHDRAWN:
                verbs.Add(BaseVerb.PLACATE);  // Try to engage them
                // Most verbs locked when withdrawn
                break;
        }

        // High relationship unlocks more options
        var npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var tokens = npcTokens.ContainsKey(ConnectionType.Trust) ? npcTokens[ConnectionType.Trust] : 0;
        if (tokens >= 5)
        {
            verbs.Add(BaseVerb.EXTRACT);  // Trust allows deeper probing
        }

        return verbs.Distinct().ToList();
    }

    /// <summary>
    /// Transform a verb into contextual narrative text
    /// </summary>
    public string GetNarrativePresentation(
        BaseVerb verb,
        NPCEmotionalState state,
        ConnectionType context,
        int tokenCount,
        StakeType stakes)
    {
        return (verb, context, state) switch
        {
            // PLACATE variations
            (BaseVerb.PLACATE, ConnectionType.Trust, NPCEmotionalState.DESPERATE) =>
                "Take her trembling hand in comfort",
            (BaseVerb.PLACATE, ConnectionType.Trust, NPCEmotionalState.HOSTILE) =>
                "I understand why you're upset...",
            (BaseVerb.PLACATE, ConnectionType.Commerce, NPCEmotionalState.DESPERATE) =>
                "Let me see what I can do about the payment",
            (BaseVerb.PLACATE, ConnectionType.Commerce, NPCEmotionalState.HOSTILE) =>
                "Perhaps we can work out a partial payment",
            (BaseVerb.PLACATE, ConnectionType.Status, _) =>
                "Of course, your position deserves respect",
            (BaseVerb.PLACATE, ConnectionType.Shadow, _) =>
                "I mean no threat - see, my hands are empty",

            // EXTRACT variations
            (BaseVerb.EXTRACT, _, NPCEmotionalState.DESPERATE) when tokenCount >= 3 =>
                "What's really troubling you? You can trust me",
            (BaseVerb.EXTRACT, ConnectionType.Trust, _) =>
                "Tell me more about this situation",
            (BaseVerb.EXTRACT, ConnectionType.Commerce, _) =>
                "What information might sweeten this deal?",
            (BaseVerb.EXTRACT, ConnectionType.Status, _) =>
                "Who else knows about this matter?",
            (BaseVerb.EXTRACT, ConnectionType.Shadow, _) =>
                "I need to know what I'm really carrying",

            // DEFLECT variations
            (BaseVerb.DEFLECT, _, NPCEmotionalState.HOSTILE) =>
                "Perhaps you should speak with the postmaster about this",
            (BaseVerb.DEFLECT, ConnectionType.Trust, _) =>
                "Have you considered asking Elena instead?",
            (BaseVerb.DEFLECT, ConnectionType.Commerce, _) =>
                "The guild sets these rates, not I",
            (BaseVerb.DEFLECT, ConnectionType.Status, _) =>
                "Surely someone of your standing has other options",
            (BaseVerb.DEFLECT, ConnectionType.Shadow, _) =>
                "I'm just the messenger - take it up with them",

            // COMMIT variations
            (BaseVerb.COMMIT, _, NPCEmotionalState.DESPERATE) =>
                "I swear I'll deliver your letter before any others today",
            (BaseVerb.COMMIT, ConnectionType.Trust, _) when tokenCount >= 5 =>
                "You have my word as a friend",
            (BaseVerb.COMMIT, ConnectionType.Commerce, _) =>
                "I'll guarantee delivery for the agreed price",
            (BaseVerb.COMMIT, ConnectionType.Status, _) =>
                "I pledge my service to your cause",
            (BaseVerb.COMMIT, ConnectionType.Shadow, _) =>
                "Consider it done - no questions asked",

            // Default fallbacks
            _ => GetGenericPresentation(verb)
        };
    }

    /// <summary>
    /// Get the attention cost for a verb in context
    /// </summary>
    public int GetAttentionCost(BaseVerb verb, NPCEmotionalState state)
    {
        // Base costs
        int baseCost = verb switch
        {
            BaseVerb.PLACATE => 1,
            BaseVerb.EXTRACT => 1,
            BaseVerb.DEFLECT => 1,
            BaseVerb.COMMIT => 2,  // Promises are more demanding
            _ => 1
        };

        // Apply state modifier
        int modifier = _stateCalculator.GetAttentionCostModifier(state);
        
        // Costs can be 0, 1, or 2
        return Math.Max(0, Math.Min(2, baseCost + modifier));
    }

    /// <summary>
    /// Generate a complete choice with all narrative elements
    /// </summary>
    public ConversationChoice GenerateChoice(
        BaseVerb verb,
        NPC npc,
        NPCEmotionalState state,
        Letter relevantLetter = null)
    {
        var npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var tokens = npcTokens.ContainsKey(ConnectionType.Trust) ? npcTokens[ConnectionType.Trust] : 0;
        var context = relevantLetter?.TokenType ?? ConnectionType.Trust;
        var stakes = relevantLetter?.Stakes ?? StakeType.REPUTATION;

        var narrativeText = GetNarrativePresentation(verb, state, context, tokens, stakes);
        var attentionCost = GetAttentionCost(verb, state);

        return new ConversationChoice
        {
            ChoiceID = Guid.NewGuid().ToString(),
            NarrativeText = narrativeText,
            AttentionCost = attentionCost,
            BaseVerb = verb,  // Store but never display
            IsAvailable = _attentionManager.CanAfford(attentionCost)
        };
    }

    private string GetGenericPresentation(BaseVerb verb)
    {
        return verb switch
        {
            BaseVerb.PLACATE => "Try to ease the tension",
            BaseVerb.EXTRACT => "Probe for more information",
            BaseVerb.DEFLECT => "Redirect the conversation",
            BaseVerb.COMMIT => "Make a promise",
            _ => "Respond carefully"
        };
    }

    /// <summary>
    /// Generate narrative tags for AI content generation
    /// </summary>
    public List<string> GenerateNarrativeTags(
        GameWorld gameWorld,
        NPC npc,
        NPCEmotionalState emotionalState,
        Letter currentLetter)
    {
        var tags = new List<string>();

        // Letter context
        if (currentLetter != null)
        {
            tags.Add($"[{currentLetter.TokenType}]");
            tags.Add($"[{currentLetter.Stakes}]");
            tags.Add($"[TTL:{currentLetter.DeadlineInDays}]");
            tags.Add($"[Weight:{currentLetter.GetRequiredSlots()}]");
        }

        // NPC state
        tags.Add($"[{emotionalState}]");
        
        // Token relationship
        var npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var tokens = npcTokens.ContainsKey(ConnectionType.Trust) ? npcTokens[ConnectionType.Trust] : 0;
        tags.Add($"[Tokens:{tokens}]");

        // Location context
        var player = gameWorld.GetPlayer();
        tags.Add($"[Location:{player.CurrentLocationSpot?.LocationId ?? "unknown"}]");

        // Attention remaining
        tags.Add($"[Attention:{_attentionManager.Current}]");

        return tags;
    }
}

// Note: ConversationChoice extension properties are added via partial class