# Design Section 2: Core Gameplay Loops

## Purpose

This document defines the three-tier gameplay loop hierarchy that structures player experience in Wayfarer. It explains the SHORT (10-30s), MEDIUM (5-15min), and LONG (hours) loops, how they interconnect, what makes them engaging, and how they create strategic depth through resource management and impossible choices.

---

## 2.1 Three-Tier Loop Hierarchy

Wayfarer's gameplay operates on three distinct temporal scales, each serving different purposes and creating different types of engagement.

### Loop Scale Overview

**SHORT LOOP (10-30 seconds):**
- Core interaction unit
- Single encounter with choice
- Immediate resource consequence
- Builds tactical rhythm

**MEDIUM LOOP (5-15 minutes):**
- Complete delivery cycle
- Chain of encounters
- Economic transaction
- Tests resource management

**LONG LOOP (hours):**
- Character development
- Relationship building
- Strategic investment
- Build identity emergence

### Why Three Tiers?

**Immediate feedback:** SHORT loop provides constant confirmation that actions matter. Resources change visibly. Progress advances measurably.

**Session structure:** MEDIUM loop creates natural play sessions. Start delivery, complete delivery, natural stopping point. Progress feels complete even in short playtimes.

**Long-term motivation:** LONG loop provides meta-progression goals. Character builds emerge over time. Relationships develop gradually. World understanding deepens.

**Engagement variety:** Players experience different challenge types across scales. Tactical optimization in SHORT loop. Resource management in MEDIUM loop. Strategic planning in LONG loop.

---

## 2.2 SHORT LOOP: Encounter-Choice-Consequence (10-30 seconds)

### Loop Structure

```
Encounter Presents
    ↓
Player Evaluates Options (2-4 choices with visible costs)
    ↓
Player Selects Choice
    ↓
Resources Change Immediately
    ↓
Progress Advances (next encounter OR destination reached)
```

### Example: Muddy Road Encounter

**Context:** Traveling route segment during delivery

**Encounter Description:**
"The path ahead has become a muddy quagmire from recent rains. Deep ruts from cart wheels make footing treacherous."

**Choice 1: "Push through with determination"**
- Costs: 3 energy, 1 time segment
- Rewards: Progress advances, guaranteed success
- Analysis: High energy cost, reliable

**Choice 2: "Find detour around mud"**
- Costs: 1 energy, 2 time segments
- Rewards: Progress advances, guaranteed success
- Analysis: Saves energy, costs time

**Choice 3: "Hire local guide"**
- Costs: 5 coins, 1 time segment
- Rewards: Progress advances, guaranteed success
- Analysis: Monetary cost, fast and low-energy

**Choice 4: "Athletic leap across planks"**
- Requirements: Physical stat threshold for availability
- Spawns: Physical Challenge (tactical layer)
- Success: 1 energy, 1 time segment, extra coins
- Failure: 5 energy, 1 time segment, coin penalty (damaged goods)
- Analysis: Gamble for efficiency, stat-gated

### No Correct Answer

The player with high energy takes choice 1. The player with time pressure takes choice 3. The player with high Physical stat gambles on choice 4. The player who has traveled this route before knows to conserve energy for harder segments ahead and takes choice 2.

**This is the core skill: resource optimization through informed choice.**

### What Makes SHORT Loop Engaging

**Visible Trade-offs:**
Every choice shows exact costs before selection. Multiple valid approaches exist (spend energy OR coins OR time). No "correct" path, only different costs.

**Resource Consequence:**
Energy and hunger deplete during travel. Must balance "advance quickly" vs "conserve resources". Running out forces failure or costly detours.

**Perfect Information:**
All costs visible. Can calculate affordability. Decision is strategic (which cost to accept), not random (guessing consequences).

**Immediate Feedback:**
Select choice → Resources update immediately → See consequence. Fast cycle builds rhythm. Confirms choices matter.

### Challenge Integration

Some choices spawn tactical challenges (Physical, Social, Mental). Challenges are separate gameplay mode with their own mechanics.

**Success:** Better resource efficiency, extra rewards
**Failure:** Worse resource costs, penalties

Challenges are optional gambles. Always alternative guaranteed choices exist. Players can avoid all challenges and complete deliveries through pure resource management.

---

## 2.3 MEDIUM LOOP: Delivery Cycle (5-15 minutes)

### Loop Structure

```
Wake at Location
    ↓
Review Current State (resources, time, available jobs)
    ↓
Accept Delivery Job (choose destination)
    ↓
Travel Route Segments (chain of SHORT loops)
    ↓
Reach Destination → Earn Coins
    ↓
Optional: Explore Destination
    ↓
Return Journey (same route, reverse direction)
    ↓
Arrive Back → Evening Phase
    ↓
Mandatory Survival Spending (food, lodging)
    ↓
Sleep → Advance Day
```

### Morning Phase: Choose Primary Activity

**Wake at current location** with full time segments available.

**Review current state:**
- Available time segments for the day
- Current resources (coins, energy, hunger)
- Current location and nearby options
- Available delivery jobs

**Choose primary activity:**

**Option 1: Accept Delivery (Primary Income)**
- View available jobs with visible rewards and distance
- Select delivery destination
- Consumes majority of day's time segments
- Earns coins upon completion
- Core gameplay activity

**Option 2: NPC Interaction (Investment)**
- Spend time with specific NPC
- Costs time segments
- Advances bond level toward mechanical benefits
- Delayed gratification for future advantage

**Option 3: Local Exploration (Discovery)**
- Investigate current location
- Costs time and resources
- Discovers new routes, NPCs, story hooks
- Optional content access

**Option 4: Equipment Purchase**
- Purchase equipment upgrades
- Costs coins for permanent improvements
- Makes deliveries easier and more efficient

### Delivery Execution: Route Travel

**Route Structure:**
Routes connect locations across different venues (districts). Each route consists of sequential segments. Player must traverse all segments to reach destination.

**Route Properties Visible Before Departure:**
- Total segment count
- Segment type distribution (fixed environmental vs random events)
- Destination venue
- Delivery reward (coins earned upon completion)

**Segment Types:**

**Fixed Environmental Segment:**
- Face-down card on first travel
- Encounter presents, player makes choice, card flips face-up
- On subsequent travels, face-up card shows exact cost before entry
- Consistent encounter each time traveled
- Learnable and optimizable
- Forms route "personality"
- Rewards route mastery

Examples: "Steep Hill" always presents climbing challenge. "Tollgate" always requires payment or detour. "Narrow Alley" always tests agility or caution.

**Event Segment:**
- Always face-down
- Random encounter drawn from event pool each time traveled
- Never predictable, never learned
- Different every time
- Cannot optimize, must adapt
- Creates uncertainty
- Tests flexibility and resource buffer

Examples: Might encounter helpful merchant OR desperate bandit OR injured traveler. Same segment location, different encounter each trip.

**Route Opacity Creates Tension:**
Don't know exactly how many encounters remain on unknown routes. Must estimate if current resources sufficient. Risk assessment becomes core skill.

### Destination and Return

**Reaching Destination:**
- Delivery completes automatically
- Coin reward earned immediately
- Optional: Explore destination location (costs remaining time segments)

**Return Journey:**
- Same route traversed in reverse
- Fixed segments already revealed (if traveled before)
- Event segments still random
- Resources already depleted from outbound trip
- Return navigation often tighter, requires careful planning

**Arriving Back:**
- Return to starting location (usually tavern hub)
- Enter evening phase (mandatory survival spending)
- Calculate net profit (delivery earnings minus survival costs)
- Sleep advances to next day

### Evening Phase: Survival Spending

**Mandatory Survival Costs:**
After primary activity, remaining time segments used for:
- Purchase food (restores hunger, costs coins)
- Rent lodging or find shelter (restores energy, costs coins)
- Survival costs consume most delivery earnings

**Optional Brief Actions (if time remains):**
- Quick NPC conversation
- Equipment purchase

**Sleep:**
- Advances to next day
- Restores energy to maximum
- Day counter increments

### What Makes MEDIUM Loop Engaging

**Economic Pressure:**
Delivery earnings barely cover survival costs. Small profit margin requires consistent successful deliveries. Single failed delivery or costly mistake can eliminate buffer.

**Example cycle:**
- Accept delivery: earn 30 coins
- Route requires: 6 time segments, various energy/hunger costs during encounters
- Return to hub: mandatory food purchase 10 coins, lodging 15 coins
- Net profit: 5 coins
- Equipment upgrade costs: 60 coins (12 successful deliveries needed)

**Resource Management Skill:**
Optimization provides measurable advantage. Learning routes reveals fixed costs. Mastering resource management enables profit. Skill improvement is satisfying.

**Risk vs Reward Decisions:**
Take longer safer route (more encounters but less danger)? Take shorter risky route (fewer encounters but higher difficulty)? Calculated risk assessment.

**Natural Session Structure:**
Complete delivery cycle provides satisfying stopping point. Progress measurable (coins earned, day advanced). Return to safe hub creates psychological closure.

---

## 2.4 LONG LOOP: Character Development (hours)

### Loop Structure

```
Repeat Delivery Cycles
    ↓
Accumulate Small Profits
    ↓
Purchase Equipment Upgrades (permanent improvements)
    ↓
Develop NPC Bonds (unlock mechanical benefits)
    ↓
Access Harder Routes (better rewards)
    ↓
Discover New Locations (expand world)
    ↓
Build Identity Through Specialization
```

### Progression Vectors

**Stat Development:**
- Challenge success grants stat XP
- NPC training provides stat boosts
- Stats gate content (choices, dialogue options, challenge difficulty)
- Specialization emerges (cannot max all stats)

**Economic Advancement:**
- Accumulate profits from deliveries
- Purchase equipment (permanent improvements)
- Equipment makes challenges easier
- Better efficiency enables higher-risk/higher-reward routes

**Relationship Building:**
- Invest time in NPC conversations
- Advance bond levels through scenes
- Unlock mechanical benefits:
  - Level 2: Route information (shortcuts, reduced segments)
  - Level 3: Economic advantage (discounts, better jobs)
  - Level 4: Stat training (permanent stat increases)
  - Level 5: Exclusive access (special routes, equipment)
- Each relationship requires separate investment

**World Discovery:**
- A-story phases unlock new regions
- New regions contain new routes
- New routes lead to new locations
- New locations have new NPCs
- World expands organically through progression

### The Impossible Choice: Investment vs Survival

**Costs:**
- Each NPC bond scene costs time segments
- Opportunity cost (could be doing delivery instead)
- Short-term resource pressure accepted

**Benefits:**
- Mechanical advantages compound over time
- Route shortcuts save time on every future delivery
- Stat increases enable new approaches permanently
- Economic benefits improve every transaction

**The Tension:**
Spend today's time/coins on NPC for future benefit, or take delivery for immediate survival needs? Bond investment requires accepting short-term resource pressure for long-term advantage.

### Build Variety Through Specialization

**Cannot max all stats.** Limited resources force specialization. Must choose which stats to develop, which approaches to enable.

**Different builds unlock different experiences:**

**Investigator Build (Insight + Cunning):**
- Dominates Mental challenges
- Spots hidden paths, solves puzzles
- Weak in Social challenges (no Rapport/Authority)
- Perfect for investigation-focused play

**Diplomat Build (Rapport + Diplomacy):**
- Dominates Social challenges
- Befriends NPCs easily, negotiates conflicts
- Weak in Mental challenges (no Insight)
- Perfect for relationship-focused play

**Leader Build (Authority + Diplomacy):**
- Balanced Social (directive and balanced)
- Commands respect, formal solutions
- Weak in Insight and Cunning
- Vulnerable in investigations requiring subtlety

**Hybrid Builds:**
- Balanced capabilities, less specialized
- Flexible but never dominant
- Can handle variety of situations adequately
- No distinct vulnerability or excellence

**Specialization creates identity:** Your character IS your build. What you excel at, what you struggle with, how you solve problems - all defined by stat distribution.

**Specialization creates vulnerability:** Routes requiring neglected stats become harder or force expensive alternative choices. Accept weakness or invest in balance.

### What Makes LONG Loop Engaging

**Build Identity Emergence:**
Over hours of play, specialization patterns emerge. Players develop distinct character identity through stat distribution and relationship choices.

**Compounding Benefits:**
Early investments pay off repeatedly. Route shortcut discovered at hour 5 saves time for all future travels. Stat point gained at hour 10 enables new options for rest of game.

**Strategic Meta-Decisions:**
Which NPCs to bond with? Which stats to prioritize? Which equipment to purchase first? Decisions made in hour 1 affect options in hour 50.

**World Understanding:**
Learning route personalities. Discovering optimal paths. Understanding NPC networks. Mastery emerges through repeated engagement.

**Infinite Horizon:**
A-story never ends. Always new regions ahead. Always more to discover. No pressure to "finish" - just continue traveling.

---

## 2.5 Loop Interdependencies

### How Loops Connect

**SHORT loop feeds MEDIUM loop:**
- Each encounter-choice is one SHORT loop
- Delivery route is sequence of SHORT loops
- Resource management in SHORT loops determines MEDIUM loop success
- Failed SHORT loop decisions compound into failed delivery

**MEDIUM loop feeds LONG loop:**
- Each delivery earns coins for equipment/NPCs
- Each completed route increases mastery
- Consistent MEDIUM loop success enables LONG loop investment
- Failed MEDIUM loops delay character advancement

**LONG loop improves SHORT loop:**
- Higher stats enable better SHORT loop choices
- Equipment makes SHORT loop encounters easier
- NPC benefits provide SHORT loop advantages
- LONG loop investment pays off in SHORT loop efficiency

**LONG loop improves MEDIUM loop:**
- Route shortcuts (from NPCs) reduce MEDIUM loop time
- Stat increases enable MEDIUM loop optimization
- Better equipment reduces MEDIUM loop risk
- LONG loop specialization defines MEDIUM loop approach

### Positive Feedback Loops

**Success Compounds:**
- Successful deliveries → Profit → Equipment → Easier deliveries → More profit
- Stat increases → Better choices → More challenge success → More stat increases
- Route mastery → Faster travel → More deliveries per day → More income

**But:** Resource scarcity prevents runaway success. Survival costs scale. Challenges remain challenging. Optimization provides efficiency, not immunity.

### Negative Feedback Loops

**Failure Compounds:**
- Failed delivery → No profit → Can't afford food → Energy penalties → Harder encounters
- Low stats → Limited choices → Forced into bad options → Resource loss → Less capability
- Equipment neglect → Harder challenges → More failures → Less income → Can't afford equipment

**But:** No soft-locks. Always recovery paths. Can take easier routes. Can skip investments temporarily. Forward progress always possible.

---

## 2.6 Calendar Pressure: Time as Universal Resource

### Time Advancement

**Daily Cycle:**
Each sleep advances calendar to next day. Day number displayed. Time is unidirectional, cannot reverse.

**Pressure Creates Priority:**
Limited days mean limited opportunities. Cannot do everything. Must choose which activities matter most.

### Time Segment Philosophy

**Time is the universal scarce resource.** Everything competes for time segments.

- Deliveries earn coins but consume time
- NPCs build capabilities but consume time
- Exploration discovers content but consumes time
- Can't do everything. Must prioritize.

**Persona parallel:** Just as Persona forces choice between dungeon crawling, social links, and stat grinding within limited calendar days, Wayfarer forces choice between deliveries (income), NPCs (investment), and exploration (discovery) within limited daily segments.

### Pressure Sources

**Daily Hunger Drain:**
Wake each morning with hunger partially depleted from previous day. Must eat daily (costs coins). Skipping food causes energy penalties.

**Opportunity Cost:**
Spending time on Activity A means NOT spending time on Activity B. Every choice closes other options. Calendar advances regardless of choice.

**No Recovery:**
Days don't reset. Calendar only moves forward. Choices are permanent. Lost time is lost forever.

### No Hard Deadlines on Deliveries

**Critical design choice:** Delivery jobs have destination and reward, but no time limit for completion. Player cannot "fail" delivery by taking too long.

**Why:** Tight economy already creates pressure (must earn coins to eat). Adding delivery deadlines would create double punishment (fail delivery AND can't afford food). Single pressure source (survival costs) is sufficient.

**Effect:** Players feel pressure from scarcity, not from arbitrary time limits. Creates contemplation space. Can pause, evaluate, make informed decisions without urgency manipulation.

---

## 2.7 The Atmospheric Action Layer

### Concept

**Two parallel action generation systems:**

**Scene-Based Actions (ephemeral content):**
- Generated when active Scene/Situation exists at location
- Story-driven narrative choices
- Temporary, disappear when scene completes

**Atmospheric Actions (persistent scaffolding):**
- Always available at every location regardless of scene state
- Navigation and utility actions
- Permanent, part of location's baseline existence

**Both coexist.** Scene actions layer on top of atmospheric actions. Player sees both simultaneously when scene active, sees only atmospheric when no scene.

### The Quiet Location Problem

**Wrong conception:** "Empty location has no actions."

**Correct conception:** "Location without scene has atmospheric actions only. Navigation always possible. Utility always available. Story content is additional layer, not requirement."

### Atmospheric Action Categories

**Navigation Actions (Critical):**

**Within-Venue Movement:**
- One "Move to X" action per adjacent location in same venue
- Instant travel, zero time cost, zero resource cost
- Example: "Move to Tavern Common Room", "Move to Tavern Upstairs"
- Generated dynamically from venue's location list

**Between-Venue Travel:**
- One "Travel to another district" action present at every location
- Opens route selection screen (separate UI mode)
- Shows available destination venues with route information
- Player selects destination → enters route segment travel gameplay
- This is how delivery travel initiates

**Universal Actions:**

**Inventory Management:**
- "Check belongings" - view resources, equipment, items
- Available everywhere, zero time cost
- Pure information display

**Time Management:**
- "Wait" - advance time segments without moving
- "Rest" - restore energy (costs time, location-dependent availability)
- Only available when contextually appropriate

**Location-Specific Actions:**

Generated from location's **LocationType** property:

**If Tavern:**
- "Order food" - spend coins, restore hunger
- "Rent room" - spend coins, prepare for sleep
- "View available deliveries" - see delivery opportunities
- "Talk to patrons" - generic NPC interaction if no specific scene

**If Shop:**
- "Browse goods" - view/purchase equipment
- "Sell items" - convert items to coins

**If Home/Safe Location:**
- "Sleep" - free energy restoration

### Action Display Priority

**When Scene Active:**
```
[Scene-Based Actions from Active Situation]
------------------------
[Atmospheric Actions]
```
Scene actions prominent, atmospheric actions below separator.

**When No Scene Active:**
```
[Location Name - Atmospheric Description]
"The tavern is quiet this evening. Few patrons remain."

[Atmospheric Actions]
```
Atmospheric actions are primary content. Location description sets mood but gameplay continues through persistent actions.

### Why Atmospheric Layer Matters

**No dead ends:** Every location has navigation actions. Player can always leave. Can always make progress.

**No soft-locks:** Even with no active scenes, player can travel, manage resources, advance time. Forward progress guaranteed.

**Coherent world:** Locations exist independently of story content. World feels real, not just scene delivery mechanism.

**Player agency:** Player controls pacing. Can engage with scenes when ready, or ignore and navigate elsewhere. Game doesn't force engagement.

---

## 2.8 Related Documentation

### Other Design Sections

- **[01_design_vision.md](01_design_vision.md)** - Core experience statement, player fantasy, emotional goals
- **[03_progression_systems.md](03_progression_systems.md)** - Stat progression, economic systems, content unlocking
- **04_choice_design_patterns.md** - Four-choice pattern, path diversity, balanced choices
- **05_challenge_systems.md** - Mental/Physical/Social tactical layer design
- **06_world_structure.md** - Spatial hierarchy, venues, routes, hex travel

### Technical Architecture (arc42)

- **[arc42/05_building_block_view.md](../05_building_block_view.md)** - Technical implementation of loop systems
- **[arc42/06_runtime_view.md](../06_runtime_view.md)** - Action flow, choice execution, state transitions
- **[arc42/08_concepts.md](../08_concepts.md)** - Session management, state persistence

### Core Philosophy

- **[WAYFARER_CORE_GAME_LOOP.md](../WAYFARER_CORE_GAME_LOOP.md)** - Complete technical specification of loops
- **[DESIGN_PHILOSOPHY.md](../DESIGN_PHILOSOPHY.md)** - Principle 6 (Resource Scarcity), Principle 10 (Perfect Information)
- **[GLOSSARY.md](../GLOSSARY.md)** - Scene, Situation, Choice, Route, Segment definitions

---

**Document Status:** Production-ready game design specification
**Last Updated:** 2025-11
**Maintained By:** Design team
