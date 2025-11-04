# Procedural Content Generation - Implementation Gaps

## Problem & Motivation

Wayfarer's narrative RPG creates exponential content authoring burden. Every location needs scenes, every NPC needs interactions, every choice spawns follow-ups. A single tutorial scene showing "player secures lodging" requires authoring situation descriptions, choice texts, outcomes, and costs. If that exact pattern appears nowhere else, the effort yields one moment of gameplay.

Traditional procedural generation fails because narrative quality collapses into templated mad-libs. AI generation alone fails because AI cannot create balanced mechanics - it doesn't understand "this costs 33% of max Focus because progression balance" or "this spawns consequence at bond threshold 5 for systemic coherence."

**The Solution:** Separate mechanical patterns from narrative manifestation. Game logic defines structure through archetypes and templates. AI applies narrative texture to concrete entity contexts. Same mechanical skeleton, infinite contextual variations.

**This Document:** Identifies what remains unimplemented in the procedural generation architecture, enabling a fresh session to complete the system without prior context.

---

## Architectural Discovery

### The Procedural Generation Vision

**SceneTemplates Are Complete Interaction Flows**

A SceneTemplate is NOT a single action. The tutorial "Secure Lodging" template encompasses:
- Locked room discovery at inn
- Conversation with innkeeper NPC
- Payment transaction (coins)
- Room unlock consequence
- Navigation to room
- Rest action (resource recovery, time passage)
- Morning awakening (new timeblock)
- Exit and room re-lock

This entire multi-step flow is ONE SceneTemplate. Procedural generation creates templates that reproduce this pattern at different locations with different NPCs and contextual variations, maintaining mechanical consistency while varying narrative flavor.

**Three-Tier Timing Model**

1. **Parse Time**: JSON templates loaded, catalogues translate categorical properties to concrete mechanical values once
2. **Instantiation Time**: Spawn conditions trigger, templates become runtime scene instances with dormant situations
3. **Query Time**: Player enters context, situations activate, choices instantiate as ephemeral actions

**Mechanical vs Narrative Separation**

- **Game Logic Owns Mechanics**: Costs, requirements, rewards, formulas, balance, progression scaling
- **AI Owns Narrative**: Descriptions, dialogue, atmospheric detail, personality expression
- **Interface**: Entity context bundling - concrete NPC/Location/Player objects with all properties inform AI prompts

The archetype catalog generates mechanical choice structures during parsing. AI generates narrative text at scene finalization when concrete entity context is available. No mixing - mechanics stay deterministic, narrative stays contextual.

### The HIGHLANDER Principle

Every concept has exactly one authoritative representation. Situations live in GameWorld flat list, scenes reference them by ID. Placement lives on scene, situations query parent. No duplication, no desynchronization risk. ProcessTimeAdvancement is the single sync point for ALL time-based side effects throughout the codebase.

### Strongly-Typed Everything

No dictionaries with string keys, no var keyword, no object types, no HashSet collections, no runtime string matching. SpawnConditions are nested records with enum properties. PlacementFilters have typed lists of personality enums. Cost templates have decimal properties for specific resources. Compiler catches errors, IDE provides autocomplete, refactoring propagates safely.

---

## Domain Model Understanding

### SceneTemplate & SituationTemplate Hierarchy

**SceneTemplate** defines complete multi-step narrative arc structure:
- Placement filters (categorical NPC/Location/Route requirements)
- Spawn conditions (when content becomes eligible)
- Situation flow rules (linear/branching progression)
- Metadata for AI narrative hints

**SituationTemplate** (embedded within scenes) specifies decision points:
- Archetype ID reference (which mechanical pattern)
- Tier level (difficulty/resource scaling)
- Narrative hints (tone, theme, context)
- Override parameters (if non-formula costs needed)

**ChoiceTemplate** (embedded within situations) defines player actions:
- Action type (instant/challenge/navigation)
- Cost formulas (resource consumption)
- Requirement formulas (stat thresholds, items)
- Reward templates (consequences on success/failure)

Templates contain ZERO narrative text. They are pure mechanical specifications. All narrative comes from AI generation at finalization using concrete entity contexts.

### Archetype as Mechanical Pattern

An archetype is a reusable interaction structure. "Negotiation with bypass" defines: challenge path (costs Focus, requires Social stat, grants bond), payment path (costs coins, instant success, no bond), fallback path (expensive, always available, reputation penalty).

This pattern applies to securing lodging, gaining warehouse access, passing checkpoints, convincing guards. The archetype specifies choice count, action types, cost formulas, requirement formulas, and reward templates - completely independent of narrative context.

### Placement Filters & Spawn Conditions

**PlacementFilter** determines WHERE content appears through categorical properties:
- NPC personality types, bond ranges, tags
- Location properties, district/region, tags
- Route terrain types, danger ratings
- Player state requirements (scales, achievements, items)

Filter evaluation at spawn time resolves categorical descriptions to concrete entities. Multiple matches trigger selection strategy (WeightedRandom, Closest, HighestBond, LeastRecent).

**SpawnConditions** determine WHEN content becomes eligible:
- Player state (completed scenes, choice history, stat thresholds, items)
- World state (time of day, weather, day ranges, location states)
- Entity state (NPC bonds, location reputation, route travel counts)

Conditions evaluated before spawning gate content behind progression, create temporal variety, react to player decisions.

### Template Mutability Contract

Templates are immutable after parsing. Hand-authored tutorial templates have concrete placements (Elena by ID) and exact costs (15 coins fixed). Procedural templates have categorical filters (any Merchant at Urban location with bond 0-2) and formula-driven costs (33% of max Focus scaled by tier).

Instances are mutable during gameplay. Scenes track current situation, situations track completion, spawned entities accumulate. Instance mutation reflects state evolution, never alters template definitions.

---

## Current State Analysis

### What Exists and Works

**Infrastructure Complete:**
- SceneTemplate and SituationTemplate entity structures exist
- PlacementFilter evaluation with all categorical properties functional
- Selection strategies fully implemented (WeightedRandom, Closest, HighestBond, LeastRecent placeholder)
- SpawnConditionsEvaluator checks all three dimensions (PlayerState, WorldState, EntityState)
- SceneInstantiator creates scene instances from templates, resolves placement
- SpawnFacade orchestrates automatic spawning via CheckAndSpawnEligibleScenes method
- Automatic triggers implemented for: NPC interaction, Scene completion, Location entry

**Tutorial Content Pattern:**
The "Secure Lodging" tutorial demonstrates the complete pattern: multi-step scene flow, NPC interaction, resource costs, navigation, time passage, recovery. This hand-authored template establishes the mechanical vocabulary. Procedural system must replicate this pattern contextually.

**Existing Catalogues:**
SituationArchetypeCatalog generates choice templates from archetype IDs during parsing. Called exclusively at parse time, never at runtime. Applies tier scaling formulas. Outputs strongly-typed choice templates. The infrastructure exists but only handful of archetypes defined.

---

## Architectural Correctness - Time Trigger (VERIFIED CORRECT)

### Time Trigger Implementation Status: ✓ FIXED

**Current Correct State:**
ProcessTimeAdvancement method in GameFacade is the HIGHLANDER single synchronization point for ALL time-based side effects. The Time spawn trigger is NOW correctly placed inside this method at line 1224.

**Verification:**
- Line 1224: `_spawnFacade.CheckAndSpawnEligibleScenes(SpawnTriggerType.Time, contextId: null);` is INSIDE ProcessTimeAdvancement
- Lines 995-998: ProcessSecureRoomIntent has explicit NOTE that spawn trigger is handled automatically
- Time-based procedural scenes now spawn after ALL time advancements:
  - Wait action (calls ProcessTimeAdvancement → Time trigger fires)
  - Rest action (calls ProcessTimeAdvancement → Time trigger fires)
  - Work action (calls ProcessTimeAdvancement → Time trigger fires)
  - Travel action (calls ProcessTimeAdvancement → Time trigger fires)
  - SecureRoom action (calls ProcessTimeAdvancement → Time trigger fires)
  - Delivery completion (calls ProcessTimeAdvancement → Time trigger fires)
  - LocationAction execution (calls ProcessTimeAdvancement → Time trigger fires)
  - NPCAction execution (calls ProcessTimeAdvancement → Time trigger fires)
  - PathCard execution (calls ProcessTimeAdvancement → Time trigger fires)

**Why This Is Correct:**
ProcessTimeAdvancement is called after EVERY time advancement throughout the codebase. It handles hunger increases, day transitions, emergency checks, scene expiration, and now automatic time-based spawning. All time-based side effects flow through this single sync point, maintaining HIGHLANDER principle.

**Impact:**
Procedural content with temporal spawn conditions (appears at evening, spawns after day 3, triggers on morning) will spawn consistently after ANY action that advances time. The time-based spawning system works correctly and architecturally.

**Status:**
This architectural violation has been FIXED. No further action required. The system now correctly implements the HIGHLANDER pattern for time-based side effects.

---

## Implementation Gaps Overview

Five major phases remain unimplemented:

1. **Archetype Library Expansion** - Define comprehensive interaction pattern catalog
2. **Template Bifurcation** - Separate tutorial (concrete) from procedural (categorical) templates
3. **Placement Resolution Enhancement** - Complete categorical filter support
4. **Condition Evaluation Orchestration** - Trigger spawn checks at all appropriate moments
5. **AI Integration Points** - Generate narrative text from entity contexts

These phases build on fully functional infrastructure. The spawning system works, placement resolution works, condition evaluation works. What's missing is content (archetypes), process (template creation workflow), and enhancement (AI narrative generation).

---

## Gap 1: Archetype Library Expansion

### What's Missing

The SituationArchetypeCatalog exists and functions correctly but contains only a handful of archetypes. The procedural generation vision requires a comprehensive library of interaction patterns covering all common RPG scenarios.

### Why It Matters

Without diverse archetypes, procedural templates have nothing to reference. Content creators cannot author procedural templates because the mechanical patterns don't exist. The system infrastructure is ready but has no patterns to instantiate.

### What Needs Building

**Identify Recurring Interaction Patterns:**
- Negotiations (challenge vs payment vs fallback)
- Transactions (buy, sell, barter)
- Gatekeepers (permission requests with requirements)
- Information exchanges (query, bribe, persuade)
- Crises (urgent time-limited decisions)
- Discoveries (exploration rewards, lore reveals)
- Conflicts (confrontation with escalation paths)
- Bargains (favors with future obligations)
- Investigations (evidence gathering sequences)

**For Each Pattern, Define:**
- Choice count (2-4 following Sir Brante pattern)
- Action types (which are instant, which start challenges, which navigate)
- Cost formulas (what percentage of player resources, which resource types)
- Requirement formulas (stat thresholds by tier, item needs)
- Reward templates (bond changes, item grants, scale shifts, scene spawns)

**Testing Approach:**
Create hand-authored test scenes using each archetype at different tiers with different player states. Verify costs feel appropriate for intended resource pressure. Ensure requirements create meaningful gates without being punitive. Confirm rewards justify effort investment.

**Documentation Required:**
Each archetype needs semantic documentation explaining intended use case (when does negotiation_with_bypass fit versus pure_negotiation), mechanical trade-offs (challenge path grants bond but costs Focus, payment path skips relationship but costs coins), and narrative flexibility (what narrative contexts support this pattern).

### How It Integrates

Archetypes are static methods in SituationArchetypeCatalog that return Lists of ChoiceTemplate objects. Called exclusively during JSON parsing when SituationTemplate references archetype ID. Output embedded directly in situation template structure. No runtime archetype logic exists - all translated to concrete choice templates at parse time.

The archetype method receives tier level and player progression benchmarks (max resources, expected stat ranges) to apply scaling formulas. Returns mechanical specifications only - no narrative strings.

---

## Gap 2: Template Bifurcation

### What's Missing

Current templates are all hand-authored tutorial content with concrete entity IDs. The tutorial "Secure Lodging" template explicitly targets Elena by ID, spawns at game start unconditionally, has hardcoded costs. No procedural templates exist that use categorical filters and formula-driven costs.

### Why It Matters

Hand-authored templates don't scale. Every NPC needs custom templates, every location needs unique content. The procedural system infrastructure is ready but unused because no procedural templates exist to instantiate.

### What Needs Building

**Separate Tutorial Templates:**
Keep existing tutorial templates as-is with concrete IDs for consistent new player experience. These establish mechanical vocabulary - players learn patterns through handcrafted examples.

**Create Procedural Versions:**
Take tutorial patterns and extract the mechanical structure into procedural templates. The "Secure Lodging" tutorial becomes template "Merchant Request at Tier 1" with placement filter for Merchant/Innkeeper personalities at Urban locations with bond 0-2. Same mechanical flow, applicable to dozens of potential NPCs.

**Template Properties:**
- Archetype ID reference (which mechanical pattern)
- Tier specification (difficulty/resource scaling)
- Placement filter (categorical entity requirements)
- Spawn conditions (temporal/progression gates)
- Narrative hints (tone, theme, context for AI)

**Content Migration:**
This is authoring work, not architectural change. The spawning system already supports both concrete and categorical placement. Templates just need to be written exploiting categorical filters and archetype references instead of hardcoded entity IDs and manual choice definitions.

### How It Integrates

SceneInstantiator already handles both types during instantiation. Concrete templates provide specific entity IDs, SceneInstantiator uses those directly. Categorical templates provide PlacementFilter, SceneInstantiator queries GameWorld entities matching filter criteria, applies selection strategy, resolves to concrete entity. Same instantiation logic accommodates both approaches.

---

## Gap 3: Placement Resolution Enhancement

### What's Missing

PlacementFilterEvaluator handles NPC personality types and location properties correctly. Route filtering for terrain types and danger ratings was commented out with incorrect assumption that routes lack necessary properties (they actually have them via hex system integration).

Placement resolution returns first match when multiple entities qualify. When five merchants match a filter, game always picks whichever appears first in GameWorld list. Selection strategies (Closest, HighestBond, LeastRecent) are implemented but LeastRecent is placeholder falling back to WeightedRandom because interaction history tracking doesn't exist yet.

### Why It Matters

Categorical filtering is the core of procedural generation. Templates say "any Merchant at Urban location" and the system must find appropriate NPCs. Commented-out route filtering prevents route-based procedural content. First-match selection creates predictability and prevents content variety.

### What Needs Building

**Uncomment Route Filtering:**
Routes have the required properties through hex system. The filtering logic exists but is disabled. Enable it and verify routes match terrain type and danger rating filters correctly.

**Implement Interaction History Tracking:**
Player needs LocationVisits dictionary tracking visit counts per location. Player needs NPCInteractions list tracking interaction timestamps per NPC. Player needs RouteTravels tracking travel counts per route.

These enable LeastRecent selection strategy to work correctly, providing variety by selecting entities player hasn't interacted with recently. Also enables spawn conditions checking minimum visit/interaction counts.

**Template-Specified Strategy:**
Some scenes want closest match (urgent crisis spawns near player), some want highest bond (trust-dependent requests target close relationships), some want random (environmental variety). Templates should declare preferred selection strategy as property.

### How It Integrates

PlacementFilterEvaluator.FindMatchingNPC and FindMatchingLocation already collect ALL matching candidates using Where().ToList(). They call ApplySelectionStrategyNPC/Location which switch on SelectionStrategy enum. The infrastructure exists completely - just need history tracking populated and LeastRecent implementation completed.

---

## Gap 4: Condition Evaluation Orchestration

### What's Missing

SpawnConditionsEvaluator service exists and checks all three condition dimensions correctly. SpawnFacade.CheckAndSpawnEligibleScenes method queries templates, evaluates conditions, instantiates eligible scenes. But this orchestration is only called at three trigger points currently: NPC interaction, Scene completion, Location entry. The Time trigger has the architectural violation described above.

Beyond fixing Time trigger, there's no background evaluation loop. Scenes become eligible mid-gameplay (bond crosses threshold, item acquired, specific day reached) but nothing checks until next major trigger point. Content appears with latency.

### Why It Matters

Temporal spawn conditions create dynamic narrative emergence. Scene requiring "player has 50+ coins and morning timeblock" should appear promptly when conditions met, not wait until next location entry. Delayed spawning creates disconnected narrative flow.

### What Needs Building

**Fix Time Trigger Placement:**
Move SpawnFacade.CheckAndSpawnEligibleScenes call INTO ProcessTimeAdvancement method. Remove from ProcessSecureRoomIntent. This makes Time trigger fire after ALL time advancements consistently.

**Spawn Queue System (Optional):**
When condition evaluation finds eligible scene mid-action (during challenge, mid-conversation), don't spawn immediately (interrupts flow). Queue for appropriate moment (next scene query at that placement). Existing duplicate prevention at SpawnFacade.cs:353-357 prevents duplicate spawns - check if scene already active.

**Multiple Trigger Integration:**
Scenes can have multiple spawn condition dimensions. Scene might require "morning timeblock AND bond >= 5 AND completed prior scene". Time trigger handles temporal aspect, NPC trigger handles bond aspect, Scene trigger handles progression aspect. CombinationLogic (AND vs OR) determines if all or any conditions must be met.

### How It Integrates

ProcessTimeAdvancement is HIGHLANDER for time effects. Called after Wait, Rest, Work, Travel, SecureRoom, LocationAction execution, NPCAction execution, PathCard execution. Adding spawn check here ensures consistent temporal evaluation across ALL gameplay time advancement.

GameFacade orchestrates cross-facade operations. Facades never call each other. GameFacade controls TimeFacade for time progression, then calls SpawnFacade for spawn checks. Clean separation maintains facade boundaries.

---

## Gap 5: AI Integration Points

### What's Missing

Templates have narrative hint properties (tone, theme, context) but nothing consumes these hints. The architecture anticipates AI generation through entity reference binding at scene finalization, but no bridge to AI exists. All templates have zero narrative text currently.

### Why It Matters

Procedural mechanical patterns without narrative texture feel robotic. Templates define complete interaction flows but describe nothing. Players see mechanical structures without contextual flavor. AI-generated narrative transforms mechanical skeletons into living moments.

### What Needs Building

**Context Bundling Service:**
At scene finalization (player commits to action spawning scene), collect:
- NPC entity object with all properties (Personality, Background, BondLevel, InteractionHistory)
- Location entity object with all properties (AtmosphericDescription, CurrentTime, Weather, Properties)
- Player entity with relevant state (PriorChoicesWithNPC, CurrentNeeds, CharacterBackground)
- Situation archetype information (pattern type, tier, mechanical structure)
- Template narrative hints (tone, theme, context)

Bundle into AI-generation-ready structure providing maximum context for appropriate narrative.

**AI Generation Trigger:**
SceneInstantiator.FinalizeScene already binds concrete entity references. This is the natural point to call AI generation. Scene transitions from provisional to active, concrete context available, generation justified (player committed, scene will actually play).

**Prompt Construction:**
Convert entity property bundles and narrative hints into appropriate AI prompt format. Prompt describes entity properties, player relationship history, current situation archetype, desired tone/theme. Requests situation description and choice action texts.

**Result Parsing:**
Extract generated narrative text from AI responses. Store directly in Situation.Description property and Choice.ActionText properties. No separate storage, no reference indirection. Direct property assignment after generation.

**Fallback Strategy:**
AI generation can fail (timeout, service unavailable, rate limit). Have reasonable defaults - generic situation descriptions from archetype names, basic choice texts from action types. AI is enhancement, not dependency.

### How It Integrates

SceneInstantiator already creates scene and situation instances. Already resolves placement to concrete NPC/Location entity objects. Already has template reference with narrative hints. The bundling logic extends existing instantiation process.

Finalization is checkpoint where player commitment occurs. Provisional scenes might be deleted without playing. Finalization ensures generation serves actual gameplay. Latency consideration is real - player waits for AI response. Requires loading states or async generation strategies at UI layer.

Templates remain immutable pure mechanical specifications. Generated narrative stored in mutable instance properties after generation completes. Clean separation between template definition and runtime narrative population.

---

## Implementation Strategy

### Phase Sequencing

**Phase 0 (ARCHITECTURAL FOUNDATION): ✓ COMPLETE**
Time trigger architectural violation has been FIXED. The spawn check is correctly placed inside ProcessTimeAdvancement method. Time-based spawn conditions now trigger correctly after all time advancements (Wait/Rest/Work/Travel/SecureRoom/Delivery/LocationAction/NPCAction/PathCard).

**Phase 1 (ARCHETYPE LIBRARY):**
Expand archetype library in SituationArchetypeCatalog. Define 15-20 core interaction patterns. Test each archetype in isolation with hand-authored test scenes at different tiers. Document semantic usage for each pattern.

**Phase 2 (CONTENT):**
Create procedural template versions of tutorial patterns. Extract mechanical structure from "Secure Lodging" into "Merchant Request" template with categorical filters. Author 10-15 procedural templates covering common scenarios.

**Phase 3 (VARIETY):**
Implement interaction history tracking on Player (LocationVisits, NPCInteractions, RouteTravels). Complete LeastRecent selection strategy using history. Uncomment route terrain filtering. Test variety in template instantiation.

**Phase 4 (OPTIONAL ENHANCEMENT):**
Implement spawn queue system if mid-action spawning causes problems. Test with condition changes during challenge execution. Verify queue defers spawning to appropriate moments.

**Phase 5 (NARRATIVE LAYER):**
Design AI context bundling structure. Implement generation trigger at finalization. Create prompt construction and result parsing. Test with subset of templates. Add fallback defaults. Expand to full template library.

### Testing Approach

**Archetype Testing:**
Each archetype gets isolated test scenes at tiers 0-3 with varying player resources (low/medium/high). Verify cost formulas produce intended resource pressure. Confirm requirement gates function at expected thresholds. Check reward values justify effort.

**Template Testing:**
Spawn procedural templates in test environment with different NPC personalities, different locations, different player states. Verify placement resolution finds appropriate entities. Confirm spawn conditions trigger at right moments. Check costs scale correctly by tier.

**Integration Testing:**
Play through complete scenarios using only procedurally-spawned content. Verify narrative flow feels coherent. Check mechanical balance remains consistent. Confirm content variety prevents repetition. Test edge cases (no matching NPCs, all conditions fail, etc.).

### Success Criteria

**Architectural Compliance:**
- Time trigger fires after ALL time advancement consistently
- No string matching at runtime (all strongly-typed)
- Templates remain immutable after parsing
- HIGHLANDER principle maintained throughout
- Catalogues called only at parse time, never at runtime

**Functional Completeness:**
- 15+ archetypes covering common interaction patterns
- 10+ procedural templates demonstrating variety
- All selection strategies functional with history tracking
- AI generation produces contextual narrative from entity properties
- Spawn triggers fire at all appropriate moments

**Quality Validation:**
- Procedural content mechanically indistinguishable from hand-authored
- Narrative coherence maintained across AI-generated text
- No repetition across multiple template instantiations
- Resource costs and requirements feel appropriate at all tiers
- Player progression gates content access logically

---

## Critical Constraints

### Catalogue Parse-Time Restriction

Under no circumstances can runtime code import or call catalogues. Situation archetype catalog generates choice templates during JSON parsing exclusively. After parse phase completes, archetype ID becomes metadata annotation for content tools, completely ignored by game logic.

This constraint ensures fail-fast behavior. Invalid archetype values crash at load with clear error, never during gameplay. Runtime bugs from bad archetypes are impossible because runtime never executes archetype logic.

### Strong Typing Throughout

No Dictionary with string keys, no var keyword, no object parameter types, no HashSet collections. SpawnConditions are strongly-typed nested records. PlacementFilters have typed lists of enums. Cost templates have decimal properties for specific resources.

Compiler catches errors at compile time. IDE provides autocomplete. Refactoring propagates safely. Watch windows show typed objects. No "object at index 2 of Dictionary entry is somehow null" debugging.

### Template Immutability

Templates loaded from JSON never change after parse phase. Properties are init-only. Collections initialized once, never modified. Template references passed throughout codebase trust template stability.

Enables safe reference sharing. Multiple threads query same template without synchronization. Debugging is deterministic - template-driven behavior depends only on template content, not runtime mutations.

### HIGHLANDER Enforcement

Every concept exists in exactly one authoritative location. Situations live in GameWorld flat list, scenes store IDs. Placement lives on scene, situations query parent. ProcessTimeAdvancement is THE ONLY place for time-based side effects.

Duplication introduces desynchronization risk. If both scene and situation store placement, one can be updated without the other. HIGHLANDER prevents these failure modes architecturally.

### AI Boundary Clarity

AI receives entity context (NPC/Location/Player objects) but never mechanical values (costs, requirements, rewards, formulas). AI prompts describe entity properties but omit game systems.

AI returns pure narrative text - situation descriptions, choice action texts, flavor. No mechanical decisions. AI cannot accidentally create unbalanced content by inventing costs. Mechanical design stays under developer control.

Boundary improves AI generation quality. Language models excel at creative text from rich context. Constraining AI to narrative domain plays to strength. Full entity objects provide maximum context for appropriate generation.

---

## Key Files & Their Roles

### C:\Git\Wayfarer\src\Services\GameFacade.cs

Pure orchestrator for UI-backend communication. Coordinates cross-facade operations. Contains ProcessTimeAdvancement method at line 1176 - the HIGHLANDER single sync point for ALL time-based side effects.

Time trigger is correctly placed at line 1224 INSIDE ProcessTimeAdvancement method. Location trigger at line 357, NPC trigger at line 1119. All automatic spawn orchestration correctly integrated.

### C:\Git\Wayfarer\src\Subsystems\Spawn\SpawnFacade.cs

Executes spawn rules and orchestrates automatic scene spawning. CheckAndSpawnEligibleScenes method at line 336 queries procedural templates, evaluates spawn conditions via SpawnConditionsEvaluator, instantiates eligible scenes via SceneInstantiator.

Contains SpawnTriggerType enum defining four trigger types: Time, Location, NPC, Scene. Called by GameFacade at appropriate orchestration points. Never calls other facades directly - maintains clean separation.

### C:\Git\Wayfarer\src\Services\SpawnConditionsEvaluator.cs

Evaluates SpawnConditions against current game state. Three evaluation dimensions: PlayerState (progression, history), WorldState (temporal, environmental), EntityState (relationships, reputation).

EvaluateAll method at line 25 checks conditions using strongly-typed properties. Returns boolean eligibility. Used by SpawnFacade to filter templates before instantiation.

### C:\Git\Wayfarer\src\Content\SceneInstantiator.cs

Factory service creating scene and situation instances from templates. Resolves placement from categorical filters to concrete entities at line 379 (FindMatchingNPC) and line 422 (FindMatchingLocation).

Selection strategy application at lines 624-745 implements Closest, HighestBond, WeightedRandom, LeastRecent. Entity reference binding for AI context happens during instantiation. FinalizeScene method is natural point for AI generation trigger.

### C:\Git\Wayfarer\src\Content\Catalogues\SituationArchetypeCatalog.cs

Translates archetype IDs into concrete choice templates during parse phase exclusively. Called when SituationTemplate references archetype ID. Receives tier level and player progression benchmarks.

Applies formula logic for costs (percentage of resources), requirements (stat thresholds by tier), rewards (bond changes, item grants). Returns List of ChoiceTemplate objects. This is where archetype library expansion happens - add new pattern methods here.

### C:\Git\Wayfarer\src\GameState\SceneTemplate.cs & SituationTemplate.cs

Immutable template definitions for scenes and situations. SceneTemplate declares placement filters, spawn conditions, situation flow rules, AI narrative hints. SituationTemplate specifies archetype ID, tier level, override parameters.

Templates contain zero narrative text. Pure mechanical specifications. Hand-authored for tutorial, procedurally defined post-tutorial. Template bifurcation creates procedural versions here referencing archetypes and categorical filters.

### C:\Git\Wayfarer\src\GameState\PlacementFilter.cs

Categorical filter for selecting placement entities procedurally. Properties for NPC personality types, location properties, route terrain types, player state requirements. SelectionStrategy property determines how multiple matches resolved.

Used by SceneTemplate to specify categorical requirements. Evaluated at spawn time by PlacementFilterEvaluator to resolve concrete entities. No concrete entity IDs in procedural templates - only categorical properties here.

### C:\Git\Wayfarer\src\GameState\SpawnConditions.cs

Strongly-typed nested record defining eligibility conditions. PlayerStateConditions (progression, history), WorldStateConditions (temporal, environmental), EntityStateConditions (relationships, reputation). CombinationLogic enum (AND vs OR) combines dimension results.

Contains AlwaysEligible sentinel value for unconditional spawning (DDD pattern - explicit domain concept, not implicit null check). Evaluated by SpawnConditionsEvaluator at spawn time.

### C:\Git\Wayfarer\src\Services\SituationCompletionHandler.cs

Handles situation completion lifecycle. Contains Scene completion trigger at line 83 - calls SpawnFacade.CheckAndSpawnEligibleScenes when scene state becomes Completed. Enables cascade spawning (completing one scene spawns follow-up scenes).

Scene owns its lifecycle via AdvanceToNextSituation method. Handler checks scene state and triggers automatic spawning orchestration appropriately.

---

## Next Session Context

This document provides complete architectural understanding for implementing remaining procedural generation features. The infrastructure is fully functional - spawning works, placement resolves, conditions evaluate, selection strategies apply.

**Architectural Foundation: ✓ COMPLETE**
The Time trigger has been correctly placed inside ProcessTimeAdvancement method (line 1224). All automatic spawn orchestration is properly integrated. There are NO architectural violations blocking procedural generation.

**What Remains:**
Content (archetype patterns), process (template authoring workflow), enhancement (AI narrative generation). These are incomplete features, not broken architecture.

**Implementation Path:**
Proceed through phases sequentially starting with Phase 1: archetypes provide patterns, templates apply patterns procedurally, history tracking enables variety, AI adds narrative texture. Each phase builds on fully functional infrastructure.

The procedural generation vision is clear: mechanical patterns (archetypes) instantiate contextually (categorical filters) with narrative texture (AI generation) creating scalable content maintaining handcrafted quality. The architecture supports this completely. Implementation just needs completing.
