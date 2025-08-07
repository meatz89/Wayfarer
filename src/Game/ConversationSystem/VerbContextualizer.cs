using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The three core conversation verbs - NEVER shown directly to players
/// </summary>
public enum BaseVerb
{
    HELP,      // Accept letters, offer assistance, build trust
    NEGOTIATE, // Trade positions, time, resources, queue manipulation
    INVESTIGATE, // Learn information, discover options, probe for secrets
    EXIT       // Leave conversation, no action taken
}

/// <summary>
/// Generates choices from queue state, then maps them to hidden verbs for consistency.
/// The player never sees "PLACATE" - they see "Take her trembling hand in comfort"
/// </summary>
public class VerbContextualizer
{
    private readonly ConnectionTokenManager _tokenManager;
    private readonly AttentionManager _attentionManager;
    private readonly LetterQueueManager _queueManager;
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    private readonly Random _random = new Random();

    public VerbContextualizer(
        ConnectionTokenManager tokenManager,
        AttentionManager attentionManager,
        LetterQueueManager queueManager,
        GameWorld gameWorld,
        ITimeManager timeManager)
    {
        _tokenManager = tokenManager;
        _attentionManager = attentionManager;
        _queueManager = queueManager;
        _gameWorld = gameWorld;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Generate choices from queue state - CONVERSATIONS ARE THE INTERFACE TO ALL MECHANICS
    /// Every choice touches multiple systems: queue, tokens, routes, NPCs, obligations
    /// </summary>
    public List<ConversationChoice> GenerateChoicesFromQueueState(NPC npc, AttentionManager attention, NPCEmotionalStateCalculator stateCalculator)
    {
        var allChoices = new List<ConversationChoice>();
        var player = _gameWorld.GetPlayer();
        
        // ANALYZE COMPLETE GAME STATE
        var npcLetters = _queueManager.GetActiveLetters()
            .Where(l => l.SenderId == npc.ID || l.SenderName == npc.Name)
            .OrderBy(l => l.DeadlineInHours)
            .ToList();
            
        var mostUrgent = npcLetters.FirstOrDefault();
        var npcState = stateCalculator.CalculateState(npc);
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var trustTokens = tokens.ContainsKey(ConnectionType.Trust) ? tokens[ConnectionType.Trust] : 0;
        var queueFull = _queueManager.GetActiveLetters().Count() >= 8;
        
        // Always include exit option (FREE)
        allChoices.Add(CreateExitChoice(npcState));
        
        // ========== BASE OPTIONS (0 ATTENTION) ==========
        
        // TOKEN EXCHANGE MECHANICS
        if (!queueFull)
        {
            // Build tokens through time investment
            allChoices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = "Let me help you with something quick.",
                AttentionCost = 0,
                BaseVerb = BaseVerb.HELP,
                IsAvailable = true,
                IsAffordable = true,
                MechanicalEffects = new List<IMechanicalEffect> 
                { 
                    new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager)
                }
            });
        }
        
        // Burn tokens for immediate favor
        if (trustTokens >= 2 && mostUrgent != null)
        {
            allChoices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = "You've helped me before. Can you hold this letter for a day?",
                AttentionCost = 0,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAvailable = true,
                IsAffordable = true,
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new BurnTokensEffect(ConnectionType.Trust, 2, npc.ID, _tokenManager),
                    new RemoveLetterTemporarilyEffect(mostUrgent.Id, _queueManager)
                }
            });
        }
        
        // QUEUE MANIPULATION MECHANICS
        if (!queueFull && mostUrgent == null && npc.HasLetterToSend())
        {
            // Offer to take their new letter
            allChoices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = "I can deliver something for you.",
                AttentionCost = 0,
                BaseVerb = BaseVerb.HELP,
                IsAvailable = true,
                IsAffordable = true,
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new AcceptLetterEffect(npc.GenerateLetter(), _queueManager),
                    new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager)
                }
            });
        }
        
        // Request deadline extension
        if (mostUrgent != null && mostUrgent.DeadlineInHours <= 48)
        {
            allChoices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = "I need one more day for your letter. The roads are difficult.",
                AttentionCost = 0,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAvailable = true,
                IsAffordable = true,
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new ExtendDeadlineEffect(mostUrgent.Id, 1, _queueManager),
                    npcState == NPCEmotionalState.DESPERATE 
                        ? new BurnTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager)
                        : new NoEffect()
                }
            });
        }
        
        // INFORMATION TRADE MECHANICS
        if (player.KnownRoutes?.Any() == true && player.KnownRoutes.TryGetValue("default", out var routes) && routes.Any())
        {
            var firstRoute = routes.First();
            allChoices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = $"I know a shortcut through the area.",
                AttentionCost = 0,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAvailable = true,
                IsAffordable = true,
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new ShareInformationEffect(firstRoute, npc),
                    new GainTokensEffect(ConnectionType.Commerce, 1, npc.ID, _tokenManager)
                }
            });
        }
        
        // ========== DEEP OPTIONS (1 ATTENTION) ==========
        
        if (attention.CanAfford(1))
        {
            // OBLIGATION CREATION
            if (npcState == NPCEmotionalState.DESPERATE && !player.HasObligationTo(npc))
            {
                allChoices.Add(new ConversationChoice
                {
                    ChoiceID = Guid.NewGuid().ToString(),
                    NarrativeText = "I'll always prioritize your letters from now on.",
                    AttentionCost = 1,
                    BaseVerb = BaseVerb.NEGOTIATE,
                    IsAvailable = true,
                    IsAffordable = true,
                    MechanicalEffects = new List<IMechanicalEffect>
                    {
                        new CreateObligationEffect($"{npc.Name}_Priority", npc.ID, player),
                        new GainTokensEffect(ConnectionType.Trust, 3, npc.ID, _tokenManager),
                        new UnlockRoutesEffect(npc.KnownRoutes(), player)
                    }
                });
            }
            
            // COMPLEX EXCHANGES - Package deal
            if (mostUrgent != null && trustTokens >= 2)
            {
                allChoices.Add(new ConversationChoice
                {
                    ChoiceID = Guid.NewGuid().ToString(),
                    NarrativeText = "Let's make a trade - I'll prioritize your letter if you introduce me to your contacts.",
                    AttentionCost = 1,
                    BaseVerb = BaseVerb.NEGOTIATE,
                    IsAvailable = true,
                    IsAffordable = true,
                    MechanicalEffects = new List<IMechanicalEffect>
                    {
                        new LetterReorderEffect(mostUrgent.Id, 1, 0, ConnectionType.Trust, _queueManager, _tokenManager, npc.ID),
                        new UnlockNPCEffect(npc.GetContact(), _gameWorld),
                        new BurnTokensEffect(ConnectionType.Trust, 2, npc.ID, _tokenManager)
                    }
                });
            }
            
            // DISCOVERY ACTIONS - Reveal hidden route
            if (npcState == NPCEmotionalState.DESPERATE || trustTokens >= 5)
            {
                allChoices.Add(new ConversationChoice
                {
                    ChoiceID = Guid.NewGuid().ToString(),
                    NarrativeText = "Tell me what you're not saying. I can see you're holding back.",
                    AttentionCost = 1,
                    BaseVerb = BaseVerb.INVESTIGATE,
                    IsAvailable = true,
                    IsAffordable = true,
                    MechanicalEffects = new List<IMechanicalEffect>
                    {
                        new DiscoverRouteEffect(npc.GetSecretRoute(), player)
                    }
                });
            }
            
            // Token type conversion
            if (tokens.ContainsKey(ConnectionType.Trust) && tokens[ConnectionType.Trust] >= 3)
            {
                allChoices.Add(new ConversationChoice
                {
                    ChoiceID = Guid.NewGuid().ToString(),
                    NarrativeText = "Could you introduce me to the merchant guild?",
                    AttentionCost = 1,
                    BaseVerb = BaseVerb.INVESTIGATE,
                    IsAvailable = true,
                    IsAffordable = true,
                    MechanicalEffects = new List<IMechanicalEffect>
                    {
                        new BurnTokensEffect(ConnectionType.Trust, 3, npc.ID, _tokenManager),
                        new GainTokensEffect(ConnectionType.Commerce, 2, npc.ID, _tokenManager),
                        new UnlockLocationEffect("MerchantGuildHall", _gameWorld)
                    }
                });
            }
        }
        
        // ========== CONTEXTUAL FILTERING ==========
        
        // Filter by emotional state
        if (npcState == NPCEmotionalState.HOSTILE)
        {
            // Remove friendly options when hostile
            allChoices.RemoveAll(c => c.NarrativeText.Contains("help") || c.NarrativeText.Contains("friend"));
        }
        
        if (npcState == NPCEmotionalState.WITHDRAWN)
        {
            // Only keep engagement options when withdrawn
            allChoices = allChoices.Where(c => 
                c.BaseVerb == BaseVerb.HELP || 
                c.ChoiceID == "exit").ToList();
        }
        
        // Filter by urgency
        var urgentChoices = allChoices.Where(c => 
            c.MechanicalEffects?.Any(e => e is LetterReorderEffect) == true &&
            mostUrgent?.DeadlineInHours <= 24).ToList();
            
        var highLeverageChoices = allChoices.Where(c =>
            c.MechanicalEffects?.Any(e => e is GainTokensEffect gte && gte.Amount >= 3) == true).ToList();
            
        var discoveryChoices = allChoices.Where(c =>
            c.MechanicalEffects?.Any(e => e is UnlockNPCEffect) == true).ToList();
            
        // Prioritize and limit to 5 choices
        var finalChoices = new List<ConversationChoice>();
        
        // Always include exit
        finalChoices.Add(allChoices.First(c => c.ChoiceID == "exit"));
        
        // Add urgent choices first
        finalChoices.AddRange(urgentChoices.Take(1));
        
        // Add high-leverage choices
        finalChoices.AddRange(highLeverageChoices.Take(1));
        
        // Add discovery choices
        finalChoices.AddRange(discoveryChoices.Take(1));
        
        // Fill remaining slots with other choices
        var remaining = allChoices.Except(finalChoices).Take(5 - finalChoices.Count);
        finalChoices.AddRange(remaining);
        
        return finalChoices.Take(5).ToList();
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
            BaseVerb = BaseVerb.EXIT, // Exit conversation without action
            IsAvailable = true,
            IsAffordable = true
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
        var deadlineText = letter.DeadlineInHours switch
        {
            <= 0 => "It's already too late, but",
            <= 6 => "There's barely time, but",
            <= 24 => "Time is running out, but",
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
                verbs.Add(BaseVerb.HELP);        // Try to appease
                verbs.Add(BaseVerb.NEGOTIATE);   // Redirect their anger
                break;
                
            case NPCEmotionalState.CALCULATING:
                verbs.Add(BaseVerb.NEGOTIATE);   // Trade resources
                verbs.Add(BaseVerb.INVESTIGATE); // Carefully probe for info
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
            // HELP variations
            (BaseVerb.HELP, ConnectionType.Trust, NPCEmotionalState.DESPERATE) =>
                "Take her trembling hand in comfort",
            (BaseVerb.HELP, ConnectionType.Trust, NPCEmotionalState.HOSTILE) =>
                "I understand why you're upset...",
            (BaseVerb.HELP, ConnectionType.Commerce, NPCEmotionalState.DESPERATE) =>
                "Let me see what I can do about the payment",
            (BaseVerb.HELP, ConnectionType.Commerce, NPCEmotionalState.HOSTILE) =>
                "Perhaps we can work out a partial payment",
            (BaseVerb.HELP, ConnectionType.Status, _) =>
                "Of course, your position deserves respect",
            (BaseVerb.HELP, ConnectionType.Shadow, _) =>
                "I mean no threat - see, my hands are empty",

            // INVESTIGATE variations
            (BaseVerb.INVESTIGATE, _, NPCEmotionalState.DESPERATE) when tokenCount >= 3 =>
                "What's really troubling you? You can trust me",
            (BaseVerb.INVESTIGATE, ConnectionType.Trust, _) =>
                "Tell me more about this situation",
            (BaseVerb.INVESTIGATE, ConnectionType.Commerce, _) =>
                "What information might sweeten this deal?",
            (BaseVerb.INVESTIGATE, ConnectionType.Status, _) =>
                "Who else knows about this matter?",
            (BaseVerb.INVESTIGATE, ConnectionType.Shadow, _) =>
                "I need to know what I'm really carrying",

            // NEGOTIATE variations (specific conditions first)
            (BaseVerb.NEGOTIATE, _, NPCEmotionalState.HOSTILE) =>
                "Perhaps you should speak with the postmaster about this",
            (BaseVerb.NEGOTIATE, _, NPCEmotionalState.DESPERATE) =>
                "I swear I'll deliver your letter before any others today",
            (BaseVerb.NEGOTIATE, ConnectionType.Trust, _) when tokenCount >= 5 =>
                "You have my word as a friend",
            (BaseVerb.NEGOTIATE, ConnectionType.Trust, _) =>
                "Have you considered asking Elena instead?",
            (BaseVerb.NEGOTIATE, ConnectionType.Commerce, _) =>
                "The guild sets these rates, not I",
            (BaseVerb.NEGOTIATE, ConnectionType.Status, _) =>
                "Surely someone of your standing has other options",
            (BaseVerb.NEGOTIATE, ConnectionType.Shadow, _) =>
                "I'm just the messenger - take it up with them",

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
            BaseVerb.HELP => 1,
            BaseVerb.NEGOTIATE => 2,  // Negotiations are more demanding
            BaseVerb.INVESTIGATE => 1,
            BaseVerb.EXIT => 0,  // Exit is always free
            _ => 1
        };

        // Apply state modifier based on emotional state
        int modifier = state switch
        {
            NPCEmotionalState.DESPERATE => -1,  // Easier when desperate
            NPCEmotionalState.HOSTILE => 1,     // Harder when hostile
            NPCEmotionalState.WITHDRAWN => 1,   // Harder when withdrawn
            _ => 0
        };
        
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
            BaseVerb.HELP => "Offer your assistance",
            BaseVerb.NEGOTIATE => "Propose a trade or deal",
            BaseVerb.INVESTIGATE => "Probe for more information",
            BaseVerb.EXIT => "Take your leave",
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
            tags.Add($"[TTL:{currentLetter.DeadlineInHours}]");
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