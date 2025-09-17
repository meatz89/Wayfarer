# Card Animation System Documentation

## Core Principle: SYNCHRONOUS GAME STATE, ASYNCHRONOUS VISUALS

### The Synchronous Principle

The fundamental design principle of Wayfarer's animation system is:

**"Game state changes are instantaneous and synchronous. Animations are purely visual feedback that never block, delay, or affect game mechanics."**

This means:
- When a player clicks LISTEN, cards are immediately drawn and exhausted in the backend
- The game state is fully updated before any animations begin
- Animations provide visual clarity to help players understand what happened
- The player can take their next action even while animations are playing
- If animations fail or are interrupted, the game continues functioning correctly

## Current Problems (as of 2025-09-16)

### Problem 1: Simultaneous Animation
**Issue**: All cards animate at the exact same time, making it impossible to track individual card movements
- When LISTEN is clicked, all Opening cards exhaust simultaneously
- All new cards appear simultaneously
- Player sees a chaotic blur of movement

**Impact**: Players cannot understand what's happening in the game

### Problem 2: Wrong Animation Directions
**Issue**: Cards don't have clear entry/exit directions
- Exhausted cards should exit to the right (into a discard pile)
- New cards should enter from the left (from a draw pile)
- Currently, cards just fade/scale without directional movement

**Impact**: No visual metaphor for where cards come from or go to

### Problem 3: CSS nth-child Doesn't Work
**Issue**: The CSS uses nth-child selectors for stagger delays, but this doesn't work with dynamic content
```css
.card-new:nth-child(1) { animation-delay: 0s; }
.card-new:nth-child(2) { animation-delay: 0.05s; }
```
**Why it fails**:
- Cards are rendered in a loop, not as direct children
- When cards are added/removed, nth-child indices change
- Multiple cards might have the same nth-child value

**Impact**: Stagger delays don't apply, everything animates simultaneously

### Problem 4: No Sequential Timing
**Issue**: Draw and exhaust animations happen at the same time
- Should be: First exhaust all cards, THEN draw new cards
- Currently: Both happen simultaneously, creating visual chaos

**Impact**: Player can't distinguish between cards leaving and cards arriving

## Detailed Solution Architecture

### 1. Animation State Management

#### Current State Structure
```csharp
public class CardAnimationState
{
    public string CardId { get; set; }
    public string State { get; set; }  // "new", "exhausting", "played-success", etc.
    public DateTime StateChangedAt { get; set; }
}
```

#### Enhanced State Structure (Proposed)
```csharp
public class CardAnimationState
{
    public string CardId { get; set; }
    public string State { get; set; }
    public DateTime StateChangedAt { get; set; }
    public double AnimationDelay { get; set; }  // NEW: Delay in seconds for this specific card
    public int SequenceIndex { get; set; }      // NEW: Position in animation sequence
    public string AnimationDirection { get; set; } // NEW: "left", "right", "up", "down"
}
```

### 2. Animation Sequencing Logic

#### Current Flow (Problematic)
```
LISTEN clicked
  ├─→ Backend: Remove Opening cards (instant)
  ├─→ Backend: Draw new cards (instant)
  ├─→ Frontend: Mark all Opening cards as exhausting (simultaneous)
  └─→ Frontend: Mark all new cards as entering (simultaneous)

SPEAK clicked
  ├─→ Backend: Play card (instant)
  ├─→ Backend: Remove Impulse cards if another was played (instant)
  ├─→ Frontend: Card flashes success/failure (no clear timing)
  └─→ Frontend: Impulse cards vanish (simultaneous, no direction)
```

#### Proposed Flow (Sequential)
```
LISTEN clicked
  ├─→ Backend: Remove Opening cards (instant)
  ├─→ Backend: Draw new cards (instant)
  ├─→ Frontend: Phase 1 - Exhaust animations
  │     ├─→ Card 1: Exhaust right (delay: 0.0s)
  │     ├─→ Card 2: Exhaust right (delay: 0.15s)
  │     └─→ Card N: Exhaust right (delay: 0.15 * N)
  └─→ Frontend: Phase 2 - Draw animations (starts after exhaust phase)
        ├─→ Card 1: Enter from left (delay: exhaust_time + 0.2s)
        ├─→ Card 2: Enter from left (delay: exhaust_time + 0.35s)
        └─→ Card N: Enter from left (delay: exhaust_time + 0.2 + 0.15*N)

SPEAK clicked (single card played)
  ├─→ Backend: Play card & process effects (instant)
  ├─→ Backend: Remove Impulse cards if needed (instant)
  ├─→ Frontend: Phase 1 - Played card animation
  │     └─→ Played card: Success/failure flash (1.25s) then exit up (0.25s)
  ├─→ Frontend: Phase 2 - Exhaust Impulse cards (if any)
  │     ├─→ Impulse 1: Exhaust right (delay: 1.5s)
  │     ├─→ Impulse 2: Exhaust right (delay: 1.65s)
  │     └─→ Impulse N: Exhaust right (delay: 1.5 + 0.15*N)
  └─→ Frontend: Phase 3 - Draw new cards (if Threading effect)
        ├─→ New Card 1: Enter from left (delay: exhaust_time + 0.2s)
        └─→ New Card N: Enter from left (delay: exhaust_time + 0.2 + 0.15*N)
```

### 3. CSS Animation Definitions

#### Exhaust Animation (Exit Right)
```css
@keyframes card-exhaust-right {
    0% {
        opacity: 1;
        transform: translateX(0) scale(1);
        filter: grayscale(0);
    }
    30% {
        opacity: 0.8;
        transform: translateX(50px) scale(0.95) rotateY(-10deg);
        filter: grayscale(0.3) sepia(0.3);
    }
    60% {
        opacity: 0.5;
        transform: translateX(150px) scale(0.8) rotateY(-20deg);
        filter: grayscale(0.6) sepia(0.6);
    }
    100% {
        opacity: 0;
        transform: translateX(300px) scale(0.3) rotateY(-30deg);
        filter: grayscale(1) sepia(1);
        max-height: 0;
        margin: 0;
        padding: 0;
    }
}
```

#### Draw Animation (Enter from Left)
```css
@keyframes card-draw-left {
    0% {
        opacity: 0;
        transform: translateX(-300px) scale(0.3) rotateY(30deg);
    }
    40% {
        opacity: 0.5;
        transform: translateX(-150px) scale(0.6) rotateY(15deg);
    }
    70% {
        opacity: 0.9;
        transform: translateX(-50px) scale(0.9) rotateY(5deg);
    }
    100% {
        opacity: 1;
        transform: translateX(0) scale(1) rotateY(0);
    }
}
```

### 4. Implementation Details

#### CardAnimationManager Changes

```csharp
public void MarkCardsForExhaustSequential(List<CardInstance> cardsToExhaust, Action stateChangedCallback)
{
    double baseDelay = 0.0;
    const double STAGGER_DELAY = 0.15; // 150ms between each card

    for (int i = 0; i < cardsToExhaust.Count; i++)
    {
        CardInstance card = cardsToExhaust[i];
        string cardId = card.InstanceId ?? card.Id ?? "";

        exhaustingCardIds.Add(cardId);

        cardStates[cardId] = new CardAnimationState
        {
            CardId = cardId,
            State = "exhausting",
            StateChangedAt = DateTime.Now,
            AnimationDelay = baseDelay + (i * STAGGER_DELAY),
            SequenceIndex = i,
            AnimationDirection = "right"
        };
    }

    stateChangedCallback?.Invoke();
}

public void MarkNewCardsSequential(List<CardInstance> newCards, double startDelay, Action stateChangedCallback)
{
    const double STAGGER_DELAY = 0.15;

    for (int i = 0; i < newCards.Count; i++)
    {
        CardInstance card = newCards[i];
        string cardId = card.InstanceId ?? card.Id ?? "";

        newCardIds.Add(cardId);

        cardStates[cardId] = new CardAnimationState
        {
            CardId = cardId,
            State = "new",
            StateChangedAt = DateTime.Now,
            AnimationDelay = startDelay + (i * STAGGER_DELAY),
            SequenceIndex = i,
            AnimationDirection = "left"
        };
    }

    stateChangedCallback?.Invoke();
}
```

#### ConversationContent.razor.cs Changes

```csharp
protected async Task ExecuteListen()
{
    // ... existing code ...

    // Track cards before action
    List<CardInstance> previousCards = Session?.Deck?.Hand?.Cards?.ToList() ?? new List<CardInstance>();

    // Execute backend action (INSTANT state change)
    ConversationTurnResult listenResult = await ConversationFacade.ExecuteListen();

    // Track cards after action
    List<CardInstance> currentCards = Session?.Deck?.Hand?.Cards?.ToList() ?? new List<CardInstance>();

    // PHASE 1: Animate exhausting cards
    List<CardInstance> exhaustedCards = previousCards
        .Where(c => c.Persistence == PersistenceType.Opening &&
                   !currentCards.Any(cc => cc.InstanceId == c.InstanceId))
        .ToList();

    if (exhaustedCards.Any())
    {
        AnimationManager.MarkCardsForExhaustSequential(exhaustedCards, () => InvokeAsync(StateHasChanged));
    }

    // Calculate when exhaust phase ends
    double exhaustPhaseTime = exhaustedCards.Count * 0.15 + 0.5; // stagger + animation duration

    // PHASE 2: Animate new cards entering
    List<CardInstance> drawnCards = currentCards
        .Where(c => !previousCards.Any(pc => pc.InstanceId == c.InstanceId))
        .ToList();

    if (drawnCards.Any())
    {
        // Start draw animations after exhaust phase completes
        double drawStartDelay = exhaustPhaseTime + 0.2; // Small gap between phases
        AnimationManager.MarkNewCardsSequential(drawnCards, drawStartDelay, () => InvokeAsync(StateHasChanged));
    }

    StateHasChanged();
}

protected async Task ExecuteSpeak()
{
    // ... existing code ...

    // Track cards before action
    List<CardInstance> cardsBeforeSpeak = Session?.Deck?.Hand?.Cards?.ToList() ?? new List<CardInstance>();
    CardInstance playedCard = SelectedCard;

    // Execute backend action (INSTANT state change)
    ConversationTurnResult turnResult = await ConversationFacade.ExecuteSpeakSingleCard(SelectedCard);
    CardPlayResult result = turnResult?.CardPlayResult;

    // Track cards after action
    List<CardInstance> cardsAfterSpeak = Session?.Deck?.Hand?.Cards?.ToList() ?? new List<CardInstance>();

    // PHASE 1: Animate played card (success/failure flash then exit up)
    bool success = result?.Success ?? false;
    AnimationManager.MarkCardAsPlayed(playedCard, success, () => InvokeAsync(StateHasChanged));

    // PHASE 2: Animate exhausting Impulse cards
    List<CardInstance> exhaustedImpulse = cardsBeforeSpeak
        .Where(c => c.Persistence == PersistenceType.Impulse &&
                   c.InstanceId != playedCard.InstanceId &&
                   !cardsAfterSpeak.Any(ac => ac.InstanceId == c.InstanceId))
        .ToList();

    if (exhaustedImpulse.Any())
    {
        // Start after played card animation (1.5s)
        AnimationManager.MarkCardsForExhaustSequential(exhaustedImpulse, 1.5, () => InvokeAsync(StateHasChanged));
    }

    // PHASE 3: Animate new cards (if Threading success effect)
    List<CardInstance> drawnCards = cardsAfterSpeak
        .Where(c => !cardsBeforeSpeak.Any(bc => bc.InstanceId == c.InstanceId))
        .ToList();

    if (drawnCards.Any())
    {
        double exhaustTime = exhaustedImpulse.Count * 0.15 + 0.5;
        double drawStartDelay = 1.5 + exhaustTime + 0.2;
        AnimationManager.MarkNewCardsSequential(drawnCards, drawStartDelay, () => InvokeAsync(StateHasChanged));
    }

    StateHasChanged();
}
```

#### ConversationContent.razor Changes

```razor
@foreach (var card in DisplayCards)
{
    var animationState = GetCardAnimationState(card);
    var animationStyle = animationState != null
        ? $"animation-delay: {animationState.AnimationDelay}s;"
        : "";

    <div class="card @GetCardCssClasses(card)"
         style="@animationStyle"
         @onclick="() => ToggleCardSelection(card)">
        <!-- Card content -->
    </div>
}
```

### 5. Timing Specifications

#### Animation Durations
- **Exhaust Animation**: 0.5s duration
- **Draw Animation**: 0.5s duration
- **Stagger Delay**: 0.15s between each card
- **Phase Gap**: 0.2s pause between exhaust and draw phases

#### Example Timeline (3 cards exhaust, 4 cards draw)
```
Time    Action
0.00s   Card 1 starts exhausting
0.15s   Card 2 starts exhausting
0.30s   Card 3 starts exhausting
0.50s   Card 1 finishes exhausting
0.65s   Card 2 finishes exhausting
0.80s   Card 3 finishes exhausting
1.00s   [Gap between phases]
1.20s   New Card 1 starts entering
1.35s   New Card 2 starts entering
1.50s   New Card 3 starts entering
1.65s   New Card 4 starts entering
1.70s   New Card 1 finishes entering
1.85s   New Card 2 finishes entering
2.00s   New Card 3 finishes entering
2.15s   New Card 4 finishes entering
```

### 6. Browser Performance Considerations

#### CSS Transform vs Position
- Use `transform: translateX()` instead of `left/right` positioning
- Transforms use GPU acceleration, position changes cause reflow
- Combine transforms: `transform: translateX(300px) scale(0.3) rotateY(-30deg)`

#### Animation Cleanup
- Remove animation classes after animations complete
- Clean up animation state objects to prevent memory leaks
- Use `animation-fill-mode: forwards` to maintain final state

#### Batch DOM Updates
- Use `StateHasChanged()` sparingly
- Batch multiple animation state changes before triggering re-render
- Use CSS containment: `contain: layout style paint`

### 7. Fallback and Error Handling

#### What if animations fail?
- Game state is already updated, so gameplay continues
- Cards will appear in their final positions (no animation)
- Player can still interact normally

#### What if browser doesn't support CSS animations?
- Cards appear instantly in final positions
- Game remains fully playable
- Consider using `@supports` CSS queries for progressive enhancement

#### What if too many cards animate?
- Limit maximum simultaneous animations (e.g., 10 cards)
- Group remaining cards into single batch animation
- Reduce stagger delay for large batches

### 8. Testing Checklist

- [ ] Single card exhaust animates correctly (right exit)
- [ ] Multiple cards exhaust with proper stagger
- [ ] Single card draw animates correctly (left entry)
- [ ] Multiple cards draw with proper stagger
- [ ] Exhaust completes before draw begins
- [ ] Animations don't block next player action
- [ ] Game state is correct even if animations are interrupted
- [ ] Performance is smooth with 10+ cards
- [ ] Mobile devices handle animations well
- [ ] Reduced motion preference is respected

### 9. Future Enhancements

#### Priority 1 (Essential)
- Fix animation directions (left/right)
- Add sequential delays
- Ensure exhaust-before-draw ordering

#### Priority 2 (Polish)
- Add subtle rotation to card movements
- Implement easing curves for more natural motion

## Card Persistence Types and Exhaust Rules

### Persistence Types
1. **Thought Cards** - Remain in hand until played, never exhaust automatically
2. **Impulse Cards** - Exhaust when ANY other card is played (not just when they're played)
3. **Opening Cards** - Exhaust on LISTEN action

### When Cards Exhaust

#### During LISTEN:
- **Opening cards** are removed from hand and animate out
- **Thought cards** remain
- **Impulse cards** remain (but are marked as "will exhaust" visually)

#### During SPEAK:
- **The played card** animates with success/failure, then exits
- **ALL Impulse cards** in hand (except the one being played if it's an Impulse) exhaust
- **Thought cards** and **Opening cards** remain (unless they were the played card)

#### Special Cases:
- Some cards have **Exhaust effects** that trigger penalties when they leave without being played
- **Threading** success effect draws new cards after the exhaust phase

## Summary

The card animation system must maintain the SYNCHRONOUS PRINCIPLE at all costs:
1. **Game state changes instantly** - Backend completes all logic immediately
2. **Animations are visual feedback only** - They help players understand what happened
3. **Never block gameplay** - Players can act even while animations play
4. **Sequential clarity** - Animations should play in logical order (exhaust → draw)
5. **Directional consistency** - Cards exit right (to discard), enter from left (from deck)

By following these principles, we create a responsive game that feels smooth and polished while maintaining rock-solid mechanical integrity.