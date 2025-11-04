# HANDOFF: Scene/Situation Spawning System MVP Implementation

## Problem & Motivation

### The Core Business Need

Wayfarer is a narrative RPG where scenes spawn dynamically based on player actions, world state, and entity conditions. The game needs a **procedural scene spawning system** that can:

1. **Conditionally spawn narrative content** - Scenes should only appear when specific conditions are met (player completed prior scene, weather is rain, NPC bond level reached, etc.)
2. **Show consequences before player commits** - Player needs to see "selecting this action will spawn scene X at location Y" BEFORE they choose
3. **Avoid infinite recursion** - Scenes contain situations with choices that can spawn more scenes - must prevent recursive instantiation
4. **Support AI content generation** - Templates use categorical properties (not hardcoded entity IDs) so AI can generate content procedurally

### The Pain Points

**Current System Issues:**
- Provisional scenes instantiate deeply (full Situation entities created immediately), violating the shallow provisional architecture spec
- Route placement filters commented out (engineer thought properties didn't exist, but they do via Hex system)
- No SpawnConditions system (scenes spawn unconditionally or not at all)
- State machine naming collision: `State.Dormant` vs `Status.Dormant`, `State.Active` vs `Status.Active` - same words, different meanings
- Scene owns SpawnRules and CurrentSituationId but has no methods to apply state transitions
- Separate Dictionary for provisional scenes when Scene.State already differentiates (unnecessary complexity)

### Why This Matters

Without conditional spawning, all scenes either spawn always or never - no dynamic narrative emergence. Without shallow provisionals, memory explodes (every unselected action creates full Situation tree in GameWorld). Without clear state machine semantics, developers waste hours debugging "is this situation active or active?"

The refactoring.md specification document describes a complete three-part spawning system (SpawnConditions + PlacementFilters + TriggerEvents) that enables Sir Brante-style consequence chains and environmental/conditional narrative moments.

---

## Architectural Discovery

### The Three-Tier Timing Model (CRITICAL PATTERN)

The codebase implements a sophisticated lazy evaluation architecture:

**Tier 1 (Parse Time):** SceneTemplates and SituationTemplates loaded from JSON into GameWorld collections. These are immutable archetypes.

**Tier 2 (Instantiation Time):** Scenes and Situations created as runtime instances from templates. Scenes stored in GameWorld.Scenes, Situations in GameWorld.Situations. Situations start in Dormant state - no actions exist yet.

**Tier 3 (Query Time):** When player enters context (location/NPC/route), SceneFacade transitions Situation from Dormant to Active, instantiating ChoiceTemplates into LocationActions/NPCActions/PathCards. Actions added to GameWorld collections.

This pattern prevents premature instantiation - actions only exist when player can see them. Template references stored on Situation for on-demand action generation.

### HIGHLANDER Principle (Single Source of Truth)

The codebase aggressively enforces ONE representation per concept:

**Pattern A (Persistence IDs + Runtime Navigation):** Scene stores `List<string> SituationIds` (references), GameWorld.Situations holds actual entities. Query time filters: `gameWorld.Situations.Where(s => scene.SituationIds.Contains(s.Id))`. ID from JSON (immutable), objects resolved at parse time, runtime uses objects only.

**Pattern B (Runtime-Only Navigation):** Situation has `Scene ParentScene` property (object reference ONLY, no ParentSceneId). Changes during gameplay, no JSON source, object everywhere consistently. Adding ID alongside object creates desync risk (forbidden).

**Pattern C (Lookup on Demand):** Rarely used - ID only, GameWorld lookup when needed, no cached object reference.

The critical rule: Never store same information in multiple forms that can desynchronize. If entity has object reference property, DO NOT add ID property for same concept. If entity has ID property for persistence, DO use object reference for runtime navigation, both populated once at parse time.

### Catalogue Pattern (Parse-Time Translation ONLY)

Catalogues translate categorical JSON properties to concrete mechanical values:

**When Used:** JSON has "Simple" / "Full" / "Fragile" (categorical descriptors), catalogue converts to concrete numbers scaled by game state (player level, difficulty, max stats). AI generates content with relative terms, catalogue applies absolute values.

**Critical Constraint:** Catalogues live in `src/Content/Catalogs/` and are ONLY called from Parsers. Runtime code (facades, services, UI) NEVER imports catalogues. After parsing, entities have concrete strongly-typed properties (int, bool, enum). No string matching at runtime, no dictionary lookups, no catalogue calls after initialization.

**Why:** Fail fast (bad JSON crashes at parse time), zero runtime overhead (calculations done once), type safety (compiler catches errors), AI content generation (AI describes intent, catalogue scales to progression).

### Template → Instance Pattern (Composition)

**SceneTemplate:** Immutable archetype with init-only properties. Contains embedded `List<SituationTemplate>` (composition, not references). Has PlacementFilter (categorical properties like PersonalityTypes, LocationProperties). Has SpawnRules defining situation flow (Linear, HubAndSpoke, Branching).

**Scene:** Mutable runtime instance with set properties. References template via TemplateId. Has concrete placement (PlacementType enum + PlacementId string). Stores `List<string> SituationIds` (references to GameWorld.Situations). Has State enum (Provisional/Active/Completed/Expired) controlling lifecycle.

**SituationTemplate:** Embedded in SceneTemplate. Contains `List<ChoiceTemplate>` (2-4 choices, Sir Brante pattern). Has ArchetypeId for procedural choice generation or hand-authored choices. Contains narrative hints for AI generation.

**Situation:** Mutable runtime instance. Stores Template reference for lazy action instantiation. Has State (Dormant/Active) controlling when ChoiceTemplates become actions. Has Status (Available/Completed/Failed) tracking lifecycle progress. Contains ProjectedBondChanges/ScaleShifts/States for perfect information display. Has SuccessSpawns/FailureSpawns for cascading chains.

Templates authored once, instances spawned many times. Template never instantiates another template (no recursion). Instance references template for lazy evaluation.

### Perfect Information Pattern (Provisional Scenes)

Player must see "this choice spawns scene X at location Y" BEFORE selecting. Implementation creates provisional scenes eagerly when action displayed, finalizes when action selected, deletes when action not selected.

**Current (Wrong) Implementation:** CreateProvisionalScene() instantiates full Situations into GameWorld.Situations immediately. Every unselected action leaves Situation garbage requiring cleanup. Memory overhead ~10x necessary. Architecture allows recursion (prevented only by state machine timing).

**Required (Correct) Implementation:** Provisional scenes are SHALLOW metadata only. No Situations instantiated until FinalizeScene(). Properties: TemplateId, PlacementType, PlacementId, DisplayName (with placeholders), SituationCount (int), EstimatedDifficulty (derived). Situations created first time when scene finalized (action selected). Cleanup trivial (delete Scene from collection, no GameWorld.Situations entries to remove).

### Data Flow Architecture (JSON → Runtime)

**Parse Time:** JSON files (20_scene_templates.json) → SceneTemplateDTO → SceneTemplateParser → SceneTemplate entity → GameWorld.SceneTemplates. Parser embeds SituationTemplates recursively. Parser calls catalogues for archetype generation if ArchetypeId present. Parser validates Sir Brante pattern (2-4 choices unless AutoAdvance).

**Spawn Time:** SceneInstantiator.CreateProvisionalScene() when action displayed OR GameWorldInitializer.SpawnInitialScenes() for IsStarter=true templates. Placement resolved via PlacementRelation enum (SameNPC, SameLocation, SpecificLocation, Generic). Placeholder replacement deferred until finalization.

**Finalization:** Player selects action → RewardApplicationService.FinalizeSceneSpawns() → SceneInstantiator.FinalizeScene() → Placeholder replacement → State transition Provisional→Active → Move to permanent storage.

**Query Time:** Player enters location → SceneFacade.GetActionsAtLocation() → Find scenes at placement → Get current situation → IF State==Dormant: ActivateSituation() → Instantiate ChoiceTemplates into LocationActions → CreateProvisionalScenesForAction() for choices with spawn rewards → Return actions.

### Placement Architecture (Categorical vs Concrete)

**PlacementFilter (Template - Categorical):** PersonalityTypes (enum list), LocationProperties (enum list), TerrainTypes (string list from Hex), MinBond/MaxBond (thresholds), RequiredStates/ForbiddenStates (player state). NO concrete IDs (locationId, npcId) except for specific starter/tutorial content. AI-friendly - describes WHAT kind of entity, not WHICH entity.

**Scene Placement (Instance - Concrete):** PlacementType enum (Location/NPC/Route), PlacementId (concrete entity ID). SceneInstantiator resolves categorical filter → concrete entity at spawn time via EvaluatePlacementFilter(). Checks NPC personality/bond, location properties, route terrain via Hex system.

**Route-Hex Integration:** Route entity has HexPath (List<AxialCoordinates>). Iterate path, query GameWorld.WorldHexGrid.GetHex(coords), access Hex.Terrain and Hex.DangerLevel properties. Count terrain types to find dominant. Route.DangerRating pre-calculated sum (no need to recalculate). Pattern already implemented in HexRouteGenerator.AnalyzeSegmentHexes() - reuse this logic.

### State Machine Architecture (Two Orthogonal Dimensions)

Investigation discovered Situation has TWO properties tracking lifecycle:

**State (SituationState enum: Dormant/Active):** Controls whether ChoiceTemplate actions have been materialized into GameWorld collections. Dormant = situation exists as data, NO actions in GameWorld.LocationActions/NPCActions/PathCards. Active = SceneFacade detected player entered context, instantiated actions. Transition triggered by SceneFacade at query time.

**Status (SituationStatus enum: Dormant/Available/Active/Completed/Failed):** Tracks situation progression from spawn through resolution. Dormant = requirements not met, invisible. Available = requirements met, selectable. Active = player engaged, challenge executing. Completed = resolved successfully. Failed = attempt failed.

**The Problem:** NAMING COLLISION. Both have "Dormant" and "Active" values meaning completely different things. State.Dormant = actions not instantiated. Status.Dormant = requirements not met. State.Active = actions instantiated. Status.Active = player engaged. Same words, different concepts, massive confusion.

**The Solution (User Approved):** These are genuinely orthogonal dimensions (proved by edge case analysis: Active+Completed is valid state when actions still exist but situation resolved). Rename for clarity: State→InstantiationState (Deferred/Instantiated), Status→LifecycleStatus (Locked/Selectable/InProgress/Completed/Failed). Zero semantic changes, pure naming clarity.

### Scene State Machine (Incomplete)

Scene entity has SpawnRules (pattern, transitions, completion condition) and CurrentSituationId property. Scene tracks progression through situations. BUT Scene has NO METHODS to apply transitions. Lifecycle logic scattered across facades (SceneFacade, SituationCompletionHandler).

**Architectural Violation:** Domain entity owns state machine data but not state machine behavior. State transitions should be Scene methods, not facade responsibilities. Expected pattern: Scene.AdvanceToNextSituation(completedSituationId) queries SpawnRules.Transitions, updates CurrentSituationId, marks scene complete if no more transitions.

**Why This Matters:** Domain entities should own their own lifecycle. Facades orchestrate, entities execute. Having lifecycle logic in facades violates single responsibility and spreads scene progression across multiple files.

---

## Domain Model Understanding

### Core Abstractions

**SceneTemplate (Parse-Time Archetype):** Immutable definition of scene structure. Properties: Id, Archetype (Linear/HubAndSpoke/Branching/AutoAdvance/etc), DisplayNameTemplate (with {placeholders}), IntroNarrativeTemplate, Tier (0-4 complexity), IsStarter (spawn at game init), ExpirationDays (optional time limit), PresentationMode (Modal=takeover vs Atmospheric=menu), ProgressionMode (Breathe=return to menu vs Cascade=continue), PlacementFilter (categorical entity selection), SpawnRules (situation flow), SituationTemplates (embedded list).

**Scene (Runtime Instance):** Mutable gameplay entity. Properties: Id, TemplateId (reference), Template (object reference), PlacementType/PlacementId (concrete placement), PresentationMode/ProgressionMode (copied from template), SituationIds (references to GameWorld.Situations flat list), SpawnRules (copied), CurrentSituationId (progression tracking), State (Provisional/Active/Completed/Expired), SourceSituationId (cleanup tracking for provisionals), IntroNarrative (replaced placeholders), Archetype, DisplayName (replaced placeholders).

**SituationTemplate (Embedded Archetype):** Embedded in SceneTemplate. Properties: Id (unique within scene), Type (Normal/Crisis), NarrativeTemplate (with {placeholders}), Priority (display order), NarrativeHints (tone/theme/context/style for AI), ChoiceTemplates (2-4 choices unless AutoAdvance), AutoProgressRewards (for AutoAdvance archetype), ArchetypeId (optional procedural generation).

**Situation (Runtime Instance):** Properties: Id, Name, Description, State (Dormant/Active - action instantiation), IsAutoAdvance, Template (reference for lazy instantiation), SystemType (Social/Mental/Physical), Type (Normal/Crisis), DeckId, TemplateId, ParentSituationId (cascade tracking), Lifecycle (SpawnTracking record), InteractionType (Instant/Challenge/Navigation), NavigationPayload, CompoundRequirement (OR-path gating), ProjectedBondChanges/ScaleShifts/States (perfect information), SuccessSpawns/FailureSpawns (cascade rules), Tier, Repeatable, GeneratedNarrative, NarrativeHints, Obligation (reference), ParentScene (reference), Status (Available/Completed/Failed - lifecycle), DeleteOnSuccess, Costs, DifficultyModifiers, SituationCards (victory conditions), ConsequenceType, SetsResolutionMethod/RelationshipOutcome, TransformDescription.

**ChoiceTemplate (Embedded Archetype):** Embedded in SituationTemplate. Properties: Id, ActionTextTemplate, ActionType (Instant/StartChallenge/Navigate), ChallengeId/ChallengeType (if StartChallenge), RequirementFormula (unlock conditions), CostTemplate (resources paid), RewardTemplate (consequences including ScenesToSpawn list), NavigationPayload (if Navigate).

**SpawnRule:** Defines cascading situation spawning. Properties: TemplateSituationId (which template to spawn from), PlacementStrategy (inherit from parent or specify), RequirementOffsets (make harder/easier), MinResolve/RequiredState/RequiredAchievement (spawn conditions).

**SceneSpawnReward:** Embedded in RewardTemplate.ScenesToSpawn. Properties: SceneTemplateId (which template), PlacementRelation (SameNPC/SameLocation/SpecificNPC/SpecificLocation/Generic), SpecificPlacementId (if Specific*), SpawnTiming (Immediate/Delayed - Delayed not implemented).

### Entity Relationships

**Scene → Situation:** One-to-many composition. Scene stores `List<string> SituationIds`, GameWorld.Situations holds actual entities (HIGHLANDER Pattern A). Situation has `Scene ParentScene` reference (Pattern B - runtime navigation). Scene owns placement (PlacementType/Id), Situation queries via GetPlacementId() method.

**SceneTemplate → SituationTemplate:** One-to-many embedded composition. SituationTemplates stored inline in SceneTemplate.SituationTemplates list, not as separate entities. Parser instantiates all situations when scene spawns. Not stored in GameWorld separately.

**SituationTemplate → ChoiceTemplate:** One-to-many embedded composition. ChoiceTemplates stored inline in SituationTemplate.ChoiceTemplates list. 2-4 choices (Sir Brante pattern) unless IsAutoAdvance. Parser validates count. Actions instantiated at query time by SceneFacade from these templates.

**Scene → Location/NPC/Route:** Scene has PlacementType enum + PlacementId string (concrete reference). Scene can be placed at ONE entity. UI queries check IsSceneAtLocation(): Location placement=direct match, NPC placement=check NPC.Location.Id, Route placement=check Route.OriginLocation.

**Situation → Template:** Situation stores Template reference (object) for lazy action instantiation. Template contains ChoiceTemplates used at query time. Not serialized (runtime-only reference).

**Choice → ProvisionalScene:** When SceneFacade instantiates actions from ChoiceTemplates, checks RewardTemplate.ScenesToSpawn. For each spawn reward, calls CreateProvisionalScene() and stores provisionalSceneId on action. Player sees "selecting this spawns X" before committing. If selected, FinalizeScene(). If not, DeleteProvisionalScene().

### Domain Invariants & Rules

**Sir Brante Pattern:** Every non-AutoAdvance situation has 2-4 choices. Exactly 2-4, never more or fewer. Four archetypal choices: stat-gated (test player skill), money (buy your way out), challenge (tactical gameplay), fallback (always available but costly). Parser validates this at parse time.

**Three-Tier Timing Enforcement:** Templates created at parse time (immutable). Scenes/Situations created at instantiation time (dormant). Actions created at query time (player enters context). Actions NEVER created at scene spawn - violates architecture. Template reference stored for lazy evaluation.

**HIGHLANDER Strictness:** Scene placement stored ONCE (PlacementType/PlacementId on Scene). Situations query via ParentScene.GetPlacementId() - no duplicate storage. Situations stored ONCE in GameWorld.Situations flat list. Scene references via SituationIds - no nested storage. Status/State control orthogonal dimensions - no computed properties that synthesize state from multiple sources.

**Catalogue Parse-Time Restriction:** Catalogues NEVER imported by facades/services/UI. Only SceneTemplateParser, SituationTemplateParser, PackageLoader call catalogues. After parsing, entities have concrete values. Runtime code uses strongly-typed properties only. String matching forbidden. Dictionary lookups forbidden.

**Perfect Information Display:** Costs visible before selection. Requirements visible with lock reason. Projected consequences visible (bond changes, scale shifts, state applications). Provisional scenes show WHERE it spawns and WHAT domain. Rewards HIDDEN until after selection (Sir Brante pattern - consequences uncertain, costs transparent).

**Placeholder Replacement Timing:** Templates have {NPCName}, {LocationName}, {PlayerName}, {VenueName} placeholders. NOT replaced at template load time. NOT replaced at scene creation time. Replaced at FinalizeScene() time when concrete placement known. PlaceholderReplacer.ReplaceAll() receives context (player, location, NPC, venue entities) and generates final display strings.

**Categorical Properties for AI:** Templates use enums and categorical descriptors, never concrete entity IDs (except starter/tutorial content). PersonalityTypes, LocationProperties, TerrainTypes enable procedural content generation. AI receives categorical properties in generation prompt, produces narrative fitting those categories. Content works with any entity matching filter.

---

## Current State Analysis

### What Exists and Works Well

**Template System:** SceneTemplate and SituationTemplate fully implemented with comprehensive properties. DTOs and parsers exist and work correctly. JSON content files (20_scene_templates.json, 21_tutorial_scenes.json) demonstrate proper structure. IsStarter spawning works (GameWorldInitializer.SpawnInitialScenes).

**Provisional Scene Lifecycle:** Three-state pattern implemented (Provisional→Active→Completed). CreateProvisionalScene(), FinalizeScene(), DeleteProvisionalScene() methods exist and orchestrate correctly. RewardApplicationService handles finalization after choice selection. State tracking works, just implemented deeply (violation of spec).

**Placement Resolution:** PlacementRelation enum works (SameNPC/SameLocation/SpecificNPC/SpecificLocation/Generic). Generic placement evaluation implemented for NPC (personality types, bond levels, player state filters) and Location (properties, player state filters). Route filtering blocked only because engineer incorrectly thought properties didn't exist.

**Three-Tier Timing:** Template loading at parse time works. Scene/Situation instantiation at spawn time works. Action instantiation at query time works via SceneFacade.ActivateSituation(). State machine prevents premature instantiation (Dormant situations don't create actions). Query time patterns correctly implemented.

**Archetype Generation:** SituationArchetypeCatalog generates 4 choices from archetype ID at PARSE TIME. If SituationTemplate has ArchetypeId, parser calls catalogue and embeds generated ChoiceTemplates. Used for procedural situation authoring. Correctly restricted to parse time (catalogue not imported by runtime code).

**Placeholder System:** PlaceholderReplacer utility exists. Called during FinalizeScene() for DisplayName and IntroNarrative. Also called during InstantiateSituation() for Situation.Description. Works correctly with context object containing entity references.

**Recursive Spawning:** SpawnFacade.ExecuteSpawnRules() handles SuccessSpawns and FailureSpawns. Called by SituationCompletionHandler and SituationFacade after situation resolution. Creates child situations with RequirementOffsets applied. Placement strategy resolved via ParentScene. Correctly integrated in completion lifecycle.

### What's Missing or Incomplete

**SpawnConditions System:** PlacementFilter exists (categorical entity selection) but SpawnConditions missing (temporal eligibility based on player/world/entity state). No way to express "spawn this scene only when weather=rain AND player completed scene X AND NPC bond >= 2". Templates either spawn always (when placement filter matches) or never. The core feature from refactoring.md not implemented.

**Route Filtering:** SceneInstantiator.FindMatchingRoute() has TerrainType and DangerRating checks COMMENTED OUT (lines 372-390). Engineer thought Route lacked properties, but Route.DangerRating exists (line 109 of RouteOption.cs), Route.HexPath provides TerrainType via Hex.Terrain. AnalyzeSegmentHexes() pattern already implemented in HexRouteGenerator - just needs to be copied.

**Scene State Machine Methods:** Scene has SpawnRules data but no behavior methods. No Scene.AdvanceToNextSituation(), no Scene.GetTransitionForCompletedSituation(), no Scene.IsComplete(). CurrentSituationId manually updated somewhere (not found in investigation). State transition logic should live in Scene domain entity, currently scattered across facades.

**Time-Based Spawning:** SceneSpawnReward has SpawnTiming (Immediate/Delayed) and DelayDays property, but no scheduler. Scenes spawn immediately or not at all. No "3 days after promising to help Marcus, spawn crisis scene" system. Would require time advancement hooks and deferred spawning queue.

**Scene Expiration:** SceneTemplate has ExpirationDays property, but no enforcement. Scenes don't auto-delete after time limit. Would require cleanup pass on time advancement checking Scene.Lifecycle.SpawnedDay + ExpirationDays < CurrentDay.

**Transition Execution:** SituationTransition entity exists (SourceSituationId, DestinationSituationId, Condition, SpecificChoiceId) but no code executes transitions. SpawnRules.Transitions parsed but unused. Scene progression likely manual CurrentSituationId changes (not found). Transition conditions (like "after situation X") not evaluated.

**Tag-Based Filtering:** PlacementFilter has NpcTags and LocationTags properties, but SceneInstantiator comments say NPCs/Locations don't have Tags properties (lines 308, 349). Tag filtering non-functional. Would need to add Tags property to NPC and Location entities.

**District/Region Filtering:** PlacementFilter has DistrictId and RegionId, but SceneInstantiator comments say Location lacks these properties (lines 337-347). Geographic filtering non-functional. Would need to add DistrictId/RegionId to Location entity.

**Selection Strategies:** When PlacementFilter matches multiple entities, code returns first match. No prioritization (highest bond, closest distance, random selection, least recently interacted). FindMatchingNPC/Location/Route use FirstOrDefault() - deterministic but not strategically interesting.

**Modal Scene UI Integration:** GetModalSceneAtLocation() query exists but unclear if UI actually renders full-screen takeover. SceneContent.razor handles modal display, but integration with automatic triggering on location entry not verified in investigation.

**Cascade Progression:** ProgressionMode enum exists (Breathe vs Cascade), but execution logic for immediate situation advancement not found. Unclear how "continue to next situation automatically" works.

### What's Architecturally Wrong

**Deep Provisional Instantiation:** CreateProvisionalScene() creates FULL Situation instances added to GameWorld.Situations immediately (lines 60-68 of SceneInstantiator). Violates shallow provisional spec. Every unselected action creates Situation tree in GameWorld requiring cleanup. Memory overhead ~10x necessary. Architecture permits recursion (prevented only by state machine timing, not by design).

**Naming Collision (State/Status):** SituationState enum has Dormant/Active. SituationStatus enum has Dormant/Available/Active/Completed/Failed. Both enums have "Dormant" and "Active" with completely different meanings. State.Dormant = actions not instantiated. Status.Dormant = requirements not met. State.Active = actions instantiated. Status.Active = player engaged. Developer confusion inevitable.

**Split Storage (ProvisionalScenes Dictionary):** GameWorld has separate `Dictionary<string, Scene> ProvisionalScenes` alongside `List<Scene> Scenes`. Scene.State already tracks Provisional vs Active. Dictionary unnecessary - should use single List with LINQ filtering. Dictionary chosen for "faster lookups" but provisional scenes short-lived (created and finalized within one action selection). Standard pattern: single collection, filter by state property.

**Missing Domain Methods (Scene):** Scene owns SpawnRules and CurrentSituationId but has no methods to manipulate them. AdvanceToNextSituation() should be Scene method, not facade responsibility. Domain entity should own its state machine. Facades orchestrate, entities execute. Current architecture scatters scene progression logic across multiple services violating single responsibility.

### Design Decisions Already In Place

**Eager Provisional Creation:** System creates provisional scenes WHEN ACTION DISPLAYED, not when action selected. Enables perfect information (player sees "this spawns X" before committing). Provisional deleted if unselected, finalized if selected. This pattern correct - just implemented too deeply (full Situation tree instead of metadata).

**No Events Architecture:** Spawn rules are DECLARATIVE DATA, not event handlers. SuccessSpawns executed synchronously by completion handler, not via event system. NO event subscriptions, NO async notifications, synchronous method calls orchestrate cascading. This is intentional architectural choice enforced by CLAUDE.md.

**Parse-Time Archetype Generation:** If SituationTemplate has ArchetypeId, parser calls SituationArchetypeCatalog during parsing, generates 4 ChoiceTemplates, embeds in template. Procedural generation happens ONCE at content load, not during gameplay. Runtime receives pre-generated choices, no catalogue calls. This pattern enables AI content authoring without runtime generation overhead.

**Categorical Filters Over Concrete IDs:** Templates use PersonalityTypes, LocationProperties, TerrainTypes (categorical), not locationId/npcId (concrete). Enables procedural placement - one template spawns at ANY merchant, not specific merchant. AI generates content with categorical requirements, game resolves to concrete entities at runtime.

**Strong Typing Throughout:** No Dictionary<string,object>, no var, no object, no HashSet<T>. Only List<T> where T is entity or enum. No weak types, compiler enforces correctness. Extension methods forbidden (hide domain logic). Helper classes forbidden (violate single responsibility). This strict typing policy enforced by CLAUDE.md.

**Holistic Refactoring Policy:** No gradual migration, no compatibility layers, no TODO comments. When refactoring, delete old code completely and replace with new. No legacy/deprecated patterns coexist. Change all files atomically or don't change at all. This scorched earth policy prevents half-refactored confusion.

---

## Design Approach & Rationale

### Core Design Decision: Shallow Provisional Architecture

**The Recursion Problem:** If we instantiate scenes to show consequences, we must instantiate situations. If we instantiate situations, we must instantiate choices. If we instantiate choices that spawn scenes, we must instantiate those scenes. Infinite loop.

**The Solution:** Provisional scenes are SHALLOW metadata only, not full instantiation. Properties: TemplateId, PlacementType, PlacementId, DisplayName (with unreplaced placeholders), SituationCount (int), EstimatedDifficulty (derived from template tier). NO Situation entities created. NO additions to GameWorld.Situations. Scene.SituationIds remains empty list.

**Why This Works:** Player needs to see "this choice spawns scene X at location Y with N situations" but doesn't need full Situation instances until scene finalized. Template properties queryable without instantiation (Domain from Template.ArchetypeSequence, SituationCount from Template.SituationTemplates.Count). When action selected, FinalizeScene() creates Situations FOR THE FIRST TIME.

**Alternatives Rejected:**
- **Full instantiation with cleanup:** Current approach. Works but wastes memory, requires complex cleanup (remove from GameWorld.Situations + Scene.SituationIds), enables recursion risk.
- **No provisional scenes:** Don't show spawn consequences. Violates perfect information principle - player can't make informed decision.
- **Template reference only:** Store SceneTemplateId on action without creating Scene. Problem: Can't show concrete placement (player needs to know WHERE it spawns, not just WHAT spawns).

**Why Shallow Wins:** 90% memory reduction (metadata strings/ints instead of full object graphs), trivial cleanup (delete Scene from list, no GameWorld.Situations entries), eliminates recursion risk (no Situations exist to transition Dormant→Active), maintains perfect information (player sees WHERE and WHAT, just not full detail).

### State Machine Naming Decision: InstantiationState vs LifecycleStatus

**The Problem:** Situation.State and Situation.Status both control situation lifecycle but have naming collision. State.Dormant != Status.Dormant. State.Active != Status.Active. Same words, completely different semantics. Developer reads "situation is Active" - which Active? Actions instantiated or player engaged?

**The Analysis:** These are genuinely orthogonal dimensions, not duplicate tracking. Proved by edge case analysis:
- Deferred + Selectable = Valid (situation visible, actions not instantiated yet)
- Instantiated + Completed = Valid (actions exist, situation resolved, cleanup later)
- Instantiated + Failed = Valid (actions exist, situation failed, retry available if Repeatable)
- Deferred + Completed = Impossible (can't complete without actions)
- Instantiated + Locked = Impossible (can't instantiate locked situations)

**The Solution:** Rename to eliminate collisions and clarify semantics. State→InstantiationState (Deferred/Instantiated) describes action materialization. Status→LifecycleStatus (Locked/Selectable/InProgress/Completed/Failed) describes progression tracking. Zero semantic changes, pure naming clarity.

**Alternatives Considered:**
- **Single state machine:** Consolidate into one enum with values covering both dimensions. Rejected because dimensions are genuinely orthogonal - situation can be Instantiated+Completed simultaneously.
- **Keep current names, better docs:** Add XML comments explaining difference. Rejected because naming collisions cause confusion regardless of documentation - developers trust names more than comments.
- **Different prefix scheme:** ActivationState/ProgressStatus or VisibilityState/ResolutionStatus. Rejected because "Instantiation" precisely describes the operation (ChoiceTemplate→Action), "Lifecycle" clearly scopes to progression.

**Why This Wins:** Zero collisions (no value appears in both enums), semantic precision (property names describe what they control), intent clarity (value names describe actual behavior), architectural honesty (names match implementation reality).

### SpawnConditions Design: Three-Part Condition Structure

**The Problem:** PlacementFilter handles WHERE scenes can appear (which entities match), but no system for WHEN scenes become eligible (temporal/state-based conditions). Need to express "spawn this scene only when: player completed prior scene AND NPC bond >= 3 AND weather is Rain".

**The Architecture:** Three condition categories with AND/OR combination logic:

**PlayerStateConditions:** CompletedScenes (list of scene IDs), ChoiceHistory (list of choice IDs), MinStats (scale thresholds), RequiredItems (inventory checks), LocationVisits (visit count thresholds). Answers "has player done X?"

**WorldStateConditions:** Weather enum, TimeBlock enum, CurrentDay range (min/max), LocationStates (discovered/hostile/etc). Answers "what's the current world state?"

**EntityStateConditions:** NPCBond (thresholds per NPC), LocationReputation, RouteTravelCount, Properties (dynamic entity properties). Answers "what's the relationship with this entity?"

**Why Three Categories:** Separates concerns - player progression vs world state vs relationships. Each category evaluated independently then combined via AND/OR logic. Enables complex conditions without nested complexity.

**Alternatives Rejected:**
- **Single flat requirement list:** Mix all condition types together. Rejected because loses semantic grouping - harder to understand intent.
- **Arbitrary boolean expressions:** Allow any combination with parentheses. Rejected because too complex for content authoring - AI can't generate, designers can't reason about.
- **Implicit AND only:** No OR support. Rejected because some scenarios need "either this or that" (example: "spawn if player chose path A OR path B").

**Integration Points:** SpawnConditionsEvaluator service evaluates conditions. Called BEFORE spawning in: SceneInstantiator.CreateProvisionalScene (check before provisional creation), SpawnFacade.ExecuteSpawnRules (check before recursive spawn), GameWorldInitializer.SpawnInitialScenes (check starter scenes). Template with failing conditions doesn't spawn - elegantly filters eligibility.

### Route Filtering Fix: Leverage Existing Hex System

**The Problem:** SceneInstantiator.FindMatchingRoute() has TerrainType and DangerRating checks commented out. Engineer wrote TODO saying "Route entity missing TerrainType, Tier, DangerRating properties" but this is incorrect.

**The Discovery:** Route.DangerRating property EXISTS (line 109 of RouteOption.cs). Route.HexPath property EXISTS (List<AxialCoordinates>). Hex entities have Terrain and DangerLevel properties accessible via GameWorld.WorldHexGrid.GetHex(coords). Reference implementation ALREADY EXISTS in HexRouteGenerator.AnalyzeSegmentHexes() - counts terrain types, finds dominant, averages danger.

**The Solution:**
- DangerRating filtering: Uncomment lines 386-390, works immediately (property exists and is populated)
- TerrainType filtering: Copy AnalyzeSegmentHexes pattern, iterate route.HexPath, query hexes, count terrain types, return most common
- Tier mapping: Route doesn't have Tier property, but can derive from DangerRating ranges (0-25=Tier 0, 26-50=Tier 1, 51-75=Tier 2, 76+=Tier 3)

**Why This Works:** Route stores HexPath, Hex stores Terrain - data already exists, just needs traversal logic. DangerRating pre-calculated (no need to sum hex danger levels). Pattern proven working in HexRouteGenerator. Copy, don't reinvent.

**Alternative Rejected:** Add TerrainType property directly to Route entity. Rejected because loses fidelity - routes traverse multiple terrain types, single property can't capture "forest transitioning to mountain" accurately. Dominant terrain calculation preserves route character.

### Scene State Machine: Domain Entity Owns Its Lifecycle

**The Problem:** Scene has SpawnRules (flow pattern, transitions, completion condition) and CurrentSituationId (progression tracking) but no methods to manipulate these. Transition logic scattered across facades. Scene can't answer "am I complete?" or "what's next situation?" without external service querying its data.

**The Solution:** Add domain methods to Scene entity:
- Scene.AdvanceToNextSituation(completedSituationId): Query SpawnRules.Transitions, find matching transition, update CurrentSituationId, mark scene complete if no more transitions
- Scene.GetTransitionForCompletedSituation(situationId): Return next situation ID based on transition rules
- Scene.IsComplete(): Check if all situations resolved (CurrentSituationId null or no valid transitions)

**Why This Matters:** Domain-driven design principle - entity owns behavior, not just data. Scene understands its own lifecycle. Facades orchestrate ("scene, advance to next situation") instead of manipulating ("let me read your transitions and update your current situation"). Encapsulation, single responsibility, testability.

**Alternatives Rejected:**
- **SceneProgressionService:** Extract scene progression logic to dedicated service. Rejected because scene progression is core scene behavior, not separate concern.
- **Keep in facades:** Current approach. Rejected because scatters scene logic across multiple files, violates single responsibility.

**Integration:** SituationCompletionHandler calls Scene.AdvanceToNextSituation() after situation resolves. SceneFacade calls Scene.IsComplete() to check if scene finished. Scene owns decision logic, facades orchestrate calls.

---

## Implementation Strategy

### Phase 1: Architectural Foundation (Critical Fixes)

Begin with the three critical violations that block all other work. These are not features, they're corrections to existing architecture that violate spec or create confusion.

**Phase 1.1 (State/Status Rename):** Create InstantiationState enum file with Deferred/Instantiated values and clear XML documentation. Rename SituationStatus file to LifecycleStatus, update enum values to Locked/Selectable/InProgress/Completed/Failed. Update Situation.cs property declarations and XML comments. Search entire codebase for SituationState references, replace with InstantiationState, update Dormant→Deferred and Active→Instantiated. Search for SituationStatus references, replace with LifecycleStatus, update Dormant→Locked, Available→Selectable, Active→InProgress. Update computed properties (IsAvailable, IsCompleted, add HasInstantiatedActions). Files affected: 2 enum files, Situation.cs, SceneFacade.cs, SituationFacade.cs, SituationCompletionHandler.cs, SpawnFacade.cs, SceneInstantiator.cs, GameFacade.cs, HexRouteGenerator.cs.

**Phase 1.2 (Shallow Provisional):** Add SituationCount and EstimatedDifficulty properties to Scene entity. In SceneInstantiator.CreateProvisionalScene(), DELETE the foreach loop that instantiates Situations (lines 60-68). Set scene.SituationCount = sceneTemplate.SituationTemplates.Count. Calculate EstimatedDifficulty from template tier and requirement complexity. Store Scene in GameWorld.ProvisionalScenes with EMPTY SituationIds list. In SceneInstantiator.FinalizeScene(), ADD the foreach loop to instantiate Situations FOR THE FIRST TIME. Iterate sceneTemplate.SituationTemplates, call InstantiateSituation(), add to GameWorld.Situations, populate Scene.SituationIds. Placeholder replacement happens here (already implemented, just moved timing). In SceneInstantiator.DeleteProvisionalScene(), simplify cleanup - just remove Scene from ProvisionalScenes collection, NO GameWorld.Situations cleanup needed (nothing was added). Files affected: Scene.cs, SceneInstantiator.cs, RewardApplicationService.cs.

**Phase 1.3 (Scene State Machine):** Add three methods to Scene.cs. AdvanceToNextSituation(completedSituationId) queries SpawnRules.Transitions for matching source, updates CurrentSituationId to destination, if no valid transitions sets CurrentSituationId to null and State to Completed. GetTransitionForCompletedSituation(situationId) returns matching SituationTransition or null. IsComplete() returns CurrentSituationId == null or State == Completed. In SituationCompletionHandler.CompleteSituation(), after applying consequences, call parentScene.AdvanceToNextSituation(situation.Id) instead of manual CurrentSituationId update. In SceneFacade, replace checks for "no more situations" with parentScene.IsComplete() calls. Files affected: Scene.cs, SituationCompletionHandler.cs, SceneFacade.cs.

**Phase 1.4 (Consolidate Collections):** In GameWorld.cs, delete ProvisionalScenes Dictionary property. In SceneInstantiator, change CreateProvisionalScene() to add scene to GameWorld.Scenes list with State=Provisional instead of GameWorld.ProvisionalScenes dictionary. Change FinalizeScene() to find scene via LINQ query (Scenes.First(s => s.Id == sceneId && s.State == SceneState.Provisional)) instead of dictionary lookup. Change state to Active without moving between collections. Change DeleteProvisionalScene() to find via LINQ and remove from Scenes list. In SceneFacade, query provisional scenes via Scenes.Where(s => s.State == SceneState.Provisional). Files affected: GameWorld.cs, SceneInstantiator.cs, SceneFacade.cs.

### Phase 2: SpawnConditions System (Core Feature)

Implement the three-part conditional spawning system that enables dynamic narrative emergence.

**Phase 2.1 (Domain Entity):** Create SpawnConditions.cs record in GameState folder. Three nested record types: PlayerStateConditions (CompletedScenes list, ChoiceHistory list, MinStats dictionary, RequiredItems list, LocationVisits dictionary), WorldStateConditions (Weather enum, TimeBlock enum, CurrentDay min/max range, LocationStates list), EntityStateConditions (NPCBond dictionary, LocationReputation dictionary, RouteTravelCount dictionary, Properties list). Root record has these three properties plus CombinationLogic enum (AND/OR). Initialize all collections inline to prevent null references (LET IT CRASH philosophy).

**Phase 2.2 (DTO and Parser):** Create SpawnConditionsDTO.cs matching domain structure. Create SpawnConditionsParser.cs with ParseSpawnConditions(dto) method returning domain entity. Validate enum values, convert collections, throw InvalidOperationException on unknown values (fail fast). No catalogue usage - SpawnConditions is data structure, not requiring translation.

**Phase 2.3 (Template Integration):** Add SpawnConditions property to SceneTemplate with init accessor. Add SpawnConditionsDTO property to SceneTemplateDTO. In SceneTemplateParser.ParseSceneTemplate(), if DTO has spawn conditions, call SpawnConditionsParser and assign to template. Update JSON schema documentation. Add example spawn conditions to test content (weather-based route scene, consequence-triggered NPC scene).

**Phase 2.4 (Evaluation Service):** Create SpawnConditionsEvaluator.cs service in Services folder. Constructor takes GameWorld and Player references. Three evaluation methods: EvaluatePlayerStateConditions checks CompletedScenes (player.CompletedSceneIds contains), ChoiceHistory (player.ChoiceHistory contains), MinStats (player.GetScale() >= threshold), RequiredItems (player.Inventory.Items contains), LocationVisits (player.LocationVisits[id] >= count). EvaluateWorldStateConditions checks Weather (gameWorld.Weather matches), TimeBlock (gameWorld.TimeBlock matches), CurrentDay (gameWorld.CurrentDay between min/max), LocationStates (location.Properties contains). EvaluateEntityStateConditions checks NPCBond (player.GetBondWithNPC() >= threshold), properties vary by entity type. EvaluateAll() combines results via CombinationLogic (AND=all true, OR=any true), returns boolean. Register as singleton in DI. Files affected: 5 new files, dependency injection configuration.

**Phase 2.5 (Integration Points):** In SceneInstantiator.CreateProvisionalScene(), before creating scene, check if template.SpawnConditions exists. If exists, call spawnConditionsEvaluator.EvaluateAll(). If false, return null (don't create provisional scene). In SpawnFacade.ExecuteSpawnRules(), before cloning situation, get source template, check spawn conditions. In GameWorldInitializer.SpawnInitialScenes(), check spawn conditions before spawning starters. Null SpawnConditions evaluates to true (always spawn). Files affected: SceneInstantiator.cs, SpawnFacade.cs, GameWorldInitializer.cs, RewardApplicationService.cs (for provisional creation).

### Phase 3: Route Filtering (Quick Win)

Enable commented-out route filtering and add terrain type support by leveraging existing Hex system.

**Phase 3.1 (DangerRating):** In SceneInstantiator.FindMatchingRoute(), uncomment lines 386-390 (MinDangerRating and MaxDangerRating checks). Route.DangerRating property exists and is populated. No other changes needed.

**Phase 3.2 (TerrainType):** In SceneInstantiator, add private helper method GetDominantTerrainForRoute(List<AxialCoordinates> hexPath, GameWorld gameWorld). Copy pattern from HexRouteGenerator.AnalyzeSegmentHexes(). Iterate hexPath, call gameWorld.WorldHexGrid.GetHex(coords) for each coordinate, access hex.Terrain property, count terrain types in dictionary, return most common. In FindMatchingRoute(), uncomment lines 372-377, call GetDominantTerrainForRoute(route.HexPath, _gameWorld), convert TerrainType enum to string, check if filter.TerrainTypes contains string.

**Phase 3.3 (Tier Mapping):** In FindMatchingRoute(), uncomment lines 379-384. Route doesn't have Tier property, so calculate from DangerRating. Use switch expression: <25 = Tier 0, <50 = Tier 1, <75 = Tier 2, >= 75 = Tier 3. Compare calculated tier to filter.RouteTier.

Single file affected: SceneInstantiator.cs. Three methods modified: FindMatchingRoute (uncomment + tier mapping), one new helper method (GetDominantTerrainForRoute).

### Phase 4: Testing and Validation

Create test content and validate the entire spawning system works correctly.

**Phase 4.1 (Test Content):** Create 31_test_spawn_conditions.json with four test scene templates. Environmental route scene with SpawnConditions (Weather=Rain AND route TerrainType contains Mountain) and PlacementFilter (TerrainType=Mountain, DangerRating 50-100). Consequence-triggered NPC scene with SpawnConditions (CompletedScenes contains prior_scene AND ChoiceHistory contains promise_choice AND NPCBond >= 2) and PlacementFilter (personality types, specific NPC for test). Location scene with SpawnConditions (TimeBlock=Evening AND CurrentDay 3-7) and PlacementFilter (location properties). Recursive spawning scene with SuccessSpawns having SpawnConditions.

**Phase 4.2 (Memory Verification):** Build project. Start debugger, set breakpoint in SceneFacade when creating provisional scenes. Step through CreateProvisionalScene, verify Scene.SituationIds empty list. Check GameWorld.Situations count before/after provisional creation - should NOT increase. Verify Scene has SituationCount and EstimatedDifficulty metadata properties populated. Select action with provisional scene, step through FinalizeScene, verify Situations created for first time, added to GameWorld.Situations, Scene.SituationIds populated. Confirm memory footprint minimal (no Situation objects in provisionals).

**Phase 4.3 (SpawnConditions Testing):** Load test content package. Set world state (Weather=Rain, TimeBlock=Evening). Navigate to mountain route, verify environmental scene spawns. Change weather to Clear, reload, verify scene doesn't spawn. Complete prerequisite scene, make promise choice, advance time 3 days, verify consequence scene spawns at correct NPC. Test with conditions not met (wrong weather, incomplete prerequisites), verify scenes don't spawn. Test recursive spawning with conditions, verify child situations spawn only when conditions satisfied.

**Phase 4.4 (Route Filtering Testing):** Create scene with route placement requiring Mountain terrain and DangerRating >= 50. Verify spawns only on dangerous mountain routes, not on plains routes or safe mountain routes. Test TerrainType filter with multiple types (Forest OR Mountain), verify matches routes with either dominant terrain. Test Tier mapping, verify tier-gated content spawns only on appropriately dangerous routes.

**Phase 4.5 (State Machine Validation):** Start scene with multiple situations. Complete first situation, verify Scene.AdvanceToNextSituation() correctly updates CurrentSituationId. Complete all situations in sequence, verify Scene.IsComplete() returns true and State transitions to Completed. Verify SceneFacade respects scene completion (doesn't try to activate nonexistent situations). Debug InstantiationState and LifecycleStatus semantics - verify naming clear in watch windows, no confusion about which dimension controls what.

**Phase 4.6 (Architectural Validation):** Review Phase 1 changes - confirm InstantiationState/LifecycleStatus have zero naming collisions, provisional scenes shallow (no Situations until finalization), Scene has state machine methods and facades call them, single Scenes collection (no ProvisionalScenes Dictionary). Review Phase 2 changes - confirm SpawnConditions entities exist, evaluator service works, integration points check conditions before spawning. Review Phase 3 changes - confirm route filtering uncommented and working with Hex system. Build must succeed with zero errors, zero warnings.

---

## Critical Constraints

### From CLAUDE.md Architecture

**HIGHLANDER Principle:** ONE concept, ONE representation, used CONSISTENTLY everywhere. Never store same information in multiple forms that can desynchronize. Scene stores SituationIds (references), GameWorld.Situations is single source of truth (entities). Situation has ParentScene object reference (runtime navigation), NO ParentSceneId property alongside (desync risk). If adding provisional scene metadata, DO NOT duplicate template properties - query template via reference.

**Catalogue Pattern:** Parse-time translation ONLY. Catalogues NEVER imported by facades/services/UI. After parsing, entities have concrete strongly-typed properties (int, bool, enum). Runtime uses properties directly, no catalogue calls, no string matching, no dictionary lookups. SpawnConditionsEvaluator is NOT a catalogue (doesn't translate categorical→concrete, evaluates runtime state against conditions).

**LET IT CRASH Philosophy:** Collections ALWAYS initialize inline on entity classes. Parsers assign directly, NO null-coalescing operators. Game logic trusts entity initialization, no defensive ?? fallbacks. SpawnConditions properties initialized empty lists/dictionaries, null SpawnConditions evaluates to "always spawn". ArgumentNullException ONLY for constructor parameters. Fail fast with descriptive errors, don't hide problems.

**No Events Architecture:** Spawn rules are DECLARATIVE DATA executed synchronously, not event handlers. SpawnFacade.ExecuteSpawnRules() called directly by completion handler, not via event subscription. NO async/await for spawning (unless IO-bound operations like AI generation). NO event aggregators, NO publish/subscribe. Synchronous method calls orchestrate cascading.

**Strong Typing Strictness:** No Dictionary<string,object>, no var, no object, no HashSet<T>. Only List<T> where T is entity or enum. SpawnConditions uses strongly-typed nested records, not Dictionary<string, List<string>>. Enum routing via switch expressions, not string matching. Compiler enforces correctness, IntelliSense guides usage.

**Holistic Refactoring:** No gradual migration, no compatibility layers, no TODO comments in production. When renaming State→InstantiationState, change ALL files atomically. Delete old enum, create new enum, update all references, build must succeed. No intermediate state where both enums coexist. Scorched earth: finish what you start, leave no legacy code.

**Parse-JSON-Entity Triangle:** Parser sets property → JSON must have field → Entity must have property. All three layers must align. Adding SpawnConditions property: add to SceneTemplate (entity), SceneTemplateDTO (JSON), SceneTemplateParser (parse logic). NO JsonPropertyName attributes (field names must match), fix source data instead of hiding mismatches.

**Semantic Honesty:** Method names, parameter names, property names must match what they actually do. InstantiationState describes action materialization (ChoiceTemplate→LocationAction). LifecycleStatus describes progression tracking (Available→InProgress→Completed). If method queries spawn conditions, name it EvaluateSpawnConditions not CheckIfSceneCanSpawn. Names are contracts, lying is forbidden.

**Three-Tier Timing Enforcement:** Parse-time (Template load), Instantiation-time (Scene/Situation creation, Dormant state), Query-time (Action instantiation, Active state). Actions NEVER created at instantiation time. Situation stores Template reference for lazy evaluation. Provisional scenes stay in Deferred instantiation state (no query-time triggers). Catalogues ONLY at parse-time, SpawnConditionsEvaluator ONLY at instantiation-time.

### From Investigation Discoveries

**Shallow Provisional Non-Negotiable:** Provisional scenes MUST be metadata only. NO Situation entities in GameWorld.Situations until FinalizeScene(). Scene.SituationIds remains empty until finalization. CreateProvisionalScene() calculates SituationCount and EstimatedDifficulty from template, stores in scene properties. Deep instantiation violates spec and enables recursion risk.

**Route-Hex Access Pattern:** Route.DangerRating is direct property access (pre-calculated). Route.TerrainType must be CALCULATED from Route.HexPath by iterating coordinates, querying GameWorld.WorldHexGrid.GetHex(coords), counting Hex.Terrain types. DO NOT add TerrainType property to Route entity (loses multi-terrain fidelity). Copy AnalyzeSegmentHexes() pattern from HexRouteGenerator, don't reinvent.

**Scene Owns State Machine:** Scene.AdvanceToNextSituation() method queries OWN SpawnRules property, updates OWN CurrentSituationId property. Method is Scene behavior, not facade responsibility. Facades call scene.AdvanceToNextSituation(), don't manipulate scene.CurrentSituationId directly. Domain entity owns lifecycle, facades orchestrate.

**Orthogonal Dimensions Proof:** InstantiationState and LifecycleStatus control independent concerns proved by edge cases (Instantiated+Completed valid, Deferred+Completed impossible). Must remain separate properties, not consolidated into single state machine. Renaming eliminates collision without merging semantics.

**Placement Inheritance:** Situation doesn't store own placement. Situation queries ParentScene.GetPlacementId(placementType) for placement. Scene owns placement (PlacementType/PlacementId), situations inherit. DO NOT add PlacementType/PlacementId properties to Situation entity. SpawnRule inherits placement from parent via PlacementStrategy enum.

**SpawnConditions Nullability:** Null SpawnConditions on SceneTemplate means "always eligible" (no temporal filtering). Evaluator checks if conditions null, returns true (spawn unconditionally). Non-null SpawnConditions with empty lists means "no restrictions in this category" (PlayerStateConditions.CompletedScenes empty = don't check completed scenes). Null vs empty have different meanings.

**Categorical Property Strategy:** SpawnConditions uses concrete values (player.CompletedSceneIds contains "scene_id"), not categorical descriptors. PlacementFilter uses categorical (PersonalityTypes, LocationProperties). SpawnConditions is runtime state evaluation, PlacementFilter is entity selection. Different purposes, different patterns. Don't confuse condition checking with filter matching.

---

## Key Files & Their Roles

### Domain Entities (src/GameState/)

**Scene.cs:** Runtime scene instance with State (Provisional/Active/Completed), SpawnRules, CurrentSituationId. Add SituationCount and EstimatedDifficulty properties for shallow provisionals. Add AdvanceToNextSituation(), GetTransitionForCompletedSituation(), IsComplete() methods for state machine. Currently 300 lines, will grow to ~350 with new methods.

**Situation.cs:** Runtime situation instance with State/Status properties. Rename State→InstantiationState, Status→LifecycleStatus. Update enum references throughout. Update computed properties (IsAvailable, IsCompleted, add HasInstantiatedActions). Currently 320 lines, minimal changes (property names and enums).

**SceneTemplate.cs:** Immutable scene archetype. Add SpawnConditions property (init-only) for conditional spawning. Currently ~150 lines, grows to ~160.

**SpawnConditions.cs (NEW):** Three-part condition structure record. PlayerStateConditions, WorldStateConditions, EntityStateConditions nested records. CombinationLogic enum. All collections initialized inline. Estimated ~80 lines.

### Enums (src/GameState/Enums/ and src/GameState/Cards/)

**SituationState.cs:** Rename file to InstantiationState.cs, rename enum, change values Dormant→Deferred, Active→Instantiated. Update XML documentation. ~20 lines.

**SituationStatus.cs:** Rename file to LifecycleStatus.cs, rename enum, change values Dormant→Locked, Available→Selectable, Active→InProgress. Update XML documentation. ~30 lines.

### DTOs (src/Content/DTOs/)

**SceneTemplateDTO.cs:** Add SpawnConditionsDTO property. Minimal change, ~5 lines added to ~150 line file.

**SpawnConditionsDTO.cs (NEW):** Matches SpawnConditions domain structure. Three nested classes. ~70 lines.

### Parsers (src/Content/Parsers/)

**SceneTemplateParser.cs:** Call SpawnConditionsParser if DTO has spawn conditions. Minimal addition to existing ~400 line file, ~10 lines added.

**SpawnConditionsParser.cs (NEW):** Parse spawn conditions from DTO to domain. Validate enums, convert collections, throw on invalid data. ~120 lines.

### Content Instantiation (src/Content/)

**SceneInstantiator.cs:** MAJOR REFACTORING. CreateProvisionalScene() - DELETE Situation instantiation loop (lines 60-68), ADD SituationCount/EstimatedDifficulty calculation. FinalizeScene() - ADD Situation instantiation loop (moved from CreateProvisionalScene). DeleteProvisionalScene() - simplify cleanup. FindMatchingRoute() - UNCOMMENT TerrainType/DangerRating checks (lines 372-390), ADD GetDominantTerrainForRoute() helper, ADD tier mapping logic. Add SpawnConditionsEvaluator call in CreateProvisionalScene (check before spawning). Currently ~500 lines, significant changes throughout.

**PackageLoader.cs:** No changes needed for Phase 1-3. May need spawn conditions validation in future. Currently ~400 lines.

### Services (src/Services/)

**SpawnConditionsEvaluator.cs (NEW):** Three evaluation methods (PlayerState, WorldState, EntityState), EvaluateAll() with AND/OR logic. Constructor takes GameWorld and Player. Singleton registration in DI. ~200 lines estimated.

**GameFacade.cs:** Update DeactivateSituation() to use InstantiationState instead of State. Minimal change to ~2000 line file.

**GameWorldInitializer.cs:** Add SpawnConditionsEvaluator call when spawning initial scenes (check conditions for IsStarter templates). ~10 lines added to ~300 line file.

### Facades (src/Subsystems/)

**SceneFacade.cs (src/Subsystems/Scene/):** Update all State references to InstantiationState, Dormant→Deferred, Active→Instantiated. Update provisional scene queries to filter Scenes list instead of ProvisionalScenes dictionary. Call Scene.IsComplete() instead of manual CurrentSituationId checks. Currently ~500 lines, changes throughout.

**SituationFacade.cs (src/Subsystems/Situation/):** Update Status references to LifecycleStatus, Active→InProgress, Available→Selectable. Currently ~400 lines, changes scattered.

**SpawnFacade.cs (src/Subsystems/Spawn/):** Update State/Status enum references. Add SpawnConditionsEvaluator call before executing spawn rules. Currently ~200 lines, minimal additions.

**SituationCompletionHandler.cs (src/Services/):** Update Status enum references. Call Scene.AdvanceToNextSituation() instead of manual CurrentSituationId manipulation. Currently ~150 lines, focused changes.

### Core State (src/GameState/)

**GameWorld.cs:** DELETE ProvisionalScenes Dictionary property. Scenes List remains. Currently ~300 lines, one property deleted.

**HexRouteGenerator.cs (src/Services/):** Update State enum references if used. Pattern source for GetDominantTerrainForRoute() (copy AnalyzeSegmentHexes logic). Currently ~1000+ lines, minimal changes if any.

### Reward Application (src/Services/)

**RewardApplicationService.cs:** Update FinalizeSceneSpawns() to call updated SceneInstantiator.FinalizeScene(). Check SpawnConditionsEvaluator before creating provisional scenes if needed. Currently ~400 lines, minimal changes.

### Content Files (src/wwwroot/content/)

**31_test_spawn_conditions.json (NEW):** Test scene templates with spawn conditions. Environmental route scene, consequence-triggered NPC scene, location scene, recursive spawning scene. ~300 lines estimated.

**20_scene_templates.json, 21_tutorial_scenes.json:** Update documentation, optionally add spawn conditions to existing templates. Changes depend on desired test coverage.

---

## Integration Context

This work builds on completed Phase 0 (architectural cleanup: SpawnTracking extraction, Placement deduplication, Status deduplication). Scene/Situation spawning system is foundational for all narrative content - every conversation, quest, environmental event, consequence uses this architecture. SpawnConditions enables emergence (scenes appear/disappear based on player actions and world state). Shallow provisional fixes memory issues blocking mobile deployment. State/Status rename unblocks developer onboarding (current naming confusion documented in team complaints).

Future work depends on this foundation: Time-based spawning (DelayDays scheduler), scene expiration (ExpirationDays enforcement), transition execution (situation cascade), tag filters (entity Tags properties), obligation integration (phase→scene linkage), selection strategies (prioritized/weighted entity matching), modal UI (forced narrative takeover), AI narrative generation (procedural content). All deferred per user MVP scope decision - core spawning first, advanced features later.

---

## Success Criteria

Phase 1 complete when: Build succeeds with zero errors, InstantiationState/LifecycleStatus properties exist with clear non-colliding names, provisional scenes create zero Situations until finalized (GameWorld.Situations unchanged during provisional creation), Scene.AdvanceToNextSituation() method exists and facades call it, GameWorld has single Scenes collection (ProvisionalScenes Dictionary deleted).

Phase 2 complete when: SpawnConditions entity/DTO/parser exist, SceneTemplate has SpawnConditions property, SpawnConditionsEvaluator evaluates all three condition types, scenes spawn ONLY when conditions met (test with false conditions, verify no spawn), test content demonstrates conditional spawning in game.

Phase 3 complete when: Route filtering by TerrainType works (test mountain route scene), Route filtering by DangerRating works (test safe vs dangerous), Route filtering by Tier works (test tier-gated content), SceneInstantiator.FindMatchingRoute() uncommented and functional.

Phase 4 complete when: Test scenes spawn correctly in-game browser, provisional scene memory minimal (debug confirms no Situations), no recursion (provisional scenes stay Deferred), player sees spawn consequences before selecting, all architectural constraints validated via review.

Overall success: MVP delivers conditional spawning (SpawnConditions), fixes three architectural violations (shallow provisionals, state/status clarity, scene state machine), enables route filtering, maintains perfect information, prevents recursion, upholds HIGHLANDER/catalogue/typing principles from CLAUDE.md. System ready for future enhancements (time-based, tags, obligations, AI generation).
