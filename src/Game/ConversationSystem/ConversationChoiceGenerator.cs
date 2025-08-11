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
    private readonly VerbOrganizedChoiceGenerator _verbChoiceGenerator;
    private readonly Player _player;
    private readonly GameWorld _gameWorld;
    private readonly ConsequenceEngine _consequenceEngine;
    private readonly LeverageCalculator _leverageCalculator;
    private readonly Wayfarer.GameState.TimeBlockAttentionManager _timeBlockAttentionManager;

    public ConversationChoiceGenerator(
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        NPCEmotionalStateCalculator stateCalculator,
        ITimeManager timeManager,
        Player player,
        GameWorld gameWorld,
        ConsequenceEngine consequenceEngine,
        LeverageCalculator leverageCalculator,
        Wayfarer.GameState.TimeBlockAttentionManager timeBlockAttentionManager)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _timeManager = timeManager;
        _player = player;
        _gameWorld = gameWorld;
        _consequenceEngine = consequenceEngine;
        _leverageCalculator = leverageCalculator;
        _timeBlockAttentionManager = timeBlockAttentionManager;
        
        // Initialize the new verb-organized choice generator with TimeBlockAttentionManager
        _verbChoiceGenerator = new VerbOrganizedChoiceGenerator(
            queueManager, tokenManager, timeManager, consequenceEngine, leverageCalculator, player, gameWorld, timeBlockAttentionManager);
    }

    public List<ConversationChoice> GenerateChoices(SceneContext context, ConversationState state)
    {
        if (context?.TargetNPC == null)
        {
            return GetFallbackChoices();
        }
        
        // Get shared attention from TimeBlockAttentionManager
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        var attentionManager = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);

        // Calculate NPC emotional state
        var npcState = _stateCalculator.CalculateState(context.TargetNPC);
        Console.WriteLine($"[ConversationChoiceGenerator] NPC {context.TargetNPC.Name} is in {npcState} state");
        
        // Find all letters involving this NPC
        var allLetters = _queueManager.GetActiveLetters();
        var relevantLetters = allLetters
            .Where(l => l.SenderId == context.TargetNPC.ID || 
                       l.SenderName == context.TargetNPC.Name ||
                       l.RecipientId == context.TargetNPC.ID ||
                       l.RecipientName == context.TargetNPC.Name)
            .ToList();
        Console.WriteLine($"[ConversationChoiceGenerator] Found {relevantLetters.Count} relevant letters");
        
        // Generate choices for each verb based on context
        var allChoices = new List<ConversationChoice>();
        
        // Always include EXIT
        allChoices.AddRange(_verbChoiceGenerator.GenerateChoicesForVerb(
            BaseVerb.EXIT, context.TargetNPC, npcState, relevantLetters));
        
        // Add verb choices based on NPC state and player attention
        if (attentionManager.Current > 0)
        {
            // HELP choices (always available if attention exists)
            var helpChoices = _verbChoiceGenerator.GenerateChoicesForVerb(
                BaseVerb.HELP, context.TargetNPC, npcState, relevantLetters);
            Console.WriteLine($"[ConversationChoiceGenerator] Generated {helpChoices.Count} HELP choices");
            allChoices.AddRange(helpChoices);
            
            // NEGOTIATE choices (available when there's queue pressure)
            if (relevantLetters.Any(l => l.DeadlineInHours < 12) || _queueManager.GetActiveLetters().Length > 5)
            {
                var negotiateChoices = _verbChoiceGenerator.GenerateChoicesForVerb(
                    BaseVerb.NEGOTIATE, context.TargetNPC, npcState, relevantLetters);
                Console.WriteLine($"[ConversationChoiceGenerator] Generated {negotiateChoices.Count} NEGOTIATE choices");
                allChoices.AddRange(negotiateChoices);
            }
            
            // INVESTIGATE choices (available when information would be valuable)
            if (attentionManager.Current >= 2 || relevantLetters.Any())
            {
                var investigateChoices = _verbChoiceGenerator.GenerateChoicesForVerb(
                    BaseVerb.INVESTIGATE, context.TargetNPC, npcState, relevantLetters);
                Console.WriteLine($"[ConversationChoiceGenerator] Generated {investigateChoices.Count} INVESTIGATE choices");
                allChoices.AddRange(investigateChoices);
            }
        }
        
        // Filter affordability based on shared attention pool
        foreach (var choice in allChoices)
        {
            choice.IsAffordable = attentionManager.CanAfford(choice.AttentionCost);
        }
        
        // Filter out locked verbs from consequence system
        allChoices = FilterLockedChoices(allChoices, context.TargetNPC);
        Console.WriteLine($"[ConversationChoiceGenerator] After filtering locked verbs: {allChoices.Count} choices");
        
        // Apply priority rules and limit to 5 choices
        var finalChoices = ApplyPriorityAndLimit(allChoices);
        
        Console.WriteLine($"[ConversationChoiceGenerator] Final: Generated {finalChoices.Count} choices for {context.TargetNPC.Name} in {npcState} state");
        foreach (var choice in finalChoices)
        {
            Console.WriteLine($"  - [{choice.BaseVerb}] {choice.ChoiceID}: {choice.MechanicalDescription} (Attention: {choice.AttentionCost})");
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
        // Ensure verb diversity: try to include at least one choice from each verb
        var result = new List<ConversationChoice>();
        
        // Always include EXIT first
        var exitChoice = choices.FirstOrDefault(c => c.BaseVerb == BaseVerb.EXIT);
        if (exitChoice != null)
        {
            result.Add(exitChoice);
        }
        
        // Group remaining choices by verb
        var choicesByVerb = choices
            .Where(c => c.BaseVerb != BaseVerb.EXIT)
            .GroupBy(c => c.BaseVerb)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => GetChoicePriority(c)).ToList());
        
        // First pass: Add the highest priority choice from each verb
        foreach (var verbGroup in choicesByVerb)
        {
            if (verbGroup.Value.Any() && result.Count < 5)
            {
                result.Add(verbGroup.Value.First());
            }
        }
        
        // Second pass: Fill remaining slots with highest priority choices across all verbs
        if (result.Count < 5)
        {
            var remainingChoices = choicesByVerb
                .SelectMany(kvp => kvp.Value.Skip(1)) // Skip the first one we already added
                .OrderBy(c => GetChoicePriority(c))
                .Take(5 - result.Count);
            
            result.AddRange(remainingChoices);
        }
        
        return result;
    }
    
    private int GetChoicePriority(ConversationChoice choice)
    {
        // Lower number = higher priority
        if (choice.ChoiceID.Contains("exit"))
            return 0;
        if (choice.ChoiceID.Contains("deliver"))
            return 1;
        // CRITICAL: Letter acceptance is the core mechanic - prioritize it!
        if (choice.ChoiceID.Contains("accept_letter") || choice.ChoiceID.Contains("accept_urgent"))
            return 2;
        if (choice.ChoiceID.Contains("promise") || choice.ChoiceID.Contains("urgent"))
            return 3;
        if (choice.AttentionCost >= 2)
            return 4;
        if (choice.BaseVerb == BaseVerb.INVESTIGATE)
            return 5;
        if (choice.AttentionCost == 1)
            return 6;
        return 7; // Basic choices
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
        // Simple fallback - just EXIT choice
        return new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "base_exit",
                NarrativeText = "\"I should go.\"",
                AttentionCost = 0,
                BaseVerb = BaseVerb.EXIT,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "→ Leave conversation",
                MechanicalEffects = new List<IMechanicalEffect> 
                { 
                    new EndConversationEffect() 
                }
            }
        };
    }
}