# HANDOFF: Scene Archetype vs Situation Archetype - Design vs Configuration

## Problem & Motivation

Runtime error encountered: `Unknown scene archetype ID: 'single_situation'`. This error revealed a fundamental misunderstanding in the relationship between scene archetypes, situation archetypes, and scene instances.

Content authors were attempting to create single-situation scenes (one-off merchant negotiations, guard checkpoints, etc.) but had no valid way to express this in the JSON authoring model. The error exposed a gap in the architecture: multi-situation archetypes existed (4-situation service arc, 3-situation transaction arc) but single-situation patterns had no archetype representation.

This matters because procedural content generation depends on clear separation between what is DESIGNED (reusable patterns defined in code) and what is CONFIGURED (where/when patterns appear, defined in JSON). Blurring this distinction leads to configuration creep into design space and violates the HIGHLANDER principle (one concept, one representation).

## Architectural Discovery

### The Two-Level Archetype System

Wayfarer has TWO distinct archetype systems operating at different granularities:

**Level 1: Scene Archetypes** (SceneArchetypeCatalog)
- Purpose: Generate multi-situation STRUCTURES with transitions
- Granularity: 2-4 situations per scene
- Examples: `service_with_location_access` (4 situations: negotiate→access→service→depart), `transaction_sequence` (3 situations: browse→negotiate→complete)
- Responsibility: Defines HOW situations connect and flow

**Level 2: Situation Archetypes** (SituationArchetypeCatalog)
- Purpose: Generate 4-CHOICE PATTERNS for individual situations
- Granularity: 1 situation = 4 choices (stat-gated, money, challenge, fallback)
- Examples: `negotiation`, `confrontation`, `investigation`, `social_maneuvering`, `crisis`
- Responsibility: Defines HOW players interact with a single situation

These systems compose: Scene archetypes call situation archetypes. The scene archetype defines WHICH situation archetypes appear and in what configuration.

### The Parser-JSON-Entity Triangle

Scene creation follows a parse-time generation pattern:

**JSON** (Data Layer): SceneTemplateDTO contains archetype ID and placement filters
**Parser** (Translation Layer): SceneTemplateParser calls catalogues at parse time only
**Catalogue** (Generation Layer): SceneArchetypeCatalog generates complete SituationTemplate arrays
**Entity** (Domain Layer): SceneTemplate stores generated templates, never calls catalogues at runtime

Critical timing: ALL catalogue calls happen at parse time (game initialization). Runtime NEVER calls catalogues. Templates are immutable after parsing.

### Three Distinct Concepts

The architecture distinguishes three layers:

**1. SceneTemplate (Pre-Authored Design)**
- Immutable archetype defining structure
- Specifies: How many situations, which situation types, transition pattern, dependent resources
- Authored ONCE in catalogue code
- Reused across MANY scene instances

**2. SituationTemplate (Pre-Authored Design)**
- Immutable archetype defining choice pattern
- Specifies: 4-choice structure (stat/money/challenge/fallback), costs, requirements
- Generated FROM situation archetypes
- Embedded within SceneTemplate

**3. Scene Instances (Runtime Configuration)**
- JSON entries specifying WHERE/WHEN a scene template appears
- Uses PlacementFilter: Which NPCs (personality types), which locations (properties), which conditions (player state)
- Uses SpawnConditions: Time windows, state requirements, achievement gates
- Does NOT define structure - only placement and eligibility

## Domain Model Understanding

### Scene Archetype as Complete Identity

A scene archetype is the complete definition of a scene's structure and content:

- **Structure**: How many situations, in what order
- **Content**: Which situation archetypes for each situation
- **Flow**: Transition pattern (Linear, Standalone, Branching, etc.)
- **Resources**: What dependent locations/items it creates (if any)

Example: `service_with_location_access` archetype DEFINES:
- Exactly 4 situations
- Situation 1 uses `social_maneuvering` OR `service_transaction` archetype (based on NPC personality)
- Situation 2 is auto-progress (no archetype)
- Situation 3 is auto-progress (no archetype)
- Situation 4 is auto-progress (no archetype)
- Creates dependent location (private room) and item (room key)
- Linear transitions (1→2→3→4)

The archetype ID encodes the entire pattern. No parameters needed.

### PlacementFilter as Pure Configuration

PlacementFilter determines WHERE a scene template spawns, not WHAT it contains:

- **NPC Filters**: Personality types, bond ranges, tags
- **Location Filters**: Property types (commercial, indoor, etc.), districts, regions
- **Route Filters**: Terrain types, danger ratings, tier levels
- **Player State Filters**: Required/forbidden states, achievements, scale positions

PlacementFilter never affects scene structure. A negotiation scene has the same 4-choice pattern whether it spawns at a Mercantile NPC or Devoted NPC - only the narrative hints and stat requirements might adjust based on context.

### SpawnConditions as Temporal Gates

SpawnConditions determine WHEN a scene becomes eligible, not what it does:

- **Player State**: Completed scenes, choice history, stats, items
- **World State**: Day ranges, time blocks, weather conditions
- **Entity State**: NPC bonds, location reputation, route travel counts

SpawnConditions are pure eligibility checks. They gate access without affecting structure.

## Current State Analysis

### Existing Multi-Situation Archetypes

The catalogue already contains five multi-situation archetypes. Each defines its complete structure internally:

**service_with_location_access**: 4 situations, generates dependent room and key, NPC-personality-aware choice generation
**transaction_sequence**: 3 situations, pure economic exchange pattern
**gatekeeper_sequence**: 2 situations, authority confrontation pattern
**consequence_reflection**: 1 situation, standalone crisis response
**inn_crisis_escalation**: 4 situations, escalating conflict with ally support

Pattern observed: Each archetype INTERNALLY decides which situation archetypes to use for each of its situations. No parameterization. The archetype name IS the definition.

### The "consequence_reflection" Precedent

`consequence_reflection` already exists as a SINGLE-SITUATION scene archetype. This proves the concept: single-situation scenes ARE scene archetypes, not a special case requiring different handling.

The error in existing code: `consequence_reflection` uses `crisis` situation archetype internally. This establishes the pattern that should apply to all single-situation scenes - the scene archetype name encodes which situation archetype it uses.

### JSON Authoring Pattern Discovered

Examining `22_procedural_scenes.json` revealed content authors were trying to express:

"I want a standalone negotiation scene at Mercantile NPCs"
"I want a standalone confrontation scene at guards"
"I want a standalone investigation scene at information brokers"

They attempted `"sceneArchetypeId": "single_situation"` with `"serviceType": "negotiation"` - semantically dishonest misuse of serviceType field. This reveals user need: they want different single-situation patterns but had no valid archetype names.

## Design Approach & Rationale

### The HIGHLANDER Principle Applied

HIGHLANDER: One concept, one representation.

**Question**: Is "which situation archetype to use" part of a scene's IDENTITY (design) or a CONFIGURATION choice?

**Answer**: IDENTITY. A negotiation scene and a confrontation scene are fundamentally different scene types:

- **Negotiation**: Diplomatic, trade-based, non-violent, rapport-focused
- **Confrontation**: Authoritative, dominance-based, potentially violent, power-focused
- **Investigation**: Analytical, discovery-based, puzzle-solving, insight-focused

These aren't configuration variants of a generic "single-situation scene." They are distinct scene archetypes with different thematic identities, appropriate rewards, and contextual usage.

### Why Parameterization is Wrong

**Rejected Approach**: Single "standalone" archetype taking `situationArchetypeId` as parameter

This violates design/configuration separation. If JSON authors specify which situation archetype, they're making DESIGN decisions in CONFIGURATION space. This leads to:

- Configuration creep: Design choices leak into JSON
- Catalogue bypass: Parser directly calls SituationArchetypeCatalog, bypassing SceneArchetypeCatalog
- Loss of scene identity: "Standalone" is meaningless - it's just "one situation of unspecified type"
- Impossibility of archetype-specific logic: Catalogue can't generate archetype-aware dependent resources or rewards

### Why Explicit Scene Archetypes are Correct

**Accepted Approach**: Six distinct single-situation scene archetypes

- `single_negotiation`
- `single_confrontation`
- `single_investigation`
- `single_social_maneuvering`
- `single_crisis`
- `single_service_transaction`

Each is a complete scene archetype that DEFINES which situation archetype it uses. JSON authors select the scene archetype name, not its internal structure.

This preserves design/configuration separation:

- **Design** (Catalogue): Defines that `single_negotiation` generates 1 situation using `negotiation` archetype
- **Configuration** (JSON): Specifies that `single_negotiation` scenes spawn at Mercantile NPCs in Commercial locations

Analogy to existing archetypes: `service_with_location_access` doesn't take parameters saying which 4 situation archetypes to use - it DEFINES them internally. Single-situation archetypes follow the same pattern.

### Verisimilitude and Elegance

**Verisimilitude**: Scene archetype names should meaningfully describe what the scene IS, not just its structure. "single_negotiation" clearly communicates intent. "standalone with parameter negotiation" is mechanical noise.

**Elegance**: Minimal interconnection. SceneArchetypeCatalog generates everything. Parser doesn't need special PATH B logic. One generation path for all scenes.

## Implementation Strategy

###  Current Violation to Remove

Parser currently has three-path routing:
- Path A: Multi-situation scenes use `sceneArchetypeId`
- Path B: Single-situation scenes use `situationArchetypeId`
- Path C: Hand-authored scenes (forbidden)

Path B is the violation. It treats situation archetype selection as configuration and bypasses the catalogue pattern.

### Correct Single-Path Architecture

Parser should have ONE path: All scenes use `sceneArchetypeId`. No special cases.

### Catalogue Expansion Pattern

SceneArchetypeCatalog needs six new entries in its switch statement. Each calls a shared helper method with a different situation archetype.

The helper method follows the existing `GenerateConsequenceReflection` pattern:
- Creates ONE SituationTemplate from the provided situation archetype
- Generates 4 choices using SituationArchetypeCatalog
- Creates SpawnRules with Standalone pattern and no transitions
- Returns SceneArchetypeDefinition with single-element situation list

This establishes a reusable pattern: any situation archetype can have a corresponding single-situation scene archetype.

### JSON Migration Pattern

Existing JSON using the violated pattern needs updating:

**Before (WRONG)**:
```
"archetype": "Standalone",
"sceneArchetypeId": "single_situation",
"serviceType": "negotiation"
```

**After (CORRECT)**:
```
"archetype": "Standalone",
"sceneArchetypeId": "single_negotiation"
```

Remove `serviceType` field entirely for standalone scenes - it's semantic dishonesty (serviceType is for service_with_location_access archetype only).

### DTO Simplification

SceneTemplateDTO should have ONLY `sceneArchetypeId`. Remove `situationArchetypeId` property entirely. This enforces HIGHLANDER: one field for scene archetype identification, no alternatives.

## Critical Constraints

### Parse-Time Only Catalogue Calls

Catalogues must NEVER be imported outside parser classes. They are parse-time code only. Runtime operates on pre-generated templates. This timing separation is sacred - breaking it introduces performance problems and architectural confusion.

### HIGHLANDER Enforcement

No redundant properties. No alternative paths. One way to identify scene archetypes: the `sceneArchetypeId` field. No exceptions.

### Immutable Templates

SceneTemplate and SituationTemplate are immutable after parsing. Runtime never modifies them. They're shared across all instances of that template. This enables efficient reuse without defensive copying.

### Design vs Configuration Separation

Design (structure, content, patterns) lives in catalogue code. Configuration (placement, eligibility, timing) lives in JSON data. Never blur this line. If JSON authors need to make design decisions, the catalogue is incomplete.

### Semantic Honesty

Property names must match reality. `serviceType` is for service archetypes only. Using it for standalone scenes is lying through the data model.

## Key Files & Their Roles

### SceneArchetypeCatalog.cs

Authoritative source for all scene archetype definitions. Contains the Generate() switch statement routing archetype IDs to generator methods. Each generator returns SceneArchetypeDefinition containing SituationTemplates, SpawnRules, and optional dependent resources.

Role in fix: Add six new case entries for single-situation archetypes. Add shared helper method GenerateSingleSituationScene() that wraps situation archetype generation in scene archetype structure.

### SceneTemplateDTO.cs

Data transfer object mapping JSON structure to domain entities. Defines what properties JSON authors can specify. This is the contract between content authoring and code.

Role in fix: Remove `SituationArchetypeId` property. Ensure only `SceneArchetypeId` exists for archetype specification. Update comments to clarify that ALL scenes use scene archetypes, including single-situation ones.

### SceneTemplateParser.cs

Translation layer converting DTOs to domain entities at parse time. Calls catalogues to generate templates. Validates required fields and enforces architectural constraints.

Role in fix: Remove three-path routing logic. Restore single-path: all scenes call SceneArchetypeCatalog.Generate() with sceneArchetypeId. No special handling for Standalone archetype. Parser should be agnostic to archetype details - catalogue handles all variation.

### 22_procedural_scenes.json

Content data file containing scene instance configurations. Each entry specifies a scene template to instantiate with placement filters and spawn conditions.

Role in fix: Update six scene entries replacing violated pattern with correct archetype names. Remove `serviceType` fields from standalone scenes. Verify all use `sceneArchetypeId` with valid archetype names from catalogue.

### SituationArchetypeCatalog.cs

Authoritative source for situation archetype definitions. Contains GetArchetype() method returning archetypes with 4-choice pattern specifications (stat requirements, coin costs, challenge types).

Role in fix: No changes needed. This catalogue is called BY SceneArchetypeCatalog when generating situations. The architecture preserves the two-level composition pattern.

## Next Session Priorities

1. **Verify understanding**: Does the distinction between design (scene archetype) and configuration (placement filter) make sense? Can you explain why `single_negotiation` and `single_confrontation` are different scene archetypes, not configuration variants?

2. **Remove violation**: Delete `SituationArchetypeId` property from DTO. Delete PATH B routing from parser. Restore single-path architecture where all scenes use `sceneArchetypeId`.

3. **Expand catalogue**: Add six scene archetype cases to SceneArchetypeCatalog switch. Implement shared helper method following `consequence_reflection` pattern.

4. **Fix JSON**: Update six scene entries in 22_procedural_scenes.json to use new archetype names. Remove semantic dishonesty (serviceType on standalone scenes).

5. **Test holistically**: Build and verify game loads all scenes without errors. Run test suite to ensure no regressions. Verify procedural scenes appear in game with correct structure.

## Architectural Lessons Learned

The two-level archetype system (scene archetypes compose situation archetypes) requires clear understanding of where composition happens. Scene archetypes DEFINE which situation archetypes they use. This is not configuration - it's core identity.

Parameterizing archetype selection feels like flexibility but actually erodes architectural clarity. It moves design decisions from code (where they belong) to data (where they don't). The solution isn't "make the archetype take parameters" - it's "create more specific archetypes."

HIGHLANDER applies at the field level: one field for archetype identification (`sceneArchetypeId`), not two alternative fields for different cases. The presence of `situationArchetypeId` alongside `sceneArchetypeId` signals architecture confusion.

Parse-time generation enables rich procedural content without runtime performance cost. The catalogue can do complex generation (multi-situation arcs, dependent resources, context-aware hints) because it runs ONCE at game startup, not on every scene spawn.

The distinction between pre-authored design artifacts (templates) and runtime configuration (instance parameters) must remain sharp. Templates define WHAT, instances define WHERE and WHEN. This separation enables content reuse and procedural variation within well-defined boundaries.
