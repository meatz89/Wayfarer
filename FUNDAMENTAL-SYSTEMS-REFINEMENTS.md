# Fundamental Systems Refinements - REVISED ARCHITECTURE

## Overview
This document tracks the refined architecture for Wayfarer's core game mechanics. Most foundational systems are implemented - these refinements complete the strategic depth and system interconnections.

## Current System Status

### ✅ Fully Implemented Core Systems
1. **Conversation Action Economy**
   - SPEAK costs focus only (no patience)
   - LISTEN costs 1 patience (unless Patient atmosphere)
   - Implementation: `ConversationOrchestrator.cs`

2. **Rapport System** 
   - Starting rapport = token count × 3
   - Each rapport point = +1% success on all cards
   - Implementation: `RapportManager.cs`

3. **Queue Displacement with Token Burning**
   - Must burn tokens with each displaced NPC
   - Token cost = positions jumped
   - Creates burden cards for damaged relationships
   - Implementation: `DisplacementCalculator.cs`

4. **Observation Chaining**
   - Second observation requires first completed
   - Familiarity requirements enforced
   - Implementation: `ObservationManager.cs:290`

5. **Resource Interdependencies**
   - Attention = 10 - (hunger ÷ 25), minimum 2
   - Work output = 8 - floor(hunger ÷ 25) coins
   - Implementation: `ResourceCalculator.cs`, `ResourceFacade.cs:306-314`

6. **Flow Battery System**
   - -3 to +3 range with state transitions at extremes
   - Persists between conversations
   - Implementation: `FlowManager.cs`, `ConversationSession.FlowBattery`

7. **Path Card Discovery**
   - Face-down cards flip permanently when discovered
   - PathCardDiscoveries dictionary tracks revealed cards
   - Implementation: `TravelFacade.cs`

8. **Exchange Card System**
   - Separate deck from conversation cards
   - Own UI for trade/buy interactions
   - Correctly implemented as separate system, not conversation cards
   - Implementation: `ExchangeOrchestrator.cs`, `ExchangeFacade.cs`

## NEW ARCHITECTURE: NPC One-Time Requests

### Core Concept
NPCs have **One-Time Requests** that are special, narrative-driven asks separate from regular conversations. Each request contains:
- **Request Cards**: Multiple cards with different rapport thresholds offering varying rewards
- **Promise Cards**: Cards that force queue position 1, burning tokens for instant rapport/rewards

### 1. NPC One-Time Request System (HIGH PRIORITY)

**Current State**: Conversation options hardcoded, single goal card per conversation

**Target Architecture**:
```
NPC
├── OneTimeRequests[]
│   ├── Request: "Elena's Urgent Letter"
│   │   ├── RequestCards[]
│   │   │   ├── Card: "Basic Delivery" (5 rapport → normal queue)
│   │   │   ├── Card: "Trusted Delivery" (10 rapport → bonus coins)
│   │   │   └── Card: "Confidant Delivery" (15 rapport → tokens)
│   │   └── PromiseCards[]
│   │       ├── Card: "I'll handle this now!" (0 rapport → position 1, +20 rapport)
│   │       └── Card: "Absolute priority!" (0 rapport → position 1, +30 rapport, +1 token)
│   └── Status: Available/Completed
├── ConversationDeck (regular cards)
└── ExchangeDeck (trade cards - separate system)
```

**Implementation Requirements**:
1. **Data Model Changes**:
   - Add `List<NPCRequest> OneTimeRequests` to NPC class
   - Each NPCRequest contains multiple RequestCards and PromiseCards
   - Track completion status per request

2. **JSON Structure**:
   ```json
   "oneTimeRequests": [
     {
       "id": "elena_urgent_letter",
       "name": "Urgent Letter to Lord Blackwood",
       "requestCards": [...],  // Different rapport thresholds
       "promiseCards": [...]   // Queue manipulation options
     }
   ]
   ```

3. **Conversation Flow**:
   - UI shows one-time requests as conversation options
   - Selecting request loads ALL its cards into hand
   - All cards visible with requirements/rewards shown
   - Completing ANY card marks request as done

### 2. Promise Cards with Queue Manipulation (HIGH PRIORITY)

**Current State**: Promise cards succeed but don't affect queue

**Target Behavior**:
1. Promise card played successfully
2. Creates obligation at position 1 immediately
3. Displaces other obligations, burning tokens with their NPCs
4. Burned tokens convert to instant rapport (5 rapport per token)
5. Optional: Grant connection tokens as additional reward
6. Show queue manipulation in conversation UI real-time

**Implementation Points**:
- `CardDeckManager.PlayCard()`: Handle promise card special effects
- Inject `DisplacementCalculator` to force queue position
- Return queue manipulation info in `CardPlayResult`
- Update UI to show displacement happening

### 3. Multiple Cards Visible Per Request (HIGH PRIORITY)

**Current State**: Single goal card per conversation

**Target State**:
- ALL request cards visible (different rapport thresholds)
- ALL promise cards visible (different queue/token costs)
- Player sees complete "ladder of risk" upfront
- Cards show exact requirements and rewards

**Key Design Principle**: Perfect information - players must see all options and their exact costs/rewards before deciding.

### 4. Remove Legacy Flow System (LOW PRIORITY)

**Remaining Issues**:
- `ConversationSession.cs:119`: `if (CurrentFlow >= 100)` check
- Delete `CurrentFlow` property entirely
- Use only `FlowBattery` (-3 to +3)

### 5. Verify Investigation Scaling (MEDIUM PRIORITY)

**Required Values**:
- QUIET spots: `"investigationScaling": {"familiarity": 2}`
- BUSY spots: `"investigationScaling": {"familiarity": 1}`
- Check all spots in `core_game_package.json`

## Architecture Principles

### Data-Driven Design
- Conversation options come from NPC JSON data
- No hardcoded conversation types in code
- NPCs define what conversations are available
- One-time requests are content, not mechanics

### System Interconnection
```
Conversation → Promise Card → Queue Displacement → Token Burning → Rapport Gain
     ↓              ↓                  ↓                 ↓              ↓
 NPC State    Obligation@1      Other NPCs      Relationships    Success%
```

### Perfect Information
- All cards visible with exact thresholds
- Queue displacement costs shown
- Token burning consequences clear
- No hidden mechanics or surprise costs

## Implementation Order

1. **NPC One-Time Request Model** - Foundation for everything else
2. **JSON Parsing for Requests** - Load from data files
3. **Multiple Cards in Conversation** - Show all options
4. **Promise Queue Manipulation** - Connect systems
5. **UI Updates** - Display from NPC data
6. **Legacy Cleanup** - Remove old flow system
7. **Investigation Verification** - Fix JSON values

## Testing Checklist

### One-Time Request System
- [ ] Elena's letter request appears in conversation options
- [ ] All 3 request cards visible (5, 10, 15 rapport thresholds)
- [ ] Both promise cards visible (position 1 options)
- [ ] Completing any card marks request as done
- [ ] Request doesn't appear again after completion

### Promise Card Mechanics
- [ ] Promise card forces obligation to position 1
- [ ] Displaced NPCs have tokens burned
- [ ] Burned tokens grant instant rapport (5 per token)
- [ ] Queue manipulation shown in UI
- [ ] Token rewards granted if specified

### Perfect Information
- [ ] All cards show rapport thresholds
- [ ] Promise cards show queue effects
- [ ] Token costs visible before playing
- [ ] Rewards clearly displayed

## Success Criteria

The system is complete when:
1. NPCs define their own one-time requests in JSON
2. Players see all request and promise cards upfront
3. Promise cards manipulate queue with token burning
4. No legacy flow checks remain
5. Investigation scaling matches design (QUIET=2, BUSY=1)

This architecture creates the interconnected systems where every decision has cascading consequences, all visible to the player before they commit.