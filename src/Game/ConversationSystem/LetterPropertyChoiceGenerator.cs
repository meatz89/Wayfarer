using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates additional conversation choices based on letter properties
/// when the NPC is a sender or recipient of letters in the queue.
/// </summary>
public class LetterPropertyChoiceGenerator
{
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly ITimeManager _timeManager;
    private readonly Player _player;
    private readonly GameWorld _gameWorld;
    
    public LetterPropertyChoiceGenerator(
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        ITimeManager timeManager,
        Player player,
        GameWorld gameWorld)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _timeManager = timeManager;
        _player = player;
        _gameWorld = gameWorld;
    }
    
    /// <summary>
    /// Generate additional choices based on letter properties.
    /// These are added on top of the base NPC state choices.
    /// </summary>
    public List<ConversationChoice> GenerateLetterBasedChoices(NPC npc, NPCEmotionalState state)
    {
        var additionalChoices = new List<ConversationChoice>();
        
        // Find all letters involving this NPC
        var relevantLetters = FindRelevantLetters(npc);
        Console.WriteLine($"[LetterPropertyChoiceGenerator] Found {relevantLetters.Count} relevant letters for {npc.Name}");
        foreach (var letter in relevantLetters)
        {
            Console.WriteLine($"  - Letter: {letter.Description}, From: {letter.SenderName}, To: {letter.RecipientName}, Position: {letter.QueuePosition}");
        }
        
        if (!relevantLetters.Any())
            return additionalChoices;
        
        // Sort by priority (deadline urgency)
        var priorityLetter = relevantLetters
            .OrderBy(l => l.DeadlineInHours)
            .First();
        
        // Generate choices based on letter properties
        
        // DEADLINE-BASED CHOICES
        if (priorityLetter.DeadlineInHours < 6)
        {
            additionalChoices.Add(CreatePromiseChoice(npc, priorityLetter));
            additionalChoices.Add(CreateExtendDeadlineChoice(priorityLetter));
        }
        
        // STAKES-BASED CHOICES
        switch (priorityLetter.Stakes)
        {
            case StakeType.SAFETY:
                additionalChoices.Add(CreateUrgentHelpChoice(npc, priorityLetter));
                additionalChoices.Add(CreateProtectChoice(npc, priorityLetter));
                break;
                
            case StakeType.REPUTATION:
                additionalChoices.Add(CreateDeepInvestigateChoice(npc, priorityLetter));
                additionalChoices.Add(CreateShareInfoChoice(npc, priorityLetter));
                break;
                
            case StakeType.WEALTH:
                additionalChoices.Add(CreateCommerceChoice(npc, priorityLetter));
                break;
                
            case StakeType.SECRET:
                additionalChoices.Add(CreateSecretChoice(npc, priorityLetter));
                break;
        }
        
        // RECIPIENT STATUS CHOICES (for high-status recipients)
        if (GetRecipientStatus(priorityLetter) >= 4)
        {
            additionalChoices.Add(CreateUnlockAccessChoice(priorityLetter));
            additionalChoices.Add(CreateGainInfluenceChoice(priorityLetter));
        }
        
        // QUEUE POSITION CHOICES (for letters far back in queue)
        var position = _queueManager.GetLetterPosition(priorityLetter.Id);
        if (position.HasValue && position.Value > 5)
        {
            additionalChoices.Add(CreateDesperateReorderChoice(npc, priorityLetter));
            additionalChoices.Add(CreateRemoveTemporarilyChoice(priorityLetter));
        }
        
        // DELIVERY CHOICE (check if ANY letter in position 1 is for this NPC)
        // Bug fix: Don't use priorityLetter here - check ALL relevant letters for position 1
        var letterInPosition1 = relevantLetters
            .FirstOrDefault(l => 
            {
                var pos = _queueManager.GetLetterPosition(l.Id);
                return pos.HasValue && pos.Value == 1 && l.RecipientName == npc.Name;
            });
            
        if (letterInPosition1 != null)
        {
            additionalChoices.Add(CreateDeliveryChoice(letterInPosition1));
        }
        
        return additionalChoices;
    }
    
    private List<Letter> FindRelevantLetters(NPC npc)
    {
        var allLetters = _queueManager.GetActiveLetters();
        Console.WriteLine($"[FindRelevantLetters] Checking {allLetters.Length} letters for NPC {npc.Name} (ID: {npc.ID})");
        
        foreach (var letter in allLetters)
        {
            Console.WriteLine($"  Letter: From {letter.SenderName} (ID: {letter.SenderId}) To {letter.RecipientName} (ID: {letter.RecipientId})");
            Console.WriteLine($"    Checking: SenderId({letter.SenderId}) == {npc.ID}? {letter.SenderId == npc.ID}");
            Console.WriteLine($"    Checking: SenderName({letter.SenderName}) == {npc.Name}? {letter.SenderName == npc.Name}");  
            Console.WriteLine($"    Checking: RecipientId({letter.RecipientId}) == {npc.ID}? {letter.RecipientId == npc.ID}");
            Console.WriteLine($"    Checking: RecipientName({letter.RecipientName}) == {npc.Name}? {letter.RecipientName == npc.Name}");
        }
        
        return allLetters
            .Where(l => l.SenderId == npc.ID || 
                       l.SenderName == npc.Name ||
                       l.RecipientId == npc.ID ||
                       l.RecipientName == npc.Name)
            .ToList();
    }
    
    private int GetRecipientStatus(Letter letter)
    {
        // Determine recipient social status (1-5 scale)
        // This could be expanded to use actual NPC data
        if (letter.RecipientName?.Contains("Lord") == true ||
            letter.RecipientName?.Contains("Lady") == true)
            return 4;
        if (letter.RecipientName?.Contains("Master") == true)
            return 3;
        if (letter.RecipientName?.Contains("Merchant") == true)
            return 2;
        return 1;
    }
    
    // DEADLINE-BASED CHOICES
    
    private ConversationChoice CreatePromiseChoice(NPC npc, Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_promise_urgent",
            NarrativeText = $"\"I swear I'll deliver your letter to {letter.RecipientName} before all others today.\"",
            AttentionCost = 2,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Creates obligation | +2 Trust",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new CreateBindingObligationEffect(npc.ID, 
                    $"Sworn to prioritize {npc.Name}'s letters"),
                new GainTokensEffect(ConnectionType.Trust, 2, npc.ID, _tokenManager)
            }
        };
    }
    
    private ConversationChoice CreateExtendDeadlineChoice(Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_extend_deadline",
            NarrativeText = "\"Give me one more day. I promise it will arrive.\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "+24 hours deadline | -1 Commerce",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new ExtendDeadlineEffect(letter.Id, 1, _queueManager),
                new BurnTokensEffect(ConnectionType.Commerce, 1, letter.SenderId, _tokenManager)
            }
        };
    }
    
    // SAFETY STAKES CHOICES
    
    private ConversationChoice CreateUrgentHelpChoice(NPC npc, Letter letter)
    {
        var position = _queueManager.GetLetterPosition(letter.Id) ?? 8;
        return new ConversationChoice
        {
            ChoiceID = "letter_urgent_help_safety",
            NarrativeText = "\"This is life or death? I'll move it to the front immediately!\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = position > 1,
            MechanicalDescription = "Position 1 | Creates obligation",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new LetterReorderEffect(letter.Id, 1, 0, ConnectionType.Trust, 
                    _queueManager, _tokenManager, npc.ID),
                new CreateObligationEffect($"urgent_help_{letter.Id}", npc.ID, _player)
            }
        };
    }
    
    private ConversationChoice CreateProtectChoice(NPC npc, Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_protect_safety",
            NarrativeText = "\"Take the hidden route through the old cemetery. It's safer.\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Unlock safe route",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new UnlockRouteEffect("Cemetery Path (Safe)")
            }
        };
    }
    
    // REPUTATION STAKES CHOICES
    
    private ConversationChoice CreateDeepInvestigateChoice(NPC npc, Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_deep_investigate",
            NarrativeText = $"\"Why is {letter.RecipientName}'s reputation at stake? What aren't you telling me?\"",
            AttentionCost = 2,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Deep investigation | 20 min",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new DeepInvestigationEffect($"The truth about {letter.RecipientName}"),
                new ConversationTimeEffect(20, _timeManager)
            }
        };
    }
    
    private ConversationChoice CreateShareInfoChoice(NPC npc, Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_share_info",
            NarrativeText = "\"I know a merchant who travels that route. Let me share what I know.\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Share route | +1 Trust",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new ShareInformationEffect(
                    new RouteOption { Name = "Merchant's Path", TravelTimeHours = 2 }, 
                    npc),
                new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager)
            }
        };
    }
    
    // WEALTH STAKES CHOICE
    
    private ConversationChoice CreateCommerceChoice(NPC npc, Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_commerce_wealth",
            NarrativeText = "\"Double my fee and I'll ensure it arrives today.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "+2 Commerce | Position 2",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new GainTokensEffect(ConnectionType.Commerce, 2, npc.ID, _tokenManager),
                new LetterReorderEffect(letter.Id, 2, 0, ConnectionType.Commerce, 
                    _queueManager, _tokenManager, npc.ID)
            }
        };
    }
    
    // SECRET STAKES CHOICE
    
    private ConversationChoice CreateSecretChoice(NPC npc, Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_secret",
            NarrativeText = "\"Your secret is safe with me. No one will know.\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "+1 Trust | Learns secret",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager),
                new CreateMemoryEffect($"secret_{letter.Id}", 
                    $"{npc.Name}'s secret about {letter.RecipientName}", 10, 7)
            }
        };
    }
    
    // HIGH-STATUS RECIPIENT CHOICES
    
    private ConversationChoice CreateUnlockAccessChoice(Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_unlock_noble_access",
            NarrativeText = $"\"Lord {letter.RecipientName}? I can get you into the Noble Quarter.\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Unlock Noble Quarter",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new UnlockLocationEffect("noble_quarter", _gameWorld)
            }
        };
    }
    
    private ConversationChoice CreateGainInfluenceChoice(Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_gain_influence",
            NarrativeText = $"\"A letter to {letter.RecipientName}? This could be useful leverage...\"",
            AttentionCost = 2,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "+1 Status | Political leverage",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new GainTokensEffect(ConnectionType.Status, 1, letter.SenderId, _tokenManager),
                new CreateMemoryEffect($"leverage_{letter.Id}", 
                    $"Political leverage over {letter.RecipientName}", 8, -1)
            }
        };
    }
    
    // QUEUE POSITION CHOICES
    
    private ConversationChoice CreateDesperateReorderChoice(NPC npc, Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_desperate_reorder",
            NarrativeText = "\"Your letter is buried! I'll prioritize it immediately!\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Position 1 | -1 Status",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new LetterReorderEffect(letter.Id, 1, 1, ConnectionType.Status, 
                    _queueManager, _tokenManager, npc.ID)
            }
        };
    }
    
    private ConversationChoice CreateRemoveTemporarilyChoice(Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "letter_remove_temporarily",
            NarrativeText = "\"Let me hold onto this while I sort out the urgent ones.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "Hold letter temporarily",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new RemoveLetterTemporarilyEffect(letter.Id, _queueManager)
            }
        };
    }
    
    // DELIVERY CHOICE
    
    private ConversationChoice CreateDeliveryChoice(Letter letter)
    {
        return new ConversationChoice
        {
            ChoiceID = "deliver_letter",
            NarrativeText = $"\"I have your letter from {letter.SenderName}. Here it is.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.EXIT,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = $"Deliver letter | +{letter.Payment} coins",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new DeliverLetterEffect(letter.Id, letter, _queueManager, _timeManager, _tokenManager)
            }
        };
    }
}