# Wayfarer Scene-Situation Architecture: Complete Design Specification

## Executive Summary

This document defines the complete architectural redesign of Wayfarer to integrate Sir Brante's choice-consequence progression model while preserving Wayfarer's unique tactical challenge depth and spatial navigation. The core innovation is the **Scene-Instance Pattern**: treating each location visit as a unique narrative moment that presents dynamically spawned Situations filtered by player state, creating emergent storylines through cascading consequence chains.

**Key Achievement:** Translate Sir Brante's linear scene progression to spatial navigation while maintaining immediate consequence feedback, perfect information display, and accumulative resource-based gating that completely eliminates boolean flags.

**Architectural Scope:** Full-stack redesign from JSON content through parsers, domain entities, facades, and UI components to create a living world that responds to player choices both mechanically and narratively.

---

## Current Wayfarer State (Pre-Implementation)

### What Already Exists (Foundation to Build On)

**Situation Entity (Renamed from Goal in Prior Refactor):**
- File: `src/GameState/Situation.cs`
- Purpose: Strategic player choices that launch challenges or resolve instantly
- Properties:
  - Placement: `PlacementLocationId`, `PlacementNpcId`, `PlacementRouteId`
  - Challenge Definition: `SystemType` (Social/Mental/Physical), `DeckId`
  - Victory Conditions: `SituationCards` (was GoalCards - tactical layer)
  - Costs: `SituationCosts` (Time, Focus, Stamina, Coins)
  - Consequences: `ConsequenceType`, `PropertyReduction`
  - State: `Status`, `IsAvailable`, `IsCompleted`, `DeleteOnSuccess`

**Challenge Systems (Complete, Unchanged):**
- Mental challenges (card-based puzzle solving)
- Physical challenges (endurance/breakthrough mechanics)
- Social challenges (conversation threading)
- All challenge flow, decks, and cards remain identical

**Time System (Segment-Based):**
- `CurrentDay` (int: 1, 2, 3...)
- `CurrentTimeBlock` (enum: Morning, Midday, Afternoon, Evening)
- `CurrentSegment` (int: segment within timeblock)
- NO DateTime, NO timestamps - everything uses segments

**Player Resources (Existing):**
- Stats: Insight, Rapport, Authority, Diplomacy, Cunning (accumulative thresholds)
- Focus (Mental challenge resource)
- Stamina (Physical challenge resource)
- Health (survival resource)
- Coins (economic resource)
- Items, Equipment (inventory system)

**World Structure (Existing):**
- Obligations → Obstacles → Situations flow (was Goals)
- Locations with Investigation Cubes (familiarity tracking)
- NPCs with Story Cubes (tactical challenge benefits)
- Routes (travel system)
- GameWorld as single source of truth

### What We're Adding (Sir Brante Integration)

**Extended Situation Properties:**
- Spawn Rules: `OnSuccessSpawns`, `OnFailureSpawns` (cascading consequences)
- Interaction Type: `InteractionType` enum (Instant, Challenge, Navigation)
- Compound Requirements: `CompoundRequirement` (multiple OR paths to unlock)
- Projected Consequences: Visible before selection
- Tier: 0-4 (content complexity, 0 = safety net)
- Spawn Tracking: `TemplateId`, `ParentSituationId`

**New Player Resources:**
- `Resolve` (int 0-30, universal consumable like Willpower)
- `Scales` (nested object with 6 int properties -10 to +10):
  - `Morality`, `Lawfulness`, `Method`, `Caution`, `Transparency`, `Fame`
- `ActiveStates` (List<ActiveState> using StateType enum):
  - Temporary conditions tracked by Day/TimeBlock/Segment
- `EarnedAchievements` (List<PlayerAchievement>):
  - Milestone markers with Day/TimeBlock/Segment earned

**New Domain Entities:**
- `Scene` (ephemeral UI construct, not stored in GameWorld)
- `PlayerScales` (nested object with 6 properties)
- `ActiveState` (StateType enum + segment-based duration)
- `PlayerAchievement` (achievement ID + segment-based earned time)
- `CompoundRequirement` (OR-path structure)
- `SpawnRule` (template reference + requirement offsets)

**New Facades:**
- `SceneFacade` (generate Scene instances per visit)
- `SituationFacade` (handle selection, resolution, routing)
- `SpawnFacade` (execute spawn rules, manage cascades)
- `ConsequenceFacade` (apply multi-axis consequences)

**New UI Components:**
- `LocationSceneScreen` (Sir Brante-style choice presentation)
- `SituationCard` (one choice display)
- `ConsequenceModal` (immediate feedback after resolution)
- `RequirementPathDisplay` (compound OR visualization)

### What's NOT Changing

**No renames needed:**
- Situation.cs exists (Goal→Situation rename complete)
- Challenge systems unchanged
- GoalCards remain (tactical victory conditions)
- Time tracking remains segment-based
- GameWorld ownership hierarchy unchanged

**Strong Typing Enforced:**
- NO dictionaries with string keys (except entity lookups by ID)
- NO lists of generic objects with type discriminators
- ALL properties strongly typed
- JSON names match C# property names exactly (NO JsonPropertyName)

---

## Table of Contents

1. [Problem Statement & Motivation](#problem-statement--motivation)
2. [Sir Brante Pattern Analysis](#sir-brante-pattern-analysis)
3. [The Scene-Instance Solution](#the-scene-instance-solution)
4. [Core Architectural Concepts](#core-architectural-concepts)
5. [Entity Architecture](#entity-architecture)
6. [Content Layer Design](#content-layer-design)
7. [Parser & Catalogue Layer](#parser--catalogue-layer)
8. [Domain Logic Layer](#domain-logic-layer)
9. [UI Component Architecture](#ui-component-architecture)
10. [Data Flow Patterns](#data-flow-patterns)
11. [AI Integration Strategy](#ai-integration-strategy)
12. [State Management & Persistence](#state-management--persistence)
13. [Resource Systems](#resource-systems)
14. [Requirement Architecture](#requirement-architecture)
15. [Spawn Cascade Mechanics](#spawn-cascade-mechanics)
16. [Player Experience Design](#player-experience-design)
17. [Validation & Safety Systems](#validation--safety-systems)
18. [Implementation Strategy](#implementation-strategy)
19. [Success Metrics](#success-metrics)

---

## Problem Statement & Motivation

### Current Wayfarer Limitations

**Static Content Exhaustion:**
Wayfarer currently uses pre-authored Goals placed at Locations. Once a player has interacted with all authored content, the world feels static and unchanging. The Investigation system provides multi-phase structure, but content remains finite and identical across playthroughs. There is no emergent narrative—the story always unfolds the same way regardless of player approach.

**Shallow Consequence Chains:**
Current consequence depth reaches only two levels: Investigations spawn Obstacles, Goals reduce Obstacle properties. When properties reach zero, Obstacles are removed. This creates a simple linear reduction pattern without branching consequences or long-term narrative threads. Player choices feel disconnected from world evolution.

**Limited Gating Diversity:**
Content access primarily gates on economic resources (can afford coins) and stat thresholds (can attempt challenge). While this works, it lacks the multi-axis gating that creates different character builds and playstyles. The same player with the same stats sees the same content every time.

**No Behavioral Identity:**
Players accumulate stats and resources, but the world treats them identically regardless of past actions. There is no reputation system tracking whether the player is altruistic vs exploitative, violent vs diplomatic, cautious vs reckless. No emergent character identity from behavioral patterns.

**Missing Immediate Feedback:**
When players complete Goals, consequences are often invisible or delayed. There's no explicit notification of what changed in the world, what new content unlocked, or where new opportunities appeared. Players must explore to discover consequences, breaking narrative momentum.

### What We Need

**Living World Simulation:**
A single playthrough should feel like an ongoing story that responds to player choices. Content should spawn dynamically based on accumulated player state, creating unique narrative arcs each playthrough. The world should remember player actions and react accordingly.

**Deep Consequence Chains:**
Choices should cascade four to five levels deep, with each resolution spawning new situations that spawn further situations. These chains should branch based on player approach, creating emergent storylines that weren't pre-authored but arise from mechanical interactions.

**Multi-Axis Character Identity:**
Player identity should emerge from accumulation patterns across multiple dimensions: stats, relationships, behavioral reputation, achievements, and temporary conditions. Different builds should access the same content through different paths, ensuring all approaches remain viable.

**Perfect Information Strategic Layer:**
Players should see all available options and all locked options with explicit requirements. Strategic planning requires knowing what's possible and what's needed to unlock it. This transparency enables meaningful choice between visible alternatives.

**Immediate Consequence Visibility:**
Every action should show its consequences explicitly: what changed, what spawned, where new opportunities appeared. The Sir Brante pattern of immediate feedback creates satisfying cause-effect chains that validate player decisions.

---

## Sir Brante Pattern Analysis

### How Sir Brante Achieves Strategic Depth

**Accumulative Stat System:**
Sir Brante gates content on numerical stat thresholds that accumulate over time. Early choices matter more because stats compound (one point in chapter 1 equals three points as adult). Multiple stats can satisfy the same requirement, creating different valid paths. Players see their character build emerge organically from accumulated choices rather than explicit class selection.

**Willpower as Universal Scarcity:**
Beyond stats, Willpower functions as a consumable resource that creates impossible choices. Even when stats qualify for multiple options, limited Willpower forces prioritization. This resource transcends all challenge types, creating inter-systemic competition that generates strategic tension.

**Perfect Information Gating:**
All choices remain visible even when locked, with explicit requirements displayed. Players see "Requires Valor 12 (you have 8)" and can plan how to reach that threshold. This transparency enables strategic planning across a three-to-four choice horizon.

**Destiny Registry (Achievement Pattern):**
Significant choices grant named achievements that later content references. Critically, achievements integrate into compound OR requirements rather than functioning as sole gates. A situation might require "Achievement X AND Stat Y >= 5" OR "Stat Z >= 12", maintaining multiple valid paths while enabling narrative coherence.

**Cascading Consequence Spawning:**
Every choice spawns one to three new choices with calculated requirements. These spawns create four-to-five level chains where content doesn't exist until earned through play. Same templates spawn with different requirements based on player state, creating unique chains per playthrough.

**Immediate Feedback Loop:**
When players select a choice, resolution shows consequences immediately: stat changes, new traits, spawned scenes. Players see cause-effect chains explicitly, creating satisfying validation of decisions and clear understanding of world reaction.

### The Core Sir Brante Flow

```
Scene Entry
  ↓ Shows narrative context
Choice Presentation
  ↓ Available choices (green) + Locked choices (gray with requirements)
Player Selection
  ↓ One choice from list
Resolution
  ↓ Shows consequences: +2 Valor, -1 Cunning, Gained Trait: Loyal
Immediate Spawn
  ↓ Next scene spawns instantly with new choices
Forced Progression
  ↓ Must engage scene to continue (linear timeline)
```

**Key Insight:** Sir Brante uses linear chronological progression (childhood → adolescence → adulthood) where scenes occur once and players move forward. You never return to previous scenes. This creates narrative momentum but limits exploration.

### Translation Challenge: Linear vs Spatial

**Sir Brante:** Linear scene chain, forced progression, chronological lock
**Wayfarer:** Spatial graph navigation, return visits, persistent locations

These seem incompatible, but the solution lies in recognizing that **Scene ≠ Location**. A scene is a temporal moment at a location, not the location itself. Multiple scenes can occur at the same location across different visits.

---

## The Scene-Instance Solution

### Core Concept: Scene as Interaction Moment

**Scene Definition:**
A Scene is a generated narrative moment presenting available choices when the player interacts with a Location, NPC, or Route. Scenes are ephemeral—generated on-demand based on current world state, not stored as permanent entities. Each visit to the same location generates a new Scene instance.

**Scene-Instance Pattern:**
```
Visit Mill (First Time)
  → Scene Instance #1: "Broken Wheel" presents initial situations

Complete "Repair Wheel"
  → Spawns new situations at Mill and elsewhere

Visit Mill (Second Time)
  → Scene Instance #2: "Wheel Fixed" presents different situations
  → References past repair in narrative
  → Shows newly spawned situations

Complete "Martha's Request"
  → Spawns more situations

Visit Mill (Third Time)
  → Scene Instance #3: "Deepening Trust" with evolved content
  → Location same, Scene different, reflects progression
```

### Bridging Linear and Spatial

**Sir Brante Linear Progression:**
- Scene A (Trial) → Choice → Scene B (Prison) spawns immediately
- Never return to Scene A
- Forced chronological momentum

**Wayfarer Spatial Adaptation:**
- Location A (Mill) → Choice → Situations spawn at Locations A, B, C
- Return to Location A triggers new Scene instance
- Same location evolves through multiple Scene instances
- Player controls pacing via navigation choices

**Immediate Feedback Preservation:**
After resolving a Situation, show Consequence Modal explicitly listing:
- Stat/bond/scale changes
- Spawned Situations and their locations
- Player chooses: Stay (regenerate Scene with new content) OR Leave (navigate elsewhere)

This maintains Sir Brante's immediate consequence revelation while preserving spatial navigation freedom.

### Scene Structure

**Scene Components:**

**Intro Narrative (Context-Setting):**
Generated narrative that references player history, current relationships, active states, and recent events. Creates immersive entry point specific to this visit instance. AI-generated to reflect accumulated player state, not static location description.

**Situation Collection (Choice Presentation):**
Available Situations (green, clickable) grouped by category: Urgent, Progression, Relationship, Opportunity, Exploration. Locked Situations (gray, shows requirements) with visible paths to unlock. Each Situation shows costs, projected consequences, and interaction type.

**Resolution Flow (Consequence Revelation):**
After Situation selection and resolution, explicitly show what changed, what spawned, where new opportunities appeared. Offer choice to stay (regenerate Scene) or leave (return to navigation).

**Scene Types:**

**Location Scene:** Interaction with physical place, presents place-based Situations
**NPC Scene:** Conversation/interaction with character, presents relationship-based Situations
**Route Scene:** Travel segment, presents path choices and conditional encounters
**Event Scene:** Special authored moments (rare), presents scripted critical choices

---

## Core Architectural Concepts

### Situation Entity (Extended from Previous Goal Refactor)

**Current State:**
The Situation entity already exists in `src/GameState/Situation.cs` (renamed from Goal in a prior refactor). It currently represents approaches to overcome Obstacles: pre-authored and placed at Locations/NPCs, launches Challenges (Mental/Physical/Social), contains SituationCards (was GoalCards) as victory conditions.

**What We're Extending:**
Situations will be enhanced to represent Sir Brante-style strategic choices with:
- Dynamic spawning based on world state (not just pre-authored placement)
- Instant resolution option (not all Situations launch challenges)
- Navigation type option (trigger movement to new locations)
- Spawn rules (completing one Situation spawns new Situations elsewhere)
- Compound OR requirements (multiple paths to unlock)
- Projected consequences (visible before selection)

**What This Extension Adds:**
- Spawn system: `OnSuccessSpawns`, `OnFailureSpawns` properties
- Interaction type: `InteractionType` enum (Instant, Challenge, Navigation)
- Requirement system: `CompoundRequirement` replacing simple gates
- Consequence projection: Display changes before commitment
- Template tracking: `TemplateId`, `ParentSituationId` for spawn chains
- Tier classification: 0-4 for content complexity

**What Stays Unchanged:**
- Challenge launching (for Challenge-type Situations)
- SituationCards (tactical victory conditions within challenges)
- Obstacle targeting (optional, Situations can target Obstacles)
- Placement context (Situations appear at specific Locations/NPCs)
- Cost structure (`SituationCosts`)
- Consequence types (`ConsequenceType`, `PropertyReduction`)

### Obstacles Remain Context

**Obstacle Purpose:**
Obstacles continue functioning as strategic problem trackers with properties (PhysicalDanger, MentalComplexity, SocialDifficulty). Multiple Situations can target the same Obstacle, progressively reducing its properties. When properties reach zero, Obstacle is defeated.

**Why Keep Separate:**
Obstacles represent persistent world state—problems that exist until solved. Situations represent available actions—choices that spawn and complete. Maintaining this distinction preserves the existing Investigation system's multi-phase structure and property-reduction mechanics.

**Refactor Without Rename:**
Investigations continue spawning Obstacles with Situations (instead of Goals). The architecture remains identical; only the entity reference changes.

### Scenes as UI Wrappers

**Scene Responsibility:**
Scenes are generated UI constructs, not stored domain entities. When player enters a Location, the Scene queries GameWorld for available Situations at that Location, filters by player state, generates contextual narrative, and presents as organized choice list.

**Scene Lifecycle:**
1. Player navigates to Location
2. System generates Scene instance (queries Situations, evaluates requirements)
3. Player sees Scene display (narrative + categorized Situations)
4. Player selects Situation
5. Resolution occurs (Instant, Challenge, or Navigation)
6. Consequence Modal displays results
7. Player chooses: Stay (regenerate Scene) OR Leave (exit)
8. Scene discarded (next visit generates new instance)

**No Scene Storage:**
Scenes exist only during active display. They're query results, not persistent entities. This keeps GameWorld clean—only domain entities stored, UI constructs generated on-demand.

### Challenges Remain Tactical Layer

**No Changes to Challenges:**
Mental, Physical, and Social challenge systems continue functioning identically. Cards, decks, challenge flow, breakthrough/danger mechanics—all unchanged.

**Integration Point:**
Challenge-type Situations define a Challenge payload (type, target obstacle, GoalCards). When selected, Situation launches the Challenge via existing ChallengeFacade. Challenge resolution returns to Situation completion flow, which applies strategic consequences and triggers spawns.

**Separation of Concerns:**
- **GoalCards:** Tactical rewards (coins, stat XP, bond changes)
- **Situations:** Strategic consequences (spawn new Situations, shift scales, apply states)

This maintains Wayfarer's unique tactical depth while layering Sir Brante's strategic choice architecture on top.

---

## Entity Architecture

### Domain Entity Relationships

**GameWorld Ownership Hierarchy:**
```
GameWorld (single source of truth)
  │
  ├─── Situations (all instances, Dormant through Completed)
  │     └─── Owns spawn rules, requirements, consequences
  │
  ├─── Locations (spatial entities)
  │     └─── References available Situations (placement, not ownership)
  │
  ├─── NPCs (social entities)
  │     └─── References available Situations (placement, not ownership)
  │
  ├─── Obstacles (problem trackers)
  │     └─── Targeted by Situations (optional)
  │
  ├─── Investigations (multi-phase structures)
  │     └─── Spawns Obstacles with Situations
  │
  ├─── Achievements (milestone markers)
  │     └─── Granted by Situation resolution
  │
  ├─── Player (protagonist state)
  │     ├─── Stats (accumulative thresholds)
  │     ├─── Resolve (universal consumable)
  │     ├─── Scales (behavioral reputation)
  │     ├─── States (temporary conditions)
  │     └─── Achievements (earned milestones)
  │
  └─── Routes, Cards, Decks, etc. (existing systems)
```

**Ownership vs Placement vs Reference:**

**Ownership:** GameWorld owns all Situations (lifecycle control). When Situation completes, GameWorld marks it completed. When Situation spawns children, GameWorld adds new instances.

**Placement:** Situations appear at Locations/NPCs for player interaction. Location stores Situation IDs to display when visited, but doesn't control lifecycle.

**Reference:** Situations can target Obstacles (reduce properties), reference parent Situations (spawn chain), reference NPCs (bond changes). These are lookup relationships, not ownership.

### Extended Entities (Already Exist, Being Enhanced)

**Situation (Primary Entity - EXISTS):**

File: `src/GameState/Situation.cs`

**Existing Properties (Keep):**
- Identity: `Id` (string)
- Placement: `PlacementLocationId`, `PlacementNpcId`, `PlacementRouteId` (string)
- Status: `Status` (SituationStatus enum), `IsAvailable`, `IsCompleted` (bool)
- Costs: `Costs` (SituationCosts object: Time, Focus, Stamina, Coins)
- Challenge: `SystemType` (TacticalSystemType enum), `DeckId` (string)
- Victory: `SituationCards` (List<SituationCard>)
- Consequences: `ConsequenceType` (enum), `PropertyReduction` (object)
- Behavior: `DeleteOnSuccess` (bool)

**NEW Properties to Add:**
- Spawn tracking: `TemplateId` (string), `ParentSituationId` (string)
- Spawn timing: `SpawnedDay` (int), `SpawnedTimeBlock` (TimeBlock), `SpawnedSegment` (int)
- Completion timing: `CompletedDay?` (int?), `CompletedTimeBlock?` (TimeBlock?), `CompletedSegment?` (int?)
- Interaction type: `InteractionType` (enum: Instant, Challenge, Navigation)
- Navigation: `NavigationPayload` (object with DestinationId, AutoTriggerScene)
- Requirements: `CompoundRequirement` (object with OR paths)
- Projected consequences: `ProjectedBondChanges`, `ProjectedScaleShifts`, `ProjectedStates` (Lists)
- Spawn rules: `OnSuccessSpawns`, `OnFailureSpawns` (List<SpawnRule>)
- Tier: `Tier` (int 0-4, default 1)
- Repeatability: `Repeatable` (bool, default false)
- Narrative: `GeneratedNarrative` (string, AI cached), `NarrativeHints` (object)

**Extended Resolve Cost:**
- Add to SituationCosts: `Resolve` (int)

### New Entities (Created for Sir Brante Integration)

**Scene (Ephemeral UI Construct - NEW):**

Not stored in GameWorld. Generated per visit from query results.

**C# Structure:**
```csharp
public class Scene
{
    public string LocationId { get; set; }
    public string NpcId { get; set; }  // if NPC scene
    public int VisitNumber { get; set; }
    public List<Situation> AvailableSituations { get; set; } = new List<Situation>();
    public List<Situation> LockedSituations { get; set; } = new List<Situation>();
    public string GeneratedNarrative { get; set; }
    public bool BlocksExit { get; set; } = false;
    public string SceneTemplateId { get; set; }  // if authored scene
}
```

**PlayerScales (Nested Object - NEW):**

Stores player's behavioral reputation across 6 fixed dimensions.

**C# Structure:**
```csharp
public class PlayerScales
{
    public int Morality { get; set; } = 0;        // -10 (Exploitative) to +10 (Altruistic)
    public int Lawfulness { get; set; } = 0;      // -10 (Rebellious) to +10 (Establishment)
    public int Method { get; set; } = 0;          // -10 (Violent) to +10 (Diplomatic)
    public int Caution { get; set; } = 0;         // -10 (Reckless) to +10 (Careful)
    public int Transparency { get; set; } = 0;    // -10 (Secretive) to +10 (Open)
    public int Fame { get; set; } = 0;            // -10 (Notorious) to +10 (Celebrated)
}

// On Player.cs
public PlayerScales Scales { get; set; } = new PlayerScales();
```

**JSON (matches C# exactly):**
```json
"scales": {
  "morality": 0,
  "lawfulness": 0,
  "method": 0,
  "caution": 0,
  "transparency": 0,
  "fame": 0
}
```

**Mechanics:**
- All start at zero (neutral)
- Modified by Situation consequences (+1 to +3 typically)
- Both extremes unlock content (different archetypes, not better/worse)
- Used in CompoundRequirement threshold checks
- Accessed via: `player.Scales.Morality`, `player.Scales.Method`

**ActiveState (State Tracking - NEW):**

**C# Structure:**
```csharp
public enum StateType
{
    // Physical (8 types)
    Wounded, Exhausted, Sick, Injured, Starving, Armed, Provisioned, Rested,
    // Mental (5 types)
    Confused, Traumatized, Inspired, Focused, Obsessed,
    // Social (7 types)
    Wanted, Celebrated, Shunned, Humiliated, Disguised, Indebted, Trusted
    // ~20 total, FIXED list
}

public enum StateCategory
{
    Physical, Mental, Social
}

public class ActiveState
{
    public StateType Type { get; set; }
    public StateCategory Category { get; set; }
    public int AppliedDay { get; set; }
    public TimeBlock AppliedTimeBlock { get; set; }
    public int AppliedSegment { get; set; }
    public int? DurationSegments { get; set; }  // null = manual clear only
}

// On Player.cs
public List<ActiveState> ActiveStates { get; set; } = new List<ActiveState>();
```

**JSON:**
```json
"activeStates": [
  {
    "type": "Wounded",
    "category": "Physical",
    "appliedDay": 1,
    "appliedTimeBlock": "Evening",
    "appliedSegment": 2,
    "durationSegments": 6
  }
]
```

**Mechanics:**
- Dual nature: blocks some Situations, enables others
- Temporary (not permanent flags)
- Maximum 3-4 simultaneous
- Segment-based duration tracking
- Auto-clear or manual clear based on conditions

**PlayerAchievement (Milestone Tracking - NEW):**

**C# Structure:**
```csharp
public enum AchievementCategory
{
    Combat, Social, Investigation, Economic, Political
}

public class PlayerAchievement
{
    public string AchievementId { get; set; }  // Reference to achievement definition
    public int EarnedDay { get; set; }
    public TimeBlock EarnedTimeBlock { get; set; }
    public int EarnedSegment { get; set; }
    public string RelatedNpcId { get; set; }      // Optional
    public string RelatedLocationId { get; set; } // Optional
}

// On Player.cs
public List<PlayerAchievement> EarnedAchievements { get; set; } = new List<PlayerAchievement>();
```

**JSON:**
```json
"earnedAchievements": [
  {
    "achievementId": "first_bond",
    "earnedDay": 2,
    "earnedTimeBlock": "Midday",
    "earnedSegment": 3,
    "relatedNpcId": "martha"
  }
]
```

**Mechanics:**
- Granted by Situation resolution
- Used in CompoundRequirement (compound OR, never sole gate)
- Visible in journal (player identity tracking)
- Enables narrative coherence (AI references specific achievements)

**Compound Requirement (Requirement Structure):**

**OR Path Architecture:**
Each requirement contains multiple OR paths. Player needs to satisfy ANY one complete path.

**Path Components:**
- Stat thresholds: List of stat-threshold pairs
- Bond requirements: List of NPC-strength pairs
- Scale thresholds: List of scale-value-operator triplets
- State requirements: Required states list, blocked states list
- Achievement requirements: List of achievement identifiers
- Economic requirements: Coin threshold

**Example Structure:**
```
Path 1: Bond(Martha) >= 7 AND Morality >= 5
Path 2: Authority >= 12 AND Coins >= 100
Path 3: Achievement(TrustedConfidant) AND Insight >= 10
```

Player satisfying ANY complete path unlocks the Situation.

**Spawn Rule (Spawn Definition):**

Embedded in Situation, defines what spawns on resolution.

**Properties:**
- Target template identifier (what Situation to spawn)
- Target placement (Location/NPC/Route where it appears)
- Requirement offsets (adjustments to base template requirements)
- Spawn conditions (when this rule fires)

**Calculation Logic:**
When spawn executes, takes template requirements and applies offsets based on current player state. Example: Template requires "Bond +2 from current", player has Bond 5, spawned instance requires Bond 7.

### Modified Domain Entities

**Player (Add New State):**

**Existing Properties (Unchanged):**
- Stats (Insight, Rapport, Authority, Diplomacy, Cunning)
- Focus, Stamina, Health (challenge-specific resources)
- Coins (economic resource)
- Time tracking (segments, day)
- Items and equipment

**New Properties:**
- Resolve (universal consumable, 0-30 range)
- Scales (list of Scale entities)
- Active States (list of State entities)
- Achievements (list of Achievement entities)
- Completed Situation identifiers (history tracking)

**Location (Modify Placement):**

**Existing Properties (Unchanged):**
- Identity (name, description, venue reference)
- Investigation cubes (familiarity tracking)
- Obstacle references (problems present here)

**New Properties:**
- Available Situation identifiers (dynamically updated)

**Removed Properties:**
- Static Goal list (replaced by dynamic Situations)

**NPC (Add Relationship State):**

**Existing Properties (Unchanged):**
- Identity (name, personality, portrait)
- Story cubes (tactical challenge benefits)

**New Properties:**
- Bond strength (relationship depth, -10 to +10 range)
- Statuses (qualitative states: Grateful, Betrayed, Allied, Hostile, etc.)
- Available Situation identifiers (dynamically updated)

**Investigation (Minimal Change):**

**Existing Properties (Mostly Unchanged):**
- Title, description, phases
- Phase structure (sequential unlocking)

**Changed Properties:**
- Phase spawned entities: Goals → Situations
- Otherwise identical structure

**Obstacle (No Changes):**

Obstacles remain unchanged. They continue tracking properties (PhysicalDanger, MentalComplexity, SocialDifficulty) that Situations can reduce.

---

## Content Layer Design

### JSON Structure Philosophy

**Template-Based Spawning:**
JSON defines Situation templates, not instances. Templates contain categorical properties and spawn rules. Runtime instantiation creates concrete Situations with calculated requirements based on player state at spawn time.

**Categorical Properties:**
JSON uses relative/categorical descriptions rather than absolute values. This enables AI content generation (AI doesn't know current game balance) and dynamic scaling (same template scales with player progression).

**Separation of Structure and Narrative:**
JSON defines mechanical structure (requirements, costs, spawns). AI generates contextual narrative at encounter time. This prevents combinatorial explosion of pre-authored text while maintaining narrative quality.

### New Content Files

**situations.json (Core Content):**

**Situation Template Definition:**
- Unique template identifier
- Human-readable name
- Interaction type (Instant, Mental, Physical, Social, Navigation)
- Placement type (Location, NPC, Route)
- Target placement identifier (where it appears)

**Requirement Definition (Categorical):**
- Stat thresholds with categorical levels (Capable, Commanding, Masterful)
- Bond requirements with relationship depth categories
- Scale requirements with threshold operators
- State requirements (required and blocked)
- Achievement requirements (list of achievements)
- Compound logic defining OR paths

**Cost Definition:**
- Resolve cost (tier-based: low/medium/high/extreme)
- System cost (tier-based)
- Economic cost (absolute or categorical)
- Time cost (segments)

**Projected Consequences (Visible):**
- Bond changes (NPC and delta)
- Scale shifts (type and delta)
- Resource gains (coins, items)
- State applications (apply or clear)

**Resolution Spawns:**
- On success spawn list
- On failure spawn list (if applicable)
- Each spawn specifies: template, target placement, requirement offsets, conditions

**Challenge Definition (If Challenge Type):**
- Challenge type (Mental, Physical, Social)
- Target Obstacle identifier (optional)
- GoalCard list with thresholds and rewards

**Navigation Definition (If Navigation Type):**
- Destination identifier
- Auto-trigger Scene flag

**Narrative Hints (For AI):**
- Tone (urgent, contemplative, intimate, confrontational)
- Theme (betrayal, loyalty, discovery, sacrifice)
- Context references (past events to mention)

**scales.json (Behavioral Reputation):**

**Scale Type Definitions:**
Each scale defines:
- Type identifier
- Range (minimum to maximum, typically -10 to +10)
- Negative extreme label (player-facing description)
- Positive extreme label (player-facing description)
- Zero-point description (neutral state)

**Six Core Scales:**
- Morality: Exploitative ↔ Altruistic
- Lawfulness: Rebellious ↔ Orthodox
- Method: Violent ↔ Diplomatic
- Caution: Reckless ↔ Careful
- Transparency: Secretive ↔ Open
- Fame: Notorious ↔ Celebrated

**states.json (Temporary Conditions):**

**State Type Definitions:**
Each state defines:
- Type identifier
- Category (Physical, Mental, Social)
- Blocked action types (Situations this state prevents)
- Enabled action types (Situations this state unlocks)
- Clear conditions (rest, treatment, time, specific action)
- Duration (segments, or manual clear)
- Player-facing description

**Achievement Templates:**

**achievements.json:**
- Category classification (Combat, Social, Investigation, Economic, Political)
- Grant conditions (which Situation outcomes grant this)
- Name and description (player-facing)
- Visual representation (icon, color)
- Related entities (NPCs, Locations this references)

**Scene Templates (Optional, Rare):**

**scene_templates.json (Authored Critical Moments):**
For hand-crafted narrative beats at key story moments:
- Trigger conditions (when this Scene fires instead of generated)
- Hand-authored intro narrative
- Situation group (specific Situations presented together)
- Hand-authored resolution narrative
- Exclusive flag (blocks location exit until resolved)

These override generated Scenes for critical story moments while allowing procedural variety elsewhere.

### Modified Existing Content

**player.json (Player Starting State):**

Add starting values:
- Resolve: 30 (full pool)
- Scales: All types at zero (neutral)
- Active States: Empty list
- Achievements: Empty list

**locations.json (Location Definitions):**

Add per location:
- Initial Situations: List of Situation template IDs to spawn on first visit
- Remove: Static Goal lists (no longer used)

**npcs.json (NPC Definitions):**

Add per NPC:
- Starting Bond strength: 0 (neutral)
- Initial Situations: List of Situation template IDs available from first meeting
- Starting Statuses: Empty list

**investigations.json (Investigation Definitions):**

Change per Investigation:
- Phase spawned entities: Goal IDs → Situation template IDs
- Phase structure otherwise identical

---

## Parser & Catalogue Layer

### Parser Responsibilities

**Boundary Translation:**
Parsers exist at the boundary between JSON (data format) and domain entities (runtime objects). They translate categorical properties from JSON into concrete mechanical values using catalogues, enforcing strong typing and validating structure.

**Stateless Pure Functions:**
All parsers are static classes with no side effects. Single-pass conversion from DTO to domain entity. No state storage, no caching, no dependencies beyond catalogues.

**Categorical to Mechanical Translation:**
Parsers call catalogues to translate relative categorical properties (Fragile durability, Capable stat level, High resolve cost) into absolute mechanical values scaled by current game state (player level, difficulty, world progression).

### New Parsers

**SituationParser (Replaces GoalParser):**

**Primary Responsibility:** Parse Situation templates from JSON into domain entities.

**Translation Steps:**
1. Parse DTO from JSON
2. Validate structure (all required fields present, types correct)
3. Translate requirements using RequirementCatalogue (categorical → calculated thresholds)
4. Translate costs using CostCatalogue (tier-based → absolute values)
5. Parse spawn rules (template references, requirement offsets)
6. Parse interaction definition (Challenge or Navigation payloads)
7. Validate references (template IDs, placement IDs exist)
8. Return strongly-typed Situation entity

**Key Translation:** Requirement offsets in spawn rules remain categorical. They're calculated at spawn time (not parse time) using current player state.

**ScaleParser:**

**Responsibility:** Parse scale type definitions and initialize player scales.

**Operations:**
- Parse scale type configurations from JSON
- Create Scale entities for player starting state
- Validate range definitions and label consistency

**StateParser:**

**Responsibility:** Parse state type definitions and create state catalogues.

**Operations:**
- Parse state configurations from JSON
- Create state type enumeration
- Define blocked/enabled action mappings
- Define clear condition rules

**AchievementParser:**

**Responsibility:** Parse achievement templates.

**Operations:**
- Parse achievement definitions
- Create achievement type catalogue
- Validate grant conditions reference valid Situations

### New Catalogues

**RequirementCatalogue (Categorical to Calculated Translation):**

**Purpose:** Translate categorical requirement descriptors into calculated numerical thresholds scaled by player progression.

**Translation Functions:**

**Stat Threshold Calculation:**
Input: Categorical level (Capable, Commanding, Masterful) + Player level
Output: Numerical threshold
Logic: Same categorical level requires higher stats at higher player levels
- Capable at Level 1 → Stat 8
- Capable at Level 5 → Stat 12 (scaled up for progression)
- Masterful at Level 1 → Stat 15
- Masterful at Level 5 → Stat 20

**Bond Requirement Calculation:**
Input: Relationship depth category (Acquaintance, Trusted, Deep) + NPC history
Output: Numerical bond strength threshold
Logic: Adjusts thresholds based on NPC personality and relationship volatility

**Compound Requirement Building:**
Input: Requirement rules from JSON (OR path definitions)
Output: Compound requirement structure (typed object with all paths)
Logic: Parses boolean logic, creates path alternatives, validates completeness

**CostCatalogue (Tier-Based Scaling):**

**Purpose:** Scale costs based on Situation tier and player progression.

**Scaling Functions:**

**Resolve Cost Calculation:**
Input: Cost tier (Low, Medium, High, Extreme) + Situation tier + Player level
Output: Absolute resolve cost
Logic: Higher tiers cost more, higher player levels cost more (scales with access to resources)
- Low/Tier1/Level1 → 2 Resolve
- Low/Tier1/Level5 → 3 Resolve
- Extreme/Tier4/Level1 → 18 Resolve
- Extreme/Tier4/Level5 → 25 Resolve

**System Cost Calculation:**
Input: Cost tier + Challenge type + Player level
Output: Absolute Focus/Stamina cost
Logic: Scales similarly to Resolve but type-specific

**Economic Cost Calculation:**
Input: Cost tier + Player wealth level
Output: Absolute coin cost
Logic: Scales with player economic progression (same perceived burden)

**ConsequenceCatalogue (Consequence Typing):**

**Purpose:** Define consequence magnitude categories and display formatting.

**Functions:**

**Bond Change Significance:**
Input: Delta magnitude
Output: Significance category (Slight, Moderate, Deep, Transformative)
Logic: Used for UI display priority and narrative generation hints

**Scale Shift Category:**
Input: Scale type + Delta
Output: Categorical description
Logic: "Shifted toward Altruism" vs "Became significantly more Altruistic"

**Consequence Display Formatting:**
Input: Consequence object
Output: Player-facing text
Logic: Formats changes for Consequence Modal display

### Modified Parsers

**InvestigationParser (Minimal Change):**

**Changed Behavior:**
- Parse Situation template IDs instead of Goal IDs in phase definitions
- Otherwise identical to current implementation

**LocationParser (Add Initial Situations):**

**Changed Behavior:**
- Parse initial Situation template ID list
- Remove static Goal parsing (no longer exists)
- Add available Situation ID list (initialized empty, populated at runtime)

**NPCParser (Add Relationship State):**

**Changed Behavior:**
- Parse starting Bond strength (typically 0)
- Parse initial Situation template ID list
- Add available Situation ID list (initialized empty)

---

## Domain Logic Layer

### Facade Architecture Philosophy

**Separation of Concerns:**
Each facade handles one domain responsibility. Facades coordinate between GameWorld and UI, never storing state themselves. All state lives in GameWorld (single source of truth).

**Synchronous Operations:**
Facades perform synchronous operations on GameWorld. No async/await except for AI calls (narrative generation). This maintains deterministic state changes.

**Single Direction Data Flow:**
UI calls Facades → Facades modify GameWorld → Facades return results → UI displays results. No reverse dependencies (GameWorld never calls UI, Facades never call UI).

### New Facades

**SceneFacade (Scene Generation):**

**Core Responsibility:** Generate Scene instances when player interacts with Locations/NPCs/Routes.

**Primary Operation: Generate Location Scene**

Input: Location identifier, Player state
Process:
1. Query Location's available Situation identifiers
2. Retrieve Situation entities from GameWorld
3. Evaluate each Situation's requirements against current Player state
4. Categorize as Available (all requirements met) or Locked (missing requirements)
5. Generate intro narrative via AI (pass player context: achievements, bonds, scales, states, recent history)
6. Check for Scene template trigger (authored Scene override)
7. Categorize Available Situations by type (Urgent, Progression, Relationship, Opportunity, Exploration)
8. Identify urgent Situations (time-sensitive, critical states)
9. Determine if Scene blocks exit (rare, forced resolution Scenes)
Output: Scene object (ephemeral, contains narrative + categorized Situations)

**Secondary Operation: Generate NPC Scene**

Similar flow but queries NPC's available Situations, focuses on relationship context, generates greeting narrative referencing Bond level and history.

**Tertiary Operation: Regenerate Scene**

Called after Situation resolution to refresh Scene display. Shows newly spawned Situations, updates narrative to reflect consequences.

**SituationFacade (Situation Lifecycle):**

**Core Responsibility:** Handle Situation selection, validation, resolution routing, and completion.

**Primary Operation: Select Situation**

Input: Situation identifier, Player state
Process:
1. Retrieve Situation from GameWorld
2. Re-validate requirements (prevent race conditions from state changes)
3. Validate costs (player can afford Resolve, coins, etc.)
4. Deduct costs from Player resources
5. Update Situation status to Active
6. Route based on interaction type:
   - Instant → Resolve immediately
   - Challenge → Initiate challenge
   - Navigation → Execute movement
Output: Selection result (success/failure + routing information)

**Instant Resolution:**

Process:
1. Apply projected consequences via ConsequenceFacade
2. Update Player stats, bonds, scales, states
3. Grant achievements if defined
4. Trigger spawn execution via SpawnFacade
5. Mark Situation as Completed
6. Generate resolution narrative via AI
Output: Resolution summary (consequences + spawned Situations)

**Challenge Initiation:**

Process:
1. Create challenge context (type, target obstacle, GoalCards)
2. Pass to existing ChallengeFacade
3. ChallengeFacade handles tactical gameplay (no changes to existing system)
Output: Challenge context (for UI to launch challenge screen)

**Challenge Completion:**

Input: Situation identifier, Challenge result (from ChallengeFacade)
Process:
1. Apply GoalCard tactical rewards (coins, stat XP, bond changes from card effects)
2. Apply Situation strategic consequences (scales, states, achievements)
3. Trigger spawns based on outcome (success or failure spawn rules)
4. Mark Situation as Completed
5. Generate resolution narrative
Output: Resolution summary

**Navigation Resolution:**

Process:
1. Apply consequences
2. Update Player current location to destination
3. Trigger spawns at destination
4. Trigger destination Scene generation if auto-trigger enabled
Output: Navigation result (new location + Scene if triggered)

**SpawnFacade (Cascade Execution):**

**Core Responsibility:** Execute spawn rules, create new Situation instances, manage spawn caps and cooldowns.

**Primary Operation: Execute Spawns**

Input: Parent Situation, Outcome (success/failure), Player state
Process:
1. Retrieve spawn rules for outcome from parent Situation
2. For each spawn rule:
   a. Retrieve target template from GameWorld
   b. Instantiate new Situation from template
   c. Calculate requirements (apply offsets from spawn rule using current Player state)
   d. Set status to Dormant (not yet available)
   e. Set parent reference to parent Situation identifier
   f. Add to GameWorld Situations collection
   g. Check if immediately Available (requirements already met)
   h. If Available, add to target Location/NPC available list
3. Apply spawn caps (max per location, max total, archetype cooldowns)
4. Replace lowest-priority Available Situations if at cap
Output: List of spawned Situations (includes status and location)

**Requirement Calculation Example:**
```
Template requirement: Bond(NPC_from_parent) >= current_bond + 3
Parent Situation involved NPC_Martha
Player current Bond(Martha): 5
Calculated requirement: Bond(Martha) >= 8

Template requirement: Stat_offset(Insight) + 2
Player current Insight: 10
Calculated requirement: Insight >= 12
```

This creates dynamic requirements that scale with player progression.

**Secondary Operation: Evaluate Dormant Situations**

Called after Player stat/bond/scale changes (consequence application).

Input: Player state
Process:
1. Query all Dormant Situations from GameWorld
2. For each Dormant Situation:
   a. Evaluate compound requirements against current Player state
   b. If any OR path satisfied, update status to Available
   c. Add to appropriate Location/NPC available Situation list
Output: List of newly Available Situations

**ConsequenceFacade (State Change Application):**

**Core Responsibility:** Apply consequences to Player and NPCs, trigger cascading evaluations.

**Primary Operation: Apply Consequences**

Input: Consequence definition, Player state
Process:
1. Update Player resources (Resolve, Coins, Focus, Stamina, Health)
2. Modify NPC Bonds (update Player Bond list, update NPC Bond strength)
3. Shift Player Scales (add deltas, clamp to -10/+10 range)
4. Apply or clear States (add to Player active states or remove)
5. Grant Achievements (add to Player achievement list)
6. Trigger SpawnFacade evaluate Dormant (newly met requirements unlock Situations)
Output: Consequence summary (all changes for display)

**Display Formatting:**

Process:
1. Format resource changes ("Gained 50 Coins (now 120)", "Lost 8 Resolve (now 12)")
2. Format bond changes ("Bond with Martha increased: 5 → 7")
3. Format scale shifts ("Morality +2 (now 8 - Altruistic)")
4. Format state changes ("Gained State: Exhausted (clear by resting)")
5. Format achievements ("Achievement Earned: Defended the Mill")
Output: Formatted text for Consequence Modal display

### Modified Facades

**GameFacade (Orchestration):**

**New Operations:**

**Enter Location:**
Input: Location identifier
Process: Call SceneFacade.GenerateLocationScene
Output: Scene object for UI display

**Interact with NPC:**
Input: NPC identifier
Process: Call SceneFacade.GenerateNPCScene
Output: Scene object for UI display

**Select Situation:**
Input: Situation identifier
Process: Call SituationFacade.SelectSituation, route to appropriate resolution
Output: Selection result

**Existing Operations (Unchanged):**
- All Challenge handling (ChallengeFacade integration)
- Time management (segment tracking, day advancement)
- Travel system (Route navigation)
- Resource management (existing patterns)

**ChallengeFacade (Minimal Integration Change):**

**Modified Operations:**

**Launch Challenge:**
Input: Situation Challenge definition (instead of Goal)
Process: Identical to existing implementation
Output: Challenge state

**Resolve Challenge:**
Input: Challenge result
Process: Identical to existing implementation
Output: GoalCard rewards + challenge outcome (success/failure)

Integration point: Returns to SituationFacade for strategic consequence application.

---

## UI Component Architecture

### Component Hierarchy Philosophy

**Authoritative Parent Pattern:**
GameScreen remains authoritative. All child components receive state via cascading parameters and communicate upward via direct parent method calls (no complex event chains).

**Dumb Display Components:**
UI components contain zero game logic. They display data and forward user actions to parent/facade. All logic lives in facades and GameWorld.

**State Flow:**
GameScreen → Child Components (one-way data flow)
User Action → Child Component → Parent Method Call → Facade → GameWorld Update → State Change → Re-render

### New UI Components

**LocationSceneScreen (Replaces Current Location Screen):**

**Responsibility:** Display Scene for Location interaction.

**Layout Structure:**
```
[Location Header: Name, Time, Resources Display]
  ↓
[Scene Narrative Section]
  Generated intro narrative (3-5 paragraphs)
  References player achievements, relationships, states
  Sets contextual atmosphere
  ↓
[Situations Section]
  [Category Tabs: Urgent | Progression | Relationship | Opportunity | Exploration]
  Each tab shows relevant Situations
  Each tab collapsible
    ↓
    [Situation Card] (multiple, categorized)
    [Situation Card]
    [Situation Card]
  ↓
[Exit Location Button]
```

**User Interaction Flow:**
1. User clicks Situation Card (expands details)
2. User reviews requirements, costs, consequences
3. User clicks Select (if Available) or views unlock paths (if Locked)
4. System validates and resolves
5. Consequence Modal appears
6. User chooses: Stay (regenerate Scene) or Leave (exit to map)

**State Management:**
- Receives Scene object from parent (GameScreen)
- No local game state
- Forwards selection to parent SelectSituation method

**SituationCard (Choice Display):**

**Responsibility:** Display individual Situation (Sir Brante-style choice).

**Layout Structure:**
```
[Title Bar]
  Situation Name | Interaction Type Icon | Status Badge
  ↓
[Requirements Section] (if Locked)
  "Unlock Paths:"
  Path 1: [Progress Bar] Bond(Martha) ✓ | Morality ✗ [1/2]
  Path 2: [Progress Bar] Insight ✓ | Coins ✓ [2/2] ← Closest!
  Path 3: [Progress Bar] Achievement ✗ | Authority ✗ [0/2]

  [How to unlock button] (links to relevant actions)
  ↓
[Costs Section]
  Icons: Resolve 8 | Focus 3 | Coins 20 | Time 1 segment
  ↓
[Projected Consequences Section]
  "If successful:"
  • Bond with Martha +2 (will reach 7)
  • Morality +1 (will reach 8 - Altruistic)
  • Gain 50 Coins
  • Unlock 2 new situations at Mill, 1 at Town Square
  ↓
[Description]
  Situation narrative text (2-3 sentences)
  Generated or from template hints
  ↓
[Select Button] (if Available, enabled)
[Locked Icon + Requirements] (if Locked, disabled)
```

**Visual States:**
- Available: Green highlight, clickable, hover effects
- Locked: Gray, shows requirements, tooltip with details
- Urgent: Red highlight, pulsing border, high priority indicator
- Selected: Expanded accordion, shows full details
- Active: Dimmed (after selection, before resolution)

**Interaction:**
- Click to expand/collapse
- Select button calls parent SelectSituation
- Requirement paths clickable (links to relevant actions)

**ConsequenceModal (Resolution Display):**

**Responsibility:** Show immediate consequence feedback (Sir Brante pattern).

**Layout Structure:**
```
[Modal Header: "Resolution"]
  ↓
[Narrative Section]
  Generated resolution narrative (2-3 paragraphs)
  Describes what happened
  References player choices and NPC reactions
  ↓
[Changes Section]
  "Your actions had consequences:"

  [Resources Subsection]
    ✓ Gained 50 Coins (now 120)
    ✗ Lost 8 Resolve (now 12)
    ✗ Lost 3 Focus (now 5)

  [Relationships Subsection]
    ✓ Bond with Martha increased: 5 → 7
    ✗ Bond with Town Guard decreased: 3 → 1

  [Reputation Subsection]
    ✓ Morality +2 (now 8 - Altruistic)
    ✓ Method -1 (now 4 - Pragmatic)

  [Conditions Subsection]
    ✗ Gained State: Exhausted (clear by resting)
    ✓ Cleared State: Wounded

  [Milestones Subsection]
    ⭐ Achievement Earned: "Defended the Mill"
  ↓
[Spawned Situations Section]
  "New opportunities available:"

  • "Martha's Secret" at Old Mill [Available Now]
  • "Track Strangers" at Forest Edge [Locked: Need Insight 8, you have 6]
  • "Report to Guard" at Town Square [Available Now]

  [Location links clickable for navigation]
  ↓
[Continue Button]
  Options via button group:
    [Stay Here] (regenerate Scene)
    [Leave] (return to map)
```

**Interaction:**
- Modal overlay (blocks other interaction)
- Shows after any Situation resolution
- Continue choice determines next action (stay/leave)

**RequirementPathDisplay (Requirement Visualization):**

**Responsibility:** Visual display of compound OR requirements with progress tracking.

**Layout Structure:**
```
[Path List]
  Path 1: [Progress Bar 60%] "Deep Trust Path"
    Requirements:
      ✓ Bond(Martha) >= 7 (you have 8)
      ✗ Morality >= 5 (you have 3)
    Missing: Morality +2
    [Show How to Gain Morality]

  Path 2: [Progress Bar 0%] "Authority Path"
    Requirements:
      ✗ Authority >= 12 (you have 9)
      ✗ Coins >= 100 (you have 45)
    Missing: Authority +3, Coins +55

  Path 3: [Progress Bar 50%] "Achievement Path"
    Requirements:
      ✗ Achievement: TrustedConfidant (not earned)
      ✓ Insight >= 10 (you have 11)
    Missing: Achievement
    [Show How to Earn Achievement]

Closest Path: Path 1 (1/2 requirements, 60%)
```

**Interaction:**
- Expandable/collapsible path details
- "Show How" buttons link to relevant Situations
- Progress bars give quick visual status
- Closest path highlighted

**CategoryTabs (Situation Organization):**

**Responsibility:** Organize Situations into collapsible categories.

**Tab Structure:**
```
[Urgent Tab] (count: 2) [Red badge]
  [Situation Card - State.Wounded clear]
  [Situation Card - Time-sensitive event]

[Progression Tab] (count: 3) [Gold badge]
  [Situation Card - Investigation advance]
  [Situation Card - Obligation step]
  [Situation Card - Mystery resolution]

[Relationship Tab] (count: 1) [Blue badge]
  [Situation Card - Martha conversation]

[Opportunity Tab] (count: 2) [Green badge]
  [Situation Card - Work for coins]
  [Situation Card - Trade offer]

[Exploration Tab] (count: 1) [Purple badge]
  [Situation Card - Investigate tracks]
```

**Default Behavior:**
- Show Urgent + Progression expanded by default
- Collapse Relationship, Opportunity, Exploration (reveal on click)
- Badge counts show total Situations in category

**Interaction:**
- Click tab to expand/collapse
- Color coding for quick category identification
- Counts update dynamically

**NPCSceneScreen (NPC Interaction):**

**Responsibility:** Display Scene for NPC conversation.

**Layout Differences from Location Scene:**
- Shows NPC portrait and character info
- Displays Bond meter (visual representation of relationship depth)
- Greeting narrative references relationship history and Bond level
- Situations focused on conversation/relationship types
- Shows NPC statuses (Grateful, Allied, Suspicious, etc.)

**Otherwise Similar Structure:**
Same Scene narrative + Situation list + Consequence Modal pattern

### Modified Existing Components

**WorldMapScreen (Add Situation Indicators):**

**New Visual Elements:**
- Location status display: "Old Mill (3 available, 2 locked, 1 urgent)"
- Urgent indicator: Red pulsing border if location has urgent Situations
- Available count badge: Green number showing Available Situations
- Locked count badge: Gray number showing Locked Situations

**Changed Interaction:**
- Click location → GameFacade.EnterLocation → Navigate to LocationSceneScreen
- Tooltip shows Situation counts on hover

**JournalScreen (Expand Tracking):**

**New Sections:**

**Active Situations Tab:**
- Organized by location
- Shows Available Situations player can pursue
- Links to locations for navigation

**Dormant Situations Tab:**
- Shows Locked Situations with requirements
- Progress bars showing how close to unlocking
- Links to actions that help meet requirements

**Completed Situations Tab:**
- History log of finished Situations
- Organized chronologically or by location
- Shows cascade chains (parent → child relationships)

**Achievements Tab:**
- Earned achievements displayed
- Organized by category
- Shows earned time (Day, TimeBlock, Segment)

**Scales Tab:**
- Current Scale values with labels
- Visual spectrum representation (-10 to +10)
- Explanation of what each Scale unlocks

**Active States Tab:**
- Current temporary conditions
- Shows clear conditions
- Time remaining (if duration-based)

**GameScreen (Authoritative State):**

**New State Properties:**
- Current Scene (if at location/NPC)
- Consequence Modal visibility
- Consequence Modal data (current summary)

**New Routes:**
- LocationScene route (displays LocationSceneScreen)
- NPCScene route (displays NPCSceneScreen)

**New Methods:**
- EnterLocation (generates Scene, navigates to screen)
- InteractWithNPC (generates Scene, navigates to screen)
- SelectSituation (calls facade, handles resolution)
- ShowConsequenceModal (displays consequence summary)
- RegenerateScene (refreshes Scene after choice)

---

## Data Flow Patterns

### Complete Flow Examples

**Flow 1: Location Entry and Scene Generation**

```
User Action:
  Player clicks "Old Mill" on world map

UI Layer:
  WorldMapScreen detects click
  Calls parent: GameScreen.EnterLocation("mill")

Frontend (GameScreen):
  Calls GameFacade.EnterLocation("mill")

Facade Layer (GameFacade):
  Updates GameWorld.Player.CurrentLocationId = "mill"
  Calls SceneFacade.GenerateLocationScene("mill", player)

Facade Layer (SceneFacade):
  1. Queries GameWorld.Locations["mill"].AvailableSituationIds
  2. Retrieves Situation entities from GameWorld.Situations
  3. For each Situation:
     Evaluates compound requirements against Player state
     Categorizes as Available or Locked
  4. Generates intro narrative:
     Queries Player context (achievements, bonds, scales, states, recent)
     Calls AI with context + location + visit number
     Receives generated narrative, caches in Scene
  5. Checks for Scene template trigger (authored Scene)
  6. Categorizes Available Situations:
     Urgent (time-sensitive, state-critical)
     Progression (investigation/obligation advance)
     Relationship (bond building)
     Opportunity (economic/resource)
     Exploration (discovery/observation)
  7. Returns Scene object

GameWorld Layer:
  Read-only queries (no state changes)

Frontend Receives:
  Scene {
    narrative: "You return to the mill...",
    available: [situation1, situation2, situation3],
    locked: [situation4, situation5],
    categories: {urgent: [], progression: [situation1], ...}
  }

UI Layer:
  GameScreen navigates to LocationSceneScreen route
  LocationSceneScreen receives Scene object
  Renders Scene narrative + categorized Situation Cards
  Player sees Sir Brante-style choice presentation
```

**Flow 2: Instant Situation Resolution**

```
User Action:
  Player clicks "Work at Mill" Situation Card
  Clicks Select button

UI Layer:
  SituationCard detects click
  Calls parent: LocationSceneScreen.SelectSituation("work_mill_01")
  LocationSceneScreen forwards to GameScreen.SelectSituation("work_mill_01")

Frontend (GameScreen):
  Calls GameFacade.SelectSituation("work_mill_01")

Facade Layer (GameFacade):
  Calls SituationFacade.SelectSituation("work_mill_01", player)

Facade Layer (SituationFacade):
  1. Retrieves Situation from GameWorld
  2. Re-validates requirements (prevent race conditions)
  3. Validates costs: Resolve >= 2, Check passes
  4. Deducts costs:
     Player.Resolve: 30 → 28
  5. Updates Situation.Status: Available → Active
  6. Routes to Instant resolution (InteractionType.Instant)

  Calls ResolveInstantSituation(situation, player)

Facade Layer (SituationFacade - Instant Resolution):
  Calls ConsequenceFacade.ApplyConsequences(situation.consequences, player)

Facade Layer (ConsequenceFacade):
  1. Applies resource changes:
     Player.Coins: 70 → 120 (+50)
  2. Applies stat XP:
     Player.Insight XP increases (toward next level)
  3. Applies scale shifts:
     Player.Scales["Method"]: 3 → 4 (+1)
  4. Applies bond changes:
     NPC["martha"].BondStrength: 5 → 6 (+1)
  5. Calls SpawnFacade.EvaluateDormant(player)

Facade Layer (SpawnFacade - Dormant Evaluation):
  1. Queries all Dormant Situations
  2. Finds "martha_request_01" (was Dormant)
  3. Requirements: Bond(Martha) >= 6
  4. Player Bond(Martha) now 6 ✓
  5. Updates Status: Dormant → Available
  6. Adds to Location["mill"].AvailableSituationIds
  7. Returns ["martha_request_01"] (newly Available)

Facade Layer (ConsequenceFacade - Continues):
  Returns consequence summary {
    resources: {coins: +50},
    stats: {insight: +xp},
    bonds: {martha: +1},
    scales: {method: +1},
    newlyAvailable: ["martha_request_01"]
  }

Facade Layer (SituationFacade - Continues):
  Calls SpawnFacade.ExecuteSpawns(situation, "success", player)

Facade Layer (SpawnFacade):
  1. Retrieves spawn rules from situation.OnSuccessSpawns
  2. Spawn rule: "busy_mill" template at "mill"
  3. Instantiates Situation from template
  4. Calculates requirements:
     Template: "Requires current_coins - 50"
     Player.Coins: 120
     Calculated: Coins >= 70
  5. Status: Dormant (requirement met, but cooldown active)
  6. Adds to GameWorld.Situations
  7. Returns spawned list

Facade Layer (SituationFacade - Continues):
  Marks Situation.Status: Active → Completed
  Generates resolution narrative via AI:
    Context: work completed, earned coins, Martha relationship
    AI generates contextual outcome text
  Returns Resolution {
    narrative: "A productive day's work...",
    consequences: summary,
    spawned: [{id, location, status}]
  }

GameWorld Layer:
  Changes applied:
    Player.Resolve: 30 → 28
    Player.Coins: 70 → 120
    Player.Scales["Method"]: 3 → 4
    NPC["martha"].BondStrength: 5 → 6
    Situation["work_mill_01"].Status: Completed
    Situation["martha_request_01"].Status: Dormant → Available
    Situation["busy_mill_01"] added (Dormant)

Frontend Receives:
  Resolution object with all changes

UI Layer:
  GameScreen shows ConsequenceModal
  Displays:
    Narrative: "A productive day's work..."
    Resources: +50 Coins (now 120), -2 Resolve (now 28)
    Relationships: Bond(Martha) +1 (now 6)
    Reputation: Method +1 (now 4)
    Spawned: "Martha's Request" at Mill [Available Now]
            "Busy Mill" at Mill [Locked: Need 70 Coins]

  User clicks Continue
  Options: Stay or Leave

  If Stay:
    GameScreen calls SceneFacade.RegenerateScene("mill", player)
    New Scene shows "Martha's Request" as Available

  If Leave:
    GameScreen navigates to WorldMapScreen
```

**Flow 3: Challenge Situation Resolution**

```
User Action:
  Player selects "Investigate Tracks" (Mental Challenge type)

UI → Frontend → Facade (Same validation as Instant):
  SituationFacade.SelectSituation validates, deducts costs, routes to Challenge

Facade Layer (SituationFacade):
  Calls InitiateChallengeSituation(situation, player)

  Creates ChallengeContext {
    type: Mental,
    deck: Investigation,
    targetObstacle: "mill_mystery_01",
    goalCards: situation.challenge.goalCards
  }

  Calls ChallengeFacade.LaunchChallenge(context, player)

Facade Layer (ChallengeFacade):
  Existing Mental challenge system (unchanged)
  Returns Challenge state for UI

GameWorld Layer:
  Player.Resolve decreased
  Player.Focus decreased
  Situation.Status: Active
  ActiveChallenge created

UI Layer:
  GameScreen navigates to MentalChallengeScreen
  Player plays challenge (existing UI, no changes)

  Player achieves GoalCard (threshold 12 Breakthrough)
  Challenge resolves successfully

Facade Layer (ChallengeFacade):
  Returns ChallengeResult {
    outcome: success,
    goalCardIndex: 1,
    goalCardRewards: {coins: +20, insightXP: +5}
  }

Frontend:
  Calls SituationFacade.CompleteChallengeSituation(situation, result)

Facade Layer (SituationFacade):
  1. Applies GoalCard tactical rewards (coins, XP)
  2. Applies Situation strategic consequences (scales, states, achievements)
  3. Calls ConsequenceFacade.ApplyConsequences
  4. Calls SpawnFacade.ExecuteSpawns
  5. Marks Situation: Completed
  6. Generates resolution narrative
  7. Returns Resolution

GameWorld Layer:
  All changes from GoalCard + Situation consequences
  Spawned Situations added

UI Layer:
  ConsequenceModal shows combined results
  (GoalCard rewards + Situation consequences + spawns)
  User continues (stay/leave)
```

**Flow 4: Cascading Spawn Chain**

```
Context:
  User completed "Repair Mill Wheel" (parent Situation)
  Situation defines OnSuccessSpawns: "martha_gratitude" template

SpawnFacade.ExecuteSpawns:
  1. Parent: "repair_wheel" just completed
  2. Outcome: success
  3. Spawn rules from parent:
     Rule: {
       template: "martha_gratitude",
       target: "mill",
       requirementOffsets: {
         bond_martha: current + 2,
         morality: current + 0
       }
     }

  4. Retrieves template "martha_gratitude" from GameWorld
  5. Current player state:
     Bond(Martha): 5
     Morality: 6

  6. Calculates requirements:
     Template base + offsets:
       Bond(Martha) >= 5 + 2 = 7
       Morality >= 6 + 0 = 6

  7. Creates Situation instance:
     Id: "martha_gratitude_instance_001"
     TemplateId: "martha_gratitude"
     TargetLocationId: "mill"
     Requirements: Bond(Martha) >= 7, Morality >= 6
     Status: Dormant (Bond requirement not met yet)
     ParentSituationId: "repair_wheel"

  8. Adds to GameWorld.Situations
  9. Returns spawned list (status: Dormant)

Later Event:
  User completes "Talk to Martha" Situation
  ConsequenceFacade applies: Bond(Martha) +2 (5 → 7)

ConsequenceFacade:
  Calls SpawnFacade.EvaluateDormant after bond change

SpawnFacade.EvaluateDormant:
  1. Queries all Dormant Situations
  2. Finds "martha_gratitude_instance_001"
  3. Evaluates requirements:
     Bond(Martha) >= 7: Player has 7 ✓
     Morality >= 6: Player has 6 ✓
  4. All requirements met!
  5. Updates Status: Dormant → Available
  6. Adds to Location["mill"].AvailableSituationIds
  7. Returns newly Available list

Next Visit to Mill:
  SceneFacade.GenerateLocationScene queries Location
  Finds "martha_gratitude_instance_001" in available list
  Displays in Scene as Available choice

  AI narrative generation receives context:
    Parent: "repair_wheel" (completed 2 days ago)
    Bond: 7 (Deep Trust)

  Generates:
    "Martha approaches with genuine warmth. The gratitude from
     your wheel repair two days ago is still evident in her eyes.
     She has something important to share."
```

**Flow 5: Compound OR Requirement Evaluation**

```
Situation:
  "Confront Martha about Theft"

Requirements (Compound OR):
  Path 1: {
    bonds: [{martha: 7}],
    scales: [{morality: 5, operator: ">="}]
  }
  Path 2: {
    stats: [{authority: 12}],
    coins: 100
  }
  Path 3: {
    achievements: ["TrustedConfidant"],
    stats: [{insight: 10}]
  }

Player State:
  Bond(Martha): 8
  Scale.Morality: 3
  Authority: 9
  Coins: 45
  Insight: 11
  Achievements: []

Evaluation (in SituationCard.RequirementPathDisplay):
  Path 1 Evaluation:
    Bond(Martha) >= 7: 8 >= 7 ✓ MET
    Morality >= 5: 3 >= 5 ✗ MISSING (need +2)
    Path Complete: ✗ (1/2 requirements, 50%)

  Path 2 Evaluation:
    Authority >= 12: 9 >= 12 ✗ MISSING (need +3)
    Coins >= 100: 45 >= 100 ✗ MISSING (need +55)
    Path Complete: ✗ (0/2 requirements, 0%)

  Path 3 Evaluation:
    Achievement TrustedConfidant: ✗ MISSING (not earned)
    Insight >= 10: 11 >= 10 ✓ MET
    Path Complete: ✗ (1/2 requirements, 50%)

Overall Result:
  Status: LOCKED (no complete path satisfied)
  Closest Path: Path 1 (50% complete, 1/2 met)
  Missing: Morality +2

UI Display:
  "Confront Martha" [LOCKED]

  Unlock Paths (3 alternatives):

  Path 1: "Deep Trust Path" [50% complete]
    Progress: [========----------] 1/2
    ✓ Bond(Martha) >= 7 (you have 8)
    ✗ Morality >= 5 (you have 3)
    Missing: Morality +2
    [Show How to Gain Morality]

  Path 2: "Authority Path" [0% complete]
    Progress: [--------------------] 0/2
    ✗ Authority >= 12 (you have 9)
    ✗ Coins >= 100 (you have 45)
    Missing: Authority +3, Coins +55

  Path 3: "Achievement Path" [50% complete]
    Progress: [========----------] 1/2
    ✗ Achievement: TrustedConfidant (not earned)
    ✓ Insight >= 10 (you have 11)
    Missing: Achievement

  Closest: Path 1 (need Morality +2)

User Interaction:
  Clicks "Show How to Gain Morality"
  System searches Available Situations with Morality-positive consequences
  Displays list: "Help without payment" (+2 Morality)
              "Donate to poor" (+1 Morality)
              "Forgive debt" (+3 Morality)
```

---

## AI Integration Strategy

### AI Call Points

**Three Primary Integration Points:**

**Scene Narrative Generation:**
When: Location entry, NPC interaction
Purpose: Create contextual intro narrative referencing player history
Frequency: Once per Scene instance (cached for display duration)

**Consequence Narrative Generation:**
When: Situation resolution
Purpose: Describe what happened and immediate reactions
Frequency: Once per Situation completion

**Requirement Explanation Generation:**
When: Player hovers locked Situation
Purpose: Explain in-fiction why requirement makes sense
Frequency: On-demand (tooltip display)

### Context System Architecture

**Player State Context:**

**Identity Components:**
- All earned Achievements (especially recent and location-relevant)
- Scale values with categorical labels (Altruistic, Violent, etc.)
- Active States with descriptive labels (Wounded, Exhausted, etc.)
- Stat values for capability references (without exact numbers)

**Relationship Context:**
- Bond strengths with relevant NPCs (numerical values + categorical: Stranger, Acquaintance, Trusted, Deep)
- NPC Statuses (Grateful, Betrayed, Allied, Hostile)
- Story Cubes with NPCs (relationship progression depth)
- Past Situations completed involving these NPCs

**Environmental Context:**
- Location properties and familiarity level (Investigation Cubes)
- Current Obstacles present
- Completed Situations at this location (history)
- Visit number (first visit vs returning)

**Temporal Context:**
- Current game day and segment
- Active Obligations with deadlines
- Recent Situation completions (last 5-10)
- Time since last visit to location

### Prompt Engineering Structure

**Scene Narrative Prompt Template:**

```
System Role:
You are a narrative generator for a low-fantasy medieval RPG. Generate immersive,
grounded narratives that reference player history and current state.

Context Data:
Location: {location.name} ({location.description})
Visit Number: {visitCount} (first visit / returning)
Familiarity: {location.investigationCubes}/10

Player State:
- Achievements: {recentAchievements relevant to location}
- Relationships: {bondStrengths with NPCs at location}
- Reputation: {scaleValues with categorical labels}
- Conditions: {activeStates with descriptions}
- Recent Actions: {last5CompletedSituations}

Available Choices:
{situationTitles that will be presented}

Task:
Generate 3-5 paragraph entry narrative for this location visit.

Requirements:
1. Reference specific achievements when relevant (not generic)
2. Explain NPC reactions based on bond levels and past events
3. Describe atmosphere influenced by player reputation (scales)
4. Acknowledge active states (wounded, exhausted) in description
5. Set up available choices naturally without explicit listing
6. Maintain verisimilitude (grounded, realistic, no anachronisms)
7. Use second-person present tense ("you approach...")

Avoid:
- Exact mechanical numbers ("you have 8 Insight")
- Meta-game terminology ("this is a Tier 3 Situation")
- Contradicting previous narratives
- Generic placeholder text

Tone: {sceneTemplate.tone or default: contemplative, grounded}
Setting: Low-fantasy medieval, realistic consequences
```

**Consequence Narrative Prompt Template:**

```
System Role:
Generate resolution narrative for player choice outcome.

Situation Context:
Choice: {situation.name}
Type: {situation.interactionType}
Outcome: {success or failure}

Consequences Applied:
Resources: {resourceChanges formatted}
Relationships: {bondChanges formatted}
Reputation: {scaleShifts formatted}
Conditions: {stateChanges formatted}
Achievements: {achievementsEarned}

Spawned Opportunities:
{spawnedSituations with locations}

Task:
Generate 2-3 paragraph resolution narrative.

Requirements:
1. Describe what the player character did
2. Show immediate NPC/environment reactions
3. Reference consequence changes naturally (no exact numbers)
4. Hint at spawned opportunities without spoiling content
5. Match tone to situation archetype

Tone: {situation.narrativeHints.tone}
Theme: {situation.narrativeHints.theme}
```

**Requirement Explanation Prompt Template:**

```
System Role:
Generate one-sentence in-fiction explanation for why requirement exists.

Situation: {situation.name}
Requirement: {requirementType} {threshold}
Player Value: {playerCurrentValue}

Task:
Explain why this capability is needed without game terminology.

Examples:
- Stat: "Only someone with commanding presence could rally the guards."
- Bond: "Martha would only trust someone she knows deeply with this secret."
- Scale: "Your reputation for violence precedes you—they demand caution."
- State: "In your exhausted condition, this would be impossible."

Generate similar explanation for this requirement.
```

### Caching and Fallback

**Narrative Caching:**

**Scene Narratives:**
- Generated once per Scene instance
- Cached in Scene object (ephemeral, discarded on exit)
- Regenerated on Scene refresh (after Situation resolution)

**Consequence Narratives:**
- Generated once per Situation resolution
- Displayed in Consequence Modal
- Not cached long-term (discarded after modal dismissal)

**Requirement Explanations:**
- Generated on-demand for tooltips
- Cached per Situation for session duration
- Cleared on game reload

**Repetition Prevention:**

**Recent Narrative Tracking:**
- Store last 10 AI-generated narratives for location
- Pass to AI as "avoid repeating" context
- Prevents identical phrasing across multiple visits

**Template Variety:**
- Multiple prompt templates per generation type
- Rotate templates to vary output style
- Different templates for different tones

**Fallback System:**

**AI Generation Failure:**
If AI call fails or times out:
1. Use template-based fallback text (generic but functional)
2. Display error gracefully (no broken UI)
3. Log failure for debugging
4. Continue gameplay without blocking

**Quality Validation:**
- Check generated text for minimum length (no empty responses)
- Validate no anachronisms (regex for modern terms)
- Ensure no contradictions with stored history
- If validation fails, retry with adjusted prompt or use fallback

---

## State Management & Persistence

### GameWorld Persistence Architecture

**Save Data Structure:**

**Player State (Extended):**
- Existing: Stats, Focus, Stamina, Health, Coins, Items, Equipment, Time (Day/TimeBlock/Segment)
- New: Resolve, Scales (PlayerScales object), Active States (with Day/TimeBlock/Segment), Achievements (with earned Day/TimeBlock/Segment), Completed Situation IDs

**Location State (Extended):**
- Existing: Investigation Cubes, Obstacle references
- New: Available Situation IDs (dynamic list)

**NPC State (Extended):**
- Existing: Story Cubes, current location
- New: Bond Strength, Statuses (status list), Available Situation IDs

**Situation State (New Collection):**
- All Situation instances (template-based + spawned)
- Properties: Status (Dormant/Available/Active/Completed), Generated narrative (cached), Spawn time (Day/TimeBlock/Segment), Completion time (Day/TimeBlock/Segment), Parent Situation ID

**Save Format:**
Extend existing JSON save structure with new sections:
```json
{
  "player": {
    "...existing properties": "...",
    "resolve": 25,
    "scales": {
      "morality": 3,
      "lawfulness": -2,
      "method": 5,
      "caution": 0,
      "transparency": 1,
      "fame": -1
    },
    "activeStates": [
      {
        "type": "Wounded",
        "category": "Physical",
        "appliedDay": 2,
        "appliedTimeBlock": "Evening",
        "appliedSegment": 3,
        "durationSegments": 6
      }
    ],
    "earnedAchievements": [
      {
        "achievementId": "first_bond",
        "earnedDay": 1,
        "earnedTimeBlock": "Midday",
        "earnedSegment": 2,
        "relatedNpcId": "martha"
      }
    ],
    "completedSituationIds": ["repair_wheel", "talk_to_elena"]
  },
  "locations": {
    "common_room": {
      "...existing": "...",
      "availableSituationIds": ["hub_physical_work", "hub_mental_work", "elenas_request"]
    }
  },
  "npcs": {
    "elena": {
      "...existing": "...",
      "bondStrength": 7,
      "statuses": ["Grateful", "Concerned"],
      "availableSituationIds": ["deep_conversation", "urgent_request"]
    }
  },
  "situations": {
    "marthas_secret": {
      "templateId": "relationship_deepening_tier2",
      "status": "Available",
      "placementLocationId": "mill",
      "spawnedDay": 2,
      "spawnedTimeBlock": "Morning",
      "spawnedSegment": 1,
      "completedDay": null,
      "completedTimeBlock": null,
      "completedSegment": null,
      "parentSituationId": "repair_wheel",
      "generatedNarrative": "..."
    }
  }
}
```

**Load Process:**

**Sequence:**
1. Load player state (including new fields, initialize if missing for old saves)
2. Load locations (rebuild available Situation lists)
3. Load NPCs (rebuild available Situation lists, initialize Bonds if missing)
4. Load Situations (rebuild spawn chains, validate parent references)
5. Evaluate Dormant Situations (check if any should be Available based on loaded player state)
6. Rebuild Location/NPC availability lists from Situation statuses

**Migration for Old Saves:**
- Detect missing new fields
- Initialize with default values (Resolve: 30, Scales: all 0, States: empty, Achievements: empty)
- Convert old Goal references to Situation references where applicable
- Graceful degradation (playable even if some new features unavailable)

### Session State Management

**Ephemeral State (Not Saved):**

**Scene State:**
- Generated narrative (regenerated on each visit)
- UI state (expanded cards, selected tabs, scroll position)
- Scene object itself (query result, not persistent entity)

**UI State:**
- Current screen/route
- Modal visibility
- Tooltip display
- Animation states

**Active Challenge State (Existing System):**
- Challenge maintains existing persistence (saved if in progress)
- No changes to existing Challenge save/load

**Temporary Display State:**
- Consequence Modal data (discarded after dismissal)
- Requirement path evaluation results (recalculated on display)
- AI generation progress indicators

---

## Resource Systems

### Complete Resource Architecture

**Five Resource Layers:**

**1. Permanent Capabilities (Stats):**

**Types:**
- Insight (observation, deduction, pattern recognition)
- Rapport (empathy, connection, emotional intelligence)
- Authority (command, presence, leadership)
- Diplomacy (persuasion, negotiation, social grace)
- Cunning (manipulation, deception, tactical thinking)

**Range:** 1-20 (scaling with player level)

**Mechanics:**
- Threshold checks (require value >= X)
- Not consumed by checks (reusable)
- Gained slowly through challenge card play (XP accumulation)
- Gate content access and challenge difficulty
- Multiple stats can satisfy same requirement (OR logic)

**2. Universal Consumable (Resolve):**

**Range:** 0-30

**Purpose:** Creates scarcity across ALL interaction types, preventing "always choose optimal action" pattern.

**Consumption:**
- Tier 1 Situations: 0-3 Resolve (always affordable)
- Tier 2 Situations: 5-8 Resolve (standard)
- Tier 3 Situations: 10-15 Resolve (high-stakes)
- Tier 4 Situations: 18-25 Resolve (defining moments)

**Restoration:**
- Rest action: +5 Resolve
- Low-intensity activities: +2 Resolve
- Natural daily recovery: +3 Resolve
- Full recovery at new day start: restore to max

**Strategic Purpose:**
Forces prioritization between multiple valid options. Even with stats qualifying for everything, limited Resolve creates impossible choices.

**3. System-Specific Consumables:**

**Focus (Mental System):**
- Range: 0-10
- Consumed by Attention cards in Mental challenges
- Recovered by rest and contemplative activities
- Mental challenge exclusive

**Stamina (Physical System):**
- Range: 0-10
- Consumed by Exertion cards in Physical challenges
- Recovered by rest and low-intensity activities
- Physical challenge exclusive

**Initiative (Social System):**
- Built during Social challenges (not pre-existing pool)
- Consumed by high-impact conversational moves
- Session-specific (resets per conversation)
- Social challenge exclusive

**Health:**
- Range: 0-10
- Damaged by high Danger in Physical challenges
- Recovered slowly (rest, treatment)
- Critical resource (reaching 0 has severe consequences)

**4. Economic Resources:**

**Coins:**
- Universal currency
- Gained through work, trade, rewards
- Spent on items, services, economic-path Situations
- Alternative path to stat-gated content (buy instead of qualify)

**Items:**
- Equipment (tools, weapons, armor)
- Consumables (supplies, potions, materials)
- Key items (unique objects for specific Situations)
- Enable or enhance Situations and challenges

**5. Relationship Resources:**

**Story Cubes (Tactical):**
- Range: 0-10 per NPC
- Provide benefits during Social challenges with that NPC
- Built through conversation and shared experiences
- Challenge-scoped impact

**Bond Strength (Strategic):**
- Range: -10 to +10 per NPC
- Gates Situation access (requirement thresholds)
- Influences narrative and NPC reactions
- Long-term relationship progression

**Distinction:**
Story Cubes for tactics (card play benefits), Bond Strength for strategy (content unlocking).

### Scale System Details

**Six Core Scales:**

**Morality Scale:**
- Range: -10 (Exploitative) to +10 (Altruistic)
- Negative: Harm justified by benefit, ends justify means, self-interest
- Zero: Pragmatic, situational ethics, case-by-case
- Positive: Self-sacrifice, compassion, helping without expectation
- Unlocks: Different Situation types at each extreme (neither better/worse)

**Lawfulness Scale:**
- Range: -10 (Rebellious) to +10 (Orthodox)
- Negative: Reject authority, undermine establishment, revolutionary
- Zero: Independent, selective compliance, situational
- Positive: Uphold order, defer to authority, maintain stability
- Unlocks: Criminal content vs official content at extremes

**Method Scale:**
- Range: -10 (Violent) to +10 (Diplomatic)
- Negative: Force first, combat preferred, intimidation
- Zero: Flexible, context-dependent approach
- Positive: Talk first, avoid force, persuasion preferred
- Unlocks: Combat-focused vs negotiation-focused Situations

**Caution Scale:**
- Range: -10 (Reckless) to +10 (Careful)
- Negative: High-risk high-reward, impulsive, bold
- Zero: Calculated risks, measured approach
- Positive: Minimize risk, thorough preparation, cautious
- Unlocks: Daring content vs methodical content

**Transparency Scale:**
- Range: -10 (Secretive) to +10 (Open)
- Negative: Hoard information, operate in shadows, compartmentalize
- Zero: Share strategically, need-to-know basis
- Positive: Share freely, transparent operations, honesty
- Unlocks: Espionage content vs public-facing content

**Fame Scale:**
- Range: -10 (Notorious) to +10 (Celebrated)
- Negative: Feared, infamous, dark reputation
- Zero: Unknown, unremarkable, no reputation
- Positive: Admired, famous, heroic reputation
- Unlocks: Infamy-based vs celebrity-based Situations

**Scale Mechanics:**

**Starting Values:** All scales at 0 (neutral)

**Modification:** Situation consequences shift scales slowly (+1 to +3 typically, rare -1 to -3)

**Compound Effects:** Multiple scales checked together (Morality 8 AND Method 7 unlocks specific peaceful altruist content)

**Both Extremes Valid:** -10 and +10 both unlock unique content, just different archetypes

**Spectrum Content:** Mid-range values (around 0) also have unique content (compromise/pragmatist Situations)

### State System Details

**State Categories and Examples:**

**Physical States:**

**Wounded:**
- Applied: Health < 30%, certain Physical challenge failures
- Blocks: Demanding Physical Situations
- Enables: Desperation Situations, medical attention Situations
- Clears: Treatment, extended rest (3+ rest actions)

**Exhausted:**
- Applied: Stamina < 30%, overexertion
- Blocks: High-intensity Situations
- Enables: Vulnerability Situations, forced rest Situations
- Clears: Rest, time passage (1 day)

**Sick:**
- Applied: Environmental hazards, contamination
- Blocks: All challenges (severe penalty)
- Enables: Treatment-seeking, quarantine Situations
- Clears: Treatment only (time doesn't clear automatically)

**Mental States:**

**Confused:**
- Applied: Failed Mental challenges, overwhelming information
- Blocks: Mental challenges temporarily
- Enables: Clarity-seeking, guided assistance Situations
- Clears: Rest, successful low-difficulty Mental action

**Inspired:**
- Applied: Discovery success, revelation moments
- Blocks: Nothing (positive state)
- Enables: Creative, insightful Situations
- Clears: Time passage (temporary inspiration fades)

**Focused:**
- Applied: Preparation activities, meditation
- Blocks: Nothing (positive state)
- Enables: Enhanced challenge performance, concentration Situations
- Clears: Distraction events, time passage

**Social States:**

**Wanted:**
- Applied: Criminal actions discovered, authority conflict
- Blocks: Official interactions, law-abiding content
- Enables: Underground contacts, fugitive Situations
- Clears: Legal resolution, escape/hiding extended time

**Celebrated:**
- Applied: Heroic acts, public achievements
- Blocks: Nothing (positive state)
- Enables: Fame-based opportunities, public obligation Situations
- Clears: Time passage (fame fades), scandal events

**Shunned:**
- Applied: Social failures, betrayals discovered
- Blocks: Content with witnesses/affected faction
- Enables: Redemption Situations, alternative faction content
- Clears: Reconciliation actions, time passage

**State Management Rules:**

**Maximum Simultaneous:** 3-4 active states (prevents overload)

**Priority System:** Critical states (Wounded, Sick) take precedence

**Clear Conditions:** Always specified per state (never permanent)

**Dual Nature:** Every state blocks some content, enables other content

---

## Requirement Architecture

### Compound OR Philosophy

**Core Principle:** Every Situation with meaningful requirements should have multiple valid paths to unlock. This ensures different character builds can access content through different resource accumulation strategies.

**Single Path Requirements (Rare):**
Only for tutorial or universal accessibility Situations. Examples: "Work for coins" (no requirements), "Rest" (always available).

**Two-Path Requirements (Common):**
Standard for Tier 1-2 Situations. Examples:
- Path A: Stat-based (Insight >= 10)
- Path B: Economic (Coins >= 50)

**Three-Path Requirements (Standard):**
Standard for Tier 3-4 Situations. Examples:
- Path A: Relationship-based (Bond >= 7 AND Achievement)
- Path B: Stat-based (Authority >= 15)
- Path C: Reputation-based (Scale.Fame >= 8 AND Coins >= 100)

**Four+ Path Requirements (Rare, High-Value Content):**
Critical story moments with maximum accessibility. Ensures nearly any build can access through some resource combination.

### Path Component Types

**Within Each OR Path:**

**AND Logic Within Paths:**
All requirements in a path must be satisfied simultaneously. Example:
```
Path: Bond(Martha) >= 7 AND Morality >= 5 AND NOT State.Wanted
```
Player must meet all three to satisfy this path.

**Stat Thresholds:**
Simple numeric comparisons. Most common requirement type.
```
Insight >= 12
Authority >= 10
```

**Bond Requirements:**
Relationship depth with specific NPCs.
```
Bond(Martha) >= 7
Bond(Town_Guard) >= 5
```

**Scale Thresholds:**
Behavioral reputation checks with operators.
```
Morality >= 8 (altruistic requirement)
Method <= -5 (violent requirement)
Fame >= 5 OR Fame <= -5 (famous or infamous, not unknown)
```

**State Requirements:**
Presence or absence of temporary conditions.
```
Required: [Armed, Provisioned] (must have both)
Blocked: [Wounded, Exhausted] (cannot have either)
```

**Achievement Requirements:**
Earned milestone markers.
```
Achievements: [SavedChild, DefendedMill]
```

**Economic Requirements:**
Coin thresholds for purchase/bribe paths.
```
Coins >= 100
```

**Item Requirements:**
Possession of specific items.
```
Items: [RoyalSeal, FakeDocuments]
```

### Requirement Calculation at Spawn

**Template Requirements (Categorical):**
JSON defines requirements using categorical offsets:
```
StatRequirement: {
  stat: "Insight",
  offset: +3,
  baseType: "current"
}
```

**Spawn-Time Calculation:**
When parent Situation spawns child:
1. Read player current Insight: 10
2. Apply offset: 10 + 3 = 13
3. Create requirement: Insight >= 13
4. Freeze in spawned Situation instance

**Why Offset-Based:**
Ensures requirements scale with player progression. Same template spawns differently at different player levels. Early-game spawn might require Insight 8, late-game spawn requires Insight 15.

**Offset Types:**

**Current Plus:**
Require current value + offset.
```
Template: "bond_current + 2"
Player Bond: 5
Spawned: Bond >= 7
```

**Absolute:**
Fixed threshold regardless of player state.
```
Template: "morality_absolute: 8"
Spawned: Morality >= 8
```

**Percentage:**
Percentage of max value.
```
Template: "insight_percent: 0.6"
Max Insight: 20
Spawned: Insight >= 12 (60% of max)
```

### Requirement Display

**Available Situations:**
Show only costs (requirements already met, no need to display).

**Locked Situations:**
Show all OR paths with visual progress:
```
Unlock Paths (3 alternatives):

Path 1: "Trust Path" [60% complete]
  ✓ Bond(Martha) >= 7 (you have 8)
  ✗ Morality >= 5 (you have 3)
  Missing: +2 Morality

Path 2: "Authority Path" [40% complete]
  ✗ Authority >= 12 (you have 9)
  ✓ Coins >= 100 (you have 120)
  Missing: +3 Authority

Path 3: "Achievement Path" [0% complete]
  ✗ Achievement: TrustedConfidant (not earned)
  Missing: Earn achievement

Closest Path: Path 1 (need +2 Morality)
[Show How to Gain Morality]
```

**Actionable Feedback:**
For closest path, provide links to Situations that help meet missing requirements. Example: "Gain Morality" button shows list of Situations with Morality-positive consequences.

---

## Spawn Cascade Mechanics

### Spawn Execution Flow

**Trigger Point:**
Situation completion (success or failure for Challenge types, always success for Instant types).

**Execution Sequence:**
1. Parent Situation completes
2. SituationFacade calls SpawnFacade.ExecuteSpawns
3. SpawnFacade retrieves spawn rules from parent (OnSuccessSpawns or OnFailureSpawns based on outcome)
4. For each spawn rule:
   - Retrieve template from GameWorld
   - Instantiate new Situation from template
   - Calculate requirements using current player state + offsets
   - Set initial status (Dormant or Available)
   - Set parent reference
   - Add to GameWorld Situations collection
   - Check spawn caps and cooldowns
   - Add to target Location/NPC if Available

**Spawn Rule Structure:**

**Template Reference:**
Which Situation template to spawn from.

**Target Placement:**
Where spawned Situation appears (Location ID, NPC ID, or Route ID).

**Requirement Offsets:**
Adjustments to template requirements based on current state.
```
Template base: Bond >= 5
Offset: +2 from current
Player current Bond: 6
Spawned requirement: Bond >= 8
```

**Spawn Conditions:**
When this spawn rule fires (optional conditional spawning).
```
Condition: Only if Scale.Morality >= 5
If player Morality < 5, this spawn doesn't fire
```

### Cascade Chain Depth

**Tier-Based Cascading:**

**Tier 1 Situations (Entry Content):**
- Spawn probability: 80%
- Typical spawns: 1-2 child Situations
- Depth potential: Spawn Tier 1-2

**Tier 2 Situations (Standard Content):**
- Spawn probability: 60%
- Typical spawns: 2-3 child Situations
- Depth potential: Spawn Tier 2-3

**Tier 3 Situations (High-Stakes Content):**
- Spawn probability: 40%
- Typical spawns: 1-2 child Situations
- Depth potential: Spawn Tier 3-4

**Tier 4 Situations (Climactic Content):**
- Spawn probability: 20%
- Typical spawns: 0-1 child Situation
- Depth potential: Rarely spawns (ending/resolution content)

**Example Cascade Chain:**
```
Tier 1: "Work at Mill" (always available)
  Spawns: "Martha's Small Request" (Tier 1, Bond 3 required)
    Spawns: "Deliver Package" (Tier 2, Bond 5 required)
      Spawns: "Discover Conspiracy" (Tier 3, Bond 7 + Insight 12 required)
        Spawns: "Confront Conspirators" (Tier 4, multiple path requirements)

Total depth: 5 levels
Player progression: Builds Bond + Insight over multiple interactions
Emergent storyline: Started with work, led to conspiracy revelation
```

### Spawn Control Systems

**Maximum Concurrent Limits:**

**Total Available Situations:**
Max 20 Available at any time across entire world. Prevents overwhelming choice paralysis.

**Per-Location Available:**
Max 5 Available per Location. Keeps Scene displays manageable.

**Per-NPC Available:**
Max 3 Available per NPC. Focuses conversations.

**Enforcement:**
When spawning would exceed cap, evaluate priority and replace lowest-priority Available Situation with new spawn.

**Priority Calculation:**
```
Priority Score:
  + Tier × 10 (higher tier = higher priority)
  - Days Available × 1 (older = lower priority, becomes stale)
  + Urgency flag × 50 (urgent always high priority)
  + Progression flag × 30 (main story content prioritized)
```

**Archetype Cooldowns:**

**Prevent Repetition:**
Can't spawn two Situations of same archetype within cooldown period.

**Cooldown Durations:**
- Rescue: 3 days (rare heroic moments)
- Betrayal: 7 days (major relationship events)
- Crisis: 2 days (tension spacing)
- Alliance: 5 days (significant partnerships)
- Romance: 10 days (gradual relationship progression)
- Discovery: 1 day (frequent but paced)
- Trade: 0 days (can repeat freely, economic)

**Branching Spawn Rules:**

**Conditional Spawning:**
Spawn rules can include conditions that determine which children spawn based on player state at completion.

**Example:**
```
Parent: "Defend Mill"

OnSuccess:
  If Scale.Method >= 0:
    Spawn "Grateful Community" (diplomatic resolution)
  If Scale.Method < 0:
    Spawn "Fearful Community" (violent resolution)

  If Bond(Martha) >= 7:
    Spawn "Martha's Secret" (trust-based follow-up)
  Else:
    Spawn "Martha Keeps Distance" (low-trust outcome)
```

This creates branching narrative based on HOW player succeeded, not just that they succeeded.

**Mutual Exclusivity:**
Some spawn rules marked exclusive—only one from set spawns based on highest-priority condition met.

### Spawn Feedback

**Immediate Display:**
Consequence Modal explicitly shows spawned Situations:
```
New Opportunities Available:
• "Martha's Secret" at Old Mill [Available Now]
• "Track Strangers" at Forest Edge [Locked: Need Insight 8]
• "Report to Guard" at Town Square [Available Now]
```

**Location Indicators:**
World map shows updated Situation counts:
```
Old Mill (4 available, 2 locked, 1 urgent)
  Previously: (3 available, 2 locked)
  Change: +1 available (newly spawned)
```

**Journal Tracking:**
Journal Situations tab shows spawn chains:
```
Completed: "Defend Mill"
  └─ Spawned: "Martha's Secret" [Available at Mill]
  └─ Spawned: "Track Strangers" [Locked at Forest Edge]
  └─ Spawned: "Report to Guard" [Available at Town Square]
```

Visual tree structure shows parent-child relationships, helping player understand consequence chains.

---

## A/B/C Thread Architecture (Soft-Lock Prevention)

### The Sir Brante Safety Net Pattern

**Sir Brante's Two-Layer Content Structure:**

**A Thread (Mandatory Life Events):**
- Core progression that happens regardless of player stats
- You age: child → youth → adult
- Family events occur
- Job assignments happen
- Death comes eventually
- **Critical Feature:** Even with terrible stats and missing ALL optional content, the A thread continues forward. You cannot soft-lock.

**B/C Threads (Optional Stat-Gated Content):**
- Special opportunities requiring high stats
- Faction-specific paths
- Romance options
- Achievement-locked content
- Branches off the main line, but always optional

**Wayfarer's Translation:**

**A Thread = Hub Location Safety Net (Tier 0 Work)**
- ONE designated hub location (Central Square/Tavern)
- Always accessible (free or minimal 1-segment travel cost)
- Contains guaranteed Tier 0 Situations with ZERO requirements
- Boring but functional survival work
- Ensures player can NEVER be completely stuck

**B/C Threads = All Other Content (Tiers 1-4)**
- Dynamic spawned Situations at all locations
- Gated by player state (stats, bonds, scales, achievements)
- Interesting cascades and consequences
- Creates the actual story
- Can be completely locked at non-hub locations

### The Mathematical Guarantee

**Hub Location MUST satisfy:**

```
∀ hub_location L:
  ∃ at least 3 Tier 0 Situations S where:
    Requirements(S) = ∅ (zero requirements)
    Cost(S) ∈ {Stamina, Focus, Time} (renewable resources only)
    Reward(S) = {Coins ≥ 5, StatXP ≥ 1} (survival + progression)
    Repeatable(S) = true (infinite repeat allowed)
```

**Translation:** The hub location ALWAYS has at least 3 Situations with:
- Zero requirements beyond resource costs
- Only renewable resource costs (no Resolve)
- Guaranteed survival rewards (coins for food/rest)
- Basic stat progression (1 XP per completion)
- Infinite repeatability (never exhausted)

### Tier 0 Work Structure

**Hub Location Guaranteed Content:**

**Physical Work (Always Available):**
- ID: "hub_physical_work" (template, instances spawned per hub)
- Name: Contextual ("Load Wagons", "Haul Grain", "Move Crates")
- Requirements: {} (none)
- Cost: {Stamina: 5} (renewable)
- Reward: {Coins: 8, StatXP: {Authority: 1}}
- InteractionType: Instant (no challenge, immediate resolution)
- Narrative: Generic, contextually appropriate
- Spawns: None (no cascades - this is safety net only)

**Mental Work (Always Available):**
- ID: "hub_mental_work"
- Name: Contextual ("Count Inventory", "Sort Deliveries", "Review Ledgers")
- Requirements: {} (none)
- Cost: {Focus: 3} (renewable)
- Reward: {Coins: 7, StatXP: {Insight: 1}}
- InteractionType: Instant
- Narrative: Generic, contextually appropriate
- Spawns: None

**Social Work (Always Available):**
- ID: "hub_social_work"
- Name: Contextual ("Serve Customers", "Help Travelers", "Manage Tables")
- Requirements: {} (none)
- Cost: {Time: 2} (renewable)
- Reward: {Coins: 6, StatXP: {Rapport: 1}}
- InteractionType: Instant
- Narrative: Generic, contextually appropriate
- Spawns: None

### Worst-Case Recovery Path

**Player in Terrible State:**
- All stats below 5
- No bonds with anyone
- No achievements earned
- Zero coins
- Wounded, exhausted states active
- Every interesting Situation locked

**Recovery Cycle (Guaranteed Path Forward):**

```
Day 1:
1. Travel to hub (always possible)
2. Physical work → 8 coins, +1 Authority XP
3. Physical work → 8 coins, +1 Authority XP
4. Mental work → 7 coins, +1 Insight XP
Total: 23 coins, 3 XP gained

Day 2:
1. Buy food (15 coins) → Restore Stamina/Focus
2. Rest action (restores Health)
3. Social work → 6 coins, +1 Rapport XP
4. Physical work → 8 coins, +1 Authority XP
Total: 14 coins, 2 XP gained

Days 3-10:
Continue working 2-3 times per day
Accumulate coins for survival
Accumulate XP for stat increases
After ~20 work sessions: +20 XP → stat increase → Tier 1 content unlocks

Days 11+:
Access easier B/C thread content
Build bonds through available Situations
Gradually unlock higher tiers
Progress out of rock-bottom state
```

**Critical:** It's boring, inefficient, and punishing—but NEVER blocked. The hub is life support.

### Non-Hub Locations Can Be Fully Locked

**Other locations have NO guarantee:**

```
Visit Mill Scene

═══════════════════════════
THE MILL - AFTERNOON
═══════════════════════════

The wheel you repaired turns steadily. Martha glances at you
but says nothing. You sense there's more here, but you haven't
earned her trust yet.

───────────────────────────
ALL SITUATIONS LOCKED
───────────────────────────

→ "Investigate Tracks" ⚠️ LOCKED
  Requires: Insight 8 (you have 6)

→ "Martha's Secret" ⚠️ LOCKED
  Requires: Bond(Martha) 5 (you have 2)

→ "Repair Upper Mechanism" ⚠️ LOCKED
  Requires: Authority 10 (you have 7) OR Stamina 8 (you have 3)

───────────────────────────
[Return to Town Hub]
───────────────────────────
```

**Player response:** Go back to hub, do work, build stats/bonds, return later when qualified.

This creates natural rhythm:
- **Hub:** Boring, safe, always available (A thread)
- **World:** Interesting, risky, conditionally available (B/C threads)

### Scene Choice Presentation (With A/B/C Thread Visual)

```
════════════════════════════════════════════════

THE BRASS BELL INN (HUB) - EVENING

════════════════════════════════════════════════

You return to the inn's warm common room. The familiar scents
of ale and woodsmoke greet you. Elena nods recognition. Thomas
sits in his usual corner. This is home base—always has work.

────────────────────────────────────────────────
A THREAD - ALWAYS AVAILABLE (Tier 0)
────────────────────────────────────────────────

→ Load wagons for Thomas
  [Physical work, 5 Stamina → 8 Coins, +1 Authority XP]

→ Help Elena with inventory
  [Mental work, 3 Focus → 7 Coins, +1 Insight XP]

→ Serve evening customers
  [Social work, 2 Time → 6 Coins, +1 Rapport XP]

────────────────────────────────────────────────
B/C THREADS - OPTIONAL CONTENT (Tiers 1-4)
────────────────────────────────────────────────

→ Elena's concern
  [Social challenge, 2 Resolve, Bond 5 → Relationship deepens]

→ Thomas's shipment request ⚠️ LOCKED
  Requires: Authority 8 (you have 7)
  [Would grant: Access to warehouse district, bond with Thomas]

→ Investigate missing patron ⚠️ URGENT
  Requires: Insight 6 (you have 6)
  [Social challenge, 5 Resolve, 3 Time → Investigation spawns]

────────────────────────────────────────────────
```

**Player ALWAYS sees:**
- Top section: Tier 0 work (boring but functional)
- Bottom section: Interesting content (may be locked)

**If everything locked:** Tier 0 work remains available for resource accumulation.

### AI Generation Pattern for Tier 0 vs Tiers 1-4

**Tier 0 (A Thread) - Formulaic Generation:**

```
Input: Hub location type (tavern, market, warehouse, town square)
Output: 3 generic work descriptions (Physical/Mental/Social)

Template Structure:
"You spend [time period] [activity]. [NPC] [minimal reaction].
The work is [adjective] but pays."

Physical Example:
"You spend hours moving heavy sacks of grain. Thomas nods
appreciation but doesn't speak. The work is exhausting but pays."

Mental Example:
"You carefully count the inventory, marking discrepancies in the
ledger. Tedious but necessary work that earns a few coins."

Social Example:
"You help customers at the counter, answering basic questions.
Simple interaction, modest pay."
```

**Tiers 1-4 (B/C Threads) - Contextual Generation:**

```
Input:
- Player history (completed Situations, achievements)
- Bonds with NPCs
- Scales (Morality, Method, etc.)
- Parent Situation context
- Location state

Output: Unique narrative drawing from accumulated context

Example:
"You return to the mill. Martha recognizes you—the one who
repaired the wheel and saved her child. Her face brightens
but then clouds. She glances toward the forest edge where you
noticed tracks last visit. 'I need to tell someone,' she says
quietly, 'and you're the only one I trust.'"
```

**The Difference:**
- **Tier 0:** Generic, repeatable, contextually appropriate but minimal narrative
- **Tiers 1-4:** Specific, unique, deeply connected to player history and choices

### Implementation Requirements for Hub Location

**Hub Designation:**
- ONE location marked as `isHub: true` in JSON
- Default: Brass Bell Inn (Common Room)
- Always accessible from world map (free or 1-segment cost)
- Cannot be permanently blocked by any game state

**Tier 0 Situation Templates:**
- Exactly 3 templates: Physical, Mental, Social work
- Requirements: {} (empty object - no requirements)
- Costs: Only renewable resources (Stamina/Focus/Time, NO Resolve)
- Rewards: Coins (5-8) + StatXP (1-2)
- Repeatable: true (never exhausted)
- DeleteOnSuccess: false (persist after completion)
- Spawns: [] (empty - no cascades)
- InteractionType: Instant (no challenge)

**Hub Scene Generator Rules:**
1. ALWAYS present Tier 0 work first (top of list)
2. THEN layer dynamic Situations below (if any available)
3. If all dynamic Situations locked, Tier 0 work still visible
4. Visual separation (A Thread section vs B/C Thread section)

**Validation:**
- On game initialization, verify hub location exists
- Verify hub has exactly 3 Tier 0 Situations
- Verify Tier 0 Situations have zero requirements
- If validation fails, throw error (cannot start game without safety net)

### Design Principle Summary

**A Thread = Life Support System**
- Always available at designated hub
- Minimal requirements (none for Tier 0)
- Boring but functional
- Prevents soft-lock
- Allows recovery from any state

**B/C Threads = The Actual Game**
- Spawned from player actions
- Gated by player state
- Interesting cascades
- Meaningful consequences
- Optional but desirable content

**Just like Sir Brante:**
- A thread = Forced life events (aging, family, jobs - unavoidable)
- B/C threads = Optional paths (romance, factions - may miss)
- Even if you fail everything optional, life continues

**In Wayfarer:**
- A thread = Hub work (basic jobs - can't be blocked)
- B/C threads = Spawned investigations/relationships (may not qualify)
- Even if locked out of all interesting content, work remains

**The safety net is invisible when you don't need it, but ALWAYS present when you do.**

---

## Player Experience Design

### The Sir Brante Experience Loop

**Core Loop (per Location Visit):**

```
1. Navigation Decision
   Player chooses where to go based on:
   - Known Available Situations (journal tracking)
   - Urgent indicators (world map flags)
   - Curiosity (explore less-visited locations)

2. Scene Entry
   Contextual narrative references:
   - Past actions at this location
   - Current relationships with NPCs here
   - Player reputation (scales)
   - Active conditions (states)
   Immersive, specific, grounded

3. Choice Presentation
   Sir Brante-style list:
   - Available Situations (green, clickable)
   - Locked Situations (gray, shows requirements)
   - Categorized (Urgent/Progression/Relationship/etc.)
   - Costs and consequences visible upfront

4. Strategic Deliberation
   Player evaluates:
   - Can I afford costs? (Resolve, resources)
   - Do I want consequences? (scale shifts, bond changes)
   - What spawns from this? (projected outcomes)
   - What else could I do instead? (opportunity cost)

5. Selection
   Choose ONE Situation (mutually exclusive)
   Lock in decision
   System validates and deducts costs

6. Resolution
   If Instant: Apply consequences immediately
   If Challenge: Launch tactical gameplay (Mental/Physical/Social)
   If Navigation: Move to new location
   Existing challenge systems unchanged (tactical depth preserved)

7. Consequence Revelation
   Modal shows explicitly:
   - What changed (stats, bonds, scales, states)
   - What spawned (new Situations, where they are)
   - Immediate feedback (Sir Brante pattern)

8. Decision Point
   Stay (regenerate Scene with new content) OR
   Leave (return to navigation)

   If Stay: Loop to step 2 (new Scene instance)
   If Leave: Loop to step 1 (navigation)
```

### Impossible Choice Architecture

**Creating Genuine Trade-offs:**

**Resolve Scarcity:**
Player has 12 Resolve remaining. Three Available Situations:
- "Martha's Urgent Request" (10 Resolve, deepens trust, spawns relationship content)
- "Investigate Conspiracy" (8 Resolve, advances main story, spawns investigation content)
- "Help Wounded Traveler" (5 Resolve, moral choice, spawns altruism content)

Can do ONE major (Martha OR Investigate) plus one minor (Traveler), or EITHER major alone. Genuine strategic choice with opportunity cost.

**Scale Tensions:**
Situation "Confront Corrupt Official":
- Path A: Violent intimidation (Method -2, Authority +1, quick resolution)
- Path B: Public exposure (Transparency +2, Fame +1, slow resolution)
- Path C: Bribe and silence (Morality -1, Coins -100, secret remains)

Each path shifts scales differently, creating long-term consequences for character identity.

**Relationship Competition:**
Bond(Martha): 7, Bond(Guard): 5
Both have urgent Situations requiring 8 Resolve each. Can only do one. Choosing Martha strains relationship with Guard (they needed help, player prioritized someone else). Choosing Guard strains Martha (she trusted player would help). Genuine relational trade-off.

### Planning Horizon

**Visibility Ahead:**

**Immediate Layer (Available Now):**
- 10-15 Available Situations across locations
- Can pursue any immediately
- Full information (costs, consequences, spawns)

**Near-Term Layer (Locked, Close to Unlocking):**
- 5-10 Dormant Situations visible in journal
- Requirements shown: "Need +2 Bond(Martha), you have 6, need 8"
- Player sees path forward: "Do X to gain Bond, then Y unlocks"
- Planning 2-3 actions ahead

**Medium-Term Layer (Locked, Future Content):**
- 10-15 Dormant Situations requiring significant progression
- Shows what's possible eventually
- Motivates resource accumulation
- Planning 5-10 actions ahead

**Unknown Layer (Not Yet Spawned):**
- Content that spawns from future completions
- Unknown until spawned
- Creates discovery and surprise
- Prevents perfect prediction (maintains tension)

**Result:** Player can plan strategically (knows what's coming soon) while preserving mystery (doesn't know everything).

### Verisimilitude Preservation

**Narrative Grounding:**

**AI References Actual Events:**
Not: "You return to the mill."
But: "You return to the mill where you saved Martha's child from the flames three days ago. Her gratitude is palpable."

**Consequences Make Fictional Sense:**
Situation: "Violently threaten merchant"
Consequence: Method -2 (reputation for violence grows)
Spawns: "Merchants Fear You" (economic consequences)
        "Guards Investigate" (legal consequences)
Fictional logic: Violence creates fear and legal attention

**Requirements Justified In-Fiction:**
Locked Situation: "Martha's Deepest Secret"
Requirement: Bond(Martha) >= 9
In-fiction: "Martha would only reveal this to someone she trusts like family."
Not: "You need relationship level 9."

**States Influence Narrative:**
Player has State: Wounded
Scene narrative: "You wince as you approach, the pain from yesterday's fight still sharp in your side. Others notice your injury, eyes widening with concern or curiosity."
Situations enabled: "Seek medical attention", "Show vulnerability to Martha"
Situations blocked: "Climb tower", "Impress with physical prowess"

**Scale Consistency:**
Player has Morality -8 (exploitative)
NPCs react accordingly:
- Vulnerable NPCs fearful
- Criminal NPCs interested
- Altruistic NPCs disgusted
Content unlocked reflects reputation:
- Extortion opportunities
- Forced compliance options
- Ruthless efficiency paths

---

## Validation & Safety Systems

### Spawn Validation

**Cycle Prevention:**

**Parent Chain Check:**
Before spawning, verify spawned Situation's template doesn't reference any ancestor in parent chain.
```
Parent Chain: A → B → C
Attempting to Spawn: C
Check: C is in parent chain
Result: INVALID (would create cycle)
Action: Skip this spawn, log warning
```

**Maximum Chain Depth:**
Hard limit: 10 levels deep
If spawning would exceed depth, spawn doesn't fire.
Prevents infinite chains from bugs.

**Reference Validation:**

**Template Exists:**
All spawn rules reference valid template IDs.
Validation at parse time: If template ID doesn't exist in GameWorld, parsing fails.

**Placement Exists:**
Target Location/NPC/Route IDs must exist.
Validation at spawn time: If placement missing, spawn fails gracefully (logs error, continues).

**Requirement Achievability:**

**No Impossible Requirements:**
Validate at parse time that requirements don't contain contradictions:
```
INVALID: State.Wounded required AND State.Rested required
(Mutually exclusive states)

INVALID: Bond(Martha) >= 10 AND Bond(Martha) <= 5
(Impossible range)
```

**At Least One Path Accessible:**
For compound OR requirements, validate at least one path is theoretically achievable with max player stats.
```
VALID: (Insight >= 20) OR (Bond >= 10) OR (Coins >= 500)
All three achievable separately

INVALID: (Insight >= 25) OR (Stat_That_Doesnt_Exist >= 10)
First path impossible (max Insight 20), second path broken
```

### Player Protection

**No Soft-Lock States:**

**Always Low-Cost Content Available:**
Every location has at least one Tier 1 Situation with minimal requirements and low Resolve cost.
Examples: "Rest" (0 cost), "Work" (2 Resolve), "Observe" (3 Resolve)
Ensures player never completely stuck.

**Resolve Floor:**
Resolve can't go below 0 (enforced at cost deduction).
If player at 0 Resolve, only 0-cost Situations available.
Daily restore guarantees minimum +3 Resolve per day.

**Forced Progression on Deadlock:**

**Detection:**
If player has no Available Situations anywhere AND all Dormant Situations have requirements the player can't reach, deadlock detected.

**Resolution:**
System automatically spawns "emergency" Situations:
- Low-difficulty content to build resources
- Situations that grant needed capabilities
- Alternative path Situations with achievable requirements

**Example:**
Player needs Insight 12 but has 8, no way to gain Insight.
System spawns: "Study in Library" (Instant, grants +2 Insight XP, low cost)

**State Accumulation Protection:**

**Maximum Active States:**
Hard cap: 4 simultaneous active States
If applying 5th State, oldest non-critical State auto-clears.
Critical States (Wounded, Sick) take priority over non-critical (Exhausted, Inspired).

**Clear Path Always Exists:**
Every State has defined clear conditions.
No permanent States (all eventually clearable).
Rest action clears most negative States.

### Content Quality Validation

**AI Output Validation:**

**Minimum Length:**
Scene narratives: 200-500 words minimum
Consequence narratives: 100-300 words minimum
If below minimum, retry generation or use fallback.

**Anachronism Detection:**
Regex check for modern terms: "computer", "phone", "internet", "modern", "contemporary"
If detected, retry with enhanced prompt or use fallback.

**Contradiction Prevention:**
Pass recent narratives (last 10 for location) to AI as "avoid contradicting" context.
Example: If previous narrative says "mill wheel broken", current narrative shouldn't say "mill operating smoothly" unless player repaired it.

**Repetition Detection:**
Check generated text against recent cache.
If similarity > 70% (string matching), retry with variety prompt.
Prevents identical phrasing across visits.

**Fallback System:**

**Template-Based Fallback:**
If AI generation fails 3 times or times out:
Use template-based generic text with variable substitution.
Example: "You arrive at {location.name}. {NPC.name} greets you {based on Bond level}."
Not ideal, but functional and doesn't break gameplay.

**Graceful Degradation:**
If all generation fails:
- Show minimal narrative (location name only)
- Display Situation list (mechanical systems work)
- Log error for debugging
- Continue gameplay (don't block player)

---

## Implementation Strategy

### Phase-Based Rollout

**Phase 1: Foundation (Weeks 1-3)**

**Objective:** Establish data structures and parsing

**Deliverables:**
- JSON structure for situations.json, scales.json, states.json, achievements.json
- Parser implementations (SituationParser, ScaleParser, StateParser, AchievementParser)
- Catalogue implementations (RequirementCatalogue, CostCatalogue, ConsequenceCatalogue)
- Domain entities (Situation, Scale, State, Achievement, CompoundRequirement, SpawnRule)
- GameWorld integration (add new collections, modify Player/Location/NPC entities)
- Basic parse-and-load test (no UI, just validate data loads correctly)

**Success Criteria:**
- All JSON parseable without errors
- GameWorld initializes with Situations, Scales, States, Achievements
- No compilation errors

**Phase 2: Core Logic (Weeks 4-6)**

**Objective:** Implement facade layer and game logic

**Deliverables:**
- SceneFacade (Scene generation, requirement evaluation, categorization)
- SituationFacade (selection validation, Instant resolution, Challenge initiation)
- SpawnFacade (spawn execution, Dormant evaluation, cap enforcement)
- ConsequenceFacade (apply changes, format display)
- GameFacade integration (EnterLocation, SelectSituation routing)
- Spawn chain tracking (parent references, depth limits)
- Dormant→Available evaluation (trigger on state changes)

**Success Criteria:**
- Complete Situation lifecycle works (Dormant → Available → Active → Completed)
- Spawn cascading functional (parent spawns children with calculated requirements)
- Instant Situations fully resolve (consequences apply, spawns trigger)
- Consequence application tested (stats/bonds/scales/states update correctly)
- No UI yet (testing via direct facade calls)

**Phase 3: Challenge Integration (Weeks 7-8)**

**Objective:** Connect Situations to existing Challenge systems

**Deliverables:**
- Challenge-type Situation flow (SituationFacade → ChallengeFacade)
- GoalCard rewards application separate from Situation consequences
- Challenge success/failure spawn triggers (OnSuccess vs OnFailure rules)
- Navigation-type Situation flow (movement + Scene trigger)

**Success Criteria:**
- Mental/Physical/Social Challenges launch from Situations
- GoalCard rewards apply correctly (tactical layer)
- Situation consequences apply after Challenge (strategic layer)
- Spawn triggers fire based on Challenge outcome
- Existing Challenge gameplay unchanged (no regressions)

**Phase 4: UI Layer (Weeks 9-11)**

**Objective:** Build Scene-based user interface

**Deliverables:**
- LocationSceneScreen component (Scene display)
- NPCSceneScreen component (NPC interaction variant)
- SituationCard component (choice display with requirements)
- ConsequenceModal component (resolution feedback)
- RequirementPathDisplay component (OR path visualization)
- CategoryTabs component (Situation organization)
- WorldMapScreen modifications (Situation count indicators)
- JournalScreen modifications (new tabs: Active/Dormant/Completed Situations, Achievements, Scales, States)
- GameScreen routing (Scene navigation, modal management)

**Success Criteria:**
- Complete Scene-based UI displays correctly
- Sir Brante-style choice presentation functional
- Requirement paths show progress and closest path
- Consequence Modal shows all changes and spawns
- Stay/Leave flow works (regenerate Scene or exit)
- Perfect information display validated (all locked requirements visible)

**Phase 5: AI Integration (Weeks 12-14)**

**Objective:** Add AI narrative generation

**Deliverables:**
- Scene narrative generation (context assembly, AI call, caching)
- Consequence narrative generation (resolution storytelling)
- Requirement explanation generation (tooltip text)
- Context system (query player state, format for AI)
- Prompt templates (Scene/Consequence/Requirement)
- Caching system (prevent regeneration, repetition tracking)
- Fallback system (template-based text if AI fails)
- Validation system (length, anachronisms, contradictions)

**Success Criteria:**
- Contextual narratives generated successfully
- AI references specific player achievements/bonds/scales
- Narratives coherent across multiple Scenes
- No anachronisms or contradictions
- Fallback text functional if AI unavailable
- Performance acceptable (< 2 second generation time)

**Phase 6: Content Authoring (Weeks 15-16)**

**Objective:** Create initial Situation template library

**Deliverables:**
- 50+ Situation templates across all tiers
- 10+ Scene templates for critical moments
- Spawn chain definitions (4-5 level depth examples)
- Compound requirement examples (multiple OR paths)
- Test all interaction types (Instant, Mental, Physical, Social, Navigation)
- Cover all categories (Urgent, Progression, Relationship, Opportunity, Exploration)

**Success Criteria:**
- Diverse template library covering all archetypes
- Tested spawn chains create emergent narratives
- Multiple unlock paths validated (different builds access content)
- No dead ends or soft-locks in authored content
- Narrative hints guide AI generation effectively

**Phase 7: Balance & Tuning (Weeks 17-18)**

**Objective:** Polish progression curve and resource balance

**Deliverables:**
- Resolve cost tuning (tier-appropriate scarcity)
- Requirement threshold tuning (achievable but challenging)
- Spawn probability tuning (frequency control)
- Cooldown tuning (repetition prevention)
- Priority score tuning (replacement logic)
- Consequence magnitude tuning (scale shift rates, bond change rates)

**Success Criteria:**
- Balanced progression curve (not too easy, not too hard)
- Resolve creates scarcity without frustration
- Different builds viable (multiple paths to content)
- No soft-locks encountered in testing

---

## Success Metrics

### Mechanical Metrics

**Content Depth:**
- Average cascade chain depth: 4+ levels
- Situations per playthrough: 100+ unique instances
- Template reuse: Each template spawns 3-5 instances per playthrough
- Overlap between playthroughs: < 20% (high variety)

**Gating Effectiveness:**
- Soft-locks encountered: 0 (always forward progress)
- Impossible requirements: 0 (all validated)
- OR paths per Situation: Average 2-3 (multiple unlock routes)
- Alternative path accessibility: 80%+ Situations accessible via 2+ distinct resource paths

**Spawn Control:**
- Available Situations concurrent: 10-20 (manageable choice)
- Spawn cap exceeded events: Logged, validated replacement logic works
- Cooldown violations: 0 (archetype spacing enforced)
- Spawn cycle detection: 0 (validation prevents)

### Player Experience Metrics

**Perfect Information:**
- Locked Situations showing requirements: 100%
- Requirement paths displayed with progress: 100%
- Consequence visibility before selection: 100% (projected consequences shown)
- Spawn notifications after resolution: 100% (explicit feedback)

**Strategic Depth:**
- Resolve scarcity effective: Players forced to prioritize, not spam all content
- Planning horizon: Players report planning 2-3 actions ahead
- Build variety: Different stat/scale distributions access same content via different paths

**Narrative Quality:**
- AI references player history: 90%+ Scene narratives mention specific achievements/relationships
- Tone consistency: 95%+ narratives match Situation archetype expectations
- Contradiction rate: < 2% (narratives contradict previous events)
- Verisimilitude: Qualitative assessment of grounded, realistic feel

## Conclusion

This architecture transforms Wayfarer from static authored content to a living world that responds to player choices through cascading mechanical consequences wrapped in contextual AI-generated narrative. The Scene-Instance pattern successfully translates Sir Brante's linear choice-consequence momentum to spatial navigation, preserving immediate feedback and perfect information while enabling exploration.

**Core Achievements:**

1. **Sir Brante's Strategic Layer:** Situations as meaningful choices with visible requirements, costs, and consequences
2. **Wayfarer's Tactical Depth:** Existing Challenge systems preserved, providing skill-based execution layer
3. **Living World Simulation:** Cascading spawns create emergent 4-5 level narrative chains unique per playthrough
4. **Perfect Information:** Players see all options (Available and Locked), all requirements, all projected consequences
5. **No Boolean Gates:** All gating via accumulative numerical resources with compound OR requirements
6. **Emergent Identity:** Character archetypes emerge organically from accumulated scale shifts and relationship patterns
7. **AI Narrative Generation:** Infinite contextual variety while maintaining narrative coherence and verisimilitude
8. **Immediate Feedback:** Consequence Modal shows spawns and changes explicitly (Sir Brante pattern in spatial context)

**The Result:**

A single playthrough generates an ongoing unique story where every choice spawns new choices, relationships deepen or fracture based on behavior, reputation spreads through accumulated actions, and the world evolves in response to the player's accumulated character identity—all while preserving Wayfarer's tactical challenge depth and spatial exploration freedom.

Players experience Sir Brante's accumulative progression and impossible choices merged with Wayfarer's five-system tactical gameplay and spatial navigation, creating a hybrid that achieves the best of both designs.
