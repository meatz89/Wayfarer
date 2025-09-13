# GameWorld Single Source of Truth Refactoring Plan

## Problem Statement
NPCRequest currently stores full ConversationCard objects, violating the core architectural principle that GameWorld must be the single source of truth. This creates duplicate data storage and potential inconsistencies.

## Current Architecture (WRONG)
```
NPCRequest
├── List<ConversationCard> RequestCards  // WRONG: Stores full objects
└── List<ConversationCard> PromiseCards  // WRONG: Stores full objects

PackageLoader:
- Retrieves cards from GameWorld.AllCardDefinitions
- Adds full card objects to NPCRequest lists

CardDeckManager:
- Reads full cards directly from NPCRequest
- Creates CardInstances from these duplicated cards
```

## Target Architecture (CORRECT)
```
NPCRequest
├── List<string> RequestCardIds  // CORRECT: Store only IDs
└── List<string> PromiseCardIds  // CORRECT: Store only IDs

PackageLoader:
- Validates card IDs exist in GameWorld.AllCardDefinitions
- Adds only IDs to NPCRequest lists

CardDeckManager:
- Reads card IDs from NPCRequest
- Retrieves actual cards from GameWorld.AllCardDefinitions
- Creates CardInstances from the single source of truth
```

## Implementation Steps

### Step 1: Update NPCRequest.cs
- Change `List<ConversationCard> RequestCards` to `List<string> RequestCardIds`
- Change `List<ConversationCard> PromiseCards` to `List<string> PromiseCardIds`
- Update `GetAllCards()` method to resolve IDs from GameWorld
- Add helper method to resolve card IDs to cards

### Step 2: Update PackageLoader.cs (Lines 657-687)
- Change from `request.RequestCards.Add(card)` to `request.RequestCardIds.Add(cardId)`
- Change from `request.PromiseCards.Add(card)` to `request.PromiseCardIds.Add(cardId)`
- Keep validation that cards exist in GameWorld.AllCardDefinitions
- Remove the card retrieval, just store the ID

### Step 3: Update CardDeckManager.cs (Lines 542-585)
- In `SelectGoalCardsForConversationType()`:
  - Retrieve cards from GameWorld.AllCardDefinitions using IDs
  - Change `foreach (var requestCard in request.RequestCards)` to iterate over IDs
  - Resolve each ID to get the actual card from GameWorld
  - Same for promise cards

### Step 4: Update any other code that accesses NPCRequest cards
- Search for all references to `request.RequestCards` and `request.PromiseCards`
- Update to use the new ID-based approach
- Ensure GameWorld is always the source for card data

## Testing Requirements
1. Verify Elena's letter request still works:
   - Request cards display with proper styling
   - Rapport thresholds are shown correctly
   - Cards become playable at threshold
   - Conversation ends when played

2. Verify no duplicate card storage:
   - Check memory usage doesn't increase with card duplication
   - Verify card changes in GameWorld are reflected everywhere

3. Verify all request types work:
   - Letter requests
   - Promise cards
   - Burden resolution

## Benefits of This Refactor
1. **Single Source of Truth**: GameWorld.AllCardDefinitions is the only place cards are stored
2. **Memory Efficiency**: No duplicate card objects in memory
3. **Consistency**: Card changes in GameWorld automatically reflected everywhere
4. **Cleaner Architecture**: Clear separation between data storage (GameWorld) and references (IDs)
5. **Easier Debugging**: Only one place to look for card data

## Code Examples

### Before (WRONG):
```csharp
// NPCRequest.cs
public List<ConversationCard> RequestCards { get; set; } = new List<ConversationCard>();

// PackageLoader.cs
if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard card))
{
    request.RequestCards.Add(card); // WRONG: Storing full object
}

// CardDeckManager.cs
foreach (var requestCard in request.RequestCards) // WRONG: Using duplicated cards
{
    CardInstance instance = new CardInstance(requestCard, npc.ID);
}
```

### After (CORRECT):
```csharp
// NPCRequest.cs
public List<string> RequestCardIds { get; set; } = new List<string>();

public List<ConversationCard> GetRequestCards(GameWorld gameWorld)
{
    var cards = new List<ConversationCard>();
    foreach (var cardId in RequestCardIds)
    {
        if (gameWorld.AllCardDefinitions.TryGetValue(cardId, out var card))
            cards.Add(card);
    }
    return cards;
}

// PackageLoader.cs
if (_gameWorld.AllCardDefinitions.ContainsKey(cardId))
{
    request.RequestCardIds.Add(cardId); // CORRECT: Storing only ID
}

// CardDeckManager.cs
foreach (var cardId in request.RequestCardIds) // CORRECT: Using IDs
{
    if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out var requestCard))
    {
        CardInstance instance = new CardInstance(requestCard, npc.ID);
    }
}
```

## Priority
**CRITICAL** - This violates core architectural principles and must be fixed immediately before any other feature work.