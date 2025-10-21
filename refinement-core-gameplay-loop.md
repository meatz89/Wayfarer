# Wayfarer V2 → Core Loop Refactoring

## Executive Summary

This document describes the complete refactoring from the V2 system to the Core Loop design. The refactoring preserves all existing V2 architecture (entities, challenge systems, card mechanics) while adding transparent economic progression through three interconnected changes:

1. **Resource Scaling:** 16 segments (not 23), 6-point pools (not 100), 1-3 intensity (not 15-70)
2. **Obligation System:** Investigations renamed to Obligations with two types (NPC-commissioned with deadlines, self-discovered without)
3. **Verisimilitude Systems:** Equipment and knowledge with context-specific applicability (not global reductions)

**Core principle:** All content physically exists from game start. Progression is economic affordability (coins → equipment → surviving dangerous routes → accessing distant locations), not arbitrary unlocking.

## Motivation: Why This Refactoring

### Problem with V2 Abstract Progression

**V2 Knowledge Gates:**
- "Complete Phase 1 to unlock Phase 2" (boolean gate)
- "Have Forest Knowledge to access Forest route" (arbitrary requirement)
- Player question: "Why can't I go to Forest?" Answer: "You don't have the knowledge token."

**Lack of verisimilitude:** Knowledge tokens abstract. Route existence vs route accessibility unclear. No resource competition (just completion checks).

### Core Loop Solution

**Economic Progression:**
- Forest route exists at start. Has PhysicalDanger 2 obstacles requiring 2 Health each (4 Health total).
- Without equipment: Attempting route costs 4 Health out of 6 total (probably lethal).
- With waders (12 coins): Water obstacles reduce to 1 Health each (2 Health total, survivable).
- Player question: "Can I reach Forest?" Answer: "Yes if you can afford survival. Check obstacle contexts and your equipment."

**Verisimilitude:** Routes are physical geography with real hazards. Equipment provides real protection. Coins buy equipment. Everything transparent.

### Problem with V2 Resource Ambiguity

**V2 Scale:**
- 100 Health → "Obstacle costs 25 Health" → How significant is this?
- 23 time segments → Why 23? How much is 1 segment worth?
- Abstract numbers don't map to player intuition

**Core Loop Solution:**
- 6 Health → "Obstacle costs 2 Health" → 33% of capacity, clearly significant
- 16 segments (4 blocks × 4) → 1 block = ¼ day, easy mental math
- Small numbers with clear proportional meaning

### Problem with V2 Equipment Genericity

**V2 Pattern:**
- "Rope reduces PhysicalDanger by 20"
- Applies to: Climbing, water crossing, guard checkpoint, mechanical puzzle
- Player question: "Why does rope help with talking to a guard?"

**Core Loop Solution:**
- "Rope applies to [Climbing, Height, Securing] contexts"
- Helps: Cliff ascent (Climbing + Height)
- Doesn't help: Creek ford (Water + Endurance), guard checkpoint (Authority)
- Player understanding: "Rope helps vertical movement and securing, not water or social."

## Core Principles Preserved from V2

### Architecture Unchanged

**Entity ownership:**
- GameWorld owns all entities via flat lists (single source of truth)
- Investigations (now Obligations) own Obstacles (lifecycle)
- Obstacles own Goals (lifecycle)
- Locations/NPCs reference entities (placement, not ownership)

**Challenge systems intact:**
- Mental: ACT/OBSERVE, Progress/Attention/Exposure, Details→Methods→Applied
- Physical: EXECUTE/ASSESS, Breakthrough/Exertion/Danger, Situation→Options→Situation
- Social: SPEAK/LISTEN, Momentum/Initiative/Doubt, Topics→Mind→Spoken

**Card mechanics preserved:**
- Card depths (1-6+)
- Stat binding (Insight/Rapport/Authority/Diplomacy/Cunning)
- Card effects (builder resources, session resources, thresholds)

**Goal-GoalCard structure maintained:**
- Goals define strategic actions available
- GoalCards define tactical victory conditions
- Inline GoalCards within Goals
- Rewards typed (GrantCoins, ModifyProperty, etc.)

### What We're NOT Changing

- Three parallel challenge systems
- Card play mechanics
- Stat progression (XP from cards, depth gating)
- GameWorld flat list architecture
- Goal placement at Locations/NPCs
- Phase system within Obligations
- Reward application (one-time, typed)

**Strategy:** Extend existing entities with new fields, scale existing values, add new equipment/route segment entities. Do not replace core architecture.

## Resource Scaling Changes

### Time: 23 Segments → 16 Segments (4 Blocks)

**Current V2:**
- 23 segments per day (arbitrary number)
- No blocks (continuous)
- Goals cost 2-4 segments
- Routes cost 3-9 segments

**Core Loop:**
- 16 segments per day (4 blocks × 4 segments)
- Blocks: Morning/Midday/Afternoon/Evening (equal, no special mechanics)
- Goals cost 1-2 segments
- Routes cost 2-4 segments
- Blocks provide temporal landmarks for player orientation

**Rationale:** 4 blocks = ¼ day each = 4 hours. Easy mental math. Morning (segments 1-4), Midday (5-8), Afternoon (9-12), Evening (13-16). Deadlines reference segments: "Deliver by segment 8" = "by midday."

**Scaling formula:** All V2 time costs ÷ 1.5 (round to whole segments).

### Health/Focus/Stamina: 100 Points → 6 Points

**Current V2:**
- Health: 100 (ambiguous value per point)
- Focus: 100 (mental capacity)
- Stamina: 100 (physical capacity)
- Goal costs: 20-40 Focus, 15-30 Stamina
- Obstacle damage: 15-70 Health

**Core Loop:**
- Health: 6 (1 point = 16.7% of life)
- Focus: 6 (mental capacity)
- Stamina: 6 (physical capacity)
- Goal costs: 2-4 Focus, 1-3 Stamina
- Obstacle damage: 1-3 Health

**Rationale:** Every point significant and countable. Player can reason: "This costs 2 Focus, I have 6 total, I can afford 3 such challenges today." Clear proportional thinking.

**Scaling formula:** All V2 resource costs ÷ 17 (round to whole points).

### Obstacle Properties: 15-70 → 1-3 Intensity

**Current V2:**
- PhysicalDanger: 15-70 (abstract scale)
- MentalComplexity: 15-70
- SocialDifficulty: 15-70
- Meaning: Unclear without calculation

**Core Loop:**
- PhysicalDanger: 1-3 (direct Health cost)
- MentalComplexity: 1-3 (direct Focus cost)
- SocialDifficulty: 1-3 (Doubt baseline contribution)
- Meaning: 1 = manageable, 2 = serious, 3 = severe

**Rationale:** Intensity directly maps to resource cost. "PhysicalDanger 2" means "2 Health risk." Equipment reduces intensity. Simple, transparent.

**Scaling formula:** V2 obstacle values ÷ 23, round to 1-3 range.

### Equipment Effects: -20 → -1 Reduction

**Current V2 (if existed):**
- Rope reduces PhysicalDanger by 20
- Lantern reduces MentalComplexity by 20

**Core Loop:**
- Rope reduces applicable obstacles by 1 intensity
- Lantern reduces applicable obstacles by 1 intensity
- Climbing gear (powerful) reduces by 2 intensity

**Scaling formula:** Equipment effects ÷ 20.

### Challenge Thresholds: 12-20 → 6-10

**Current V2:**
- Progress threshold: 12-20
- Breakthrough threshold: 12-20
- Momentum threshold: 12-20

**Core Loop:**
- Progress threshold: 6-10
- Breakthrough threshold: 6-10
- Momentum threshold: 6-10

**Rationale:** With 6-point resource pools and scaled card power, thresholds must scale proportionally.

**Scaling formula:** V2 thresholds ÷ 2.

### Card Power: Scaled Proportionally

**Current V2 example:**
- Depth 1: +2 Momentum, 2 Initiative cost
- Depth 4: +6 Momentum, 4 Initiative cost

**Core Loop:**
- Depth 1: +1 Momentum, 1 Initiative cost
- Depth 2: +2 Momentum, 1 Initiative cost
- Depth 3: +2 Momentum, 2 Initiative cost
- Depth 4: +3 Momentum, 2 Initiative cost
- Depth 5: +4 Momentum, 3 Initiative cost

**Rationale:** Cards must scale with thresholds. Reaching threshold 8 should require 4-8 card plays depending on stat level.

**Scaling approach:** Author new card values targeting scaled thresholds, not mechanical conversion.

### Coins: Scaled for Equipment Economy

**Core Loop coin values:**
- NPC obligations: 15-50 coins
- Self-discovered obligations: 0 coins (rewards are items/unlocks)
- Individual goals: 5-15 coins
- Work goals (grindable): 5-8 coins

**Equipment costs:**
- Rations: 5 coins (consumable)
- Rope: 10 coins
- Waders: 12 coins
- Lantern: 15 coins
- Tools: 20 coins
- Quality clothing: 25 coins (permanent)
- Climbing gear: 30 coins (powerful)

**Economic curve:**
- Early (0-50 coins): Can afford 1-2 basic equipment
- Mid (50-150 coins): Can afford most equipment + repairs
- Late (150+ coins): Economic pressure removed

## Investigation → Obligation Reframing

### Rename Investigation Entity to Obligation

**V2 entity: Investigation**
- Owns Obstacles (lifecycle)
- Has phases (sequential progression)
- Spawns new content

**Core Loop entity: Obligation**
- **Same structure, new name**
- Still owns Obstacles (no architectural change)
- Still has phases (no architectural change)
- Still spawns new content (no architectural change)

**Why rename:** "Investigation" implies player-driven curiosity. "Obligation" encompasses both commissioned work (NPC requests) and self-discovered mysteries. Broader semantic scope.

### Add New Fields to Obligation Entity

```
Obligation (extends V2 Investigation):
  // Existing V2 fields preserved
  Id
  Name
  Description
  Phases (List<ObligationPhase>)
  
  // New fields added
  ObligationType: NPCCommissioned | SelfDiscovered
  PatronNpcId (nullable, only if NPCCommissioned)
  DeadlineSegment (nullable, only if NPCCommissioned)
  CompletionRewardCoins
  CompletionRewardItems (List<string>)
  CompletionRewardRouteReveals (List<string>)
  SpawnedObligationIds (List<string>)
```

### Two Obligation Types

**NPCCommissioned Obligations:**
- Source: Social challenge GoalCard reward (CreateObligation type)
- Purpose: Represent paid work with time pressure
- Deadline: Specific segment number (8, 12, or 16)
- Rewards: Coins + Story cubes
- Failure consequence: Remove Story cubes from patron NPC
- Phases: Typically single-phase (complete task at destination)

**Example - Elena's Letter Delivery:**
```
Obligation:
  Id: "elena_urgent_delivery"
  Name: "Urgent Letter to Mill"
  ObligationType: NPCCommissioned
  PatronNpcId: "elena"
  DeadlineSegment: 8
  CompletionRewardCoins: 25
  
Phase 1:
  RequiredGoalIds: ["deliver_elena_letter"]
  GoalLocations: Mill location
```

**SelfDiscovered Obligations:**
- Source: Mental/Social challenge GoalCard reward (SpawnObligation type)
- Purpose: Represent curiosity-driven investigation
- Deadline: None (explore at leisure)
- Rewards: Equipment, route reveals, spawn new obligations
- Cannot fail: No time pressure
- Phases: Multi-phase (investigation progression)

**Example - Mill Mystery:**
```
Obligation:
  Id: "mill_mystery"
  Name: "Investigate Mill Damage"
  ObligationType: SelfDiscovered
  DeadlineSegment: null
  CompletionRewardItems: ["climbing_gear"]
  SpawnedObligationIds: ["forest_ruins_investigation"]
  
Phase 1: Examine exterior (2 Mental goals at Mill courtyard)
Phase 2: Interview owner (1 Social goal with mill_owner NPC)
Phase 3: Search interior (2 Mental goals at Mill interior)
Phase 4: Confront suspect (1 Social goal with suspect NPC)
```

### Obligation Lifecycle

**NPCCommissioned Flow:**
1. Player completes Social challenge with NPC
2. GoalCard reward: CreateObligation (defines patron, destination, deadline, rewards)
3. Obligation added to PlayerState.ActiveObligations
4. UI displays: "Elena: Deliver letter to Mill. Deadline: Segment 8. Reward: 25 coins, 2 Story cubes"
5. Player travels to destination, completes required goal
6. System checks: Goal completion → Active obligation fulfilled?
7. Grant rewards (coins, Story cubes), remove from active list
8. OR deadline passed → Remove Story cubes from patron, remove obligation

**SelfDiscovered Flow:**
1. Player completes goal (Mental investigation or Social conversation)
2. GoalCard reward: SpawnObligation (defines new investigation)
3. Obligation added to PlayerState.ActiveObligations (no deadline)
4. Obstacles appear at phase locations
5. Player completes phase goals → Next phase unlocks
6. All phases complete → Grant rewards (items, routes, spawned obligations)
7. Remove from active list

**V2 Investigation phases preserved:** Same phase structure, same sequential unlocking, same multi-location progression. Just now split into two semantic types with different reward/deadline structures.

### Route Navigation Flow

**Player selects route (Town → Mill):**

**Segment 1: Forest Approach**
- Display 2 paths initially:
  - Path A: "Direct trail" (1 segment, 1 stamina, no obstacle)
  - Path B: "Bramble shortcut" (1 segment, 0 stamina, obstacle: Physical/Precision/1)
- Player chooses Path A (faster, no risk)

**Segment 2: Creek Crossing**
- Display 2 paths initially:
  - Path A: "Shallow ford" (1 segment, 2 stamina, obstacle: Physical/Water+Endurance/2)
  - Path B: "Rope bridge" (2 segments, 1 stamina, obstacle: Physical/Precision+Height/2)
- If ExplorationCubes ≥ 2: Reveal Path C "Stepping stones" (1 segment, 1 stamina, obstacle: Physical/Precision/1)
- Player chooses based on equipment and risk tolerance

**After segment 2:** Arrive at Mill (2-4 segments total depending on choices).

### Exploration Cube Effect (Revised from Core-Loop Design)

**Problem with "reduce time" effect:** Distance doesn't shrink with familiarity (verisimilitude violation).

**Better effect: Reveal hidden optimal paths**
- Each route has hidden paths (HiddenUntilExploration threshold)
- Exploration cubes reveal better options
- First travel: Only obvious (often dangerous) paths visible
- After familiarity: Optimal (safer/faster) paths visible

**Example:**
- Creek Route initial paths: 2 obvious paths (both have trade-offs)
- At 2 ExplorationCubes: Reveal optimal stepping stone crossing (lower stamina, lower danger)
- At 5 ExplorationCubes: Reveal all hidden shortcuts

**Verisimilitude:** You learn the good crossings, animal trails, safe shortcuts. Geography constant, knowledge improves.

## Obstacle Taxonomy System

### Three Properties Define Every Obstacle

**1. SystemType** (unchanged from V2)
- Physical, Mental, Social
- Determines which challenge system launches

**2. Contexts** (new)
- List of specific challenge contexts
- Physical: Climbing, Water, Strength, Precision, Endurance, Combat, Height, Cold, Navigation, Securing
- Mental: Darkness, Mechanical, Spatial, Deduction, Search, Pattern, Memory, Code
- Social: Authority, Deception, Persuasion, Intimidation, Empathy, Negotiation, Etiquette

**3. Intensity** (replaces PhysicalDanger/MentalComplexity/SocialDifficulty)
- 1 = manageable (1/6 resource cost)
- 2 = serious (2/6 resource cost)
- 3 = severe (3/6 resource cost)

### Obstacle Examples

**Cliff Ascent (Manor exterior):**
```
Obstacle:
  SystemType: Physical
  Contexts: [Climbing, Height]
  Intensity: 3
  Description: "Sheer rock face, sixty feet to ledge, few handholds"
```

**Flooded Basement (Mill interior):**
```
Obstacle:
  SystemType: Physical
  Contexts: [Water, Navigation, Darkness]
  Intensity: 2
  Description: "Chest-deep cold water, floating debris, no light"
```

**Suspicious Mill Owner (Mill NPC):**
```
Obstacle:
  SystemType: Social
  Contexts: [Deception, Authority]
  Intensity: 2
  Description: "Watches carefully, deflects questions about damage"
```

**Dark Passage (Manor Route Segment 3):**
```
Obstacle:
  SystemType: Mental
  Contexts: [Darkness, Navigation]
  Intensity: 2
  Description: "Narrow tunnel, no natural light, uneven footing"
```

## Equipment System with Context Specificity

### Equipment Entity (New)

```
Equipment:
  Id
  Name
  CoinCost
  ApplicableContexts: List<string>
  IntensityReduction (1-2)
  UsageType: Exhaustible | Consumable | Permanent
  ExhaustAfterUses (1-3, if Exhaustible)
  RepairCost
  CurrentState: Active | Exhausted
```

### Equipment Catalog

**Rope (10 coins):**
- ApplicableContexts: [Climbing, Height, Securing]
- IntensityReduction: 1
- UsageType: Exhaustible
- ExhaustAfterUses: 2
- RepairCost: 3

**Helps with:**
- Cliff ascent (Climbing + Height) → 3 becomes 2
- Waterwheel repair (Mechanical + Height) → 2 becomes 1

**Doesn't help:**
- Creek ford (Water + Endurance) → no matching contexts
- Dark passage (Darkness + Navigation) → no matching contexts

**Waders (12 coins):**
- ApplicableContexts: [Water, Cold]
- IntensityReduction: 1
- UsageType: Exhaustible
- ExhaustAfterUses: 2
- RepairCost: 4

**Helps with:**
- Creek ford (Water + Endurance) → 2 becomes 1
- Flooded basement (Water + Navigation + Darkness) → 2 becomes 1

**Doesn't help:**
- Cliff ascent (Climbing + Height) → no water
- Social challenges → wrong domain

**Lantern (15 coins):**
- ApplicableContexts: [Darkness, Search, Night]
- IntensityReduction: 1
- UsageType: Exhaustible
- ExhaustAfterUses: 3
- RepairCost: 5

**Helps with:**
- Dark passage (Darkness + Navigation) → 2 becomes 1
- Flooded basement (Water + Navigation + Darkness) → 2 becomes 1
- Night investigation → applicable

**Tools (20 coins):**
- ApplicableContexts: [Mechanical, Precision, Locks]
- IntensityReduction: 1
- UsageType: Exhaustible
- ExhaustAfterUses: 2
- RepairCost: 7

**Helps with:**
- Waterwheel repair (Mechanical + Height) → 2 becomes 1
- Lock picking (Precision + Locks) → applicable

**Climbing Gear (30 coins):**
- ApplicableContexts: [Climbing, Height, Exposure]
- IntensityReduction: 2 (powerful)
- UsageType: Exhaustible
- ExhaustAfterUses: 1
- RepairCost: 15

**Helps with:**
- Cliff ascent (Climbing + Height) → 3 becomes 1 (strong effect)

**Quality Clothing (25 coins):**
- ApplicableContexts: [Authority, Persuasion, Negotiation]
- IntensityReduction: 1 (affects baseline Doubt)
- UsageType: Permanent (never exhausts)

**Helps with:**
- All Social challenges with matching contexts
- Reduces baseline Doubt permanently

**Rations (5 coins):**
- ApplicableContexts: [ANY] (special case)
- Effect: Auto-resolve any single obstacle
- UsageType: Consumable (single use)

**Universal tool:** Can apply to unexpected situations (appease guard dog, bribe informant).

### Equipment Application Logic

**When obstacle presented:**

1. Display obstacle specification:
```
Obstacle: Flooded Basement
Location: Mill interior
System: Physical
Contexts: [Water, Navigation, Darkness]
Intensity: 2
```

2. Check player equipment for matching contexts:
```
Equipment Analysis:
- Waders: Matches [Water] → Reduces by 1
- Lantern: Matches [Darkness] → Reduces by 1
- Rope: No match → Not applicable
- Tools: No match → Not applicable
```

3. Display applicability:
```
Equipment Options:
✓ Waders (Active): Intensity 2 → 1 (exhausts after use)
✓ Lantern (Active): Further reduce to 0 (exhausts after use)
✗ Rope: Not applicable (requires Climbing/Height)
✗ Tools: Not applicable (requires Mechanical)
```

4. Player chooses:
- Use both (waders + lantern): Intensity 2 → 0 (trivial, auto-success)
- Use waders only: Intensity 2 → 1 (manageable challenge)
- Use neither: Face full Intensity 2

5. Equipment exhausts if used (flip Active → Exhausted)

### Equipment Purchase Flow

**NPC Vendor Extension:**

Add to NPC entity:
```
NPC:
  // Existing V2 fields preserved
  // ...
  
  // New field
  AvailableEquipment: List<string> (equipment IDs)
```

**Purchase UI:**
1. Player views NPC equipment list
2. Click item: "Buy Rope for 10 coins?"
3. Spend coins, receive item immediately
4. Item added to PlayerState.OwnedEquipment
5. **No time cost:** Equipment shopping happens "between moments"

**Sell-back:** Player can sell equipment for 50% value (liquidity mechanism, prevents soft-lock).

## Knowledge Cube Specificity

### Three Cube Types (Location/NPC/Route Specific)

**Investigation Cubes (per Location):**
```
Location:
  // Existing V2 fields
  // ...
  
  // New field
  InvestigationCubes (0-10)
```

- Effect: -1 Exposure baseline per cube (Mental challenges only)
- Scope: Applies ONLY to Mental challenges at this specific location
- Grant: Mental goal completion at this location (+1-2 cubes)
- Verisimilitude: Familiarity with mill layout doesn't help forest navigation

**Story Cubes (per NPC):**
```
NPC:
  // Existing V2 fields
  // ...
  
  // New field
  StoryCubes (0-10)
```

- Effect: -1 Doubt baseline per cube (Social challenges only)
- Scope: Applies ONLY to Social challenges with this specific NPC
- Grant: Social goal completion with this NPC (+1-2 cubes)
- Loss: Failed obligations remove 2-3 cubes (relationship damage)
- Verisimilitude: Trust with Elena doesn't transfer to Martha

**Exploration Cubes (per Route):**
```
Route:
  // Existing V2 fields
  // ...
  
  // New field
  ExplorationCubes (0-10)
```

- Effect: Reveal hidden paths at thresholds, reduce stamina costs
- Scope: Applies ONLY to this specific route
- Grant: Successful route completion (+1), scouting goals (+2)
- Verisimilitude: Knowing creek crossings doesn't help manor route

### Cube Application Examples

**Mill Mental Challenge:**
- Base Exposure: 5
- Mill InvestigationCubes: 4
- Effective Exposure: 1 (5 - 4)
- Result: First Mill investigation hard, fifth Mill investigation trivial

**Forest Mental Challenge:**
- Base Exposure: 5
- Forest InvestigationCubes: 0 (haven't been there)
- Mill cubes don't apply (different location)
- Effective Exposure: 5 (full difficulty)
- Result: Each location requires separate mastery

**Elena Social Challenge:**
- Base Doubt: 5
- Elena StoryCubes: 3
- Effective Doubt: 2 (5 - 3)
- Martha StoryCubes: 0 (different NPC)
- Martha challenge: Full Doubt 5 (Elena relationship doesn't help)

## GoalCard Reward Extensions

### Existing V2 Rewards Preserved

- GrantCoins (5-50)
- ModifyProperty (reduce obstacle properties)
- GrantCubes (Investigation/Story/Exploration)

### New Reward Types

**CreateObligation (NPCCommissioned):**
```
GoalCard Reward:
  Type: CreateObligation
  PatronNpcId: "elena"
  DestinationLocationId: "mill"
  RequiredGoalIds: ["deliver_letter"]
  DeadlineSegment: 8
  RewardCoins: 25
  RewardStoryTokens: 2
```

Usage: Social GoalCard creates timed obligation with coin reward.

**SpawnObligation (SelfDiscovered):**
```
GoalCard Reward:
  Type: SpawnObligation
  NewObligationId: "forest_ruins_investigation"
  TargetLocations: ["forest_clearing", "ruins_exterior"]
```

Usage: Mental/Social GoalCard spawns new investigation.

**GrantEquipment:**
```
GoalCard Reward:
  Type: GrantEquipment
  EquipmentId: "climbing_gear"
```

Usage: Completing difficult obligation grants equipment directly.

**UnlockRouteSegment:**
```
GoalCard Reward:
  Type: UnlockRouteSegment
  RouteId: "manor_route"
  SegmentPosition: 2
  PathId: "hidden_servant_entrance"
```

Usage: Reveal hidden optimal path in route.

## How Systems Interconnect

### Economic Loop (Primary Progression)

```
Complete NPCCommissioned obligation
  ↓
Earn 25 coins (patron reward)
  ↓
Buy rope (10 coins) + waders (12 coins) = 3 coins remaining
  ↓
Creek Route obstacles: Water+Endurance/2 → waders reduce to 1
  ↓
Affordable route to Forest (2 Health cost instead of 4)
  ↓
Complete Forest investigation
  ↓
Earn climbing gear + spawn Manor investigation
  ↓
Manor Route accessible (climbing gear makes cliffs safe)
  ↓
Repeat at higher tier
```

**Verisimilitude:** Earning money → buying tools → overcoming physical hazards → reaching destinations → discovering mysteries. Transparent cause-effect chain.

### Power Curve (Background)

```
Play cards in challenges
  ↓
Earn XP = card depth automatically
  ↓
Stats increase (Insight 1 → 2 at 8 XP)
  ↓
Access higher depth cards (now can play Depth 3)
  ↓
Higher depth cards more powerful (+2 Momentum instead of +1)
  ↓
Challenges complete faster (5 actions instead of 8)
  ↓
Spend fewer resources (3 Focus instead of 6)
  ↓
Afford more goals per day
  ↓
Progression accelerates
```

**Invisible safety net:** Stats prevent soft-locking. Early content always completable, just slower without equipment. High stats compensate for lack of equipment.

### Context Mastery (Local Efficiency)

```
Complete Mental goal at Mill
  ↓
Earn +2 Mill InvestigationCubes
  ↓
Mill Exposure baseline: 5 → 3
  ↓
Future Mill Mental challenges cheaper
  ↓
More efficient Mill investigation
  ↓
Build cubes faster (compound efficiency)
  ↓
Mill becomes trivial (10 cubes = Exposure 0)
```

**Verisimilitude:** Familiarity breeds mastery. First visit difficult, tenth visit easy. Localized expertise, not global power creep.

### Impossible Choices (Strategic Depth)

**Scenario: Player at segment 3, multiple competing priorities**

**Player state:**
- 16 segments remaining
- 40 coins
- Own: Rope (Active)
- Stats: 2 (decent)

**Active NPCCommissioned obligations:**
- Elena: Deliver letter to Mill, deadline segment 8, reward 25 coins
- Martha: Deliver package to Forest, deadline segment 12, reward 40 coins

**Available self-discovered obligations:**
- Mill Mystery: 4 phases, no deadline, rewards climbing gear + spawn Forest Ruins
- Town Market: 3 phases, no deadline, rewards 0 coins but spawn Mill Mystery

**Options:**

**A. Rush both NPC obligations (earn money, maintain relationships):**
- Mill delivery: 2 segments travel + 1 segment goal = 3 segments
- Forest delivery: 3 segments travel + 1 segment goal = 4 segments
- Total: 7 segments
- Result: Both deadlines met, earn 65 coins total
- Trade-off: No investigation progress, content not unlocked

**B. Elena + investigate Mill (unlock content, risk Martha):**
- Elena delivery: 3 segments
- Mill Mystery Phase 1-2: 4 segments
- Total: 7 segments
- Result: Elena met (+25 coins), Martha failed (-3 StoryCubes), Mill partially done
- Trade-off: Relationship damage but content progress

**C. Buy equipment first, prioritize Martha (prepare for Forest):**
- Buy waders (12 coins) for Forest obstacles
- Martha delivery: 4 segments
- Total: 4 segments, 12 segments remaining
- Result: Martha met (+40 coins), Elena failed (-2 StoryCubes), have 68 coins + waders
- Trade-off: Can now afford Forest investigation, lost Elena relationship

**D. Ignore obligations, investigate (maximum content discovery):**
- Mill Mystery full: 8 segments
- Town Market: 6 segments
- Total: 14 segments
- Result: Both obligations failed (-5 StoryCubes total), earn 0 coins
- Earn: Climbing gear, unlock Forest Ruins + Manor investigations
- Trade-off: Relationship damage but content explosion

**No optimal choice.** Every option has genuine costs and genuine benefits. Context determines best path.

### Equipment Specialization (Strategic Purchasing)

**Scenario: Player planning Mill Mystery investigation**

**Review Mill obstacles (from scouting or past failure):**
1. Waterwheel (Mechanical + Height, Intensity 2)
2. Flooded basement (Water + Darkness, Intensity 2)
3. Mill owner (Deception + Authority, Intensity 2)

**Player owns:**
- Rope (Climbing, Height, Securing)
- 35 coins remaining

**Coverage analysis:**
- Waterwheel: Rope helps (Height) but not Mechanical → Partial help
- Flooded basement: Rope doesn't help (no Water or Darkness) → Need waders or lantern
- Mill owner: Rope doesn't help (not social) → Need quality clothing

**Purchase options:**
- Tools (20 coins): Full Waterwheel coverage (Mechanical) → 1 obstacle solved
- Lantern (15 coins): Full Basement coverage (Darkness) + waders separately → 1 obstacle solved
- Waders (12 coins): Partial Basement coverage (Water) + lantern separately → 1 obstacle solved
- Quality clothing (25 coins): Full Mill owner coverage → 1 obstacle solved

**Impossible choice:**
- Can't afford comprehensive coverage (need 57 coins, have 35)
- Must prioritize: Which obstacles accept at full intensity?
- OR: Attempt Mill, fail specific obstacles, return to buy targeted equipment

**Learning pattern:**
1. First attempt: Limited equipment, some obstacles hard
2. Identify: "Flooded basement was hardest, needed waders + lantern"
3. Second attempt: Buy waders, basement now manageable
4. Third attempt: Buy lantern too, basement trivial

**Preparation rewarded:** Each visit more successful, equipment investment pays off.

## Session Structure (16 Segments, 4 Blocks)

### Day Cycle

**Morning (Segments 1-4):**
- Wake: Focus/Stamina tokens flip Spent → Ready (full pools)
- Planning: Review active obligations, deadlines, available goals
- Activities begin

**Midday (Segments 5-8):**
- First deadline checkpoint (urgent obligations)
- Continue activities

**Afternoon (Segments 9-12):**
- Second deadline checkpoint (standard obligations)
- Continue activities

**Evening (Day segments 13-16 = Evening segments 1-4):**
- Final deadline checkpoint (relaxed obligations)
- Evening Segment 4 (day segment 16): Day ends automatically

**All blocks equal:** No special mechanics. Just temporal landmarks for player orientation ("deliver by midday" = Midday segment 4 / day segment 8).

**Note:** Internally, TimeState uses RELATIVE segments (1-4 within each block), not absolute day positions. This document uses absolute day segments (1-16) for conceptual clarity when describing full-day flow.

### Day End Processing

**Segment 16 reached:**

1. **Obligation deadline checks:**
   - Query ActiveObligations: deadline ≤ 16?
   - Incomplete obligations: Remove StoryCubes from patron, remove obligation
   - Example: Elena deadline 8, now segment 16, incomplete → Remove 2 StoryCubes from Elena

2. **Sleep recovery:**
   - All Focus tokens: Spent → Ready
   - All Stamina tokens: Spent → Ready
   - Health: No auto-recovery (requires rest goals or items)

3. **Session evaluation screen:**
   - Display obligations met/failed
   - Display coins earned/spent (net change)
   - Display new content discovered
   - Display stats increased
   - Display equipment acquired

4. **Persistence:**
   - Coins (cumulative)
   - Equipment (owned, current state)
   - InvestigationCubes (per location)
   - StoryCubes (per NPC)
   - ExplorationCubes (per route)
   - Stats and XP (cumulative)
   - Active obligations (carry forward)
   - Failed obligation consequences (StoryCube losses persist)

### Multi-Session Progression Example

**Session 1 (New game):**
- Start: Town, 0 coins, no equipment, Stats 1
- Accept Elena obligation (deadline 8)
- Complete Town Market investigation (3 phases, 6 segments)
- Earn: 40 coins, spawn Mill Mystery
- Complete Elena delivery (3 segments)
- Earn: 25 coins, +2 Elena StoryCubes
- End: 65 coins, Stats 1→2 (from card XP), Mill Mystery available

**Session 2:**
- Start: 65 coins, Stats 2, Mill Mystery available
- Buy rope (10 coins) + waders (12 coins) = 43 coins remaining
- Begin Mill Mystery (4 phases across 8 segments)
- Complete Phase 1-3 (6 segments)
- Phase 4 incomplete (ran out of time)
- Earn: +4 Mill InvestigationCubes (from phases 1-3)
- End: 43 coins, Stats 2→3, Mill Mystery partial

**Session 3:**
- Start: 43 coins, Stats 3, Mill InvestigationCubes 4
- Mill challenges easier (Exposure 5→1 from cubes)
- Complete Mill Mystery Phase 4 (2 segments)
- Earn: Climbing gear (free), spawn Forest Ruins, +30 coins (patron reward)
- Total: 73 coins, climbing gear, Forest accessible
- Buy lantern (15 coins) = 58 coins remaining
- Begin Forest Ruins (2 segments into Phase 1)
- End: 58 coins, Stats 3, Forest Ruins partial

**Session 4:**
- Start: 58 coins, Stats 3, Forest Ruins partial, comprehensive equipment
- Complete Forest Ruins (4 phases, 8 segments)
- Earn: 120 coins (patron reward), spawn Manor investigation
- Total: 178 coins (economic pressure removed)
- Manor route accessible (climbing gear makes PhysicalDanger 3 obstacles safe)
- Begin Manor investigation (late-game content)

**Each session builds on previous. No resets. Persistent world state.**

## Soft-Lock Prevention Mechanisms

### 1. Work Goals (Grindable Safety Net)

**Always available at Town location:**
```
Goal: "Chop Wood"
  Time: 2 segments
  Focus: 0
  Reward: 5 coins
  Repeatable: Yes
  
Goal: "Deliver Messages"
  Time: 3 segments
  Stamina: 1
  Reward: 8 coins
  Repeatable: Yes
```

**Worst-case earning:** 16 segments ÷ 2 × 5 coins = 40 coins per day minimum.

**Purpose:** Inefficient but guaranteed income. Player stuck with 0 coins can grind 2-3 sessions for comprehensive equipment.

### 2. Equipment Sell-Back (Liquidity)

**Sell mechanism:**
- Any equipment: 50% return value
- Rope (10 coins) → sell for 5 coins
- Climbing gear (30 coins) → sell for 15 coins

**Purpose:** Bought wrong equipment for situation? Sell, rebuy correct equipment. Loss (50%) creates friction without hard-blocking.

### 3. Multiple Resolution Paths

**Every obstacle offers:**
1. Challenge: Tactical engagement (skill-based)
2. Equipment: Apply owned items (context-specific)
3. Cost: Pay coins to bypass (economic)
4. Damage: Accept Health loss, continue (resource trade)

**Example - Cliff obstacle (PhysicalDanger 3):**
- Challenge: Launch Physical challenge (risk 3 Health on failure)
- Climbing gear: Reduce to 1 (risk 1 Health on failure)
- Pay 15 coins: Hire guide, auto-success
- Accept: Lose 3 Health immediately, proceed

**Player always has options.** Might be expensive (half your Health), but never blocked.

### 4. Stats Compensate for Equipment

**Invisible safety:**
- Stats 1, no equipment: Mental challenge costs 6 Focus (full pool), 8 actions
- Stats 4, no equipment: Same challenge costs 3 Focus, 4 actions
- Stats 6, no equipment: Same challenge costs 2 Focus, 3 actions

**High stats enable progress without equipment.** Slower (more actions, more Focus), but possible.

**Worst-case scenario:**
- Player: 0 coins, all equipment exhausted, Stats 1
- Solution path:
  1. Grind work goals 2-3 sessions (80-120 coins total)
  2. Repair all equipment (20-30 coins)
  3. OR complete low-tier challenges repeatedly to build Stats 1→3
  4. Higher stats make challenges cheaper, progress becomes affordable

**Inefficient, frustrating, but never impossible.**

## Implementation Strategy

### Phase 1: Value Scaling (Content Changes Only)

**No entity changes, only value adjustments:**
- Time costs: All ÷ 1.5 (round to whole)
- Resource pools: All ÷ 17 (6-point scale)
- Obstacle properties: Recalculate to 1-3 intensity
- Equipment effects: Recalculate to -1 or -2
- Challenge thresholds: All ÷ 2
- Card power: Author new values for scaled thresholds
- Coin values: Scale to equipment economy

**Testing:** All V2 challenges still function, just with scaled numbers. No architectural changes yet.

### Phase 2: Entity Extensions

**Add new fields to existing entities:**
```
Obligation (formerly Investigation):
  + ObligationType enum
  + PatronNpcId
  + DeadlineSegment
  + CompletionRewardCoins
  + CompletionRewardItems
  + SpawnedObligationIds

Obstacle:
  + Contexts: List<string>
  - Remove: PhysicalDanger/MentalComplexity/SocialDifficulty
  + Intensity (1-3)

Route:
  + Segments: List<RouteSegment>
  + ExplorationCubes

Location:
  + InvestigationCubes

NPC:
  + StoryCubes
  + AvailableEquipment: List<string>
```

**Testing:** Existing content works with new fields (populated with defaults).

### Phase 3: New Entities

**Create new entities:**
```
RouteSegment:
  Position
  PathCollectionId (references PathCardCollection)
  Description
  SegmentType (FixedPath, Event, Encounter)

PathCard (enhanced with travel properties):
  TravelTimeSegments
  StaminaCost
  ObstacleId
  Name, NarrativeText
  ExplorationThreshold (replaces HiddenUntilExploration)
  HungerEffect, HealthEffect, StaminaRestore
  CoinReward, TokenGains

Equipment:
  Id, Name, CoinCost
  ApplicableContexts
  IntensityReduction
  UsageType, ExhaustAfterUses, RepairCost
  CurrentState

PlayerState extensions:
  + OwnedEquipment: List<Equipment>
  + ActiveObligations: List<Obligation>
```

**Testing:** Equipment can be purchased, route segments can be navigated.

### Phase 4: Resolution Logic

**Equipment-obstacle matching:**
- When obstacle presented, query player equipment
- Calculate: Equipment.ApplicableContexts ∩ Obstacle.Contexts
- Display applicable equipment with reduction preview
- Player chooses which equipment to use
- Apply reduction, exhaust equipment

**Route navigation:**
- Display segment with available paths
- Filter paths: HiddenUntilExploration ≤ Route.ExplorationCubes
- Player chooses path
- Deduct time/stamina
- Present obstacle (if exists) with resolution options
- Advance to next segment

**Testing:** Equipment reduces correct obstacles, route choices affect costs.

### Phase 5: GoalCard Reward Types

**Add new reward type handlers:**
- CreateObligation: Add to ActiveObligations with deadline
- SpawnObligation: Add to ActiveObligations without deadline
- GrantEquipment: Add to PlayerState.OwnedEquipment
- UnlockRouteSegment: Set path visibility

**Testing:** Playing GoalCards creates obligations, grants equipment.

### Phase 6: Obligation Lifecycle

**Day-end processing:**
- Check all ActiveObligations
- DeadlineSegment ≤ current segment AND not complete?
- Remove StoryCubes from PatronNpcId
- Remove obligation from active list

**Obligation completion:**
- Goal completion → check ActiveObligations
- If RequiredGoalIds satisfied → grant rewards
- Remove from active list

**Testing:** Deadlines enforce, failures penalize, completions reward.

### Phase 7: Session Evaluation UI

**Day-end screen:**
- Obligations: Met/Failed list
- Economy: Coins earned/spent, net change
- Progression: New obligations discovered, equipment acquired
- Stats: XP gained, levels increased
- Resources: Current Health/Focus/Stamina

**Testing:** Clear feedback on session outcomes.

### Phase 8: Content Authoring

**Author starter content:**
- Town tier: 3 self-discovered obligations, 2 NPC obligations, work goals
- Equipment catalog: 6-8 items with context tags
- Routes: Town→Mill→Forest with segments and path choices
- Context tags: Assign to all obstacles

**Testing:** Complete 1-hour session demonstrating full loop (NPCCommissioned obligation → earn coins → buy equipment → travel route → complete self-discovered obligation → unlock next tier).

## Example: Complete Play Session

### Session Setup

**Player start state:**
- Location: Tavern (home base)
- Segments remaining: 16
- Coins: 0
- Equipment: None
- Stats: All 1 (starting)
- Health: 6/6, Focus: 6/6, Stamina: 6/6

**Available content:**
- Elena (NPC at Tavern): Social challenge available
- Town Market investigation: 3 Mental goals at Town locations
- Work goals: Chop wood (repeatable, 5 coins, 2 segments)

### Morning (Segments 1-4)

**Segment 1: Talk to Elena**
- Launch Social challenge (Momentum threshold 8)
- Elena personality: [Authority, Empathy]
- Cards played: 6 actions using Rapport/Diplomacy cards
- Stats: Rapport 1 (can only use Depth 1-2 cards)
- Result: Reach Momentum 8
- Play GoalCard: "Accept Urgent Request"
  - Reward type: CreateObligation
  - PatronNpcId: "elena"
  - DestinationLocationId: "mill"
  - RequiredGoalIds: ["deliver_letter"]
  - DeadlineSegment: 8
  - RewardCoins: 25
  - RewardStoryTokens: 2
- Obligation added to ActiveObligations
- XP earned: 6 cards × Depth 1-2 average = 9 XP → Rapport 1→2 (threshold 8)
- Time spent: 1 segment
- UI: "Elena: Deliver letter to Mill. Deadline: Segment 8. Reward: 25 coins, 2 Story cubes."

**Segments 2-4: Begin Town Market Investigation**
- Phase 1 Goal: "Examine Market Stalls" (Mental, [Search, Deduction], Focus 2, 1 segment)
- Launch Mental challenge (Progress threshold 6)
- Cards played: 4 actions using Insight cards
- Result: Reach Progress 6
- Play GoalCard: "Find Hidden Ledger"
  - Rewards: +10 coins, +2 Town InvestigationCubes
- Time spent: 1 segment
- Coins: 10
- Town InvestigationCubes: 2

**Segment 4: Second Town Market Goal**
- "Search Chapel Records" (Mental, [Memory, Search], Focus 2, 1 segment)
- Town InvestigationCubes 2 → Exposure baseline reduced (5→3)
- Challenge easier due to familiarity
- Result: Complete, earn +10 coins
- Coins: 20

**Morning end: Segment 4 complete, 12 segments remaining**

### Midday (Segments 5-8)

**Segment 5: Third Town Market Goal**
- "Interview Baker" (Social, [Negotiation], Initiative pool, 1 segment)
- Result: Complete Town Market investigation
- Rewards: +20 coins (patron reward), spawn Mill Mystery
- Coins: 40
- New obligation: "Mill Mystery" (SelfDiscovered, no deadline)

**Segment 5-6: Equipment Shopping**
- View available equipment at Town vendor
- Coins: 40
- Options: Rope (10), Waders (12), Lantern (15), Tools (20)
- Decision: Buy rope (helps Climbing/Height) + rations (emergency)
- Purchase: Rope (10) + Rations (5) = 15 spent
- Coins: 25
- Time: 0 (shopping doesn't consume segments)

**Segment 6-8: Travel to Mill (Creek Route)**
- Route: 2 segments total (if good choices)
- ExplorationCubes: 0 (first time, no hidden paths)

**Segment 1: Forest Approach**
- Path A: Direct trail (1 segment, 1 stamina, no obstacle)
- Path B: Bramble shortcut (1 segment, 0 stamina, Physical/Precision/1)
- Choose Path A (safe)
- Time: 1 segment, Stamina: 5/6 remaining

**Segment 2: Creek Crossing**
- Path A: Shallow ford (1 segment, 2 stamina, Physical/Water+Endurance/2)
  - Waders would reduce to 1 (don't own)
  - Rope doesn't help (no Climbing context)
- Path B: Rope bridge (2 segments, 1 stamina, Physical/Precision+Height/2)
  - Rope reduces to 1 (Height match!)
- Choose Path B (rope helps, worth extra time)
- Time: 2 segments (arrive at segment 8)
- Stamina: 4/6 remaining

**Arrive at Mill: Segment 8 (deadline met for Elena!)**

**Elena obligation deadline 8 reached:**
- Goal "deliver_letter" at Mill not yet complete
- But current segment = deadline, not past yet
- Continue to complete delivery

### Afternoon (Segments 9-12)

**Segment 9: Complete Elena Delivery**
- Goal: "Deliver Elena's Letter" (Mill location, Mental, [Search], Focus 1, 1 segment)
- Simple goal (just delivering)
- Complete, obligation fulfilled
- Rewards: +25 coins, +2 Elena StoryCubes
- Coins: 50
- Elena StoryCubes: 2 (relationship strengthened)

**Segment 9-12: Begin Mill Mystery (Phase 1)**
- Mill Mystery Phase 1: "Examine Waterwheel" (Mill exterior)
- Obstacle: (Mechanical + Height, Intensity 2)
- Equipment analysis:
  - Rope: Matches [Height] → Reduces 2→1
  - Want tools for [Mechanical] but don't own
- Mental challenge (Progress 6, Focus 3 base → 2 with rope effect)
- Launch challenge
- Cards played: 5 actions (Stats now 2, access Depth 3 cards)
- Focus spent: 2
- Result: Complete
- Rewards: +10 coins, +2 Mill InvestigationCubes
- Time: 2 segments
- Coins: 60
- Mill InvestigationCubes: 2

**Segment 12: Rope Exhausted**
- Rope used 2 times (Elena route bridge + Waterwheel)
- State: Exhausted
- Need repair (3 coins) to use again

**Afternoon end: Segment 12, 4 segments remaining**

### Evening (Day segments 13-16 = Evening segments 1-4)

**Decision point: Continue Mill Mystery or return to Town?**
- Mill Mystery Phase 2: "Search Flooded Basement" (Water+Darkness, Intensity 2)
- No waders (Water), no lantern (Darkness) → Would face full Intensity 2
- Option: Accept 2 Focus cost (have 4 Focus remaining) OR wait

**Segment 13: Attempt Flooded Basement (risky)**
- Launch Mental challenge without equipment
- Base Focus cost: 2
- Mill InvestigationCubes 2 → Exposure reduced (5→3)
- Challenge manageable due to location familiarity
- Complete: +10 coins, +2 Mill InvestigationCubes
- Focus: 2/6 remaining
- Coins: 70

**Segment 14-15: Mill Mystery Phase 3**
- "Interview Mill Owner" (Social, [Deception+Authority], Intensity 2)
- Elena StoryCubes don't help (different NPC)
- Mill Owner StoryCubes: 0
- Baseline Doubt: 5
- Challenge harder
- Complete but costly: Initiative expenditure high
- Rewards: +10 coins, +1 Mill Owner StoryCubes
- Time: 2 segments
- Coins: 80

**Segment 16: Day Ends**
- Cannot complete Phase 4 (need more segments)
- Mill Mystery partial (3/4 phases)

### Day End Processing

**Obligation checks:**
- Elena obligation: Complete ✓
- No other obligations

**Sleep recovery:**
- Focus: 2→6 (fully restored)
- Stamina: 4→6 (fully restored)
- Health: 6/6 (no damage taken)

**Session Evaluation Screen:**

```
SESSION COMPLETE

Obligations:
✓ Elena Delivery - Completed (+25 coins, +2 Story cubes)

Economy:
Earned: 80 coins
Spent: 15 coins (equipment)
Net: +65 coins

Progression:
- Mill Mystery discovered (3/4 phases complete)
- Equipment acquired: Rope (exhausted), Rations (unused)
- Stats: Rapport 1→2 (now access Depth 3 cards)
- Cubes: Town Investigation +2, Mill Investigation +4, Elena Story +2, Mill Owner Story +1

Resources:
Health: 6/6
Focus: 6/6 (restored)
Stamina: 6/6 (restored)
Coins: 65

Next Session:
- Complete Mill Mystery Phase 4 for patron reward + spawn Forest investigation
- Consider buying waders (12) + lantern (15) for better equipment coverage
- Creek Route ExplorationCubes: 1 (traveled once, slightly familiar)
```

### Session Analysis

**Strategic decisions made:**
- Accepted Elena obligation (economic choice for coins)
- Completed Town Market first (coin generation before equipment purchase)
- Bought rope + rations (specific contexts, not comprehensive coverage)
- Chose rope bridge path (equipment synergy)
- Attempted flooded basement without equipment (risky but successful)

**Impossible choices faced:**
- Buy rope OR waders OR save coins? (chose rope for Elena route)
- Complete Town Market OR rush Elena delivery? (chose Market for coins first)
- Continue Mill Mystery OR return to Town? (continued, paid resource cost)

**Learning outcomes:**
- Flooded basement hard without waders/lantern (identified equipment gap)
- Mill familiarity helped (InvestigationCubes reduced difficulty)
- Need to repair rope before next use (resource management)

**Next session goals:**
- Complete Mill Mystery Phase 4 (earn patron reward, spawn content)
- Buy waders + lantern with accumulated coins
- Attempt Forest investigation (if spawned)

## Design Validation Against Principles

### Principle 1: Single Source of Truth
✅ GameWorld owns Obligations/Obstacles/Goals via flat lists
✅ Obligations own Obstacles (lifecycle)
✅ Locations/NPCs reference via IDs (placement)

### Principle 2: Strong Typing
✅ List<Obligation>, List<Equipment>, List<RouteSegment>
✅ ObligationType enum (NPCCommissioned | SelfDiscovered)
✅ No dictionaries of generic types

### Principle 3: Ownership vs Placement
✅ Obligations own Obstacles (lifecycle control)
✅ Goals appear at Locations (placement metadata)
✅ Clear distinction maintained

### Principle 4: Inter-Systemic Rules (No Boolean Gates)
✅ Economic affordability (coins → equipment → survival)
✅ Time scarcity (16 segments, competing obligations)
✅ Resource competition (Focus/Stamina/Health pools)
✅ NO boolean unlocking anywhere

### Principle 5: Typed Rewards
✅ CreateObligation, SpawnObligation, GrantEquipment, GrantCoins
✅ Applied at completion (not continuous evaluation)
✅ Explicit system connections

### Principle 6: Resource Scarcity
✅ 16 segments (limited, must prioritize)
✅ 6-point pools (every point matters)
✅ Coins scarce early (can't buy all equipment)
✅ Impossible choices emerge naturally

### Principle 7: One Purpose Per Entity
✅ Obligations: Structure mysteries or commissioned work
✅ Obstacles: Contain goals with context tags
✅ Goals: Define challenges
✅ Equipment: Reduce specific context intensities
✅ Routes: Provide segment-by-segment navigation

### Principle 8: Verisimilitude
✅ Routes are physical geography
✅ Equipment has specific applicability (context tags)
✅ Knowledge is location/NPC/route specific
✅ Coins from patrons (commissioned work pays)
✅ Time as hours (4 blocks = 4-hour periods)

### Principle 9: Minimal Interconnection
✅ Obligations → Obstacles (ownership)
✅ Goals → Challenges (launching)
✅ Equipment → Obstacles (context-specific reduction)
✅ One connection per system boundary

### Principle 10: Perfect Information
✅ All costs visible before commitment
✅ Equipment applicability shown (context matching)
✅ Deadlines explicit (segment numbers)
✅ Rewards displayed (typed, specific)
✅ Strategic layer transparent, tactical layer hidden until engaged

## Conclusion

This refactoring preserves V2's architecture entirely while adding transparent economic progression through:

1. **Scaled resources:** Small numbers (6 Health, 16 segments) with clear proportional meaning
2. **Obligation types:** NPCCommissioned (deadlines, coins) vs SelfDiscovered (exploration, items)
3. **Context specificity:** Equipment and knowledge apply to defined contexts (verisimilitude)
4. **Route segments:** Path choices at each segment with obstacle resolution options
5. **Impossible choices:** Resource scarcity + competing priorities = strategic depth

**No entities removed. No systems replaced. Only extended and refined.**

The game loop: Complete obligations → Earn coins → Buy equipment → Survive routes → Access locations → Discover obligations → Repeat at higher tier.

Everything physically exists from start. Progression is affordability, not unlocking.