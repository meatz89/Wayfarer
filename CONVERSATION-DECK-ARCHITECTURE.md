# Conversation Deck Architecture

## Overview
The conversation system uses a dual-deck architecture where the Player owns the base conversation abilities (starter deck) and NPCs own unique progression cards that unlock based on relationship development.

## Core Principles

### 1. Player Owns the Starter Deck
- **Location**: `Player.ConversationDeck`
- **Contents**: 20 basic conversation cards (universal abilities)
- **Usage**: These cards are used in EVERY conversation with ANY NPC
- **Progression**: Cards gain XP and level up through use (future feature)
- **Examples**: "I hear you", "Tell me more", "That's interesting", etc.

### 2. NPCs Own Progression Decks
- **Location**: `NPC.ProgressionDeck`
- **Contents**: 5-15 unique cards specific to that NPC
- **Unlock Mechanism**: Cards unlock at token thresholds (1, 3, 6, 10, 15 tokens)
- **Usage**: These cards are shuffled into the player's deck when conversing with that NPC
- **Examples**: "Marcus's Bargain" (Commerce), "Elena's Trust" (Trust), etc.

### 3. NPCs Keep Other Specialized Decks
- **ExchangeDeck**: Quick trade cards (Mercantile NPCs only)
- **BurdenDeck**: Cards from past conflicts
- **ObservationDeck**: Location-based observation cards
- **Requests**: Letter/Promise cards at rapport thresholds

## Deck Composition During Conversations

When starting a conversation with an NPC:

```
Final Deck = Player.ConversationDeck + Unlocked NPC.ProgressionDeck Cards + Observation Cards
```

### Example:
- Player has 20 starter cards
- Player has 3 Commerce tokens with Marcus
- Marcus has cards unlocking at 1 and 3 tokens
- Final deck: 22 cards (20 player + 2 Marcus progression cards)

## Token-Based Progression

### Token Thresholds
- **1 Token**: First signature card (basic connection)
- **3 Tokens**: Second card (developing relationship)
- **6 Tokens**: Third card (established relationship)
- **10 Tokens**: Fourth card (strong bond)
- **15 Tokens**: Fifth card (deep connection)

### Card Properties
Each progression card has:
- `npcId`: Which NPC it belongs to
- `tokenThreshold`: Tokens required to unlock
- `tokenType`: Which token type unlocks it (Trust, Commerce, Status, Shadow)

## Implementation Status ✅ COMPLETE

### What Was Implemented

1. **Player Conversation Deck** ✅
   - Added `ConversationDeck` property to `Player` class
   - Initialized with ALL cards from `defaultDeck` in JSON
   - Safety checks added for empty deck scenarios

2. **NPC Progression Decks** ✅
   - Renamed NPC `ConversationDeck` to `ProgressionDeck`
   - NPCs ONLY get cards from `npcDecks` section in JSON
   - MinimumTokensRequired must be explicitly set in JSON (no defaults)
   - Updated all references throughout codebase

3. **Deck Composition Logic** ✅
   - `CardDeckManager.CreateConversationDeck()` now:
     - Starts with player's starter deck (from defaultDeck)
     - Adds unlocked NPC progression cards based on tokens
     - Combines with observation cards
     - Logs warnings if deck is empty

4. **Token-Based Unlocking** ✅
   - `GetUnlockedProgressionCards()` checks player tokens
   - Cards unlock at thresholds using `MinimumTokensRequired`
   - Token thresholds must be defined in JSON for each card

5. **Legacy Code Removal** ✅
   - Removed `NPCConversationDeckMappings` from GameWorld
   - NPCs no longer receive defaultDeck cards
   - Clear separation between player starter and NPC progression cards

## Implementation Flow

### 1. Conversation Start
```csharp
// CardDeckManager.CreateConversationDeck()
1. Get player's conversation deck (20 starter cards)
2. Check player's tokens with this NPC
3. Add unlocked progression cards from NPC.ProgressionDeck
4. Add any observation cards
5. Shuffle and return session deck
```

### 2. Card Selection
```csharp
// During conversation
- All cards in the combined deck are available
- Player cards represent base social skills
- NPC cards represent specialized interactions
- Both types follow same mechanics (focus cost, success rate, etc.)
```

### 3. Progression Tracking
```csharp
// After successful SPEAK
- Card gains 1 XP (future feature)
- If card levels up, apply bonuses
- Level 5 "Mastered" cards bypass forced LISTEN on failure
```

## Benefits of This Architecture

### 1. Clear Ownership
- Player owns their conversation abilities
- NPCs own relationship-specific interactions
- No confusion about "whose cards are these?"

### 2. Portable Progression
- Player's deck improvements work everywhere
- Each NPC relationship feels unique
- Clear sense of character growth

### 3. Mechanical Depth
- Base deck provides consistency
- NPC cards add variety and strategy
- Token earning has clear rewards

### 4. Narrative Coherence
- Starter cards = learned social skills
- NPC cards = understanding specific people
- Progression = deepening relationships

## JSON Structure

### Starter Cards (Player Deck)
```json
{
  "id": "active_listening",
  "description": "I hear what you're saying",
  "starter": true,  // Marks this as a starter card
  "focus": 1,
  "difficulty": "Easy",
  "tokenType": "Trust"
}
```

### NPC Progression Cards
```json
{
  "id": "marcus_special_deal",
  "description": "Marcus's Special Deal",
  "npcId": "marcus",
  "tokenThreshold": 3,
  "tokenType": "Commerce",
  "focus": 2,
  "difficulty": "Medium"
}
```

## Migration Path

### Phase 1: Core Structure ✅ COMPLETE
1. ✅ Add `ConversationDeck` to Player
2. ✅ Rename NPC `ConversationDeck` to `ProgressionDeck`
3. ✅ Update all references to use correct deck
4. ✅ Update `CardDeckManager` to use player deck

### Phase 2: Content Loading ✅ COMPLETE
1. ✅ Update `PackageLoader` to initialize player deck
2. ✅ Load NPC progression cards separately
3. ✅ Remove NPCConversationDeckMappings from GameWorld

### Phase 3: Token Integration ✅ COMPLETE
1. ✅ Add token checking in GetUnlockedProgressionCards
2. ✅ Filter progression cards by MinimumTokensRequired
3. ⏳ Test token-based unlocking

### Phase 4: Future Features
1. Card XP and leveling system
2. Mastered cards bypass forced LISTEN
3. Deck management UI

## Testing Strategy

### Unit Tests
- Player deck initialization
- NPC progression deck loading
- Token threshold checking
- Deck composition logic

### Integration Tests
- Full conversation with combined deck
- Token earning unlocks new cards
- Save/load with new structure

### E2E Tests
- Start conversation with 0 tokens (only player deck)
- Earn tokens and see new cards
- Verify each NPC has unique cards

## Backwards Compatibility

This is a BREAKING CHANGE that requires:
1. Migration of existing save files
2. Update of all content JSON files
3. Complete removal of old deck system

NO compatibility layers will be maintained - clean break to new system.