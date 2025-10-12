# Wayfarer Obstacle System - Clean Architecture

## Core Philosophy

Obstacles create impossible choices through multiple valid approaches with genuine trade-offs. Players express values through which costs they accept. Resolution feels satisfying because success is earned through strategic decision-making.

## Three Core Entities

### Obstacles

Strategic barriers spawned by game systems (investigations, travel, events). Property containers with inline goals.

**Properties (0-3 scale):**
- PhysicalDanger: How difficult physical approaches are
- SocialDifficulty: How difficult social approaches are  
- MentalComplexity: How difficult mental approaches are

**State:**
- Active, Resolved, or Transformed
- ResolutionMethod: How player overcame it (Violence/Diplomacy/Stealth/Authority/Cleverness/Preparation)
- RelationshipOutcome: Social impact (Hostile/Neutral/Friendly/Allied/Obligated)

**Content:**
- Goals list (inline, not references)

### Goals

Approaches to overcome obstacles. Each goal lives inside its parent obstacle.

**Placement:**
- LocationId OR NpcId: Where this goal's button appears in UI

**Challenge:**
- SystemType: Mental/Physical/Social
- DeckId: Which challenge deck to use

**Availability:**
- Property thresholds checked against parent obstacle
- Required knowledge from other completed goals (local IDs)
- Required items
- Goal dependencies (local IDs within obstacle)

**Resolution:**
- Consequence type: Resolution/Bypass/Transform/Modify/Grant
- Effects: What happens on success

**Victory:**
- GoalCards with momentum thresholds and rewards

### Knowledge Cards

Cards granted by goals, added to future challenge decks.

**Scope:**
- Default: Apply to parent obstacle only
- Explicit: Apply to specific obstacle by ID (for cross-obstacle knowledge)
- Universal: Apply everywhere (rare)

---

## Property Scale Meanings

**0 - Trivial:** Approach is straightforward, minimal challenge
**1 - Minor:** Low difficulty, standard costs
**2 - Significant:** Moderate difficulty, notable investment  
**3 - Severe:** Extreme difficulty or impossible without modification

Properties gate goal availability. Goals check "maxPhysicalDanger ≤ 2" against obstacle's current properties.

---

## Consequence Types

### Resolution
Obstacle permanently overcome. Marked as Resolved, removed from active play.

**Benefits:** Decisive, clean completion, narrative closure
**Downsides:** Highest costs, often violent, irreversible
**Sets:** ResolutionMethod (Violence/Authority typical), RelationshipOutcome (often Hostile or Neutral)

### Bypass
Player passes, obstacle persists for world.

**Benefits:** Low cost, quick, reversible, no relationship damage
**Downsides:** Loose end persists, no relationship built, no authority gained
**Sets:** ResolutionMethod (Stealth/Cleverness typical), RelationshipOutcome (Neutral)

### Transform
Obstacle fundamentally changed. All properties set to zero, description updated.

**Benefits:** Builds relationships, earns authority, eases future
**Downsides:** Time investment, creates obligations, relationship maintenance
**Sets:** ResolutionMethod (Diplomacy/Authority typical), RelationshipOutcome (Friendly/Allied/Obligated)

### Modify
Obstacle properties reduced by specified amounts. Other goals may unlock.

**Benefits:** Incremental progress, unlocks approaches, flexible
**Downsides:** Doesn't resolve, requires follow-up, higher total investment
**Sets:** ResolutionMethod (Preparation), RelationshipOutcome (usually Neutral)

### Grant
Player receives knowledge cards or items. Obstacle unchanged.

**Benefits:** Tactical advantage, informs decisions, low risk
**Downsides:** No progress, time spent, must leverage tactically
**Sets:** No resolution method (obstacle not resolved)

---

## Goal Effects

What happens when goal succeeds:

**Resources:** Coins, items added to player inventory

**Property Changes:** Reduce PhysicalDanger/SocialDifficulty/MentalComplexity by fixed amounts

**Transform:** New description for obstacle when transformed

**Knowledge:** Grant cards scoped to obstacle or universal

**Relationships:** Modify relationship value with associated NPC

**State:** Set ResolutionMethod and RelationshipOutcome on obstacle

---

## Goal Requirements

Gates when goals are available to player:

**Property Thresholds:**
- MaxPhysicalDanger: Obstacle's PhysicalDanger must be ≤ this
- MaxSocialDifficulty: Obstacle's SocialDifficulty must be ≤ this  
- MaxMentalComplexity: Obstacle's MentalComplexity must be ≤ this

**Player State:**
- RequiredKnowledgeIds: Must have completed specific goals (references goal local IDs within this obstacle)
- RequiredItemIds: Must possess these items
- MinStatLevel: Must have stat at this level (rare)

**Dependencies:**
- RequiredGoalIds: Must complete these goals first (local IDs)
- BlockedByGoalIds: Cannot do if these are complete (local IDs)

System continuously evaluates requirements. When properties change via Modify goals, other goals automatically become available.

---

## Storage Architecture

**GameWorld:** Dictionary of all active obstacles by ID

**Locations:** List of obstacle IDs that have goals appearing at this location

**NPCs:** List of obstacle IDs that have goals appearing with this NPC

**UI Flow:** Player visits location → get location's obstacle IDs → for each obstacle, find goals with PlacementLocationId matching location → show goals that meet requirements

---

## Distributed Interaction Pattern

Key architectural feature: One obstacle can have goals scattered across multiple locations.

**Example - Gatekeeper Obstacle:**
- Goal "Pay Fee" appears at North Gate (PlacementLocationId: north_gate)
- Goal "Ask About Miller" appears at Town Square (PlacementLocationId: town_square)  
- Goal "Get Official Pass" appears at Town Hall (PlacementLocationId: town_hall)
- All three goals are inline within the gatekeeper obstacle
- All three affect the same obstacle's properties
- Player discovers connections through exploration

This creates strategic preparation gameplay where you improve situations from multiple angles before committing to resolution.

---

## Template System (AI Generation Only)

Templates guide AI content generation. They are NOT runtime entities.

### Obstacle Archetypes

High-level patterns with property ranges and narrative guidance.

**Authority Gate archetype:**
- PhysicalDanger 1-2, SocialDifficulty 1-2, MentalComplexity 0-1
- Narrative: Someone with power blocking access
- Common in: Town gates, checkpoints, bureaucratic barriers

**Physical Barrier archetype:**
- PhysicalDanger 1-3, SocialDifficulty 0, MentalComplexity 1-3  
- Narrative: Environmental obstacle blocking path
- Common in: Collapsed passages, natural hazards, broken structures

**Hostile Entity archetype:**
- PhysicalDanger 2-3, SocialDifficulty 2-3, MentalComplexity 1-2
- Narrative: Aggressive actor requiring resolution
- Common in: Bandits, wild animals, hostile encounters

### Goal Templates

Common approach patterns with typical structure.

**Direct Confrontation:**
- Physical, Resolution consequence
- Requires PhysicalDanger ≤ 2
- High resource costs, violent resolution method

**Negotiate Passage:**
- Social, Bypass consequence  
- Requires SocialDifficulty ≤ 1
- Costs coins, diplomatic resolution method

**Build Relationship:**
- Social, Transform consequence
- Requires SocialDifficulty ≤ 2
- Time investment, friendly outcome, diplomatic method

**Gather Intelligence:**
- Mental, Grant consequence
- Requires MentalComplexity ≤ 2
- Grants knowledge cards, no obstacle change

**Establish Authority:**
- Social, Modify consequence
- Requires SocialDifficulty ≤ 2
- Reduces properties, grants items, authority method

**Find Workaround:**
- Mental, Bypass consequence
- Requires MentalComplexity ≤ 2  
- Low cost, cleverness method

### AI Generation Process

When investigation phase activates:

1. AI selects appropriate obstacle archetype for narrative context
2. AI generates obstacle properties within archetype ranges
3. AI selects 3-6 goal templates that fit obstacle type
4. AI generates complete obstacle with inline goals
5. AI writes contextual descriptions for obstacle and each goal
6. AI generates knowledge card text for Grant goals
7. System spawns complete obstacle in GameWorld

No runtime template matching. No string comparisons. Templates are generation guidance only.

---

## Structural Patterns

Common obstacle configurations that create different player experiences:

### Simple Fork
Multiple immediate resolution options at same location. Player chooses between direct trade-offs.

Properties all at 0-2 (manageable). Multiple goals with different consequences appear at obstacle location.

### Distributed Preparation  
Resolution options at one location, preparation scattered elsewhere. Player discovers and chains improvements.

One property at 2-3 initially. Modify goals at distant locations reduce properties, unlocking better approaches.

### Sequential Unlocking
Early goals unlock later goals through property reduction. Player breaks down difficult obstacle step by step.

One property starts at 3 (impossible). Modify goals required first, reducing to 2, then 1, then resolution possible.

### Knowledge Chain
Grant goals provide knowledge that unlocks optimal approaches. Player chooses between immediate costly success or delayed optimal success.

Multiple costly approaches available immediately. Grant goal unlocks better approach via knowledge requirement.

### Branching Transformation
Early transformation fundamentally changes obstacle nature. Different transformations lead to different downstream possibilities.

Transform goals set different RelationshipOutcomes. Future goals check outcome to determine availability.

---

## Runtime Flow

### Obstacle Spawning

Investigation phase (or travel system) activates:
- Spawn obstacle with generated properties
- Add to GameWorld.Obstacles
- For each inline goal, check PlacementLocationId/NpcId
- Add obstacle ID to appropriate Location/NPC lists

### Player Visits Location

UI displays available goals:
- Get location's obstacle ID list
- For each obstacle, examine inline goals
- Filter goals where PlacementLocationId matches this location  
- Check each goal's requirements against obstacle properties and player state
- Show goals that meet all requirements as buttons

### Player Attempts Goal

Challenge launch:
- Validate requirements still met
- Get challenge deck by DeckId
- Find knowledge cards scoped to this obstacle
- Add knowledge cards to challenge deck
- Launch Mental/Physical/Social challenge

### Goal Success

Apply effects:
- Award coins and items
- Reduce obstacle properties if Modify/Transform
- Update obstacle description if Transform
- Set ResolutionMethod and RelationshipOutcome
- Grant knowledge cards if Grant consequence
- Modify relationship values
- Mark obstacle state (Resolved if Resolution)

### Automatic Re-evaluation

After effects applied:
- System checks all goals in this obstacle
- Requirements re-evaluated against new properties
- Goals become visible/hidden automatically
- No manual state management needed

---

## Knowledge Card System

Cards granted by goals, available in future challenges.

**Granted When:** Goal with Grant consequence succeeds, or any goal includes card grant in effects

**Scoped To:** Parent obstacle by default (implicitly scoped), specific obstacle by ID (rare), or universal (very rare)

**Added To Challenge:** When player attempts any goal in scoped obstacle, system finds all granted cards and adds to challenge deck

**Card Effects:** Should feel qualitatively different, not just better numbers. Examples:
- Generate cross-system resources (Physical card that generates Leads)
- Cost reduction mechanics (next Statement costs no Initiative)
- Damage mitigation (ignore first Danger increase)
- Resource conversion (spend Progress to reduce Exposure)

---

## State Tracking Enums

### ObstacleState
- Active: Currently blocking
- Resolved: Permanently overcome, removed from play
- Transformed: Fundamentally changed, still exists but neutral

### ResolutionMethod  
How obstacle was overcome (for AI narrative context):
- Unresolved: Not yet overcome
- Violence: Forced, destroyed, attacked
- Diplomacy: Negotiated, befriended, persuaded
- Stealth: Sneaked, avoided, bypassed undetected
- Authority: Official channels, credentials, legal power
- Cleverness: Outsmarted, found workaround, exploited weakness
- Preparation: Methodical reduction over multiple attempts

### RelationshipOutcome
Social impact (for AI narrative and future goal availability):
- Hostile: Made enemies, damaged relationships
- Neutral: No relationship established or maintained
- Friendly: Built positive relationship
- Allied: Deep alliance formed, strong bond
- Obligated: Favors owed or owing, future expectations

These enums provide AI with semantic context for narrative generation and enable goal requirements based on obstacle state without string matching.

---

## Design Principles

**Separation of concerns:** Obstacles are property containers. Goals are challenge launchers. Challenges are tactical gameplay.

**Inline composition:** Goals live inside obstacles. No separate goal entities, no complex targeting.

**Simple gating:** Property thresholds and knowledge requirements. No formulas, no string matching.

**Dynamic availability:** Requirements continuously evaluated. Property changes automatically update visible goals.

**Distributed interaction:** One obstacle, goals across world. Preparation happens at different locations than resolution.

**Template guidance:** AI uses templates to generate content. Templates are not runtime entities.

**Impossible choices:** Each consequence has genuine trade-offs. Player expresses values through accepted costs.

**Satisfying victories:** Downsides come from choice context, not punishment. Success feels earned.