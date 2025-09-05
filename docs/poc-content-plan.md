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
- "Disconnected plea" - Focus 3, Hard, Scale by (20 - rapport) / 5, Impulse
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

### 2. Request Cards

#### Elena's Urgent Letter (CRITICAL)
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
    "position": 1,
    "deadline": 60,
    "payment": 5,
    "tokenReward": {"Trust": 2}
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

### 3. Exchange Cards

#### Marcus's Trade Cards
- "Trade Goods" - 5 coins for common item
- "Bulk Purchase" - 10 coins for 3 items
- "Special Deal" - 8 coins for rare item

#### Bertram's Service Cards
- "Hot Meal" - 3 coins → -20 hunger
- "Room for Night" - 5 coins → full stamina + health
- "Mug of Ale" - 2 coins → +3 patience
- "Information" - 1 coin → reveal observation

#### Guard's Permit Card
- "Noble District Permit" - 10 coins → unlocks main gate route

### 4. Location Spot Actions

#### Market Square Actions
- "Work: Haul Goods" - Spend 2 attention → Gain 8 coins
- "Work: Scribe Letters" - Spend 3 attention → Gain 12 coins
- "Observe: Guard Patterns" - Spend 1 attention → Gain observation card
- "Observe: Merchant Routes" - Spend 1 attention → Gain route knowledge

#### Copper Kettle Actions
- "Rest by Fire" - Spend 5 coins → Restore stamina
- "Order Food" - Spend 3 coins → Reduce hunger by 20
- "Listen to Gossip" - Spend 1 attention → Gain observation

#### Noble District Actions
- "Work: Noble Errands" - Spend 2 attention → Gain 15 coins
- "Observe: Estate Schedules" - Spend 1 attention → Gain timing knowledge

### 5. Observation Cards (Player Deck Additions)

These cards are added to the player's conversation deck when observations are made:

- **"Merchant Route Knowledge"**
  - Focus: 1, Very Easy
  - Effect: Unlocks secret merchant route to Noble District
  - Expires: Never

- **"Guard Shift Patterns"**
  - Focus: 1, Very Easy
  - Effect: Set Pressured atmosphere
  - Expires: 24 hours

- **"Elena's Desperation"**
  - Focus: 1, Very Easy
  - Effect: Set Informed atmosphere (next card auto-succeeds)
  - Expires: 48 hours

- **"Crowded Market"**
  - Focus: 1, Very Easy
  - Effect: Next action costs 0 patience
  - Expires: 4 hours

- **"Emergency Rapport"**
  - Focus: 1, Very Easy
  - Effect: Set rapport to 15
  - Expires: 12 hours

### 6. Location Structure

#### Market Square
- **central_fountain** (crossroads, public, -1 patience)
  - Travel hub spot
  - Work actions available
- **merchant_row** (commercial, busy)
  - Marcus location
- **guard_post** (authority, guarded)
  - Guard Captain location
  - Permit exchange available
- **north_alcove** (discrete, private, +1 patience)
  - Hidden observation point

#### Copper Kettle Tavern
- **main_hall** (crossroads, public, -1 patience)
  - Travel hub spot
- **corner_table** (private, quiet, +1 patience)
  - Elena location (afternoon only)
- **bar_counter** (service, social)
  - Bertram location
  - All tavern services

#### Noble District
- **gate_entrance** (crossroads, guarded)
  - Travel hub spot
  - Checkpoint location
- **blackwood_estate** (noble, exclusive)
  - Lord Blackwood location

#### Courier Office
- **office_desk** (crossroads, private)
  - Starting location
  - Travel hub spot

### 7. Travel Routes

#### Primary Routes
1. **Courier Office ↔ Market Square**
   - 10 minutes, free
   - Always available

2. **Market Square ↔ Copper Kettle**
   - 15 minutes, free
   - Always available

3. **Market Square → Noble District (Main Gate)**
   - 25 minutes, free
   - Requires: Noble District Permit

4. **Market Square → Noble District (Merchant Route)**
   - 35 minutes, free
   - Requires: Merchant Route Knowledge (observation)
   - Discrete, avoids guards

5. **Copper Kettle ↔ Courier Office**
   - 20 minutes, free
   - Always available

## Implementation Priority

### Phase 1: Core Conversation System
1. Implement base deck with personality filtering
2. Add Elena's urgent letter request
3. Test conversation flow with Elena
4. Verify rapport system and flow tracking

### Phase 2: Economy Loop
1. Add Marcus's exchange cards
2. Add Bertram's service cards
3. Implement location spot actions
4. Test resource management

### Phase 3: Travel & Exploration
1. Create all location spots
2. Implement both Noble District routes
3. Add observation system
4. Test complete gameplay loop

### Phase 4: Polish
1. Balance card focuses and difficulties
2. Tune economy values
3. Adjust timing for tension
4. Verify all three loops integrate

## Success Criteria

The POC is complete when:
1. Player can start conversation with disconnected Elena
2. Elena's request becomes playable at appropriate focus capacity
3. Request gains Impulse+Opening when playable
4. Player manages rapport to improve success chances
5. Flow tracks success/failure correctly
6. Starting rapport equals connection tokens
7. Noble District accessible via permit OR secret route
8. Economy loop supports gameplay (work → coins → services)
9. All three core loops demonstrated in single playthrough

## Key Narrative Beats

1. **Opening**: Player in Market Square, Elena disconnected at Copper Kettle
2. **Crisis**: Elena's letter has fixed urgent terms (1hr, position 1)
3. **Challenge**: Building rapport for request success
4. **Obstacle**: Getting to Noble District (permit or knowledge)
5. **Resolution**: Deliver letter before sunset deadline
6. **Consequence**: Elena's fate determined by player success

This content package provides a complete, focused POC demonstrating all core Wayfarer mechanics with the refined rapport/flow system.