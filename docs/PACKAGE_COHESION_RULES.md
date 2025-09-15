# Package Cohesion Rules

## Overview

This document defines which entities must be packaged together to maintain referential integrity during the content loading process. The Wayfarer content system loads packages atomically - each package is fully processed before moving to the next - which means cross-package dependencies can fail if entities are separated incorrectly.

## Core Principle

**Entities that directly reference each other must be in the same package.**

The content loading system processes packages atomically. When a package is loaded, all its content must be resolvable using either:
1. Content within the same package
2. Content already loaded from previous packages
3. Skeleton placeholders (for graceful degradation)

## Mandatory Cohesion Rules

### 1. NPCRequests and Their Cards

**Rule**: NPCRequests must be in the same package as ALL cards they reference.

**Entities that must be together**:
- NPCRequest definitions
- Request cards referenced in `requestCards[]`
- Promise cards referenced in `promiseCards[]`

**Example**:
```json
// These MUST be in the same package:
{
  "cards": [
    { "id": "elena_letter_request", "type": "Request", ... },
    { "id": "elena_priority_promise", "type": "Promise", ... }
  ],
  "npcRequests": [
    {
      "id": "elena_urgent_letter_request",
      "requestCards": ["elena_letter_request"],
      "promiseCards": ["elena_priority_promise"]
    }
  ]
}
```

**Why**: During NPCRequest initialization, the system validates that all referenced cards exist and throws a `PackageLoadException` if they don't.

### 2. NPCs and Their Initial Decks

**Rule**: NPCs must be in the same package as cards referenced in their initial decks.

**Entities that must be together**:
- NPC definitions
- Cards referenced in `conversationDeck[]`
- Cards referenced in `requestDeck[]`
- Cards referenced in `exchangeDeck[]`
- Cards referenced in `observationDeck[]`
- Cards referenced in `burdenDeck[]`

**Example**:
```json
// These MUST be in the same package:
{
  "npcs": [
    {
      "id": "elena",
      "persistentDecks": {
        "conversationDeck": ["elena_intro", "elena_gossip"],
        "requestDeck": ["elena_letter_request"]
      }
    }
  ],
  "cards": [
    { "id": "elena_intro", "type": "Conversation", ... },
    { "id": "elena_gossip", "type": "Conversation", ... },
    { "id": "elena_letter_request", "type": "Request", ... }
  ]
}
```

**Why**: NPC initialization populates their decks immediately, requiring all referenced cards to exist.

### 3. Letters and Their Card References

**Rule**: Letters must be in the same package as any cards they create or reference.

**Entities that must be together**:
- Letter definitions
- Embedded card definitions in letter content
- Cards referenced by letter effects

**Example**:
```json
// These MUST be in the same package:
{
  "letters": [
    {
      "id": "urgent_letter",
      "cardRewards": ["urgent_reward_card"]
    }
  ],
  "cards": [
    { "id": "urgent_reward_card", "type": "Promise", ... }
  ]
}
```

**Why**: Letter processing creates cards immediately upon delivery.

### 4. Card Effects and Target Cards

**Rule**: Cards with effects that reference other cards must be in the same package as their targets.

**Entities that must be together**:
- Cards with `UnlockCard` effects
- Cards with `AddCardToDeck` effects
- The target cards referenced in these effects

**Example**:
```json
// These MUST be in the same package:
{
  "cards": [
    {
      "id": "unlock_secret",
      "effect": {
        "type": "UnlockCard",
        "targetCardId": "secret_knowledge"
      }
    },
    { "id": "secret_knowledge", "type": "Conversation", ... }
  ]
}
```

**Why**: Card effects are validated during initialization.

### 5. Deck Compositions and Referenced Cards

**Rule**: Deck composition templates must be in the same package as all cards they reference.

**Entities that must be together**:
- Deck composition definitions
- All cards referenced in the composition counts

**Example**:
```json
// These MUST be in the same package:
{
  "deckCompositions": {
    "npcDecks": {
      "elena": {
        "conversationDeck": {
          "elena_intro": 2,
          "elena_gossip": 1
        }
      }
    }
  },
  "cards": [
    { "id": "elena_intro", ... },
    { "id": "elena_gossip", ... }
  ]
}
```

**Why**: Deck compositions are applied immediately during package loading.

## Acceptable Separations

### 1. NPCs and Locations

NPCs can reference locations in other packages. The system will create skeleton locations if needed.

**Example**:
```json
// Package A - OK to be separate
{ "npcs": [{ "id": "elena", "locationId": "market_square" }] }

// Package B - Loaded later
{ "locations": [{ "id": "market_square", ... }] }
```

**Resolution**: A skeleton location is created for "market_square" when Package A loads, then replaced when Package B loads.

### 2. Observation Cards and Target NPCs

Observation cards can target NPCs in other packages.

**Example**:
```json
// Package A - OK to be separate
{
  "cards": [{
    "id": "observe_merchant",
    "targetNpcId": "marcus",
    "targetDeck": "observation"
  }]
}

// Package B - Loaded later
{ "npcs": [{ "id": "marcus", ... }] }
```

**Resolution**: Cards are added to NPC decks when the NPC becomes available.

### 3. Location Spots and Their Properties

Location spots can be defined separately from their parent locations if the location uses skeleton generation.

## Package Organization Guidelines

### Recommended Package Structure

1. **Core Cards Package** (`cards_and_decks_package.json`)
   - All card definitions
   - NPCRequests (since they reference cards)
   - Deck compositions
   - Card-related progression

2. **Core Game Package** (`core_game_package.json`)
   - NPCs (without embedded cards)
   - Locations
   - Game rules
   - Currency exchanges

3. **Content Packages** (story-specific)
   - Letters with their associated cards
   - Story-specific NPCs and their cards
   - Quest-related content bundles

### Anti-Patterns to Avoid

❌ **Don't split related cards across packages**
```json
// WRONG - Promise card in different package than request
// Package A: { "cards": [{ "id": "request_1", "type": "Request" }] }
// Package B: { "cards": [{ "id": "promise_1", "type": "Promise" }] }
```

❌ **Don't separate NPCRequests from their cards**
```json
// WRONG - NPCRequest can't find its cards
// Package A: { "npcRequests": [{ "promiseCards": ["promise_1"] }] }
// Package B: { "cards": [{ "id": "promise_1" }] }
```

❌ **Don't split deck compositions from cards**
```json
// WRONG - Deck composition can't find cards
// Package A: { "deckCompositions": { "elena": { "card_1": 2 } } }
// Package B: { "cards": [{ "id": "card_1" }] }
```

## Validation Checklist

Before splitting content across packages, verify:

- [ ] All cards referenced by NPCRequests are in the same package
- [ ] All cards in NPC initial decks are in the same package as the NPC
- [ ] All cards referenced by letters are in the same package
- [ ] All target cards for card effects are in the same package
- [ ] All cards in deck compositions are in the same package

## Error Resolution

If you encounter a `PackageLoadException` with missing dependencies:

1. Identify the missing entity type and ID from the error message
2. Locate both the referencing entity and the referenced entity
3. Move them to the same package (prefer moving to the package with more related content)
4. Rebuild and test

## Future Considerations

The current atomic loading system may be enhanced with:
- Deferred validation for cross-package references
- Multi-phase loading within packages
- Dependency graph analysis for automatic ordering

However, until such enhancements are implemented, strict cohesion rules must be followed to ensure reliable content loading.