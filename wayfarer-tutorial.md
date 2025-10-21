# Wayfarer Tutorial: Evening Arrival at Millhaven

## Design Philosophy

The player arrives worn and weary at evening, needing shelter before nightfall. One decision, immediate stakes, narrative resonance. The tutorial is a single impossible choice that teaches resource scarcity, work safety net, and the core economic loop. 10 minutes. Then Day 2 begins with the real game.

**Kingkiller Chronicles vibe:** Exhausted traveler stumbling into an inn at dusk, needing rest, discovering opportunity.

---

## Opening: Evening Segment 1 (Evening - Dusk)

**You've been walking since dawn. Your legs ache, your coin purse feels light, and the sun is setting. The warm glow of The Brass Bell Inn beckons from across the square.**

**Current State:**
- Time: Evening Segment 1 of 4 (13th segment of full day - 3 more evening segments until midnight)
- Location: Millhaven - Town Square
- Health: 4/6 (road-worn, minor injuries from travel)
- Focus: 6/6 (rested mind, tired body)
- Stamina: 3/6 (exhausted from day's travel)
- Coins: 8

**Screen Display:**
```
THE BRASS BELL INN - EVENING

Lamplight spills from windows. Inside: warmth, food, safety. The common 
room hums with evening conversations. Elena, the innkeeper, wipes down 
tables while watching travelers settle in for the night.

You have 8 coins. Night approaches.

IMMEDIATE NEED:
► Secure a Room - 10 coins
  "A bed, a roof, safety through the night. Recovery: All resources to maximum."
  
YOU'RE SHORT: 2 coins

ELENA (INNKEEPER):
"Need a room? 10 coins. Or..." *she glances at the common room* "...I could 
use help with the evening crowd. Busy night. One hour of work, I'll cover 
the difference and feed you."

► "Help with evening service" (Social work goal, 1 segment)
  - Estimated: 5 coins, simple conversation interactions
  - Time: 1 segment (leaves 2 segments until midnight)
  - Reward: Enough for room + meal

THOMAS (WAREHOUSE FOREMAN) - at corner table:
"Heard you might need work. Got crates that need moving tonight before 
the morning shipment. Heavy lifting, but pays."

► "Warehouse Loading" (Physical work goal, 1 segment)  
  - Estimated: 5 coins, physical labor
  - Costs: 1 Stamina (you have 3/6, manageable)
  - Time: 1 segment
  - Reward: Enough for room

OTHER VISIBLE:
► Sleep Outside (No cost, no segment)
  - Effect: -2 Health (4/6 → 2/6), unsafe, no recovery
  - Free, but risky and exhausting
  
[Other NPCs visible but not offering immediate work]
[Other locations visible but evening darkness falling]
```

**The Decision:**

Player must choose immediately. No exploring. No shopping. Just:
1. **Work for Elena** (Social - use Focus, preserve Stamina)
2. **Work for Thomas** (Physical - use Stamina, preserve Focus)  
3. **Sleep rough** (Save resources, take damage and risk)

**Design Note:** 
- Immediate tension (need 2 more coins, night approaching)
- Clear options with clear trade-offs
- Work safety net exists (can always earn basics)
- Different resource costs (Social vs Physical)
- Time pressure (3 segments until midnight)
- Narrative weight (exhausted traveler needing shelter)

---

## Player Choice: Elena's Service (Social Path)

**Player chooses:** Help Elena with evening service

**Reasoning:** "I have 6/6 Focus but only 3/6 Stamina. Social work uses Focus. Save Stamina for tomorrow."

---

## Social Challenge: "Evening Service"

```
SOCIAL CHALLENGE: Help with Evening Crowd
Momentum: 0/6 (threshold)
Doubt: 0/10 (failure)
Initiative: 0
Cadence: 0
Baseline Doubt: 2 (simple work, low pressure)

[SPEAK] [LISTEN]
```

**Challenge proceeds (abbreviated):**
- Foundation cards generate Initiative (+1 each)
- Depth 1-2 cards build Momentum (+1-2 each)
- Reach threshold 6 in ~4 card plays
- Cadence and Doubt managed through LISTEN timing

```
GOAL CARD PLAYED: "Competent Service" (Threshold 6)

"You move through the common room with practiced ease. Drinks delivered, 
conversations managed, chaos ordered. Elena notices."

Elena: "You've done this before. Good work. Room's yours, and here—" 
*slides a plate of bread and stew across the bar* "—on the house."

CHALLENGE COMPLETE:
- Focus: 6 → 5 (-1, light social exertion)
- Time: Evening Segment 1 → 2 (2 more evening segments until midnight)

REWARDS:
- Coins: 8 → 13 (+5)
- Elena StoryCubes: 0 → 1 (first positive interaction)
- Meal consumed: Health 4 → 6 (restored from food)

Stat XP: Rapport +2, Diplomacy +1
```

**Design Note:** 
- Single Social challenge teaches card system basics
- Small Focus cost (1 of 6)
- StoryCubes introduced (relationship progression)
- Work safety net demonstrated (can always earn basics)
- Immediate payoff (shelter + healing secured)

---

## Evening Segment 2: Securing the Room

**Player returns to Elena:**

```
SECURE A ROOM: 10 coins
Coins: 13 → 3 (-10)

Elena: "Upstairs, second door. I'll have someone bring up water for washing."

[Advance to rest]
```

---

## Night Recovery

```
═══════════════════════════════════════════════════
END OF DAY - REST AT INN
═══════════════════════════════════════════════════

Night passes. You sleep deeply in a real bed for the first time in days.
Morning light through the window. Town sounds rising. A new day begins.

RECOVERY:
- Health: 6/6 (already full from meal)
- Focus: 5 → 6 (mental rest)
- Stamina: 3 → 6 (physical rest)

RESOURCES AT DAWN (DAY 2):
- Health: 6/6
- Focus: 6/6  
- Stamina: 6/6
- Coins: 3

PROGRESSION:
- Elena StoryCubes: 1 (innkeeper remembers you)
- Town Square StoryCubes: 1 (familiar with common room)
- Rapport: 2 XP toward Level 2
- Diplomacy: 1 XP toward Level 2

═══════════════════════════════════════════════════
```

---

## Day 2, Segment 1 (Morning): The Game Begins

**You wake refreshed. The exhaustion of travel has passed. Millhaven awaits.**

```
THE BRASS BELL INN - COMMON ROOM (Morning)

Elena: "Sleep well? There's talk you might have skills worth putting to use. 
Thomas mentioned something about work at the warehouse if you need coins. 
And I heard the Miller's been having trouble with his waterwheel—might be 
worth investigating if you've got the curiosity."

CURRENT STATE:
Time: Morning Segment 1 of 4 (1st segment of full day - full day ahead)
Coins: 3 (low, but recovered)
All Resources: 6/6/6 (ready for anything)

THE WORLD OPENS:

OBLIGATIONS AVAILABLE:
► Elena: "Deliver a letter to the Mill" (NPC Commissioned)
  - Deadline: Segment 8 (Midday)
  - Reward: 25 coins
  - Requires: Travel to Mill (2 segments, obstacles present)
  
► Thomas: "Warehouse inventory count" (Work goal)
  - No deadline
  - Reward: 5 coins
  - Simple, repeatable income

► Self-Discovered: "Silent Mill Mystery" 
  - Investigate waterwheel problem (no deadline, unknown rewards)
  - Explore at your own pace

LOCATIONS AVAILABLE:
► Town Square (current)
► Equipment Shop (purchase gear with context tags)
► Warehouse District
► Mill Road (route with obstacles: [Water] creek crossing)

WHAT YOU'VE LEARNED:
✓ Resource scarcity (needed 2 more coins)
✓ Work safety net (can earn basics through labor)
✓ Social challenges (card-based tactical gameplay)
✓ Resource pools (6-point, every point matters)
✓ Recovery (rest restores all resources)
✓ StoryCubes (relationship/location familiarity)

WHAT YOU DISCOVER NOW:
? Obligations (deadlines vs exploration)
? Equipment contexts (specific tags for specific obstacles)
? Route travel (multi-segment with obstacles)
? Three challenge types (Social/Mental/Physical)
? Economic loop (coins → equipment → access)
? Context cubes (reduce challenge difficulty)

The tutorial is complete. You understand resource scarcity and work fundamentals.
Everything else? You'll discover through play.
```

---

## Tutorial Design Success

**Time to complete:** ~10 minutes
- Arrive at inn (1 min)
- Examine options (1 min)
- Social challenge (5 min)
- Secure room and sleep (1 min)
- Wake to full world (2 min)

**What the player experienced:**
- Immediate tension (need shelter, not enough coins)
- Clear choice (Social vs Physical vs Sleep rough)
- Meaningful consequence (work vs damage)
- Mechanical depth (Social challenge satisfying)
- Economic loop (work → coins → needs met)
- Recovery system (rest = full resources)
- Narrative weight (exhausted traveler finding refuge)

**What the player learned:**
- 6-point resource pools
- Resource competition (Focus vs Stamina)
- Work safety net exists
- Social challenge basics
- StoryCubes progression
- Time segments and blocks
- Economic reasoning

**What the player wants to do next:**
- Accept Elena's delivery obligation (25 coins!)
- Buy equipment for route obstacles
- Investigate Mill mystery
- Explore town further
- Meet more NPCs
- See Physical and Mental challenges

**The Kingkiller Chronicles vibe:**
Kvothe arriving at the Waystone Inn worn and weary, Kote offering shelter and work, the warmth of lamplight and the promise of story. Our player arrives at The Brass Bell, Elena offers shelter and work, the warmth of recovered strength and the promise of adventure.

Small opening. Immediate stakes. Narrative resonance. Then the world opens.

---

## Alternative: Physical Work Path

**If player chose Thomas's warehouse work:**

```
PHYSICAL CHALLENGE: "Warehouse Loading"
Breakthrough: 0/6
Exertion: 2 (from Stamina 3, rounded up)
Danger: 0/10
Aggression: 0

[Challenge proceeds with ASSESS/EXECUTE mechanics]
[Different cards, different rhythm than Social]

COMPLETE:
- Stamina: 3 → 2 (-1)
- Coins: 8 → 13 (+5)
- Thomas StoryCubes: 0 → 1
- Authority +2 XP, Cunning +1 XP

[Same room secured, same rest, same Day 2 opening]
```

**Both paths valid.** Social preserves Stamina. Physical preserves Focus. Player chose based on current resources. That's the core design: resource competition creates meaningful choice.

---

## Alternative: Sleep Rough Path

**If player chose to save resources:**

```
SLEEP OUTSIDE: Town Square bench

Cold. Uncomfortable. Street noise. Fitful sleep.

EFFECTS:
- No coins spent (still have 8)
- Health: 4 → 2 (-2 from exposure)
- Stamina: 3 → 3 (no recovery)
- Focus: 6 → 6 (no recovery)

DAY 2 STARTS:
- Health: 2/6 (dangerous, need healing)
- Stamina: 3/6 (still tired)
- Focus: 6/6 (rested mind)
- Coins: 8 (more money but worse condition)

Trade-off: Saved coins but damaged and vulnerable. 
Need healing (Rations 5 coins, or risk challenges at 2 Health).
Valid strategy, but risky.
```

**Three paths, all functional, all teach core loop through different trade-offs.**

The tutorial isn't teaching systems. It's teaching **decision-making under constraint.**