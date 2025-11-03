# WAYFARER SCENE/SITUATION ARCHITECTURE - CONCEPTUAL REASONING

## THE FUNDAMENTAL PROBLEM: ENTITY ROLE CONFUSION

### What Is A Situation, Really?

The current Situation entity has accumulated 60+ properties across 16 different conceptual domains because we haven't clearly defined WHAT A SITUATION IS at the conceptual level.

**The Core Question:** Is a Situation...
- A narrative moment? (Description, NarrativeHints)
- A placement container? (PlacementLocation, PlacementNpc, PlacementRouteId)
- A spawn tracker? (SpawnedDay, SpawnedTimeBlock, CompletedDay)
- A requirement definition? (CompoundRequirement, ProjectedBondChanges)
- A consequence definition? (ConsequenceType, SetsResolutionMethod, TransformDescription)
- A template instance? (TemplateId, Template, ParentSituationId)
- A tactical challenge? (SystemType, DeckId, Category, ConnectionType)
- A state machine? (State, Status, IsAvailable, IsCompleted)

**The answer:** It's trying to be ALL of these simultaneously. This is the root cause.

### Why This Happens: The God Object Anti-Pattern

When an entity is the "entry point" for a subsystem, developers naturally add properties to it because "that's where the data lives." Over time, the entity becomes a dumping ground for any information remotely related to the concept.

**The Symptom:** Opening Situation.cs and spending 30 seconds scanning to find the property you need because there are 60+ properties with no clear grouping.

**The Disease:** Lack of clear conceptual boundaries about what BELONGS to Situation vs. what is CONTEXT that Situation exists within.

---

## THE CONCEPTUAL MODEL: SEPARATION OF CONCERNS

### Situation Is A Narrative Moment (Core Identity)

At its essence, a Situation represents a specific narrative moment where the player makes a meaningful choice. That's it. Everything else is either:
- **Context** (where it appears, when it appeared)
- **Configuration** (how it behaves, what it costs)
- **Lifecycle** (what state it's in)

**Core Identity Properties (belongs to Situation):**
- Id - uniqueness
- Name - what it's called
- Description - what's happening narratively
- InteractionType - how player engages (instant/challenge/navigation)
- Type - narrative weight (Normal/Crisis)

Everything else should be questioned: Does this define WHAT the situation IS, or does it define WHERE/WHEN/HOW it exists?

### The Placement Problem: Who Owns Location?

**Current State:** Both Scene AND Situation store placement information
- Scene has: PlacementType (enum), PlacementId (string)
- Situation has: PlacementLocation (object), PlacementNpc (object), PlacementRouteId (string), ParentScene (object)

**The Architectural Question:** If Scene already knows where it's placed, why does Situation store duplicate placement data?

**The Answer Reveals The Problem:** Because code was written that queries Situation directly without going through Scene. This created a "shortcut" dependency that bypassed the proper hierarchy.

**The Correct Conceptual Model:**
- **Scene** = A narrative container that OWNS placement (it's placed at a location/NPC/route)
- **Situation** = A narrative moment WITHIN a scene (it inherits context from parent)

**Why This Matters (Deeply):**

When you move a Scene from one location to another (hypothetically, for a dynamic event), do you want to update:
- ONE place (Scene.PlacementId) → Situations inherit automatically
- MULTIPLE places (Scene.PlacementId + every Situation.PlacementLocation) → Risk of desync

**This is HIGHLANDER Pattern A in action:** Scene has the ID (persistence), parser resolves to object (runtime), Situations QUERY the object when needed (no duplicate storage).

**The Placement Is Not A Property Of The Situation - It's A Property Of The SCENE The Situation Is Embedded Within.**

This is a fundamental shift in mental model: Situations don't "have" placement, they "inherit context from their container."

---

## THE STATUS REDUNDANCY: TWO TRUTHS FOR ONE CONCEPT

**Current State:**
```
Status: SituationStatus enum (Available/InProgress/Completed)
IsAvailable: bool
IsCompleted: bool
```

**The Problem:** Three different ways to represent the same concept (situation lifecycle state).

**Why This Happened:** Historical accumulation
1. First, Status enum was added (proper state machine)
2. Later, IsAvailable boolean was added (convenience check)
3. Later, IsCompleted boolean was added (another convenience check)
4. Now Complete() method sets BOTH Status enum AND IsCompleted boolean

**The Conceptual Error:** Booleans are COMPUTED PROPERTIES, not STORAGE properties.

**The Mental Model Shift:**

State machines should have ONE source of truth (the enum), with booleans as VIEWS into that truth:
- IsAvailable → computed from Status == Available
- IsCompleted → computed from Status == Completed

**Why This Matters (Deeply):**

When you have two sources of truth, you create the risk of:
1. Setting Status = Completed but forgetting IsCompleted = true (desync)
2. Code checking IsCompleted in one place, Status in another (inconsistency)
3. Future developer adding Status == InProgress check but IsCompleted is still false (confusion)

**The Desync Is Not Just A Bug Risk - It's An Architectural Violation Of Single Source Of Truth.**

Every piece of information should be stored EXACTLY ONCE, with all other access being queries/computations from that source.

---

## THE SPAWN TRACKING CONCEPTUAL MODEL

**Current State:**
```
SpawnedDay, SpawnedTimeBlock, SpawnedSegment (when created)
CompletedDay, CompletedTimeBlock, CompletedSegment (when finished)
ParentSituationId (who spawned this)
TemplateId (what template spawned this)
```

**The Question:** Is spawn tracking part of Situation's identity, or is it lifecycle metadata?

**The Conceptual Distinction:**

**Identity** = What makes this Situation unique (Id, Name, Description)
**Lifecycle Metadata** = Historical tracking of when things happened

**Why This Matters:** Identity properties are NECESSARY for the entity to function. Lifecycle metadata is OPTIONAL - useful for analytics/debugging but not required for gameplay.

**The Mental Model:** Spawn tracking is like a "receipt" for the Situation's creation - it documents the transaction but isn't part of the transaction itself.

**Practical Implication:** If you removed all spawn tracking properties, would Situation still function? YES. If you removed Description? NO. This reveals which properties are CORE vs. PERIPHERAL.

**The Deeper Principle:** Entities should be organized by conceptual necessity, not convenience. Just because spawn tracking is "about" Situations doesn't mean it BELONGS on the Situation entity.

---

## THE REQUIREMENT SYSTEM: CONFIGURATION VS. IDENTITY

**Current State:**
```
CompoundRequirement (unlock requirements)
ProjectedBondChanges (shown consequences)
ProjectedScaleShifts (shown consequences)
ProjectedStates (shown consequences)
DifficultyModifiers (situational difficulty changes)
```

**The Question:** Are these properties defining WHAT the Situation IS, or HOW the Situation BEHAVES?

**The Conceptual Distinction:**

**WHAT** = Situation is a negotiation with a merchant
**HOW** = Situation requires Rapport 3 OR Money 10, grants +2 Merchant bond, shown to player transparently

**The Mental Model Shift:** Requirements and consequences are CONFIGURATION DATA that control behavior, not IDENTITY DATA that defines existence.

**Why This Matters (Architecturally):**

When you design a system, you want to separate:
- **Invariants** (always true about the entity)
- **Configuration** (varies by instance but doesn't change identity)

A Situation with different requirements is STILL THE SAME SITUATION CONCEPTUALLY (negotiation with merchant). The requirements don't change its nature, they parameterize its behavior.

**The Deeper Implication:** If requirements are configuration, they should be grouped as "SituationConfiguration" or similar, making it explicit that these properties are parameters, not core identity.

---

## THE CONSEQUENCE SYSTEM: EFFECT VS. IDENTITY

**Current State:**
```
ConsequenceType (Resolution/Bypass/Transform/Modify/Grant)
SetsResolutionMethod (for AI narrative context)
SetsRelationshipOutcome (affects future interactions)
TransformDescription (new description if Transform)
```

**The Question:** Is the consequence part of what the Situation IS, or is it the EFFECT of completing the Situation?

**The Conceptual Error:** Conflating "what happens" with "what it is."

**The Mental Model:**

A situation is like a button. The button has:
- **Identity:** Label, appearance, location
- **Behavior:** What happens when pressed

The consequence is the BEHAVIOR, not the identity. Storing consequence type on Situation is like storing "door opens" on a button - it's mixing metaphors.

**Why This Matters (Philosophically):**

In domain-driven design, entities represent CONCEPTS, not PROCEDURES. A Situation represents a moment of choice. The consequences are PROCEDURES that execute after choice is made.

**The Correct Model:** Situations have OUTCOMES (data about what happens), not PROCEDURES (logic to execute). The ConsequenceFacade handles EXECUTION, Situation just declares WHAT SHOULD HAPPEN.

**This Is Why We Have Facades:** To separate WHAT (domain entities) from HOW (execution logic).

---

## THE STATE MACHINE: DORMANT → ACTIVE

**Current Implementation:**
```
State: SituationState enum (Dormant/Active)
```

**Why This Is Correct:** This is one of the few properly designed properties!

**The Conceptual Model:** Query-Time Action Instantiation (Tier 3)

**Why Actions Aren't Created Immediately:**

When a Scene spawns with 3 Situations, why not create all 12 actions (4 choices × 3 situations) immediately?

**Memory Efficiency Answer:** Player might never see 2 of those situations (different routes chosen), wasting memory.

**Architectural Answer:** This is the WRONG answer. Memory is cheap.

**The REAL Answer:** Situations are TEMPLATES at spawn time. They don't have concrete actions until player ENTERS THE CONTEXT that makes them actionable.

**The Deep Principle:** Lazy Evaluation

Dormant Situations exist as POTENTIALITY - they CAN spawn actions, but haven't YET. Active Situations exist as ACTUALITY - they HAVE spawned actions and are actionable.

**Why This Matters (Philosophically):**

This is the difference between:
- **Potential:** The situation exists in the game world
- **Actual:** The player is in the context where the situation is actionable

The state machine models this ontological distinction: Dormant = potential, Active = actualized.

**This Is Why Template Property Exists:** Situations carry their Template so they can actualize into concrete Actions when the context demands it.

---

## THE PLACEMENT INHERITANCE MODEL

**The Core Conceptual Problem:** Situations need to know where they appear in the UI, but storing placement directly violates single source of truth.

**The Question:** How do we model "Situation appears at Location X" without storing Location X on Situation?

**The Wrong Model:** Direct Storage
```
Situation stores PlacementLocation → Direct property → Easy to query → DUPLICATE DATA
```

**The Correct Model:** Inherited Context
```
Situation has ParentScene → Scene has PlacementId → Situation QUERIES Scene for placement → SINGLE SOURCE OF TRUTH
```

**Why This Is Hard To Accept:** It feels inefficient. "Why not just store it directly?"

**The Counterargument:** Efficiency is not the primary concern. CORRECTNESS is.

**What Happens When Scene Moves?**

Imagine a "traveling merchant" scene that moves between locations:
- **Direct Storage Model:** Update Scene.PlacementId + update every Situation.PlacementLocation → N+1 updates, risk of forgetting one
- **Inherited Context Model:** Update Scene.PlacementId → Situations automatically inherit new placement → 1 update, zero risk

**This Is HIGHLANDER In Action:** ONE place stores the truth, EVERYONE ELSE queries it.

**The Deeper Principle:** Derived Properties Should Not Be Stored

When Property B can be COMPUTED from Property A, storing both creates desync risk. Store A, compute B on demand.

PlacementLocation is DERIVED from Scene.PlacementId (via lookup in GameWorld). Therefore, storing PlacementLocation violates this principle.

---

## THE SPAWN CONDITIONS SYSTEM (NOT YET IMPLEMENTED)

**The Conceptual Question:** When should a Scene be eligible to spawn?

**Current State:** PlacementFilter exists (WHERE to spawn) but no SpawnConditions (WHEN to spawn).

**The Distinction:**

**PlacementFilter** = "This scene can spawn on NPCs with Merchant profession and Bond 3+"
**SpawnConditions** = "This scene can only spawn if player has completed Promise scene AND 2+ days have passed"

**Why Both Are Needed:**

PlacementFilter answers: "Which entities match?" (entity selection)
SpawnConditions answers: "Is NOW the right time?" (temporal eligibility)

**The Mental Model:** Two-Stage Filtering

Stage 1: Is the template eligible RIGHT NOW? (SpawnConditions)
Stage 2: Which entities match the filter? (PlacementFilter)

If Stage 1 fails, don't even check Stage 2 (performance + correctness).

**Why This Matters (Deeply):**

Without SpawnConditions, you can only control WHERE scenes spawn, not WHEN. This prevents time-based narrative flows.

**Example:** "Marcus's business fails 3 days after you refused help"

With PlacementFilter alone: Scene spawns at Marcus immediately after refusal
With SpawnConditions: Scene waits 3 days THEN spawns at Marcus

**The spawn timing is a PROPERTY OF THE NARRATIVE, not a property of Marcus.** SpawnConditions models this correctly.

---

## THE SELECTION STRATEGY SYSTEM (NOT YET IMPLEMENTED)

**The Conceptual Question:** When multiple entities match PlacementFilter, which one do we choose?

**Current State:** First match is used (arbitrary).

**The Problem:** "First" has no semantic meaning. Why should the first NPC in the list be preferred over others?

**The Correct Mental Model:** Explicit Priority

Selection should be INTENTIONAL, not ACCIDENTAL. If multiple NPCs match, the game designer should specify:
- Prefer highest bond (story continuity)
- Prefer least recently interacted (variety)
- Prefer closest to player (convenience)
- Prefer random (unpredictability)

**Why This Matters (Philosophically):**

Implicit defaults hide design intent. If code uses "first match," future developers don't know if:
1. Designer intended first match (explicit choice)
2. Developer used first match as quick implementation (accident)

Explicit SelectionStrategy documents design intent: "I WANT highest bond, and here's why."

**The Deeper Principle:** Make Implicit Decisions Explicit

Every decision the system makes should be traceable to a design choice, not an implementation accident.

---

## THE TRIGGER EVENT MODEL

**The Conceptual Question:** What causes the system to check if scenes should spawn?

**Current State:** 4 of 6 triggers implemented (Location entry, Route begin, NPC interaction, Action completion). Missing: Time advancement, Obligation completion.

**The Mental Model:** Event-Driven Spawning

Scenes don't spawn "randomly" - they spawn in RESPONSE to player actions or world state changes.

**The Six Trigger Categories:**

1. **Spatial Triggers:** Player enters location → Check scenes with PlacementFilter matching location
2. **Social Triggers:** Player talks to NPC → Check scenes with PlacementFilter matching NPC
3. **Action Triggers:** Player completes action → Check scenes with SceneSpawnReward in action
4. **Temporal Triggers:** Day advances → Check scenes with time-based SpawnConditions
5. **Progression Triggers:** Obligation phase completes → Check scenes with phase rewards
6. **Recursive Triggers:** Situation completes → Check SuccessSpawns/FailureSpawns

**Why Six, Not Sixty?**

These six categories cover ALL possible spawn triggers because they represent the fundamental ways player state changes:
- Where they are (Spatial)
- Who they're with (Social)
- What they do (Action)
- When it is (Temporal)
- What milestone reached (Progression)
- What cascades (Recursive)

Any other trigger would be a sub-category of these six.

**The Deeper Principle:** Completeness Through Orthogonality

The trigger categories are ORTHOGONAL (independent). You can't derive one from another. This ensures the system is COMPLETE (covers all cases).

---

## THE DELAYED SPAWNING MODEL

**The Conceptual Question:** How do we model "spawn this scene 3 days from now"?

**The Wrong Model:** Callbacks/Events
```
When action completed → Set timer → Fire callback 3 days later
```

**Why Wrong:** Callbacks don't persist across save/load. Timer state is hidden.

**The Correct Model:** Declarative Scheduling
```
When action completed → Add PendingSpawn to GameWorld with SpawnOnDay = CurrentDay + 3
On day advance → Check PendingSpawns, execute ready ones
```

**Why Correct:** PendingSpawns are PERSISTED DATA. Save/load works automatically. No hidden state.

**The Deeper Principle:** Data Over Code

Represent future intentions as DATA (pending spawns), not CODE (callbacks). Data serializes naturally. Code doesn't.

**The Mental Model:** The Pending Spawn Is A Promise

When player makes a choice, they're making a PROMISE to the game world: "This scene will appear in 3 days."

The PendingSpawn entity is the RECORD of that promise. On day 3, the system FULFILLS the promise by spawning the scene.

**This Is Why It's Called "Pending":** It's waiting to be fulfilled, but the promise exists NOW (in GameWorld).

---

## THE SCENE EXPIRATION MODEL

**The Conceptual Question:** How do we model "this scene is only available for 5 days"?

**The Mental Model:** Time-Limited Opportunities

Some narrative moments are ephemeral - they vanish if player doesn't act quickly. This creates urgency and consequence.

**Why NOT Use Callbacks:**

Same reason as delayed spawning - callbacks don't persist, state is hidden.

**The Correct Model:** Expiration Metadata + Checker
```
SceneTemplate has ExpirationDays (metadata)
Scene stores SpawnedDay (when created)
On day advance → Check if (CurrentDay - SpawnedDay) > ExpirationDays → Expire scene
```

**Why Correct:** All state is VISIBLE in Scene entity. No hidden timers. Save/load works automatically.

**The Deeper Principle:** Explicit Over Implicit

The scene's expiration state should be QUERYABLE at any time:
- "When was this scene spawned?" → SpawnedDay
- "How long is it valid?" → Template.ExpirationDays
- "When does it expire?" → SpawnedDay + ExpirationDays
- "Is it expired?" → CurrentDay > (SpawnedDay + ExpirationDays)

All questions answerable from DATA, not CODE.

---

## THE AI NARRATIVE GENERATION MODEL

**The Conceptual Question:** How do we generate contextually appropriate narrative?

**The Wrong Model:** Hardcoded Templates
```
NarrativeTemplate: "Marcus the merchant confronts you at the tavern."
```

**Problem:** Only works for Marcus at tavern. Doesn't adapt to different NPCs/locations.

**The Correct Model:** Placeholders + AI Generation
```
NarrativeTemplate: "{NPCName} the {NPCProfession} confronts you at the {LocationName}."
NarrativeHints: { tone: "tense", theme: "business dispute" }
```

**At spawn time:**
1. Replace placeholders with actual entity data
2. OR, if template empty, call AI generator with hints + context
3. Cache generated narrative on Situation

**Why This Works:**

The template is a MECHANICAL SKELETON. The placeholders are SUBSTITUTION POINTS. The AI fills in NARRATIVE FLESH.

**The Deeper Principle:** Separation of Structure and Content

The STRUCTURE of the narrative moment is fixed (confrontation at location).
The CONTENT is dynamic (which NPC, which location, what tone).

Templates provide structure. Placeholders + AI provide content.

**Why Verisimilitude Matters:**

If scene says "Marcus confronts you at Copper Kettle Tavern" but scene is actually spawned at Thomas at Market Square, the narrative BREAKS IMMERSION.

Placeholders ensure MECHANICAL TRUTH (entity IDs) matches NARRATIVE TRUTH (entity names in text).

**This Is Why Finalization Replaces Placeholders:** Provisional scenes have placeholder skeletons. Active scenes have concrete narratives.

---

## THE ROUTE FILTERING MODEL (INCOMPLETE)

**The Conceptual Question:** How do we model "this scene spawns on mountain wilderness routes"?

**Current State:** PlacementFilter can target Route type, but Route entity lacks properties for filtering (TerrainType, Tier, DangerRating).

**The Missing Piece:** Categorical Properties on Routes

NPCs have PersonalityType, Locations have LocationProperties → These enable categorical filtering
Routes need TerrainType, Tier, DangerRating → Same pattern

**Why This Matters (Deeply):**

Without categorical properties, PlacementFilter can only target:
- Specific routes by ID (too rigid - hardcoded)
- All routes (too broad - no selectivity)

With categorical properties, PlacementFilter can target:
- Mountain terrain routes (natural category)
- High-tier routes (difficulty category)
- High-danger routes (risk category)

**The Principle:** Categorical Filtering Requires Categorical Properties

You can't filter by categories that don't exist. This seems obvious but is profound:

If the domain model doesn't represent a concept, the filtering system can't use that concept.

**Adding terrain/tier/danger to Route isn't "just adding properties" - it's ENRICHING THE DOMAIN MODEL with concepts that enable new kinds of filtering.**

---

## THE HOLISTIC INTEGRATION PRINCIPLE

**The Meta-Question:** Why does all of this architectural reasoning matter?

**The Answer:** Because architecture is FRACTAL. Bad decisions at the entity level propagate to the system level.

**Example Propagation:**

Bad: Situation stores PlacementLocation directly
→ Creates duplicate state (Scene also has placement)
→ Facades must decide which to query (inconsistency)
→ Moving scenes requires N+1 updates (fragility)
→ Future features built on wrong pattern (debt compounds)

Good: Situation queries Scene for placement
→ Single source of truth (Scene owns placement)
→ Facades always query Scene (consistency)
→ Moving scenes requires 1 update (robustness)
→ Future features follow correct pattern (debt avoided)

**The cascade of consequences from ONE architectural decision is immense.**

**The Deeper Principle:** Architecture Is Prediction

When you design an entity, you're predicting:
- What properties it needs
- What relationships it has
- What operations it supports
- What guarantees it provides

Good architecture makes CORRECT predictions. Bad architecture makes WRONG predictions that future developers must work around.

**This Is Why Phase 0 Is Critical:** Fix the foundational predictions before building features on top. Otherwise, every new feature inherits the foundational flaws.

---

## THE SYNTHESIS: WHY THIS PLAN EXISTS

**The Fundamental Insight:** The Scene/Situation system is 85% correct architecturally. The remaining 15% are concentrated violations that must be addressed before expansion.

**The Three Critical Violations:**

1. **Situation property proliferation** → Violates single responsibility → Fix by extracting logical groups
2. **Placement reference duplication** → Violates HIGHLANDER → Fix by making Scene authoritative
3. **Status tracking redundancy** → Violates single source of truth → Fix by making Status enum authoritative

**Why Fix These First:**

Adding SpawnConditions to a Situation with 60+ properties → Adds to confusion
Adding SelectionStrategy to placement logic with duplicate references → Adds to inconsistency
Adding time triggers to status tracking with two truths → Adds to desync risk

**Clean the foundation → Build on solid ground → Additions remain clean.**

**The Architectural Principle:** Refactor Before Extend

When a system has violations, don't extend it - that propagates violations to new code. Instead:
1. Refactor to remove violations
2. Validate correctness
3. THEN extend with new features

This is why the plan has 8 phases with Phase 0 as foundation - it's not arbitrary sequencing, it's CORRECT sequencing based on dependency relationships.

---

## CONCLUSION: THE ARCHITECTURAL MINDSET

This document explains the REASONING behind every decision in the implementation plan. The key insights:

1. **Entities should represent concepts, not convenience** → Situation is a narrative moment, not a god object
2. **Derived properties should not be stored** → Placement is derived from Scene, status booleans derived from enum
3. **Configuration should be separated from identity** → Requirements are parameters, not essence
4. **Lazy evaluation enables efficiency** → Dormant → Active state machine models potentiality → actuality
5. **Explicit is better than implicit** → SelectionStrategy documents intent, SpawnConditions documents timing
6. **Data over code** → Pending spawns and expiration are data, not callbacks
7. **Structure and content are separate** → Templates provide structure, placeholders + AI provide content
8. **Domain richness enables filtering** → Categorical properties on Routes enable categorical filtering
9. **Architecture is fractal** → Entity-level decisions propagate to system-level consequences
10. **Refactor before extend** → Clean foundation → Clean additions

**The implementation plan exists because the reasoning in this document demands it.** The plan is the MANIFESTATION of these principles, not arbitrary file changes.
