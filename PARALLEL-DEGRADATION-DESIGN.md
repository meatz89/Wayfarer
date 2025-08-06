# Parallel Degradation Design Document
## The Simplified Wayfarer: Time-Based Transparent Mechanics

### Executive Summary

After extensive design debate and analysis, we're pivoting from complex hidden cascades to a **transparent parallel degradation system**. Everything happens on visible timelines. Players manage time, not abstract resources. All mechanics are diegetic and clear.

## Core Design Principles

### 1. Time is the Only Resource
- No attention points
- No focus management
- No token currencies
- Just **time** measured in minutes/hours

### 2. Everything is Visible
- All countdowns shown
- All consequences clear
- All timelines transparent
- No hidden mechanics

### 3. Parallel, Not Sequential
- Multiple timelines run simultaneously
- Your actions consume time, not trigger events
- The world continues whether you watch or not
- Opportunity cost, not combo chains

## The Timeline System

### How It Works

```
Current Time: 2:00 PM
Available Actions:

Elena's Letter     [⏰ 3 hours remaining]
├─ Quick Placate   (15 min) → Buys 1 hour
├─ Negotiate       (30 min) → Buys 2 hours  
├─ Full Resolve    (60 min) → Completes
└─ Ignore          (0 min)  → Continues countdown

Guard Shift        [⏰ 1 hour remaining]
├─ Observe Now     (15 min) → Learn rotation
├─ Wait for Change (60 min) → Better opportunity
└─ Ignore          (0 min)  → Miss information

Marcus Departure   [⏰ 2 hours remaining]
├─ Trade Info      (30 min) → Get route data
├─ Buy Goods       (30 min) → Commerce opportunity
└─ Ignore          (0 min)  → Marcus leaves

Market Closing     [⏰ 4 hours remaining]
├─ Shop Now        (30 min) → Full selection
├─ Rush Later      (15 min) → Limited selection
└─ Skip            (0 min)  → Market closes

Energy Depletion   [⏰ 6 hours remaining]
├─ Rest Now        (90 min) → Full recovery
├─ Quick Nap       (30 min) → Partial recovery
└─ Push Through    (0 min)  → Exhaustion penalty
```

### Timeline Types

1. **Letter Deadlines** - Delivery pressure
2. **NPC Availability** - When people leave/arrive
3. **Location Changes** - Shops closing, shifts changing
4. **Resource Depletion** - Energy, supplies
5. **Environmental** - Weather, daylight

### Action Time Costs

```
QUICK (15 minutes):
- Scan letter contents
- Brief observation
- Simple transaction
- Quick question

STANDARD (30 minutes):
- Normal conversation
- Shopping
- Letter negotiation
- Information gathering

EXTENDED (60 minutes):
- Deep interaction
- Complex negotiation
- Thorough investigation
- Major decision

LONG (90 minutes):
- Full rest
- Travel between districts
- Major quest resolution
- Rally point actions
```

## Rally Points (Synchronization Mechanics)

### The Problem They Solve
Pure degradation creates only anxiety. Rally points create rhythm - moments where multiple timelines pause or reset.

### Examples

**"Host Dinner at Inn" (90 minutes)**
- Resets all social deadlines by 6 hours
- Costs 20 silver
- Can only do once per day
- Creates strategic breathing room

**"Visit Postmaster" (60 minutes)**
- Extends all letter deadlines by 2 hours
- Costs reputation
- Limited uses per week
- Emergency pressure valve

**"Market Gossip Session" (30 minutes)**
- Reveals all NPC locations for next 3 hours
- Costs nothing first time, escalates
- Information multiplier

## Mechanical Transparency

### What Players See

```
[ELENA'S SITUATION]
Letter: Urgent (3 hours remaining)
Stakes: Her reputation with Lord Aldwin
Options:
- Prioritize her letter → Lord B angry (-Status)
- Let it expire → Elena devastated (-Trust)
- Negotiate middle ground → Both disappointed

[CLEAR COUNTDOWN]
⏰ 3:00:00 → 2:59:59 → 2:59:58...
```

### What Players DON'T See
- Hidden verb names (PLACATE, EXTRACT)
- Numerical relationship values
- Abstract token counts
- Invisible cascade triggers
- AI-generated variations

## Production Requirements

### Content Needed (Minimal)

**Per NPC (5 total):**
- 30 timeline events @ 10 min each = 5 hours
- 20 consequence descriptions @ 5 min = 1.7 hours
- 10 rally point interactions @ 10 min = 1.7 hours
- **Total: 8.4 hours × 5 = 42 hours**

**System Content:**
- 20 time cost descriptions = 1 hour
- 10 location state changes = 2 hours
- 5 rally point descriptions = 1 hour
- UI text and tooltips = 2 hours
- **Total: 6 hours**

**TOTAL CONTENT: 48 hours**

### Development Time

**Core Systems:**
- Timeline manager: 20 hours
- Countdown UI: 15 hours
- Action system: 20 hours
- Save/load: 10 hours
- **Subtotal: 65 hours**

**Content Integration:**
- NPC implementation: 25 hours
- Location states: 15 hours
- Rally points: 20 hours
- **Subtotal: 60 hours**

**Polish & Testing:**
- Balance: 20 hours
- Bug fixes: 15 hours
- UI polish: 10 hours
- **Subtotal: 45 hours**

**TOTAL DEVELOPMENT: 170 hours**

**GRAND TOTAL: 218 hours**

## Why This Works

### 1. Radical Simplicity
- One resource (time)
- One mechanic (countdown)
- Clear consequences
- No hidden systems

### 2. Production Feasible
- 218 hours vs 600+ for complex systems
- No AI dependency
- Single writer can deliver
- Testable and debuggable

### 3. Player Clarity
- See all pressures
- Understand all trade-offs
- Plan strategies
- Learn through play

### 4. Emergent Complexity
- 5 parallel timelines create depth
- Rally points add strategy
- Time management is universally understood
- Mastery through repetition

## Example Play Session

```
9:00 AM - Start Day
- Elena letter: 8 hours
- Guard shift: 2 hours
- Market close: 9 hours
- Energy: 10 hours
- Weather change: 4 hours

9:15 AM - Quick scan Elena's letter (15 min)
- Learn it's reputation stakes
- See negotiation options

9:45 AM - Negotiate with Elena (30 min)
- Buy 2 hours on her deadline
- Learn about Marcus

10:45 AM - Wait for guard shift (60 min)
- Strategic patience
- Better access after shift

11:00 AM - Talk to Marcus (30 min)
- Trade route information
- He leaves at 11:30

12:30 PM - Host lunch rally (90 min)
- Reset social timers
- Elena now has 8 hours again
- Expensive but worth it

1:00 PM - Focused letter delivery (60 min)
- Complete Elena's letter
- Reputation saved

2:00 PM - Market shopping (30 min)
- Still good selection
- Prepare for tomorrow

2:30 PM - Rest (90 min)
- Reset energy
- Ready for evening

4:00 PM - Evening activities begin...
```

## Implementation Priority

### Week 1: Core Loop
1. Timeline data structure
2. Countdown displays
3. Basic action system
4. Time advancement

### Week 2: Content
1. 5 NPC timelines
2. Consequence implementation
3. Location states
4. Rally points

### Week 3: Polish
1. UI refinement
2. Balance testing
3. Save system
4. Tutorial

## Success Metrics

- Players understand system in 5 minutes
- No confusion about consequences
- Strategic depth emerges from simple rules
- 3-hour demo showcases full system
- Can add NPCs without exponential complexity

## Conclusion

This isn't a narrative game with mechanical elements. It's a **time-management puzzle with narrative theming**. By embracing transparency and simplicity, we can deliver a complete, polished experience in 218 hours that would take 1000+ hours with hidden complexity and procedural generation.

The game becomes: **"Can you juggle 5 critical deadlines while the clock never stops?"**

Simple. Clear. Achievable. Engaging.