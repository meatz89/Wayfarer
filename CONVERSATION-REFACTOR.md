# ConversationFacade Refactoring Plan

## Current Problem
ConversationFacade has grown to 1,815 lines after absorbing ConversationOrchestrator and CardDeckManager functionality during the SCORCHED EARTH refactoring. It now violates single responsibility principle by handling too many concerns.

## Existing Classes to Leverage
- **AtmosphereManager** (263 lines) - Handles atmosphere state and effects
- **CategoricalEffectResolver** (404 lines) - Pure projection functions for effect calculations
- **ExchangeHandler** (379 lines) - Handles exchange mechanics
- **FlowManager** (249 lines) - Manages flow battery and state transitions
- **FocusManager** (125 lines) - Manages focus pool
- **PersonalityRuleEnforcer** (199 lines) - Enforces NPC personality rules
- **RapportManager** (123 lines) - Tracks rapport changes
- **StrangerConversationManager** (230 lines) - Manages stranger conversations

## Refactoring Strategy

### 1. Create ConversationDeckBuilder (NEW CLASS)
Extract from ConversationFacade lines 1066-1686:
- `CreateConversationDeck()` - Main deck creation method
- `GetUnlockedProgressionCards()` - Token-based card unlocking
- `SelectGoalCardsForConversationType()` - Goal card selection logic
- `SelectConnectionTokenGoalCard()` - Token goal cards
- `SelectBurdenResolutionCard()` - Burden resolution cards

### 2. Expand SessionCardDeck
Move card action logic directly to the deck (lines 1132-1499):
- `ExecuteListenAction()` - Already has DrawToHand(), add LISTEN logic
- `RemoveImpulseCardsFromHand()` - Already has ExhaustFromHand()
- `ExhaustOpeningCards()` - Opening card exhaustion
- `UpdateCardPlayabilityBasedOnFocus()` - Playability updates

### 3. Expand RapportManager
Move rapport and goal card management (lines 1370-1411, 1689-1735):
- `UpdateGoalCardPlayabilityAfterListen()` - Rapport threshold checking
- `UpdateRequestCardPlayability()` - Request card state
- Goal card selection methods (already identified in #1)

### 4. Move to ObligationQueueManager
Extract letter generation logic (lines 946-1024):
- `ShouldGenerateLetter()` - Letter generation conditions
- `CreateLetterObligation()` - Standard letter creation
- `CreateUrgentLetter()` - Urgent letter creation

### 5. Expand FlowManager
Move conversation finalization (lines 855-941):
- `ShouldEndConversation()` - End conditions
- `FinalizeConversation()` - Calculate outcome
- `CalculateTokenReward()` - Token calculations

### 6. Expand CategoricalEffectResolver
Move card play mechanics (lines 1162-1365, 1504-1540):
- `PlayCard()` core logic - Keep as projections
- `ExecuteExhaustEffect()` - Convert to projection
- Hidden momentum system
- Success determination with pre-rolls

### 7. Clean ConversationFacade
After extraction, ConversationFacade should only:
- Manage current session
- Coordinate between managers
- Provide public API methods
- Handle session lifecycle

## Implementation Order

1. **Create ConversationDeckBuilder** - New file with deck creation logic
2. **Expand SessionCardDeck** - Add card action methods
3. **Expand RapportManager** - Add goal card management
4. **Expand ObligationQueueManager** - Add letter generation
5. **Expand FlowManager** - Add finalization logic
6. **Expand CategoricalEffectResolver** - Add play mechanics as projections
7. **Update ConversationFacade** - Remove extracted code, add delegations
8. **Update ServiceConfiguration** - Register ConversationDeckBuilder
9. **Test with Playwright** - Verify conversation flow works

## Expected Result
- ConversationFacade reduced from 1,815 to ~400 lines
- Each class has single, clear responsibility
- No new abstraction layers (except ConversationDeckBuilder)
- Leverages existing manager pattern
- Maintains PROJECTION PRINCIPLE for effects
- Easier to test and maintain

## Critical Notes
- Follow SCORCHED EARTH principle - no compatibility layers
- Maintain HIGHLANDER principle - one source of truth for card state
- Keep PROJECTION PRINCIPLE - CategoricalEffectResolver only returns projections
- Test thoroughly - conversations are core gameplay