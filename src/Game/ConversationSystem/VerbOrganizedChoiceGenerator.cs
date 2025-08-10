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
                // Add 1-2 free flavor choices that maintain state
                choices.AddRange(GenerateFreeFlavorChoices(npc, state));
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
    /// HELP verb choices: Accept burdens to build trust.
    /// Core tension: Taking letters fills queue but builds relationships.
    /// </summary>
    private List<ConversationChoice> GenerateHelpChoices(
        NPC npc, 
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        var currentTrust = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Trust, 0);
        var queueSlots = _queueManager.GetActiveLetters().Length;
        var hasSpace = queueSlots < 8;
        
        // 1 ATTENTION: Accept letter + build trust (core HELP mechanic)
        if (hasSpace && npc.HasLetterToOffer)
        {
            var offeredLetter = npc.GenerateLetterOffer();
            string acceptText = GenerateAcceptLetterNarrativeText(npc, state, offeredLetter);
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_accept_letter",
                NarrativeText = acceptText,
                AttentionCost = 1,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "Accept letter | +1 Trust",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new AcceptLetterEffect(offeredLetter, _queueManager),
                    new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager)
                }
            });
        }
        
        // 1 ATTENTION: Pure trust building (when no letters or queue full)
        string helpText = GenerateHelpNarrativeText(npc, state, 1);
        choices.Add(new ConversationChoice
        {
            ChoiceID = "help_build_trust",
            NarrativeText = helpText,
            AttentionCost = 1,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "+2 Trust",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new GainTokensEffect(ConnectionType.Trust, 2, npc.ID, _tokenManager)
            }
        });
        
        // 2 ATTENTION: Accept difficult letter for more trust
        if (hasSpace && npc.HasUrgentLetter())
        {
            var urgentLetter = npc.GenerateUrgentLetter();
            string urgentText = GenerateAcceptUrgentNarrativeText(npc, state, urgentLetter);
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_accept_urgent",
                NarrativeText = urgentText,
                AttentionCost = 2,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "Accept urgent letter | +3 Trust",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new AcceptLetterEffect(urgentLetter, _queueManager),
                    new GainTokensEffect(ConnectionType.Trust, 3, npc.ID, _tokenManager)
                }
            });
        }
        else
        {
            // 2 ATTENTION: Deeper commitment - more trust (fallback)
            string commitText = GenerateHelpNarrativeText(npc, state, 2);
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_deeper_commitment",
                NarrativeText = commitText,
                AttentionCost = 2,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "+4 Trust",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new GainTokensEffect(ConnectionType.Trust, 4, npc.ID, _tokenManager)
                }
            });
        }
        
        // 3 ATTENTION: Locked deep bond - requires 5 trust
        var isLocked = currentTrust < 5;
        string bondText = isLocked 
            ? "\"I wish I could do more, but...\"" 
            : GenerateHelpNarrativeText(npc, state, 3);
        choices.Add(new ConversationChoice
        {
            ChoiceID = "help_deep_bond",
            NarrativeText = bondText,
            AttentionCost = 3,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = !isLocked,
            IsAvailable = !isLocked,
            MechanicalDescription = isLocked ? "[LOCKED] Requires 5 Trust" : "+6 Trust",
            MechanicalEffects = isLocked
                ? new List<IMechanicalEffect> { new LockedEffect("Requires 5 Trust") }
                : new List<IMechanicalEffect> { new GainTokensEffect(ConnectionType.Trust, 6, npc.ID, _tokenManager) }
        });
        
        return choices;
    }
    
    /// <summary>
    /// NEGOTIATE verb choices: Queue manipulation and token trading.
    /// Pure queue management - no letter acceptance (that's HELP's job).
    /// </summary>
    private List<ConversationChoice> GenerateNegotiateChoices(
        NPC npc,
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        var commerceTokens = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Commerce, 0);
        var statusTokens = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Status, 0);
        
        // 1 ATTENTION OPTION A: Simple swap with clear cost
        if (relevantLetters.Count >= 2 && commerceTokens >= 2)
        {
            var letter1 = relevantLetters[0];
            var letter2 = relevantLetters[1];
            var pos1 = _queueManager.GetLetterPosition(letter1.Id) ?? 8;
            var pos2 = _queueManager.GetLetterPosition(letter2.Id) ?? 8;
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_swap",
                NarrativeText = GenerateNegotiateNarrativeText(npc, state, "swap", pos1, pos2),
                AttentionCost = 1,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = commerceTokens >= 2,
                IsAvailable = true,
                MechanicalDescription = "Swap positions | -2 Commerce",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new SwapLetterPositionsEffect(letter1.Id, letter2.Id, 2,
                        ConnectionType.Commerce, _queueManager, _tokenManager, npc.ID)
                }
            });
        }
        
        // 1 ATTENTION OPTION B: Open interface (no cost)
        choices.Add(new ConversationChoice
        {
            ChoiceID = "negotiate_interface",
            NarrativeText = GenerateNegotiateNarrativeText(npc, state, "interface"),
            AttentionCost = 1,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Open queue interface",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new OpenQueueInterfaceEffect()
            }
        });
        
        // 1 ATTENTION OPTION C: Trade tokens (Commerce for Status)
        if (commerceTokens >= 3)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_token_trade",
                NarrativeText = GenerateNegotiateNarrativeText(npc, state, "trade"),
                AttentionCost = 1,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "Trade 3 Commerce → 2 Status",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new BurnTokensEffect(ConnectionType.Commerce, 3, npc.ID, _tokenManager),
                    new GainTokensEffect(ConnectionType.Status, 2, npc.ID, _tokenManager)
                }
            });
        }
        
        // 2 ATTENTION: Move urgent letter to front - expensive but powerful
        var urgentLetter = relevantLetters
            .Where(l => l.DeadlineInHours < 12)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();
            
        if (urgentLetter != null)
        {
            var currentPos = _queueManager.GetLetterPosition(urgentLetter.Id) ?? 8;
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_priority",
                NarrativeText = GenerateNegotiateNarrativeText(npc, state, "priority"),
                AttentionCost = 2,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = statusTokens >= 3,
                IsAvailable = true,
                MechanicalDescription = "Move to position 1 | -3 Status",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new LetterReorderEffect(urgentLetter.Id, 1, 3, 
                        ConnectionType.Status, _queueManager, _tokenManager, npc.ID)
                }
            });
        }
        
        // 3 ATTENTION: Trade tokens for status (locked if insufficient commerce)
        var isLocked = commerceTokens < 5;
        choices.Add(new ConversationChoice
        {
            ChoiceID = "negotiate_trade_tokens",
            NarrativeText = isLocked 
                ? "\"I don't have enough to trade...\"" 
                : GenerateNegotiateNarrativeText(npc, state, "trade"),
            AttentionCost = 3,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = !isLocked,
            IsAvailable = !isLocked,
            MechanicalDescription = isLocked ? "[LOCKED] Requires 5 Commerce" : "Trade tokens | -5 Commerce +3 Status",
            MechanicalEffects = isLocked
                ? new List<IMechanicalEffect> { new LockedEffect("Requires 5 Commerce tokens") }
                : new List<IMechanicalEffect> 
                { 
                    new BurnTokensEffect(ConnectionType.Commerce, 5, npc.ID, _tokenManager),
                    new GainTokensEffect(ConnectionType.Status, 3, npc.ID, _tokenManager)
                }
        });
        
        return choices;
    }
    
    /// <summary>
    /// INVESTIGATE verb choices: Information discovery at time cost.
    /// Mechanical rule: INVESTIGATE only reveals info and costs time, no token changes.
    /// </summary>
    private List<ConversationChoice> GenerateInvestigateChoices(
        NPC npc,
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        var trustTokens = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Trust, 0);
        
        // 1 ATTENTION: Learn schedule with time cost
        choices.Add(new ConversationChoice
        {
            ChoiceID = "investigate_schedule",
            NarrativeText = GenerateInvestigateNarrativeText(npc, state, "schedule"),
            AttentionCost = 1,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Learn schedule | 30 min",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new LearnNPCScheduleEffect(npc.ID, _gameWorld, _player),
                new ConversationTimeEffect(30, _timeManager)
            }
        });
        
        // 1 ATTENTION: Reveal letter property with time cost
        if (relevantLetters.Any())
        {
            var letter = relevantLetters.First();
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "investigate_letter",
                NarrativeText = GenerateInvestigateNarrativeText(npc, state, "letter"),
                AttentionCost = 1,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "Reveal contents | 20 min",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new RevealLetterPropertyEffect(letter.Id, "stakes", _queueManager, _player),
                    new ConversationTimeEffect(20, _timeManager)
                }
            });
        }
        
        // 2 ATTENTION: Deep investigation with major time cost
        choices.Add(new ConversationChoice
        {
            ChoiceID = "investigate_deep",
            NarrativeText = GenerateInvestigateNarrativeText(npc, state, "deep"),
            AttentionCost = 2,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Deep investigation | 45 min",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new DeepInvestigationEffect($"{npc.Name}'s connections and obligations"),
                new ConversationTimeEffect(45, _timeManager)
            }
        });
        
        // 3 ATTENTION: Locked - reveal network/conspiracy
        var isLocked = trustTokens < 3;
        choices.Add(new ConversationChoice
        {
            ChoiceID = "investigate_network",
            NarrativeText = "\"Tell me who's really pulling the strings.\"",
            AttentionCost = 3,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = !isLocked,
            IsAvailable = !isLocked,
            MechanicalDescription = isLocked ? "[LOCKED] Requires 3 Trust" : "Reveal network | 60 min",
            MechanicalEffects = isLocked
                ? new List<IMechanicalEffect> { new LockedEffect("Requires 3 Trust") }
                : new List<IMechanicalEffect> 
                { 
                    new DiscoverLetterNetworkEffect(
                        relevantLetters.FirstOrDefault()?.Id ?? "none",
                        _queueManager, _player),
                    new ConversationTimeEffect(60, _timeManager)
                }
        });
        
        return choices;
    }
    
    /// <summary>
    /// Check if player can deliver a letter to this NPC.
    /// Delivery is special - it's always available if conditions are met.
    /// Mockup-aligned: Delivery is a free action with a single clear effect.
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
                NarrativeText = $"\"I have your letter from {letterInPosition1.SenderName}.\"",
                AttentionCost = 0, // Delivery is free
                BaseVerb = BaseVerb.EXIT,
                IsAffordable = true,
                IsAvailable = true,
                Priority = 100, // Always show first
                MechanicalDescription = $"Deliver letter | +{letterInPosition1.Payment} coins",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new DeliverLetterEffect(letterInPosition1.Id, _queueManager, _timeManager)
                }
            };
        }
        
        return null;
    }
    
    /// <summary>
    /// Exit choice is always available and does NOTHING except end conversation.
    /// Mockup-aligned: Free choice = maintain state, no other effects.
    /// </summary>
    private ConversationChoice CreateExitChoice()
    {
        return new ConversationChoice
        {
            ChoiceID = "base_exit",
            NarrativeText = "\"I should go.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.EXIT,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "→ Maintains current state",
            MechanicalEffects = new List<IMechanicalEffect> 
            { 
                new EndConversationEffect() 
            }
        };
    }
    
    /// <summary>
    /// Generate narrative text for HELP choices that feels specific but stays mechanical.
    /// </summary>
    private string GenerateHelpNarrativeText(NPC npc, NPCEmotionalState state, int attentionLevel)
    {
        // Base it on emotional state for variety, but keep focused on trust-building
        return (state, attentionLevel) switch
        {
            (NPCEmotionalState.DESPERATE, 1) => "\"I understand your urgency. Let me see what I can do.\"",
            (NPCEmotionalState.DESPERATE, 2) => "\"This is serious. I want to help you through this.\"",
            (NPCEmotionalState.DESPERATE, 3) => "\"We'll get through this crisis together.\"",
            
            (NPCEmotionalState.HOSTILE, 1) => "\"I know things have been difficult. Let me make it right.\"",
            (NPCEmotionalState.HOSTILE, 2) => "\"I want to repair what's been damaged between us.\"",
            (NPCEmotionalState.HOSTILE, 3) => "\"Let's rebuild our trust, step by step.\"",
            
            (NPCEmotionalState.CALCULATING, 1) => "\"I'd like to build something meaningful with you.\"",
            (NPCEmotionalState.CALCULATING, 2) => "\"Our connection matters to me.\"",
            (NPCEmotionalState.CALCULATING, 3) => "\"Let's deepen our understanding.\"",
            
            (NPCEmotionalState.WITHDRAWN, 1) => "\"I'd like to help, if you'll let me.\"",
            (NPCEmotionalState.WITHDRAWN, 2) => "\"Let me show you I'm reliable.\"",
            (NPCEmotionalState.WITHDRAWN, 3) => "\"I want to earn your trust.\"",
            
            _ => "\"I want to help.\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for accepting a letter (HELP verb).
    /// </summary>
    private string GenerateAcceptLetterNarrativeText(NPC npc, NPCEmotionalState state, Letter letter)
    {
        return state switch
        {
            NPCEmotionalState.DESPERATE => $"\"Of course I'll deliver this. {letter.RecipientName} will have it soon.\"",
            NPCEmotionalState.HOSTILE => "\"Despite everything, I'll take your letter.\"",
            NPCEmotionalState.CALCULATING => "\"I can handle this delivery for you.\"",
            NPCEmotionalState.WITHDRAWN => "\"Leave it with me. I'll see it delivered.\"",
            _ => $"\"I'll deliver your letter to {letter.RecipientName}.\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for accepting an urgent letter (HELP verb, higher attention).
    /// </summary>
    private string GenerateAcceptUrgentNarrativeText(NPC npc, NPCEmotionalState state, Letter letter)
    {
        var hoursText = letter.DeadlineInHours <= 4 ? "immediately" : "as quickly as possible";
        return state switch
        {
            NPCEmotionalState.DESPERATE => $"\"This is urgent? I'll handle it {hoursText}, I promise.\"",
            NPCEmotionalState.HOSTILE => $"\"Even with our troubles, I won't let this fail. It'll be delivered {hoursText}.\"",
            NPCEmotionalState.CALCULATING => $"\"An urgent matter deserves priority. I'll ensure it reaches {letter.RecipientName}.\"",
            NPCEmotionalState.WITHDRAWN => $"\"Urgent... I understand. Consider it done.\"",
            _ => $"\"I'll prioritize this urgent delivery to {letter.RecipientName}.\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for NEGOTIATE choices focused on queue/priorities.
    /// </summary>
    private string GenerateNegotiateNarrativeText(NPC npc, NPCEmotionalState state, string negotiationType, params int[] positions)
    {
        return negotiationType switch
        {
            "swap" when positions.Length >= 2 => 
                $"\"Could we rearrange positions {positions[0]} and {positions[1]}?\"",
            "interface" => 
                state == NPCEmotionalState.DESPERATE 
                    ? "\"Let me reorganize these urgent matters.\"" 
                    : "\"Let's review my delivery priorities.\"",
            "priority" => 
                state == NPCEmotionalState.DESPERATE
                    ? "\"This can't wait - I need to move it forward.\""
                    : "\"I need to prioritize this delivery.\"",
            "trade" =>
                state == NPCEmotionalState.CALCULATING
                    ? "\"I propose a token exchange - mutually beneficial.\""
                    : "\"Let's trade - my merchant connections for your noble influence.\"",
            _ => "\"Let's adjust my priorities.\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for INVESTIGATE choices focused on understanding.
    /// </summary>
    private string GenerateInvestigateNarrativeText(NPC npc, NPCEmotionalState state, string investigationType)
    {
        return investigationType switch
        {
            "schedule" => 
                state == NPCEmotionalState.DESPERATE
                    ? "\"I need to know where to find help tomorrow.\""
                    : "\"Tell me about everyone's schedules.\"",
            "letter" => 
                "\"I need to understand what's really at stake here.\"",
            "deep" => 
                state == NPCEmotionalState.DESPERATE
                    ? "\"There's more to this crisis. I need the full picture.\""
                    : "\"Help me understand the deeper connections.\"",
            "network" => 
                "\"Who's really behind all of this?\"",
            _ => "\"I need to understand this situation.\""
        };
    }
    
    /// <summary>
    /// Validates that effect bundles follow clean design principles.
    /// Each attention level has a maximum number of allowed effects.
    /// </summary>
    private bool ValidateEffectBundle(List<IMechanicalEffect> effects, int attentionCost)
    {
        // 0 attention = 1 effect (exit/delivery only)
        // 1 attention = 1-2 effects (simple action, maybe with time)
        // 2 attention = 2-3 effects (complex action with trade-off)
        // 3 attention = 2-3 effects (major action, locked behind requirements)
        
        int maxEffects = attentionCost switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            3 => 3,
            _ => 1
        };
        
        return effects.Count <= maxEffects;
    }
    
    /// <summary>
    /// Builds clean mechanical descriptions without excessive icons or redundant info.
    /// </summary>
    private string BuildMechanicalDescription(List<IMechanicalEffect> effects, BaseVerb verb)
    {
        var parts = new List<string>();
        
        foreach (var effect in effects)
        {
            var descriptions = effect.GetDescriptionsForPlayer();
            if (descriptions != null && descriptions.Count > 0)
            {
                var desc = descriptions[0];
                
                // Simplify descriptions based on effect type
                string part = desc.Category switch
                {
                    EffectCategory.TokenGain when desc.TokenAmount > 0 => 
                        $"+{desc.TokenAmount} {desc.TokenType}",
                    EffectCategory.TokenSpend when desc.TokenAmount > 0 => 
                        $"-{desc.TokenAmount} {desc.TokenType}",
                    EffectCategory.TimePassage when desc.TimeMinutes > 0 => 
                        $"{desc.TimeMinutes} min",
                    EffectCategory.LetterAdd => 
                        "Accept letter",
                    EffectCategory.ObligationCreate => 
                        "Creates obligation",
                    EffectCategory.LetterSwap => 
                        "Swap positions",
                    EffectCategory.LetterReorder => 
                        $"Move to position {desc.LetterPosition}",
                    EffectCategory.InterfaceAction => 
                        "Open queue interface",
                    EffectCategory.InformationGain => 
                        desc.Text,
                    EffectCategory.StateChange when desc.Text.Contains("Maintain") => 
                        "→ Maintains current state",
                    _ => desc.Text
                };
                
                if (!string.IsNullOrEmpty(part) && !parts.Contains(part))
                {
                    parts.Add(part);
                }
            }
        }
        
        return parts.Count > 0 ? string.Join(" | ", parts) : "No effect";
    }
    
    /// <summary>
    /// Generate 1-2 free choices that add personality without mechanical changes.
    /// These maintain state but offer emotional variety.
    /// </summary>
    private List<ConversationChoice> GenerateFreeFlavorChoices(NPC npc, NPCEmotionalState state)
    {
        var choices = new List<ConversationChoice>();
        
        // Add profession-specific flavor choice
        string professionChoice = npc.Profession switch
        {
            Professions.Merchant => "\"How's business treating you?\"",
            Professions.Scholar => "\"Any interesting discoveries lately?\"",
            Professions.Craftsman => "\"What are you working on these days?\"",
            Professions.Noble => "\"How are things at court?\"",
            _ => "\"How have you been?\""
        };
        
        choices.Add(new ConversationChoice
        {
            ChoiceID = "free_profession_flavor",
            NarrativeText = professionChoice,
            AttentionCost = 0,
            BaseVerb = BaseVerb.EXIT,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "→ Maintains current state",
            MechanicalEffects = new List<IMechanicalEffect> { new MaintainStateEffect() }
        });
        
        // Add emotional state-specific choice
        if (state == NPCEmotionalState.DESPERATE || state == NPCEmotionalState.HOSTILE)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "free_empathy",
                NarrativeText = state == NPCEmotionalState.DESPERATE 
                    ? "\"Is there anything else on your mind?\""
                    : "\"I understand you're frustrated with me.\"",
                AttentionCost = 0,
                BaseVerb = BaseVerb.EXIT,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "→ Maintains current state",
                MechanicalEffects = new List<IMechanicalEffect> { new MaintainStateEffect() }
            });
        }
        
        return choices;
    }
}