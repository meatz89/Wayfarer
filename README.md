# Wayfarer POC Development Guide

## The Vision: Why This Game Is Brilliant

Imagine waking up in Millbrook with 2 coins to your name and you're exhausted from yesterday's journey. You've got a contract to deliver tools to some merchant in Crossbridge by Sunday, and today's Thursday. Sounds simple, right?

But here's where it gets brilliant. The morning market is opening and you could work for 4 coins, which would solve your immediate problem - you need food and shelter tonight. But the merchant caravan to Crossbridge? It leaves at noon. If you work the morning shift, you miss it. And the next caravan isn't until tomorrow noon.

This is what makes the game incredible - **everything costs something else**.

## Core Design Philosophy

The genius is how it all connects:
- You can't just optimize money because you need stamina to work
- You can't just optimize stamina because you need money to travel efficiently  
- You can't just optimize time because sometimes the slow route is the only one you can afford
- You can't carry every profitable item because you only have 3 inventory slots

Every single choice ripples forward:
- Take the cheap transport? Arrive exhausted
- Work too much? Can't travel far
- Focus on contracts? Miss arbitrage opportunities
- Focus on trading? Fail contracts and lose reputation

## Core Systems Overview

### 1. Time Block System
- Day divided into: Dawn | Morning | Afternoon | Evening | Night
- Actions consume time blocks (work = 1 block, travel = 1-3 blocks)
- Once spent, they're gone - creates constant pressure

### 2. Stamina System
- Daily pool of 0-6 points based on rest quality
- Every action drains stamina (work: 2-3, travel: 2-3)
- When it hits 0, you can ONLY rest
- Creates daily action budget

### 3. Route/Transport System
- Multiple ways between locations (walk free vs. pay for speed)
- Each route has: coin cost, stamina cost, time cost
- Missing scheduled transport means waiting or taking worse option

### 4. Trading System
- Each location has different buy/sell prices
- Limited inventory (3 slots) forces choices
- Profit = sell price - buy price - transport cost
- Creates arbitrage opportunities

### 5. Contract System
- Timed obligations (deliver X to Y by day Z)
- Creates time pressure and route planning needs
- Failure has consequences (reputation loss)

## POC Implementation Plan

### What You Already Have
- ✓ Basic rest system
- ✓ Time advancement system
- ✓ Location/location spot actions
- ✓ Resource costs and checks
- ✓ Travel between locations
- ✓ Basic inventory (array of 3)

### What You Need to Add

## Phase 1: Multiple Routes (Highest Impact)

### Why This First
Adding multiple routes immediately makes every travel decision interesting. "Do I pay for speed or walk for free?" This single addition transforms the game from "click to travel" to "how do I optimize this?"

### Implementation

For each location pair, add 2+ transport options:

```
Millbrook → Crossbridge:
- Walk: {cost: 0, stamina: 2, timeBlocks: 2}
- Merchant Cart: {cost: 3, stamina: 0, timeBlocks: 1, departsAt: "Noon"}

Millbrook → Thornwood:
- Walk: {cost: 0, stamina: 3, timeBlocks: 2}
- Guide: {cost: 5, stamina: 1, timeBlocks: 1} [LOCKED - discovered through play]
```

### Player Experience
When player selects "Travel to Crossbridge", show:
```
How do you want to travel?

[Walk] - Free, 2 stamina, arrives Evening
[Merchant Cart] - 3 coins, departs at Noon, arrives Afternoon

Current resources: 4 coins, 3 stamina
Current time: Morning
```

### Key Decisions This Creates
- Save money but arrive exhausted?
- Pay for comfort but have less for trading?
- Wait for scheduled transport or leave now?

## Phase 2: Trading System

### Why This Second
Trading gives PURPOSE to routes. Once players can buy low and sell high, every transport decision becomes part of a larger economic puzzle.

### Implementation

Add price matrices to each location:

```
Millbrook:
- Herbs: Buy 2, Sell 1
- Tools: Buy 8, Sell 4
- Rare Book: Buy 15, Sell 8

Crossbridge:
- Herbs: Buy 4, Sell 2  
- Tools: Buy 6, Sell 3
- Rare Book: Buy 20, Sell 15

Thornwood:
- Herbs: Buy 1, Sell 4
- Tools: Buy 10, Sell 12
- Rare Book: Not available
```

### Market Interface
At each market location, show:
```
=== Millbrook Market ===
       Buy    Sell
Herbs   2c     1c    [Buy] [Sell]
Tools   8c     4c    [Buy] [Sell]
Book   15c     8c    [Buy]

Your coins: 6
Inventory: [Herbs] [Empty] [Empty]
```

### The Magic Moment
Player realizes: "Herbs cost 1 in Thornwood but sell for 4 in Millbrook! If I can get there and back, that's 3 coins profit per herb. But the journey costs stamina and time..."

## Phase 3: Discovery System

### Why This Third
Shows players the world expands through their actions, not just given to them. Creates moments of "I found something!"

### Implementation

Two types of discoveries:

**Route Discovery**
In Thornwood, add afternoon action:
```
[Work with Hunter] - 2 stamina
Result: "The hunter shows you a forest path to Crossbridge!"
Unlocks: Thornwood → Crossbridge (Forest Path: 1 timeBlock, 3 stamina, free)
```

**Location Discovery**
In Crossbridge, add conversation:
```
[Talk to Merchant] - 0 stamina
"She mentions Riverside desperately needs tools - they pay double!"
Unlocks: Riverside location on map
```

### Discovery Feedback
When player discovers something:
```
=== New Route Discovered! ===
Forest Path: Thornwood → Crossbridge
Fast but exhausting (1 time, 3 stamina)

Your map has been updated.
```

## Phase 4: Contract System

### Why This Last
Contracts tie everything together with time pressure. They transform sandbox trading into purposeful journeys.

### Implementation

Start with one simple contract:
```
Contract: {
  description: "Deliver tools to Crossbridge merchant",
  item: "tools",
  destination: "Crossbridge",
  dueDay: 5,
  currentDay: 1,
  payment: 15,
  penalty: "Cannot take merchant work anywhere"
}
```

### Contract Display
Always visible:
```
Active Contract: Deliver tools to Crossbridge
Time remaining: 4 days
Reward: 15 coins
[Complete Delivery] (only shows when in Crossbridge with tools)
```

### The Pressure
This creates cascading decisions:
- Tools cost 8 in Millbrook but 6 in Crossbridge
- But you need to GET to Crossbridge to buy them cheaper
- But you also need money to buy them at all
- But working for money takes time
- But the contract is ticking...

## Phase 5: Enhanced Inventory

### Current State
You have: Basic array[3]

### Enhancement
Make it visual and interactive:
```
Inventory: [Herbs] [Empty] [Tools]
           [Drop]         [Drop]
```

When buying with full inventory:
```
Inventory Full! 
Drop an item to make room:
[Drop Herbs] [Drop Tools] [Cancel]
```

### Why This Matters
Forces immediate decisions. Found a rare book worth 20 coins but carrying contract tools? Agonizing choice!

## The Complete POC Experience

### Starting State
- Day 1, Dawn, Millbrook
- 5 stamina, 2 coins
- Empty inventory
- Contract: Deliver tools by day 5

### The Opening Decision Tree
1. **Work morning for 4 coins?**
   - Pro: Can afford tool purchase sooner
   - Con: Miss morning travel options

2. **Travel immediately?**
   - Pro: More time at destination
   - Con: Arrive with no money

3. **Check market first?**
   - Discover herb prices
   - See tool cost (8 coins - you need 6 more!)

### The Emerging Strategy
Player discovers through play:
1. Herbs are cheap in Thornwood (1c) but sell for 4c in Crossbridge
2. Tools are cheaper in Crossbridge (6c vs 8c in Millbrook)
3. The forest path saves time but exhausts you
4. The merchant cart is reliable but costs money
5. Working too much leaves no stamina for travel

### The "Aha!" Moment
"If I take the cart to Thornwood (3c), buy 3 herbs (3c), walk back to save money, sell herbs in Crossbridge (12c income!), I can buy tools there cheap AND have money left over!"

But wait - that takes 3 days. Contract is due in 5. Is there time?

## Implementation Checklist

### Hour 1: Multiple Routes
- [ ] Add route selection interface when traveling
- [ ] Implement different costs per route (coins/stamina/time)
- [ ] Add "departs at" for scheduled transport
- [ ] Show "missed departure" when time has passed

### Hour 2: Trading System  
- [ ] Add price matrices to each location
- [ ] Create buy/sell interface at markets
- [ ] Implement inventory full checking
- [ ] Show profit potential (sell price - buy price)

### Hour 3: Discovery + Contracts
- [ ] Add Hunter action in Thornwood (discovers forest path)
- [ ] Add Merchant conversation in Crossbridge (discovers Riverside)
- [ ] Implement contract timer and display
- [ ] Add "Complete Contract" action when conditions met
- [ ] Implement contract failure penalty

### Hour 4: Polish
- [ ] Visual inventory slots with drop capability
- [ ] Clear resource cost preview before actions
- [ ] "Can't afford" messaging when lacking resources
- [ ] Discovery celebration screen
- [ ] Contract completion fanfare

## Success Metrics

You know your POC is working when players:
1. Agonize over whether to pay for transport
2. Get excited discovering price differences
3. Feel the pressure of contract deadlines
4. Have "aha!" moments about route optimization
5. Want to play "just one more day" to execute their plan

## Final Note

The beauty is in the simplicity. You're not adding complex mechanics - you're adding simple systems that interconnect in complex ways. Every coin matters, every time block counts, every stamina point is precious, and every decision cascades into the next.

That's what makes it brilliant. That's what keeps players coming back. That's Wayfarer.