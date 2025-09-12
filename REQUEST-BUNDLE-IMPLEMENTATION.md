# Request Bundle Implementation Plan

## Overview
The current implementation only loads a single card from an NPCRequest instead of the entire bundle. This document outlines the changes needed to properly implement the Request bundle system where ALL cards from a selected Request are loaded into the conversation.

## Current Architecture Issues

### 1. Single Card Loading
**File**: `CardDeckManager.cs`
**Line**: 522-524
```csharp
// For now, return the first card from the request
// Later this will load ALL cards from the request  
var firstCard = request.RequestCards.FirstOrDefault() ?? request.PromiseCards.FirstOrDefault();
```
**Problem**: Only returns ONE card instead of the entire Request bundle.

### 2. Single Goal Card Expectation
**File**: `ConversationOrchestrator.cs`
**Line**: 64, 121-124
```csharp
(SessionCardDeck deck, CardInstance goalCard) = _deckManager.CreateConversationDeck(...);
// Later...
if (goalCard != null)
{
    session.ActiveCards.Add(goalCard);
}
```
**Problem**: Expects and handles only a single goal card, not multiple request cards.

### 3. No Promise Card Separation
**Current Flow**: Single card added to active pile
**Required Flow**: 
- Request cards → Active pile (visible but unplayable until rapport threshold met)
- Promise cards → Draw pile (shuffled with conversation cards)

## Target Architecture

### Request Bundle Structure
```
NPCRequest
├── RequestCards[] (Multiple rapport thresholds)
│   ├── Basic (5 rapport) → Active pile
│   ├── Enhanced (10 rapport) → Active pile  
│   └── Premium (15 rapport) → Active pile
└── PromiseCards[] (Queue manipulation)
    ├── Standard Promise (+5 rapport) → Draw pile
    └── Urgent Promise (+10 rapport) → Draw pile
```

### Conversation Flow
1. Player selects Request conversation option
2. System loads the ENTIRE Request bundle
3. Promise cards added to deck BEFORE shuffling
4. Deck shuffled (now includes promise cards)
5. ALL request cards added to active pile as unplayable
6. Initial hand drawn from shuffled deck
7. During play:
   - Request cards become playable when rapport threshold reached
   - Promise cards can be drawn and played normally

## Implementation Changes

### Step 1: Update CardDeckManager.CreateConversationDeck()

**Current Signature**:
```csharp
public (SessionCardDeck deck, CardInstance requestCard) CreateConversationDeck(...)
```

**New Signature**:
```csharp
public (SessionCardDeck deck, List<CardInstance> requestCards) CreateConversationDeck(...)
```

**Changes**:
1. Return List<CardInstance> instead of single CardInstance
2. When Request ID provided:
   - Load ALL cards from the Request
   - Add promise cards to the deck for shuffling
   - Return request cards separately for active pile

### Step 2: Update SelectGoalCardForConversationType()

**Current Implementation**:
```csharp
var firstCard = request.RequestCards.FirstOrDefault() ?? request.PromiseCards.FirstOrDefault();
if (firstCard != null)
{
    CardInstance goalInstance = new CardInstance(firstCard, npc.ID);
    // ... setup single card
    return goalInstance;
}
```

**New Implementation**:
```csharp
List<CardInstance> requestInstances = new List<CardInstance>();

// Add ALL request cards to be returned for active pile
foreach (var requestCard in request.RequestCards)
{
    CardInstance instance = new CardInstance(requestCard, npc.ID);
    instance.Context = new CardContext { RapportThreshold = requestCard.RapportThreshold };
    instance.Properties.Add(CardProperty.Unplayable);
    requestInstances.Add(instance);
}

// Add promise cards to the deck for shuffling (not returned)
foreach (var promiseCard in request.PromiseCards)
{
    CardInstance instance = new CardInstance(promiseCard, npc.ID);
    deck.AddCard(instance); // Add to deck for shuffling
}

return requestInstances; // Return all request cards for active pile
```

### Step 3: Update ConversationOrchestrator.CreateSession()

**Current Implementation**:
```csharp
(SessionCardDeck deck, CardInstance goalCard) = _deckManager.CreateConversationDeck(...);
// ...
if (goalCard != null)
{
    session.ActiveCards.Add(goalCard);
}
```

**New Implementation**:
```csharp
(SessionCardDeck deck, List<CardInstance> requestCards) = _deckManager.CreateConversationDeck(...);
// ...
if (requestCards != null && requestCards.Any())
{
    session.ActiveCards.AddRange(requestCards); // Add ALL request cards
}
```

### Step 4: Update Request Card Playability Check

Ensure `UpdateRequestCardPlayability()` checks rapport threshold for ALL request cards in active pile, not just one.

## Testing Plan

### Test Case 1: Elena's Letter Request
1. Start game
2. Navigate to Elena
3. Verify "Available Requests" conversation option appears
4. Select the option
5. Verify:
   - Multiple request cards visible in active pile (3 different thresholds)
   - Promise cards NOT visible initially (in draw pile)
   - Request cards show as unplayable initially
   - Can draw promise cards through LISTEN action
   - Request cards become playable when rapport threshold reached

### Test Case 2: Promise Card Functionality
1. During Request conversation, draw until promise card appears
2. Play promise card
3. Verify:
   - Rapport increases immediately
   - Queue manipulation occurs (if applicable)
   - Can now reach higher request card thresholds

### Test Case 3: Multi-Threshold Goals
1. Start Request conversation
2. Build rapport naturally
3. Verify each request card becomes playable at its threshold:
   - Basic at 5 rapport
   - Enhanced at 10 rapport  
   - Premium at 15 rapport
4. Complete any card to finish the request

## Success Criteria

✅ Request conversation loads ALL cards from Request bundle
✅ Request cards appear in active pile (unplayable initially)
✅ Promise cards shuffled into draw pile
✅ Request cards become playable at rapport thresholds
✅ Promise cards provide rapport boost when played
✅ Completing ANY request card marks Request as complete
✅ UI displays all cards correctly

## Files to Modify

1. `/mnt/c/git/wayfarer/src/Subsystems/Conversation/CardDeckManager.cs`
   - Update CreateConversationDeck() signature and implementation
   - Modify SelectGoalCardForConversationType() to return list
   - Add promise cards to deck, return request cards separately

2. `/mnt/c/git/wayfarer/src/Subsystems/Conversation/ConversationOrchestrator.cs`
   - Update CreateSession() to handle multiple request cards
   - Add all request cards to active pile

3. `/mnt/c/git/wayfarer/src/Subsystems/Conversation/ConversationFacade.cs`
   - May need updates to StartConversation() if signature changes

## Risk Assessment

- **Low Risk**: Changes are localized to conversation initialization
- **No Breaking Changes**: Existing conversations without Requests continue to work
- **Backward Compatible**: Single goal card scenarios still function

## Timeline

1. ✅ Analysis and Planning (Complete)
2. ✅ Implementation (Complete)
   - ✅ Update CardDeckManager
   - ✅ Update ConversationOrchestrator
   - ✅ Fix deck shuffling order
   - ⏳ Test with Elena's scenario
3. ⏳ Testing and Validation (In Progress)
4. ⏳ Documentation Updates (In Progress)

## Implementation Details

### Changes Made

#### CardDeckManager.cs
- ✅ Changed return type from single `CardInstance` to `List<CardInstance>`
- ✅ Renamed method to `SelectGoalCardsForConversationType` (plural)
- ✅ Implemented loading ALL cards from Request bundle:
  - Request cards added to returned list for active pile
  - Promise cards added directly to deck for shuffling
- ✅ Updated fallback logic to return lists instead of single cards

#### ConversationOrchestrator.cs
- ✅ Updated to receive `List<CardInstance> requestCards`
- ✅ Changed to add ALL request cards to active pile with `AddRange()`
- ✅ Comments updated to reflect new bundle behavior

#### SessionCardDeck.cs
- ✅ Removed automatic shuffling in `CreateFromTemplates`
- ✅ Made `ShuffleDrawPile` public so it can be called after promise cards added
- ✅ Deck now shuffled in CardDeckManager AFTER all cards (including promise cards) are added

## Implementation Flow

### When a Request conversation is selected:

1. **CardDeckManager.CreateConversationDeck()**
   - Creates SessionCardDeck from conversation cards (no shuffle)
   - Adds observation cards if any
   - Calls SelectGoalCardsForConversationType()

2. **SelectGoalCardsForConversationType()**
   - Finds the specific Request by ID
   - Creates CardInstances for ALL request cards → returns for active pile
   - Creates CardInstances for ALL promise cards → adds to deck
   - Promise cards now in draw pile (unseen)
   - Request cards returned separately

3. **Deck Shuffling**
   - Deck.ShuffleDrawPile() called AFTER promise cards added
   - Ensures promise cards are mixed into draw pile

4. **ConversationOrchestrator.CreateSession()**
   - Receives List<CardInstance> of request cards
   - Adds ALL request cards to active pile (visible but unplayable)
   - Draws initial hand from shuffled deck (may include promise cards)