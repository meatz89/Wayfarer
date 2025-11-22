# Entity Resolution Boundaries - Complete Architecture

## The Question

Where exactly are entity object references resolved and stored in the Scene/Situation/Choice instantiation pipeline with procedural generation from categorical properties? Where does resolution happen ONCE per HIGHLANDER principle?

## The Answer: Three-Boundary Architecture

```
JSON (categorical) → PARSE → Templates (categorical) → SPAWN → Scenes (objects) → QUERY → Actions (objects)
                    TIME                              TIME                      TIME
```

### BOUNDARY 1: PARSE-TIME (JSON → Templates)

**Input:** JSON text with categorical properties
**Process:** Parser creates immutable template catalog
**Output:** SceneTemplate, SituationTemplate with categorical filters
**Storage:** GameWorld.SceneTemplates (immutable library)
**Resolution:** NONE - templates remain categorical forever

Templates remain categorical throughout their lifecycle. The template entity structure contains an Id property (permitted for templates as immutable archetypes), a PlacementFilter property holding categorical requirements, and a collection of embedded situation templates. The placement filter structure itself contains categorical enum properties for location type, quality tier, and venue type - all describing categories and types rather than specific entity instances.

**Key Insight:** Templates are archetypes. They describe WHAT KIND of entities are needed, not WHICH SPECIFIC entities.

### BOUNDARY 2: SPAWN-TIME (Templates → Scene Instances) ⭐ RESOLUTION HAPPENS HERE

**Input:** SceneTemplate with categorical filters
**Process:** SceneInstantiator converts categorical → objects via EntityResolver
**Output:** Scene with resolved entity object references
**Storage:** GameWorld.Scenes with embedded Situations
**Resolution:** YES - THIS IS THE SINGLE RESOLUTION POINT

The spawn process performs resolution in a specific sequence. First, the instantiator calls EntityResolver to find or create the placement location by passing GameWorld's location collection, the template's placement filter, and the game world reference. The resolver returns a resolved location object which gets stored once in the scene's PlacementLocation property. A new scene instance is constructed containing a template reference, the resolved placement location object, and an empty situations collection. Then for each situation template, the instantiator conditionally resolves entities - if the situation has a character filter, it calls EntityResolver to find or create that character and stores the returned object reference; if the situation has a location filter, it similarly resolves and stores that location object reference. Each situation instance is created with its template reference, the resolved character object (if any), the resolved location object (if any), and a parent scene reference, then added to the scene's situations collection. Finally, the fully-constructed scene with all embedded situations and resolved entity references is added to GameWorld and returned.

**Critical Properties:**

The scene entity structure contains a Template property referencing the immutable template, a PlacementLocation property storing the resolved location object (not a filter, not an ID string), an optional PlacementNPC property storing a resolved character object if character-placed, and a Situations collection containing embedded situation instances each with their own resolved entity references. Each situation entity contains a Template property referencing its template, a RequiredLocation property storing a resolved location object, a RequiredNPC property storing a resolved character object, and a ParentScene property referencing back to the containing scene.

**EntityResolver.FindOrCreate Logic:**

The EntityResolver implements a two-phase find-or-create pattern. In the FIND phase, it queries the existing locations collection for entities matching the filter's categorical properties - checking location type equality, quality threshold satisfaction, and venue type matching. If a matching entity exists, the resolver immediately returns that existing object reference. In the CREATE phase when no match is found, the resolver generates a new location entity by calling a name generator with the filter, copying categorical properties from the filter (location type, quality), resolving or creating the parent venue via recursive find-or-create with the venue type, and calculating hex coordinates based on placement constraints. The newly created location is immediately added to GameWorld's locations collection, then the resolver returns the new object reference.

**Key Insight:** Resolution happens ONCE at spawn-time. If entity exists, return it. If not, create it. Either way, Scene stores object reference. Never resolves again.

### BOUNDARY 3: QUERY-TIME (Scene → Actions)

**Input:** Scene with resolved entity object references
**Process:** SceneFacade queries active Situation, instantiates Actions
**Output:** LocationAction/NPCAction with entity references
**Storage:** GameWorld.LocationActions/NPCActions (ephemeral, recreated on load)
**Resolution:** NONE - inherits Scene's resolved objects

The facade's action retrieval process operates on already-resolved entities without performing any resolution itself. The method accesses the scene's current situation object reference, creates an empty action collection, then iterates through the current situation's template's choice templates. For each choice template, it constructs an action instance by copying object references: the SourceSituation receives the current situation object, the SourceLocation receives the scene's already-resolved placement location object, and the TargetLocation receives a location object resolved during spawn (obtained through a helper method). All other action properties are populated from template data. Each constructed action is added to the collection, and the complete collection of actions (all containing inherited object references from the scene) is returned.

**Key Insight:** Actions are ephemeral UI representations. They store object references copied from Scene. No resolution happens. Actions deleted when Situation completes.

## Multi-Scene Entity Sharing

**Question:** What if two Scenes need the same Location?

**Answer:** Both resolve to SAME object instance.

When the first scene spawns with a specific location filter, EntityResolver finds or creates a matching location and stores that object reference in the scene's PlacementLocation property. When a second scene later spawns with an identical filter (matching categorical properties), EntityResolver queries existing locations, finds the previously created location (categorical properties align), and returns the same object instance. Both scenes now store references to the identical location object in memory. Object equality comparison confirms both scenes reference the same instance - changes to the location's state are visible to both scenes because they share the same object reference.

Both Scenes point to same Location object. Changes to Location visible to both. This is correct and efficient.

## Entity State Changes

**Question:** If NPC moves locations, how do Scenes track this?

**Answer:** Scenes store NPC object reference. System queries NPC's current state.

When a scene is created, it stores an object reference to the required character entity at spawn-time. Later during gameplay, if that character moves to a different location (the character's CurrentLocation property is updated to reference a different location object), the scene continues holding its original object reference to that same character entity. When the player attempts a scene action requiring character interaction, the system evaluates by comparing the player's current location with the character's current location property (accessed through the scene's character object reference). The scene doesn't duplicate the character's state - it maintains a reference to the living entity whose properties can change, and the system always queries the current property values through that reference.

Scene stores object. NPC's properties can change. Scene always has current reference to query latest state.

## Dependent Resource Creation

**Question:** What if Scene needs Location that doesn't exist yet (private room)?

**Answer:** EntityResolver creates it at spawn-time with contextual properties.

When a scene requires a location that may not exist yet (such as a private sublocation within a venue), the situation template's JSON contains a location filter specifying categorical properties like location type, quality tier, and a venue relation indicating it should be within the same venue as the scene's placement. At spawn-time, EntityResolver performs contextual resolution by querying GameWorld's locations collection pre-filtered to only locations matching the scene's placement venue (constraining the search to locations within the same venue context), then applying the template's location filter. If no matching location exists, the resolver creates a new location entity with a generated name, the categorical location type from the filter, the quality tier from the filter, the venue inherited from the scene's placement location (maintaining contextual coherence), and hex coordinates calculated relative to the scene's location. The newly created location is immediately added to GameWorld, and the situation stores the object reference to this created location in its RequiredLocation property.

Created Location immediately added to GameWorld. Future Scenes can find it. No special tracking needed.

## The HIGHLANDER Guarantee

**Single Resolution Point:** Spawn-time boundary (Template → Scene)
**Single Storage:** Scene/Situation properties store resolved object references
**Single Access Path:** All downstream code uses Scene's objects directly

**Violations:**
- ❌ Storing PlacementId string, looking up later
- ❌ Re-resolving filters at query-time
- ❌ Actions storing entity names instead of objects
- ❌ Services receiving string IDs, looking up entities

**Correct:**
- ✅ Scene.PlacementLocation stores Location object at spawn
- ✅ Situation.RequiredNPC stores NPC object at spawn
- ✅ Actions inherit object references from Scene
- ✅ Services receive object parameters directly

## Summary Diagram

```
JSON LAYER
┌─────────────────────────────────────┐
│ Categorical Properties Only         │
│ { "locationType": "CommonRoom",    │
│   "quality": "Standard" }          │
└─────────────────────────────────────┘
                 │
                 │ PARSE-TIME
                 ▼
TEMPLATE LAYER (Immutable Catalog)
┌─────────────────────────────────────┐
│ PlacementFilter (categorical)       │
│ NpcFilter (categorical)            │
│ NO object references               │
└─────────────────────────────────────┘
                 │
                 │ SPAWN-TIME ⭐ RESOLUTION HERE
                 ▼
SCENE LAYER (Mutable Instances)
┌─────────────────────────────────────┐
│ PlacementLocation: Location ◄────── │ Object stored
│ RequiredNPC: NPC ◄───────────────── │ Object stored
│ RequiredLocation: Location ◄─────── │ Object stored
└─────────────────────────────────────┘
                 │
                 │ QUERY-TIME (no resolution)
                 ▼
ACTION LAYER (Ephemeral UI)
┌─────────────────────────────────────┐
│ SourceLocation: Location ◄────────  │ Inherited
│ TargetNPC: NPC ◄────────────────────│ Inherited
│ (copied from Scene)                 │
└─────────────────────────────────────┘
```

**The answer:** Entity resolution happens EXACTLY ONCE at spawn-time boundary (Template → Scene), resolved objects stored in Scene/Situation properties, all downstream code uses those object references directly, never re-resolving.