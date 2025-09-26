# Elena's Letter POC Implementation Plan

## Overview

This document outlines the complete implementation plan for updating Wayfarer's content and systems to support the refined Elena's Letter POC scenario. The plan addresses both JSON content updates and missing core systems identified through pipeline analysis.

## Table of Contents

1. [Critical Issues Identified](#critical-issues-identified)
2. [Content File Updates](#content-file-updates)
3. [Missing Systems Implementation](#missing-systems-implementation)
4. [Validation Strategy](#validation-strategy)
5. [Implementation Order](#implementation-order)

## Critical Issues Identified

### 1. Foundation Deck Balance Crisis
**Problem**: Current foundation cards don't support Level 1 gameplay
- **Current**: Only 5 depth 1-2 cards available to Level 1 players
- **Required**: 15+ foundation cards with 80% generating Initiative
- **Impact**: Players can't build Initiative to play higher-cost cards

### 2. Depth Filtering System Missing
**Problem**: No stat-based card access restrictions implemented
- **Current**: All cards available regardless of player stat levels
- **Required**: Level 1 players limited to depths 1-2 cards only
- **Pipeline Status**: JSON depth values exist, parser works, but no filtering in conversation system

### 3. Investigation System Incomplete
**Problem**: Market Square familiarity → observation unlock chain broken
- **Current**: Observations exist in JSON but no investigation mechanics
- **Required**: Investigation actions build familiarity, unlock observation cards
- **Missing**: Stat-gated investigation approaches (Insight 2+, Rapport 2+, etc.)

### 4. XP System Misconfiguration
**Problem**: Elena gives wrong XP multiplier
- **Current**: Elena conversation difficulty = 2 (2 XP per card)
- **Required**: Elena conversation difficulty = 3 (3 XP per card)
- **Pipeline Status**: XP calculation works correctly, just wrong JSON value

### 5. Starting Conditions Wrong
**Problem**: Player starts with incorrect resources and obligations
- **Current**: 10 coins, Viktor package to warehouse recipient
- **Required**: 0 coins, Viktor package to Marcus for 7 coins
- **Impact**: Breaks entire economic flow of the scenario

## Content File Updates

### 05_gameplay.json - Starting Conditions

#### Current Problems
```json
"playerConfig": {
  "coins": 10,           // WRONG - should be 0
  "health": 100,         // CORRECT
  "hunger": 50,          // CORRECT
},
"startingObligations": [{
  "recipientId": "warehouse_recipient",  // WRONG - should be marcus
  "payment": 10,                        // WRONG - should be 7
  "deadline": 480,                      // CORRECT (12:00 PM)
}]
```

#### Required Changes
```json
"playerConfig": {
  "coins": 0,                    // START WITH NO MONEY
  "health": 100,
  "hunger": 50,
  "satchelCapacity": 10,         // ADD - carrying capacity
  "satchelWeight": 3             // ADD - Viktor's package weight
},
"startingObligations": [{
  "recipientId": "marcus",       // CHANGE - deliver to Marcus
  "payment": 7,                  // CHANGE - reduced payment
  "deadline": 480,               // 12:00 PM deadline
  "weight": 3,                   // ADD - package weight
  "description": "Viktor's package for Marcus at Market Square"
}]
```

### 03_npcs.json - NPC Updates

#### Elena Updates
```json
{
  "id": "elena",
  "name": "Elena",
  "conversationDifficulty": 3,    // CHANGE from 2 - gives 3 XP per card
  "personalityType": "DEVOTED",   // CORRECT - +2 doubt instead of +1
  "signatureCards": [             // ADD - Trust token requirements
    {
      "tokenRequirement": 1,
      "cardId": "elena_faith",
      "name": "Elena's Faith",
      "stat": "rapport",
      "depth": 3
    },
    {
      "tokenRequirement": 3,
      "cardId": "shared_understanding",
      "stat": "rapport",
      "depth": 4
    },
    {
      "tokenRequirement": 6,
      "cardId": "elena_trust",
      "stat": "authority",
      "depth": 5
    },
    {
      "tokenRequirement": 10,
      "cardId": "emotional_bond",
      "stat": "rapport",
      "depth": 6
    }
  ],
  "requests": [{
    "id": "elena_letter_request",
    "goalThresholds": {           // ADD - tiered goals
      "basic": 8,                 // Letter weight 1, deadline 5 PM
      "enhanced": 12,             // Priority letter weight 2, +1 Trust token
      "premium": 16               // Legal documents weight 3, +2 Trust tokens
    }
  }]
}
```

#### Marcus Updates
```json
{
  "id": "marcus",
  "name": "Marcus",
  "conversationDifficulty": 2,    // CORRECT - gives 2 XP per card
  "personalityType": "MERCANTILE", // CORRECT - highest Initiative card double effect
  "signatureCards": [             // ADD - Commerce token requirements
    {
      "tokenRequirement": 1,
      "cardId": "marcus_bargain",
      "stat": "commerce",
      "depth": 3
    },
    {
      "tokenRequirement": 3,
      "cardId": "trade_knowledge",
      "stat": "commerce",
      "depth": 4
    },
    {
      "tokenRequirement": 6,
      "cardId": "commercial_trust",
      "stat": "commerce",
      "depth": 5
    },
    {
      "tokenRequirement": 10,
      "cardId": "marcus_favor",
      "stat": "authority",
      "depth": 6
    }
  ],
  "exchanges": [                  // ADD - purchase options
    {
      "id": "buy_food",
      "cost": {"coins": 2},
      "reward": {"hunger": -50},
      "name": "Buy Food"
    },
    {
      "id": "buy_bread",
      "cost": {"coins": 3},
      "reward": {"item": "bread", "weight": 1},
      "name": "Buy Bread"
    },
    {
      "id": "join_caravan",
      "cost": {"coins": 10},
      "requirement": {"Commerce": 2},
      "reward": {"transport": "noble_quarter"},
      "name": "Join Caravan to Noble Quarter"
    }
  ]
}
```

#### Stranger NPC Replacements
Replace current strangers with POC-specific ones:

```json
{
  "id": "tea_vendor_morning",
  "name": "Tea Vendor",
  "level": 1,                    // 1 XP per card played
  "personalityType": "STEADFAST", // All effects capped at ±2
  "locationId": "market_square",
  "timeBlock": "morning",
  "rewards": {
    "basic": {"coins": 2},
    "enhanced": {"coins": 4},
    "premium": {"item": "tea", "effect": "+2 Initiative in conversations"}
  }
},
{
  "id": "pilgrim_morning",
  "name": "Pilgrim",
  "level": 1,
  "personalityType": "DEVOTED",   // Doubt increases +2
  "locationId": "market_square",
  "timeBlock": "morning",
  "rewards": {
    "basic": {"blessing": "+1 starting Initiative today"},
    "enhanced": {"observation": "reveals_one_observation"},
    "premium": {"item": "holy_symbol", "weight": 1}
  }
},
{
  "id": "traveling_scholar_midday",
  "name": "Traveling Scholar",
  "level": 2,                    // 2 XP per card played
  "personalityType": "STEADFAST",
  "locationId": "copper_kettle_tavern",
  "timeBlock": "midday",
  "rewards": {
    "basic": {"info": "route_information"},
    "enhanced": {"coins": 5},
    "premium": {"training": "next_10_insight_cards_+1_xp"}
  }
},
{
  "id": "foreign_merchant_afternoon",
  "name": "Foreign Merchant",
  "level": 3,                    // 3 XP per card played
  "personalityType": "MERCANTILE", // Highest Initiative card double effect
  "locationId": "market_square",
  "timeBlock": "afternoon",
  "rewards": {
    "basic": {"item": "trade_goods", "weight": 2, "value": 8},
    "enhanced": {"item": "exotic_permit", "weight": 1},
    "premium": {"partnership": "-1_coin_all_exchanges_today"}
  }
}
```

### 02_cards.json - Card Updates

#### Foundation Cards Addition (10+ new cards)
All depth 1-2, initiativeCost 0, 80% must give Initiative:

```json
{
  "id": "rapport_gentle_nod",
  "depth": 1,
  "initiativeCost": 0,
  "boundStat": "rapport",
  "effects": {"success": {"Initiative": 1}},
  "description": "A subtle acknowledgment that builds conversational energy."
},
{
  "id": "insight_careful_observation",
  "depth": 1,
  "initiativeCost": 0,
  "boundStat": "insight",
  "effects": {"success": {"Initiative": 2}},
  "description": "Careful observation opens new conversational paths."
},
{
  "id": "authority_confident_presence",
  "depth": 1,
  "initiativeCost": 0,
  "boundStat": "authority",
  "effects": {"success": {"Initiative": 1, "Doubt": 1}},
  "description": "Confident bearing commands attention but creates tension."
},
{
  "id": "commerce_value_assessment",
  "depth": 1,
  "initiativeCost": 0,
  "boundStat": "commerce",
  "effects": {"success": {"Initiative": 1, "DrawCards": 1}},
  "description": "Quick assessment of mutual benefit opens possibilities."
},
{
  "id": "cunning_subtle_inquiry",
  "depth": 1,
  "initiativeCost": 0,
  "boundStat": "cunning",
  "effects": {"success": {"Initiative": 2, "Doubt": -1}},
  "description": "Indirect questioning builds understanding while reducing tension."
},
{
  "id": "rapport_empathetic_response",
  "depth": 2,
  "initiativeCost": 0,
  "boundStat": "rapport",
  "effects": {"success": {"Initiative": 1, "Momentum": 1}},
  "description": "Understanding response that builds both energy and progress."
},
{
  "id": "insight_pattern_recognition",
  "depth": 2,
  "initiativeCost": 0,
  "boundStat": "insight",
  "effects": {"success": {"Initiative": 3}},
  "description": "Recognizing patterns provides significant conversational advantage."
},
{
  "id": "authority_clear_direction",
  "depth": 2,
  "initiativeCost": 0,
  "boundStat": "authority",
  "effects": {"success": {"Initiative": 2, "Momentum": 1}},
  "description": "Clear direction builds momentum through decisive action."
},
{
  "id": "commerce_mutual_benefit",
  "depth": 2,
  "initiativeCost": 0,
  "boundStat": "commerce",
  "effects": {"success": {"Initiative": 1, "Momentum": 2}},
  "description": "Identifying mutual benefit creates strong forward progress."
},
{
  "id": "cunning_strategic_pause",
  "depth": 2,
  "initiativeCost": 0,
  "boundStat": "cunning",
  "effects": {"success": {"Initiative": 2, "DrawCards": 1}},
  "description": "Strategic silence opens new conversational options."
}
```

#### Signature Cards for Elena (4 Trust-gated)
```json
{
  "id": "elena_faith",
  "type": "Signature",
  "depth": 3,
  "initiativeCost": 2,
  "boundStat": "rapport",
  "tokenRequirement": {"Trust": 1},
  "effects": {"success": {"Momentum": 3}},
  "description": "Elena's growing faith in you strengthens your words."
},
{
  "id": "shared_understanding",
  "type": "Signature",
  "depth": 4,
  "initiativeCost": 3,
  "boundStat": "rapport",
  "tokenRequirement": {"Trust": 3},
  "effects": {"success": {"Momentum": 4, "Doubt": -1}},
  "description": "Deep understanding allows for powerful, reassuring communication."
},
{
  "id": "elena_trust",
  "type": "Signature",
  "depth": 5,
  "initiativeCost": 4,
  "boundStat": "authority",
  "tokenRequirement": {"Trust": 6},
  "effects": {"success": {"Momentum": 5}},
  "description": "Elena's complete trust allows you to speak with absolute authority."
},
{
  "id": "emotional_bond",
  "type": "Signature",
  "depth": 6,
  "initiativeCost": 5,
  "boundStat": "rapport",
  "tokenRequirement": {"Trust": 10},
  "effects": {"success": {"Momentum": 6, "Doubt": -3}},
  "description": "The emotional bond between you creates profound understanding."
}
```

#### Signature Cards for Marcus (4 Commerce-gated)
```json
{
  "id": "marcus_bargain",
  "type": "Signature",
  "depth": 3,
  "initiativeCost": 2,
  "boundStat": "commerce",
  "tokenRequirement": {"Commerce": 1},
  "effects": {"success": {"Momentum": 2, "DrawCards": 1}},
  "description": "Marcus appreciates your business sense."
},
{
  "id": "trade_knowledge",
  "type": "Signature",
  "depth": 4,
  "initiativeCost": 3,
  "boundStat": "commerce",
  "tokenRequirement": {"Commerce": 3},
  "effects": {"success": {"Momentum": 4}},
  "description": "Shared trade knowledge creates powerful business connections."
},
{
  "id": "commercial_trust",
  "type": "Signature",
  "depth": 5,
  "initiativeCost": 4,
  "boundStat": "commerce",
  "tokenRequirement": {"Commerce": 6},
  "effects": {"success": {"Momentum": 5, "DrawCards": 1}},
  "description": "Marcus's commercial trust opens new opportunities."
},
{
  "id": "marcus_favor",
  "type": "Signature",
  "depth": 6,
  "initiativeCost": 5,
  "boundStat": "authority",
  "tokenRequirement": {"Commerce": 10},
  "effects": {"success": {"Momentum": 6}},
  "description": "Marcus's favor grants you significant influence in negotiations."
}
```

#### Observation Cards
```json
{
  "id": "safe_passage_knowledge",
  "type": "Observation",
  "depth": 3,
  "initiativeCost": 2,
  "boundStat": "insight",
  "effects": {"success": {"Momentum": 4}},
  "description": "Knowledge of safe routes provides crucial conversational advantage with Elena.",
  "triggerFamiliarity": 1,
  "location": "market_square"
},
{
  "id": "merchant_caravan_route",
  "type": "Observation",
  "depth": 3,
  "initiativeCost": 3,
  "boundStat": "commerce",
  "effects": {"success": {"UnlockExchange": "caravan_transport"}},
  "description": "Understanding caravan routes unlocks Marcus's transport option.",
  "triggerFamiliarity": 2,
  "location": "market_square"
}
```

### 01_foundation.json - Investigation System

#### Market Square Investigation
```json
{
  "id": "market_square",
  "investigation": {
    "baseAction": {
      "timeSegments": 1,
      "familiarityGain": {
        "morning": 2,     // Quiet bonus
        "default": 1      // Busy times
      }
    },
    "statGatedApproaches": {
      "insight": {
        "levelRequired": 2,
        "name": "Systematic Observation",
        "bonus": {"familiarity": 1},
        "description": "Methodical analysis reveals additional patterns"
      },
      "rapport": {
        "levelRequired": 2,
        "name": "Local Inquiry",
        "effect": "learn_npc_observation_preferences",
        "description": "Friendly conversation reveals what NPCs want to hear"
      },
      "commerce": {
        "levelRequired": 2,
        "name": "Purchase Information",
        "cost": {"coins": 2},
        "reward": {"familiarity": 1},
        "description": "Pay for insider information"
      }
    },
    "observationUnlocks": {
      "familiarity_1": "safe_passage_knowledge",
      "familiarity_2": "merchant_caravan_route"
    }
  }
}
```

## Missing Systems Implementation

### 1. Conversation Deck Filtering System

**File**: `src/Subsystems/Conversation/ConversationDeckBuilder.cs` (NEW)

**Purpose**: Filter conversation type decks based on player stat levels

**Requirements**:
- Level 1 players: Access depths 1-2 only
- Level 2 players: Access depths 1-3
- Level 3 players: Access depths 1-4
- etc.

**Integration Point**: `ConversationFacade.StartConversation()` must call deck builder

### 2. Investigation System

**File**: `src/Subsystems/Investigation/InvestigationFacade.cs` (NEW)

**Purpose**: Handle location investigation, familiarity building, observation unlocking

**Requirements**:
- Base investigation: +1 familiarity, 1 time segment
- Time-of-day bonuses: Morning at Market Square = +2 familiarity
- Stat-gated approaches: Additional bonuses for Level 2+ stats
- Observation unlock: Check familiarity thresholds, add cards to NPC decks

### 3. Player Weight System

**File**: `src/GameState/Player.cs` (MODIFY)

**Purpose**: Track carrying capacity, prevent overloading

**Requirements**:
- MaxCarryingCapacity property (default 10)
- CurrentWeight calculation from inventory + obligations
- CanCarry(additionalWeight) validation
- UI display of weight status

### 4. Exchange System Integration

**File**: `src/Subsystems/Exchange/ExchangeFacade.cs` (MODIFY)

**Purpose**: Connect NPC exchange options to actual mechanics

**Requirements**:
- Marcus food purchase: 2 coins → -50 hunger
- Marcus bread purchase: 3 coins → bread item (weight 1)
- Marcus caravan: 10 coins + 2 Commerce tokens → instant Noble Quarter transport

### 5. Time/Deadline System

**File**: `src/Subsystems/Time/TimeManager.cs` (MODIFY)

**Purpose**: Enforce NPC departures and quest deadlines

**Requirements**:
- Lord Blackwood departure at 5:00 PM
- Deadline validation for letter delivery
- UI warnings as deadlines approach

## Validation Strategy

### 1. Pipeline Testing

For each major change, verify the complete pipeline:

1. **JSON → Parser**: Values correctly extracted from JSON
2. **Parser → Model**: Domain models populated with correct data
3. **Model → Mechanics**: Game systems use model data (no hardcoded values)
4. **Mechanics → UI**: UI displays calculated/dynamic values

### 2. Elena's Letter Scenario Walkthrough

Complete end-to-end test following optimal path:

1. **Start**: 0 coins, Level 1 stats, Viktor package (weight 3/10)
2. **9:00 AM**: Investigate Market Square (+2 familiarity, unlock Safe Passage Knowledge)
3. **12:00 PM**: Marcus conversation (deliver Viktor package, +7 coins, 2 XP per card)
4. **12:20 PM**: Use Commerce tokens for caravan access
5. **2:00 PM**: Elena conversation (3 XP per card, use observation card)
6. **3:00 PM**: Caravan to Noble Quarter, deliver letter to Lord Blackwood
7. **Verify**: Complete in 9-12 segments with proper resource management

### 3. Stat Progression Testing

Verify depth filtering works correctly:

1. **Level 1**: Can only play depth 1-2 cards
2. **Level 2**: Can play depth 1-3 cards
3. **Signature Cards**: Unlock based on token counts, not stat levels
4. **XP Calculation**: NPC conversation difficulty multiplies XP correctly

### 4. Personality Rule Testing

Verify all personality rules work:

1. **Elena (DEVOTED)**: Doubt increases +2 instead of +1
2. **Marcus (MERCANTILE)**: Highest Initiative card gets double effect
3. **Strangers**: STEADFAST caps effects at ±2

## Implementation Order

### Phase 1: Content Updates (Immediate)
1. Fix starting conditions (05_gameplay.json)
2. Fix Elena conversation difficulty (03_npcs.json)
3. Add foundation cards (02_cards.json)
4. Add signature cards (02_cards.json)
5. Add observation cards (02_cards.json)

### Phase 2: Core Systems (Week 1)
1. Implement conversation deck filtering
2. Implement basic investigation system
3. Implement player weight system
4. Connect exchange system to Marcus

### Phase 3: Polish & Validation (Week 2)
1. Add stranger NPC replacements
2. Implement time/deadline system
3. Complete Elena's Letter scenario testing
4. Full pipeline validation

## Success Criteria

✅ **Content Accuracy**: All JSON values match refined POC specifications
✅ **Pipeline Integrity**: No hardcoded values bypass content system
✅ **Gameplay Balance**: Foundation cards enable Level 1 progression
✅ **Scenario Completeness**: Elena's Letter finishable in 9-12 segments
✅ **System Integration**: All mechanics work together seamlessly
✅ **Alternative Paths**: Multiple stat builds offer viable approaches

## Risk Mitigation

### High Risk: Deck Filtering System
- **Risk**: Complex integration with existing conversation system
- **Mitigation**: Implement gradually, test with simple cases first

### Medium Risk: Investigation System
- **Risk**: New system with many integration points
- **Mitigation**: Start with basic familiarity building, add complexity incrementally

### Low Risk: Content Updates
- **Risk**: JSON syntax errors, parsing failures
- **Mitigation**: Validate JSON after each change, test parsing immediately

This implementation plan ensures the refined Elena's Letter POC scenario works exactly as designed while maintaining system integrity and providing clear validation criteria.