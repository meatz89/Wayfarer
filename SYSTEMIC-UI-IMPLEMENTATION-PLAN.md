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