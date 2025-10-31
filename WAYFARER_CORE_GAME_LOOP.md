# Wayfarer: Core Game Loop Design

**Version:** 1.0  
**Date:** 2025-10-31  
**Model:** Courier Life + Persona Calendar + Tight Economy

---

## Executive Summary

Wayfarer is a single-player visual novel RPG where you play a courier managing scarce resources across a hex-based city. The core loop is **accepting delivery jobs → navigating route segments with choice-driven situations → earning coins → spending on survival → repeat**. Resource pressure creates impossible choices. Optimization skill determines success. Visual novel scenes provide narrative content but gameplay comes first.

**Design Philosophy:** GAME first, story simulator second. Tight economy where delivery earnings barely cover survival costs. Every choice has clear resource trade-offs. No boolean gates, only graduated costs competing for shared scarce resources.

---

## Core Loop Structure

### Three-Tier Loop Hierarchy

**SHORT LOOP (10-30 seconds):**
- Situation presents narrative context
- Player evaluates 2-4 choices showing visible costs
- Player selects choice
- Resources change immediately
- Progress advances to next situation

**MEDIUM LOOP (5-15 minutes):**
- Wake at location
- Accept delivery job
- Travel route segments (chain of situation-choice cycles)
- Reach destination, earn coins
- Return to starting location
- Spend coins on survival (food, lodging)
- Sleep advances day

**LONG LOOP (hours):**
- Repeat delivery cycles
- Accumulate small profits
- Purchase equipment upgrades (permanent improvements)
- Develop NPC bonds (unlock mechanical benefits)
- Access harder routes with better rewards
- Discover new locations and opportunities

### What Makes It Engaging

**Visible Trade-offs:**
Every choice shows exact costs before selection. Multiple valid approaches exist (spend energy OR coins OR time). No "correct" path, only different costs.

**Resource Pressure:**
Energy and hunger deplete during travel. Must balance "advance quickly" vs "conserve resources". Running out forces failure or costly detours.

**Route Opacity:**
Don't know exactly how many situations remain on unknown routes. Must estimate if current resources sufficient. Risk assessment becomes core skill.

**Optimization Rewards:**
Learning routes reveals fixed segment costs. Mastering resource management enables profit. Skill improvement is measurable and satisfying.

---

## Daily Rhythm Structure

### Morning Phase

**Wake at current location** with full time segments available.

**Review current state:**
- Available time segments for the day
- Current resources (coins, energy, hunger)
- Current location and nearby options
- Available delivery jobs

**Choose primary activity:**

**Accept Delivery (Primary Income):**
- View available jobs with visible rewards and distance
- Select delivery destination
- Consumes majority of day's time segments
- Earns coins upon completion
- Core gameplay activity

**NPC Interaction (Investment):**
- Spend time with specific NPC
- Costs time segments
- Advances bond level toward mechanical benefits
- Delayed gratification for future advantage

**Local Exploration (Discovery):**
- Investigate current location
- Costs time and resources
- Discovers new routes, NPCs, story hooks
- Optional content access

**Equipment Purchase:**
- Purchase equipment upgrades
- Costs coins for permanent improvements
- Makes deliveries easier and more efficient

### Evening Phase

**Mandatory Survival Spending:**
After primary activity, remaining time segments used for:
- Purchase food (restores hunger, costs coins)
- Rent lodging or find shelter (restores energy, costs coins)
- Survival costs consume most delivery earnings

**Optional Brief Actions:**
If time remains:
- Quick NPC conversation
- Equipment purchase

**Sleep:**
- Advances to next day
- Restores energy to maximum
- Day counter increments

### Time Segment Philosophy

**Time is the universal scarce resource.** Everything competes for time segments. Deliveries earn coins but consume time. NPCs build capabilities but consume time. Can't do everything. Must prioritize.

**Persona parallel:** Just as Persona forces choice between dungeon crawling, social links, and stat grinding within limited calendar days, Wayfarer forces choice between deliveries (income), NPCs (investment), and exploration (discovery) within limited daily segments.

---

## Delivery Execution: The Core Gameplay

### Route Structure

Routes connect locations across different venues (districts). Each route consists of sequential segments. Player must traverse all segments to reach destination.

**Route Properties:**
- Total segment count (visible before departure)
- Segment type distribution (fixed environmental vs random events)
- Destination venue
- Delivery reward (coins earned upon completion)

### Segment Types

**Fixed Environmental Segment:**

Face-down card on first travel. Situation presents, player makes choice, card flips face-up revealing exact effect. On subsequent travels, face-up card shows cost before entry.

**Characteristics:**
- Consistent situation each time traveled
- Learnable and optimizable
- Forms route "personality"
- Rewards route mastery

**Examples:**
- "Steep Hill" always presents climbing challenge
- "Tollgate" always requires payment or detour
- "Narrow Alley" always tests agility or caution

**Event Segment:**

Always face-down. Random situation drawn from event pool each time traveled. Never predictable, never learned.

**Characteristics:**
- Different every time
- Cannot optimize, must adapt
- Creates uncertainty
- Tests flexibility and resource buffer

**Examples:**
- Might encounter helpful merchant OR desperate bandit OR injured traveler
- Same segment location, different situation each trip
- Some routes have more event segments (higher variance, higher risk)

### Situation-Choice Structure

**Each segment presents one situation with 2-4 choices.**

**Example Situation: "Muddy Road Ahead"**

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

**No Correct Answer. Only Different Costs.**

The player with high energy takes choice 1. The player with time pressure takes choice 3. The player with high Physical stat gambles on choice 4. The player who has traveled this route before knows to conserve energy for harder segments ahead and takes choice 2.

**This is the core skill: resource optimization through informed choice.**

### Challenge Integration

Some choices spawn tactical challenges (Physical, Social, Mental). Challenges are separate gameplay mode with their own mechanics (outside scope of this document).

**Success:** Better resource efficiency, extra rewards
**Failure:** Worse resource costs, penalties

Challenges are optional gambles. Always alternative guaranteed choices exist. Players can avoid all challenges and complete deliveries through pure resource management.

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

---

## NPC Bond System

### Structure

NPCs have bond levels representing relationship depth. Spending time with NPC advances bond through conversation scenes. Each bond level is one scene with choices.

**Bond Progression:**
- Bond Level 1: Initial meeting, basic introduction
- Bond Level 2: Deeper conversation, personal story
- Bond Level 3: Meaningful connection, trust established
- Bond Level 4: Close relationship, mutual respect
- Bond Level 5: Deep bond, special access

**Scene Structure:**
Each bond level scene presents 2-4 dialogue choices. Choices affect relationship flavor (friendly vs professional, humorous vs serious) but all paths advance bond. No failure states. The investment is time/coins, not correct dialogue selection.

### Mechanical Benefits (GAME FIRST)

**Bond Level Rewards:**

**Level 2 Unlock: Route Information**
- NPC reveals hidden shortcut on specific route
- Permanently reduces segment count on that route
- Information persists, applies to all future travels
- Mechanical benefit: time segment savings

**Level 3 Unlock: Economic Advantage**
- Discount at NPC's shop or service
- Reduced costs for specific resource type
- Or: NPC offers special delivery jobs with better pay
- Mechanical benefit: improved coin efficiency

**Level 4 Unlock: Stat Training**
- NPC trains player in their expertise area
- Permanent stat point increase in specific stat
- One-time benefit, significant impact
- Mechanical benefit: unlocks new choices, easier challenges

**Level 5 Unlock: Exclusive Access**
- NPC offers unique delivery opportunities
- Access to special routes or locations
- Special equipment purchase options
- Or: Emergency resource assistance (safety net)
- Mechanical benefit: new opportunities, reduced risk

### Investment Tension

**Costs:**
- Each bond scene costs time segments
- Opportunity cost (could be doing delivery instead)

**Benefits:**
- Mechanical advantages compound over time
- Route shortcuts save time on every future delivery
- Stat increases enable new approaches permanently
- Economic benefits improve every transaction

**The Impossible Choice:**
Spend today's time/coins on NPC for future benefit, or take delivery for immediate survival needs? Bond investment requires accepting short-term resource pressure for long-term advantage.

### NPC Availability

NPCs exist at specific locations. Some NPCs require stat thresholds to begin bonding (cannot approach merchant without minimum Rapport). Creates progression gates through stat development.

---

## Stat System

### Five Core Stats

**Insight:** Observation, deduction, pattern recognition  
**Rapport:** Empathy, friendliness, emotional connection  
**Authority:** Command, intimidation, formal power  
**Diplomacy:** Negotiation, persuasion, balanced approach  
**Cunning:** Deception, street smarts, unconventional thinking

### Stats Gate Content

**Dialogue Options:**
Higher stats unlock additional dialogue choices in NPC scenes and route situations. Locked choices display required stat level (Requirement Inversion Principle: show "Need Authority 3" not binary hide).

**Challenge Difficulty:**
Higher relevant stat makes challenges easier, increases success probability, reduces resource costs on failure.

**NPC Access:**
Some NPCs require minimum stat level to begin bonding. Merchant needs Rapport 3. Guard captain needs Authority 2. Thief contact needs Cunning 3.

**Choice Availability:**
Route situation choices sometimes require stats. "Spot hidden path" needs Insight 2. "Intimidate bandits" needs Authority 3. Locked choices visible with requirement path shown.

### Stat Progression

**Challenge Success:**
Successfully completing Physical/Social/Mental challenges grants small stat increases in relevant stats. Incremental growth through repeated success.

**NPC Training:**
Bond Level 4 with specific NPCs grants permanent stat point increase. Significant one-time boost. Limited by NPC availability.

### Specialization is Intentional

**Cannot max all stats.** Limited resources force specialization. Must choose which stats to develop, which approaches to enable.

**Different builds unlock different routes:**
- High Insight/Cunning: Investigative approach, spot hidden paths, solve puzzles
- High Rapport/Diplomacy: Social approach, befriend NPCs easily, negotiate conflicts
- High Authority/Diplomacy: Leadership approach, command respect, formal solutions
- Hybrid builds: Balanced capabilities, less specialized

**Specialization creates vulnerability:** Routes requiring neglected stats become harder or force expensive alternative choices. Accept weakness or invest in balance.

---

## Calendar Pressure

### Time Advancement

**Daily Cycle:**
Each sleep advances calendar to next day. Day number displayed. Time is unidirectional, cannot reverse.

**Pressure Creates Priority:**
Limited days mean limited opportunities. Cannot do everything. Must choose which activities matter most.

### Pressure Sources

**Daily Hunger Drain:**
Wake each morning with hunger partially depleted from previous day. Must eat daily (costs coins). Skipping food causes energy penalties.

### No Hard Deadlines on Deliveries

**Critical design choice:** Delivery jobs have destination and reward, but no time limit for completion. Player cannot "fail" delivery by taking too long.

**Why:** Tight economy already creates pressure (must earn coins to eat). Adding delivery deadlines would create double punishment (fail delivery AND can't afford food). Single pressure source (survival costs) is sufficient.

---

## The Atmospheric Action Layer

### Concept

**Two parallel action generation systems:**

**Scene-Based Actions (ephemeral content):**
Generated when active Scene/Situation exists at location. Story-driven narrative choices. Temporary, disappear when scene completes.

**Atmospheric Actions (persistent scaffolding):**
Always available at every location regardless of scene state. Navigation and utility actions. Permanent, part of location's baseline existence.

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

---

## Action Generation Flow (High-Level)

### Query Sequence

**Player enters location:**

**Step 1: Scene Query**
- Check GameWorld for active Scenes at this location
- If Scene exists with Situation at this location:
  - Activate Situation (Dormant → Active state transition)
  - Instantiate ChoiceTemplates into action entities
  - Generate provisional Scenes for choices with spawn rewards
  - Categorize actions by requirement availability

**Step 2: Atmospheric Generation**
- Query location's venue for adjacent locations
  - Generate one "Move to X" action per adjacent location
- Generate one "Travel to another district" action (universal)
- Query location's LocationType for location-specific actions
  - Generate food/lodging actions if Tavern
  - Generate shop actions if Shop
- Generate universal actions (inventory, time management)

**Step 3: Combine and Display**
- If scene actions exist: display scene actions, separator, atmospheric actions
- If no scene actions: display location description, atmospheric actions only
- UI renders unified action list

### Execution Routes

**Player selects scene action:**
- Route to GameFacade for full execution pipeline
- Apply costs, evaluate requirements, spawn challenges
- Apply rewards, advance Situation, spawn new Scenes
- Delete ephemeral actions, cleanup provisional Scenes

**Player selects navigation action:**
- If within-venue: immediate location transition, zero cost
- If between-venue: open route selection screen, await destination choice

**Player selects universal action:**
- If inventory: open inventory display (no state change)
- If wait/rest: advance time segments, restore resources

---

## Resource Pressure Philosophy

### Tight Economy Design

**Delivery earnings barely cover survival costs.** Small profit margin requires consistent successful deliveries. Single failed delivery or costly mistake can eliminate buffer. Resource management skill is survival requirement.

**Example cycle:**
- Accept delivery: earn 30 coins
- Route requires: 6 time segments, various energy/hunger costs during situations
- Return to hub: mandatory food purchase 10 coins, lodging 15 coins
- Net profit: 5 coins
- Equipment upgrade costs: 60 coins (12 successful deliveries needed)

**Why tight economy:**
- Every choice matters mechanically
- Optimization skill provides measurable advantage
- Investment decisions (NPCs, equipment) feel genuinely costly
- Resource management is core gameplay challenge
- Failure is possible but recoverable (can take easier routes, skip investments)

### The Impossible Choice Framework

**Impossible choice:** Two or more valid options, insufficient resources for both, must accept one cost to avoid another.

**Wayfarer examples:**

**Route situation:**
- Spend energy now (tire yourself) OR spend time segments (delay return, risk missing next job) OR spend coins (reduce profit margin)

**Daily priority:**
- Take delivery for survival income OR develop NPC bond for future advantage OR explore for content discovery

**Stat development:**
- Develop Insight/Cunning (investigative specialist) OR Rapport/Diplomacy (social specialist) OR balanced approach (no specialization advantage)

**Long-term planning:**
- Save for expensive equipment upgrade (delayed gratification) OR maintain higher resource buffer (safety and flexibility)

**No correct answer exists.** Different players prioritize differently. Different situations favor different choices. Skill is adapting strategy to current constraints and recognizing which costs affordable versus which costs catastrophic.

---

## Design Principles Applied

**Principle 1: Single Source of Truth**
- GameWorld owns all entities in flat lists
- Scenes reference locations by ID, don't own them
- NPCs exist independently, Scenes spawn at their locations

**Principle 2: Strong Typing**
- List<LocationAction>, List<NPCAction>, List<PathCard>
- No dictionaries of generic types
- Every relationship is explicitly typed

**Principle 3: Ownership vs Placement**
- Locations are permanent scaffolding (owned by GameWorld)
- Scenes are ephemeral content (placed at locations temporarily)
- NPCs are persistent entities (placed at locations, can move)

**Principle 4: Inter-Systemic Rules Over Boolean Gates**
- Choices cost resources (energy/coins/time), not unlocked by flags
- Stats gate choices through arithmetic comparison (need Insight 3, have Insight 2)
- NPC bonds earned through time investment, grant mechanical benefits

**Principle 5: Typed Rewards as System Boundaries**
- ChoiceReward has specific types (coins, energy, stat increases, scene spawns)
- No boolean flags set, only explicit typed effects applied

**Principle 6: Resource Scarcity Creates Impossible Choices**
- All systems compete for time segments
- Energy, hunger, coins all scarce
- Cannot do everything, must prioritize

**Principle 10: Perfect Information with Hidden Complexity**
- All choice costs visible before selection (strategic layer)
- Challenge outcomes uncertain until played (tactical layer)
- Route segment count visible, but event segments unpredictable

---

## Summary

Wayfarer's core loop is **resource-constrained delivery gameplay with visual novel presentation.** The courier accepts jobs, navigates route segments through choice-driven situations, earns coins barely sufficient for survival, and gradually builds capabilities through optimization skill and strategic investment.

**Tight economy creates pressure.** Every choice matters. Resource management is the core skill. Optimization is rewarded. Investment decisions are genuinely costly.

**Atmospheric actions solve the quiet location problem.** Navigation always possible. Utility always available. Scene content layers on top of persistent scaffolding.

**Calendar pressure drives urgency.** Time segments scarce. Days advance inexorably. Cannot do everything. Must prioritize impossible choices.

**GAME first, story second.** Mechanical systems create strategic depth. Narrative content provides context and flavor. Visual novel scenes are delivery vehicle for choice-driven resource management gameplay, not pure story consumption.

The player who masters route optimization, manages resources carefully, invests strategically in NPCs and equipment, and plans daily priorities will thrive. The player who makes careless choices, ignores resource pressure, and fails to plan will struggle. This is intentional. This is the game.
