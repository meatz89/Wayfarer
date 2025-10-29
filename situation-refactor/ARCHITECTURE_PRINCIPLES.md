# Scene-Situation Architecture Principles

## Overview

This document describes the high-level architecture for the Scene-Situation system in Wayfarer. This system enables dynamic content spawning, perfect information gameplay, and AI-generated post-tutorial content.

**Key Insight**: Scenes are spawned from templates and contain Situations. When the player enters a location/NPC/route context, Situations activate and present Choices. These Choices ARE the runtime actions (LocationAction/NPCAction/PathCard) - there is no separate Choice entity.

---

## The Three-Tier Timing Model

The architecture operates across three distinct timing phases:

### Tier 1: Parse Time (Template Creation)
- **Who**: ContentParsers (SceneParser, SituationParser, etc.)
- **When**: Game initialization from JSON
- **What**: Create immutable templates (SceneTemplate, SituationTemplate, ChoiceTemplate)
- **Output**: Templates stored in GameWorld catalogues

### Tier 2: Instantiation Time (Scene/Situation Creation)
- **Who**: SceneInstantiator
- **When**: Scene spawned as reward from action execution
- **What**: Create Scene from SceneTemplate with concrete placement
- **Output**: Scene with embedded Situations in DORMANT state

### Tier 3: Query Time (Action Creation)
- **Who**: SceneFacade
- **When**: Player enters location/NPC/route view
- **What**: Instantiate ChoiceTemplates → LocationAction/NPCAction/PathCard
- **Output**: Ephemeral actions stored in GameWorld flat collections

**CRITICAL**: Actions are NOT created at Tier 2. They are created lazily at Tier 3 when player enters context.

---

## Choice = Action Architecture

**There is NO separate runtime Choice entity.**

The three action types serve dual purposes:

1. **LocationAction** - Choice at location AND execution target
2. **NPCAction** - Choice with NPC AND execution target
3. **PathCard** - Choice on route AND execution target

**How It Works**:

```
ChoiceTemplate (immutable template)
    ↓ (Tier 3: Player enters location/NPC/route)
LocationAction/NPCAction/PathCard (ephemeral instance)
    ↓ (Player selects action)
GameFacade.ExecuteLocationAction/NPCAction/PathCard
```

**Key Properties**:
- `action.ChoiceTemplate` - Read-only reference to template (requirements/costs/rewards)
- `action.SituationId` - Source Situation for cleanup after execution
- `action.ProvisionalSceneId` - Preview of spawned Scene (perfect information)

**Lifecycle**:
1. Player enters context → SceneFacade creates actions from Situation's ChoiceTemplates
2. Player sees actions as choices (UI displays them)
3. Player selects action → GameFacade executes action
4. GameFacade deletes ALL actions for that Situation
5. Next time player enters context → Actions recreated fresh

**Why Ephemeral**: Actions are query-time instances, not persisted state. Same ChoiceTemplate can instantiate different actions each time (placeholder replacement, dynamic context).

---

## Provisional Scene Pattern (Perfect Information)

When a Choice spawns a new Scene as a reward, the player sees WHERE and WHAT that Scene will be BEFORE selecting the Choice.

**How It Works**:

1. **Tier 3 (Query Time)**: SceneFacade creates action from ChoiceTemplate
2. **If ChoiceTemplate has SceneSpawnReward**: SceneInstantiator creates Scene with `State = Provisional`
3. **Provisional Scene is "mechanical skeleton"**:
   - Fully formed (concrete placement, narrative text, embedded Situations)
   - All placeholders replaced
   - NOT yet active (doesn't affect world state)
4. **Action stores ProvisionalSceneId** (LocationAction/NPCAction) or **SceneId** (PathCard)
5. **Player sees preview**: UI shows "This action will spawn [Scene Name] at [Location]"
6. **Player selects action**:
   - Selected action: `SceneInstantiator.FinalizeScene()` → State = Active
   - Other actions: `SceneInstantiator.DeleteProvisionalScene()` → Deleted
7. **Only ONE Scene becomes Active**, all others discarded

**Why This Matters**:
- **Perfect Information**: Player makes strategic decisions with full knowledge
- **No Hidden Surprises**: Player knows consequences before committing
- **Dynamic Content Ready**: AI can generate Scenes, player sees them before selecting

**Storage**:
- Provisional Scenes: `GameWorld.ProvisionalScenes` (Dictionary, temporary)
- Active Scenes: `GameWorld.Scenes` (List, permanent)

---

## Scene-Situation Composition

Scenes and Situations follow composition-over-duplication architecture:

### SceneTemplate → Scene (Instance)
- **Template** = Immutable blueprint (created at Tier 1)
- **Instance** = Runtime entity with concrete placement (created at Tier 2)
- **Composition**: `Scene.Template` reference (NOT copied properties)
- **Instance adds**: PlacementType, PlacementId, State, embedded Situations

### SituationTemplate → Situation (Instance)
- **Template** = Immutable blueprint with ChoiceTemplates (created at Tier 1)
- **Instance** = Runtime entity within Scene (created at Tier 2)
- **Composition**: `Situation.Template` reference (NOT copied properties)
- **Instance adds**: State (Dormant/Active), parent Scene reference

### ChoiceTemplate → LocationAction/NPCAction/PathCard (Instance)
- **Template** = Immutable requirements/costs/rewards (created at Tier 1)
- **Instance** = Ephemeral choice (created at Tier 3)
- **Composition**: `action.ChoiceTemplate` reference (NOT copied properties)
- **Instance adds**: Concrete narrative (placeholders replaced), SituationId, ProvisionalSceneId

**Why Composition**:
- Memory efficiency (no duplication of template data)
- Consistent semantics (template changes propagate)
- Clear separation (template = design, instance = runtime state)

---

## Situation State Machine

Situations have two states controlling action instantiation:

### State: Dormant (Initial)
- **When**: Scene just created (Tier 2)
- **What**: Situation exists but player hasn't entered context
- **Actions**: NONE (not yet created)
- **Transition**: Player enters location/NPC/route → Active

### State: Active (Player Engaged)
- **When**: Player enters location/NPC/route with this Situation
- **What**: SceneFacade instantiates ChoiceTemplates → Actions
- **Actions**: Created and added to GameWorld flat collections
- **Transition**: After action executed → Dormant (actions deleted, ready for re-entry)

**Critical Flow**:
```
Scene spawns
  ↓
Situations created (State = Dormant)
  ↓
Player enters location/NPC/route
  ↓
SceneFacade.GetActionsAtLocation/ForNPC/ForRoute()
  ↓
If Situation.State == Dormant → ActivateSituationForLocation/NPC/Route()
  ↓
Foreach ChoiceTemplate → Create LocationAction/NPCAction/PathCard
  ↓
Create ProvisionalScenes for actions with spawn rewards
  ↓
Situation.State = Active
  ↓
Return actions to UI
```

---

## Dynamic Content Vision

The architecture supports AI-generated content post-tutorial:

### Static Content (Tutorial)
- Hand-authored JSON in Core package
- Parsers read → Templates created → Scenes spawned
- Player experiences curated story

### Dynamic Content (Post-Tutorial)
- AI generates new JSON packages at runtime
- Same Parsers process dynamic JSON
- Same SceneInstantiator spawns Scenes
- Same SceneFacade creates actions
- **Indistinguishable from static content** (architecture-neutral)

**Why It Works**:
- Templates and instances separate (JSON → Template → Instance)
- Placeholders enable context-aware narrative
- Catalogues translate categorical properties → concrete values
- Provisional Scenes preview AI-generated content before commitment

**Example Flow**:
```
Player completes tutorial
  ↓
AI generates "Mysterious Stranger" package JSON
  ↓
Parser loads JSON → Creates SceneTemplate
  ↓
Scene spawns at player's current location
  ↓
Player enters location → Sees "Talk to Mysterious Stranger" action
  ↓
Player selects → GameFacade executes
  ↓
AI-generated Scene spawns new Situations dynamically
```

---

## Atmospheric Existence

Locations, NPCs, and Routes exist independently of Scenes.

### World Without Scenes
- **Locations** have atmospheric properties (name, description, properties)
- **NPCs** have personality, bonds, relationships
- **Routes** have travel time, stamina costs
- **NO scene-related actions** (no LocationActions, NPCActions, PathCards)

### World With Scenes
- **Scenes spawn** at locations/NPCs/routes (placement context)
- **Situations activate** when player enters
- **Actions appear** as choices for player
- **Atmospheric properties still present** (world state unchanged)

**Key Insight**: Scenes are overlays on persistent world. Locations exist before Scenes spawn, during Scene activity, and after Scenes complete.

**Example**:
```
Crossroads location exists (atmospheric description, travel hub property)
  ↓
Player can travel through (no scenes, just navigation)
  ↓
"Suspicious Guard" Scene spawns at Crossroads
  ↓
Player enters Crossroads → Sees "Confront Guard" action (from Scene)
  ↓
Player can ALSO still navigate away (atmospheric properties unaffected)
  ↓
Player completes "Confront Guard" → Scene removed
  ↓
Crossroads returns to atmospheric state (still exists, just no scene actions)
```

---

## Execution Architecture (GameFacade HIGHLANDER)

**HIGHLANDER Principle**: There can be only ONE orchestrator.

### GameFacade Owns ALL Execution
- `ExecuteLocationAction(actionId, situationId)` - Location choices
- `ExecuteNPCAction(actionId, situationId)` - NPC conversation choices
- `ExecutePathCard(cardId, situationId)` - Route travel choices

### No Other Executors
- ❌ LocationFacade does NOT execute actions
- ❌ NPCFacade does NOT execute actions
- ❌ RouteFacade does NOT execute actions
- ❌ SceneFacade does NOT execute actions (only queries)

### Execution Flow
```
UI calls GameFacade.ExecuteLocationAction(actionId, situationId)
  ↓
GameFacade fetches action from GameWorld.LocationActions
  ↓
GameFacade validates requirements (ChoiceTemplate.RequirementFormula)
  ↓
GameFacade applies costs (ChoiceTemplate.CostTemplate)
  ↓
GameFacade applies rewards (ChoiceTemplate.RewardTemplate)
  ↓
If reward includes SceneSpawn → FinalizeScene (Provisional → Active)
  ↓
GameFacade deletes all actions for this Situation (cleanup)
  ↓
Situation.State = Dormant (ready for re-entry)
```

**Why Single Orchestrator**:
- Consistent execution logic across all action types
- Single place to apply requirements/costs/rewards
- Unified provisional Scene finalization
- Clear action cleanup lifecycle

---

## Summary

**Three-Tier Timing**: Parse → Instantiate → Query

**Choice = Action**: No separate entity, actions ARE choices

**Provisional Scenes**: Perfect information preview before selection

**Composition**: Templates referenced, not copied

**State Machine**: Dormant → Active (player entry) → Dormant (after execution)

**Dynamic Content**: AI-generated JSON processed identically to static

**Atmospheric World**: Locations/NPCs/Routes exist without Scenes

**Single Orchestrator**: GameFacade executes ALL actions

---

*This architecture enables strategic depth through perfect information, supports dynamic AI-generated content, and maintains clean separation between templates and instances.*
