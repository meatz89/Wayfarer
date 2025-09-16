# HIGHLANDER PRINCIPLE REFACTOR - Card System

## Problem Statement
The conversation card system violates the HIGHLANDER PRINCIPLE ("There can be only ONE") by having multiple duplicate card collections scattered across different classes:

### Current Violations:
1. **SessionCardDeck** uses `List<CardInstance>` directly instead of Pile abstraction
2. **ConversationSession** has 7 different card collections:
   - `SessionCardDeck Deck` - The proper deck
   - `Pile DrawPile` - DUPLICATE of Deck's internal drawPile
   - `Pile ExhaustPile` - DUPLICATE of Deck's internal discardPile
   - `Pile ActiveCards` - Should be in Deck as handPile
   - `List<CardInstance> PlayedCards` - Should be in Deck as playedPile
   - `List<CardInstance> DiscardedCards` - DUPLICATE of discardPile
   - `List<CardInstance> RequestPile` - Should be in Deck as requestPile

3. **Cards not being reshuffled** because they go to ExhaustPile instead of Deck.DiscardCard()

## Solution: ONE Deck System with Pile Abstraction

### Core Principles:
- **HIGHLANDER**: ONE SessionCardDeck manages ALL card state
- **CONSISTENCY**: Use Pile abstraction everywhere, NEVER List<CardInstance>
- **NO LEGACY**: No compatibility properties, no fallback code
- **COMPLETE**: Update ALL references immediately

## Implementation Plan

### Step 1: Refactor SessionCardDeck
Transform SessionCardDeck to use Pile for ALL collections and own ALL card state:

```csharp
public class SessionCardDeck
{
    // ALL piles use Pile abstraction
    private readonly Pile drawPile = new();
    private readonly Pile discardPile = new();
    private readonly Pile handPile = new();      // Was ActiveCards
    private readonly Pile requestPile = new();   // Was separate RequestPile
    private readonly Pile playedPile = new();    // Was PlayedCards

    // Public access
    public Pile Hand => handPile;
    public Pile RequestCards => requestPile;
    public Pile PlayedHistory => playedPile;
}
```

Key changes:
- Replace all `List<CardInstance>` with `Pile`
- Add handPile, requestPile, playedPile
- Provide direct Pile access (no IReadOnlyList)
- Add PlayCard() method that handles all transitions
- Fix reshuffle to work properly

### Step 2: Clean ConversationSession
Remove ALL duplicate card collections:

```csharp
public class ConversationSession
{
    public SessionCardDeck Deck { get; set; }  // ONLY card reference

    // DELETE: DrawPile, ExhaustPile, ActiveCards, PlayedCards,
    //         DiscardedCards, RequestPile, HandCards, Hand
}
```

### Step 3: Update All References

#### CardDeckManager.cs (5 exhaust locations):
- Line 284: `session.ExhaustPile.Add()` → `session.Deck.DiscardCard()`
- Line 296: `session.ExhaustPile.Add()` → `session.Deck.DiscardCard()`
- Line 455: `session.ExhaustPile.Add()` → `session.Deck.DiscardCard()`
- Line 485: `session.ExhaustPile.Add()` → `session.Deck.DiscardCard()`
- Line 527: `session.ExhaustPile.Add()` → `session.Deck.DiscardCard()`

All ActiveCards references:
- `session.ActiveCards` → `session.Deck.Hand`
- `session.ActiveCards.Remove()` → `session.Deck.Hand.Remove()`
- `session.ActiveCards.AddRange()` → `session.Deck.Hand.AddRange()`

#### ConversationOrchestrator.cs:
- Remove initialization of DrawPile, ExhaustPile
- `ActiveCards = new Pile()` → Remove (Deck already has it)
- `session.RequestPile.AddRange()` → `session.Deck.RequestCards.AddRange()`

#### ConversationContent.razor.cs:
- `Session.ActiveCards.Cards` → `Session.Deck.Hand.Cards`
- `Session.RequestPile` → `Session.Deck.RequestCards`

### Step 4: Add Documentation
Add clear HIGHLANDER comments to prevent future violations:

```csharp
/// HIGHLANDER PRINCIPLE: This is the ONE AND ONLY card system.
/// ALL card collections live here. NO EXCEPTIONS.
/// Uses Pile abstraction consistently - NEVER List<CardInstance>.
/// NO compatibility layers, NO legacy code, NO fallbacks.
```

## Expected Results

1. **Cards will reshuffle properly** - Fixed the bug where LISTEN stops drawing cards
2. **Clean architecture** - ONE place for all card state
3. **Consistent abstraction** - Pile used everywhere
4. **No duplicate state** - Eliminates synchronization bugs
5. **Clear ownership** - SessionCardDeck owns ALL card operations

## Files to Modify

1. `/mnt/c/git/wayfarer/src/GameState/SessionCardDeck.cs` - Complete rewrite
2. `/mnt/c/git/wayfarer/src/GameState/ConversationSession.cs` - Remove 6 properties
3. `/mnt/c/git/wayfarer/src/Subsystems/Conversation/CardDeckManager.cs` - Update all references
4. `/mnt/c/git/wayfarer/src/Subsystems/Conversation/ConversationOrchestrator.cs` - Update initialization
5. `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor.cs` - Update UI references
6. Any other files using ActiveCards, RequestPile, etc.

## Testing Requirements

After implementation:
1. Build the project
2. Start a conversation
3. Play cards until deck exhausts
4. Verify LISTEN still draws cards (reshuffle working)
5. Verify request cards appear at rapport threshold
6. Verify no duplicate state issues