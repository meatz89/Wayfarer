# ConversationSubsystem Implementation Context

## CRITICAL REQUIREMENTS
- **NO FALLBACKS** - Complete implementation only
- **NO COMPATIBILITY LAYERS** - Direct migration
- **NO TODOS** - Every method fully implemented
- **NO PLACEHOLDERS** - Real working code only
- **DELETE NOTHING YET** - Keep old code, just delegate to new

## Complete Method Migration Map

### From ConversationManager.cs (833 lines) → Multiple Managers

**Public Methods to Migrate:**
```csharp
// Line 52 → ConversationFacade
public ConversationSession StartConversation(string npcId, ConversationType conversationType, List<CardInstance> observationCards = null)

// Line 295 → ConversationFacade
public void EndConversation()

// Line 327 → ConversationFacade
public bool IsConversationActive()

// Line 333 → ConversationFacade
public ConversationSession GetCurrentSession()

// Line 339 → ConversationFacade
public ConversationOutcome GetLastOutcome()

// Line 432 → ConversationFacade
public List<ConversationAction> GetAvailableActions()

// Line 474 → ConversationFacade
public ConversationTurnResult ProcessAction(ConversationAction action)

// Line 686 → ConversationFacade
public ConversationMemento SaveState()

// Line 713 → ConversationFacade
public void RestoreState(ConversationMemento memento)
```

**Private Methods Distribution:**
```csharp
// To ConversationOrchestrator:
- Line 133: CreateSession()
- Line 345: ProcessListenAction()
- Line 387: ProcessSpeakAction()
- Line 510: ProcessExchangeResponse()
- Line 552: CheckGoalCardUrgency()
- Line 609: FinalizeConversation()

// To CardDeckManager:
- Line 228: CreateConversationDeck()
- Line 272: AddGoalCardToDeck()
- Line 744: DrawCards()
- Line 780: PlayCard()

// To EmotionalStateManager:
- Line 577: ProcessStateTransition()
- Line 621: GetEmotionalStateWeightLimit()
- Line 645: CalculateStateChange()

// To ComfortManager:
- Line 663: UpdateComfort()
- Line 677: CheckComfortThreshold()

// To DialogueGenerator:
- Line 816: GenerateNPCResponse()
```

### From ConversationSession.cs (1,348 lines) → Refactor but Keep

**This file should be REFACTORED, not split:**
- Keep as ConversationSession but simplify
- Move orchestration logic to ConversationOrchestrator
- Move deck operations to CardDeckManager
- Keep as state container and basic operations

**Key Methods to Extract:**
```csharp
// Lines 230-550: Card playing logic → CardDeckManager
// Lines 780-1050: State transition logic → EmotionalStateManager
// Lines 1100-1200: Comfort calculations → ComfortManager
// Lines 1250-1348: Narrative generation → DialogueGenerator
```

### From GameFacade.cs → ConversationFacade

**Conversation Methods:**
```csharp
// Line 2231
public async Task<ConversationContext> CreateConversationContext(string npcId, ConversationType conversationType = ConversationType.FriendlyChat)

// Line 2428
public ConversationTurnResult ProcessConversationTurn(ConversationAction action)

// Line 2486
public void EndCurrentConversation()

// Line 2502
public ConversationStateViewModel GetConversationState()

// Line 950 (Exchange-specific)
public async Task<ExchangeContext> CreateExchangeContext(string npcId)
```

## Required Directory Structure

```
/mnt/c/git/wayfarer/src/Subsystems/Conversation/
├── ConversationFacade.cs         (300+ lines, all public methods)
├── ConversationOrchestrator.cs   (400+ lines, flow control)
├── CardDeckManager.cs            (350+ lines, deck operations)
├── EmotionalStateManager.cs      (250+ lines, state transitions)
├── ComfortManager.cs             (200+ lines, comfort system)
├── DialogueGenerator.cs          (250+ lines, text generation)
├── ExchangeHandler.cs            (200+ lines, exchange system)
└── ConversationStateTracker.cs   (150+ lines, state management)
```

## Dependencies to Inject

### ConversationFacade Constructor:
```csharp
public ConversationFacade(
    GameWorld gameWorld,
    ConversationOrchestrator orchestrator,
    CardDeckManager deckManager,
    EmotionalStateManager stateManager,
    ComfortManager comfortManager,
    DialogueGenerator dialogueGenerator,
    ExchangeHandler exchangeHandler,
    ConversationStateTracker stateTracker,
    // External dependencies:
    NPCRepository npcRepository,
    ObservationSystem observationSystem,
    TimeManager timeManager,
    NPCDeckFactory deckFactory
)
```

## Implementation Requirements

### ConversationFacade.cs (300+ lines)
**MUST Include:**
- StartConversation() - Full implementation
- EndConversation() - Clean up all state
- ProcessAction() - Delegate to orchestrator
- GetCurrentSession() - Return active session
- CreateConversationContext() - Build complete context
- GetAvailableActions() - Determine valid actions
- SaveState() / RestoreState() - Persistence

### ConversationOrchestrator.cs (400+ lines)
**MUST Include:**
- ProcessListenAction() - Handle LISTEN action
- ProcessSpeakAction() - Handle SPEAK action  
- ProcessExchangeResponse() - Handle exchanges
- CheckGoalCardUrgency() - 3-turn rule
- FinalizeConversation() - End conversation properly
- ValidateAction() - Check if action is legal

### CardDeckManager.cs (350+ lines)
**MUST Include:**
- CreateConversationDeck() - Build from templates
- DrawCards() - Draw based on emotional state
- PlayCard() - Execute card effects
- ShuffleDeck() - Randomize deck
- AddGoalCard() - Insert goal into deck
- ManageHandSize() - 7 card limit
- HandlePersistence() - Fleeting vs Persistent

### EmotionalStateManager.cs (250+ lines)
**MUST Include:**
- ProcessStateTransition() - Change states
- GetWeightLimit() - State-based weight limits
- GetDrawCount() - Cards drawn per state
- ValidateStateChange() - Check if transition legal
- GetStateTraits() - State characteristics
- ComfortTriggeredTransition() - At ±3 comfort

### ComfortManager.cs (200+ lines)
**MUST Include:**
- UpdateComfort() - Modify comfort value
- CheckThreshold() - Check for ±3
- ResetComfort() - After state change
- CalculateComfortChange() - Based on card weight
- GetComfortDescription() - Narrative text

### DialogueGenerator.cs (250+ lines)
**MUST Include:**
- GenerateNPCResponse() - Create dialogue
- GenerateActionDescription() - Describe actions
- GenerateStateTransitionText() - State changes
- GenerateCardPlayText() - Card effects
- GenerateEmotionalCues() - Body language

## GameFacade Updates Required

### Constructor Changes:
```csharp
// ADD:
private readonly ConversationFacade _conversationFacade;

// In constructor parameter list, ADD:
ConversationFacade conversationFacade,

// In constructor body, ADD:
_conversationFacade = conversationFacade;
```

### Method Updates:
```csharp
// REPLACE Line 2231-2325:
public async Task<ConversationContext> CreateConversationContext(string npcId, ConversationType conversationType = ConversationType.FriendlyChat)
{
    return await _conversationFacade.CreateConversationContext(npcId, conversationType);
}

// REPLACE Line 2428-2450:
public ConversationTurnResult ProcessConversationTurn(ConversationAction action)
{
    return _conversationFacade.ProcessAction(action);
}

// Continue for all conversation methods...
```

## Validation Requirements

1. **All methods implemented** - No empty bodies
2. **Compiles without errors** - Run `dotnet build`
3. **No TODOs** - Search should find nothing
4. **No NotImplementedException** - Search should find nothing
5. **GameFacade delegates properly** - All conversation methods
6. **ConversationManager still works** - Don't delete yet
7. **UI still functions** - Test conversation in game

## Testing Commands

```bash
# Check all files created
ls -la /mnt/c/git/wayfarer/src/Subsystems/Conversation/

# Check file sizes (must be substantial)
wc -l /mnt/c/git/wayfarer/src/Subsystems/Conversation/*.cs

# Check for TODOs
grep -r "TODO" /mnt/c/git/wayfarer/src/Subsystems/Conversation/

# Check for NotImplementedException
grep -r "NotImplementedException" /mnt/c/git/wayfarer/src/Subsystems/Conversation/

# Compile
cd /mnt/c/git/wayfarer/src && dotnet build

# Test conversation
ASPNETCORE_URLS="http://localhost:5099" timeout 30 dotnet run --no-build
```

## Common Pitfalls to Avoid

1. **Empty Methods** - Every method must work
2. **Null Returns** - Return proper values
3. **Breaking Card Flow** - Maintain draw/play mechanics
4. **State Corruption** - Preserve state integrity
5. **Breaking UI** - Keep exact same API

## Success Criteria

- [ ] All 8 files created with substantial content
- [ ] ConversationManager methods migrated (32 methods)
- [ ] ConversationSession refactored (keep working)
- [ ] GameFacade delegates all conversation calls
- [ ] Compiles without errors
- [ ] Can start conversation in game
- [ ] Can play cards
- [ ] State transitions work
- [ ] Comfort system functions
- [ ] No TODOs or placeholders

## Card System CRITICAL Details

### Card Instance vs Template
- **ConversationCard**: Template (immutable definition)
- **CardInstance**: Runtime instance with unique ID
- **SessionCardDeck**: Manages instances for session
- **HandDeck**: Player's current hand (max 7)

### Persistence Types
- **Fleeting**: Removed from hand on LISTEN
- **Persistent**: Stays in hand
- **Goal**: Must play within 3 turns
- **Opportunity**: One-time use

### Emotional State Effects
```
DESPERATE: Weight 1 max, draws 2
HOSTILE: Weight 0, draws 1 (burden only)
TENSE: Weight 2 max, draws 1
GUARDED: Weight 1 max, draws 1
NEUTRAL: Weight 3 max, draws 2
OPEN: Weight 3 max, draws 2
EAGER: Weight 3 max, draws 2
CONNECTED: Weight 4 max, draws 2
```

This is CRITICAL - get states wrong and conversations break!