# Wayfarer Dynamic Content System: Design Document

## CRITICAL ARCHITECTURE CLARIFICATION

**READ THIS FIRST:** The corrected architecture is documented in [TEMPLATE_ARCHITECTURE.md](./TEMPLATE_ARCHITECTURE.md). This document describes the overall design vision, but specific entity details should reference the template architecture document.

**KEY CORRECTIONS:**
- **Scene** = Ephemeral spawning orchestrator (creates multiple Situations in configurations)
- **Situation** = Persistent narrative context containing 2-4 **action references** (LocationAction/ConversationOption/TravelCard IDs)
- **Actions** = Existing entities that Situations reference, NOT inline definitions
- **SituationCard** = TACTICAL layer only (victory conditions inside challenges), NOT part of Scene/Situation architecture

**This document's references to "Goals" spawned by Situations should be understood as:**
- Situations contain action references
- Actions may trigger challenges OR execute instantly (Sir Brante pattern)
- When action triggers challenge, that's what creates the Goal entity (tactical layer)

---

## Executive Summary

Wayfarer is pivoting from a static content model to a dynamic consequence-driven system inspired by "The Life and Suffering of Sir Brante." This document defines a complete architectural redesign where pre-authored Scene templates orchestrate Situation spawning, creating emergent narratives through mechanical consequence chains while preserving the strategic depth and verisimilitude of the original design.

**Core Innovation:** Separate mechanical structure (templates with spawn rules) from narrative flesh (AI-generated contextual descriptions). Content spawns deterministically from player choices, with AI providing coherent narrative wrapping at encounter time.

**Key Achievement:** Avoid boolean gate anti-pattern by using accumulative numerical resources (stats, Bonds, Scales, Resolve) and compound OR requirements, ensuring multiple valid paths to content access.

---

## Problem Statement and Motivation

### Current Wayfarer Architecture Limitations

**Static Content Model:**
- All Investigations, Obstacles, Goals authored in JSON at design time
- Content exists whether player interacts or not
- Limited branching (pre-authored paths only)
- Finite content pool creates replay limitations
- No response to emergent player behavior patterns

**Limited Consequence Depth:**
- Investigations spawn Obstacles at Locations
- Goals reduce Obstacle properties
- Obstacles removed when properties reach zero
- Consequence chain depth: 2 levels (Investigation → Obstacle → removal)
- No long-term narrative threads from player choices

**Missing Sir Brante Elements:**
- No persistent character identity (behavioral reputation)
- No cascading consequence chains (choice today spawns content tomorrow)
- No multiple-path access (same content gated same way for all builds)
- No accumulative progression feel (just economic access gating)
- Limited relationship consequence (just numerical cubes, no narrative depth)

### What Sir Brante Does Right

**Accumulative Stat System:**
- Stats as thresholds (check value, don't consume)
- Willpower as consumable (universal scarcity resource)
- Multiple stats satisfy same requirement (OR logic)
- Choices shift stats incrementally (build over time)
- Emergent character builds from accumulation patterns

**Visible Gating with Alternative Paths:**
- All choices visible even when locked
- Requirements shown explicitly
- Multiple stat combinations unlock same content
- Different builds reach same content differently
- Perfect information enables strategic planning

**Destiny Registry (Milestone Markers):**
- Major choices grant visible Achievements
- Later content checks Achievement presence
- Creates narrative coherence (references past decisions)
- Not boolean flags (integrated as compound requirements with stat alternatives)
- Player identity emerges from Achievement collection

**Consequence Spawning:**
- Choice A spawns Choice B with calculated requirements
- Spawn chains create 5+ level narrative depth
- Content doesn't exist until earned through play
- Mechanical chains authored, narrative details procedural
- Creates organic story progression

### Why This Matters for Wayfarer

**Replayability Crisis:**
- Static content exhausted after 1-2 playthroughs
- No response to different player approaches
- Same Investigations available same way always
- Limited narrative variety despite mechanical depth

**Shallow Consequence Problem:**
- Completing Goal has immediate local effect only
- No ripples through world state
- No long-term narrative threads
- Player choices feel disconnected from world evolution

**Identity Gap:**
- Player has stats and cubes but no reputation
- World treats player identically regardless of past actions
- No behavioral specialization beyond stat distribution
- No emerging character arc

**Solution:** Implement Sir Brante's accumulative progression and consequence spawning while maintaining Wayfarer's five-system challenge architecture and resource competition design.

---

## Core Architectural Changes

### From Static Content to Template Instantiation

**Old Model:**
```
Design time: Author 50 Investigations with all phases
Runtime: Player discovers pre-existing Investigations
Content: Fixed, finite, identical across playthroughs
```

**New Model:**
```
Design time: Author 50-100 Situation templates with spawn rules
Runtime: Player choices spawn Situation instances with calculated requirements
Content: Generated, cascading, unique per playthrough based on choices
```

### From Simple Spawning to Cascade Architecture

**Old Model:**
```
Investigation → spawns Obstacle → spawns Goals → player completes Goal → Obstacle removed
Depth: 2 levels
Branching: None (linear sequence)
Reuse: Each Investigation used once
```

**New Model:**
```
Situation_A → spawns Situation_B (tier +1) → spawns Situation_C (tier +1) → continues
Depth: 4+ levels with branching
Branching: Multiple spawn paths based on player state
Reuse: Templates spawn many instances across playthrough
```

### From Economic Gating to Multi-Axis Gating

**Old Model:**
```
Content gated by:
- Coins (can afford equipment)
- Stats (can attempt challenge)
- Completed phases (linear progression)
```

**New Model:**
```
Content gated by:
- Stats (capability thresholds)
- Bonds (relationship depth)
- Scales (behavioral reputation)
- States (temporary conditions)
- Achievements (milestone markers)
- Resolve (universal scarcity)
- Economic resources (Coins, Items)
- Compound OR logic (multiple valid paths)
```

---

## Entity Architecture and Relationships

### New Core Entities

**Situation (Template):**
- Archetype: SituationArchetype enum (Rescue, Betrayal, Alliance, Crisis, Discovery, Trade, Obligation, Rivalry, Romance, Conflict)
- Tier: int (1-4)
- Requirements: List of RequirementRule objects (typed structures, not formulas)
- SpawnRules: List of SpawnRule objects (typed references to templates)
- InteractionType: InteractionType enum (LocationPhysical, LocationMental, NpcSocial, RouteTravel)
- NarrativeHints: string (for AI consumption only, not used in game logic)

**Situation (Instance):**
- TemplateReference: SituationTemplate (typed reference, not string ID)
- CalculatedRequirements: List of CalculatedRequirement objects (frozen values at spawn time)
- TargetNpc: NPC typed reference (null if Location-based)
- TargetLocation: Location typed reference (null if NPC-based)
- TargetRoute: Route typed reference (null if not Route-based)
- Status: SituationStatus enum (Dormant, Available, Active, Completed)
- GeneratedNarrative: string (AI output, stored once)
- ParentSituation: Situation typed reference (what spawned this, null if root)

**Achievement:**
- Milestone marker from significant choices
- Category: AchievementCategory enum (Combat, Social, Investigation, Economic, Political)
- RelatedNpc: NPC typed reference (null if not NPC-related)
- RelatedLocation: Location typed reference (null if not Location-related)
- Created by GoalCard rewards
- Used in Situation requirements
- Visible in player journal

**Scale:**
- ScaleType enum: Morality, Lawfulness, Fame, Method, Transparency, Caution
- Value: int (-10 to +10)
- Modified slowly by GoalCard consequences
- Both extremes valuable (unlock different content)

**State:**
- StateType enum: Wounded, Exhausted, Sick, Injured, Starving, Confused, Traumatized, Inspired, Focused, Obsessed, Wanted, Celebrated, Shunned, Humiliated, Disguised, Indebted, Armed, Equipped, Provisioned, Rested, Prepared (predefined list of ~25)
- Category: StateCategory enum (Physical, Mental, Social)
- Applied by GoalCard consequences or events
- Cleared through specific actions
- Dual nature: blocks some content, enables other content

**Resolve:**
- Universal consumable resource (0-30 range)
- Consumed by demanding choices across all challenge types
- Separate from Focus (Mental), Stamina (Physical), Initiative (Social)
- Restored by rest and low-intensity activities
- Creates scarcity across all interaction types

### Modified Existing Entities

**Player:**
- ADD: List of Achievement typed references
- ADD: List of PlayerScale objects (ScaleType enum + int value pairs)
- ADD: List of StateType enums (active states)
- ADD: Resolve int value (0-30)
- KEEP: All existing (Stats as int 1-20, Focus int, Stamina int, Health int, Coins int, List of Item typed references)

**NPC:**
- ADD: BondStrength int (-10 to +10)
- ADD: List of NpcStatus enums (Grateful, Betrayed, Allied, Hostile, Trusting, Suspicious, Indebted - predefined list)
- ADD: List of Situation typed references (available here)
- KEEP: Story Cubes int (0-10, for tactical challenge benefits)
- KEEP: List of Goal typed references (currently available)
- KEEP: Personality enum (for Social challenge modifiers)

**Location:**
- ADD: List of Situation typed references (available here)
- KEEP: Investigation Cubes int (familiarity 0-10)
- KEEP: List of Goal typed references (currently available)
- KEEP: List of Obstacle typed references (obstacles present here)

**Goal:**
- ADD: SpawnedBySituation: Situation typed reference (tracking provenance)
- ADD: ResolveCost int (universal resource cost)
- EXPAND: Requirements object with typed fields:
  - StatThresholds: List of StatRequirement objects (StatType enum + int threshold)
  - ScaleThresholds: List of ScaleRequirement objects (ScaleType enum + int threshold + ComparisonOperator enum)
  - RequiredStates: List of StateType enums
  - BlockedByStates: List of StateType enums  
  - RequiredAchievements: List of Achievement typed references
  - MinimumBond: int (with target NPC, null if not applicable)
- KEEP: All existing (placement references, challenge type, victory conditions)

**GoalCard:**
- ADD: Rewards structure with typed objects:
  - AchievementGrant: AchievementTemplate typed reference (template to instantiate)
  - BondChanges: List of BondChange objects:
    - TargetNpc: NPC typed reference
    - Amount: int (can be negative)
  - ScaleShifts: List of ScaleShift objects:
    - Scale: ScaleType enum
    - Amount: int (typically +1 to +3, can be negative)
  - StateApplications: List of StateApplication objects:
    - State: StateType enum
    - Apply: bool (true = add state, false = remove state)
  - StatusApplications: List of StatusApplication objects:
    - TargetNpc: NPC typed reference
    - Status: NpcStatus enum
    - Apply: bool (true = add status, false = remove status)
  - SituationSpawns: List of SituationSpawnTrigger objects:
    - TargetTemplate: SituationTemplate typed reference
    - RequirementOffsets: List of RequirementOffset objects (how to modify base template requirements)
    - TargetNpc: NPC typed reference (null if Location-based)
    - TargetLocation: Location typed reference (null if NPC-based)
  - ResolveRestore: int (amount to restore)
- KEEP: All existing (Coins int, List of Item typed references, ObstaclePropertyReduction typed object)

**Investigation:**
- CHANGE: Now just one Situation archetype among many
- Can spawn from other Situations (not just initial content)
- Still spawns Obstacles with Goals (existing pattern preserved)
- PatronNpcId references commissioning NPC (verisimilitude)

**Obstacle:**
- NO CHANGES: Still property containers
- Still targeted by Goals
- Still have PhysicalDanger/SocialDifficulty/MentalComplexity
- Still reduced by GoalCard consequences
- Now spawned by Situations instead of just Investigations

**Route PathCards:**
- ADD: Can spawn Situations at destination
- ADD: Can reference Scale/State requirements
- KEEP: All existing travel mechanics

### Ownership Hierarchy

**GameWorld owns everything:**
```
GameWorld
├── Situations (all instances, DORMANT through COMPLETED)
├── Goals (all instances, active and completed)
├── Obstacles (all instances, active and resolved)
├── Investigations (now just special Situation type)
├── Achievements (all earned milestones)
├── NPCs (all characters)
├── Locations (all places)
└── Routes (all paths)
```

**Reference patterns (all strongly typed, no string IDs):**
```
Location.AvailableSituations → List<Situation> typed references
Location.ActiveGoals → List<Goal> typed references
Location.Obstacles → List<Obstacle> typed references

NPC.AvailableSituations → List<Situation> typed references
NPC.ActiveGoals → List<Goal> typed references

Situation.SpawnedGoals → List<Goal> typed references
Situation.SpawnedObstacles → List<Obstacle> typed references
Situation.SpawnedSituations → List<Situation> typed references (children)

Goal.SpawnedBySituation → Situation typed reference (parent)
Goal.TargetObstacle → Obstacle typed reference (nullable)

Obstacle.Goals → List<Goal> typed references (targeting it)
```

**No string-based lookups:** All references are typed. GameWorld owns entities in dictionaries for lifecycle management, but entities reference each other directly through typed references, not string IDs.

---

## Content Generation Flow

### Phase 1: Template Authoring (Design Time)

**What Happens:**
- Designer creates 50-100 Situation templates
- Each template defines:
  - Archetype: SituationArchetype enum value
  - Requirements: List of RequirementRule typed objects
  - SpawnRules: List of SpawnRule typed objects  
  - ConsequenceDefinitions: Embedded in GoalCard reward structures
  - InteractionType: InteractionType enum (determines where it spawns Goals)
  - Tier: int (1-4 based on complexity)

**Example Template Structure:**
```
Template: "Desperate Rescue" (typed object, not string-based)
Archetype: SituationArchetype.Rescue
Tier: 2
Requirements: List<RequirementRule>
  - RequirementRule {
      Type: RequirementType.StatThreshold
      Stat: StatType.Authority
      BaseValue: 0 (current player value used)
      Offset: 3 (requires current + 3)
      Operator: ComparisonOperator.GreaterEqual
    }
  - RequirementRule {
      Type: RequirementType.ResourceMinimum
      ResourceType: ResourceType.Resolve
      Value: 8
      Operator: ComparisonOperator.GreaterEqual
    }
  - RequirementRule {
      Type: RequirementType.StateAbsence
      State: StateType.Exhausted
    }

SpawnRules: List<SpawnRule>
  - SpawnRule {
      TargetTemplate: SituationTemplate typed reference to "Grateful Family"
      Conditions: List<SpawnCondition>
        - SpawnCondition {
            Type: ConditionType.BondThreshold
            TargetNpcFromParent: true (use beneficiary from parent Situation)
            Threshold: 5
            Operator: ComparisonOperator.GreaterEqual
          }
    }
  - SpawnRule {
      TargetTemplate: SituationTemplate typed reference to "Obligation Repayment"
      Conditions: List<SpawnCondition>
        - SpawnCondition {
            Type: ConditionType.BondThreshold
            TargetNpcFromParent: true
            Threshold: 5
            Operator: ComparisonOperator.Less
          }
    }

InteractionType: InteractionType.LocationPhysical
GoalDefinition: (what Goal to spawn)
  - TargetObstacleArchetype: ObstacleArchetype typed reference
  - ObstacleProperties: { PhysicalDanger: 2 }
  - ResolveCost: 8
  - GoalCards: List<GoalCardTemplate>
      - Threshold: 12
      - Rewards:
          - BondChange { TargetNpcFromParent: true, Amount: 3 }
          - AchievementGrant: AchievementTemplate reference
          - ScaleShift { Scale: ScaleType.Morality, Amount: 2 }
          - SituationSpawns: (references SpawnRules above)

NarrativeHints: "Urgent tone, life-threatening danger, heroic sacrifice theme" (string for AI only)
```

**No string parsing:** All formulas are typed objects. Offsets are int values. References are typed. No magic strings anywhere.

---

### Phase 2: Instance Spawning (Choice Completion Time)

**What Happens:**
- Player completes GoalCard containing SituationSpawnTrigger in rewards
- System retrieves TargetTemplate typed reference
- System instantiates new Situation from template
- **Requirements calculated using typed structures:**
  - Template has: RequirementRule { Stat: Authority, Offset: 3 }
  - Player has: Authority 8
  - Instance creates: CalculatedRequirement { Stat: Authority, Threshold: 11, Operator: GreaterEqual }
- Entity references stored as typed references
- Status set to: SituationStatus.Dormant
- Added to GameWorld.Situations dictionary

**Example Instance (all typed, no strings):**
```
Instance: (Situation typed object)
  TemplateReference: (SituationTemplate typed reference to "Desperate Rescue")
  Status: SituationStatus.Dormant
  CalculatedRequirements: List<CalculatedRequirement>
    - CalculatedRequirement {
        Type: RequirementType.StatThreshold
        Stat: StatType.Authority
        Threshold: 11 (was current 8 + offset 3)
        Operator: ComparisonOperator.GreaterEqual
      }
    - CalculatedRequirement {
        Type: RequirementType.ResourceMinimum
        ResourceType: ResourceType.Resolve
        Threshold: 8
        Operator: ComparisonOperator.GreaterEqual
      }
    - CalculatedRequirement {
        Type: RequirementType.StateAbsence
        State: StateType.Exhausted
      }
  TargetNpc: (NPC typed reference to "npc_child_003")
  TargetLocation: (Location typed reference to "loc_burning_house")
  ParentSituation: (Situation typed reference to parent)
  GeneratedNarrative: null (not yet generated)
```

**Why Spawn Here:**
- Requirements frozen as typed objects (deterministic)
- Player knows exact thresholds to reach (perfect information)
- Situation exists as goal to work toward (planning horizon)
- No runtime formula parsing (all pre-calculated typed values)

---

### Phase 3: Availability Check (Continuous Evaluation)

**What Happens:**
- Every time player stats/resources change, system evaluates DORMANT Situations
- Checks all requirements against current player state
- When satisfied: DORMANT → AVAILABLE
- Situation added to appropriate entity's AvailableSituationIds list
  - If Location-based: Location.AvailableSituationIds
  - If NPC-based: NPC.AvailableSituationIds
  - If Route-based: RouteSegment.AvailableSituationIds

**Example State Change:**
```
Player gains Authority 8 → 11 (from completing different challenge)
System evaluates all DORMANT Situations
Finds sit_rescue_047 requirements now satisfied
Status: DORMANT → AVAILABLE
Added to loc_burning_house.AvailableSituationIds
```

**Why Check Here:**
- Dynamic eligibility (content appears when earned)
- No manual tracking needed (automatic system response)
- Player discovers new options organically
- Creates sense of world reactivity

---

### Phase 4: Narrative Generation (Encounter Time)

**What Happens:**
- Player enters Location or talks to NPC with AVAILABLE Situation
- AI generation triggered
- **AI queries player state for context:**
  - All Achievements (especially those involving referenced entities)
  - BondStrength with involved NPCs
  - Scale values (behavioral reputation)
  - Active States (current conditions)
  - Completed Situations in same category (pattern recognition)
  - Investigation Cubes at Location (familiarity)
  - Story Cubes with NPC (relationship depth)
- **AI generates contextual narrative:**
  - Scene description referencing player history
  - Explains why this Situation available now (requirements)
  - Choice text for Goals that will be spawned
  - Wraps mechanical consequences in story context
- Generated text stored in Situation instance
- Status: AVAILABLE → ACTIVE

**Example Generation:**
```
AI Input Context:
- Player has Achievement "Rescued Miller's Child" (past pattern)
- Player has BondStrength +3 with Child's Father
- Player has Morality 8 (altruistic reputation)
- Player has Authority 11 (commanding presence)
- Location Investigation Cubes: 5 (familiar with area)

AI Generated Narrative:
"Smoke pours from the upper windows of the old manor. Through 
the chaos, you recognize the child who used to play near the 
mill—the one whose father still speaks of your intervention with 
gratitude. The crowd stands paralyzed by fear, but your reputation 
for decisive action has them turning to you. The child's screams 
echo from the third floor. You've faced fire before. You know 
what it costs."

Choice Text:
"Rush into the burning building. Your commanding presence might 
inspire others to help, but the danger is real and immediate. 
This will drain you physically and emotionally."
```

**Why Generate Here:**
- Narrative coherence (references actual player history)
- Contextual appropriateness (matches current game state)
- Prevents pre-authored text feeling generic
- Creates unique story each playthrough

---

### Phase 5: Goal Spawning (Activation Time)

**What Happens:**
- Situation template defines what Goals to spawn
- System creates Goal instances with:
  - Template-defined challenge type
  - Calculated costs and requirements
  - Placement at specific Location or NPC
  - References to Situation parent and target Obstacle (if any)
- Goals added to Location.ActiveGoalIds or NPC.ActiveGoalIds
- Player sees Goals as action buttons in UI
- Situation Status: ACTIVE (waiting for player interaction)

**Example Goal Creation:**
```
From sit_rescue_047 template:
Creates Goal:
  - Name: "Rescue Trapped Child"
  - SystemType: Physical
  - PlacementLocationId: "loc_burning_house"
  - TargetObstacleId: "obs_fire_danger_012" (PhysicalDanger 2)
  - ResolveCost: 8
  - Requirements: (already met, since Situation is ACTIVE)
  - GoalCards: [defined in template, see next phase]
  - SpawnedBySituationId: "sit_rescue_047"

Goal added to loc_burning_house.ActiveGoalIds
Player sees button: "Rescue Trapped Child" in location UI
```

**Why Spawn Here:**
- Player has confirmed interest (entered context)
- Narrative already generated (coherent framing)
- Requirements already met (no false promises)
- Immediate actionable choice

---

### Phase 6: Challenge Execution (Player Choice Time)

**What Happens:**
- Player clicks Goal button
- System validates Resolve cost and requirements (recheck)
- Deducts Resolve
- Launches appropriate challenge (Mental/Physical/Social)
- Player plays through challenge using existing mechanics
- Challenge resolves to GoalCard played or failure

**Example Challenge:**
```
Player clicks "Rescue Trapped Child"
Resolve: 15 → 7 (cost 8)
Physical Challenge launched:
  - Deck: Athletics type (climbing/speed)
  - TargetObstacle.PhysicalDanger: 2
  - Breakthrough threshold: 12
  - Danger threshold: 10
  - Exertion budget from Stamina
  
Player plays challenge...
Reaches Breakthrough 12
Plays GoalCard 2 (threshold 12)
```

**No Changes:** Existing challenge systems work identically. Only difference is Goals spawned dynamically by Situations instead of statically authored.

---

### Phase 7: Consequence Application (Completion Time)

**What Happens:**
- GoalCard consequences applied immediately
- **Immediate effects:**
  - Coins, Items added to inventory
  - Obstacle properties reduced
  - Stats gain XP
  - Context Cubes added (Investigation/Story/Exploration)
- **Relationship effects:**
  - BondStrength modified with NPCs
  - StatusTags applied to NPCs
- **Progression effects:**
  - Achievement created and added to player
  - Scales shifted (Morality, Lawfulness, etc.)
  - States applied or cleared
- **Cascade trigger:**
  - SituationSpawn rewards instantiate new Situations
  - New Situations enter DORMANT status with calculated requirements
  - Cycle begins again

**Example Consequences:**
```
GoalCard 2 Rewards Applied:
- Coins: +15
- Health: -1 (slight injury from smoke)
- Bond with Child's Father: +3 (now 6 total)
- Bond with Rescued Child: created at +5 (gratitude)
- Achievement created: "Saved Child from Fire"
- Morality: +2 (now 10, extremely altruistic)
- Method: -1 (used force to break door, now 7)
- State applied: "Exhausted" (physically draining)
- ObstaclePropertyReduction: PhysicalDanger -2 (fire still burns but victim saved)

SituationSpawn triggered:
- Template "Grateful Family" instantiated
- Requirements calculated:
  - Bond with Father >= 9 (current 6, NOT MET)
  - Morality >= 5 (current 10, MET)
  - Authority >= 8 (current 11, MET)
- Status: DORMANT (waiting for Bond to increase)
- Will become AVAILABLE when Bond reaches 9 with Father
```

**Why Apply Here:**
- Immediate feedback (player sees consequences)
- Mechanical determinism (no hidden calculations)
- Cascade spawning creates future content
- Player planning horizon expands (can see what's coming)

---

### Phase 8: Completion and Recursion

**What Happens:**
- Situation marked COMPLETED
- Goal removed from Location/NPC active lists (if delete-on-success)
- Spawned child Situations now exist (some DORMANT, some AVAILABLE)
- Player can pursue available content or work toward DORMANT content
- System repeats from Phase 3 for new Situations

**Example Completion:**
```
sit_rescue_047 Status: ACTIVE → COMPLETED
Goal "Rescue Trapped Child" removed from location
New Situation spawned: sit_grateful_family_018
  - Status: DORMANT (Bond 6/9 required)
  - Player knows: "Need +3 Bond with Father to unlock"
  - Player can pursue: Other Goals with Father to build Bond
  
Meanwhile, other Situations may have become AVAILABLE:
- sit_rival_firefighter_003 (Fame ≥ 5, player now Fame 7)
- sit_fire_investigation_021 (state: "Exhausted", player has it)

Player has multiple threads to pursue, all spawned organically
```

---

## Resource and State Systems

### The Five Resource Layers

**Permanent Capabilities (Stats):**
- Insight, Rapport, Authority, Diplomacy, Cunning (1-20 scale)
- Threshold checks (require value ≥ X)
- Not consumed by checks (can reuse)
- Gained slowly through challenge card play (XP accumulation)
- Gate content access and challenge difficulty

**Universal Consumable (Resolve):**
- 0-30 range
- Consumed by demanding Situations across ALL interaction types
- Creates scarcity beyond system-specific resources
- Restored by rest, low-intensity activities
- Prevents "always choose optimal action" pattern
- Forces prioritization between equally valid options

**System-Specific Consumables:**
- Focus: Mental challenges only, consumed by Attention
- Stamina: Physical challenges only, consumed by Exertion
- Initiative: Social challenges only, built during session
- Health: Physical risk only, damaged by high Danger
- These remain unchanged from current Wayfarer

**Economic Resources:**
- Coins: Universal currency
- Items: Equipment and consumables
- Function unchanged from current Wayfarer

**Relationship Resources:**
- Story Cubes: Tactical challenge benefits (0-10 per NPC)
- BondStrength: Strategic content access (-10 to +10 per NPC)
- Function: Cubes for tactics, Bonds for spawning

---

### Scales (Behavioral Reputation)

**Core Scales (6 recommended):**

**Morality: Altruism ↔ Exploitation**
- -10: Exploitative, harm justified by benefit
- 0: Pragmatic, situational ethics
- +10: Altruistic, self-sacrifice

**Lawfulness: Rebellion ↔ Establishment**
- -10: Outlaw, reject authority
- 0: Independent, selective compliance
- +10: Orthodox, uphold order

**Fame: Obscurity ↔ Renown**
- -10: Notorious, feared
- 0: Unknown, unremarkable
- +10: Celebrated, admired

**Method: Violence ↔ Diplomacy**
- -10: Force first, combat preferred
- 0: Flexible, context-dependent
- +10: Talk first, avoid force

**Transparency: Secretive ↔ Open**
- -10: Hoard information, operate in shadows
- 0: Share strategically
- +10: Share freely, transparent

**Caution: Reckless ↔ Careful**
- -10: High-risk high-reward, impulsive
- 0: Calculated risks
- +10: Minimize risk, thorough preparation

**Mechanical Properties:**
- Modified slowly by GoalCard consequences (+1 to +3 typically)
- Both extremes unlock content (different archetypes, not better/worse)
- Mid-range also has unique content (compromise Situations)
- Used in Situation requirements (threshold checks)
- Visible to player (inform identity)
- Start at 0 (neutral), shift through play

---

### States (Temporary Conditions)

**Physical States:**
- Wounded: Health < 30%, blocks demanding Physical, enables desperation
- Exhausted: Stamina < 30%, blocks intensity, enables vulnerability
- Sick: Environmental hazard result, blocks all challenges
- Injured: Failed Physical Challenge, blocks Physical specifically
- Starving: Extended travel without food, severe penalties

**Mental States:**
- Confused: Failed Mental Challenge, blocks Mental temporarily
- Traumatized: Crisis failure, blocks similar crises
- Inspired: Discovery success, enhances Mental challenges
- Focused: Preparation activity, enhances all challenges
- Obsessed: Deep Investigation, tunnel vision effect

**Social States:**
- Wanted: By authorities, blocks official interactions, enables underground
- Celebrated: By community, enhances Social, creates obligations
- Shunned: By faction, blocks that faction's content
- Humiliated: Social failure, blocks witnesses
- Disguised: Intentional concealment, enables infiltration
- Indebted: Owing favor, creates obligation pressure

**Mechanical Properties:**
- Applied by GoalCard consequences or events
- Cleared through specific actions (rest, treatment, time passage)
- Dual nature: blocks some content, enables other content
- Maximum 3-4 simultaneous (prevents overload)
- No permanent states (prevents boolean flags)
- Finite predefined list (20-25 total)

---

## How This Maps to Current Wayfarer

### Location Actions

**Current:** Pre-authored Goals at Locations
**New:** Situations spawn Goals at Locations dynamically

**Example Current:**
```
Mill Location has Goals:
- "Examine Waterwheel" (always available)
- "Work at Mill" (always available, repeatable)
```

**Example New:**
```
Mill Location starts with Tier 1 Situations:
- "Basic Mill Work" (always AVAILABLE, repeatable, Tier 1)
- Player completes Investigation → spawns "Waterwheel Mystery" Situation
- Waterwheel Mystery becomes AVAILABLE → spawns Goal "Examine Waterwheel"
- Player completes Examination → spawns "Broken Mechanism" Situation
- And cascade continues...
```

**Change:** Goals still launch Mental Challenges at Locations. Just spawned by Situations instead of pre-existing.

---

### Route Travel

**Current:** Pre-authored PathCards on Route segments
**New:** Situations can spawn PathCards or spawn Situations at destination

**Example Current:**
```
Route Segment 2 has PathCards:
- "Main Road" (safe, slow)
- "Forest Path" (risky, fast)
```

**Example New:**
```
Route Segment 2 starts with basic PathCards
Player has state: "Wanted" → spawns "Fugitive Path" Situation
Situation adds PathCard: "Hidden Trail" (requires Wanted state)
Player selects PathCard "Help Wounded Traveler"
  → spawns "Grateful Merchant" Situation at destination
```

**Change:** Routes still use segment/PathCard mechanics. Situations add dynamic options and destination consequences.

---

### Mental Challenges

**Current:** Goals target Obstacles with MentalComplexity at Locations
**New:** Situations spawn Obstacles and Goals together

**Example Current:**
```
Investigation Phase spawns:
- Obstacle at Library: MentalComplexity 2
- Goal "Research Ancient Text": launches Mental Challenge
```

**Example New:**
```
Situation "Archive Mystery" spawns:
- Obstacle at Library: MentalComplexity 2  
- Goal "Research Ancient Text": launches Mental Challenge
- On completion: spawns "Hidden Translation" Situation (cascade)
```

**Change:** Mental Challenges unchanged. Just triggered by dynamically spawned Goals instead of pre-authored ones.

---

### Physical Challenges

**Current:** Goals target Obstacles with PhysicalDanger at Locations
**New:** Situations spawn Obstacles and Goals together, add consequence chains

**Example Current:**
```
Investigation Phase spawns:
- Obstacle at Tower: PhysicalDanger 3
- Goal "Climb Tower": launches Physical Challenge
```

**Example New:**
```
Situation "Tower Ascent" spawns:
- Obstacle at Tower: PhysicalDanger 3
- Goal "Climb Tower": launches Physical Challenge
- On success: spawns "Eagle's View" Situation (discovery)
- On failure with Wounded state: spawns "Recovery Arc" Situation
```

**Change:** Physical Challenges unchanged. Situations add branching consequences based on outcome and state.

---

### Social Challenges

**Current:** Goals at NPCs launch Social Challenges
**New:** Situations spawn Goals at NPCs based on BondStrength and history

**Example Current:**
```
Elena NPC has Goals:
- "Discuss Mill Problem" (Investigation phase)
- "Accept Delivery Request" (always available)
```

**Example New:**
```
Elena starts with Tier 1 Situations:
- "First Meeting" (always AVAILABLE)
- Player builds Bond to 3 → spawns "Trusted Confidant" Situation
- Trusted Confidant spawns Goal: "Discuss Mill Problem"
- Player builds Bond to 7 → spawns "Romantic Interest" Situation
- And cascade continues based on player choices...
```

**Change:** Social Challenges unchanged. BondStrength now gates Situation spawning, creating narrative depth.

---

### Investigations

**Current:** Primary content structure, phases spawn Obstacles sequentially
**New:** Just one Situation archetype among many, can spawn recursively

**Example Current:**
```
Investigation "Mill Mystery":
- Phase 1: Spawns Obstacle at Mill
- Phase 2 (requires Phase 1): Spawns Obstacle at Warehouse  
- Phase 3 (requires Phase 2): Spawns Obstacle at Town Hall
- Complete Investigation: Rewards granted
```

**Example New:**
```
Situation "Mill Mystery" (Investigation archetype):
- Spawns Obstacle and Goals at Mill
- On completion: spawns "Warehouse Connection" Situation
- Warehouse completion: spawns "Town Hall Corruption" Situation
- Town Hall completion: spawns "Trial of the Miller" Situation
- Each Situation can branch based on player approach
```

**Change:** Investigations become templates that spawn chains. Lose linear phase structure, gain branching depth.

---

## Avoiding the Boolean Gate Trap

### What Boolean Gates Are

**Anti-Pattern:**
```
if (completed_quest_X == true) {
    unlock_content_Y
}
```

**Why Bad:**
- Single path to content (no build variety)
- Binary state (have or don't have, no spectrum)
- Permanent (once set, always set)
- No alternative access (must complete X to get Y)
- Creates linear progression (no player expression)

---

### How We Avoid Them

**Mechanism 1: Numerical Accumulation Instead of Binary Flags**

**Boolean Gate (Bad):**
```
Situation requires: completed_rescue_quest == true
```

**Numerical Alternative (Good):**
```
Situation requires: Authority >= 10 OR Bond >= 5 OR Morality >= 8
```

**Why Better:**
- Three valid paths (stat focus, relationship focus, reputation focus)
- Spectrum access (partial progress visible)
- Build variety (different characters reach content differently)
- Strategic planning (player chooses which resource to accumulate)

---

**Mechanism 2: Compound OR Requirements**

**Boolean Gate (Bad):**
```
Situation requires: 
  has_item_A == true 
  AND completed_quest_B == true
  AND relationship_level >= 5
```

**Compound OR Alternative (Good):**
```
Situation requires:
  (Authority >= 12 AND has_item_A) 
  OR (Bond >= 7) 
  OR (Cunning >= 15 AND Coins >= 100)
```

**Why Better:**
- Multiple valid combinations
- Different builds access same content
- Economic alternative (money solves problems)
- Social alternative (relationships solve problems)
- Capability alternative (skill solves problems)

---

**Mechanism 3: Scales Provide Spectrum, Not Binary**

**Boolean Gate (Bad):**
```
Situation requires: is_heroic == true
```

**Scale Alternative (Good):**
```
Situation requires: Morality >= 7
```

**Why Better:**
- Gradual approach (visible progress toward threshold)
- Reversible (Morality can decrease)
- Spectrum content (different Morality ranges unlock different content)
- Both extremes valid (-10 and +10 both unlock content, just different types)

---

**Mechanism 4: States Are Temporary, Not Permanent**

**Boolean Flag (Bad):**
```
Player sets: is_wanted = true (permanent reputation damage)
```

**State Alternative (Good):**
```
Player gains: state "Wanted" (clearable through quest or time)
```

**Why Better:**
- Recoverable (not permanent soft-lock)
- Dual nature (blocks some content, enables other content)
- Strategic choice (accept temporary restriction for opportunity)
- Time-sensitive (creates urgency without permanence)

---

**Mechanism 5: Achievements Used as Narrative Context, Rarely as Gates**

**Boolean Gate (Bad):**
```
Situation requires: has_achievement("Rescued_Child") == true
Only way to access content
```

**Narrative Context (Good):**
```
Situation requires:
  (has_achievement("Rescued_Child") AND Bond >= 3)
  OR (Morality >= 8 AND Authority >= 12)
```

**Why Better:**
- Achievement is one path, not only path
- Alternative stat combination works
- Achievement provides narrative coherence when present
- But content still accessible through different builds

---

**Mechanism 6: Resource Costs, Not Completion Checks**

**Boolean Gate (Bad):**
```
Situation requires: completed_tutorial == true
```

**Resource Gate (Good):**
```
Situation requires: Resolve >= 5 AND Coins >= 20
```

**Why Better:**
- Affordability question (can you pay the cost?)
- Not prerequisite question (did you do the task?)
- Multiple ways to accumulate resources
- Economic depth (opportunity cost between options)

---

### The Core Principle

**Gate on "What resources have you accumulated?" not "What specific thing did you do?"**

Even when we check Achievements, the Achievement came from a resource-gated choice. The gate exists at the point of earning the Achievement (where stats/resources were checked), not at the point of checking the Achievement (which is just narrative context in compound OR requirements).

**Example Chain:**
```
Step 1: Rescue Situation requires Authority >= 10 (resource gate)
Step 2: Completing Rescue grants Achievement (milestone marker)
Step 3: Future Situation requires (Achievement + Bond >= 5) OR (Morality >= 8)
        Achievement is narrative context in compound requirement
        Alternative stat path available
        Real gate is still numerical resources
```

---

## AI Narrative Generation Requirements

### Context Queries (What AI Sees)

**Player Identity:**
- All Achievements earned (history of significant choices)
- Scale values (behavioral reputation: Morality, Lawfulness, Fame, Method, Transparency, Caution)
- Active States (current conditions: Wounded, Exhausted, Wanted, etc.)
- Stat values (capabilities: Insight, Rapport, Authority, Diplomacy, Cunning)

**Relationship Context:**
- BondStrength with involved NPCs (numerical depth)
- StatusTags on NPCs (qualitative states: "Grateful", "Betrayed", "Allied")
- Story Cubes with NPCs (relationship progression)
- Past Situations completed involving these NPCs

**Environmental Context:**
- Location properties (Delicate, Obscured, Layered, etc.)
- Investigation Cubes at Location (familiarity)
- Current obstacles present
- Recent events at Location

**Temporal Context:**
- Current game day (for time-sensitive references)
- Active Obligations with deadlines
- Recent Situation completions (what just happened)

---

### Generation Constraints

**Must Include:**
- References to relevant Achievements when present
  - "Because you rescued the child last month..."
  - "Your reputation for solving mysteries precedes you..."
- References to BondStrength state
  - "After all you've been through together..." (high Bond)
  - "The stranger regards you with suspicion..." (low/negative Bond)
- References to Scale reputation
  - "Your altruistic nature is well-known..." (high Morality)
  - "Your ruthless reputation makes them nervous..." (low Morality)
- Explanation of why requirements exist
  - "Only someone with commanding presence..." (Authority requirement)
  - "Your exhaustion is obvious, they expect desperation..." (Exhausted state requirement)

**Must Avoid:**
- Exact mechanical values ("Requires Authority 12")
  - Instead: "Requires commanding presence" or "Only a true leader..."
- Meta-references to game systems
  - Never: "This is a Tier 3 Situation"
  - Never: "You need +2 Bond to unlock..."
- Breaking verisimilitude
  - Don't explain spawn mechanics
  - Don't reference templates or archetypes
  - Stay in narrative voice

**Tone Matching:**
- Discovery Situations: Curious, mysterious, investigative
- Crisis Situations: Urgent, tense, high-stakes, immediate
- Alliance Situations: Collaborative, hopeful, strategic
- Betrayal Situations: Dark, conflicted, morally complex
- Romance Situations: Intimate, vulnerable, emotional
- Conflict Situations: Intense, violent, decisive
- Trade Situations: Professional, transactional, pragmatic
- Obligation Situations: Pressured, committed, determined

---

### Choice Text Generation

**Format:**
- 2-4 sentences describing the action
- Explain what player character will do
- Hint at consequences without spoilers
- Match tone to Situation archetype
- Reference player capabilities naturally

**Examples:**

**Discovery (Mental):**
"Examine the strange markings carved into the waterwheel. Your experience investigating similar mechanisms suggests this might reveal the mill's problems. It will require concentration to decipher the patterns, but the insight could be valuable."

**Crisis (Physical):**
"Rush into the burning building to save the trapped child. You'll need to move fast and decisively—the stairs are already collapsing. The danger is real. People have died doing exactly this."

**Alliance (Social):**
"Propose a formal partnership with Elena's merchant network. Your reputation and her connections could create something powerful in this region. This level of commitment will cost both time and resources, but the opportunities could be significant."

**Betrayal (Social):**
"Feed false information to Marcus about the caravan route. He trusts you completely—he'll walk right into the ambush. But once you cross this line, there's no taking it back. Your conscience will carry this weight."

---

## Implementation Strategy

### Phase 1: Core Infrastructure (Weeks 1-2)

**New Entities:**
- Create Situation entity (template and instance)
- Create Achievement entity
- Create Scale entity (6 core scales)
- Create State entity (20-25 states)
- Add Resolve resource to Player

**Modified Entities:**
- Add Achievement list to Player
- Add Scale dictionary to Player
- Add State list to Player
- Add Resolve value to Player
- Add BondStrength to NPC
- Add StatusTags to NPC
- Add AvailableSituationIds to Location
- Add AvailableSituationIds to NPC
- Expand Goal.Requirements object
- Expand GoalCard.Rewards object

**Deliverable:** All entities defined and connected to GameWorld ownership

---

### Phase 2: Template Library (Weeks 3-4)

**Archetype Implementation:**
- Create 5 core archetypes (Rescue, Discovery, Alliance, Trade, Crisis)
- 3-4 variants per archetype for Tier 1-2
- Define spawn rules between templates
- Write narrative hints for AI generation

**Requirement System:**
- Implement formula calculation at spawn time
- Implement compound OR requirement checking
- Implement Scale threshold checking
- Implement State presence/absence checking
- Implement Achievement checking

**Deliverable:** 20-25 functional templates with spawn rules tested

---

### Phase 3: Cascade Mechanics (Weeks 5-6)

**Spawn System:**
- Implement instance creation from templates
- Implement requirement calculation using player state
- Implement DORMANT status tracking
- Implement continuous evaluation for AVAILABLE status
- Implement cascade triggering from GoalCard rewards

**Goal Spawning:**
- Implement dynamic Goal creation from Situations
- Implement Goal placement at Locations/NPCs
- Implement Resolve cost checking
- Implement parent Situation tracking

**Deliverable:** Full spawn cascade working from choice to new Situation

---

### Phase 4: AI Integration (Weeks 7-8)

**Context System:**
- Implement player state query system
- Implement relationship history tracking
- Implement Achievement narrative integration
- Implement Scale reference generation

**Generation Pipeline:**
- Integrate AI call at AVAILABLE → ACTIVE transition
- Pass context to AI (Achievements, Bonds, Scales, States)
- Store generated narrative in Situation instance
- Generate choice text for Goals

**Deliverable:** Contextual narrative generation working for all archetypes

---

### Phase 5: Balance and Polish (Weeks 9-10)

**Requirement Tuning:**
- Test requirement formulas across all tiers
- Ensure multiple valid paths to content
- Verify no soft-locks from one-path gating
- Balance resource scarcity (Resolve especially)

**Spawn Rate Control:**
- Tune cooldowns between same archetype
- Limit concurrent AVAILABLE Situations (max 20)
- Ensure steady progression feel
- Test cascade depth and branching

**Deliverable:** Balanced progression curve, no dead ends, smooth cascade feel

---

### Phase 6: Content Expansion (Weeks 11+)

**Additional Archetypes:**
- Implement remaining archetypes (Betrayal, Rivalry, Romance, Conflict, Obligation)
- Create Tier 3-4 variants
- Add edge case templates
- Expand archetype variety

**System Integration:**
- Route PathCard Situation spawning
- Location exploration Situations
- Faction reputation Situations
- Economic progression Situations

**Deliverable:** 50+ templates covering all interaction types and tiers

---

## Validation Against Design Principles

### Principle 1: Single Source of Truth

✅ **Maintained:**
- GameWorld owns all entities via flat dictionaries
- Situations own spawned Goals/Obstacles (lifecycle control)
- Locations/NPCs reference by ID only (placement, not ownership)
- No ambiguous ownership chains

---

### Principle 2: Strong Typing

✅ **Maintained:**
- List<Situation>, List<Achievement>, List<Scale>, List<State>
- No Dictionary<string, object>
- All relationships explicit (SituationId → Situation lookup)
- Requirement checking via typed objects, not string matching

---

### Principle 3: Ownership vs Placement vs Reference

✅ **Maintained:**
- Situations own Goals (lifecycle: created and destroyed together)
- Goals appear at Locations (placement: UI context only)
- Locations reference Situations (lookup: no lifecycle control)
- Clear distinction preserved

---

### Principle 4: Inter-Systemic Rules Over Boolean Gates

✅ **Enhanced:**
- No boolean flags anywhere (all numerical or compound)
- Resources compete (Resolve competes with Focus/Stamina/Coins/Time)
- Scales compete (can't maximize all simultaneously)
- Opportunity cost creates strategic depth
- Multiple valid paths to same content (OR requirements)

---

### Principle 5: Typed Rewards as System Boundaries

✅ **Maintained:**
- GoalCard rewards are typed (AchievementGrant, BondChange, ScaleShift, SituationSpawn)
- Applied at completion, not continuous evaluation
- Explicit system connections (Social → Bond → Situation spawn)
- No hidden state queries

---

### Principle 6: Resource Scarcity Creates Impossible Choices

✅ **Enhanced:**
- Resolve as universal scarcity (all content types compete)
- States create temporary restrictions (forces adaptation)
- Scales create build commitment (can't be all things)
- Multiple AVAILABLE Situations but limited resources to pursue all
- Economic constraints remain (Coins, Time, Focus, Stamina)

---

### Principle 7: One Purpose Per Entity

✅ **Maintained:**
- Situations: Define spawn patterns and consequence chains
- Goals: Launch challenges at specific locations/NPCs
- Obstacles: Property containers for challenge difficulty
- Achievements: Milestone markers for narrative coherence
- Scales: Behavioral reputation for content access
- States: Temporary conditions for dynamic eligibility
- Each entity has single clear purpose

---

### Principle 8: Verisimilitude in Entity Relationships

✅ **Maintained:**
- Situations spawn from player choices (organic cause-effect)
- Goals appear at logical locations (burning building rescue at burning building)
- Relationships deepen through interaction (BondStrength from shared experiences)
- Reputation spreads through actions (Scales shift from behavior patterns)
- Nothing arbitrary (all connections narratively justified)

---

### Principle 9: Minimal Interconnection

✅ **Maintained:**
- Systems connect at explicit boundaries:
  - Situations → Goals (spawning)
  - Goals → Challenges (launching)
  - Challenges → GoalCard rewards (resolution)
  - GoalCard rewards → Situations (cascade)
- Clean separation preserved
- No tangled web of cross-system dependencies

---

### Principle 10: Perfect Information with Hidden Complexity

✅ **Maintained:**
- **Strategic layer visible:**
  - All AVAILABLE Situations shown
  - All requirements displayed
  - All Resolve costs known
  - All Scale thresholds visible
  - DORMANT Situations show what's needed to unlock
- **Tactical layer hidden:**
  - Challenge card draws unknown
  - Exact challenge flow uncertain
  - Skill-based execution required
- Perfect information enables strategic planning
- Hidden tactics create engaging moment-to-moment play

---

## Critical Success Factors

### 1. Requirement Formula Quality

**Risk:** Formulas that make content too easy or too hard to access
**Mitigation:** 
- Test each tier's formulas extensively
- Ensure multiple valid paths exist
- Verify no impossible combinations
- Player feedback on accessibility

---

### 2. AI Narrative Coherence

**Risk:** Generated text feels generic or disconnected from player history
**Mitigation:**
- Comprehensive context queries (all Achievements, Bonds, Scales)
- Strong narrative hints in templates
- Fallback patterns for low-context situations
- Human review of AI output quality

---

### 3. Cascade Balance

**Risk:** Too many or too few Situations spawning
**Mitigation:**
- Cooldown systems prevent spam
- Maximum concurrent AVAILABLE limit (20)
- Spawn conditions tuned per tier
- Parent-child tracking prevents loops

---

### 4. Resource Scarcity Tuning

**Risk:** Resolve either trivial or prohibitive
**Mitigation:**
- Test recovery rate vs consumption rate
- Ensure low-cost options always available (safety net)
- High-value content costs more (strategic choices)
- Balance tension without frustration

---

### 5. Template Coverage

**Risk:** Certain player builds or situations feel unsupported
**Mitigation:**
- Multiple archetypes per interaction type
- OR requirements validate different builds
- Edge case templates for unusual combinations
- Iterative expansion based on play patterns

---

## Success Metrics

**Content Depth:**
- Average cascade chains reach 4+ levels
- Player sees 100+ unique Situations per playthrough
- Less than 20% Situation overlap between playthroughs

**Player Expression:**
- All 6 Scales develop distinct profiles by mid-game
- Different stat builds access 80%+ same content through different paths
- Relationship patterns create unique NPC constellation per playthrough

**Narrative Coherence:**
- AI references player history in 90%+ ACTIVE Situations
- Generated text matches archetype tone in 95%+ cases
- Players report story feeling "reactive and personalized"

**Strategic Depth:**
- Players report meaningful impossible choices
- Resolve scarcity forces prioritization
- Multiple playthroughs feel distinct due to different cascade chains

---

## Conclusion

This system transforms Wayfarer from static content discovery to dynamic consequence generation. By separating mechanical templates from narrative flesh, we achieve Sir Brante's accumulative progression depth and cascade consequence chains while maintaining Wayfarer's five-system challenge architecture and resource competition design.

The key innovation: Gate on numerical accumulation (stats, Bonds, Scales, Resolve) with compound OR requirements, ensuring multiple valid paths to content. Situations spawn from player choices with calculated requirements, creating emergent narratives through mechanical chains that AI wraps in contextual story.

Implementation priority: Core infrastructure → Template library → Cascade mechanics → AI integration → Balance tuning → Content expansion. Each phase builds on previous, enabling iterative testing and refinement.

This architecture avoids the boolean gate trap entirely while creating deeper progression, longer consequence chains, and more replayable content than the current static model. The result: A game that feels alive and reactive, where every playthrough tells a unique story through the player's accumulated choices, relationships, and reputation.
