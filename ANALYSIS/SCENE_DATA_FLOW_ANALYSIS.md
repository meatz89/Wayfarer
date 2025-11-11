# Complete Scene Template Data Flow: JSON → Parse → Runtime

**Document Type:** Diagnostic Analysis (Point-in-Time)

**Analysis Date:** 2025-01

**Purpose:** Traces scene template data flow from JSON → Parser → Facade → Catalogue, identifies gaps in categorical property implementation.

**Status:** Findings documented in [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) under "Categorical Property Translation" (status: 🚧 IN PROGRESS).

**For Current Architecture:** See [ARCHITECTURE.md](ARCHITECTURE.md) and [PROCEDURAL_CONTENT_GENERATION.md](PROCEDURAL_CONTENT_GENERATION.md) for authoritative documentation.

---

## Overview
This document traces how scene templates flow from JSON through the parser, facade, catalogue, to runtime generation.

---

## 1. JSON AUTHORING LAYER (SceneTemplateDTO)

**Source:** `/home/user/Wayfarer/src/Content/Core/21_tutorial_scenes.json`

```json
{
  "sceneTemplates": [
    {
      "id": "tutorial_secure_lodging",
      "archetype": "Linear",
      "sceneArchetypeId": "tutorial_secure_lodging",  // ← KEY: Tells catalogue which pattern to generate
      "presentationMode": "Modal",
      "tier": 0,
      "isStarter": true,
      "placementFilter": {
        "placementType": "NPC",
        "npcId": "elena"                              // ← Concrete entity binding
      }
    }
  ]
}
```

**Corresponding DTO Class:** `SceneTemplateDTO`
```csharp
public class SceneTemplateDTO
{
    public string Id { get; set; }
    public string Archetype { get; set; }
    public string SceneArchetypeId { get; set; }     // ← PARSED BY PARSER
    public int Tier { get; set; }
    public PlacementFilterDTO PlacementFilter { get; set; }
    public string ServiceType { get; set; }          // ← CURRENTLY UNUSED (line 108)
    // ... other properties
}
```

**Gap Identified:** `SceneTemplateDTO.ServiceType` exists but is never populated in JSON and never read by parser!

---

## 2. PARSING LAYER (SceneTemplateParser)

**File:** `/home/user/Wayfarer/src/Content/Parsers/SceneTemplateParser.cs`

### 2.1 Entry Point: ParseSceneTemplate()

```csharp
public SceneTemplate ParseSceneTemplate(SceneTemplateDTO dto)
{
    // Line 67-68: Read sceneArchetypeId from JSON
    if (string.IsNullOrEmpty(dto.SceneArchetypeId))
        throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required 'sceneArchetypeId'");

    // Line 74: RESOLVE NPC from PlacementFilter
    NPC contextNPC = ResolveNPCFromPlacementFilter(dto.PlacementFilter, dto.Id);
    
    // Line 75: RESOLVE LOCATION from PlacementFilter
    Location contextLocation = ResolveLocationFromPlacementFilter(dto.PlacementFilter, contextNPC, dto.Id);
    
    // Lines 77-78: Extract IDs
    string npcId = contextNPC?.ID;
    string locationId = contextLocation?.Id;

    // Lines 89-93: Call facade with resolved entity IDs
    SceneArchetypeDefinition archetypeDefinition = _generationFacade.GenerateSceneFromArchetype(
        dto.SceneArchetypeId,      // ← "tutorial_secure_lodging"
        dto.Tier,                  // ← 0
        npcId,                     // ← "elena"
        locationId);               // ← "elena_room" (from elena.Location)
    
    // Lines 95-96: Extract generated situations and rules
    List<SituationTemplate> situationTemplates = archetypeDefinition.SituationTemplates;
    SituationSpawnRules spawnRules = archetypeDefinition.SpawnRules;
    
    // Lines 110-127: Create SceneTemplate with generated content
    SceneTemplate template = new SceneTemplate
    {
        Id = dto.Id,
        SituationTemplates = situationTemplates,
        SpawnRules = spawnRules,
        // ... other fields
    };
    
    return template;
}
```

**Critical Calling Sequence:**
1. `ParseSceneTemplate(dto)` is called with SceneTemplateDTO
2. Resolves concrete NPC and Location entities from GameWorld
3. Calls `_generationFacade.GenerateSceneFromArchetype()` with:
   - `dto.SceneArchetypeId` (categorical identifier)
   - `dto.Tier` (numerical tier)
   - `npcId` (resolved entity ID or null)
   - `locationId` (resolved entity ID or null)

**Missing Information:** Parser DOES NOT pass:
- ServiceType (read from JSON but not passed to facade)
- ServiceQuality (not in JSON, not passed)
- SpotComfort (not in JSON, not passed)
- NPCDemeanor (not in JSON, not calculated from NPC state)

---

## 3. FACADE LAYER (SceneGenerationFacade)

**File:** `/home/user/Wayfarer/src/Subsystems/ProceduralContent/SceneGenerationFacade.cs`

```csharp
public SceneArchetypeDefinition GenerateSceneFromArchetype(
    string archetypeId,
    int tier,
    string npcId,        // ← From parser
    string locationId)   // ← From parser
{
    // Lines 49-51: Look up entities from GameWorld (expensive operation!)
    NPC contextNPC = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
    Location contextLocation = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
    Player contextPlayer = _gameWorld.GetPlayer();

    // Line 53: Create GenerationContext from entities
    GenerationContext context = GenerationContext.FromEntities(tier, contextNPC, contextLocation, contextPlayer);

    // Line 55: Call catalogue (pure function)
    SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate(archetypeId, tier, context);

    return definition;
}
```

**What Happens Here:**
1. Facade receives IDs and tier from parser
2. **RE-LOOKS UP** entities from GameWorld (duplication!)
3. Creates GenerationContext via `FromEntities()` factory method
4. Passes context to catalogue

**ARCHITECTURAL ISSUE:** Parser already has entity references but passes only IDs, forcing facade to re-lookup!

---

## 4. GENERATION CONTEXT BUILDER (GenerationContext)

**File:** `/home/user/Wayfarer/src/Content/Generators/GenerationContext.cs`

### 4.1 Property Definitions

```csharp
public class GenerationContext
{
    // Entity context (from parser)
    public int Tier { get; set; }
    public PersonalityType? NpcPersonality { get; set; }
    public string NpcLocationId { get; set; }
    public string NpcId { get; set; }
    public string NpcName { get; set; }
    public int PlayerCoins { get; set; }
    public List<LocationPropertyType> LocationProperties { get; set; } = new();

    // CATEGORICAL PROPERTIES (defaults never changed!)
    public ServiceType ServiceType { get; set; } = ServiceType.Lodging;        // ← HARDCODED
    public ServiceQuality ServiceQuality { get; set; } = ServiceQuality.Standard; // ← HARDCODED
    public SpotComfort SpotComfort { get; set; } = SpotComfort.Standard;       // ← HARDCODED
    public NPCDemeanor NpcDemeanor { get; set; } = NPCDemeanor.Neutral;         // ← HARDCODED
}
```

### 4.2 Factory Method: FromEntities()

```csharp
public static GenerationContext FromEntities(
    int tier,
    NPC npc,
    Location location,
    Player player)
{
    return new GenerationContext
    {
        Tier = tier,
        NpcPersonality = npc?.PersonalityType,          // ← From NPC
        NpcLocationId = npc?.Location?.Id,              // ← From NPC.Location
        NpcId = npc?.ID,                                // ← From NPC
        NpcName = npc?.Name ?? "",                      // ← From NPC
        PlayerCoins = player?.Coins ?? 0,               // ← From Player
        LocationProperties = location?.LocationProperties ?? new()  // ← From Location
        
        // ServiceType, ServiceQuality, SpotComfort, NPCDemeanor 
        // ↓↓↓ NOT SET - USE DEFAULTS ↓↓↓
    };
}
```

**Data Extraction Sources:**
- `Tier` ← SceneTemplateDTO.tier (passed through facade)
- `NpcPersonality` ← NPC.PersonalityType (from GameWorld lookup)
- `NpcLocationId` ← NPC.Location.Id (from GameWorld lookup)
- `NpcId` ← NPC.ID (from GameWorld lookup)
- `NpcName` ← NPC.Name (from GameWorld lookup)
- `PlayerCoins` ← Player.Coins (from GameWorld lookup)
- `LocationProperties` ← Location.LocationProperties (from GameWorld lookup)

**Missing Sources:**
- `ServiceType` ← ??? (not from JSON, not from entity)
- `ServiceQuality` ← ??? (should be derived from Location properties or tier)
- `SpotComfort` ← ??? (should be derived from Location properties or tier)
- `NPCDemeanor` ← ??? (should be derived from NPC.RelationshipFlow or NPC.BondStrength)

---

## 5. CATALOGUE GENERATION (SceneArchetypeCatalog)

**File:** `/home/user/Wayfarer/src/Content/Catalogs/SceneArchetypeCatalog.cs`

### 5.1 Entry Point: Generate()

```csharp
public static SceneArchetypeDefinition Generate(
    string archetypeId,
    int tier,
    GenerationContext context)  // ← Receives context from facade
{
    return archetypeId?.ToLowerInvariant() switch
    {
        "service_with_location_access" => GenerateServiceWithLocationAccess(tier, context),
        "tutorial_secure_lodging" => GenerateTutorialSecureLodging(tier, context),
        "transaction_sequence" => GenerateTransactionSequence(tier, context),
        // ...
    };
}
```

### 5.2 Using Context: GenerateServiceWithLocationAccess()

```csharp
private static SceneArchetypeDefinition GenerateServiceWithLocationAccess(int tier, GenerationContext context)
{
    // Line 120: Read NPC personality from context to determine archetype
    SituationArchetype negotiateArchetype = DetermineNegotiationArchetype(context);
    
    // Line 121: Read context to generate narrative hints
    NarrativeHints negotiateHints = GenerateServiceNegotiationHints(context, serviceId);
    
    // ...
}
```

### 5.3 Helper: DetermineNegotiationArchetype() (Lines 871-884)

```csharp
private static SituationArchetype DetermineNegotiationArchetype(GenerationContext context)
{
    if (context.NpcPersonality == PersonalityType.DEVOTED)
    {
        return SituationArchetypeCatalog.GetArchetype("social_maneuvering");
    }

    if (context.NpcPersonality == PersonalityType.MERCANTILE)
    {
        return SituationArchetypeCatalog.GetArchetype("service_transaction");
    }

    return SituationArchetypeCatalog.GetArchetype("service_transaction");
}
```

**Uses from GenerationContext:**
- `context.NpcPersonality` ✓ (properly set from NPC)
- `context.NpcLocationId` ✓ (properly set)

### 5.4 Helper: GenerateServiceNegotiationHints() (Lines 886-918)

```csharp
private static NarrativeHints GenerateServiceNegotiationHints(GenerationContext context, string serviceId)
{
    // Lines 890-894: Read NpcPersonality
    if (context.NpcPersonality == PersonalityType.DEVOTED)
    {
        hints.Tone = "empathetic";
        hints.Theme = "human_connection";
    }
    else if (context.NpcPersonality == PersonalityType.MERCANTILE)
    {
        hints.Tone = "transactional";
        hints.Theme = "economic_exchange";
    }

    // Lines 908-915: Read PlayerCoins (context-aware narrative)
    if (context.PlayerCoins < 10)
    {
        hints.Style = "desperate";
    }
    else
    {
        hints.Style = "standard";
    }

    return hints;
}
```

**Uses from GenerationContext:**
- `context.NpcPersonality` ✓ (properly set)
- `context.PlayerCoins` ✓ (properly set)
- `context.ServiceType` ✗ (NEVER USED anywhere in catalogue!)
- `context.ServiceQuality` ✗ (NEVER USED anywhere in catalogue!)
- `context.SpotComfort` ✗ (NEVER USED anywhere in catalogue!)
- `context.NPCDemeanor` ✗ (NEVER USED anywhere in catalogue!)

---

## 6. CATALOGUE PROPERTIES THAT SHOULD USE CONTEXT

**Currently Unused in Catalogue Code:**

1. **ServiceType** (enum: Lodging, Bathing, Healing)
   - Should determine which resources are restored
   - Should select situation archetype variations
   - Currently: Hardcoded in catalogue methods

2. **ServiceQuality** (enum: Basic, Standard, Premium, Luxury)
   - Cost multiplier: 0.6x to 2.4x
   - Should scale choice costs dynamically
   - Currently: Not used, costs hardcoded

3. **SpotComfort** (enum: Basic, Standard, Premium)
   - Restoration multiplier: 1x to 3x
   - Should scale rewards dynamically
   - Currently: Not used, rewards hardcoded

4. **NPCDemeanor** (enum: Friendly, Neutral, Hostile)
   - Stat threshold multiplier: 0.6x to 1.4x
   - Should scale requirement thresholds
   - Currently: Not used, requirements hardcoded

---

## 7. COMPLETE CALLING SEQUENCE DIAGRAM

```
PARSE TIME
│
└─→ SceneTemplateParser.ParseSceneTemplate(SceneTemplateDTO)
    │
    ├─ Read: dto.SceneArchetypeId (e.g., "tutorial_secure_lodging")
    ├─ Read: dto.Tier (e.g., 0)
    ├─ Read: dto.PlacementFilter
    │
    ├─ ResolveNPCFromPlacementFilter(filter)
    │  └─ Look up NPC "elena" in GameWorld.NPCs
    │     └─ Return NPC entity
    │
    ├─ ResolveLocationFromPlacementFilter(filter, npc)
    │  └─ Derive location from NPC.Location
    │     └─ Return Location entity
    │
    └─→ _generationFacade.GenerateSceneFromArchetype(
        sceneArchetypeId="tutorial_secure_lodging",
        tier=0,
        npcId="elena",
        locationId="elena_room")
        │
        └─→ SceneGenerationFacade.GenerateSceneFromArchetype()
            │
            ├─ npc = GameWorld.NPCs.FirstOrDefault(n => n.ID == "elena")
            ├─ location = GameWorld.Locations.FirstOrDefault(l => l.Id == "elena_room")
            ├─ player = GameWorld.GetPlayer()
            │
            └─→ GenerationContext context = GenerationContext.FromEntities(
                tier=0,
                npc=elena_entity,
                location=elena_room_entity,
                player=player_entity)
                │
                ├─ context.Tier = 0
                ├─ context.NpcPersonality = MERCANTILE (from npc.PersonalityType)
                ├─ context.NpcLocationId = "elena_room"
                ├─ context.NpcId = "elena"
                ├─ context.NpcName = "Elena"
                ├─ context.PlayerCoins = 50 (from player.Coins)
                ├─ context.LocationProperties = [restful, indoor, private]
                │
                ├─ context.ServiceType = Lodging ✗ (HARDCODED DEFAULT)
                ├─ context.ServiceQuality = Standard ✗ (HARDCODED DEFAULT)
                ├─ context.SpotComfort = Standard ✗ (HARDCODED DEFAULT)
                └─ context.NPCDemeanor = Neutral ✗ (HARDCODED DEFAULT)
                    │
                    └─→ SceneArchetypeCatalog.Generate(
                        archetypeId="tutorial_secure_lodging",
                        tier=0,
                        context=context_with_defaults)
                        │
                        └─→ GenerateTutorialSecureLodging(tier, context)
                            │
                            ├─ DetermineNegotiationArchetype(context)
                            │  └─ Reads: context.NpcPersonality = MERCANTILE
                            │     Returns: "service_transaction" archetype
                            │
                            ├─ GenerateServiceNegotiationHints(context, serviceId)
                            │  ├─ Reads: context.NpcPersonality = MERCANTILE
                            │  ├─ Reads: context.PlayerCoins = 50
                            │  └─ Sets tone/theme based on personality
                            │
                            └─ Create SituationTemplates (3 situations)
                               with ChoiceTemplates (hardcoded rewards)
```

---

## 8. WHERE CATEGORICAL PROPERTIES SHOULD BE SET

### Option A: From JSON (SceneTemplateDTO)

Add fields to JSON:
```json
{
  "id": "lodging_scene",
  "sceneArchetypeId": "service_with_location_access",
  "serviceType": "Lodging",
  "serviceQuality": "Standard",
  "spotComfort": "Premium",
  "placementFilter": { ... }
}
```

Then in parser:
```csharp
var context = new GenerationContext
{
    // ... existing fields
    ServiceType = Enum.Parse<ServiceType>(dto.ServiceType),
    ServiceQuality = Enum.Parse<ServiceQuality>(dto.ServiceQuality),
    // Pass to facade
};
```

### Option B: Derive from Entity State (Recommended)

**ServiceQuality** ← Location.Tier mapping:
```csharp
ServiceQuality = location.Tier switch
{
    <= 1 => ServiceQuality.Basic,
    2 => ServiceQuality.Standard,
    3 => ServiceQuality.Premium,
    _ => ServiceQuality.Luxury
};
```

**SpotComfort** ← Location.LocationProperties:
```csharp
SpotComfort = location.LocationProperties.Contains(LocationPropertyType.comfortable)
    ? SpotComfort.Premium
    : location.LocationProperties.Contains(LocationPropertyType.adequate)
        ? SpotComfort.Standard
        : SpotComfort.Basic;
```

**NPCDemeanor** ← NPC.RelationshipFlow:
```csharp
NPCDemeanor = npc.RelationshipFlow switch
{
    <= 9 => NPCDemeanor.Hostile,
    <= 19 => NPCDemeanor.Neutral,
    _ => NPCDemeanor.Friendly
};
```

**ServiceType** ← JSON + NPC Profession:
```csharp
ServiceType = npc.Profession switch
{
    Professions.Innkeeper => ServiceType.Lodging,
    Professions.Bathhouse_Attendant => ServiceType.Bathing,
    Professions.Healer => ServiceType.Healing,
    _ => ServiceType.Lodging
};
```

### Option C: Hybrid (JSON Overrides, Entity Defaults)

Allow JSON to specify, but default to derived values if not specified.

---

## 9. CURRENT GAPS SUMMARY

| Property | Source | Current Status | Should Be |
|----------|--------|-----------------|-----------|
| `Tier` | SceneTemplateDTO.tier | ✓ Properly set | ✓ Correct |
| `NpcPersonality` | NPC.PersonalityType | ✓ Properly set | ✓ Correct |
| `NpcId` | Resolved from PlacementFilter | ✓ Properly set | ✓ Correct |
| `NpcName` | NPC.Name | ✓ Properly set | ✓ Correct |
| `NpcLocationId` | NPC.Location.Id | ✓ Properly set | ✓ Correct |
| `PlayerCoins` | Player.Coins | ✓ Properly set | ✓ Correct |
| `LocationProperties` | Location.LocationProperties | ✓ Properly set | ✓ Correct |
| `ServiceType` | ??? | ✗ Hardcoded to Lodging | ✓ Should come from JSON or NPC Profession |
| `ServiceQuality` | ??? | ✗ Hardcoded to Standard | ✓ Should derive from Location.Tier |
| `SpotComfort` | ??? | ✗ Hardcoded to Standard | ✓ Should derive from Location.LocationProperties |
| `NPCDemeanor` | ??? | ✗ Hardcoded to Neutral | ✓ Should derive from NPC.RelationshipFlow |

---

## 10. ARCHITECTURAL RECOMMENDATIONS

1. **Populate GenerationContext completely** in SceneGenerationFacade.GenerateSceneFromArchetype()
   - Derive ServiceQuality from location.Tier
   - Derive SpotComfort from location.LocationProperties
   - Derive NPCDemeanor from npc.RelationshipFlow
   - Read ServiceType from JSON or NPC.Profession

2. **Use categorical properties in catalogue methods**
   - Scale choice costs by ServiceQuality multiplier
   - Scale reward amounts by SpotComfort multiplier
   - Scale requirement thresholds by NPCDemeanor multiplier
   - Vary situation archetypes by ServiceType

3. **Add catalogue calls to use context**
   - Create helper methods: `ScaleChoiceCost()`, `ScaleReward()`, `ScaleRequirement()`
   - Call these from catalogue generation methods

4. **Update test to verify context population**
   - Assert all context properties are properly set
   - Verify catalogue uses all context properties
   - Test that variations work correctly

