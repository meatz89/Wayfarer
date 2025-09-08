# POC Content Implementation Plan

## Overview

The POC demonstrates all three core game loops through a single critical path: accepting Elena's urgent marriage refusal letter and delivering it to Lord Blackwood before he leaves at 5:00 PM. The challenge lies in discovering the precise action order that makes success possible.

## Core Design

**The Puzzle**: Players naturally try to help desperate Elena or earn coins for the Noble Quarter checkpoint. Both approaches fail. The solution requires building Market Square familiarity through investigation, discovering knowledge that helps specific NPCs, and managing resources with perfect precision.

**The Discovery**: Every seemingly inefficient action (investigating twice, buying food before working, building infrastructure before the main quest) is actually essential. The optimal path emerges through understanding system interactions.

## Starting Conditions

### Player State
- **Attention**: 10 (no reduction since hunger is 50)
- **Coins**: 0
- **Hunger**: 50
- **Health**: 100
- **Tokens**: 0 with all NPCs
- **Familiarity**: 0 at all locations

### Obligation Queue
- Position 1: Viktor's package to Marcus (3-hour deadline, pays 7 coins)

### Time
- Start: 9:00 AM Tuesday
- Deadline: Lord Blackwood leaves at 5:00 PM (8 hours available)

## NPCs

### Elena (Copper Kettle Tavern, Corner Table)
- **Personality**: Devoted (15 base patience)
- **Starting State**: Disconnected (3 focus, 1 card draw)
- **Availability**: Always present (lives upstairs, helps uncle)
- **Persistent Decks**:
  - Conversation: 20 standard cards
  - Request: Contains "Elena's Urgent Letter" card
  - Observation: Receives "Safe Passage Knowledge" from Market Square
  - Burden: Empty
  - Exchange: None (not mercantile)
- **Special Mechanic**: Playing "Safe Passage Knowledge" immediately advances her to Neutral state

### Marcus (Market Square, Merchant Row)
- **Personality**: Mercantile (12 base patience)
- **Starting State**: Neutral
- **Availability**: Always during market hours
- **Cannot Leave**: Merchandise would be stolen
- **Persistent Decks**:
  - Conversation: 20 standard cards
  - Request: Contains letter to Warehouse District
  - Observation: Receives "Merchant Caravan Route" from Market Square
  - Burden: Empty
  - Exchange: Food purchase, caravan trip
- **Special Mechanic**: Caravan exchange requires 2+ Commerce tokens AND route card played

### Lord Blackwood (Noble Quarter, Blackwood Manor)
- **Personality**: Proud (10 base patience)
- **Starting State**: Neutral
- **Availability**: Until 5:00 PM sharp
- **Quick Delivery**: Elena's noble seal ensures formal respect (1 attention conversation)

### Warehouse Recipient (Warehouse District)
- **Function**: Receives Marcus's letter
- **No Conversation**: Simple delivery notification

## Locations and Spots

### Market Square
**Spots**:
- **Fountain**: Morning=Quiet, Afternoon=Busy, Evening=Closing
- **Merchant Row**: Always Commercial (Marcus location)
- **Guard Post**: Always Authority (checkpoint to Noble Quarter)

**Investigation Scaling**:
- Morning at Fountain (Quiet): 1 attention → +2 familiarity
- Afternoon at Fountain (Busy): 1 attention → +1 familiarity

**Observations**:
- First (Familiarity 1+): "Safe Passage Knowledge" → Elena's observation deck
- Second (Familiarity 2+ AND first done): "Merchant Caravan Route" → Marcus's observation deck

### Copper Kettle Tavern
**Spots**:
- **Common Room**: Public, travel hub
- **Corner Table**: Private (Elena location, +1 patience)
- **Bar**: Commercial

### Noble Quarter
**Spots**:
- **Blackwood Manor**: Noble (Lord Blackwood location)
- **Gate**: Guarded (checkpoint entrance)

### Warehouse District
**Spots**:
- **Warehouse Entrance**: Commercial (delivery point)

## Routes

### Market Square ↔ Copper Kettle Tavern
- **Time**: 15 minutes
- **Cost**: Free
- **Requirements**: None

### Market Square ↔ Warehouse District
- **Time**: 20 minutes
- **Cost**: Free
- **Requirements**: None

### Market Square → Noble Quarter (Checkpoint)
- **Time**: 25 minutes
- **Cost**: Free (but needs permit)
- **Requirements**: Noble District Permit (costs 20 coins - impossible)

### Market Square → Noble Quarter (Merchant Caravan)
- **Time**: 20 minutes
- **Cost**: 10 coins
- **Requirements**: Unlocked via "Merchant Caravan Route" card AND 2+ Commerce tokens

## Cards

### Observation Cards

**Safe Passage Knowledge**
- **Source**: Market Square first observation
- **Destination**: Elena's observation deck
- **Effect**: Immediately advances Elena to Neutral state
- **Focus Cost**: 0 (special SPEAK action)
- **Consumed**: Yes

**Merchant Caravan Route**
- **Source**: Market Square second observation
- **Destination**: Marcus's observation deck
- **Effect**: Unlocks "Join merchant caravan" exchange
- **Focus Cost**: 0 (special SPEAK action)
- **Consumed**: Yes

### Request Cards

**Elena's Urgent Letter**
- **Type**: Letter request in Elena's request deck
- **Focus**: 5 (requires Neutral state or Prepared atmosphere)
- **Difficulty**: Very Hard (40% base)
- **Success**: Creates delivery obligation to Lord Blackwood
- **Failure**: Adds burden card to Elena
- **Fixed Terms**: Position next available, no payment

**Marcus's Letter**
- **Type**: Letter request in Marcus's request deck
- **Focus**: 4
- **Difficulty**: Hard (50% base)
- **Success**: Creates delivery to Warehouse District
- **Token Reward**: 2 Commerce tokens with Marcus

### Exchange Cards

**Buy Food** (Marcus)
- **Cost**: 2 coins
- **Effect**: Reset hunger to 0
- **Requirements**: None

**Join Merchant Caravan** (Marcus)
- **Cost**: 10 coins
- **Effect**: One-time transport to Noble Quarter
- **Requirements**: 2+ Commerce tokens with Marcus AND "Merchant Caravan Route" played
- **Type**: Quick exchange (1 attention)

**Buy Noble District Permit** (Guard Captain)
- **Cost**: 20 coins
- **Effect**: Permanent checkpoint access
- **Purpose**: Deliberate dead end (impossible to afford)

## Work Mechanics

### Market Square Work
- **Base Output**: 5 coins
- **Scaling**: 5 - floor(hunger/25)
- **At Hunger 0**: 5 coins
- **At Hunger 50**: 3 coins
- **Time Cost**: 4 hours
- **Attention Cost**: 2

## The Only Successful Path

### Morning (9:00 AM - 2:20 PM)

1. **Investigate Market Square at Fountain** (1 attention)
   - Morning Quiet spot: +2 familiarity (0→2)
   - Time: 9:10 AM

2. **Observe Market Square** (0 attention)
   - Familiarity 1+ unlocked: Gain "Safe Passage Knowledge" for Elena
   - Time: 9:10 AM

3. **Converse with Marcus** (2 attention)
   - Deliver Viktor's package: +7 coins
   - Accept Marcus's letter to Warehouse
   - Time: 9:30 AM

4. **Travel to Warehouse District** (20 minutes)
   - Time: 9:50 AM

5. **Deliver Marcus's letter** (0 attention)
   - Gain 2 Commerce tokens with Marcus
   - Time: 9:50 AM

6. **Return to Market Square** (20 minutes)
   - Time: 10:10 AM

7. **Buy food from Marcus** (0 attention)
   - Spend 2 coins (5 remaining)
   - Hunger: 50→0

8. **Work at Market Square** (2 attention)
   - At hunger 0: +5 coins (total: 10)
   - Time: 2:10 PM

### Afternoon (2:10 PM - 4:00 PM)

9. **Investigate Market Square at Fountain** (1 attention)
   - Afternoon Busy spot: +1 familiarity (2→3)
   - Time: 2:20 PM

10. **Observe Market Square** (0 attention)
    - Familiarity 2+ AND first observation done: Gain "Merchant Caravan Route" for Marcus
    - Time: 2:20 PM

11. **Quick exchange with Marcus** (1 attention)
    - Play "Merchant Caravan Route" card (0 focus)
    - Unlocks caravan (have 2 Commerce tokens)
    - Exchange 10 coins for caravan trip
    - Time: 2:30 PM

12. **Travel to Copper Kettle** (15 minutes)
    - Time: 2:45 PM

13. **Converse with Elena** (2 attention)
    - Play "Safe Passage Knowledge" (advances to Neutral)
    - Accept letter at 5 focus capacity
    - Time: 3:10 PM

14. **Return to Market Square** (15 minutes)
    - Time: 3:25 PM

15. **Take caravan to Noble Quarter** (20 minutes)
    - Time: 3:45 PM

16. **Quick delivery to Lord Blackwood** (1 attention)
    - Elena's noble seal ensures success
    - Time: 4:00 PM

**Success**: 1 hour to spare, 0 coins, 0 attention remaining

## Why Every Other Path Fails

### Rush to Elena First
Without "Safe Passage Knowledge", her Disconnected state (3 focus, 1 draw) makes reaching the 5-focus request mathematically improbable.

### Investigate Only Once
First investigation gives +2 familiarity, allowing first observation. But second observation requires familiarity 2+, impossible without second investigation.

### Skip Food Purchase
Working at hunger 50 yields only 3 coins. Total becomes 8 coins (3 work + 5 after Viktor), cannot afford 10-coin caravan.

### Skip Marcus's Letter
No Commerce tokens with Marcus means caravan exchange stays locked even with route knowledge.

### Try Checkpoint
Guard's permit costs 20 coins. Maximum possible is 12 (7 Viktor + 5 work), making this a deliberate dead end.

### Wrong Investigation Timing
Morning investigation gives +2 familiarity (efficient). Afternoon gives only +1 (inefficient). Wrong timing requires extra investigation actions.

## Resource Mathematics

### Attention Budget
- Start: 10
- Investigate (morning): -1
- Marcus conversation: -2
- Work: -2
- Investigate (afternoon): -1
- Marcus exchange: -1
- Elena conversation: -2
- Lord Blackwood: -1
- **Total**: 10 exactly

### Coin Flow
- Start: 0
- Viktor delivery: +7
- Food purchase: -2
- Work (fed): +5
- Caravan: -10
- **Final**: 0 exactly

### Token Progression
- Start: 0 Commerce
- Marcus letter delivery: +2 Commerce
- Enables caravan when combined with route card

### Time Usage
- Available: 8 hours (9 AM to 5 PM)
- Used: 7 hours
- Buffer: 1 hour

### Familiarity Progression
- Market Square: 0→2 (morning) →3 (afternoon)
- Enables two observations at correct thresholds

## Key Design Achievements

### Perfect Resource Tension
Every resource is exactly sufficient with optimal play. One mistake in ordering or resource management causes failure.

### Discovery Through Failure
Players naturally try obvious approaches (help Elena, earn coins) and fail, learning the systems through experimentation.

### Istanbul-Style Elegance
Investigation efficiency depends on timing (morning Quiet vs afternoon Busy), creating meaningful decisions about when to act.

### No Red Herrings
Every element serves the solution. The checkpoint exists but costs too much. Every NPC and location matters.

### Narrative Coherence
Every mechanic makes story sense: Elena calms when learning escape routes, Marcus trusts established partners, workers perform better when fed.

## Testing Checklist

1. **Elena inaccessible without preparation**: Disconnected state too difficult
2. **Checkpoint truly impossible**: 20 coins unattainable
3. **Food purchase necessary**: Work output insufficient without
4. **Both observations required**: First for Elena, second for Marcus
5. **Investigation timing matters**: Morning more efficient than afternoon
6. **Token gate works**: Caravan locked without Commerce tokens
7. **Perfect resource usage**: 0 coins, 0 attention at end
8. **Time pressure real but fair**: 1 hour buffer with optimal play

## Failure States

- **Elena conversation fails**: No Safe Passage Knowledge
- **Cannot afford caravan**: Skipped food or wrong work timing
- **Caravan locked**: Missing Commerce tokens or route card
- **Miss deadline**: Inefficient routing or too many attempts
- **Queue blocked**: Viktor's package not delivered first
- **Insufficient attention**: Wrong conversation types chosen

## Extensions for Future Content

This POC structure supports:
- Additional observation levels at higher familiarity
- More NPCs providing alternate routes
- Token requirements for different exchanges
- Time-of-day dependent investigations
- Burden card resolution mechanics
- Multiple simultaneous obligations

The tight resource constraints and precise ordering demonstrate mastery of all three core loops while maintaining narrative coherence and mechanical elegance.