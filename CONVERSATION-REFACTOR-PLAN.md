# Conversation System Architecture Refactoring Plan

## Problem Statement

The current conversation card system has a critical bug where played cards disappear completely instead of going to the discard pile for reshuffling. The root cause is an overly complex architecture with 7+ layers of indirection that makes it impossible to track where cards are being lost.

### Current Broken Architecture
```
UI (ConversationContent.razor.cs)
  → ConversationFacade.ExecuteSpeakSingleCard()  [WRONG - bypasses GameFacade!]
    → ConversationFacade.ProcessAction()
      → ConversationOrchestrator.ProcessSpeakAction()
        → CardDeckManager.PlayCard()
          → SessionCardDeck.PlayCard()
            → Pile.Add() / Pile.Remove()
```

### Symptoms
- Player starts with 20 cards in starter deck
- After playing cards, they completely disappear
- Logs show "0 available (draw pile and discard both empty)"
- Cards are not being recycled, breaking the core gameplay loop

## Architectural Principles Being Violated

1. **UI MUST call GameFacade** - Currently UI calls ConversationFacade directly
2. **Single Source of Truth** - Card state is managed across too many layers
3. **No unnecessary abstraction** - ConversationOrchestrator and CardDeckManager add no value
4. **Clear ownership** - Nobody clearly owns the deck lifecycle

## Correct Architecture

Per our architectural principles:

```
UI (ConversationContent.razor.cs)
  → GameFacade.PlayConversationCard()
    → ConversationFacade.PlayCard()
      → ConversationSession.PlayCard()
        → SessionCardDeck.PlayCard()
```

## Refactoring Plan

### Phase 1: Critical Bug Fix
Fix `SessionCardDeck.PlayCard()` to properly add cards to discard pile with comprehensive logging:

```csharp
public void PlayCard(CardInstance card)
{
    if (card == null)
    {
        Console.WriteLine("[SessionCardDeck] ERROR: PlayCard called with null card!");
        return;
    }

    Console.WriteLine($"[SessionCardDeck] Playing card {card.Id} from hand");

    if (!handPile.Remove(card))
    {
        Console.WriteLine($"[SessionCardDeck] ERROR: Card {card.Id} not found in hand!");
        return;
    }

    playedPile.Add(card);
    discardPile.Add(card);

    Console.WriteLine($"[SessionCardDeck] Card {card.Id} added to discard. Discard count: {discardPile.Count}");

    // Validate total card count remains constant
    int totalCards = handPile.Count + drawPile.Count + discardPile.Count;
    Console.WriteLine($"[SessionCardDeck] Total cards in circulation: {totalCards}");
}
```

### Phase 2: Add GameFacade Methods
Create proper entry points in GameFacade:

```csharp
public async Task<ConversationTurnResult> PlayConversationCard(CardInstance card)
{
    if (_conversationFacade == null)
        throw new InvalidOperationException("No active conversation");

    return await _conversationFacade.PlayCard(card);
}

public async Task<ConversationTurnResult> ExecuteListen()
{
    if (_conversationFacade == null)
        throw new InvalidOperationException("No active conversation");

    return await _conversationFacade.ExecuteListen();
}
```

### Phase 3: Update UI Layer
- Change all `ConversationFacade` calls to `GameFacade`
- Remove direct facade access
- Follow proper architectural boundaries

### Phase 4: Simplify ConversationFacade
- Delete `ProcessAction()` and its complexity
- Delete `ConversationAction` DTO (unnecessary)
- Implement `PlayCard()` and `ExecuteListen()` directly
- Move essential Orchestrator logic here

### Phase 5: Delete Unnecessary Classes
- **ConversationOrchestrator** - Move logic to ConversationFacade
- **CardDeckManager** - Move logic to SessionCardDeck and ConversationSession
- **ConversationAction** - Unnecessary DTO

### Phase 6: Empower ConversationSession
Make ConversationSession the owner of conversation state:
- Owns SessionCardDeck
- Owns FocusManager, FlowManager, AtmosphereManager
- Methods: `PlayCard()`, `Listen()`, `GetAvailableCards()`
- Clear state management without scattered logic

## Benefits of Refactoring

1. **Follows architectural principles** - UI → GameFacade → Facades
2. **4 layers instead of 7+** - Massive simplification
3. **Clear responsibilities** - Each layer has one job
4. **Cards can't disappear** - Direct tracked path from hand to discard
5. **Easier to debug** - Linear flow with comprehensive logging
6. **Single source of truth** - SessionCardDeck owns all card state

## Validation Requirements

After refactoring, these invariants MUST hold:

1. **Total card count never changes** - Cards only move between piles, never created/destroyed
2. **Every PlayCard() adds to discard** - No exceptions
3. **Reshuffling moves ALL discard cards** - Complete transfer to draw pile
4. **No orphaned cards** - Every card exists in exactly one pile
5. **Logging tracks every movement** - Full audit trail

## Implementation Order

1. Fix SessionCardDeck.PlayCard() [CRITICAL - fixes immediate bug]
2. Add GameFacade methods
3. Update UI to use GameFacade
4. Refactor ConversationFacade
5. Delete ConversationOrchestrator
6. Delete CardDeckManager
7. Test thoroughly with Playwright

## Success Criteria

- Cards played are ALWAYS available for redraw after reshuffling
- No "0 cards available" errors when cards should exist
- Clean architectural boundaries: UI → GameFacade → ConversationFacade → Session → Deck
- Comprehensive logging shows card lifecycle
- Total card count remains constant throughout conversation