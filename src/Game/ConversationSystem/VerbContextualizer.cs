using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The three core conversation verbs - NEVER shown directly to players
/// </summary>
public enum BaseVerb
{
    HELP,        // Accept letters, offer assistance
    NEGOTIATE,   // Trade positions, time, resources
    INVESTIGATE  // Learn information, discover options
}

/// <summary>
/// Generates choices from queue state, then maps them to hidden verbs for consistency.
/// The player never sees "PLACATE" - they see "Take her trembling hand in comfort"
/// </summary>
public class VerbContextualizer
{
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly AttentionManager _attentionManager;
    private readonly LetterQueueManager _queueManager;
    private readonly GameWorld _gameWorld;
    private readonly Random _random = new Random();

    public VerbContextualizer(
        ConnectionTokenManager tokenManager,
        NPCEmotionalStateCalculator stateCalculator,
        AttentionManager attentionManager,
        LetterQueueManager queueManager,
        GameWorld gameWorld)
    {
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _attentionManager = attentionManager;
        _queueManager = queueManager;
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Generate choices from queue state following IMPLEMENTATION-PLAN.md rules:
    /// 1. Always include 1 free "exit" option
    /// 2. Show 1-2 contextual HELP options  
    /// 3. Show 1-2 NEGOTIATE options if applicable
    /// 4. Show 1 INVESTIGATE option if attention available
    /// 5. Never exceed 5 total choices
    /// </summary>
    public List<ConversationChoice> GenerateChoicesFromQueueState(NPC npc, AttentionManager attention)
    {
        var choices = new List<ConversationChoice>();
        var player = _gameWorld.GetPlayer();
        
        // ANALYZE QUEUE STATE
        var npcLetters = _queueManager.GetActiveLetters()
            .Where(l => l.SenderId == npc.ID || l.SenderName == npc.Name)
            .OrderBy(l => l.DeadlineInDays)
            .ToList();
            
        var mostUrgent = npcLetters.FirstOrDefault();
        var npcState = _stateCalculator.CalculateState(npc);
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        
        // RULE 1: Always include 1 free "exit" option
        choices.Add(CreateExitChoice(npcState));
        
        // RULE 2: Show 1-2 contextual HELP options
        if (mostUrgent != null)
        {
            // HELP: Accept to deliver their letter
            choices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = $"I understand. Your letter is {GetQueuePositionText(mostUrgent.QueuePosition)} in my queue.",
                AttentionCost = 0,
                BaseVerb = BaseVerb.HELP,
                IsAvailable = true,
                IsAffordable = true,
                BodyLanguageHint = "→ Maintains current state"
            });
            
            // HELP: Offer concrete assistance with their letter
            if (attention.CanAfford(1) && npcState == NPCEmotionalState.DESPERATE)
            {
                choices.Add(new ConversationChoice
                {
                    ChoiceID = Guid.NewGuid().ToString(),
                    NarrativeText = "I'll make sure your letter is delivered today, I promise.",
                    AttentionCost = 1,
                    BaseVerb = BaseVerb.HELP,
                    IsAvailable = true,
                    IsAffordable = true,
                    BodyLanguageHint = "♥ +1 Trust token | ⏱ Commits to delivery",
                    MechanicalEffect = new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager)
                });
            }
        }
        
        // RULE 3: Show 1-2 NEGOTIATE options if applicable
        if (mostUrgent != null && mostUrgent.QueuePosition > 1 && attention.CanAfford(1))
        {
            var statusBurn = CalculateStatusBurn(mostUrgent);
            choices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = "I'll prioritize your letter. Let me check what that means...",
                AttentionCost = 1,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAvailable = true,
                IsAffordable = true,
                BodyLanguageHint = statusBurn > 0 
                    ? $"✓ Opens negotiation | ⚠ Must burn {statusBurn} Status with others"
                    : "✓ Can move to top without cost",
                MechanicalEffect = new LetterReorderEffect(
                    mostUrgent.Id, 1, statusBurn, ConnectionType.Status,
                    _queueManager, _tokenManager, npc.ID)
            });
        }
        
        // Complex negotiation (2 attention points)
        if (attention.CanAfford(2) && mostUrgent != null)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = "I swear I'll deliver your letter before any others today.",
                AttentionCost = 2,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAvailable = true,
                IsAffordable = true,
                BodyLanguageHint = "♥ +2 Trust tokens immediately | ⛓ Creates Binding Obligation",
                MechanicalEffect = new CompoundEffect(new List<IMechanicalEffect> 
                {
                    new GainTokensEffect(ConnectionType.Trust, 2, npc.ID, _tokenManager),
                    new LetterReorderEffect(mostUrgent.Id, 1, 0, ConnectionType.Trust,
                        _queueManager, _tokenManager, npc.ID)
                })
            });
        }
        
        // RULE 4: Show 1 INVESTIGATE option if attention available
        if (attention.CanAfford(1))
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = mostUrgent != null 
                    ? $"Tell me more about the situation with {mostUrgent.RecipientName}..."
                    : "What's been happening in the district lately?",
                AttentionCost = 1,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAvailable = true,
                IsAffordable = true,
                BodyLanguageHint = "ℹ Gain information | ⏱ +15 minutes conversation",
                MechanicalEffect = new ConversationTimeEffect(15, null)
            });
        }
        
        // RULE 5: Never exceed 5 total choices
        return choices.Take(5).ToList();
    }
    
    private ConversationChoice CreateExitChoice(NPCEmotionalState state)
    {
        var text = state switch
        {
            NPCEmotionalState.DESPERATE => "I need to go, but I'll handle this...",
            NPCEmotionalState.HOSTILE => "I should leave.",
            NPCEmotionalState.CALCULATING => "I'll be on my way.",
            NPCEmotionalState.WITHDRAWN => "Farewell.",
            _ => "Goodbye."
        };
        
        return new ConversationChoice
        {
            ChoiceID = "exit",
            NarrativeText = text,
            AttentionCost = 0,
            BaseVerb = BaseVerb.HELP, // Exit is a form of help (ending conversation)
            IsAvailable = true,
            IsAffordable = true,
            BodyLanguageHint = "Exit conversation"
        };
    }
    
    private (int attention, int tokens) CalculateReorderCost(Letter letter, NPCEmotionalState state)
    {
        // Base cost depends on how far we're moving the letter
        int positionJump = letter.QueuePosition - 1;
        
        // Emotional state modifies costs
        var attentionCost = state switch
        {
            NPCEmotionalState.DESPERATE => Math.Max(0, positionJump / 2), // Cheaper when desperate
            NPCEmotionalState.CALCULATING => positionJump / 2 + 1,
            _ => positionJump
        };
        
        var tokenCost = Math.Max(1, positionJump - 1); // Minimum 1 token
        
        return (Math.Min(3, attentionCost), tokenCost); // Cap attention at 3
    }
    
    private string GenerateDesperateHelpText(NPC npc, Letter letter)
    {
        var deadlineText = letter.DeadlineInDays switch
        {
            <= 0 => "It's already too late, but",
            1 => "There's barely time, but",
            _ => "Yes,"
        };
        
        var stakesHint = letter.Stakes switch
        {
            StakeType.SAFETY => "lives depend on this",
            StakeType.REPUTATION => "reputations hang in the balance",
            StakeType.WEALTH => "fortunes will be lost",
            StakeType.SECRET => "secrets must be protected",
            _ => "this is critical"
        };
        
        return $"{deadlineText} I'll make sure your letter reaches {letter.RecipientName} - {stakesHint}";
    }
    
    private string GeneratePlacatingText(NPC npc, Dictionary<ConnectionType, int> tokens)
    {
        // Check if we're in debt
        var totalTokens = tokens.Values.Sum();
        
        if (totalTokens < 0)
        {
            return "I know I owe you, and I haven't forgotten...";
        }
        else if (totalTokens > 3)
        {
            return "We've been through a lot together. Let me explain...";
        }
        else
        {
            return "Please, let me explain about the delay...";
        }
    }
    
    private string GenerateReorderText(Letter letter, NPCEmotionalState state)
    {
        if (letter.QueuePosition == 2)
        {
            return "I'll handle your letter next, I promise";
        }
        else if (state == NPCEmotionalState.CALCULATING)
        {
            return $"I could prioritize your letter ahead of the others...";
        }
        else
        {
            return $"Your letter needs to move up in my priorities";
        }
    }
    
    private string GetQueuePositionText(int position)
    {
        return position switch
        {
            1 => "first",
            2 => "second",
            3 => "third",
            4 => "fourth",
            5 => "fifth",
            6 => "sixth",
            7 => "seventh",
            8 => "eighth",
            _ => $"position {position}"
        };
    }
    
    private int CalculateStatusBurn(Letter letter)
    {
        // Count how many Status-type letters are ahead of this one
        var lettersAhead = _queueManager.GetActiveLetters()
            .Where(l => l.QueuePosition < letter.QueuePosition && l.TokenType == ConnectionType.Status)
            .Count();
        return lettersAhead;
    }

    /// <summary>
    /// Get available verbs based on game state (using new 3-verb system)
    /// </summary>
    public List<BaseVerb> GetAvailableVerbs(NPC npc, NPCEmotionalState state, Player player)
    {
        var verbs = new List<BaseVerb>();

        // Different states unlock different verbs
        switch (state)
        {
            case NPCEmotionalState.DESPERATE:
                verbs.Add(BaseVerb.HELP);        // They need assistance
                verbs.Add(BaseVerb.NEGOTIATE);   // Can negotiate from weakness
                verbs.Add(BaseVerb.INVESTIGATE); // They'll share information freely
                break;
                
            case NPCEmotionalState.HOSTILE:
                verbs.Add(BaseVerb.HELP);        // Try to help/appease
                // NEGOTIATE and INVESTIGATE locked when hostile
                break;
                
            case NPCEmotionalState.CALCULATING:
                verbs.Add(BaseVerb.NEGOTIATE);   // Trade resources
                verbs.Add(BaseVerb.INVESTIGATE); // Carefully probe for info
                // HELP less useful when calculating
                break;
                
            case NPCEmotionalState.WITHDRAWN:
                verbs.Add(BaseVerb.HELP);        // Offer assistance to engage
                // Most verbs locked when withdrawn
                break;
        }

        // High relationship unlocks more options
        var npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var tokens = npcTokens.ContainsKey(ConnectionType.Trust) ? npcTokens[ConnectionType.Trust] : 0;
        if (tokens >= 5)
        {
            verbs.Add(BaseVerb.INVESTIGATE);  // Trust allows deeper probing
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