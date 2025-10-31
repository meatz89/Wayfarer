# Scene-Situation System Design

## Core Philosophy

**Scenes orchestrate dynamic narrative emergence through spatial situation spawning.**

The Scene-Situation spawning system generates story beats by spawning narrative contexts (Situations) at locations, NPCs, and routes. Unlike obstacles (persistent challenges), Scene orchestrators are ephemeral - they spawn multiple Situations in various configurations (sequential, parallel, branching), then discard themselves.

---

## ⚠️ PRIME DIRECTIVE: PLAYABILITY OVER IMPLEMENTATION ⚠️

**THE FUNDAMENTAL RULE: A game that compiles but is unplayable is WORSE than a game that crashes.**

Before implementing ANY Scene/Situation content:

### Mandatory Playability Validation

1. **Can the player REACH the Scene placement from game start?**
   - If Scene spawns at Location: Can player navigate there via routes?
   - If Scene spawns at NPC: Is NPC at accessible location?
   - If Scene spawns on Route: Can player initiate that route travel?
   - Trace COMPLETE path from starting location

2. **Are Situations VISIBLE and INTERACTIVE in UI?**
   - Situation appears when player enters location/conversation/route
   - Situation presents 2-4 choices as cards/buttons
   - Each choice shows costs, requirements, and consequences
   - Player can select and execute choices

3. **Do Scenes spawn and cascade correctly?**
   - isStarter Scenes spawn at game initialization
   - Completing choices spawns follow-up Scenes as defined
   - Scene spawn rewards create Scenes at valid placements
   - Cascading Situations appear when prerequisites met

### Fail-Fast Enforcement

**❌ FORBIDDEN - Silent defaults that hide unplayable Scenes:**
```csharp
// WRONG - Scene has no Situations, player sees empty screen
if (scene.Situations != null && scene.Situations.Any()) { DisplaySituations(scene.Situations); }

// WRONG - Situation has no choices, player cannot interact
var choices = situation.ChoiceTemplates ?? new List<ChoiceTemplate>();
```

**✅ REQUIRED - Throw exceptions for missing critical content:**
```csharp
// CORRECT - Validates Scene has Situations
if (!scene.Situations.Any())
    throw new InvalidOperationException($"Scene '{scene.Id}' has no Situations - player cannot interact!");

// CORRECT - Validates Situation has 2-4 choices (Sir Brante pattern)
if (situation.ChoiceTemplates.Count < 2 || situation.ChoiceTemplates.Count > 4)
    throw new InvalidDataException($"Situation '{situation.Id}' has {situation.ChoiceTemplates.Count} choices - must have 2-4 (Sir Brante pattern)!");

// CORRECT - Validates Scene placement exists
if (scene.PlacementType == PlacementType.Location)
{
    Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == scene.PlacementId);
    if (location == null)
        throw new InvalidOperationException($"Scene '{scene.Id}' spawns at unknown location '{scene.PlacementId}' - player cannot reach!");
}
```

### The Playability Test for Scenes

For EVERY Scene implemented:

1. **Spawn validation** → Does Scene spawn at accessible placement?
2. **UI visibility** → Does Situation render in location/conversation/route UI?
3. **Choice display** → Do all 2-4 choices appear as clickable options?
4. **Execution** → Does selecting choice execute and apply consequences?
5. **Cascade** → Do follow-up Scenes/Situations spawn correctly?

**If ANY step fails, Scene content is INACCESSIBLE.**

---

## Strategic Layer Hierarchy

**THREE LEVELS OF ABSTRACTION:**

### Level 1: Scene (Ephemeral Orchestrator)
- Spawns from SceneTemplate
- Creates multiple Situations at various placements
- Defines configuration (sequential, parallel, branching, conditional)
- Discards itself after spawning Situations
- NOT stored in GameWorld (ephemeral)

### Level 2: Situation (Persistent Narrative Context)
- Spawns from SituationTemplate within a Scene
- Contains 2-4 action references (by ID)
- Appears at one Location/NPC/Route
- Persists until player completes one of its actions
- Stored in GameWorld.Situations

### Level 3: Actions (Player Choices)
- Existing entities: LocationAction, ConversationOption, TravelCard
- NOT created by Scenes (already exist in GameWorld)
- Situations reference them by ID
- Player selects ONE action from Situation's 2-4 options

---

## Spawn Orchestration Patterns

### Pattern Categories

**1. Linear Progression**
- Scene spawns Situation A
- After A completes, Scene spawns Situation B
- After B completes, Scene spawns Situation C
- Scene concludes when chain reaches end

**2. Hub and Spoke**
- Scene spawns central Situation + 3 parallel Situations
- All available simultaneously
- Scene concludes when all completed (or central completed, depending on rules)

**3. Branching Consequences**
- Scene spawns initial Situation
- Success spawns Situations D, E, F
- Failure spawns Situations G, H, I
- Different narrative paths based on outcome

**4. Discovery Chain**
- Scene spawns Situation at known Location A
- Completing reveals Location B (previously hidden)
- Scene spawns follow-up Situation at Location B
- Progression through spatial discovery

**5. Conditional Multi-Spawn**
- Scene evaluates player state (stats, items, relationships)
- Spawns different Situation combinations based on conditions
- Creates tailored narrative experiences

**6. Timed Cascade**
- Scene spawns Situation with time limit
- Completing before deadline: spawns one set of Situations
- Completing after deadline: spawns degraded set
- Missing entirely: spawns consequence Situations

**7. Converging Paths**
- Scene spawns multiple independent Situations
- When ALL completed, Scene spawns finale Situation
- Requires player to pursue all threads

**8. Mutually Exclusive Paths**
- Scene spawns two Situations simultaneously
- Completing one removes/blocks the other
- Forces permanent choice

**See**: `situation-spawn-patterns.md` for detailed pattern documentation

---

### 2. Situation Appearance

**Player Experience:**
```
Player navigates to Location
→ UI queries GameWorld.Situations for this Location
→ Finds Situation(s) with PlacementType=Location, PlacementId=currentLocationId
→ Displays Situation narrative text
→ Shows 2-4 actions as player choices (cards)
```

**Important**: Actions themselves are NOT Situation-specific. They're existing entities that Situation references. Same action might appear in multiple Situations.

---

### 3. Player Action Selection

**When player selects action from Situation:**

**Instant Action (cost/reward only):**
```
1. GameFacade.ExecuteLocationAction(actionId)
2. Validate requirements (stats, items, etc.)
3. Apply costs (resources, time)
4. Apply rewards (stats, items, discoveries)
5. Mark Situation as Completed
6. Remove Situation from GameWorld.Situations
7. Trigger parent Scene completion check
```

**Challenge Action (starts Social/Mental/Physical):**
```
1. GameFacade.StartChallenge(actionId, challengeType)
2. Load challenge configuration (deck, goal cards, etc.)
3. Enter challenge subsystem (tactical layer)
4. Player plays challenge
5. On challenge completion:
   - Apply challenge rewards
   - Mark Situation as Completed
   - Remove Situation from GameWorld.Situations
   - Trigger parent Scene completion check
```

**Navigation Action (TravelCard):**
```
1. GameFacade.ExecuteTravelCard(cardId)
2. Validate route exists
3. Apply travel costs (time, resources)
4. Move player to destination Location
5. Mark Situation as Completed (if travel was the Situation action)
6. Trigger parent Scene completion check
```

---

### 4. Scene Completion

**Completion Conditions:**
- **AllComplete**: All spawned Situations completed
- **AnyComplete**: Any one Situation completed
- **TimeExpired**: Segment/day limit reached
- **PlayerChoice**: Specific Situation marked as "Scene conclusion"

**On Scene Completion:**
```
1. SceneFacade evaluates completion condition
2. Determines outcome (success/failure/neutral based on which Situations completed)
3. Evaluates followUpSpawns rules:
   - Success path → Spawn new Scene(s) / Situation(s)
   - Failure path → Spawn different Scene(s) / Situation(s)
4. Spawn follow-ups (recursive Scene spawning)
5. Discard Scene (ephemeral, not stored)
```

---

### 5. Situation Expiration

**Time Limit:**
- Situation has optional `TimeLimit` (segments)
- TimeFacade tracks active Situations with limits
- On limit reached: Mark Situation as Expired
- May trigger consequence spawns (Scene defines expired outcome)

**Blocking:**
- Situation A may block Situation B (mutually exclusive)
- Completing A removes B from GameWorld.Situations
- Player sees B disappear from UI

**Prerequisite Failure:**
- Situation requirements may become impossible (NPC dies, location destroyed)
- Mark Situation as Failed
- Remove from GameWorld.Situations
- May trigger alternative spawns

---

## Sir Brante Pattern in Wayfarer Context

**Sir Brante Structure:**
- Player sees Situation narrative
- 2-4 choices presented
- Each choice has:
  - Requirements (visible)
  - Costs (visible)
  - Outcomes (hidden until selected)
- Some choices instant, some start challenges
- Choice locks in outcome, progresses story

**Wayfarer Implementation:**
- **Situation** = Sir Brante narrative moment
- **Actions** = Sir Brante choices (2-4 options)
- Player navigates to Location/NPC/Route
- Sees Situation narrative
- Selects one of 2-4 Actions
- Action executes (instant or challenge)
- Situation completes, story progresses

**Key Difference:**
- Sir Brante: Linear progression through scripted situations
- Wayfarer: Spatial navigation, dynamic situation spawning, emergent narrative

---

## Design Constraints

### Sir Brante Pattern Requirements
1. **2-4 Choices**: Every Situation must offer 2-4 actions (no more, no less)
2. **Narrative Context**: Situation provides story/context, actions are responses
3. **Mixed Types**: Actions can be instant (cost/reward) or challenge-starting
4. **Visible Requirements**: All action requirements shown to player before selection
5. **Hidden Outcomes**: Exact rewards/consequences hidden until action selected
6. **Progression**: Selecting action completes Situation, advances story

### Spatial Navigation
1. **Placement Persistence**: Situation persists at location until completed
2. **Player Discovery**: Player navigates to placement to discover Situation
3. **Multiple Situations**: One placement can host multiple Situations simultaneously
4. **Priority Display**: Higher priority Situations shown more prominently

### Strategic Layer Separation
1. **No Tactical Mechanics**: Situations do NOT contain SituationCards (those are challenge victory conditions)
2. **Action References Only**: Situations reference existing actions by ID, don't create them
3. **Scene Orchestration**: Scenes spawn Situations, Situations don't spawn Situations directly
4. **Ephemeral Scenes**: Scenes discard after spawning, not stored in GameWorld

### Categorical Design
1. **Template Filters**: Use categorical properties, not concrete IDs
2. **Parse-Time Translation**: All categorical → concrete at instantiation
3. **Dynamic Scaling**: Requirements scale based on player progression
4. **AI-Friendly**: Templates describable without knowing exact game state

---

## Summary

**Scenes** are ephemeral orchestrators that spawn **Situations** in various configurations. **Situations** are persistent narrative contexts appearing at locations/NPCs/routes, offering players 2-4 **action** choices. Actions (LocationAction/ConversationOption/TravelCard) are existing entities that Situations reference, NOT inline definitions.

This architecture enables:
- **Emergent Narrative**: Dynamic story beats generated from templates
- **Spatial Discovery**: Player navigates world to find spawned Situations
- **Strategic Choice**: Player selects among visible action options with clear costs/requirements
- **Reusability**: Templates instantiate with different entities, creating variety
- **AI Generation**: Categorical templates enable procedural content creation

The Scene-Situation system is the **STRATEGIC LAYER** of Wayfarer's narrative engine. It sits above the tactical challenge layer (Social/Mental/Physical with SituationCards) and provides the framework for dynamic quest/story generation across the game world.
