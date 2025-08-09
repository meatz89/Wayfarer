using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Wayfarer.GameState;

/// <summary>
/// Generates conversation choices using additive system:
/// Base choices from NPC state + Additional choices from letter properties
/// </summary>
public class ConversationChoiceGenerator
{
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly ITimeManager _timeManager;
    private readonly BaseConversationTemplate _baseTemplate;
    private readonly LetterPropertyChoiceGenerator _letterChoiceGenerator;
    private readonly Player _player;
    private readonly GameWorld _gameWorld;
    private readonly ConsequenceEngine _consequenceEngine;

    public ConversationChoiceGenerator(
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        NPCEmotionalStateCalculator stateCalculator,
        ITimeManager timeManager,
        Player player,
        GameWorld gameWorld,
        ConsequenceEngine consequenceEngine = null)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _timeManager = timeManager;
        _player = player;
        _gameWorld = gameWorld;
        _consequenceEngine = consequenceEngine;
        
        // Initialize the additive system components
        _baseTemplate = new BaseConversationTemplate(tokenManager, timeManager);
        _letterChoiceGenerator = new LetterPropertyChoiceGenerator(
            queueManager, tokenManager, timeManager, player, gameWorld);
    }

    public List<ConversationChoice> GenerateChoices(SceneContext context, ConversationState state)
    {
        if (context?.TargetNPC == null || context.AttentionManager == null)
        {
            return GetFallbackChoices();
        }

        // Calculate NPC emotional state
        var npcState = _stateCalculator.CalculateState(context.TargetNPC);
        Console.WriteLine($"[ConversationChoiceGenerator] NPC {context.TargetNPC.Name} is in {npcState} state");
        
        // STEP 1: Get base choices for NPC emotional state (1-2 choices)
        var baseChoices = _baseTemplate.GetBaseChoices(context.TargetNPC, npcState);
        Console.WriteLine($"[ConversationChoiceGenerator] Base template returned {baseChoices.Count} choices");
        
        // STEP 2: Get additional choices from letter properties
        var letterChoices = _letterChoiceGenerator.GenerateLetterBasedChoices(context.TargetNPC, npcState);
        Console.WriteLine($"[ConversationChoiceGenerator] Letter generator returned {letterChoices.Count} additional choices");
        
        // STEP 3: Combine and deduplicate choices
        var allChoices = CombineAndDeduplicateChoices(baseChoices, letterChoices, context.AttentionManager);
        Console.WriteLine($"[ConversationChoiceGenerator] After deduplication: {allChoices.Count} choices");
        
        // STEP 3.5: Filter out locked verbs from consequence system
        allChoices = FilterLockedChoices(allChoices, context.TargetNPC);
        Console.WriteLine($"[ConversationChoiceGenerator] After filtering locked verbs: {allChoices.Count} choices");
        
        // STEP 4: Apply priority rules and limit to 5 choices
        var finalChoices = ApplyPriorityAndLimit(allChoices);
        
        Console.WriteLine($"[ConversationChoiceGenerator] Final: Generated {finalChoices.Count} choices for {context.TargetNPC.Name} in {npcState} state");
        foreach (var choice in finalChoices)
        {
            Console.WriteLine($"  - {choice.ChoiceID}: {choice.MechanicalDescription}");
        }
        
        return finalChoices;
    }
    
    private List<ConversationChoice> CombineAndDeduplicateChoices(
        List<ConversationChoice> baseChoices,
        List<ConversationChoice> letterChoices,
        AttentionManager attentionManager)
    {
        var allChoices = new List<ConversationChoice>();
        
        // Add all base choices first (they have priority)
        allChoices.AddRange(baseChoices);
        
        // Add letter choices, checking for duplicates
        foreach (var letterChoice in letterChoices)
        {
            // Check if choice is affordable
            if (!attentionManager.CanAfford(letterChoice.AttentionCost))
            {
                letterChoice.IsAffordable = false;
            }
            
            // Check for duplicate effect types
            bool isDuplicate = allChoices.Any(existing => 
                AreSimilarChoices(existing, letterChoice));
            
            if (!isDuplicate)
            {
                allChoices.Add(letterChoice);
            }
        }
        
        return allChoices;
    }
    
    private bool AreSimilarChoices(ConversationChoice choice1, ConversationChoice choice2)
    {
        // Choices are similar if they have the same verb and similar effects
        if (choice1.BaseVerb != choice2.BaseVerb)
            return false;
        
        // Check if they have the same primary effect type
        var effect1Types = choice1.MechanicalEffects?.Select(e => e.GetType()).ToList() ?? new List<Type>();
        var effect2Types = choice2.MechanicalEffects?.Select(e => e.GetType()).ToList() ?? new List<Type>();
        
        // If both reorder letters, they're duplicates
        if (effect1Types.Contains(typeof(LetterReorderEffect)) && 
            effect2Types.Contains(typeof(LetterReorderEffect)))
            return true;
        
        // If both create obligations, they're duplicates
        if (effect1Types.Contains(typeof(CreateObligationEffect)) && 
            effect2Types.Contains(typeof(CreateObligationEffect)))
            return true;
        
        return false;
    }
    
    private List<ConversationChoice> ApplyPriorityAndLimit(List<ConversationChoice> choices)
    {
        // Priority order:
        // 1. EXIT (always first)
        // 2. Delivery choices
        // 3. Urgent/binding choices (high attention cost)
        // 4. Investigation choices
        // 5. Basic help/negotiate choices
        
        var prioritized = choices
            .OrderBy(c => GetChoicePriority(c))
            .Take(5)
            .ToList();
        
        return prioritized;
    }
    
    private int GetChoicePriority(ConversationChoice choice)
    {
        // Lower number = higher priority
        if (choice.ChoiceID.Contains("exit"))
            return 0;
        if (choice.ChoiceID.Contains("deliver"))
            return 1;
        if (choice.ChoiceID.Contains("promise") || choice.ChoiceID.Contains("urgent"))
            return 2;
        if (choice.AttentionCost >= 2)
            return 3;
        if (choice.BaseVerb == BaseVerb.INVESTIGATE)
            return 4;
        if (choice.AttentionCost == 1)
            return 5;
        return 6; // Basic choices
    }
    
    private List<ConversationChoice> FilterLockedChoices(List<ConversationChoice> choices, NPC targetNPC)
    {
        if (_consequenceEngine == null)
            return choices;
        
        var filtered = new List<ConversationChoice>();
        
        foreach (var choice in choices)
        {
            // Check if this verb is locked for this NPC
            var verb = MapBaseVerbToConversationVerb(choice.BaseVerb);
            if (verb.HasValue && _consequenceEngine.IsVerbLocked(targetNPC.ID, verb.Value))
            {
                // Replace with locked version that shows why it's unavailable
                var lockedChoice = CreateLockedChoice(choice, targetNPC);
                filtered.Add(lockedChoice);
            }
            else
            {
                filtered.Add(choice);
            }
        }
        
        return filtered;
    }
    
    private BaseVerb? MapBaseVerbToConversationVerb(BaseVerb baseVerb)
    {
        // Direct mapping since we're using BaseVerb in ConsequenceEngine
        return baseVerb == BaseVerb.EXIT ? null : baseVerb;
    }
    
    private ConversationChoice CreateLockedChoice(ConversationChoice original, NPC npc)
    {
        return new ConversationChoice
        {
            ChoiceID = original.ChoiceID + "_locked",
            NarrativeText = $"[TRUST BROKEN] {original.NarrativeText}",
            MechanicalDescription = $"{npc.Name} refuses - too many failed deliveries",
            BaseVerb = original.BaseVerb,
            AttentionCost = 999, // Make it unaffordable
            IsAffordable = false,
            MechanicalEffects = new List<IMechanicalEffect>() // No effects
        };
    }
    
    private List<ConversationChoice> GetFallbackChoices()
    {
        // Simple fallback using base template
        var fallbackNPC = new NPC { ID = "unknown", Name = "Someone" };
        return _baseTemplate.GetBaseChoices(fallbackNPC, NPCEmotionalState.WITHDRAWN);
    }
}