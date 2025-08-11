using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;
using Wayfarer.Models;

/// <summary>
/// Generates conversation choices organized by verb identity.
/// Each verb has distinct mechanical purpose and attention costs.
/// </summary>
public class VerbOrganizedChoiceGenerator
{
    private readonly LetterQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly ITimeManager _timeManager;
    private readonly ConsequenceEngine _consequenceEngine;
    private readonly Player _player;
    private readonly GameWorld _gameWorld;
    private readonly Wayfarer.GameState.TimeBlockAttentionManager _timeBlockAttentionManager;
    
    public VerbOrganizedChoiceGenerator(
        LetterQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        ITimeManager timeManager,
        ConsequenceEngine consequenceEngine,
        Player player,
        GameWorld gameWorld,
        Wayfarer.GameState.TimeBlockAttentionManager timeBlockAttentionManager)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _timeManager = timeManager;
        _consequenceEngine = consequenceEngine;
        _player = player;
        _gameWorld = gameWorld;
        _timeBlockAttentionManager = timeBlockAttentionManager;
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
    /// Emotional states strongly affect what help is offered/accepted.
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
        var playerTier = _player.CurrentTier;
        
        // Emotional states affect letter offers
        // DESPERATE: Offers urgent, high-stakes letters
        // ANXIOUS: Offers time-sensitive letters  
        // CALCULATING: Offers profitable letters
        // HOSTILE: Refuses to offer letters
        // WITHDRAWN: No letters available
        
        // 1 ATTENTION: Accept letter - but what's offered depends on state!
        if (hasSpace && npc.HasLetterToOffer && state != NPCEmotionalState.HOSTILE)
        {
            var offeredLetter = npc.GenerateLetterOffer();
            
            // Modify letter urgency based on emotional state
            if (state == NPCEmotionalState.DESPERATE)
            {
                offeredLetter.DeadlineInHours = Math.Min(offeredLetter.DeadlineInHours, 6);
                offeredLetter.Payment = (int)(offeredLetter.Payment * 1.5); // Desperate pays more
            }
            else if (state == NPCEmotionalState.ANXIOUS)
            {
                offeredLetter.DeadlineInHours = Math.Min(offeredLetter.DeadlineInHours, 12);
            }
            
            string acceptText = GenerateAcceptLetterNarrativeText(npc, state, offeredLetter);
            bool isAvailable = playerTier >= TierLevel.T1;
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_accept_letter",
                NarrativeText = acceptText,
                AttentionCost = state == NPCEmotionalState.DESPERATE ? 0 : 1, // Desperate makes it free!
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = isAvailable,
                MechanicalDescription = state == NPCEmotionalState.DESPERATE 
                    ? $"URGENT: {offeredLetter.SenderName}'s letter ({offeredLetter.DeadlineInHours}h!) +${offeredLetter.Payment}"
                    : $"Accept {offeredLetter.SenderName}'s letter (trust on delivery)",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new AcceptLetterEffect(offeredLetter, _queueManager)
                }
            });
        }
        
        // 1 ATTENTION: Pure trust building - affected by emotional state
        // HOSTILE blocks this entirely
        if (state != NPCEmotionalState.HOSTILE)
        {
            string helpText = GenerateHelpNarrativeText(npc, state, 1);
            bool trustBuildAvailable = playerTier >= TierLevel.T1;
            
            // Emotional state affects trust gain amount
            int trustGain = state switch
            {
                NPCEmotionalState.DESPERATE => 3,  // Desperate NPCs bond quickly
                NPCEmotionalState.ANXIOUS => 2,    // Normal trust gain
                NPCEmotionalState.CALCULATING => 1, // Calculating NPCs are cautious
                NPCEmotionalState.WITHDRAWN => 1,  // Withdrawn NPCs barely engage
                _ => 2
            };
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_build_trust",
                NarrativeText = helpText,
                AttentionCost = 1,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = true,
                IsAvailable = trustBuildAvailable,
                MechanicalDescription = $"+{trustGain} Trust with {npc.Name}",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new GainTokensEffect(ConnectionType.Trust, trustGain, npc.ID, _tokenManager)
                }
            });
        }
        else
        {
            // HOSTILE: Trust building is blocked
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_build_trust_blocked",
                NarrativeText = GenerateHelpNarrativeText(npc, state, 1),
                AttentionCost = 999, // Unaffordable
                BaseVerb = BaseVerb.HELP,
                IsAffordable = false,
                IsAvailable = false,
                MechanicalDescription = $"{npc.Name} refuses your help (HOSTILE)",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new LockedEffect("Trust blocked while HOSTILE")
                }
            });
        }
        
        // 2 ATTENTION: Accept difficult letter - emotional state affects what's offered
        if (hasSpace && npc.HasUrgentLetter() && state != NPCEmotionalState.WITHDRAWN)
        {
            var urgentLetter = npc.GenerateUrgentLetter();
            
            // DESPERATE NPCs offer impossible deadlines
            if (state == NPCEmotionalState.DESPERATE)
            {
                urgentLetter.DeadlineInHours = Math.Min(4, urgentLetter.DeadlineInHours);
                urgentLetter.Payment = (int)(urgentLetter.Payment * 2); // Double payment for impossible task
            }
            else if (state == NPCEmotionalState.HOSTILE)
            {
                urgentLetter.Payment = (int)(urgentLetter.Payment * 0.5); // Hostile pays less
            }
            
            bool canAcceptUrgent = playerTier >= TierLevel.T2 || state == NPCEmotionalState.DESPERATE;
            string urgentText = GenerateAcceptUrgentNarrativeText(npc, state, urgentLetter);
            
            // DESPERATE makes urgent letters cheaper to accept
            int urgentCost = state == NPCEmotionalState.DESPERATE ? 1 : 2;
                
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_accept_urgent",
                NarrativeText = urgentText,
                AttentionCost = urgentCost,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = canAcceptUrgent,
                IsAvailable = canAcceptUrgent,
                MechanicalDescription = state == NPCEmotionalState.DESPERATE
                    ? $"CRISIS: {urgentLetter.DeadlineInHours}h deadline! +${urgentLetter.Payment}!"
                    : $"Urgent letter ({urgentLetter.DeadlineInHours}h) +${urgentLetter.Payment}",
                MechanicalEffects = canAcceptUrgent
                    ? new List<IMechanicalEffect> { new AcceptLetterEffect(urgentLetter, _queueManager) }
                    : new List<IMechanicalEffect> { new LockedEffect("Requires Associate standing") }
            });
        }
        else
        {
            // 2 ATTENTION: Deeper commitment - more trust (fallback)
            // Deep commitment requires T2 (Associate)
            bool canDeepCommit = playerTier >= TierLevel.T2;
            string commitText = canDeepCommit
                ? GenerateHelpNarrativeText(npc, state, 2)
                : "\"I'm not yet established enough for such commitments...\"";
                
            choices.Add(new ConversationChoice
            {
                ChoiceID = "help_deeper_commitment",
                NarrativeText = commitText,
                AttentionCost = 2,
                BaseVerb = BaseVerb.HELP,
                IsAffordable = canDeepCommit,
                IsAvailable = canDeepCommit,
                MechanicalDescription = canDeepCommit
                    ? $"+4 Trust with {npc.Name} immediately"
                    : "Requires Associate standing for deeper commitments",
                MechanicalEffects = canDeepCommit
                    ? new List<IMechanicalEffect> { new GainTokensEffect(ConnectionType.Trust, 4, npc.ID, _tokenManager) }
                    : new List<IMechanicalEffect> { new LockedEffect("Requires Associate standing") }
            });
        }
        
        // 3 ATTENTION: Locked deep bond - requires T3 (Confidant) AND 5 trust
        // Deep bonds are only possible between confidants who already trust each other
        var needsTier = playerTier < TierLevel.T3;
        var needsTrust = currentTrust < 5;
        var isLocked = needsTier || needsTrust;
        
        string bondText = isLocked 
            ? "\"I wish I could do more, but...\"" 
            : GenerateHelpNarrativeText(npc, state, 3);
            
        string lockReason = needsTier 
            ? "Requires Confidant standing"
            : needsTrust 
                ? $"Requires 5 Trust with {npc.Name}"
                : "";
                
        choices.Add(new ConversationChoice
        {
            ChoiceID = "help_deep_bond",
            NarrativeText = bondText,
            AttentionCost = 3,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = !isLocked,
            IsAvailable = !isLocked,
            MechanicalDescription = isLocked 
                ? lockReason 
                : $"+6 Trust with {npc.Name} (creates deep bond)",
            MechanicalEffects = isLocked
                ? new List<IMechanicalEffect> { new LockedEffect(lockReason) }
                : new List<IMechanicalEffect> { new GainTokensEffect(ConnectionType.Trust, 6, npc.ID, _tokenManager) }
        });
        
        return choices;
    }
    
    /// <summary>
    /// NEGOTIATE verb choices: Queue manipulation and token trading.
    /// Emotional states affect negotiation costs and options.
    /// </summary>
    private List<ConversationChoice> GenerateNegotiateChoices(
        NPC npc,
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        var commerceTokens = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Commerce, 0);
        var statusTokens = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Status, 0);
        var trustTokens = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Trust, 0);
        var playerTier = _player.CurrentTier;
        
        // CRITICAL: Only negotiate letters that are ACTUALLY IN THE QUEUE
        var lettersInQueue = relevantLetters.Where(l => 
            _queueManager.GetLetterPosition(l.Id).HasValue).ToList();
        
        // Emotional states affect negotiation costs
        // DESPERATE: Accepts bad deals (cheaper for player)
        // CALCULATING: Demands fair trades (normal cost)
        // HOSTILE: Demands more (expensive)
        
        // 1 ATTENTION OPTION A: Simple swap - cost varies by emotional state
        if (lettersInQueue.Count >= 2)
        {
            var letter1 = lettersInQueue[0];
            var letter2 = lettersInQueue[1];
            var pos1 = _queueManager.GetLetterPosition(letter1.Id) ?? 8;
            var pos2 = _queueManager.GetLetterPosition(letter2.Id) ?? 8;
            bool canSwap = playerTier >= TierLevel.T1;
            
            // Emotional state affects swap cost
            int swapCost = state switch
            {
                NPCEmotionalState.DESPERATE => 1,   // Desperate accepts any deal
                NPCEmotionalState.ANXIOUS => 2,     // Normal cost
                NPCEmotionalState.CALCULATING => 2, // Fair trade
                NPCEmotionalState.HOSTILE => 4,     // Demands more
                NPCEmotionalState.WITHDRAWN => 2,   // Indifferent
                _ => 2
            };
            
            bool canAfford = commerceTokens >= swapCost;
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_swap",
                NarrativeText = GenerateNegotiateNarrativeText(npc, state, "swap", pos1, pos2),
                AttentionCost = 1,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = canAfford && canSwap,
                IsAvailable = canSwap,
                MechanicalDescription = state == NPCEmotionalState.DESPERATE
                    ? $"DEAL: Swap {pos1} ↔ {pos2} (only {swapCost} Commerce!)"
                    : $"Swap slots {pos1} ↔ {pos2} (burn {swapCost} Commerce)",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new SwapLetterPositionsEffect(letter1.Id, letter2.Id, swapCost,
                        ConnectionType.Commerce, _queueManager, _tokenManager, npc.ID)
                }
            });
        }
        
        // 1 ATTENTION OPTION B: Open interface (no cost)
        // Queue interface is always T1 (basic functionality)
        choices.Add(new ConversationChoice
        {
            ChoiceID = "negotiate_interface",
            NarrativeText = GenerateNegotiateNarrativeText(npc, state, "interface"),
            AttentionCost = 1,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = true,
            IsAvailable = playerTier >= TierLevel.T1, // Always available
            MechanicalDescription = "Open queue interface",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new OpenQueueInterfaceEffect()
            }
        });
        
        // 1 ATTENTION OPTION C: REFUSE a letter - emotional state affects cost
        var letterFromThisNPC = lettersInQueue.FirstOrDefault(l => l.SenderId == npc.ID || l.SenderName == npc.Name);
        if (letterFromThisNPC != null)
        {
            // Emotional state affects refuse cost
            int refuseCost = state switch
            {
                NPCEmotionalState.DESPERATE => 5,    // Devastating to refuse desperate NPC
                NPCEmotionalState.ANXIOUS => 3,      // Normal damage
                NPCEmotionalState.CALCULATING => 2,  // They understand business
                NPCEmotionalState.HOSTILE => 1,      // Already damaged relationship
                NPCEmotionalState.WITHDRAWN => 1,    // They don't care much
                _ => 3
            };
            
            bool canRefuse = playerTier >= TierLevel.T2 && trustTokens >= refuseCost;
            string refuseText = GenerateRefuseLetterNarrativeText(npc, state, letterFromThisNPC);
                
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_refuse_letter",
                NarrativeText = refuseText,
                AttentionCost = 1,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = canRefuse,
                IsAvailable = canRefuse,
                MechanicalDescription = state == NPCEmotionalState.DESPERATE
                    ? $"BETRAY {letterFromThisNPC.SenderName} | Burns {refuseCost} Trust!"
                    : $"Refuse {letterFromThisNPC.SenderName}'s letter | -{refuseCost} Trust",
                MechanicalEffects = canRefuse
                    ? new List<IMechanicalEffect> { new RefuseLetterEffect(
                        letterFromThisNPC.Id, npc.ID, npc.Name, _queueManager, _tokenManager) }
                    : new List<IMechanicalEffect> { new LockedEffect($"Need {refuseCost} Trust to refuse") }
            });
        }
        
        // 1 ATTENTION OPTION D: Trade tokens (Commerce for Status)
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
                MechanicalDescription = $"Trade 3 Commerce → 2 Status with {npc.Name}",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new BurnTokensEffect(ConnectionType.Commerce, 3, npc.ID, _tokenManager),
                    new GainTokensEffect(ConnectionType.Status, 2, npc.ID, _tokenManager)
                }
            });
        }
        
        // 2 ATTENTION: Move urgent letter to front - expensive but powerful
        // Advanced queue manipulation requires T2 (Associate)
        var urgentLetter = lettersInQueue
            .Where(l => l.DeadlineInHours < 12)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();
            
        if (urgentLetter != null)
        {
            var currentPos = _queueManager.GetLetterPosition(urgentLetter.Id) ?? 8;
            bool canPrioritize = playerTier >= TierLevel.T2;
            
            // Find who currently occupies position 1 (they get displaced)
            var letterInPosition1 = _queueManager.GetLetterAt(1);
            string displacedNPCName = letterInPosition1?.SenderName ?? "unknown";
            string displacedNPCId = letterInPosition1?.SenderId ?? "";
            
            // Simple displacement cost calculation
            var tokenTypeToBurn = letterInPosition1?.TokenType ?? ConnectionType.Status;
            int tokenCost = 3; // Base cost for displacement
            
            if (!string.IsNullOrEmpty(displacedNPCId))
            {
                // Calculate simple leverage-based displacement cost
                int leverage = _tokenManager.GetLeverage(displacedNPCId, letterInPosition1.TokenType);
                tokenCost = Math.Max(2, leverage + 2); // Simple cost: leverage + base
                
                // Use the letter's token type for displacement
                tokenTypeToBurn = letterInPosition1.TokenType;
            }
            
            var tokensWithDisplaced = !string.IsNullOrEmpty(displacedNPCId)
                ? _tokenManager.GetTokensWithNPC(displacedNPCId)
                : new Dictionary<ConnectionType, int>();
            
            var canAfford = tokensWithDisplaced.GetValueOrDefault(tokenTypeToBurn, 0) >= tokenCost;
            string priorityText = canPrioritize
                ? GenerateNegotiateNarrativeText(npc, state, "priority")
                : "\"I don't have the standing to rearrange priorities like that...\"";
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "negotiate_priority",
                NarrativeText = priorityText,
                AttentionCost = 2,
                BaseVerb = BaseVerb.NEGOTIATE,
                IsAffordable = canAfford && canPrioritize,
                IsAvailable = letterInPosition1 != null && canPrioritize,
                MechanicalDescription = canPrioritize
                    ? $"Move to position 1 | Burn {tokenCost} {tokenTypeToBurn} with {displacedNPCName}"
                    : "Requires Associate standing to prioritize letters",
                MechanicalEffects = canPrioritize
                    ? new List<IMechanicalEffect> { new LetterReorderEffect(urgentLetter.Id, 1, tokenCost, 
                        tokenTypeToBurn, _queueManager, _tokenManager, displacedNPCId) }
                    : new List<IMechanicalEffect> { new LockedEffect("Requires Associate standing") }
            });
        }
        
        // 3 ATTENTION: Trade tokens for status (requires T3 Confidant for major trades)
        // Major token trading requires both resources AND standing
        var needsTier = playerTier < TierLevel.T3;
        var needsCommerce = commerceTokens < 5;
        var isLocked = needsTier || needsCommerce;
        
        string lockReason = needsTier
            ? "Requires Confidant standing for major trades"
            : needsCommerce
                ? "Requires 5 Commerce tokens"
                : "";
                
        choices.Add(new ConversationChoice
        {
            ChoiceID = "negotiate_trade_tokens",
            NarrativeText = isLocked 
                ? "\"I don't have enough influence for such trades...\"" 
                : GenerateNegotiateNarrativeText(npc, state, "trade"),
            AttentionCost = 3,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = !isLocked,
            IsAvailable = !isLocked,
            MechanicalDescription = isLocked 
                ? lockReason 
                : "Trade tokens | -5 Commerce +3 Status",
            MechanicalEffects = isLocked
                ? new List<IMechanicalEffect> { new LockedEffect(lockReason) }
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
    /// Emotional states determine what information is revealed.
    /// </summary>
    private List<ConversationChoice> GenerateInvestigateChoices(
        NPC npc,
        NPCEmotionalState state,
        List<Letter> relevantLetters)
    {
        var choices = new List<ConversationChoice>();
        var trustTokens = _tokenManager.GetTokensWithNPC(npc.ID).GetValueOrDefault(ConnectionType.Trust, 0);
        var playerTier = _player.CurrentTier;
        
        // Emotional states affect information availability:
        // DESPERATE: Reveals everything, no time cost
        // ANXIOUS: Reveals urgent info quickly
        // CALCULATING: Standard investigation costs
        // HOSTILE: Blocks most investigation
        // WITHDRAWN: Vague info only
        
        // 1 ATTENTION: Learn schedule - time cost varies by state
        bool canInvestigateBasic = playerTier >= TierLevel.T2 || state == NPCEmotionalState.DESPERATE;
        string scheduleText = GenerateInvestigateNarrativeText(npc, state, "schedule");
        
        // Time cost depends on emotional state
        int scheduleCost = state switch
        {
            NPCEmotionalState.DESPERATE => 0,    // Spills everything immediately
            NPCEmotionalState.ANXIOUS => 5,      // Quick info
            NPCEmotionalState.CALCULATING => 10, // Standard cost
            NPCEmotionalState.HOSTILE => 20,     // Makes you work for it
            NPCEmotionalState.WITHDRAWN => 15,   // Takes time to engage
            _ => 10
        };
        
        bool isBlocked = state == NPCEmotionalState.HOSTILE && playerTier < TierLevel.T3;
        
        choices.Add(new ConversationChoice
        {
            ChoiceID = "investigate_schedule",
            NarrativeText = scheduleText,
            AttentionCost = 1,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = canInvestigateBasic && !isBlocked,
            IsAvailable = canInvestigateBasic && !isBlocked,
            MechanicalDescription = isBlocked
                ? "HOSTILE: Information blocked"
                : state == NPCEmotionalState.DESPERATE
                    ? $"FREE INFO: {npc.Name}'s full schedule!"
                    : $"Learn {npc.Name}'s schedule (+{scheduleCost} min)",
            MechanicalEffects = canInvestigateBasic && !isBlocked
                ? new List<IMechanicalEffect>
                  {
                      new LearnNPCScheduleEffect(npc.ID, _gameWorld, _player),
                      new ConversationTimeEffect(scheduleCost, _timeManager)
                  }
                : new List<IMechanicalEffect> { new LockedEffect(isBlocked ? "Blocked by HOSTILE state" : "Requires Associate standing") }
        });
        
        // 1 ATTENTION: Reveal letter property with time cost
        // Letter investigation also requires T2 (Associate)
        if (relevantLetters.Any())
        {
            var letter = relevantLetters.First();
            bool canInvestigateLetter = playerTier >= TierLevel.T2;
            string letterText = canInvestigateLetter
                ? GenerateInvestigateNarrativeText(npc, state, "letter")
                : "\"I shouldn't pry into such matters yet...\"";
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "investigate_letter",
                NarrativeText = letterText,
                AttentionCost = 1,
                BaseVerb = BaseVerb.INVESTIGATE,
                IsAffordable = canInvestigateLetter,
                IsAvailable = canInvestigateLetter,
                MechanicalDescription = canInvestigateLetter
                    ? $"Examine {letter.SenderName}'s letter (+15 min)"
                    : "Requires Associate standing to examine letters",
                MechanicalEffects = canInvestigateLetter
                    ? new List<IMechanicalEffect>
                      {
                          new RevealLetterPropertyEffect(letter.Id, "stakes", _queueManager, _player),
                          new ConversationTimeEffect(15, _timeManager)
                      }
                    : new List<IMechanicalEffect> { new LockedEffect("Requires Associate standing") }
            });
        }
        
        // 2 ATTENTION: Deep investigation with major time cost
        // Deep investigation requires T2 (Associate) as well
        bool canDeepInvestigate = playerTier >= TierLevel.T2;
        string deepText = canDeepInvestigate
            ? GenerateInvestigateNarrativeText(npc, state, "deep")
            : "\"I need more standing before uncovering such connections...\"";
            
        choices.Add(new ConversationChoice
        {
            ChoiceID = "investigate_deep",
            NarrativeText = deepText,
            AttentionCost = 2,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = canDeepInvestigate,
            IsAvailable = canDeepInvestigate,
            MechanicalDescription = canDeepInvestigate
                ? $"Uncover {npc.Name}'s network (+20 min)"
                : "Requires Associate standing for deep investigation",
            MechanicalEffects = canDeepInvestigate
                ? new List<IMechanicalEffect>
                  {
                      new DeepInvestigationEffect($"{npc.Name}'s connections and obligations"),
                      new ConversationTimeEffect(20, _timeManager)
                  }
                : new List<IMechanicalEffect> { new LockedEffect("Requires Associate standing") }
        });
        
        // 3 ATTENTION: Locked - reveal network/conspiracy
        // Conspiracy investigation requires T3 (Confidant) AND trust
        var needsTier = playerTier < TierLevel.T3;
        var needsTrust = trustTokens < 3;
        var isLocked = needsTier || needsTrust;
        
        string networkText = isLocked
            ? "\"I'm not in a position to ask about such matters...\""
            : "\"Tell me who's really pulling the strings.\"";
            
        string lockReason = needsTier
            ? "Requires Confidant standing to uncover conspiracies"
            : needsTrust
                ? $"Requires 3 Trust with {npc.Name}"
                : "";
                
        choices.Add(new ConversationChoice
        {
            ChoiceID = "investigate_network",
            NarrativeText = networkText,
            AttentionCost = 3,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = !isLocked,
            IsAvailable = !isLocked,
            MechanicalDescription = isLocked 
                ? lockReason 
                : $"Expose conspiracy around {npc.Name} (+30 min)",
            MechanicalEffects = isLocked
                ? new List<IMechanicalEffect> { new LockedEffect(lockReason) }
                : new List<IMechanicalEffect> 
                { 
                    new DiscoverLetterNetworkEffect(
                        relevantLetters.FirstOrDefault()?.Id ?? "none",
                        _queueManager, _player),
                    new ConversationTimeEffect(30, _timeManager)
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
            // Calculate trust reward for display
            int trustReward = letterInPosition1.DeadlineInHours < 12 ? 5 :
                             letterInPosition1.DeadlineInHours < 24 ? 4 : 3;
            
            return new ConversationChoice
            {
                ChoiceID = "deliver_letter",
                NarrativeText = $"\"I have your letter from {letterInPosition1.SenderName}.\"",
                AttentionCost = 0, // Delivery is free
                BaseVerb = BaseVerb.EXIT,
                IsAffordable = true,
                IsAvailable = true,
                Priority = 100, // Always show first
                MechanicalDescription = $"Deliver {letterInPosition1.SenderName}'s letter → +{letterInPosition1.Payment} coins, +{trustReward} Trust",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new DeliverLetterEffect(
                        letterInPosition1.Id, 
                        letterInPosition1,
                        _queueManager, 
                        _timeManager,
                        _tokenManager)
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
            MechanicalDescription = "End conversation",
            MechanicalEffects = new List<IMechanicalEffect> 
            { 
                new EndConversationEffect() 
            }
        };
    }
    
    /// <summary>
    /// Generate narrative text for HELP choices that reflects emotional state strongly.
    /// DESPERATE NPCs are vulnerable, ANXIOUS are focused, CALCULATING are strategic.
    /// </summary>
    private string GenerateHelpNarrativeText(NPC npc, NPCEmotionalState state, int attentionLevel)
    {
        // Emotional states drive the narrative - make them VISIBLE
        return (state, attentionLevel) switch
        {
            // DESPERATE: Vulnerability, pleading, desperation bleeding through
            (NPCEmotionalState.DESPERATE, 1) => "\"Please, I'll do anything. Just help me.\"",
            (NPCEmotionalState.DESPERATE, 2) => "\"I'm begging you. I need this more than you know.\"",
            (NPCEmotionalState.DESPERATE, 3) => "\"Save me. I have nowhere else to turn.\"",
            
            // ANXIOUS: Focused on their specific urgent need
            (NPCEmotionalState.ANXIOUS, 1) => "\"This is urgent. Can you help?\"",
            (NPCEmotionalState.ANXIOUS, 2) => "\"Time is running out. I need your commitment.\"",
            (NPCEmotionalState.ANXIOUS, 3) => "\"Every moment matters. Please, prioritize this.\"",
            
            // CALCULATING: Strategic, measured, transactional
            (NPCEmotionalState.CALCULATING, 1) => "\"Let's establish mutual benefit.\"",
            (NPCEmotionalState.CALCULATING, 2) => "\"I propose a deeper arrangement.\"",
            (NPCEmotionalState.CALCULATING, 3) => "\"Our interests could align perfectly.\"",
            
            // HOSTILE: Blocked, demanding, no trust
            (NPCEmotionalState.HOSTILE, 1) => "\"After what you've done? Help means nothing.\"",
            (NPCEmotionalState.HOSTILE, 2) => "\"Actions, not words. Prove yourself.\"",
            (NPCEmotionalState.HOSTILE, 3) => "\"I don't trust your promises anymore.\"",
            
            // WITHDRAWN: Minimal engagement, indifferent
            (NPCEmotionalState.WITHDRAWN, 1) => "\"If you insist on helping...\"",
            (NPCEmotionalState.WITHDRAWN, 2) => "\"I suppose you could try.\"",
            (NPCEmotionalState.WITHDRAWN, 3) => "\"Do what you will.\"",
            
            _ => "\"I could use your help.\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for accepting a letter based on emotional state.
    /// State + Stakes create the narrative tension.
    /// </summary>
    private string GenerateAcceptLetterNarrativeText(NPC npc, NPCEmotionalState state, Letter letter)
    {
        // Let emotional state transform how the letter acceptance feels
        return (state, letter.Stakes) switch
        {
            // DESPERATE + Stakes combinations
            (NPCEmotionalState.DESPERATE, StakeType.SAFETY) => 
                $"\"Please! This could save {letter.RecipientName}'s life!\"",
            (NPCEmotionalState.DESPERATE, StakeType.REPUTATION) => 
                $"\"My entire standing depends on {letter.RecipientName} getting this!\"",
            (NPCEmotionalState.DESPERATE, StakeType.WEALTH) => 
                $"\"I'll lose everything if {letter.RecipientName} doesn't receive this!\"",
            (NPCEmotionalState.DESPERATE, _) => 
                $"\"I'm begging you to deliver this to {letter.RecipientName}.\"",
            
            // ANXIOUS + Stakes combinations
            (NPCEmotionalState.ANXIOUS, StakeType.SAFETY) => 
                $"\"This is urgent - {letter.RecipientName} needs warning immediately.\"",
            (NPCEmotionalState.ANXIOUS, StakeType.REPUTATION) => 
                $"\"My reputation with {letter.RecipientName} hangs on this delivery.\"",
            (NPCEmotionalState.ANXIOUS, StakeType.WEALTH) => 
                $"\"Time is money - {letter.RecipientName} awaits this urgently.\"",
            (NPCEmotionalState.ANXIOUS, _) => 
                $"\"Please hurry this to {letter.RecipientName}.\"",
            
            // CALCULATING + Stakes combinations
            (NPCEmotionalState.CALCULATING, StakeType.WEALTH) => 
                $"\"A profitable arrangement for {letter.RecipientName}. Handle it well.\"",
            (NPCEmotionalState.CALCULATING, StakeType.REPUTATION) => 
                $"\"This will position us favorably with {letter.RecipientName}.\"",
            (NPCEmotionalState.CALCULATING, _) => 
                $"\"Deliver this to {letter.RecipientName}. There's advantage in it.\"",
            
            // HOSTILE doesn't offer letters easily
            (NPCEmotionalState.HOSTILE, _) => 
                "\"Take it then. See if I care about your promises.\"",
            
            // WITHDRAWN is disconnected
            (NPCEmotionalState.WITHDRAWN, _) => 
                $"\"If you must... {letter.RecipientName} expects it.\"",
            
            _ => $"\"Please deliver this to {letter.RecipientName}.\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for accepting an urgent letter.
    /// Emotional state + urgency creates dramatic tension.
    /// </summary>
    private string GenerateAcceptUrgentNarrativeText(NPC npc, NPCEmotionalState state, Letter letter)
    {
        var hoursText = letter.DeadlineInHours <= 4 ? "NOW" : letter.DeadlineInHours <= 8 ? "TODAY" : "SOON";
        
        return (state, letter.Stakes) switch
        {
            // DESPERATE combinations - maximum drama
            (NPCEmotionalState.DESPERATE, StakeType.SAFETY) => 
                $"\"PLEASE! Lives hang in the balance! {letter.RecipientName} needs this {hoursText}!\"",
            (NPCEmotionalState.DESPERATE, StakeType.REPUTATION) => 
                $"\"I'm ruined if {letter.RecipientName} doesn't get this {hoursText}!\"",
            (NPCEmotionalState.DESPERATE, StakeType.WEALTH) => 
                $"\"Everything depends on {letter.RecipientName} receiving this {hoursText}!\"",
            (NPCEmotionalState.DESPERATE, _) => 
                $"\"I'm begging - {letter.RecipientName} MUST have this {hoursText}!\"",
            
            // ANXIOUS combinations - focused urgency
            (NPCEmotionalState.ANXIOUS, StakeType.SAFETY) => 
                $"\"Critical warning for {letter.RecipientName} - deliver {hoursText}!\"",
            (NPCEmotionalState.ANXIOUS, StakeType.REPUTATION) => 
                $"\"My reputation with {letter.RecipientName} expires {hoursText}.\"",
            (NPCEmotionalState.ANXIOUS, _) => 
                $"\"Urgent for {letter.RecipientName} - needed {hoursText}.\"",
            
            // CALCULATING sees opportunity in urgency
            (NPCEmotionalState.CALCULATING, _) => 
                $"\"Premium delivery to {letter.RecipientName}. Worth expediting.\"",
            
            // HOSTILE doesn't offer urgent letters
            (NPCEmotionalState.HOSTILE, _) => 
                $"\"Take it or leave it. {letter.RecipientName} can wait.\"",
            
            // WITHDRAWN is disconnected even from urgency
            (NPCEmotionalState.WITHDRAWN, _) => 
                $"\"Urgent, apparently. For {letter.RecipientName}.\"",
            
            _ => $"\"Urgent delivery to {letter.RecipientName} ({hoursText}).\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for refusing a letter - this should feel heavy, consequential.
    /// The emotional state of the NPC affects how they react to your betrayal.
    /// </summary>
    private string GenerateRefuseLetterNarrativeText(NPC npc, NPCEmotionalState state, Letter letter)
    {
        // This is a moment of human failure - the NPC's state shows in their reaction
        return state switch
        {
            NPCEmotionalState.DESPERATE => 
                "\"NO! You can't do this to me! Not now! Please!\"",
            NPCEmotionalState.ANXIOUS => 
                "\"What? But you promised! This is urgent!\"",
            NPCEmotionalState.CALCULATING => 
                "\"I see. Breaking our arrangement will have... consequences.\"",
            NPCEmotionalState.HOSTILE => 
                "\"Of course you'd fail me. I expected nothing less.\"",
            NPCEmotionalState.WITHDRAWN => 
                "\"Oh. I suppose it doesn't matter anyway.\"",
            _ => "\"You're returning my letter? After promising to deliver it?\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for NEGOTIATE choices that reflects emotional state.
    /// DESPERATE NPCs accept bad deals, CALCULATING seek advantage.
    /// </summary>
    private string GenerateNegotiateNarrativeText(NPC npc, NPCEmotionalState state, string negotiationType, params int[] positions)
    {
        return (state, negotiationType) switch
        {
            // DESPERATE: Will accept anything, make bad deals
            (NPCEmotionalState.DESPERATE, "swap") when positions.Length >= 2 => 
                $"\"Please! Just move them around - positions {positions[0]} and {positions[1]}!\"",
            (NPCEmotionalState.DESPERATE, "interface") => 
                "\"Do whatever you need with the queue. I trust you!\"",
            (NPCEmotionalState.DESPERATE, "priority") => 
                "\"Move it to the front! I'll pay any price!\"",
            (NPCEmotionalState.DESPERATE, "trade") =>
                "\"Take whatever tokens you need. Just help me!\"",
            
            // ANXIOUS: Focused on specific urgent needs
            (NPCEmotionalState.ANXIOUS, "swap") when positions.Length >= 2 => 
                $"\"Can we quickly swap {positions[0]} and {positions[1]}?\"",
            (NPCEmotionalState.ANXIOUS, "interface") => 
                "\"Let's fix the queue - time is critical.\"",
            (NPCEmotionalState.ANXIOUS, "priority") => 
                "\"This needs to jump ahead. It's urgent.\"",
            (NPCEmotionalState.ANXIOUS, "trade") =>
                "\"I need different tokens. Can we trade quickly?\"",
            
            // CALCULATING: Strategic trades, seeking advantage
            (NPCEmotionalState.CALCULATING, "swap") when positions.Length >= 2 => 
                $"\"A strategic swap of {positions[0]} and {positions[1]} benefits us both.\"",
            (NPCEmotionalState.CALCULATING, "interface") => 
                "\"Let's optimize the queue arrangement.\"",
            (NPCEmotionalState.CALCULATING, "priority") => 
                "\"Moving this forward serves our mutual interests.\"",
            (NPCEmotionalState.CALCULATING, "trade") =>
                "\"I propose an exchange - favorable rates for both parties.\"",
            
            // HOSTILE: Demands, no negotiation
            (NPCEmotionalState.HOSTILE, _) => 
                "\"Fix your failures. Now.\"",
            
            // WITHDRAWN: Minimal interest
            (NPCEmotionalState.WITHDRAWN, _) => 
                "\"Rearrange as you see fit.\"",
            
            // Default fallbacks for specific types
            (_, "swap") when positions.Length >= 2 => 
                $"\"Could we swap positions {positions[0]} and {positions[1]}?\"",
            (_, "interface") => 
                "\"Let's review the queue.\"",
            (_, "priority") => 
                "\"I need to move this forward.\"",
            (_, "trade") =>
                "\"Shall we trade tokens?\"",
            _ => "\"Let's negotiate.\""
        };
    }
    
    /// <summary>
    /// Generate narrative text for INVESTIGATE choices that reflects emotional state.
    /// DESPERATE NPCs reveal everything, CALCULATING are selective.
    /// </summary>
    private string GenerateInvestigateNarrativeText(NPC npc, NPCEmotionalState state, string investigationType)
    {
        return (state, investigationType) switch
        {
            // DESPERATE: Spills everything, no secrets
            (NPCEmotionalState.DESPERATE, "schedule") => 
                "\"Everyone avoids me now. Here's where they hide...\"",
            (NPCEmotionalState.DESPERATE, "letter") => 
                "\"Look at it! See what's destroying me!\"",
            (NPCEmotionalState.DESPERATE, "deep") => 
                "\"I'll tell you everything. Every secret. Just help!\"",
            (NPCEmotionalState.DESPERATE, "network") => 
                "\"They're all against me! I'll name names!\"",
            
            // ANXIOUS: Focused revelations about urgent matters
            (NPCEmotionalState.ANXIOUS, "schedule") => 
                "\"You need to know when to catch them.\"",
            (NPCEmotionalState.ANXIOUS, "letter") => 
                "\"Check the letter - you'll see why I'm worried.\"",
            (NPCEmotionalState.ANXIOUS, "deep") => 
                "\"There's more at stake than you realize.\"",
            (NPCEmotionalState.ANXIOUS, "network") => 
                "\"Careful - powerful people are involved.\"",
            
            // CALCULATING: Information as currency
            (NPCEmotionalState.CALCULATING, "schedule") => 
                "\"Strategic timing is everything. Let me enlighten you.\"",
            (NPCEmotionalState.CALCULATING, "letter") => 
                "\"The letter's properties might interest you.\"",
            (NPCEmotionalState.CALCULATING, "deep") => 
                "\"Information has value. This is worth knowing.\"",
            (NPCEmotionalState.CALCULATING, "network") => 
                "\"The web of connections might surprise you.\"",
            
            // HOSTILE: Refuses to share
            (NPCEmotionalState.HOSTILE, _) => 
                "\"Why should I tell you anything?\"",
            
            // WITHDRAWN: Vague, disconnected
            (NPCEmotionalState.WITHDRAWN, "schedule") => 
                "\"People come and go...\"",
            (NPCEmotionalState.WITHDRAWN, "letter") => 
                "\"Look if you want.\"",
            (NPCEmotionalState.WITHDRAWN, "deep") => 
                "\"Does it matter?\"",
            (NPCEmotionalState.WITHDRAWN, "network") => 
                "\"Everyone's connected somehow.\"",
            
            _ => "\"Let me investigate.\""
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
            MechanicalDescription = "End conversation",
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
                MechanicalDescription = "End conversation",
                MechanicalEffects = new List<IMechanicalEffect> { new MaintainStateEffect() }
            });
        }
        
        return choices;
    }
}