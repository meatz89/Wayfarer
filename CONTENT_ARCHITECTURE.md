# Wayfarer Content Architecture

This document describes the principles governing pre-authored templates, archetypes, dynamic entity creation, scene spawning, choices, and placement strategies.

---

## 1. Core Principle: Scenes Are Scenes

A-story, B-story, C-story are CATEGORIES of scenes, not different data structures. All scenes use identical SceneTemplate structure. Category determines RULES and VALIDATION, not structure.

**SceneTemplate contains:**
- Category (MainStory, SideStory, Encounter)
- Main story sequence number (A-Story only)
- Scene archetype reference (enum)
- Location activation filter
- Embedded situation templates (ordered list)

**Category Effects (Parse-Time):**

| Category | Validation | Enrichment |
|----------|------------|------------|
| MainStory (A) | Fallback required in every situation | All final choices spawn next A-scene |
| SideStory (B) | None | None |
| Encounter (C) | None | None |

**HIGHLANDER Compliance:** One SceneArchetypeCatalog contains ALL reusable patterns. No separate A-story catalog.

---

## 2. Two Orthogonal Concepts

### Authoring Methods (How Templates Are Created)

| Method | Description |
|--------|-------------|
| **Fully Authored** | Complete template with embedded situations and choices |
| **Archetype-Generated** | Template specifies archetype, catalog generates situations |

Both produce identical SceneTemplate structure. Runtime cannot distinguish.

### Story Categories (Property Combination)

Story categories are distinguished by a **combination of properties**, not a single axis:

| Property | A-Story | B-Story | C-Story |
|----------|---------|---------|---------|
| **Scene Count** | Infinite chain | Multi-scene arc (3-8) | Single scene |
| **Repeatability** | One-time (sequential) | One-time (completable) | Repeatable |
| **Fallback Required** | Yes (every situation) | No | No |
| **Can Fail** | Never | Yes | Yes |
| **Resource Flow** | Sink (travel costs) | Source (significant) | Source (incremental) |
| **Typical Scope** | World expansion | Venue depth | Location/route |
| **Player Initiation** | Automatic | Voluntary | Organic |

**A-Story is special.** Frieren principle: infinite, never-ending. Primary purpose is world expansion—creating new places to explore, new people to meet, new paths to travel. Player must ALWAYS be able to progress.

**B-Story provides substantial engagement.** Multi-scene arcs the player voluntarily initiates. Significant resource rewards fund A-story progression.

**C-Story provides texture.** Single-scene encounters during routine play. Repeatable. Incremental resource rewards ensure player is never stuck.

---

## 2.1 A-Story Guarantees

A-Story scenes have strict requirements to prevent soft-locks:

| Guarantee | Enforcement |
|-----------|-------------|
| **Fallback exists** | Every situation has at least one choice with NO requirements |
| **Progression assured** | ALL final situation choices spawn next A-scene |
| **World expansion** | Scenes create new venues/districts/regions/routes/NPCs |

These guarantees are enforced at parse-time. A-Story scenes that violate them fail validation.

---

## 2.2 B/C Story Flexibility

B and C stories have relaxed rules:

- Can require stats, resources, or completed prerequisites
- Can include challenge paths without fallbacks
- Can fail (scene marked Failed, not Completed)
- Focus on narrative depth, not world expansion

---

## 2.3 Parse-Time Flow

**Authoring method resolution:**
1. If archetype specified → catalog generates situation templates
2. If fully authored → situations used directly
3. Result: identical SceneTemplate structure

**Category-specific processing:**
- MainStory (A): Validate fallbacks exist, enrich final choices with next-scene spawn, add to ordered A-Story list
- SideStory (B): Standard spawn conditions only
- Encounter (C): Standard spawn conditions only

---

## 3. SceneArchetypeCatalog (Unified)

### Principles

- Single catalog contains ALL reusable scene patterns
- Strongly-typed enum IDs (compile-time validated)
- Same archetype works for any scene category (A/B/C)
- Archetypes generate situation templates with embedded choice templates

### HIGHLANDER Compliance

ONE catalog. No separate A-story catalog, B-story catalog, etc. Category determines validation and enrichment rules applied to the generated content, not the source of patterns.

---

## 4. A-Story Scene Definitions

A-scenes are instance definitions loaded DEFERRED at game initialization. They reference archetypes and specify activation filters.

**Required properties:**
- Scene archetype ID (enum, compile-time validated)
- Category (MainStory)
- Main story sequence number (determines list position)
- Location activation filter (when scene activates)
- Spawn conditions (when scene enters Deferred state)

**Key rules:**
- Archetype ID is enum, not string. Compile-time validated.
- Location activation filter uses singular identity dimensions with exact match.
- Scenes activate on location enter only. No NPC-based activation.
- Spawn conditions determine WHEN scene becomes eligible.

---

## 5. Archetype-Generated Situation Templates

Archetypes generate embedded situation templates with these properties:

**Per situation:**
- Situation archetype reference
- Location filter (where situation takes place)
- NPC filter (who is involved, null = solo)
- Route filter (for travel situations, null = non-travel)

**Filter rules:**
- Proximity is MANDATORY on all filters. No default.
- At least one identity dimension (privacy/safety/activity/purpose) required.
- Null filter = no entity needed for that type.

**Progression:** Situations embedded in list order. Scene advances through indices sequentially until complete. No explicit transition rules needed.

---

## 6. PlacementFilter Structure

### Filter Properties

All properties singular. Exact match semantics. Null = don't filter on that dimension.

**Required:**
- Proximity (how to search relative to context)

**Identity dimensions (at least one required):**
- Privacy
- Safety  
- Activity
- Purpose
- Profession (for NPC filters)

**Venue placement (for LocationFilters):**
- VenueTypes (list of acceptable venue types)

### Two Filter Concepts for Locations

| Concept | Properties | Used For |
|---------|-----------|----------|
| **Identity dimensions** | Purpose, Privacy, Safety, Activity | Situations finding matching locations |
| **VenueFilter** | VenueTypes list | Location finding its containing venue |

### PlacementProximity Values

| Value | Meaning |
|-------|---------|
| SameLocation | Use context location directly |
| SameVenue | Search within venue's locations |
| AdjacentLocation | Search hex neighbors |
| SameDistrict | Search district's venues |
| SameRegion | Search region's districts |
| Anywhere | Unrestricted search |
| RouteDestination | Use route's destination location (from prior RouteFilter in same scene) |

---

## 7. Template vs Instance Lifecycle

### Parse Time: Templates Only

- SceneTemplates created with ALL SituationTemplates embedded
- Templates are immutable blueprints
- A-Story validation and enrichment applied to templates
- Templates added to GameWorld
- No instances exist yet

### Scene Instantiation (Deferred State)

- Scene Instance created from SceneTemplate
- Scene Instance references its template
- Scene.Situations list is EMPTY
- NO Situation Instances exist
- Scene waiting for activation trigger

**Key insight:** Deferred scenes don't need Situation Instances because player cannot interact with them. Only the Scene Instance exists, holding a reference to its template.

### Scene Activation (Active State)

- Situation Instances created from SceneTemplate.SituationTemplates
- Entity resolution (find-or-create) for each Situation Instance
- Situation Instances added to Scene.Situations
- Entities populated on Situation Instances
- Scene ready for play

**Key insight:** Situation Instances only exist after activation. Creating them earlier wastes memory and complicates entity resolution timing.

### Query Time

- ChoiceTemplates instantiated as LocationActions
- Ephemeral: created on demand, deleted after execution

---

## 8. A-Story Enrichment (Parse-Time)

For MainStory category, parser applies special rules:

### Validation

- Every situation must have at least one choice with NO requirements (fallback)
- Validation failure = parse error (content rejected)

### Enrichment

- ALL final situation choices receive spawn reward for next A-scene
- Next scene determined by list index (not string matching)
- GameWorld maintains ordered A-Story list

### World Expansion Purpose

A-Story scenes are designed to CREATE new world content. B/C stories have full freedom but tend toward different patterns per the property matrix in §2.

All categories CAN create any entity type. The distinction is tendency, not restriction. A-Story is the engine of world growth; B/C add depth to existing world.

---

## 9. A-Story Infinite Progression

### Frieren Principle

The journey never ends. A-Story is an infinite spine of world expansion. Player travels, discovers new places, meets new people, moves on. No final destination.

### Authored Tutorial

Initial A-story scenes are hand-authored to teach mechanics while establishing the world. Number is flexible based on tutorial needs.

### Procedural Continuation

When authored A-Story scenes exhausted, ProceduralAStoryService generates next scene:

1. Select archetype via category rotation
2. Generate template with world expansion targets
3. Process through PackageLoader (same pipeline as authored)
4. Append to ordered A-Story list

### Category Rotation

Procedural generation rotates through archetype categories to ensure variety:
- Investigation archetypes (discovery, information gathering)
- Social archetypes (relationships, connections)
- Confrontation archetypes (conflict, opposition)
- Crisis archetypes (urgent decisions, moral choices)

Specific archetypes within each category are implementation details subject to change.

---

## 10. Scene Progression

SituationTemplates define order. Situation Instances hold runtime state.

**Flow:**
1. Scene activates → Situation Instances created from templates
2. Scene starts at first Situation Instance
3. Player completes situation (selects choice)
4. Scene index advances to next Situation Instance
5. Player returns to location view
6. Player navigates to next situation's location
7. Scene resumes when player enters matching location
8. Repeat until final situation complete
9. Scene marked Completed, spawn rewards applied

**Multi-location scenes:** Player must travel between situation locations. Scene waits for player to arrive at correct location before resuming.

---

## 11. Situation Presentation

How situations appear depends on which entities they reference. Three distinct patterns exist.

### Location-Only Situations (Modal)

When situation has Location but NO NPC:
1. Player enters location matching situation's Location
2. IMMEDIATE: Modal appears with situation choices
3. Player MUST select a choice (no escape)
4. After selection: costs/rewards applied, return to location view

**Feel:** Sir Brante style. Enter a place, confronted with decision. Cannot leave until resolved.

### Location + NPC Situations (Conversation Entry)

When situation has BOTH Location AND NPC:
1. Player enters location matching situation's Location
2. Player uses "Look Around" action
3. NPCs at location displayed
4. For each NPC with active situation: conversation option appears
5. Player clicks conversation option (voluntary entry)
6. Modal appears with situation choices (mandatory decision)
7. After selection: costs/rewards applied, return to location view

**Feel:** You spot someone. You can approach them or not. Once you engage, you must see it through.

### Route Situations (Travel Segments)

When situation has Route:
1. Player initiates travel on route
2. Route divided into segments (each segment = one situation)
3. At each segment: modal appears with situation choices
4. Player MUST select a choice (no turning back mid-journey)
5. After selection: costs/rewards applied, continue to next segment
6. After final segment: arrive at destination

**Feel:** Journey encounters. The road presents challenges. You handle each as it comes.

### Entity Filter Determines Presentation

| LocationFilter | NpcFilter | RouteFilter | Presentation |
|----------------|-----------|-------------|--------------|
| Set | null | null | Location modal (immediate) |
| Set | Set | null | NPC conversation (Look Around → click → modal) |
| Set | null | Set | Route segment (during travel) |

LocationFilter is always required. NpcFilter and RouteFilter determine presentation mode.

### Mandatory Decision Principle

Once a situation is entered (by any path), player MUST make a choice. No backing out. This creates weight—approaching an NPC or entering a location has consequences.

The Four-Choice Archetype guarantees at least one choice is always available (fallback), so player is never stuck.

---

## 12. Entity Resolution (Find-Or-Create)

### When Resolution Occurs

Entity resolution happens at SCENE ACTIVATION, simultaneously with Situation Instance creation.

| Phase | Scene.Situations | Entity References |
|-------|------------------|-------------------|
| Parse Time | N/A (no Scene Instance) | N/A |
| Scene Instantiation (Deferred) | Empty list | N/A |
| Scene Activation | Created from SituationTemplates | Resolved (find-or-create) |

### Activation Process

1. For each SituationTemplate in SceneTemplate:
   - Create Situation Instance
   - Resolve Location via filter (find-or-create)
   - Resolve NPC via filter if present (find-or-create)
   - Resolve Route via filter if present (find-or-create)
   - Add Situation Instance to Scene.Situations

### Location Placement Principle

Locations have two filter concepts:
- **Identity dimensions** (Purpose, Privacy, Safety, Activity) - what the location IS, used by situations finding locations
- **VenueFilter** (VenueTypes list) - what venue types can contain this location

| Location Origin | How Venue Determined |
|-----------------|---------------------|
| **Authored** | Match venue.VenueType against location.VenueFilter (venues parsed first) |
| **Scene-created** | Proximity from context + VenueFilter match |

### Hex Position Principle

Location.HexPosition is the sole source of truth for spatial placement. One location per hex.

| Concept | Implementation |
|---------|---------------|
| **Source of truth** | Location.HexPosition |
| **Constraint** | One location per hex |
| **Scene-created** | Assigned to available hex within venue (not occupied by existing location) |

Hex position assigned procedurally to unoccupied hex within matched venue.

### Situation Entities: Three Types

Each Situation Instance can reference up to three dependent entities (one per type):

| Filter | Entity | Constraint |
|--------|--------|------------|
| LocationFilter | Location | One per situation, mandatory |
| NpcFilter | NPC | One per situation, null = solo |
| RouteFilter | Route | One per situation, null = non-travel |

### Separated Responsibilities (HIGHLANDER)

| Class | Responsibility |
|-------|----------------|
| **EntityResolver** | FIND only (pure query) |
| **PackageLoader** | CREATE only (single path for all creation) |
| **SceneInstantiator** | Create Situation Instances, orchestrate find-or-create |

No circular dependencies. Each class has single purpose.

### Parse-Time vs Activation Behavior

| Context | Caller | If Not Found |
|---------|--------|--------------|
| **Parse-time** | Parsers | Fail fast (content error) |
| **Activation** | SceneInstantiator | Create via PackageLoader |

**Parse-time principle:** Referenced entities must already exist. Missing entity = malformed content.

**Activation principle:** Scene may require entities that don't exist yet. Missing entity = create dynamically via PackageLoader.

### PackageLoader Creation

PackageLoader is the SINGLE path for entity creation. It handles:
- Parsing DTO to domain entity
- Venue/hex assignment
- Origin tracking (SceneCreated for runtime entities)
- GameWorld registration

---

## 13. Text Generation Rules

### NO PLACEHOLDERS

Placeholder syntax like `{NPCName}`, `{LocationName}` is FORBIDDEN in templates.

| Approach | When to Use |
|----------|-------------|
| **Generic text** | Works standalone without substitution |
| **Null** | AI generates contextually appropriate text |

**Rationale:** AI text generation pass will replace all narrative content. Templates should contain either generic text that works standalone, or null for full AI generation.

---

## 14. Choice Patterns by Category

### A-Story: Fallback Required

Every A-Story situation MUST have at least one choice with NO requirements.

| Choice | Requirement | Cost | Outcome |
|--------|-------------|------|---------|
| Stat-gated | PrimaryStat ≥ threshold | None | Best |
| Money-gated | None | Coins | Good |
| Challenge | None | Resolve | Variable |
| **Fallback** | **None** | Time | Worst |

The fallback guarantees forward progress. A-Story validation rejects situations without one.

### B/C Stories: Flexible

B and C stories can have any choice structure:

- All choices can have requirements
- Challenge-only situations allowed
- Failure states permitted
- No mandatory fallback

This enables narrative tension, gating, and consequences that A-Story cannot have.

---

## 15. Dual-Tier Action Architecture

LocationAction discriminated by presence of ChoiceTemplate.

### Tier 1: Atmospheric (Permanent)

| Aspect | Description |
|--------|-------------|
| Source | LocationActionCatalog at parse-time |
| Storage | GameWorld.LocationActions |
| Properties | Direct Costs, Rewards |
| Purpose | Prevent soft-locks |

### Tier 2: Scene-Based (Ephemeral)

| Aspect | Description |
|--------|-------------|
| Source | SceneFacade at query-time |
| Storage | Not stored |
| Properties | Via ChoiceTemplate |
| Purpose | Narrative choices |

**Screen-Based Display:** Actions are mutually exclusive per screen context:
- **Location with active situation** → Scene-based actions only
- **Location without situation** → Atmospheric actions only
- **NPC with active situation** → Action to enter situation
- **NPC without situation** → Cannot interact

---

## 16. Location Accessibility

| Origin | Accessibility |
|--------|---------------|
| **Authored** | Always accessible (TIER 1 guarantee) |
| **SceneCreated** | Accessible only when active scene's current situation is at that location |

Scene-created locations become inaccessible when scene completes or advances past their situation.

---

## 17. Complete Flow

### Parse Time (Startup)

1. PackageLoader processes JSON packages
2. SceneTemplates created with SituationTemplates embedded
3. Archetype references resolved via catalog
4. A-Story templates validated for fallbacks
5. A-Story final choices enriched with next-scene spawn
6. Templates added to GameWorld

### Scene Instantiation (Spawn Condition Met)

1. Scene Instance created from SceneTemplate
2. Scene Instance references its template
3. Scene.Situations = EMPTY LIST
4. Scene state = Deferred
5. NO Situation Instances created

### Scene Activation (Player Enters Location)

1. Player enters location matching scene's activation filter
2. Scene state transitions Deferred → Active
3. For each SituationTemplate in scene.Template:
   - Create Situation Instance
   - Resolve entities (find-or-create)
   - Add to Scene.Situations
4. Scene ready for play

### Runtime (Per Situation)

1. SceneFacade generates LocationActions from current situation's choices
2. Player selects choice, costs/rewards applied
3. Scene index advances to next Situation Instance
4. Player returns to location view
5. Player navigates to next situation's location
6. Scene resumes when player enters matching location

Presentation varies by situation type (see §11):
- Location-only: Modal appears immediately on location entry
- Location + NPC: Conversation option appears when player looks around
- Route: Segment appears during travel

### Completion

1. Final situation complete
2. Spawn rewards applied (next A-scene instantiated as Deferred)
3. Scene state set to Completed
4. Scene-created locations become inaccessible

---

## 18. Key Principles

### Template vs Instance Separation

| Concept | Template | Instance |
|---------|----------|----------|
| **SceneTemplate** | Immutable blueprint with SituationTemplates | Scene Instance created at instantiation (Deferred), Situations empty |
| **SituationTemplate** | Embedded in SceneTemplate | Situation Instance created at activation (Active) |
| **ChoiceTemplate** | Embedded in SituationTemplate | LocationAction created at query-time |

Templates are patterns. Instances hold runtime state. Deferred scenes have NO Situation Instances—only created at activation.

### HIGHLANDER

- ONE SceneArchetypeCatalog (no A-story catalog)
- ONE scene structure (category distinguishes rules)
- ONE path for content (JSON → Parser → Entity)
- ONE path for entity creation (DTO → PackageLoader)

### Separated Responsibilities

| Class | Responsibility |
|-------|----------------|
| **EntityResolver** | FIND only |
| **PackageLoader** | CREATE only |
| **SceneInstantiator** | Create Situation Instances, orchestrate find-or-create |

### Location Placement

Locations have identity dimensions (what it IS) and VenueFilter (what venue types contain it). Venue matched by `venue.VenueType IN location.VenueFilter`. Scene-created adds Proximity from context. 

Location.HexPosition is sole source of truth. One location per hex. Scene-created locations assigned to unoccupied hexes within venue.

### Explicit Over Implicit

- Proximity MANDATORY on all filters
- At least one identity dimension required
- Null filter = explicit "no entity needed"
- Singular dimension names (no AND/OR ambiguity)

### No Placeholders

- Variable substitution syntax FORBIDDEN
- Use generic text or null for AI generation

### Strongly-Typed Enums

- Scene and situation archetype IDs are enums
- Parsed at parse-time with fail-fast
- Compile-time validation

### Guaranteed Forward Progress

**A-Story specific:**
- Every situation has fallback choice (no requirements)
- ALL final choices spawn next A-scene
- Validation rejects non-compliant scenes

**Universal:**
- Atmospheric actions always exist
- Authored locations always accessible

### Story Category Separation

Story categories are distinguished by a combination of properties (see §2 for full matrix):

| Property | A-Story | B-Story | C-Story |
|----------|---------|---------|---------|
| Scene count | Infinite | Multi (3-8) | Single |
| Repeatability | One-time | One-time | Repeatable |
| Can fail? | Never | Yes | Yes |
| Fallback? | Required | No | No |
| Resource flow | Sink | Source (significant) | Source (incremental) |

Scope describes tendency, not restriction. All categories can create any entity type.

### A-Story List-Based Progression

- A-scenes stored in ordered list by sequence
- Next scene determined by list index, not string matching
- Procedural generation appends to list when exhausted
- No ID-based lookups for A-story progression
