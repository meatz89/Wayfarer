# ConversationSubsystem Analysis Context

## CRITICAL: What You MUST Deliver

You are analyzing the conversation system for COMPLETE migration to a ConversationSubsystem.
You MUST provide:
1. EXACT line numbers for EVERY method
2. Complete method signatures
3. Actual grep output as proof
4. Dependencies and relationships
5. UI component usage

## Files You MUST Analyze Completely

### Core Conversation Files:
1. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Managers/ConversationManager.cs` (833 lines)
2. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Models/ConversationSession.cs` (1,348 lines)
3. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Models/ConversationContext.cs`
4. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Models/ConversationOutcome.cs`

### Card System Files:
5. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/CardDeck.cs` (483 lines)
6. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/HandDeck.cs` (94 lines)
7. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/SessionCardDeck.cs` (274 lines)
8. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/CardInstance.cs` (107 lines)
9. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/ConversationCard.cs` (229 lines)

### State Management:
10. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/EmotionalState.cs` (199 lines)
11. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/ConversationType.cs` (65 lines)

### Exchange System:
12. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/ExchangeMemory.cs` (99 lines)
13. `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/ExchangeNarrative.cs` (193 lines)

### GameFacade Conversation Methods:
14. `/mnt/c/git/wayfarer/src/Services/GameFacade.cs` - Find ALL conversation-related methods

### UI Components:
15. `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor.cs` (1,784 lines)
16. `/mnt/c/git/wayfarer/src/Pages/ConversationScreen.razor.cs` (719 lines)

## Required Analysis Output

### 1. ConversationManager.cs Analysis (833 lines)
You MUST provide:
- Complete list of ALL public methods with line numbers
- Complete list of ALL private methods with line numbers
- Dependencies (what it calls)
- State it manages

### 2. ConversationSession.cs Analysis (1,348 lines)
You MUST provide:
- ALL public properties with line numbers
- ALL public methods with line numbers
- ALL private methods with line numbers
- State management methods
- Card handling methods
- Turn processing methods

### 3. Card System Analysis
For EACH card-related file:
- How cards are created
- How cards are drawn
- How cards are played
- How decks are managed
- Instance vs Template pattern

### 4. GameFacade Conversation Methods
Find and list with line numbers:
- StartConversation methods
- EndConversation methods
- ProcessConversationTurn methods
- GetConversationState methods
- Any other conversation methods

### 5. Dependencies Map
Show what calls what:
- GameFacade → ConversationManager
- ConversationManager → ConversationSession
- ConversationSession → Card System
- UI → GameFacade

## Validation Commands You Must Run

```bash
# Count methods in ConversationManager
grep -E "public|private|protected.*\(" /mnt/c/git/wayfarer/src/Game/ConversationSystem/Managers/ConversationManager.cs | wc -l

# Count methods in ConversationSession  
grep -E "public|private|protected.*\(" /mnt/c/git/wayfarer/src/Game/ConversationSystem/Models/ConversationSession.cs | wc -l

# Find conversation methods in GameFacade
grep -n "Conversation" /mnt/c/git/wayfarer/src/Services/GameFacade.cs | head -20

# Check card system files
ls -la /mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/*.cs
```

## Expected Deliverables

1. **Method Inventory**: 
   - ConversationManager: ~30-40 methods
   - ConversationSession: ~50-60 methods
   - GameFacade: ~10-15 conversation methods

2. **Categorization for New Architecture**:
   - Methods for ConversationOrchestrator
   - Methods for CardDeckManager
   - Methods for EmotionalStateManager
   - Methods for ComfortManager
   - Methods for DialogueGenerator

3. **Migration Map**:
   - What goes to ConversationFacade
   - What goes to internal managers
   - What can be deleted
   - What must be preserved

## Red Flags (Your work will be REJECTED if):
- No line numbers provided
- Vague descriptions like "handles cards"
- No grep output shown
- Methods missing from inventory
- Claims without proof

## Success Criteria
- EVERY method accounted for
- EXACT line numbers for everything
- Clear categorization for migration
- Complete dependency map
- All 2,181+ lines analyzed