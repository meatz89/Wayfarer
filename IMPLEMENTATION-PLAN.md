# Wayfarer Conversation System - Implementation Plan

## Current Status: 90% COMPLETE - BUILD SUCCESSFUL - CONVERSATION SYSTEM WORKING
Started: 2025-08-20
Last Updated: 2025-08-20 (Session 3)

## Overview
Complete rewrite of the conversation system as an elegant card-drafting game inspired by Jaipur's strategic mechanics but maintaining emotional authenticity.

## Core Design Principles

### The Key Innovation: Emotional States as Complete Rulesets
Each of the 9 emotional states defines THREE things:
1. **LISTEN effect** - How many cards drawn (1-3)
2. **SPEAK constraint** - Maximum weight allowed (1-4)
3. **LISTEN transition** - State change when listening

This creates Jaipur-like strategic tension through changing game rules rather than literal markets.

### The 9 Emotional States
```
NEUTRAL      - Draw 2, Weight 3, Listen→Neutral
GUARDED      - Draw 1, Weight 2, Listen→Neutral  
OPEN         - Draw 3, Weight 3, Listen→Open
CONNECTED    - Draw 3, Weight 4, Listen→Connected + auto-depth
TENSE        - Draw 1, Weight 1, Listen→Guarded
EAGER        - Draw 3, +3 bonus for sets, Listen→Eager
OVERWHELMED  - Draw 1, Max 1 card only, Listen→Neutral
DESPERATE    - Draw 2+crisis, Crisis free, Listen→Hostile!
HOSTILE      - Cannot converse
```

## Implementation Phases

### Phase 1: Complete Deletion ✅ COMPLETED
- Deleted all CSS files
- Deleted entire /Game/ConversationSystem/ folder
- Deleted ConversationScreen UI files
- Deleted conversation services and content
- Clean slate achieved

### Phase 2: Build New System ✅ COMPLETE
Created from scratch:
```
/src/Game/ConversationSystem/
├── Core/
│   ├── EmotionalState.cs ✅
│   ├── ConversationCard.cs ✅ (with letter delivery & obligation manipulation)
│   ├── CardDeck.cs ✅
│   └── ConversationRules.cs ✅ (merged into EmotionalState.cs)
├── Managers/
│   ├── ConversationManager.cs (next)
│   ├── CardSelectionManager.cs ✅
│   └── StateTransitionManager.cs (may not need separate)
└── Models/
    ├── ConversationSession.cs (next)
    ├── CardPlayResult.cs ✅
    └── ConversationOutcome.cs (next)
```

**Completed Core Components:**
- ✅ EmotionalState with 9 states and complete rulesets
- ✅ ConversationCard with all properties including letter delivery/obligation manipulation
- ✅ CardDeck managing NPC-specific decks based on personality
- ✅ CardSelectionManager enforcing weight limits and combination rules
- ✅ CardPlayResult tracking comfort, bonuses, and state changes

### Phase 3: Create UI Components ✅ COMPLETE
- ✅ ConversationScreen.razor (matching mockup exactly)
- ✅ Dynamic generation from game state
- ✅ Multi-card selection interface
- ✅ Weight tracking display
- ✅ State indicator with effects
- ✅ Letter delivery through conversation support
- ✅ Obligation manipulation support

### Phase 4: CSS Structure
```
/src/wwwroot/css/
├── conversation.css       # Main conversation styles
├── cards.css             # Card display and selection
└── game-base.css         # Foundation styles
```

## Key Mechanics

### Card Selection Rules
- **Single Card**: CAN change emotional state
- **Multiple Cards**: CANNOT change state (express within current state)
- **State Cards**: MUST be played alone
- **Crisis Cards**: MUST be played alone, FREE in DESPERATE

### Set Bonuses (Coherent Expression)
- 2 same type: +2 comfort
- 3 same type: +5 comfort
- 4+ same type: +8 comfort
- EAGER state: Additional +3 for 2+ cards

### Persistence Types
- **Persistent**: Stay in hand when listening
- **Opportunity**: VANISH when listening (fleeting moments)
- **OneShot**: Removed after playing (major confessions)
- **Burden**: Cannot vanish (negative cards)
- **Crisis**: Free in desperate states

### Weight System
Emotional bandwidth represented by weight limits:
- Weight 0: Trivial (nods, small talk)
- Weight 1: Light topics
- Weight 2: Moderate sharing
- Weight 3: Heavy emotional content

### Success Calculation
```
Base 70% - (Weight × 10%) + (Status tokens × 3%)
Min 10%, Max 95%
```

## Letter Delivery & Obligation Manipulation ✅ COMPLETE

### Letter Delivery Through Conversation
- ConversationCard.CanDeliverLetter property enables personal delivery
- ConversationManager.DeliverLetterThroughConversation() handles delivery
- NPCDeckFactory generates "I have your letter right here" cards
- Delivers letter at position 1 to current NPC

### Obligation Manipulation Types
Created ObligationManipulationType enum with 6 types:
1. **Prioritize** - Move to position 1
2. **BurnToClear** - Spend tokens to clear path 
3. **Purge** - Remove using tokens
4. **ExtendDeadline** - Pay for more time
5. **Transfer** - Change recipient
6. **Cancel** - High relationship requirement

### Implementation
- ObligationQueueManager.ManipulateObligation() handles all types
- Token costs vary by manipulation type (2-4 tokens)
- Cancel requires 10+ total tokens (relationship gate)
- ConversationManager.ExecuteObligationManipulation() connects to UI

## Technical Decisions

### Why Complete Rewrite?
1. **Clean Architecture**: No legacy code to work around
2. **Exact Implementation**: Direct from design doc
3. **Faster Development**: ~16 hours vs 20+ for refactoring
4. **Simpler Testing**: No compatibility concerns
5. **Better Performance**: Optimized from start

### Key Differences from Old System
- States define complete rulesets (not just modifiers)
- Multi-card selection with weight limits
- Set bonuses for same-type cards
- State changes structurally prevented from conflicts
- Crisis escalation path (DESPERATE→HOSTILE)

## Testing Strategy
- Unit tests for state transitions
- Integration tests for card selection
- Playwright E2E for UI interactions
- Balance testing for all 9 states

## Success Metrics
✅ UI matches mockup exactly but dynamically generated
✅ 9 emotional states with complete rulesets
⬜ Multi-card selection with weight limits
⬜ Set bonuses working correctly
⬜ State changes only on single cards
⬜ Opportunities vanish on LISTEN
⬜ Crisis cards free in DESPERATE
⬜ All values from game state (not hardcoded)

## Implementation Status Update

### ✅ COMPLETED (2025-08-20)
1. ✅ Deleted ALL old CSS files  
2. ✅ Deleted entire old conversation system
3. ✅ Deleted AI narrative system (incompatible with new design)
4. ✅ Created new conversation core models:
   - EmotionalState.cs with 9 states and rulesets
   - ConversationCard.cs with all properties
   - CardDeck.cs for NPC-specific decks
   - NPCDeckFactory.cs for card generation
   - CardSelectionManager.cs for weight/combination rules
   - ConversationManager.cs coordinating everything
5. ✅ Built new ConversationScreen.razor matching mockup
6. ✅ Created new CSS structure (game-base.css, conversation.css, cards.css)
7. ✅ Removed old conversation references from GameFacade
8. ✅ Created SceneContext for literary UI system
9. ✅ Fixed ServiceConfiguration to use new system

### ✅ COMPLETED (2025-08-20 Continued)
- ✅ Deleted IMechanicalEffect.cs entirely (no stubs)
- ✅ Removed ConvertMechanicalEffectsToDisplay from GameFacade
- ✅ Removed ALL old conversation methods from GameFacade
- ✅ REMOVED ALL NAMESPACES (except Wayfarer.Pages for Blazor)
- ✅ Simplified entire codebase structure
- Compilation errors: 858 → 206 → 582 → 252 (structural issues)

### ✅ LETTER & OBLIGATION FEATURES IMPLEMENTED
1. ✅ **Letter Delivery Cards**:
   - `CardType.Letter` enum value added
   - `CanDeliverLetter` property on cards
   - `DeliveryLetterTemplate` for personal delivery dialogue
   - `CreateLetterDelivery()` factory method
   
2. ✅ **Obligation Manipulation Cards**:
   - `CardType.Obligation` enum value added
   - `ManipulatesObligations` property on cards
   - `ObligationManipulationType` enum with 6 types:
     - Negotiate (change terms)
     - Transfer (pass to another NPC)
     - Cancel (request cancellation)
     - Expedite (faster completion)
     - Delay (more time)
     - Clarify (get information)
   - `CreateObligationDiscussion()` factory method

### ✅ GAMEWORLD & CONVERSATION INTEGRATION COMPLETE

**GameWorld Rebuilt**:
- Single source of truth for ALL game state
- NO dependencies (managers depend on GameWorld)
- Conversation state stored directly in GameWorld
- NPCConversationState persists across sessions
- ConversationSession for active conversations

**ConversationManager Features**:
- ✅ Full DI (no new() calls)
- ✅ Letter delivery through conversation (DeliverLetterThroughConversation method)
- ✅ Obligation manipulation (6 types: Negotiate, Transfer, Cancel, Expedite, Delay, Clarify)
- ✅ Automatic card generation for letters/obligations
- ✅ State-based rule enforcement
- ✅ Set bonuses and special state effects
- ✅ Attention management integration

### 📋 NEXT STEPS
1. Fix remaining 44 compilation errors (down from 166)
2. Test with Playwright
3. Balance emotional state transitions
4. Polish UI interactions

## Session 2 Accomplishments (2025-08-20)
- ✅ Fixed 122 compilation errors (73% reduction)
- ✅ Converted Location properties to enums
- ✅ Removed all legacy conversation system code
- ✅ Fixed service dependencies and DI issues
- ✅ Added missing methods to core systems
- ✅ GameWorld now has NPCs collection
- ✅ Full card-based conversation integration

## Session 3 Accomplishments (2025-08-20 Evening)
- ✅ Fixed all remaining 44 compilation errors
- ✅ BUILD NOW SUCCESSFUL (0 errors, 10 warnings)
- ✅ Fixed NPC ID passing through NavigationCoordinator
- ✅ Fixed ConversationManager to inject TokenMechanicsManager
- ✅ Fixed CardDeck initialization - cards now populate
- ✅ Fixed ConversationScreen error handling
- ✅ Fixed ConversationManager singleton issue
- ✅ Fixed CSS for conversation UI containers
- ✅ Cards display properly (3 cards in hand)
- ✅ Conversation system tested and working with Garrett NPC
- ✅ LISTEN/SPEAK actions functional
- ✅ Turn counter and emotional states working

## Session 4 Progress (2025-08-20 Late Evening)

### COMPLETED MECHANICS
1. **Emotional State-Based Mechanics** ✅
   - States already manipulate conversation mechanics via StateRuleset
   - Each state defines complete ruleset (draw count, weight limits, transitions)
   - State transitions working through ConversationRules
   - Crisis states (DESPERATE) create urgency and special rules

2. **Categorical Card Generation** ✅
   - Refactored to use CardTemplateType enum instead of text
   - CardContext provides strongly-typed categorical data
   - Personality type determines card pool via NPCDeckFactory
   - Frontend will map templates to actual narrative text
   - No text generation in backend - only categorical models

3. **Dice Rolling System** ✅
   - Already implemented in CardSelectionManager.PlaySelectedCards()
   - Uses card.CalculateSuccessChance() method on ConversationCard
   - Success chance = Base 70% - (Weight × 10%) + (Status tokens × 3%)
   - Clamped 10% min, 95% max in ConversationCard
   - Each card rolls individually with results tracked
   - SingleCardResult tracks roll, success, and comfort gained

4. **Card Effect Application** ✅
   - Comfort application working
   - State changes working for single cards
   - Set bonuses calculated
   - Letter delivery and obligation flags set
   - Dice rolling implemented with success/failure

5. **Categorical Card System** ✅
   - Removed all hardcoded text from backend
   - Created CardTemplateType enum for categorical templates
   - Created CardContext for strongly-typed context data
   - Frontend CardTextRenderer maps templates to narrative text
   - All NPCDeckFactory and CardDeck refactored to use templates
   - Backend provides only categorical data, frontend generates text

## FULL IMPLEMENTATION PLAN - EXACT UI WITH SYSTEMATIC CONTENT

### Phase 1: Emotional State Mechanics System ⚡ PRIORITY
**Create state-driven conversation mechanics**
```
/src/Game/ConversationSystem/StateEffects/
├── StateEffectProcessor.cs - Apply state rules to mechanics
├── StateTransitionRules.cs - Define valid state transitions
└── CrisisEscalation.cs - Handle desperate→hostile path
```
- States manipulate: draw count, weight limits, transitions
- DESPERATE: Crisis cards free, draws crisis cards, escalates to HOSTILE
- CONNECTED: Auto-depth progression, max weight 4
- TENSE: Weight limit 1, minimal draw
- Each state = complete ruleset, not modifiers

### Phase 2: Dynamic Card Generation System ⚡ PRIORITY
**Generate cards based on NPC state and context**
```
/src/Game/ConversationSystem/Generation/
├── CardTextGenerator.cs - Generate dynamic card text
├── PersonalityCardPool.cs - Personality-specific cards
├── StateCardGenerator.cs - State-based card creation
└── ObservationCardBuilder.cs - Convert observations to cards
```
- NPCDeckFactory enhanced with:
  - Emotional state awareness
  - Personality-driven text
  - Context-sensitive cards (deadlines, letters)
  - Observation integration
  - Dynamic crisis cards

### Phase 3: Dice Rolling & Success System ⚡ PRIORITY
**Implement success/failure mechanics**
```
/src/Game/ConversationSystem/Resolution/
├── DiceRoller.cs - Core rolling mechanics
├── SuccessCalculator.cs - Calculate success chances
└── OutcomeResolver.cs - Apply results to game state
```
- Formula: 70% - (Weight × 10%) + (Status tokens × 3%)
- Clamped: 10% min, 95% max
- Visual feedback in UI
- Multiple cards = multiple rolls
- Track individual card results

### Phase 4: Effect Application System ⚡ PRIORITY
**Apply card effects to game state**
```
/src/Game/ConversationSystem/Effects/
├── ComfortEffectHandler.cs - Comfort gains/losses
├── StateChangeHandler.cs - Emotional state transitions
├── LetterDeliveryHandler.cs - Handle letter mechanics
└── TokenEffectHandler.cs - Relationship token changes
```
- Success effects: comfort, tokens, state changes
- Failure effects: reduced gains, negative states
- Special effects: letter delivery, obligations
- Depth progression on thresholds

### Phase 5: Narrative Generation System
**Create contextual narrative and dialogue**
```
/src/GameState/Narrative/
├── ConversationNarrativeGenerator.cs - Scene narrative
├── NPCDialogueTemplates.cs - Personality dialogues
├── StateNarrativeLibrary.cs - State-specific text
└── UrgencyDialogueSystem.cs - Deadline-aware dialogue
```
- Generate based on:
  - Current emotional state
  - NPC personality type
  - Active deadlines
  - Recent player actions
  - Location context

### Phase 6: UI Implementation - Match Mockups EXACTLY
**Update UI to match HTML mockups precisely**

**ConversationScreen Updates:**
- Exact div structure from mockup
- Progress bars with thresholds
- Card display with outcomes
- Weight tracker
- State effects display
- Dice roll feedback

**LocationScreen Updates:**
- Atmosphere text section
- NPC cards with states
- Observation opportunities
- Area navigation
- Action grid

**New Components:**
- TravelSelectionModal.razor
- TravelEncounterScreen.razor
- CardOutcomeDisplay.razor
- DiceRollAnimation.razor

### Phase 7: CSS Alignment
**Copy exact styles from mockups**
- conversation.css - All card styles, outcomes, states
- location.css - NPCs, observations, areas
- game-base.css - Consistent base styles
- animations.css - Dice rolls, state transitions

### Phase 8: Content Template System
**Systematic content generation**
```
/src/Content/Generation/
├── ContentTemplateSystem.cs - Template management
├── VariationEngine.cs - Generate varied content
├── PersonalityVoice.cs - NPC-specific speech
└── ContextualContent.cs - Situation-aware text
```

### Implementation Order:
1. **State Mechanics** - Foundation for everything
2. **Card Generation** - Creates conversation content
3. **Dice System** - Determines outcomes
4. **Effect Application** - Updates game state
5. **Narrative Generation** - Creates text
6. **UI Updates** - Display everything
7. **CSS Alignment** - Visual fidelity
8. **Content Templates** - Variety and depth

## Timeline
- Phase 1: ✅ Complete (30 min)
- Phase 2: ✅ Complete (8 hours actual)
- Phase 3: ✅ Complete (4 hours actual)
- Phase 4: In Progress - Critical mechanics implementation
- Testing: In Progress - Basic functionality confirmed

**Total Progress: ~60% complete - CORE WORKING, MECHANICS NEEDED**

## Notes
- Using emotional states as rulesets is the key innovation
- This is already a card-drafting system, not a forced adaptation
- Maintains emotional authenticity while adding strategic depth
- Binary choices (LISTEN/SPEAK) create clear decision points
- Weight limits force meaningful expression choices