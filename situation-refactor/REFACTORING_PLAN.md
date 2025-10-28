# Scene-Situation Refactoring Plan

## Current State Analysis

### Existing Infrastructure

**Spawn System (src/Subsystems/Spawn/, src/GameState/, src/Content/):**
- SpawnRule.cs - Domain model for spawn rules
- SpawnRuleDTO.cs - DTO for JSON deserialization
- SpawnRuleParser.cs - Converts DTO to domain
- SpawnFacade.cs - Executes spawn logic

**Current Concept (WRONG):**
- Situations spawn other Situations
- Uses Situation entities as templates (clones them)
- Stores spawned Situations in GameWorld.Situations
- References via NPC.ActiveSituationIds / Location.ActiveSituationIds

**What's Right:**
- Placement strategy concept (SameAsParent, SpecificLocation, SpecificNPC)
- Requirement offset concept
- Spawn conditions concept
- Validation and filtering logic structure

---

## Required Changes

### Phase 1: Delete Old System ✅
- Deleted SceneFacade_1.cs
- Deleted SceneDTO.cs
- Deleted SceneParser.cs
- Deleted ScenePropertyReductionDTO.cs
- Deleted SceneRewardService.cs
- Deleted ScenePropertyReduction.cs
- Deleted SceneIntensityCalculator.cs
- Deleted SceneSituationFilter.cs

### Phase 2: Create Correct Entities

**Scene.cs** (NEW - replaces old ephemeral Scene):
- Persistent entity stored in GameWorld.Scenes
- Contains embedded List<Situation> (not IDs)
- Contains SituationSpawnRules for cascade
- PlacementType and PlacementId
- CurrentSituationId tracking
- State (Active, Completed, Expired, **Provisional**)
- **Template property** (reference to SceneTemplate)

**Situation.cs** (MODIFY existing):
- Remove from GameWorld.Situations (now embedded in Scene)
- Keep narrative, choices structure
- Add List<Choice> (2-4 choices - Sir Brante pattern)

**Choice.cs** (NEW):
- Action text
- CompoundRequirement
- Resource cost
- ChoiceReward (including SceneSpawnReward list)
- ActionType (Instant, StartChallenge, Navigate)
- **ProvisionalSceneId** (string, nullable - references provisional Scene)

**ChoiceReward.cs** (NEW):
- Resource rewards (coins, resolve, bond changes)
- State/achievement grants
- **List<SceneSpawnReward>** - key feature

**SceneSpawnReward.cs** (NEW):
- SceneTemplateId
- PlacementRelation (SameLocation, SameNPC, Specific)
- SpecificPlacementId (if Specific)
- DelayDays

**SituationSpawnRules.cs** (NEW):
- SpawnPattern enum
- Initial situation ID
- List<SituationTransition>
- CompletionCondition

### Phase 3: Create Template System

**SceneTemplate.cs** (NEW):
- Template ID
- Archetype
- PlacementFilter (categorical requirements)
- List<SituationTemplate>
- SituationSpawnRules structure
- Starter flag

**SceneTemplateDTO.cs** (NEW):
- JSON deserialization structure

**SceneTemplateParser.cs** (NEW):
- Parse DTO to domain
- Validate structure

**PlacementFilter.cs** (NEW):
- PlacementType
- Personality type filters (NPC)
- Location tag filters
- Bond thresholds
- Player state requirements

**GameWorld Additions:**
- Add **SceneState.Provisional** enum value
- Add **Choice.ProvisionalSceneId** property
- Add **GameWorld.ProvisionalScenes** dictionary (string sceneId → Scene)
- Add **Scene.Template** property

**Purpose:**
- Provisional Scenes enable perfect information before Choice execution
- Player sees WHERE Scene spawns before committing to Choice
- UI can display Scene type, archetype, placement in Choice card preview
- Strategic decision-making: "This choice spawns Social Scene at Inn with Innkeeper"

### Phase 4: Create SceneInstantiator

**NEW: SceneInstantiator.cs** (src/Subsystems/Scene/):

**Method: CreateProvisionalScene(templateId, placementRelation, context)**
- Gets SceneTemplate from GameWorld.SceneTemplates
- Selects best entity from PlacementFilter (Location, NPC, or Route)
- Creates Scene with concrete placement (PlacementType + PlacementId)
- Instantiates SituationTemplates into embedded Situations (WITH placeholder text)
- Sets Scene.State = **Provisional**
- Sets Scene.Template = SceneTemplate reference
- Returns Scene object
- **Does NOT finalize placeholders yet**

**Method: FinalizeScene(provisionalScene)**
- Replaces placeholders in all Situations: {NPCName} → actual name, {LocationName} → actual name
- Generates intro narrative based on archetype + placement
- Sets Scene.State = **Active**
- Moves Scene from GameWorld.ProvisionalScenes → GameWorld.Scenes
- Returns finalized Scene

**Method: DeleteProvisionalScene(sceneId)**
- Removes Scene from GameWorld.ProvisionalScenes
- Cleans up any Choice references
- NO deletion from GameWorld.Scenes (not there yet)

**Why Provisional Scenes:**
- Player sees Scene placement BEFORE executing Choice
- UI displays "This choice spawns Social Scene at [Inn] with [Innkeeper]"
- Perfect information enables strategic planning
- Multiple Choices can preview different Scenes simultaneously

### Phase 5: Wire to Situation Instantiation

**MODIFY: SceneInstantiator.cs (continued from Phase 4)**

When creating Situation from SituationTemplate:
- For each ChoiceTemplate with SceneSpawnReward list:
  - Call **CreateProvisionalScene** for each SceneSpawnReward
  - Set **Choice.ProvisionalSceneId** on the instantiated Choice
  - Store provisional Scene in **GameWorld.ProvisionalScenes**

**Flow:**
```
SceneInstantiator creates Scene from template
  → Instantiates Situations from SituationTemplates
    → For each Situation's ChoiceTemplate:
      → If ChoiceTemplate.SceneSpawnRewards exists:
        → For each SceneSpawnReward:
          → CreateProvisionalScene(reward.TemplateId, reward.PlacementRelation, context)
          → Set Choice.ProvisionalSceneId = provisionalScene.Id
          → Store in GameWorld.ProvisionalScenes
```

**Result:**
- When player views Situation, Choices already have provisional Scenes created
- UI can fetch provisional Scene by Choice.ProvisionalSceneId
- Display placement info to player BEFORE they commit to Choice

### Phase 6: Wire to Choice Execution

**NEW: ChoiceExecutionService.cs** (or extend SceneFacade)

**Method: ExecuteChoice(sceneId, situationId, choiceId)**
1. Get Choice from Situation
2. Validate requirements and costs
3. Apply resource costs
4. **If Choice.ProvisionalSceneId exists:**
   - Get provisional Scene from GameWorld.ProvisionalScenes
   - Call **SceneInstantiator.FinalizeScene(provisionalScene)**
   - Scene moves to GameWorld.Scenes (State = Active)
5. **For ALL OTHER Choices in same Situation:**
   - If Choice.ProvisionalSceneId exists:
     - Call **SceneInstantiator.DeleteProvisionalScene(otherChoice.ProvisionalSceneId)**
     - Cleanup unused provisional Scenes
6. Apply other ChoiceRewards (resources, states, etc.)
7. Advance Scene to next Situation or complete

**Why This Approach:**
- Selected Choice's Scene becomes real (finalized)
- Unselected Choices' Scenes deleted (never existed in player's timeline)
- No wasted memory (only active Scenes in GameWorld.Scenes)
- Perfect information maintained (player saw options before choosing)

### Phase 7: Update SceneFacade

**SceneFacade.cs** (REWRITE):
- GetActiveScenesAtLocation(locationId)
- GetActiveScenesForNPC(npcId)
- GetActiveScenesOnRoute(routeId)
- GetProvisionalScenesByIds(List<string> sceneIds) - for UI preview
- AdvanceScene(sceneId) - follow spawn rules, trigger next Situation

### Phase 8: Remove Old References

**NPC.cs:**
- Remove SceneIds property
- Keep ActiveSituationIds for backward compat (but mark deprecated)

**RouteOption.cs:**
- Remove SceneIds property

**Obligation.cs:**
- Remove ScenesSpawned
- Add SpawnedSceneIds (simple tracking)

**Location.cs:**
- Remove SceneIds if exists
- Keep ActiveSituationIds for backward compat

### Phase 9: PackageLoader Integration

**PackageLoader.cs:**
- Add LoadSceneTemplates method
- Store in GameWorld.SceneTemplates

**GameWorld.cs:**
- Add SceneTemplates dictionary
- Keep Scenes list
- Add SpawnInitialScenes method

### Phase 10: UI Integration

**LocationContent.razor:**
- Query GameWorld.Scenes instead of GameWorld.Situations
- Render current Situation from Scene
- Show Choices as cards
- Execute via SceneFacade.ExecuteChoice

**TravelManager.cs:**
- Query GameWorld.Scenes for route obstacles
- Replace equipment-intensity logic with Scene-Situation

---

## Implementation Order

1. ✅ Delete old equipment Scene system
2. Create new entity models (Scene, Choice, ChoiceReward, SceneState.Provisional, Choice.ProvisionalSceneId)
3. Create template system (SceneTemplate, SituationTemplate, ChoiceTemplate, PlacementFilter)
4. Create SceneInstantiator with provisional Scene logic (CreateProvisionalScene, FinalizeScene, DeleteProvisionalScene)
5. Wire provisional Scene creation to Situation instantiation (when Situation created, create provisional Scenes for Choices)
6. Wire Choice execution to Scene finalization (finalize selected, delete unselected)
7. Update SceneFacade with query methods (GetActiveScenesAtLocation, GetProvisionalScenesByIds)
8. Update PackageLoader and GameWorld (SceneTemplates, ProvisionalScenes dictionary)
9. Remove old entity references (NPC.SceneIds, etc.)
10. Update UI components (display provisional Scene info in Choice cards)
11. Test end-to-end (verify perfect information, Scene lifecycle, cleanup)

---

## Key Architectural Principles

- Scenes are PERSISTENT (GameWorld.Scenes)
- Situations are EMBEDDED (Scene.Situations, not separate list)
- Templates use FILTERS (not concrete IDs)
- Choices spawn SCENES (via rewards)
- Sir Brante: 2-4 Choices per Situation
- Spawn rules create CASCADES within Scene
- **PROVISIONAL SCENES enable perfect information:**
  - Created EAGERLY when Situation instantiated
  - Player sees placement BEFORE selecting Choice
  - Finalized ONLY if Choice executed
  - Deleted if Choice NOT selected
  - UI displays "This spawns Social Scene at [Inn] with [Innkeeper]"

---

## Next Steps

Continue with Phase 2: Create correct entity models starting with Scene.cs, Situation modifications, and Choice.cs.
