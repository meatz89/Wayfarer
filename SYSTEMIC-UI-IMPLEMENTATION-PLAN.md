# 🎮 Wayfarer: Complete Systemic UI Implementation Plan

## Executive Summary
Transform Wayfarer from AI-driven narrative generation to fully systemic mechanics where ALL choices and dialogue emerge from game state (letter queue, NPC emotions, token relationships).

## 🚨 Current State Analysis

### What Exists But Isn't Connected
1. **VerbContextualizer** - Transforms hidden verbs into narrative choices
2. **NPCEmotionalStateCalculator** - Calculates NPC states from queue
3. **Letter.GetStakesHint()** - Generates narrative from letter properties
4. **AttentionManager** - Manages conversation focus economy

### What's Using AI Instead of Mechanics
1. **ConversationManager** - Uses INarrativeProvider for choices
2. **Choice generation** - Templates from AI, not queue state
3. **NPC dialogue** - Generated text, not systemic properties

## ⚠️ ACTUAL Implementation (What EXISTS in Code)

### Core Verbs: 4 Hidden Verbs (BaseVerb in VerbContextualizer)
- **PLACATE** - Reduce tension, buy time
- **EXTRACT** - Get information or favors  
- **DEFLECT** - Redirect pressure elsewhere
- **COMMIT** - Make binding promises

### Token Types: 4 Types (including Shadow)
- **Trust** - Personal bonds
- **Commerce** - Professional reliability
- **Status** - Social standing
- **Shadow** - Shared secrets, complicity

### NPC States: 4 states from NPCEmotionalStateCalculator
- **DESPERATE** - Urgent need (TTL < 2 or SAFETY stakes)
- **HOSTILE** - Angry (has overdue letters)
- **CALCULATING** - Balanced pressure, normal interaction
- **WITHDRAWN** - No engagement (no letters)

## 📋 Implementation Phases

### Phase 1: Create SystemicNarrativeProvider (2 hours)
Replace the AI narrative provider with systemic generation.

```csharp
public class SystemicNarrativeProvider : INarrativeProvider
{
    private readonly VerbContextualizer _verbContextualizer;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly AttentionManager _attentionManager;
    private readonly MechanicalEffectsCalculator _effectsCalculator;
    private readonly SystemicDialogueGenerator _dialogueGenerator;
    private readonly GameWorld _gameWorld;
    
    public async Task<List<ConversationChoice>> GenerateChoices(
        SceneContext context, 
        ConversationState state, 
        List<ChoiceTemplate> templates)
    {
        var choices = new List<ConversationChoice>();
        var npc = context.TargetNPC;
        var player = state.Player;
        
        // 1. Calculate NPC emotional state
        var npcState = _stateCalculator.CalculateState(npc);
        
        // 2. Get available verbs from VerbContextualizer (it has BaseVerb enum, not ConversationVerb)
        var player = _gameWorld.GetPlayer();
        var availableVerbs = _verbContextualizer.GetAvailableVerbs(npc, npcState, player);
        
        // 3. Find relevant letters in queue
        var relevantLetters = _queueManager.GetActiveLetters()
            .Where(l => l.SenderId == npc.ID || l.RecipientId == npc.ID)
            .OrderBy(l => l.DeadlineInDays)
            .ToList();
        
        // 4. Always add FREE exit option
        choices.Add(CreateExitChoice(npcState));
        
        // 5. Generate verb-based choices using VerbContextualizer
        foreach (var verb in availableVerbs)
        {
            var letter = relevantLetters.FirstOrDefault();
            var choice = _verbContextualizer.GenerateChoice(verb, npc, npcState, letter);
            
            // Add mechanical effects based on actual verb
            choice.Mechanics = _effectsCalculator.CalculateEffects(verb, npc, letter, npcState);
            choices.Add(choice);
        }
        
        // 6. Add queue manipulation choices if letters exist
        if (relevantLetters.Any())
        {
            choices.AddRange(GenerateQueueChoices(relevantLetters, npcState));
        }
        
        return choices;
    }
    
    private List<ConversationVerb> GetSystemicVerbs(NPCEmotionalState state, SceneContext context)
    {
        var verbs = new List<ConversationVerb>();
        
        switch (state)
        {
            case NPCEmotionalState.DESPERATE:
                verbs.Add(ConversationVerb.HELP);      // They need immediate assistance
                verbs.Add(ConversationVerb.NEGOTIATE); // Willing to trade anything
                verbs.Add(ConversationVerb.INVESTIGATE); // Will share information freely
                break;
                
            case NPCEmotionalState.HOSTILE:
                verbs.Add(ConversationVerb.HELP);      // Try to calm them
                verbs.Add(ConversationVerb.NEGOTIATE); // Defensive negotiation only
                // INVESTIGATE locked when hostile
                break;
                
            case NPCEmotionalState.CALCULATING:
                verbs.Add(ConversationVerb.NEGOTIATE); // Open to trades
                verbs.Add(ConversationVerb.INVESTIGATE); // Can probe for info
                verbs.Add(ConversationVerb.HELP);      // Conditional assistance
                break;
                
            case NPCEmotionalState.WITHDRAWN:
                verbs.Add(ConversationVerb.HELP);      // Try to engage them
                // Most verbs locked when withdrawn
                break;
        }
        
        return verbs;
    }
    
    private ConversationChoice GenerateVerbChoice(
        ConversationVerb verb,
        NPC npc,
        NPCEmotionalState state,
        Letter letter)
    {
        var narrativeText = GetVerbNarrative(verb, state, letter);
        var attentionCost = GetVerbAttentionCost(verb, state);
        
        return new ConversationChoice
        {
            ChoiceID = $"{verb}_{npc.ID}_{Guid.NewGuid()}",
            NarrativeText = narrativeText,
            AttentionCost = attentionCost,
            IsAffordable = _attentionManager.CanAfford(attentionCost)
        };
    }
    
    private string GetVerbNarrative(ConversationVerb verb, NPCEmotionalState state, Letter letter)
    {
        return (verb, state) switch
        {
            (ConversationVerb.HELP, NPCEmotionalState.DESPERATE) =>
                "I'll do everything I can to deliver your letter immediately",
            (ConversationVerb.HELP, NPCEmotionalState.HOSTILE) =>
                "Please, let me explain about the delay...",
            (ConversationVerb.NEGOTIATE, NPCEmotionalState.DESPERATE) =>
                "If I prioritize your letter, what can you offer in return?",
            (ConversationVerb.NEGOTIATE, NPCEmotionalState.CALCULATING) =>
                "Perhaps we can arrange a mutually beneficial trade",
            (ConversationVerb.INVESTIGATE, NPCEmotionalState.NEUTRAL) =>
                "Tell me more about this letter's importance",
            (ConversationVerb.INVESTIGATE, NPCEmotionalState.CALCULATING) =>
                "I need to understand the full situation first",
            _ => "Let's discuss this matter"
        };
    }
    
    private int GetVerbAttentionCost(ConversationVerb verb, NPCEmotionalState state)
    {
        // Base costs from IMPLEMENTATION-PLAN.md
        int baseCost = verb switch
        {
            ConversationVerb.HELP => 1,
            ConversationVerb.NEGOTIATE => 1,
            ConversationVerb.INVESTIGATE => 1,
            _ => 0
        };
        
        // State modifiers (from NPCEmotionalStateCalculator)
        if (state == NPCEmotionalState.DESPERATE)
            baseCost = Math.Max(0, baseCost - 1); // Easier when desperate
        else if (state == NPCEmotionalState.HOSTILE)
            baseCost = Math.Min(2, baseCost + 1); // Harder when hostile
            
        return baseCost;
    }
}
```

### Phase 2: Mechanical Effects Calculator (1 hour)

```csharp
public class MechanicalEffectsCalculator
{
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    
    public MechanicalEffectsCalculator(
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
    }
    
    public List<MechanicEffectViewModel> CalculateEffects(
        ConversationVerb verb, 
        NPC npc, 
        Letter letter,
        NPCEmotionalState state)
    {
        var effects = new List<MechanicEffectViewModel>();
        
        switch (verb)
        {
            case ConversationVerb.HELP:
                if (letter != null)
                {
                    effects.Add(new() 
                    { 
                        Icon = "✓", 
                        Description = $"Accept {letter.SenderName}'s letter", 
                        Type = MechanicEffectType.Positive 
                    });
                    effects.Add(new() 
                    { 
                        Icon = "📜", 
                        Description = $"Queue position {GetNextQueuePosition()}", 
                        Type = MechanicEffectType.Neutral 
                    });
                }
                if (state == NPCEmotionalState.DESPERATE)
                {
                    effects.Add(new() 
                    { 
                        Icon = "♥", 
                        Description = "+1 Trust token", 
                        Type = MechanicEffectType.Positive 
                    });
                }
                effects.Add(new() 
                { 
                    Icon = "⏱", 
                    Description = "+15 minutes", 
                    Type = MechanicEffectType.Negative 
                });
                break;
                
            case ConversationVerb.NEGOTIATE:
                if (letter != null && letter.QueuePosition > 1)
                {
                    effects.Add(new() 
                    { 
                        Icon = "→", 
                        Description = $"Move to position 1", 
                        Type = MechanicEffectType.Neutral 
                    });
                    effects.Add(new() 
                    { 
                        Icon = "⚠", 
                        Description = $"Burns {CalculateReorderCost(letter.QueuePosition)} tokens", 
                        Type = MechanicEffectType.Negative 
                    });
                }
                else
                {
                    effects.Add(new() 
                    { 
                        Icon = "🤝", 
                        Description = "Trade positions/resources", 
                        Type = MechanicEffectType.Neutral 
                    });
                }
                effects.Add(new() 
                { 
                    Icon = "⏱", 
                    Description = "+30 minutes", 
                    Type = MechanicEffectType.Negative 
                });
                break;
                
            case ConversationVerb.INVESTIGATE:
                effects.Add(new() 
                { 
                    Icon = "ℹ", 
                    Description = "Gain information", 
                    Type = MechanicEffectType.Positive 
                });
                if (state == NPCEmotionalState.DESPERATE)
                {
                    effects.Add(new() 
                    { 
                        Icon = "🗺️", 
                        Description = "Learn secret route", 
                        Type = MechanicEffectType.Positive 
                    });
                }
                effects.Add(new() 
                { 
                    Icon = "⏱", 
                    Description = "+20 minutes", 
                    Type = MechanicEffectType.Negative 
                });
                break;
        }
        
        return effects;
    }
    
    private int CalculateReorderCost(int fromPosition)
    {
        // Each position jump costs 1 token
        return fromPosition - 1;
    }
    
    private int GetNextQueuePosition()
    {
        var activeLetters = _queueManager.GetActiveLetters();
        return activeLetters.Count + 1; // Next available position
    }
}
```

### Phase 3: Queue Manipulation Choices (1 hour)

```csharp
public class QueueChoiceGenerator
{
    public List<ConversationChoice> GenerateQueueChoices(
        List<Letter> npcLetters,
        NPCEmotionalState state,
        AttentionManager attention)
    {
        var choices = new List<ConversationChoice>();
        
        foreach (var letter in npcLetters)
        {
            // Reorder choice
            if (letter.QueuePosition > 1)
            {
                var reorderCost = CalculateReorderCost(letter.QueuePosition, state);
                
                choices.Add(new ConversationChoice
                {
                    ChoiceID = $"reorder_{letter.Id}",
                    NarrativeText = GenerateReorderNarrative(letter, state),
                    AttentionCost = 1,
                    ChoiceType = ConversationChoiceType.QueueManipulation,
                    Mechanics = new List<MechanicEffectViewModel>
                    {
                        new() 
                        { 
                            Icon = "→", 
                            Description = $"Move to position 1", 
                            Type = MechanicEffectType.Neutral 
                        },
                        new() 
                        { 
                            Icon = "⚠", 
                            Description = $"Burns {reorderCost} tokens", 
                            Type = MechanicEffectType.Negative 
                        }
                    }
                });
            }
            
            // Refuse choice (if desperate)
            if (state == NPCEmotionalState.DESPERATE)
            {
                choices.Add(new ConversationChoice
                {
                    ChoiceID = $"refuse_{letter.Id}",
                    NarrativeText = "I cannot carry this burden right now...",
                    AttentionCost = 0,
                    BaseVerb = BaseVerb.DEFLECT,
                    Mechanics = new List<MechanicEffectViewModel>
                    {
                        new() 
                        { 
                            Icon = "✗", 
                            Description = "Remove from queue", 
                            Type = MechanicEffectType.Negative 
                        },
                        new() 
                        { 
                            Icon = "💔", 
                            Description = "-3 Trust tokens", 
                            Type = MechanicEffectType.Negative 
                        }
                    }
                });
            }
        }
        
        return choices;
    }
    
    private string GenerateReorderNarrative(Letter letter, NPCEmotionalState state)
    {
        return (state, letter.Stakes) switch
        {
            (NPCEmotionalState.DESPERATE, StakeType.SAFETY) => 
                "I'll deliver your warning immediately, before anything else",
            (NPCEmotionalState.DESPERATE, StakeType.REPUTATION) => 
                "Your honor matters more than my other obligations",
            (NPCEmotionalState.HOSTILE, _) => 
                "Fine. I'll prioritize your letter to avoid further conflict",
            (NPCEmotionalState.CALCULATING, StakeType.WEALTH) => 
                "For the right price, I can expedite delivery",
            _ => "I'll see what I can do about moving your letter up"
        };
    }
}
```

### Phase 4: NPC Dialogue Generation from Queue (1 hour)

```csharp
public class SystemicDialogueGenerator
{
    public string GenerateNPCDialogue(
        NPC npc,
        NPCEmotionalState state,
        Letter mostUrgentLetter,
        SceneContext context)
    {
        if (mostUrgentLetter == null)
        {
            return GenerateNoLetterDialogue(npc, state);
        }
        
        // Build dialogue from letter properties
        var stakesHint = mostUrgentLetter.GetStakesHint();
        var urgency = GetUrgencyDescription(mostUrgentLetter.DeadlineInDays);
        
        return (state, mostUrgentLetter.Stakes) switch
        {
            (NPCEmotionalState.DESPERATE, StakeType.REPUTATION) =>
                $"The letter contains {stakesHint}. {urgency} If this isn't delivered, my standing will be ruined.",
                
            (NPCEmotionalState.DESPERATE, StakeType.SAFETY) =>
                $"This is {stakesHint}! {urgency} Lives depend on this reaching {mostUrgentLetter.RecipientName}!",
                
            (NPCEmotionalState.HOSTILE, _) =>
                $"You still haven't delivered my letter about {stakesHint}. {urgency} This negligence is unacceptable.",
                
            (NPCEmotionalState.CALCULATING, StakeType.WEALTH) =>
                $"My letter concerns {stakesHint}. {urgency} There may be profit in swift delivery.",
                
            _ => $"I need this letter delivered. It's about {stakesHint}. {urgency}"
        };
    }
    
    private string GetUrgencyDescription(int deadlineDays)
    {
        return deadlineDays switch
        {
            <= 0 => "It's already overdue!",
            1 => "It must arrive TODAY.",
            2 => "Tomorrow is the absolute latest.",
            <= 3 => "Time is running short.",
            _ => "There's still time, but don't delay."
        };
    }
}
```

### Phase 5: Update ConversationFactory (30 min)

```csharp
public class ConversationFactory
{
    private readonly SystemicNarrativeProvider _systemicProvider;
    private readonly VerbContextualizer _verbContextualizer;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    
    public async Task<ConversationManager> CreateConversation(
        SceneContext context,
        Player player)
    {
        // Populate context with systemic data
        context.AttentionManager = new AttentionManager();
        
        // Calculate NPC state for context
        var npcState = _stateCalculator.CalculateState(context.TargetNPC);
        context.NPCEmotionalState = npcState;
        
        // Find relevant letters
        var relevantLetters = _queueManager.GetActiveLetters()
            .Where(l => l.SenderId == context.TargetNPC.ID)
            .ToList();
        context.RelevantLetters = relevantLetters;
        
        // Create conversation with systemic provider
        var conversationManager = new ConversationManager(
            context,
            state,
            _systemicProvider, // Use systemic instead of AI
            context.GameWorld);
            
        return conversationManager;
    }
}
```

### Phase 6: Update ConversationScreen Integration (30 min)

```csharp
public class ConversationScreenBase : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        // Start actual conversation, not static mockup
        Model = await GameFacade.StartConversationAsync(NpcId);
    }
    
    protected async Task SelectChoice(string choiceId)
    {
        // Process choice and update conversation
        Model = await GameFacade.ContinueConversationAsync(choiceId);
        
        // If conversation complete, navigate away
        if (Model?.IsComplete == true)
        {
            Navigation.NavigateTo("/location");
        }
    }
}
```

### Phase 7: Service Registration (15 min)

```csharp
public static class SystemicUIConfiguration
{
    public static IServiceCollection AddSystemicUI(this IServiceCollection services)
    {
        // Replace AI provider with systemic
        services.AddSingleton<INarrativeProvider, SystemicNarrativeProvider>();
        services.AddSingleton<MechanicalEffectsCalculator>();
        services.AddSingleton<QueueChoiceGenerator>();
        services.AddSingleton<SystemicDialogueGenerator>();
        
        // Keep existing services
        services.AddSingleton<VerbContextualizer>();
        services.AddSingleton<NPCEmotionalStateCalculator>();
        services.AddSingleton<AttentionManager>();
        
        return services;
    }
}
```

## 📊 Implementation Timeline

| Phase | Task | Time | Dependencies |
|-------|------|------|--------------|
| 1 | Create SystemicNarrativeProvider | 2h | None |
| 2 | Implement MechanicalEffectsCalculator | 1h | Phase 1 |
| 3 | Create QueueChoiceGenerator | 1h | Phase 1 |
| 4 | Build SystemicDialogueGenerator | 1h | None |
| 5 | Update ConversationFactory | 30m | Phases 1-4 |
| 6 | Fix ConversationScreen integration | 30m | Phase 5 |
| 7 | Update service registration | 15m | All phases |
| 8 | Testing and debugging | 1h | All phases |

**Total: 7 hours**

## 🎯 Success Criteria

### Must Have (Core Systemic Mechanics)
- [ ] ALL choices generated from verbs + NPC state + queue
- [ ] NO AI generation for basic conversations
- [ ] Mechanical effects visible for every choice
- [ ] Queue manipulation choices available
- [ ] NPC dialogue emerges from letter properties

### Should Have (Polish)
- [ ] Obligation creation/tracking
- [ ] Token burn preview
- [ ] Time cost calculations
- [ ] Route discovery through EXTRACT verb

### Could Have (Future)
- [ ] Complex multi-party negotiations
- [ ] Chain letter effects
- [ ] Weather/time modifiers

## 🧪 Testing Strategy

### Test Case 1: Elena with Urgent Letter
```
Given: Elena has Trust letter, REPUTATION stakes, 1 day deadline
Then: Elena state = DESPERATE
And: Choices include PLACATE, COMMIT, EXTRACT
And: Dialogue mentions "marriage proposal" (from GetStakesHint)
```

### Test Case 2: Hostile NPC
```
Given: Marcus has overdue Commerce letter
Then: Marcus state = HOSTILE  
And: COMMIT locked, only PLACATE and DEFLECT available
And: Dialogue is accusatory about late delivery
```

### Test Case 3: No Letters
```
Given: NPC has no letters in queue
Then: NPC state = WITHDRAWN
And: Only PLACATE available (to engage)
And: Minimal dialogue options
```

## 🚀 Implementation Order

1. **Start with SystemicNarrativeProvider** - Core of the system
2. **Add MechanicalEffectsCalculator** - Make choices meaningful
3. **Implement QueueChoiceGenerator** - Queue manipulation
4. **Create SystemicDialogueGenerator** - Dynamic NPC speech
5. **Wire everything together** - Factory and services
6. **Test with real gameplay** - Verify emergence

## ⚠️ Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Choices feel repetitive | High | Use context multiplication (verb × state × stakes = variety) |
| No narrative variety | Medium | Add personality traits to modify verb presentation |
| Performance issues | Low | Cache NPC states, lazy-load choices |
| Breaking existing saves | High | Keep AI provider as fallback for old conversations |

## 📝 Key Design Decisions

### Why Hidden Verbs?
Players see narrative actions ("Take her hand in comfort") not mechanical verbs (PLACATE). This maintains immersion while having consistent mechanics.

### Why Queue-Driven?
The queue IS the game. All pressure, choices, and consequences flow from letter management. This creates emergent narrative from simple rules.

### Why No AI for Basic Choices?
Deterministic choices are predictable and learnable. Players can master the system. AI only for deep investigation (3 attention cost).

## 🎮 Final Result

When complete, conversations will:
- Generate ALL choices from game state
- Show clear mechanical consequences
- Create narrative from letter properties
- Allow queue manipulation through dialogue
- Feel dynamic without randomness
- Support player mastery through consistency

The mockup's promise of "dynamically generated from systemic mechanics" will finally be TRUE.