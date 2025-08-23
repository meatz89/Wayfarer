# WAYFARER CONVERSATION SYSTEM - COMPLETE IMPLEMENTATION PLAN

**Created**: 2025-08-22  
**Status**: 🚧 IN PROGRESS  
**Design Doc**: /docs/conversation-system.md  
**UI Mockup**: /UI-MOCKUPS/conversation-screen.html

## 📊 EXECUTIVE SUMMARY

Implementing a complete conversation system with 3 conversation types, 3 deck types per NPC, and full emotional state mechanics. System is 85% complete with core mechanics working. Need to add exchange system, multiple deck types, and missing conversation types.

## 🎯 SUCCESS CRITERIA

- [ ] All 3 conversation types functional (Quick Exchange, Crisis, Standard)
- [ ] 3 deck types per NPC (Exchange, Conversation, Crisis)
- [ ] Perfect information display - all costs/effects visible
- [ ] No infinite resource exploits
- [ ] Letter generation through conversations
- [ ] Full emotional state transitions working
- [ ] Playwright tests passing

## 📋 PHASE 1: EXCHANGE SYSTEM (Priority: HIGH)

### Goals
Implement instant resource trading system with 0 attention cost

### ✅ COMPLETED Tasks (Session 31)
- [x] **1.1 Create ExchangeCard class**
  - Resource cost structure (coins, tokens, health, stamina)
  - Resource reward structure
  - ~~Daily usage tracking~~ NO LIMITS - unlimited use
  - Narrative context mapping
  
- [x] **1.2 Implement QuickExchange conversation type**
  - 0 attention cost ✅
  - No patience system ✅
  - Instant accept/refuse ✅
  - No emotional states ✅
  
- [ ] **1.3 Exchange Implementation (CRITICAL - NOT DONE)**
  - **ONE card per NPC per day** (randomly selected)
  - Card shows even if unaffordable (locked state)
  - No usage limits - unlimited use
  - Need: `TodaysExchangeCard` property on NPC
  - Need: Daily refresh logic at dawn
  - Need: Exchange data in npcs.json
  
- [x] **1.4 Modify ConversationScreen for Exchange mode**
  - Use same ConversationScreen.razor ✅
  - Hide emotional state elements when in Exchange mode ✅
  - Hide patience/comfort bars for exchanges ✅
  - Show simple cost/reward cards ✅
  - Accept/Decline instead of Listen/Speak ✅
  
- [ ] **1.5 Exchange Execution (TODO LEFT IN CODE)**
  - Need: ExecuteExchange method in GameFacade
  - Resource validation (can player afford?)
  - Resource deduction and rewards
  - No tracking needed (unlimited use)

### Files to Create/Modify
```
NEW: /src/Game/ConversationSystem/Core/ExchangeCard.cs
NEW: /src/Game/ConversationSystem/Core/ResourceExchange.cs
MOD: /src/Services/NPCDeckFactory.cs
MOD: /src/Services/ConversationManager.cs
MOD: /src/Pages/ConversationScreen.razor (support Exchange mode)
MOD: /src/Pages/LocationScreen.razor
```

### Design Decisions
- **Refresh**: Cards refresh at dawn (start of day)
- **Limit**: Each card usable once per day
- **Priority**: Exchange available if no crisis

## 📋 PHASE 2: MULTIPLE DECK TYPES (Priority: HIGH)

### Goals
Support 3 separate decks per NPC with proper selection logic

### Tasks
- [ ] **2.1 Extend CardDeck class**
  - Support deck type enum (Exchange, Conversation, Crisis)
  - Lazy initialization for memory efficiency
  - Deck switching logic
  
- [ ] **2.2 Modify NPCDeckFactory**
  - Initialize 3 decks per NPC
  - Load from JSON content
  - Personality-based generation
  
- [ ] **2.3 Update ConversationManager**
  - Deck selection by conversation type
  - Priority system (Crisis > Exchange > Standard)
  - State-based availability
  
- [ ] **2.3 Crisis conversation flow**
  - 1 attention cost
  - 3 patience only
  - Crisis deck exclusive
  - Forces resolution

### Files to Modify
```
MOD: /src/Game/ConversationSystem/Core/CardDeck.cs
MOD: /src/Services/NPCDeckFactory.cs
MOD: /src/Game/ConversationSystem/Managers/ConversationManager.cs
NEW: /src/Game/ConversationSystem/Core/DeckType.cs
MOD: /Content/NPCs/*.json (add deck definitions)
```

### Design Decisions
- **Priority**: Crisis > Exchange > Standard
- **Memory**: Lazy deck initialization
- **Crisis timeout**: Crisis cards expire after 3 time blocks

## 📋 PHASE 3: MISSING CONVERSATION TYPES (Priority: MEDIUM)

### Goals
Implement Standard and Crisis conversation types with proper gating

### Tasks
- [ ] **3.1 Standard Conversation**
  - 3 attention cost
  - 12 patience
  - Relationship level 3+ gate
  - Better comfort/letter rewards
  
- [ ] **3.2 Crisis Resolution**
  - 1 attention cost
  - 3 patience
  - Forced when crisis cards present
  - Blocks other conversations
  
- [ ] **3.3 Conversation selection UI**
  - Show available types
  - Display requirements/costs
  - Visual indicators for locked types
  - Clear mechanical information

### Files to Create/Modify
```
NEW: /src/Game/ConversationSystem/Core/ConversationType.cs
MOD: /src/Game/ConversationSystem/Managers/ConversationManager.cs
MOD: /src/Pages/ConversationScreen.razor
NEW: /src/Pages/Components/ConversationTypeSelector.razor
MOD: /src/Services/GameFacade.cs
```

### Design Decisions
- **Crisis force**: Cannot start other conversations if crisis present
- **Attention refund**: No refunds - spent on attempt

## 📋 PHASE 3: ENHANCED FEATURES (Priority: LOW)

### Goals
Polish and enhance the conversation experience

### Tasks
- [ ] **3.1 Letter delivery integration**
  - Deliver letters through conversation cards
  - Visual feedback for delivery
  - Obligation updates
  
- [ ] **3.2 Set bonus visualization**
  - Show potential bonuses
  - Highlight matching types
  - Animate bonus application
  
- [ ] **3.3 Obligation manipulation**
  - Cards that extend/reduce deadlines
  - Visual preview of changes
  - Confirmation required
  
- [ ] **3.3 Depth advancement**
  - Visual depth indicator
  - Unlock notifications
  - Card availability changes

### Files to Modify
```
MOD: /src/Pages/ConversationScreen.razor
MOD: /src/Pages/Components/CardDisplay.razor
NEW: /src/Pages/Components/SetBonusIndicator.razor
MOD: /src/Game/ConversationSystem/Core/ConversationCard.cs
MOD: /src/Services/ObligationQueueManager.cs
```

## 🧪 TESTING STRATEGY

### Unit Tests
```csharp
// Test files to create
/tests/ExchangeCardTests.cs
/tests/DeckSelectionTests.cs
/tests/ConversationTypeTests.cs
/tests/RefreshMechanicsTests.cs
```

### Playwright E2E Tests
```javascript
// Test scenarios
- Quick Exchange flow (0 attention)
- Crisis forces resolution
- Exchange daily refresh
- Deck switching during conversation
- Set bonus calculation
- Letter delivery through cards
```

### Manual Testing Checklist
- [ ] Launch game without crashes
- [ ] Quick Exchange costs 0 attention
- [ ] Exchange cards refresh at dawn
- [ ] Crisis conversations force resolution
- [ ] Emotional states transition correctly
- [ ] Weight limits enforced
- [ ] Set bonuses calculate properly
- [ ] No infinite resource exploits

## 🚨 CRITICAL DESIGN DECISIONS

### User Clarifications (IMPORTANT)
1. **Conversation Type Selection**: Player CHOOSES conversation type from location screen - NPCs don't force types
2. **Crisis Behavior**: Crisis state LOCKS other conversation options (they become unclickable) but doesn't auto-start
3. **Available Actions**: Location screen shows available conversation types based on NPC's deck composition
4. **Exchange UI**: NOT a separate screen - uses ConversationScreen with simplified display (no emotional states/patience)
5. **Deck-Based Actions**: NPCs might offer only exchange, only standard, or both based on their decks

### From Systems Architect Review
1. **Exchange Refresh Formula**: Daily at dawn, each card once per day
2. **Crisis Resolution**: Added via observations, removed by playing, expire after 3 time blocks
3. **Deck Priority**: Crisis locks other options, not auto-selected
5. **Attention Policy**: No refunds, spent on attempt
6. **Memory Strategy**: Lazy initialization of decks

### From UI/UX Review
1. **Cognitive Load**: Max 3 data points visible per card
2. **Progressive Disclosure**: Details on hover/tap only
3. **Visual Weight**: Blocks instead of numbers
3. **State Visual**: Color/animation not text descriptions
5. **Exchange UI**: Slide-out panel not separate screen
6. **Focus Mode**: Show only essential information by default

### From Narrative Designer Review
1. **Mechanical Generation**: All text from templates + context
2. **No Static Content**: Everything systemically generated
3. **Emotional Truth**: States affect narrative tone
3. **Relationship Evolution**: Through deck changes not numbers

## 📈 PROGRESS TRACKING

### Session 30 (2025-08-22) - ACTUAL PROGRESS
- [x] Created comprehensive implementation plan
- [x] Analyzed existing codebase thoroughly
- [x] Got feedback from all specialized agents
- [x] Learned critical architecture clarifications:
  - Player CHOOSES conversation type from location screen
  - Crisis LOCKS other options (not auto-select)
  - Exchange uses same ConversationScreen (not separate UI)
- [x] Started implementation:
  - Created ExchangeCard.cs with resource exchange system
  - Created ConversationType.cs with all conversation types
  - Fixed HIGHLANDER violation (removed PersonalityArchetype)
  - Modified NPC.cs to support 3 deck types
  - Modified ConversationManager for type selection
  - Modified GameFacade to generate conversation type options
- [⚠️] Left with 7 build errors

### Session 31 (2025-08-22) - BUILD FIXED, EXCHANGE PARTIAL
- [x] Fixed ALL 7 build errors - project now compiles
- [x] Added missing methods:
  - ConversationSession.StartExchange()
  - ConversationSession.StartCrisis()
  - TokenMechanicsManager.GetRelationshipLevel()
  - GameWorld.GetPlayerResourceState()
- [x] Fixed parameter passing issues
- [x] Implemented Exchange UI in ConversationScreen
- [⚠️] LEFT MAJOR TODO: Exchange execution logic
- [❌] NO TESTING DONE - game never launched
- [❌] No exchange data added to npcs.json
- [❌] No crisis templates created

### Session 32 (2025-08-22) - EXCHANGE SYSTEM COMPLETED
- [x] Implemented ExecuteExchange method in GameFacade
- [x] Fixed StartInteraction to support conversation types
- [x] Added ConversationType to InteractionOptionViewModel
- [x] Updated NavigationCoordinator to pass conversation type
- [x] Modified ConversationScreen to use correct conversation type
- [x] Exchange cards are automatically generated from NPC personality
- [x] Tested with Playwright - exchanges work correctly!
- [x] Marcus's labor exchange: 3 stamina → 8 coins VERIFIED WORKING

### Completion Status
```
Phase 1: Exchange System      [███████░░░] 70% ⚠️ Works but violates requirements
Phase 2: Multiple Decks       [████░░░░░░] 40% (structure exists, exchange deck working)
Phase 3: Conversation Types   [██░░░░░░░░] 20% (QuickExchange works, others untested)
Phase 4: Enhanced Features    [░░░░░░░░░░] 0%
Testing: E2E Tests           [██░░░░░░░░] 20% (Only exchange tested, violates core reqs)

Overall:                     [███░░░░░░░] 30% (Major violations found, needs rework)
```

### ⚠️ CRITICAL VIOLATIONS & BUGS FOUND (Session 33)

#### Core Requirement Violations:
- ❌ **ALL DIALOGUE IS HARDCODED** - Not systemically generated
- ❌ **NO JSON TEMPLATES FOR DIALOGUE** - Everything is in switch statements
- ❌ **VIOLATES CORE REQUIREMENT** - "no static content" is completely violated

#### Critical Bugs:
- ❌ **NPC CONVERSATION DECKS NEVER INITIALIZED** - NPCDeckFactory never called during startup
- ❌ **STANDARD CONVERSATIONS IMPOSSIBLE** - ConversationDeck is always null
- ❌ **ONLY QUICKEXCHANGE WORKS** - Because ExchangeDeck has lazy initialization
- ❌ **NO DECK INITIALIZATION IN CONTENT PIPELINE** - Phase3_NPCDependents doesn't initialize decks

#### Untested Features:
- ❌ **CRISIS CONVERSATIONS** - Code exists but can't be tested (no deck)
- ❌ **DEEP CONVERSATIONS** - Code exists but can't be tested (no deck)
- ❌ **STANDARD CONVERSATIONS** - Code exists but can't be tested (no deck)
- ❌ **LETTER GENERATION** - Never tested if letters generate from conversations

### ⚠️ CRITICAL: What's Actually Working
- ✅ Build compiles successfully
- ✅ Exchange system executes trades correctly
- ✅ Quick Exchange conversations working
- ✅ Resource trading verified (stamina → coins)
- ✅ Daily exchange card selection working
- ✅ UI properly shows exchange offers
- ❓ Crisis conversations not tested
- ❓ Deep conversations not tested

## 🔍 EXISTING SYSTEM ANALYSIS

### What's Already Working ✅
- Emotional state system (9 states)
- Card mechanics (weight, comfort, success)
- Basic conversation flow (Listen/Speak)
- Patience system
- Token mechanics
- UI components
- NPC deck management (single deck)
- Deep (Standard) conversations

### What's Missing ❌
- Exchange system entirely
- Multiple deck types
- Crisis conversations
- Conversation type selection
- Daily refresh mechanics
- Set bonus visualization

## 📝 IMPLEMENTATION NOTES

### Architecture Clarifications
- **Location Screen**: Generates conversation actions based on NPC deck availability
- **Action Generation**: Check which decks NPC has, offer those conversation types
- **Crisis Locking**: When crisis deck has cards, other action buttons are disabled/grayed
- **Conversation Screen**: Single screen that adapts based on conversation type
- **Exchange Display**: Hide emotional states, patience, comfort bars for QuickExchange

### Memory Considerations
- 3 decks × 20 cards × 50 NPCs = 3000 cards
- Use lazy initialization
- Clear unused decks after conversations
- Pool card instances where possible

### Performance Optimizations
- Cache deck compositions
- Precompute success rates
- Batch UI updates
- Use CSS animations not JS

### Edge Cases to Handle
- Player spam-clicking exchanges
- Crisis during conversation
- Attention at boundary values
- Deck empty scenarios
- Save/load with active conversation

## 🔴 CRITICAL TODO LEFT IN CODE (Session 32 Priority)

### Exchange Execution (ConversationScreen.razor.cs line 472)
```csharp
// TODO: Implement exchange execution logic
```

This TODO needs:
1. **ExecuteExchange method in GameFacade**:
   - Validate player has resources for costs
   - Deduct costs from player
   - Apply rewards to player
   - Handle token rewards via TokenMechanicsManager
   - Return success/failure

2. **Daily Exchange Selection in NPC**:
   - Add `TodaysExchangeCard` property
   - Refresh at dawn (pick random exchange)
   - Show as locked if unaffordable

3. **Exchange Data in npcs.json**:
   - Define actual exchange cards per NPC
   - Cost/reward pairs
   - Template types for narrative generation

## 🚀 NEXT STEPS (Session 32)

1. **FIRST**: Launch game and test if it even runs
2. **Implement ExecuteExchange**: Complete the TODO
3. **Add TodaysExchangeCard**: Daily selection logic
4. **Add exchange data**: Define in npcs.json
5. **TEST WITH PLAYWRIGHT**: Finally verify it works

## 📊 RISK ASSESSMENT

### High Risk
- Exchange exploit potential (infinite resources)
- Memory pressure from 3 decks per NPC
- UI cognitive overload

### Mitigation
- Daily usage limits on exchanges
- Lazy deck initialization
- Progressive disclosure UI

## ✅ DEFINITION OF DONE

A phase is complete when:
1. All code implemented and compiling
2. Unit tests written and passing
3. Playwright tests written and passing
3. Manual testing completed
5. No known bugs or exploits
6. Documentation updated
7. Progress tracked in this file

---

**Remember**: NEVER mark as complete without testing. Always verify with Playwright before claiming done.