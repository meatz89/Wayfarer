using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Generates conversation choices organized by verb identity.
/// Each verb has distinct mechanical purpose and attention costs.
/// </summary>
public class VerbOrganizedChoiceGenerator
{
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly ITimeManager _timeManager;
    private readonly ConsequenceEngine _consequenceEngine;
    private readonly Player _player;
    private readonly GameWorld _gameWorld;
    
    public VerbOrganizedChoiceGenerator(
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        ITimeManager timeManager,
        ConsequenceEngine consequenceEngine,
        Player player,
        GameWorld gameWorld)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _timeManager = timeManager;
        _consequenceEngine = consequenceEngine;
        _player = player;
        _gameWorld = gameWorld;
    }
    
    /// <summary>
    /// Generate choices based on verb context and NPC state.
    /// Each verb generates distinct types of choices.
    /// </summary>
    public List<ConversationChoice> GenerateChoicesForVerb(
        BaseVerb verb, 
        NPC npc, 
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        
        switch (verb)
        {
            case BaseVerb.HELP:
                choices.AddRange(GenerateHelpChoices(npc, state, relevantLetters));
                break;
                
            case BaseVerb.NEGOTIATE:
                choices.AddRange(GenerateNegotiateChoices(npc, state, relevantLetters));
                break;
                
            case BaseVerb.INVESTIGATE:
                choices.AddRange(GenerateInvestigateChoices(npc, state, relevantLetters));
                break;
                
            case BaseVerb.EXIT:
                choices.Add(CreateExitChoice());
                break;
        }
        
        // Check for delivery opportunity (special case, always available if conditions met)
        var deliveryChoice = CheckForDeliveryOpportunity(npc, relevantLetters);
        if (deliveryChoice != null)
        {
            choices.Insert(0, deliveryChoice); // Delivery always appears first
        }
        
        return choices;
    }
    
    /// <summary>
    /// HELP verb choices: Build long-term relationships through mutual aid.
    /// Always costs 1 attention. Focus on trust-building and obligations.
    /// </summary>
    private List<ConversationChoice> GenerateHelpChoices(
        NPC npc, 
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        
        // 1. Accept new letter (always available as trust-building)
        // Remove HasLetterToOffer check - NPCs can always potentially offer letters
        if (_queueManager.GetActiveLetters().Length < 8)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_accept_letter",
                NarrativeText = "\"Of course I'll help. Give me the letter.\"",
                AttentionCost = 1,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "📜 Accept new letter | ♥ +1 Trust",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new AcceptLetterEffect(npc.GenerateLetterOffer(), _queueManager),
                    new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager),
                    new ConversationTimeEffect(15, _timeManager)
                }
            });
        }
        
        // 2. Create binding obligation for urgent letters
        var urgentLetter = relevantLetters
            .Where(l => l.DeadlineInHours < 6)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();
            
        // Relax condition - any urgent letter creates opportunity for obligation
        if (urgentLetter != null)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_create_obligation",
                NarrativeText = $"\"I swear on my honor to deliver this to {urgentLetter.RecipientName} first.\"",
                AttentionCost = 1,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "⛓ Create obligation | ♥ +2 Trust | Priority for their letters",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new CreateBindingObligationEffect(npc.ID, 
                        $"Sworn to prioritize {npc.Name}'s letters"),
                    new GainTokensEffect(ConnectionType.Trust, 2, npc.ID, _tokenManager),
                    new ConversationTimeEffect(20, _timeManager)
                }
            });
        }
        
        // 3. Share information freely (builds trust)
        // Always available - player can share observations even without formal memories
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_share_info",
                NarrativeText = "\"Let me share what I've learned about the routes here.\"",
                AttentionCost = 1,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "ℹ Share route knowledge | ♥ +1 Trust",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new ShareInformationEffect(
                        new RouteOption { Name = "Market Shortcut", TravelTimeHours = 1 }, 
                        npc),
                    new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager),
                    new ConversationTimeEffect(10, _timeManager)
                }
            });
        }
        
        // 4. Offer assistance (simple trust building)
        choices.Add(new ConversationChoice
        {
            ChoiceID = "help_build_trust",
            NarrativeText = "\"How can I help you today?\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "♥ +1 Trust | Learn their needs",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager),
                new GainInformationEffect($"{npc.Name} needs help with {GetNPCNeed(npc)}", InfoType.Rumor),
                new ConversationTimeEffect(15, _timeManager)
            }
        });
        
        return choices;
    }
    
    /// <summary>
    /// NEGOTIATE verb choices: Immediate pressure relief through resource management.
    /// Costs 1-2 attention based on complexity. Focus on queue manipulation.
    /// </summary>
    private List<ConversationChoice> GenerateNegotiateChoices(
        NPC npc,
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        
        // 1. Reorder letter to better position (PRIMARY NEGOTIATE FUNCTION)
        // Relax from > 3 to >= 2 to provide more opportunities
        var letterNeedingReorder = relevantLetters
            .Where(l => _queueManager.GetLetterPosition(l.Id) >= 2)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();
            
        if (letterNeedingReorder != null)
        {
            var currentPos = _queueManager.GetLetterPosition(letterNeedingReorder.Id) ?? 8;
            var tokenCost = Math.Max(1, (currentPos - 1) / 2); // Cost scales with distance
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_reorder_urgent",
                NarrativeText = $"\"Move your letter to the front - for a price.\"",
                AttentionCost = 1,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Commerce, 0) >= tokenCost,
                IsAvailable = true,
                MechanicalDescription = $"✓ Move to position 1 | 🪙 -{tokenCost} Commerce",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new LetterReorderEffect(letterNeedingReorder.Id, 1, tokenCost, 
                        ConnectionType.Commerce, _queueManager, _tokenManager, npc.ID),
                    new ConversationTimeEffect(10, _timeManager)
                }
            });
        }
        
        // 2. Swap two letter positions (more complex negotiation)
        if (relevantLetters.Count >= 2)
        {
            var letter1 = relevantLetters[0];
            var letter2 = relevantLetters[1];
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_swap_letters",
                NarrativeText = "\"Let's rearrange these priorities - it'll cost you.\"",
                AttentionCost = 2, // More complex = more attention
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Status, 0) >= 1,
                IsAvailable = relevantLetters.Count >= 2,
                MechanicalDescription = "🔄 Swap two letters | 👑 -1 Status",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new SwapLetterPositionsEffect(letter1.Id, letter2.Id, 1,
                        ConnectionType.Status, _queueManager, _tokenManager, npc.ID),
                    new ConversationTimeEffect(15, _timeManager)
                }
            });
        }
        
        // 3. Extend deadline (costs tokens, not free)
        var rushLetter = relevantLetters
            .Where(l => l.DeadlineInHours < 12)
            .FirstOrDefault();
            
        if (rushLetter != null)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_extend_deadline",
                NarrativeText = "\"Give me more time - I'll make it worth your while.\"",
                AttentionCost = 1,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Commerce, 0) >= 2,
                IsAvailable = true,
                MechanicalDescription = "⏱ +24 hours deadline | 🪙 -2 Commerce",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new ExtendDeadlineEffect(rushLetter.Id, 1, _queueManager),
                    new BurnTokensEffect(ConnectionType.Commerce, 2, npc.ID, _tokenManager),
                    new ConversationTimeEffect(10, _timeManager)
                }
            });
        }
        
        // 4. Trade tokens for immediate queue benefit
        if (_tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Trust, 0) >= 3)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_leverage_trust",
                NarrativeText = "\"Remember all I've done for you? I need this favor now.\"",
                AttentionCost = 2,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "♥ -3 Trust → Clear position 1",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new BurnTokensEffect(ConnectionType.Trust, 3, npc.ID, _tokenManager),
                    // Custom effect to clear position 1 for urgent delivery
                    new ConversationTimeEffect(5, _timeManager)
                }
            });
        }
        
        return choices;
    }
    
    /// <summary>
    /// INVESTIGATE verb choices: Strategic information gathering.
    /// Costs 1-3 attention based on depth. Focus on revealing hidden information.
    /// </summary>
    private List<ConversationChoice> GenerateInvestigateChoices(
        NPC npc,
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        
        // 1. Reveal letter properties (1 attention - basic investigation)
        var mysteryLetter = relevantLetters.FirstOrDefault();
        if (mysteryLetter != null)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "investigate_reveal_sender",
                NarrativeText = $"\"Who really sent this letter to {mysteryLetter.RecipientName}?\"",
                AttentionCost = 1,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "🔍 Reveal true sender",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new RevealLetterPropertyEffect(mysteryLetter.Id, "sender", _queueManager, _player),
                    new ConversationTimeEffect(20, _timeManager)
                }
            });
        }
        
        // 2. Predict consequences (2 attention - deeper investigation)
        if (relevantLetters.Any())
        {
            var criticalLetter = relevantLetters
                .OrderBy(l => l.DeadlineInHours)
                .First();
                
            choices.Add(new ConversationChoice
            {
                ChoiceID = "investigate_predict_consequence",
                NarrativeText = "\"What happens if this letter doesn't arrive on time?\"",
                AttentionCost = 2,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "⚠️ Learn failure consequences",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new PredictConsequenceEffect(criticalLetter.Id, _queueManager, _consequenceEngine, _player),
                    new ConversationTimeEffect(30, _timeManager)
                }
            });
        }
        
        // 3. Learn NPC schedule (2 attention - tactical information)
        choices.Add(new ConversationChoice
        {
            ChoiceID = "investigate_learn_schedule",
            NarrativeText = $"\"When can I usually find {npc.Name}? I need to know.\"",
            AttentionCost = 2,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "📅 Learn daily schedule",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new LearnNPCScheduleEffect(npc.ID, _gameWorld, _player),
                new ConversationTimeEffect(25, _timeManager)
            }
        });
        
        // 4. Discover letter network (3 attention - deep investigation)
        if (relevantLetters.Any())
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "investigate_letter_network",
                NarrativeText = "\"How are all these letters connected? There's a pattern here...\"",
                AttentionCost = 3,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "🕸️ Discover letter connections",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new DiscoverLetterNetworkEffect(relevantLetters.First().Id, _queueManager, _player),
                    new ConversationTimeEffect(40, _timeManager)
                }
            });
        }
        
        // 5. Discover hidden routes (3 attention - strategic knowledge)
        if (state == NPCEmotionalState.CALCULATING)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "investigate_secret_route",
                NarrativeText = "\"There must be faster ways through the city. Tell me what you know.\"",
                AttentionCost = 3,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "🗺️ Discover secret route",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new DiscoverRouteEffect(
                        new RouteOption 
                        { 
                            Name = "Underground Passage", 
                            TravelTimeHours = 1,
                            Description = "A hidden route that saves significant time"
                        }, 
                        _player),
                    new ConversationTimeEffect(45, _timeManager)
                }
            });
        }
        
        return choices;
    }
    
    /// <summary>
    /// Check if player can deliver a letter to this NPC.
    /// Delivery is special - it's always available if conditions are met.
    /// </summary>
    private ConversationChoice CheckForDeliveryOpportunity(NPC npc, List<Letter> relevantLetters)
    {
        // Check if ANY letter in position 1 is for this NPC
        var letterInPosition1 = relevantLetters
            .FirstOrDefault(l => 
            {
                var pos = _queueManager.GetLetterPosition(l.Id);
                return pos.HasValue && pos.Value == 1 && l.RecipientName == npc.Name;
            });
            
        if (letterInPosition1 != null)
        {
            return new ConversationChoice
            {
                ChoiceID = "deliver_letter",
                NarrativeText = $"\"I have your letter from {letterInPosition1.SenderName}. Here it is.\"",
                AttentionCost = 0, // Delivery doesn't cost attention
                BaseVerb = BaseVerb.EXIT,
                IsAffordable = true,
                IsAvailable = true,
                Priority = 100, // Always show first
                MechanicalDescription = $"📜 Deliver letter | +{letterInPosition1.Payment} coins",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new DeliverLetterEffect(letterInPosition1.Id, _queueManager, _timeManager)
                }
            };
        }
        
        return null;
    }
    
    /// <summary>
    /// Exit choice is always available.
    /// </summary>
    private ConversationChoice CreateExitChoice()
    {
        return new ConversationChoice
        {
            ChoiceID = "base_exit",
            NarrativeText = "\"I should go. Time is pressing.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.EXIT,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "→ Leave conversation",
            MechanicalEffects = new List<IMechanicalEffect> 
            { 
                new EndConversationEffect() 
            }
        };
    }
    
    private string GetNPCNeed(NPC npc)
    {
        // Generate contextual need based on NPC type
        return npc.Profession switch
        {
            Professions.Merchant => "trade route information",
            Professions.Scholar => "medical supplies delivery",
            Professions.Craftsman => "commercial contacts",
            Professions.Noble => "urgent messages",
            _ => "reliable delivery"
        };
    }
}