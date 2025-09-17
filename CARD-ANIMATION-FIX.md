# Card Animation Fix - Complete Implementation

## Problem Analysis

### The Core Issue
When cards are exhausted (Opening cards on LISTEN, Impulse cards on SPEAK), they are **immediately removed from the backend state**. This causes:
1. Blazor to re-render instantly
2. DOM elements to disappear before animations can play
3. Cards to just "POOF" vanish with no visual feedback

The previous attempt failed because it tried to animate cards that no longer existed in the DOM.

## Solution: Temporary Exhaust Store

### Key Insight
**Keep exhausted cards in a separate display-only collection** while their animations play. The backend state changes instantly (SYNCHRONOUS PRINCIPLE), but the visual layer maintains temporary copies for animation.

### Architecture

```
Backend State (Instant)          Display Layer (Animated)
─────────────────────────        ──────────────────────────
Session.Deck.Hand.Cards  ──┐
  - Card A                 │     ExhaustingCardStore
  - Card B (Opening)       ├───> - Card B copy (animating)
  - Card C (Impulse)       │     - Card C copy (animating)
  - Card D                 │
                           │     After animation:
After LISTEN:              │     - Cleaned up automatically
  - Card A                 │
  - Card D                 │
  - New Card E             │
  - New Card F             └───> GetAllDisplayCards combines both
```

## Implementation Details

### 1. ExhaustingCardStore
- Stores **copies** of cards during exhaust animation
- Tracks animation timing (delay, direction, sequence)
- Auto-cleanup based on animation duration
- Thread-safe with lock protection

### 2. Modified Card Flow

#### LISTEN Action
```csharp
// 1. Track cards BEFORE backend changes
var openingCards = cards.Where(c => c.Persistence == Opening);

// 2. Store copies for animation
ExhaustingCardStore.AddExhaustingCards(openingCards, baseDelay: 0.0);

// 3. Execute backend (removes cards instantly)
await ConversationFacade.ExecuteListen();

// 4. Mark new cards for draw animation (after exhaust phase)
var exhaustTime = openingCards.Count * 0.15 + 0.5;
AnimationManager.MarkNewCardsSequential(newCards, exhaustTime + 0.2);
```

#### SPEAK Action
```csharp
// 1. Track Impulse cards BEFORE backend changes
var impulseCards = cards.Where(c => c.Persistence == Impulse && c != playedCard);

// 2. Store copies for animation (delay after played card animation)
ExhaustingCardStore.AddExhaustingCards(impulseCards, baseDelay: 1.5);

// 3. Execute backend (removes cards instantly)
await ConversationFacade.ExecuteSpeakSingleCard(playedCard);

// 4. New cards enter after all animations
```

### 3. Display Integration

```csharp
GetAllDisplayCards() {
    var displayCards = new List<CardDisplayInfo>();

    // 1. Add exhausting cards (temporary copies)
    foreach (var exhausting in ExhaustingCardStore.GetExhaustingCards()) {
        displayCards.Add(new CardDisplayInfo {
            Card = exhausting.Card,
            IsExhausting = true,
            AnimationDelay = exhausting.AnimationDelay,
            AnimationDirection = exhausting.AnimationDirection
        });
    }

    // 2. Add current hand cards
    displayCards.AddRange(Session.Deck.Hand.Cards);

    // 3. Sort promise cards first
    return SortCardsWithPromiseFirst(displayCards);
}
```

### 4. CSS Animations

```css
/* Exhaust to right (discard pile metaphor) */
@keyframes card-exhaust-right {
    0% {
        opacity: 1;
        transform: translateX(0) scale(1);
    }
    100% {
        opacity: 0;
        transform: translateX(300px) scale(0.3) rotateY(-30deg);
        max-height: 0;
        margin: 0;
        padding: 0;
    }
}

/* Draw from left (deck metaphor) */
@keyframes card-draw-left {
    0% {
        opacity: 0;
        transform: translateX(-300px) scale(0.3) rotateY(30deg);
    }
    100% {
        opacity: 1;
        transform: translateX(0) scale(1) rotateY(0);
    }
}

.card-exhausting {
    animation: card-exhaust-right 0.5s ease-out forwards;
    /* animation-delay set inline per card */
}

.card-new {
    animation: card-draw-left 0.5s ease-out forwards;
    /* animation-delay set inline per card */
}
```

### 5. Razor Template

```razor
@foreach (var displayCard in GetAllDisplayCards())
{
    var animationStyle = displayCard.AnimationDelay > 0
        ? $"animation-delay: {displayCard.AnimationDelay:F2}s;"
        : "";

    <div class="card @GetCardClasses(displayCard)"
         style="@animationStyle">
        <!-- Card content -->
    </div>
}
```

## Timing Specifications

### Animation Durations
- **Exhaust animation**: 0.5s
- **Draw animation**: 0.5s
- **Stagger delay**: 0.15s between cards
- **Phase gap**: 0.2s between exhaust and draw
- **Cleanup**: animation delay + 0.6s

### Example Timeline (3 Opening cards exhaust, 4 new cards draw)
```
0.00s - Opening Card 1 starts exhausting
0.15s - Opening Card 2 starts exhausting
0.30s - Opening Card 3 starts exhausting
0.50s - Opening Card 1 finishes, removed from DOM
0.65s - Opening Card 2 finishes, removed from DOM
0.80s - Opening Card 3 finishes, removed from DOM
1.00s - [Gap between phases]
1.20s - New Card 1 starts entering
1.35s - New Card 2 starts entering
1.50s - New Card 3 starts entering
1.65s - New Card 4 starts entering
2.15s - All animations complete
```

## SYNCHRONOUS PRINCIPLE Maintained

1. **Backend state changes instantly** - Cards removed immediately
2. **Display layer handles animation** - Temporary copies for visual feedback
3. **Game remains playable** - Can take actions during animations
4. **Graceful degradation** - If animations fail, game continues

## Testing Plan

1. Start conversation with Marcus (Trade Opportunities)
2. Click LISTEN
   - Verify Opening cards exhaust sequentially to the right
   - Verify new cards draw sequentially from the left
   - Check 0.15s stagger between each card
3. Click a Thought card to select, then SPEAK
   - Verify played card animates (success/failure flash)
   - Verify Impulse cards exhaust sequentially after
   - Verify any new cards (Threading effect) draw after
4. Verify no empty space remains after animations
5. Check that cards are clickable during animations

## Success Criteria

✅ Cards exhaust with visible sequential animation (not instant POOF)
✅ Clear directional movement (exhaust right, draw left)
✅ Player can track individual card movements
✅ No empty space after animations complete
✅ Game state remains consistent
✅ Animations don't block gameplay