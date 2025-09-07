# POC Content Implementation Plan

## Overview
This document outlines the complete content needed for the Elena's Letter POC scenario. The POC demonstrates all three core game loops: card-based conversations, obligation queue management, and location/travel systems.

## Core Design: Shared Base Deck System

All NPCs draw from a single base conversation deck, with cards filtered by personality type. This approach:
- Reduces content duplication
- Ensures mechanical consistency
- Allows personality-driven variation
- Simplifies balancing

Each card includes a `personalityTypes` array that determines which NPCs can use it:
- `DEVOTED` - Trust-focused (Elena)
- `MERCANTILE` - Commerce-focused (Marcus)
- `STEADFAST` - Balanced utility (Bertram)
- `ALL` - Universal cards available to everyone

## Content Requirements

### 1. Base Conversation Deck (~45 cards)

#### Trust-Focused Cards (15 cards)
**Personality Types: ["DEVOTED", "ALL"]**
- "I understand" - Focus 1, Easy, +1 rapport
- "Share sympathy" - Focus 2, Medium, +2 rapport
- "Express deep trust" - Focus 3, Hard, +3 rapport
- "Gentle reassurance" - Focus 0, Easy, Set Patient atmosphere
- "Share vulnerability" - Focus 4, Very Hard, +4 rapport, Impulse
- "Build on trust" - Focus 3, Hard, Scale by Trust tokens
- "Desperate plea" - Focus 3, Hard, Scale by (20 - rapport) / 5, Impulse
- "Patient approach" - Focus 2, Hard, Scale by (patience / 3)
- "Listen carefully" - Focus 1, Medium, Draw 1 card
- "Open mind" - Focus 0, Easy, Set Receptive atmosphere

#### Commerce-Focused Cards (15 cards)
**Personality Types: ["MERCANTILE", "ALL"]**
- "Fair deal" - Focus 1, Easy, +1 rapport
- "Highlight opportunity" - Focus 2, Medium, +2 rapport
- "Leverage connections" - Focus 3, Hard, Scale by Commerce tokens
- "Final offer" - Focus 5, Very Hard, +5 rapport, Impulse
- "Prepare argument" - Focus 3, Medium, Add 2 focus
- "Business proposition" - Focus 2, Medium, +2 rapport
- "Market knowledge" - Focus 1, Easy, Draw 1 card
- "Trade secrets" - Focus 3, Hard, Set Informed atmosphere
- "Negotiate terms" - Focus 2, Medium, Add 1 focus
- "Close the deal" - Focus 4, Hard, +4 rapport

#### Utility Cards (15 cards)
**Personality Types: ["STEADFAST", "ALL"]**
- "Gather thoughts" - Focus 2, Medium, Draw 2 cards
- "Build momentum" - Focus 2, Medium, Add 1 focus
- "Take a breath" - Focus 0, Easy, Set Patient atmosphere
- "Change subject" - Focus 1, Easy, Set Neutral atmosphere
- "Press advantage" - Focus 3, Medium, Add 2 focus
- "Find common ground" - Focus 2, Medium, +2 rapport
- "Steady approach" - Focus 1, Easy, +1 rapport
- "Clear the air" - Focus 1, Medium, Reset atmosphere
- "Make time" - Focus 2, Medium, Draw 1 card
- "Stay focused" - Focus 0, Easy, Set Focused atmosphere

#### Universal Cards (ALL personality types)
- "Interrupt" - Focus 1, Hard, Set Receptive, Opening
- "Final statement" - Focus 5, Very Hard, +5 rapport, Set Final, Impulse
- "Quick response" - Focus 1, Easy, +1 rapport, Opening
- "Thoughtful pause" - Focus 1, Medium, Draw 1 card

### 2. NPC Observation Deck Cards

#### Safe Passage Knowledge (Elena's Observation Deck)
```json
{
  "id": "safe_passage_knowledge",
  "name": "Safe Passage Knowledge",
  "type": "ObservationCard",
  "focus": 0,
  "persistence": "Persistent",
  "description": "Knowledge of merchant caravan routes calms Elena's panic",
  "effect": {
    "type": "AdvanceEmotionalState",
    "targetState": "Neutral"
  },
  "npcDeck": "elena_observation"
}
```

#### Merchant Caravan Route (Marcus's Observation Deck)
```json
{
  "id": "merchant_caravan_route",
  "name": "Merchant Caravan Route",
  "type": "ObservationCard",
  "focus": 0,
  "persistence": "Persistent",
  "description": "Detailed knowledge of Marcus's private caravan schedule",
  "effect": {
    "type": "UnlockExchange",
    "exchangeId": "marcus_caravan_transport"
  },
  "npcDeck": "marcus_observation"
}
```

### 3. Request Cards

#### Elena's Urgent Letter
```json
{
  "id": "elena_urgent_refusal",
  "type": "Request",
  "focus": 5,
  "startingState": "Unplayable",
  "becomesPlayableAt": 5,
  "properties": ["GainsImpulseAndOpeningWhenPlayable"],
  "description": "Please take my letter!",
  "difficulty": "VeryHard",
  "successEffect": {
    "type": "CreateObligation",
    "obligationType": "delivery",
    "position": "nextAvailable",
    "deadline": 300,
    "recipient": "Lord Blackwood",
    "noPayment": true
  },
  "failureEffect": {
    "type": "AddBurdenCard",
    "count": 1
  },
  "exhaustEffect": {
    "type": "EndConversation",
    "result": "Failed to help Elena"
  }
}
```

### 4. Exchange Cards

#### Marcus's Trade Cards
- "Buy Food" - 2 coins → Reset hunger to 0
- "Join Merchant Caravan" - 10 coins → One-time transport to Noble Quarter (requires 2 Commerce tokens + "Merchant Caravan Route" played)

#### Guard's Permit Card
- "Noble District Permit" - 20 coins → Unlocks checkpoint route (deliberate dead end)

### 5. Location Spot Actions

#### Market Square Actions
- "Investigate" - Spend 1 attention → Gain familiarity (scaled by spot property: Quiet=+2, Busy=+1)
- "Observe" - Spend 0 attention → Gain observation card to NPC deck (requires familiarity)
- "Work: Haul Goods" - Spend 2 attention → Gain coins (5 - floor(hunger/25))

#### Copper Kettle Actions
- "Investigate" - Spend 1 attention → Gain +1 familiarity
- "Rest by Fire" - Spend 5 coins → Restore stamina

#### Noble Quarter Actions
- No special actions (destination only)

### 6. Location Structure

#### Market Square
- **fountain** (Quiet in morning, Busy in afternoon, Closing in evening)
  - Investigation scaling varies by time
  - Observation point for both cards
- **merchant_row** (Commercial)
  - Marcus location
  - Work action available
- **guard_post** (Authority)
  - Guard Captain location
  - Permit exchange available (20 coins)

#### Copper Kettle Tavern
- **common_room** (Public)
  - Travel hub spot
- **corner_table** (Private)
  - Elena location (always available)
- **bar_counter** (Service)
  - Bertram location (not used in POC)

#### Noble District
- **gate_entrance** (Guarded)
  - Checkpoint location
- **blackwood_manor** (Noble)
  - Lord Blackwood location

#### Warehouse District
- **warehouse_entrance** (Commercial)
  - Marcus's letter recipient

### 7. Travel Routes

#### Primary Routes
1. **Market Square ↔ Copper Kettle**
   - 15 minutes, free
   - Always available

2. **Market Square ↔ Warehouse District**
   - 20 minutes, free
   - Always available

3. **Market Square → Noble District (Checkpoint)**
   - 25 minutes, free
   - Requires: Noble District Permit

4. **Market Square → Noble District (Caravan)**
   - 20 minutes, 10 coins
   - Requires: Marcus exchange unlocked via observation card

## Implementation Priority

### Phase 1: Core Conversation System
1. Implement base deck with personality filtering
2. Add NPC observation decks
3. Add Elena's urgent letter request
4. Test conversation flow with Elena
5. Verify rapport system and flow tracking

### Phase 2: Location Mechanics
1. Implement familiarity system (0-3 per location)
2. Add investigation action (scales with spot properties)
3. Add observation system (requires familiarity, costs 0 attention)
4. Test familiarity progression

### Phase 3: Economy Loop
1. Add Marcus's exchange cards
2. Implement token-gated exchanges
3. Add work action with hunger scaling
4. Test resource management

### Phase 4: Travel & Integration
1. Create all location spots
2. Implement both Noble District routes
3. Add complete observation flow
4. Test complete gameplay loop

## Success Criteria

The POC is complete when:
1. Player can investigate Market Square to build familiarity
2. First observation adds "Safe Passage Knowledge" to Elena's deck
3. Elena conversation becomes possible with observation card
4. Elena's request becomes playable at appropriate focus capacity
5. Second observation adds "Merchant Caravan Route" to Marcus's deck
6. Marcus's caravan exchange unlocks when card played
7. Noble District accessible via caravan OR impossible permit
8. Work scales with hunger (work while fed gives more coins)
9. All three core loops demonstrated in single playthrough

## Key Narrative Beats

1. **Opening**: Player at Market Square with Viktor's package, Elena desperate at tavern
2. **Discovery**: Investigation reveals knowledge that helps specific NPCs
3. **Crisis**: Elena needs trust through demonstrated knowledge of escape routes
4. **Challenge**: Building infrastructure before attempting main quest
5. **Obstacle**: Getting to Noble District (permit too expensive, need caravan)
6. **Resolution**: Deliver letter before 5 PM deadline
7. **Consequence**: Elena's fate determined by player success

This content package provides a complete, focused POC demonstrating all core Wayfarer mechanics with the refined investigation/observation system where discovered knowledge goes directly to relevant NPC observation decks.