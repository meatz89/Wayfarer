# Wayfarer Technical Glossary

## PURPOSE

This document provides canonical definitions for all specialized terms used across Wayfarer documentation and codebase. When terminology conflicts arise, this glossary is authoritative.

**Last Updated:** 2025-01 (Post-scene architecture refactoring)

---

## SPATIAL HIERARCHY

### Venue
**Type:** Entity
**Owner:** GameWorld.Venues
**Definition:** Cluster of related Locations within hex radius (typically 1-2 hexes). Maximum 7 Locations per Venue. Represents conceptual grouping (e.g., "The Rusty Tankard Inn").
**Movement:** Travel BETWEEN Venues requires Route travel with Scenes. Movement WITHIN Venue is instant.
**Example:** "The Rusty Tankard Inn" (venue) contains "Common Room" (location), "Upper Floor" (location), "Cellar" (location).

### Location
**Type:** Entity
**Owner:** GameWorld.Locations
**Definition:** Specific place within a Venue where player can be, NPCs can be present, and Scenes can be placed. Atomic unit of spatial positioning.
**Properties:** Name, VenueId (reference to parent Venue), HexCoordinates, IsVenueTravelHub (exactly one per Venue).
**Movement:** Players navigate between Locations within same Venue instantly. Travel to Locations in different Venues requires Route travel.
**Historical Note:** Earlier architecture used "Spot" as synonym. Current codebase uses "Location" exclusively.
**Example:** "Common Room" (specific location within The Rusty Tankard Inn).

### Hex
**Type:** Entity (procedural)
**Owner:** GameWorld.WorldHexGrid.Hexes
**Definition:** Grid cell in hex-based world map. Contains terrain type, danger level, and optional LocationId. Used for pathfinding and procedural generation.
**Visibility:** Never directly visible to player. Backend scaffolding for Route generation.
**Properties:** AxialCoordinates (Q, R), TerrainType enum, DangerLevel (0-10), LocationId (optional reference).

### Route
**Type:** Entity
**Owner:** GameWorld.Routes
**Definition:** Bidirectional travel path connecting two Venue hub Locations. Contains HexPath (sequence of Hex coordinates), DangerRating, TimeSegments, TransportType.
**Generation:** Currently AUTHORED in JSON packages. Hex-based procedural generation designed but not implemented.
**Usage:** Player selects Route, initiates travel, experiences Situations during journey, arrives at destination.

---

## NARRATIVE ARCHITECTURE (SCENE SYSTEM)

### SceneTemplate
**Type:** Template (immutable)
**Owner:** GameWorld.SceneTemplates
**Definition:** Immutable archetype defining Scene structure. Contains embedded SituationTemplates, PlacementFilter (categorical entity selection), SpawnConditions (player/world/entity state requirements).
**Lifecycle:** Loaded at parse-time from JSON. Never modified at runtime. Spawns Scene instances via SceneInstantiator.
**Key Properties:** Id, Archetype (SpawnPattern enum), PlacementFilter, SpawnConditions, SituationTemplates (embedded list), Tier (0-4), ExpirationDays.

### Scene
**Type:** Runtime Entity (mutable)
**Owner:** GameWorld.Scenes
**Definition:** Persistent narrative container spawned from SceneTemplate. Contains embedded Situations, tracks CurrentSituation, manages state machine (Provisional/Active/Completed/Expired).
**Lifecycle States:**
- **Provisional:** Created for perfect information preview, Situations not yet instantiated. Can be finalized or discarded.
- **Active:** Finalized and playable. Situations instantiated, CurrentSituation set.
- **Completed:** All Situations finished. Scene persists but filtered from active queries.
- **Expired:** ExpiresOnDay reached. Scene persists but filtered from queries.
**Key Properties:** Id, TemplateId, PlacementType, PlacementId, Situations (embedded list), CurrentSituation (object reference), State (SceneState enum).
**Historical Note:** Replaces Goal/Obstacle architecture. Earlier docs may reference "Goals" - these are Scenes in current architecture.

### SituationTemplate
**Type:** Template (embedded in SceneTemplate)
**Storage:** SceneTemplate.SituationTemplates
**Definition:** Immutable template defining Situation structure. Contains ChoiceTemplates (2-4 choices per Sir Brante pattern) or ArchetypeId (for catalogue generation).
**Archetype Pattern:** If ArchetypeId specified, SituationArchetypeCatalog.GetArchetype() generates 4 ChoiceTemplates at parse-time.
**Key Properties:** Id, Type (Normal/Crisis), ArchetypeId (nullable string), ChoiceTemplates (embedded list), NarrativeTemplate, RequiredLocationId, RequiredNpcId.

### Situation
**Type:** Runtime Entity (embedded in Scene)
**Storage:** Scene.Situations
**Definition:** Narrative moment within Scene. Embedded in parent Scene (NOT in separate GameWorld collection). Starts with InstantiationState.Deferred, transitions to Instantiated when player enters context.
**State Machine:**
- **InstantiationState:** Deferred (actions not created) → Instantiated (actions created in GameWorld collections).
- **LifecycleStatus:** Selectable (available to player) → Completed/Failed (finished).
**Key Properties:** Id, Template (SituationTemplate reference), InstantiationState enum, LifecycleStatus enum, ParentScene (object reference), LastChoiceId, LastChallengeSucceeded (for conditional transitions).
**Three-Tier Timing:** Templates (parse-time) → Situations (spawn-time, deferred) → Actions (query-time).
**Historical Note:** Do NOT confuse with "Encounter" (route-travel synonym) or "Challenge" (tactical subsystem).

### ChoiceTemplate
**Type:** Template (embedded in SituationTemplate)
**Storage:** SituationTemplate.ChoiceTemplates
**Definition:** Immutable template defining choice structure. Contains PathType, RequirementFormula, CostTemplate, RewardTemplate, ActionType.
**PathType Enum:** InstantSuccess (immediate resolution), Challenge (enters tactical layer), Fallback (always available poor outcome).
**Key Properties:** Id, PathType enum, ActionTextTemplate (with {Placeholders}), RequirementFormula (CompoundRequirement), CostTemplate (ChoiceCost), RewardTemplate (ChoiceReward), ActionType enum, ChallengeType (if PathType.Challenge).

### Action (General)
**Type:** Runtime Entity (two distinct patterns)
**Definition:** Player-executable choice in UI. Actions exist in TWO architectural forms with different lifecycles:

**1. Static Atmospheric Actions:**
- **Owner:** GameWorld.LocationActions (ONLY LocationAction type)
- **Source:** Generated once at parse-time from LocationActionCatalog
- **Purpose:** Core gameplay freedom (Travel, Work, Rest, Intra-Venue Movement)
- **Lifecycle:** Created during package loading → Stored permanently in GameWorld.LocationActions → Never deleted
- **Availability:** Always present regardless of scene state (prevents dead ends and soft-locks)
- **Persistence:** Saved in save files, part of world state

**2. Ephemeral Scene-Based Actions:**
- **Owner:** None (created fresh on query, passed by object reference, NOT stored)
- **Source:** Generated at query-time from ChoiceTemplate when Situation is active
- **Purpose:** Narrative scene content (choices within Scene-Situation architecture)
- **Lifecycle:** Created when SceneFacade queries active Situation → Returned to UI directly → Discarded after execution
- **Availability:** Appear/disappear with parent Scene state
- **Persistence:** NOT saved (recreated from templates on game load)
- **Concrete Types:** LocationAction (scene at location), NPCAction (scene at NPC), PathCard (scene on route)

**CRITICAL:** LocationAction exists in BOTH forms. NPCAction and PathCard are ONLY ephemeral (no static equivalent).

**Historical Note:** Earlier architecture stored ephemeral actions in GameWorld collections. Current architecture passes ephemeral actions by direct object reference (no storage).

### Atmospheric Action
**Type:** Runtime Entity (persistent)
**Owner:** GameWorld.LocationActions
**Definition:** Always-available LocationAction providing core gameplay scaffolding independent of scene state. Prevents dead ends and soft-locks by ensuring player always has Travel/Work/Rest/Movement options.
**Generated:** Parse-time via LocationActionCatalog.GenerateActionsForLocation()
**Examples:**
- **Travel:** Initiate route travel from venue hub to another venue
- **Work:** Perform delivery jobs to earn coins
- **Rest:** Restore Health/Stamina by consuming resources (food/lodging)
- **Intra-Venue Movement:** Navigate between locations within same venue
**Design Philosophy:** Atmospheric actions form persistent baseline existence ensuring player freedom and forward progress regardless of narrative state. Scene-based actions layer on top as temporary narrative content.
**See Also:** Static Action (synonym), Ephemeral Action (opposite pattern), design/02_core_gameplay_loops.md lines 491-578

### Static Action
**Synonym:** Atmospheric Action (see above)
**Usage Note:** "Static" emphasizes parse-time generation and permanent storage. "Atmospheric" emphasizes design role as persistent scaffolding. Same concept, different emphasis.

### Ephemeral Action
**Type:** Runtime Entity (query-time instance)
**Owner:** None (not stored, passed by object reference)
**Definition:** Temporary action created fresh when SceneFacade queries active Situation, passed directly to UI, discarded after execution. Represents narrative choice within Scene-Situation architecture.
**Generated:** Query-time via SceneFacade.GetActionsAtLocation/GetActionsForNPC/GetPathCardsForRoute
**Lifecycle:** Created → Returned to UI → Passed to GameFacade execution → Discarded
**Three-Tier Timing:** Actions are Tier 3 (query-time instantiation from Tier 2 Situations which reference Tier 1 Templates)
**Examples:**
- NPCAction: "Negotiate with innkeeper" (from Scene placed at Elena)
- LocationAction: "Investigate crime scene" (from Scene placed at mill location)
- PathCard: "Help stranded traveler" (from Scene placed on route)
**Design Philosophy:** Ephemeral actions appear/disappear with parent Scene state, creating dynamic narrative content layered on top of persistent atmospheric actions.
**See Also:** Atmospheric Action (opposite pattern), Action (General) for complete distinction

---

## TIMING TERMINOLOGY

### Parse-Time
**When:** Game initialization, package loading.
**What Happens:** JSON parsed → DTOs created → Catalogues translate categorical properties → Templates stored in GameWorld collections.
**Example:** SituationArchetypeCatalog.GetArchetype("confrontation") generates 4 ChoiceTemplates from archetype pattern.
**One-Time:** Never repeats after initial load (unless new package loaded).

### Spawn-Time
**When:** Scene instantiation (trigger varies: progression, time advancement, player action).
**What Happens:** SceneInstantiator converts immutable Template into mutable Scene instance → Evaluates SpawnConditions → Resolves PlacementFilter → Creates Scene/Situations in GameWorld → AI generates narrative if template lacks narrative.
**Multiple Times:** Scenes spawn throughout gameplay as conditions met.

### Query-Time
**When:** Player enters context (location, NPC interaction, route travel).
**What Happens:** SceneFacade queries active Scene → Situation transitions from Deferred to Instantiated → ChoiceTemplates converted to concrete Actions → Actions added to GameWorld collections.
**Lazy Instantiation:** Actions NOT created at spawn-time. Created on-demand when needed.

### Runtime
**Broad Term:** Encompasses all execution after parse-time. Includes spawn-time, query-time, and player interaction time.
**Avoid Using:** Ambiguous. Use specific terms (spawn-time/query-time) for clarity.

---

## ARCHITECTURAL PATTERNS

### Catalogue Pattern
**Definition:** Parse-time translation from categorical JSON properties to concrete runtime types.
**Three Phases:**
1. **JSON (Authoring):** Categorical/relative properties ("Friendly", "Standard", "High")
2. **Parsing (Translation):** Catalogue converts categorical → concrete (int, bool, enum). **Happens ONCE at parse-time.**
3. **Runtime:** Use concrete properties directly. **NO catalogue calls, NO string matching, NO dictionaries.**
**Example:** SituationArchetypeCatalog converts archetypeId string → 4 ChoiceTemplates with concrete requirements/costs/rewards.
**FORBIDDEN:** Runtime catalogue calls. Catalogues ONLY imported in Parser classes, NEVER in Facades or Managers.

### HIGHLANDER Principle
**Definition:** "There can be only ONE." One concept, one representation. No redundant storage, no duplicate paths.
**Examples:**
- Situations stored in Scene.Situations (embedded), NOT in separate GameWorld.Situations collection.
- InstantiationState enum is single source of truth, computed properties derive from it.
- Templates reference entities by ID, resolved ONCE at parsing/spawning, cached as object reference.
**Anti-Pattern:** Storing both object reference AND ID for same entity (creates desync risk).

### Sentinel Values Over Null
**Definition:** Never use null for domain logic. Create explicit sentinel objects with internal flags.
**Example:** SpawnConditions.AlwaysEligible (sentinel with internal IsAlwaysEligible flag) instead of null SpawnConditions.
**Why:** Parser returns sentinel, evaluator checks flag, throw on actual null. Fail-fast prevents silent errors.

### Let It Crash
**Definition:** Initialize collections inline on entities. Never null-coalesce when querying. Let missing data crash with descriptive errors.
**Example:** `public List<Situation> Situations { get; set; } = new List<Situation>();` (inline initialization)
**FORBIDDEN:** `parsed ?? new List<T>()` (null-coalescing), `situations?.Select(...)` (null-propagation on properties that should never be null).
**Why:** Fails fast, easier debugging, single source of truth, forces fixing root cause.

### ID Antipattern
**Definition:** NEVER encode data in ID strings. NEVER parse IDs to extract data.
**FORBIDDEN:**
- Encoding: `Id = $"move_to_{destinationId}"`
- Parsing: `StartsWith("move_to_")`, `Substring()`, `Split()`
- Routing: `if (action.Id == "secure_room")`
**CORRECT:**
- ActionType enum for routing (switch on enum, NOT ID)
- Strongly-typed properties (DestinationLocationId property, not extracted from Id)
- IDs acceptable for: Uniqueness, debugging, simple passthrough. NEVER for logic.

---

## CONTENT GENERATION TERMINOLOGY

### Procedural Content
**Definition:** Content generated from rules/algorithms rather than manually authored. Includes Scene spawning from templates, route generation from hex pathfinding, AI narrative generation.
**Current Status:** Scene spawning from templates IMPLEMENTED. Hex-based route generation DESIGNED. AI narrative generation IN PROGRESS.

### Categorical Property
**Definition:** Descriptive/relative property in JSON that catalogues translate to concrete values. Enables AI content generation without knowing global game state.
**Examples:** NPCDemeanor (Friendly/Neutral/Hostile), Quality (Basic/Standard/Premium/Luxury), PowerDynamic (Dominant/Equal/Submissive).
**Why:** AI doesn't know current player progression/balance. Categorical properties scale dynamically at parse-time based on game context.

### 5-System Scene Spawning Architecture
**Definition:** Scene spawning flows through five distinct systems with clear separation of concerns. Each system has one responsibility.
**Systems:**
1. **Scene Selection (Decision Logic):** Evaluates SpawnConditions to decide WHEN to spawn (SceneFacade, SituationRewardExecutor)
2. **Scene Specification (Data Structure):** Stores categorical requirements ONLY in SceneSpawnReward (no concrete IDs)
3. **Package Generator (SceneInstantiator):** Creates JSON package with PlacementFilterDTO (categorical specs), does NOT resolve entities
4. **Entity Resolver (EntityResolver):** Resolves categorical filters to concrete entity objects via FindOrCreate pattern (query existing, generate if needed)
5. **Scene Instantiator (SceneParser):** Creates Scene with direct object references from pre-resolved entities
**Key Principle:** Categorical filters throughout, no concrete IDs until System 4 resolution, entities resolved eagerly before scene construction.

### Entity Resolver (System 4)
**Definition:** Service that resolves categorical PlacementFilterDTO specifications to concrete entity objects using FindOrCreate pattern.
**Location:** EntityResolver service, called by PackageLoader.
**Pattern:** Query existing entities first (reuse via categorical matching), generate new entities if no match (eager creation), return concrete entity objects (NOT IDs).
**Methods:** FindOrCreateLocation(filter), FindOrCreateNPC(filter), FindOrCreateRoute(filter).
**Output:** Pre-resolved entity objects passed to System 5 (SceneParser) for scene construction.

### PlacementFilterDTO
**Definition:** Data structure containing categorical requirements for entity placement (LocationProperties, NpcPersonalityTypes, LocationTags, SelectionStrategy).
**Usage:** Written to JSON by System 3 (SceneInstantiator), read by System 4 (EntityResolver) for FindOrCreate resolution.
**Properties:** LocationProperties (Indoor, Private, Safe), LocationTags (lodging, secure), NpcPersonalityTypes (Innkeeper, Merchant), SelectionStrategy (Closest, LeastRecentlyUsed).
**Key Principle:** NO concrete entity IDs. All requirements expressed categorically. Enables procedural content generation without hardcoded references.

---

## OVERLOADED TERMS (DISAMBIGUATE)

### Challenge
**Three Meanings:**
1. **Tactical Challenge System:** Mental/Physical/Social subsystems. Separate gameplay mode with deck, cards, resources.
2. **Challenge PathType:** ChoicePathType.Challenge (choice leads to tactical challenge).
3. **Challenge Situation:** Narrative situation involving difficulty/risk (informal usage).
**Disambiguation:** Always specify "Tactical Challenge" (subsystem), "Challenge Choice" (PathType), or "Challenge Situation" (narrative).

### Goal
**Historical Term:** Earlier architecture (pre-2025) used Goal/Obstacle entities for progression. Replaced by Scene/Situation architecture.
**Current Equivalent:** "Goal" → "Scene". If documentation references "Goals", translate to "Scenes".
**Why Replaced:** Scene architecture more flexible, supports Sir Brante multi-situation patterns, enables procedural content generation.

### Spot
**Historical Term:** Earlier docs used "Spot" for sub-areas within locations.
**Current Standard:** "Location". Codebase has no Spot class.
**Spatial Hierarchy:** Venue → Location (current). NOT Venue → Location → Spot.

### Archetype
**Two Meanings:**
1. **Scene Archetype (SpawnPattern enum):** Linear, HubAndSpoke, Branching, Converging, etc. Defines situation flow within Scene.
2. **Situation Archetype (SituationArchetypeCatalog):** Confrontation, Negotiation, Investigation, etc. 21 patterns for generating choices.
**Disambiguation:** Always specify "Scene Archetype" (spawn pattern) or "Situation Archetype" (choice pattern).

---

## RESOURCE TYPES

### Strategic Resources
**Definition:** Persistent player resources tracked across entire game. Constrain options, require management.
**Examples:** Coins, Health, Stamina, Focus, Hunger.
**Tracking:** Player object properties.
**Usage:** Choice costs, situation requirements, scene spawning conditions.

### Tactical Resources
**Definition:** Temporary resources used within tactical challenge sessions. Reset after session ends.
**Examples:**
- **Mental:** Progress, Attention, Exposure
- **Physical:** Breakthrough, Danger, Momentum
- **Social:** Doubt, Rapport, Trust
**Tracking:** Challenge session objects (MentalSession, PhysicalSession, SocialSession).
**Usage:** Card costs, victory thresholds, failure conditions.

### Localized Resources
**Definition:** Per-entity progression tracking. Represents mastery/familiarity with specific entity.
**Examples:**
- **InvestigationCubes:** Per-Location (0-10 scale). Reduces Mental Exposure.
- **StoryCubes:** Per-NPC (0-10 scale). Reduces Social Doubt.
- **ExplorationCubes:** Per-Route (0-10 scale). Reduces route danger.
- **MasteryCubes:** Per-Physical-Deck (0-10 scale). Reduces Physical Danger.
**Why:** Progression feels earned and specific rather than generic. Encourages revisiting same entities to build mastery.

---

## STATE MACHINE TERMINOLOGY

### Provisional
**Scene State:** Scene created for perfect information preview. Situations not instantiated. Player sees metadata (SituationCount, EstimatedDifficulty) without full content.
**Transition:** Provisional → Active (when player selects provisional scene choice) OR Provisional → Deleted (when different choice selected).
**Why:** Enables showing player "this choice spawns a 3-situation scene with Medium difficulty" before commitment.

### Active
**Scene State:** Scene finalized and playable. Situations instantiated, CurrentSituation set.
**Availability:** Player can engage with CurrentSituation at appropriate context (location/NPC).
**Transition:** Active → Completed (all situations finished) OR Active → Expired (ExpiresOnDay reached).

### Completed
**Scene State:** All situations finished. CurrentSituation = null.
**Cleanup:** Scene marked for removal from queries. Dependent resources cleaned up (if configured).

### Expired
**Scene State:** ExpiresOnDay reached before completion.
**Cleanup:** Scene filtered from queries. Represents missed opportunity.

### Deferred
**Situation InstantiationState:** Situation exists but Actions not created. Template stored, waiting for query-time instantiation.
**Transition:** Deferred → Instantiated (when player enters context and SceneFacade queries).

### Instantiated
**Situation InstantiationState:** Actions created in GameWorld collections (LocationActions/NPCActions/PathCards). Player can execute choices.

### Selectable
**Situation LifecycleStatus:** Situation available for player to select.
**UI:** Rendered as clickable choice.

### Completed / Failed
**Situation LifecycleStatus:** Situation finished (success or failure).
**Cleanup:** Situation removed from available options. Scene checks for next situation or marks complete.

---

## A-STORY TERMINOLOGY

### A-Story (Main Story)
**Definition:** Primary narrative progression. Sequential tutorial scenes A1-A10 (authored), then procedural A11+ (infinite).
**Current Status:** A1-A3 implemented. A4-A10 designed. A11+ procedural generation designed but not implemented.
**Sequence:** A1 → A2 → A3 → ... → A10 (tutorial) → A11+ (procedural continuation).

### Tutorial Scenes
**Definition:** A1-A10 authored scenes introducing core mechanics.
**Duration:** Currently ~30-60 minutes (A1-A3). Target 2-4 hours (A1-A10).
**Properties:** StoryCategory = MainStory, MainStorySequence = 1-10.

### Procedural A-Story
**Definition:** A11+ scenes generated by AI from templates with validation.
**Status:** DESIGNED. Not implemented.
**Architecture:** Template library → AI generates scenes → Validation checks → Spawn if valid → Emergency fallback if generation fails.

---

## GAME DESIGN PRINCIPLES

### Soft-Lock
**Definition:** A game state where forward progress becomes impossible due to design flaws, forcing the player to restart.
**Examples:** Required item consumed in wrong context, NPC killed before completing their quest, progression gate unreachable.
**Wayfarer Policy:** Soft-locks are FORBIDDEN. Every game state must have forward progress path. Design principle: "No Soft-Locks" is Tier 1 (Non-Negotiable). See DESIGN_PHILOSOPHY.md.
**Prevention Mechanisms:** Required items cannot be consumed, NPCs cannot die permanently, all scenes have fallback choices, resource deadlocks prevented by design.

### Verisimilitude
**Definition:** The quality of seeming real or truthful in a fictional context. In game design, verisimilitude means mechanical systems align with fiction.
**Examples:**
- **Good Verisimilitude:** Investigations spawn Obstacles (natural flow: investigation uncovers problems).
- **Poor Verisimilitude:** Locations own Goals (backwards: locations don't generate objectives).
**Wayfarer Principle:** "Verisimilitude in Entity Relationships" (Design Principle #8). Entity relationships must match conceptual model. Fiction supports mechanics, mechanics express fiction.
**Test:** Does the relationship explanation feel backwards or confusing? If yes, verisimilitude violated.

### Playability
**Definition:** The quality of being actually playable, not just compilable. A game that compiles but has inaccessible content or broken progression is unplayable.
**Wayfarer Principle:** "Playability Over Compilation" - Game that compiles but is unplayable is WORSE than crash. Before marking task complete, verify: (1) Can player REACH this from game start? (2) Are ALL actions VISIBLE and EXECUTABLE? (3) Forward progress from every state?
**Enforcement:** Test in browser, verify EVERY link works, trace exact path from game start. Inaccessible content is worthless.

### Perfect Information
**Definition:** Game state where all costs, requirements, and rewards are visible to player BEFORE making decision.
**Strategic Layer:** Perfect information required - player sees all available scenes, situations, choices, costs, requirements, rewards before selecting.
**Tactical Layer:** Hidden complexity allowed - specific cards, draw order, challenge flow not visible before entry.
**Why:** Enables strategic decision-making. Player can calculate whether to attempt challenge before entering. No "gotcha" moments where cost revealed after commitment.

---

## IMPLEMENTATION STATUS MARKERS

Use these markers when documenting features:

- **IMPLEMENTED:** Fully working, tested, in production.
- **IN PROGRESS:** Partially implemented, actively being developed.
- **DESIGNED:** Architecture documented, not yet coded.
- **PLANNED:** Intended future feature, no detailed design yet.
- **DEPRECATED:** No longer used, kept for backward compatibility or historical reference.
- **REMOVED:** Previously existed, deleted from codebase.

---

## RELATED DOCUMENTATION

- **ARCHITECTURE.md:** Overall system architecture, entity ownership, data flow.
- **SCENE_INSTANTIATION_ARCHITECTURE.md:** Detailed scene lifecycle, marker resolution, dependent resources.
- **INFINITE_A_STORY_ARCHITECTURE.md:** A-story sequencing, procedural generation design.
- **DESIGN_PHILOSOPHY.md:** 12 core design principles.
- **CLAUDE.md:** Claude AI agent instructions, coding standards, refactoring guidelines.

---

## GLOSSARY MAINTENANCE

**When to Update:**
- New entity type added to codebase.
- Terminology standardization (replacing old term with new).
- Architectural refactoring changes entity relationships.
- Implementation status changes (Designed → Implemented).

**Update Responsibility:**
- Developer adding new entity: Add glossary entry.
- Developer refactoring architecture: Update affected entries, add historical notes.
- Documentation reviewer: Check consistency between glossary and other docs.

**Conflict Resolution:**
- Glossary is authoritative for terminology.
- Codebase is authoritative for implementation status.
- SCENE_INSTANTIATION_ARCHITECTURE.md and INFINITE_A_STORY_ARCHITECTURE.md are authoritative for scene system design.
