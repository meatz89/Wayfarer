# Literary UI Implementation Documentation

## Overview

This document describes the complete implementation plan for Wayfarer's literary UI system, where the letter queue drives all narrative through mechanical properties and AI interpretation.

## Updated: 2025-01-06

### Core System Changes
- Letter-driven narrative (queue IS the story)
- Graduated attention economy (0/1/2 points for depth)
- 4 social navigation verbs (hidden from UI)
- NPC states emerge from letter properties
- All narrative generated from mechanical tags

## Implementation Architecture

### 1. Letter System Implementation

**Location**: `/src/GameState/Letter.cs`

```csharp
public struct Letter {
    public TokenType Type { get; set; }      // Trust|Commerce|Status|Shadow
    public StakeType Stakes { get; set; }    // REPUTATION|WEALTH|SAFETY|SECRET  
    public int Weight { get; set; }          // 1-5 queue slots
    public int TTL { get; set; }             // Days remaining
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
}

public enum StakeType {
    REPUTATION,  // Social consequences
    WEALTH,      // Financial impact
    SAFETY,      // Physical danger
    SECRET       // Hidden information
}
```

### 2. NPC State Calculator

**Location**: `/src/GameState/NPCStateCalculator.cs`

```csharp
public class NPCStateCalculator {
    public NPCEmotionalState CalculateState(NPC npc, LetterQueue queue) {
        var theirLetters = queue.GetLettersFrom(npc.Id);
        if (!theirLetters.Any()) return NPCEmotionalState.WITHDRAWN;
        
        var mostUrgent = theirLetters.OrderBy(l => l.TTL).First();
        
        if (mostUrgent.TTL < 2 || mostUrgent.Stakes == StakeType.SAFETY) {
            return NPCEmotionalState.DESPERATE;
        }
        
        if (queue.HasOverdueLettersTo(npc.Id)) {
            return NPCEmotionalState.HOSTILE;
        }
        
        return NPCEmotionalState.CALCULATING;
    }
}

public enum NPCEmotionalState {
    DESPERATE,    // Urgent need, easier interaction
    HOSTILE,      // Angry, harder interaction
    CALCULATING,  // Balanced, normal interaction
    WITHDRAWN     // No engagement, limited options
}
```

### 3. Attention System (Graduated)

**Location**: `/src/GameState/AttentionManager.cs`

```csharp
public class AttentionManager {
    private int currentPoints = 3;
    public const int MAX_POINTS = 3;
    
    // Graduated focus levels
    public const int PERIPHERAL = 0;  // Free observation
    public const int ENGAGED = 1;     // Basic interaction
    public const int DEEP = 2;        // Complex actions
    
    public bool CanAfford(int cost) => currentPoints >= cost;
    
    public void Spend(int cost) {
        currentPoints = Math.Max(0, currentPoints - cost);
        if (currentPoints == 0) {
            TriggerSceneEnd();
        }
    }
    
    public string GetNarrativeDescription() {
        return currentPoints switch {
            3 => "Your mind is clear and focused",
            2 => "You remain attentive, though some focus spent",
            1 => "Your concentration wavers",
            0 => "Mental fatigue clouds your thoughts",
            _ => ""
        };
    }
}
```

### 4. Verb System (Hidden Mechanics)

**Location**: `/src/GameState/VerbSystem.cs`

```csharp
public enum BaseVerb {
    PLACATE,  // Reduce tension
    EXTRACT,  // Get information
    DEFLECT,  // Redirect pressure
    COMMIT    // Make promises
}

public class VerbContextualizer {
    public string GetNarrativePresentation(
        BaseVerb verb, 
        NPCEmotionalState state, 
        TokenType context,
        int tokenCount) 
    {
        return (verb, context, state) switch {
            (BaseVerb.PLACATE, TokenType.Trust, NPCEmotionalState.DESPERATE) 
                => "Take her trembling hand in comfort",
            (BaseVerb.PLACATE, TokenType.Commerce, NPCEmotionalState.HOSTILE) 
                => "Offer a partial payment to ease tensions",
            (BaseVerb.EXTRACT, TokenType.Trust, _) when tokenCount >= 3
                => "Ask what's really troubling them",
            (BaseVerb.COMMIT, _, NPCEmotionalState.DESPERATE)
                => "Promise to prioritize their letter",
            _ => GetDefaultPresentation(verb)
        };
    }
    
    public int GetAttentionCost(BaseVerb verb, NPCEmotionalState state) {
        var baseCost = 1;
        
        return state switch {
            NPCEmotionalState.DESPERATE => Math.Max(0, baseCost - 1),
            NPCEmotionalState.HOSTILE => Math.Min(2, baseCost + 1),
            _ => baseCost
        };
    }
}
```

### 5. UI Components

#### AttentionDisplay.razor

```razor
@inherits ComponentBase

<div class="attention-display">
    <div class="attention-orbs">
        @for (int i = 0; i < 3; i++)
        {
            <span class="orb @(i < CurrentAttention ? "golden" : "spent")">●</span>
        }
    </div>
    <div class="attention-narrative">
        <em>@AttentionManager.GetNarrativeDescription()</em>
    </div>
</div>

@code {
    [Inject] IAttentionManager AttentionManager { get; set; }
    
    private int CurrentAttention => AttentionManager.CurrentPoints;
}
```

#### PeripheralAwareness.razor

```razor
@inherits ComponentBase

<div class="peripheral-awareness">
    @if (HasDeadlinePressure)
    {
        <div class="deadline-whisper fade-in">
            @GetDeadlineNarrative()
        </div>
    }
    
    @if (HasEnvironmentalHints)
    {
        <div class="environment-hint fade-in">
            @GetEnvironmentNarrative()
        </div>
    }
</div>

@code {
    [Inject] IGameFacade GameFacade { get; set; }
    
    private bool HasDeadlinePressure => 
        GameFacade.GetLetterQueue().Any(l => l.TTL <= 3);
    
    private string GetDeadlineNarrative() {
        var urgent = GameFacade.GetLetterQueue()
            .OrderBy(l => l.TTL)
            .First();
        return $"The weight of {urgent.SenderId}'s letter presses against your ribs";
    }
}
```

#### InternalThoughtChoice.razor

```razor
@inherits ComponentBase

<div class="thought-choices">
    @foreach (var choice in GeneratedChoices)
    {
        <div class="thought-choice @(choice.CanAfford ? "" : "unaffordable")"
             @onclick="() => SelectChoice(choice)">
            <em>@choice.NarrativeText</em>
            @if (choice.AttentionCost > 0)
            {
                <span class="attention-dots">
                    @for (int i = 0; i < choice.AttentionCost; i++)
                    {
                        <span class="dot">●</span>
                    }
                </span>
            }
        </div>
    }
</div>

@code {
    [Parameter] public List<GeneratedChoice> GeneratedChoices { get; set; }
    [Parameter] public EventCallback<GeneratedChoice> OnChoiceSelected { get; set; }
    
    private async Task SelectChoice(GeneratedChoice choice) {
        if (choice.CanAfford) {
            await OnChoiceSelected.InvokeAsync(choice);
        }
    }
}
```

#### BodyLanguageDisplay.razor

```razor
@inherits ComponentBase

<div class="body-language">
    <div class="npc-name">@NPCName</div>
    <div class="emotional-state">
        <em>@GenerateBodyLanguage()</em>
    </div>
</div>

@code {
    [Parameter] public string NPCName { get; set; }
    [Parameter] public NPCEmotionalState State { get; set; }
    [Parameter] public StakeType Stakes { get; set; }
    
    private string GenerateBodyLanguage() {
        return (State, Stakes) switch {
            (NPCEmotionalState.DESPERATE, StakeType.REPUTATION) 
                => "fingers worrying their shawl, eyes darting to the door",
            (NPCEmotionalState.HOSTILE, StakeType.WEALTH) 
                => "arms crossed tight, jaw clenched with suppressed anger",
            (NPCEmotionalState.CALCULATING, StakeType.SECRET) 
                => "measured breathing, each word carefully chosen",
            (NPCEmotionalState.WITHDRAWN, StakeType.SAFETY) 
                => "shoulders hunched, avoiding direct eye contact",
            _ => "watching you with guarded interest"
        };
    }
}
```

### 6. CSS Styling

**Location**: `/src/wwwroot/css/literary-ui.css`

```css
:root {
    --attention-gold: #ffd700;
    --pressure-red: #8b0000;
    --comfort-warm: #d2691e;
    --mystery-purple: #4b0082;
    --shadow-dark: #2c2416;
    --parchment: #fefdfb;
}

/* Attention Display */
.attention-display {
    position: fixed;
    top: 20px;
    left: 50%;
    transform: translateX(-50%);
    text-align: center;
    z-index: 1000;
}

.attention-orbs {
    display: flex;
    gap: 12px;
    justify-content: center;
    margin-bottom: 8px;
}

.orb {
    font-size: 24px;
    transition: all 0.3s ease;
}

.orb.golden {
    color: var(--attention-gold);
    text-shadow: 0 0 10px var(--attention-gold);
}

.orb.spent {
    color: var(--shadow-dark);
    opacity: 0.3;
}

/* Peripheral Awareness */
.peripheral-awareness {
    position: fixed;
    top: 60px;
    right: 20px;
    max-width: 250px;
}

.deadline-whisper,
.environment-hint {
    background: rgba(44, 36, 22, 0.8);
    color: var(--parchment);
    padding: 8px 12px;
    border-radius: 4px;
    margin-bottom: 8px;
    font-size: 12px;
    font-style: italic;
    opacity: 0.9;
}

.fade-in {
    animation: fadeIn 0.5s ease-in;
}

/* Thought Choices */
.thought-choices {
    padding: 20px;
    max-width: 600px;
    margin: 0 auto;
}

.thought-choice {
    background: var(--parchment);
    border-left: 3px solid transparent;
    padding: 12px 16px;
    margin-bottom: 12px;
    cursor: pointer;
    transition: all 0.2s ease;
    position: relative;
}

.thought-choice:hover {
    border-left-color: var(--attention-gold);
    transform: translateX(4px);
}

.thought-choice.unaffordable {
    opacity: 0.5;
    cursor: not-allowed;
}

.attention-dots {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
}

.attention-dots .dot {
    color: var(--attention-gold);
    font-size: 12px;
    margin-left: 4px;
}

/* Body Language */
.body-language {
    text-align: center;
    padding: 20px;
}

.npc-name {
    font-size: 20px;
    font-weight: bold;
    margin-bottom: 8px;
}

.emotional-state {
    color: #6b5d4f;
    font-style: italic;
    font-size: 14px;
}

/* Animations */
@keyframes fadeIn {
    from { opacity: 0; transform: translateY(-10px); }
    to { opacity: 1; transform: translateY(0); }
}
```

## Implementation Plan

### Phase 1: Core Systems (Week 1)
- [ ] Implement Letter struct with all properties
- [ ] Create NPCStateCalculator
- [ ] Update AttentionManager for graduated system
- [ ] Implement VerbContextualizer

### Phase 2: State Management (Week 2)
- [ ] Create LetterQueue management
- [ ] Implement pressure calculation
- [ ] Add consequence system for expired letters
- [ ] Create state persistence

### Phase 3: UI Components (Week 3)
- [ ] Build AttentionDisplay component
- [ ] Create PeripheralAwareness system
- [ ] Implement InternalThoughtChoice
- [ ] Add BodyLanguageDisplay

### Phase 4: AI Integration (Week 4)
- [ ] Create tag generation system
- [ ] Implement narrative generation pipeline
- [ ] Add contextual variation rules
- [ ] Create fallback templates

### Phase 5: Scene Flow (Week 5)
- [ ] Implement scene state machine
- [ ] Add consequence cascade system
- [ ] Create scene transition logic
- [ ] Add pressure escalation

### Phase 6: Testing & Polish (Week 6)
- [ ] Edge case handling
- [ ] Balance testing
- [ ] UI polish and animations
- [ ] Performance optimization

## Critical Implementation Notes

### Never Show These
- Verb names (PLACATE, EXTRACT, etc.)
- Numerical values (pressure = 11)
- Mechanical states (DESPERATE, HOSTILE)
- Token counts as numbers
- Raw stakes labels

### Always Show These
- Narrative descriptions of states
- Body language and emotional cues
- Thoughts as italicized choices
- Golden attention orbs
- Environmental hints as prose

### Key Principles
1. **Letters drive everything** - NPC states emerge from queue
2. **Attention is precious** - 3 points create hard choices
3. **Verbs stay hidden** - Show actions not mechanics
4. **Context creates variety** - Same verb, different meaning
5. **Consequences cascade** - Every choice has ripples

## Testing Checklist

### Core Mechanics
- [ ] Letters generate appropriate NPC states
- [ ] Attention costs modify based on state
- [ ] Verbs present contextually
- [ ] Consequences apply correctly

### UI Presentation  
- [ ] No mechanical text visible
- [ ] Attention displays as golden orbs
- [ ] Choices appear as thoughts
- [ ] Body language replaces state display

### Scene Flow
- [ ] Peripheral awareness always active
- [ ] Focus costs appropriate attention
- [ ] Scene ends at 0 attention
- [ ] Unattended NPCs react

### Narrative Generation
- [ ] AI receives correct tags
- [ ] Generated text feels natural
- [ ] Context influences output
- [ ] Fallbacks work correctly

## Production Estimate

- **Core Systems**: 40 hours
- **State Management**: 40 hours  
- **UI Components**: 40 hours
- **AI Integration**: 40 hours
- **Scene Flow**: 40 hours
- **Testing & Polish**: 40 hours
- **Content Creation**: 60 hours

**Total: 300 hours**

This achieves the literary UI vision through pure mechanical generation, with the letter queue as the narrative engine.