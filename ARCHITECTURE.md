# WAYFARER SYSTEM ARCHITECTURE

**CRITICAL: This document MUST be read and understood before making ANY changes to the Wayfarer codebase.**

## TABLE OF CONTENTS

1. [System Overview](#system-overview)
2. [Content Pipeline Architecture](#content-pipeline-architecture)
3. [Action Execution Pipeline](#action-execution-pipeline)
4. [GameWorld State Management](#gameworld-state-management)
5. [Service & Subsystem Layer](#service--subsystem-layer)
6. [UI Component Architecture](#ui-component-architecture)
7. [Data Flow Patterns](#data-flow-patterns)
8. [Critical Architectural Principles](#critical-architectural-principles)
9. [Component Dependencies](#component-dependencies)

---

## FUNDAMENTAL GAME SYSTEM ARCHITECTURE

**⚠️ READ THIS FIRST - THIS IS THE MOST IMPORTANT SECTION ⚠️**

### THE CORE PROGRESSION FLOW

**This is the single most important diagram in the entire codebase. Every feature must fit into this flow:**

```
Obligation (multi-phase mystery structure - domain entity)
  ↓ spawns
Obstacles (challenges in the world - domain entity)
  ↓ contain
Goals (approaches to overcome obstacles - domain entity)
  ↓ appear at
Locations/NPCs/Routes (placement context - NOT ownership)
  ↓ when player engages, Goals become
Challenges (Social/Mental/Physical - gameplay subsystems)
  ↓ player plays
GoalCards (tactical victory conditions - domain entity)
  ↓ achieve
Goal Completion (mechanical outcomes)
  ↓ contributes to
Obstacle Progress (tracked progress)
  ↓ leads to
Obstacle Defeated
  ↓ advances
Obligation Phase Completion
  ↓ unlocks
Next Obligation Phase / Completion
```

### CRITICAL TERMINOLOGY DISTINCTIONS

**These terms are NOT interchangeable - using them incorrectly breaks the architecture:**

#### Obligation ≠ Card
- **Obligation** = Multi-phase mystery/quest structure (e.g., "Investigate the Missing Grain")
- **GoalCard** = Tactical card played during challenges (e.g., "Sharp Remark" card)
- **NEVER** create "ObligationCard" entities - this makes no architectural sense
- There is NO such thing as an "obligation card" in the game

#### Goal ≠ GoalCard
- **Goal** = Strategic approach to overcome an obstacle (e.g., "Persuade the guard")
  - Appears at specific location/NPC
  - Defines challenge type (Social/Mental/Physical)
  - Contains GoalCards as victory conditions
- **GoalCard** = Tactical card played to achieve goal (e.g., "Reach 15 Understanding")
  - Has mechanical costs and effects
  - Played during active challenge
- **Goals CONTAIN GoalCards**, not the other way around

#### Obstacle ≠ Challenge
- **Obstacle** = Persistent barrier in the world (e.g., "Suspicious Guard", "Locked Gate")
  - Exists in GameWorld permanently
  - Contains multiple Goals (different approaches)
  - Tracks progress across all goal attempts
- **Challenge** = Active tactical gameplay session (Social/Mental/Physical subsystem)
  - Temporary - starts when player engages a Goal
  - Ends when Goal succeeds/fails
  - NOT a persistent entity

#### Location/NPC ≠ Owner
- **Locations** and **NPCs** are PLACEMENT CONTEXT where Goals appear
- They do NOT own Goals
- **Obstacles** own Goals (lifecycle control)
- **Obligations** own Obstacles (lifecycle control)
- Locations/NPCs just provide spatial/narrative context for where Goals are accessible

### ENTITY OWNERSHIP HIERARCHY

```
GameWorld (single source of truth)
 │
 ├─ Obligations (List<Obligation>)
 │   ├─ Spawns Obstacles (by ID reference)
 │   └─ Tracks multi-phase progress
 │
 ├─ Obstacles (List<Obstacle>)
 │   ├─ Owned by Obligations
 │   ├─ Contains Goals (by ID reference)
 │   └─ Tracks progress across goal attempts
 │
 ├─ Goals (List<Goal>)
 │   ├─ Owned by Obstacles
 │   ├─ Has locationId (PLACEMENT - appears at location)
 │   ├─ Has npcId (PLACEMENT - optional social context)
 │   ├─ Defines challenge type (Social/Mental/Physical)
 │   └─ Contains GoalCards (List<GoalCard> inline - OWNERSHIP)
 │
 ├─ Locations (List<Location>)
 │   └─ Does NOT own Goals - just placement context
 │
 └─ NPCs (List<NPC>)
     └─ Does NOT own Goals - just social context
```

### DATA FLOW EXAMPLE

**Player discovers obligation "Investigate the Missing Grain":**

1. **Discovery**: `ObligationDiscoveryModal` shows narrative
2. **Spawn**: Obligation spawns Obstacle "Merchant's Suspicion"
3. **Goals Created**: Obstacle contains 3 Goals:
   - Goal A: "Persuade merchant" (Social) → appears at Market, NPC=Merchant
   - Goal B: "Search ledger" (Mental) → appears at Storage Room
   - Goal C: "Follow suspicious patron" (Physical) → appears at Market
4. **Player Engagement**: Player clicks Goal A "Persuade merchant"
5. **Challenge Starts**: Social challenge begins with Goal A's GoalCards
6. **Tactical Play**: Player plays GoalCards to build Understanding/Trust/etc.
7. **Goal Completion**: Player reaches victory condition (e.g., 15 Understanding)
8. **Progress**: Obstacle "Merchant's Suspicion" progress increases
9. **Obstacle Defeated**: When enough goals completed, obstacle defeated
10. **Obligation Phase**: Current phase completes, next phase unlocks

### FORBIDDEN PATTERNS (DELETE ON SIGHT)

```csharp
// ❌ WRONG - "ObligationCard" as entity (NO SUCH THING)
public class ObligationCard { }
public class ObligationCardDTO { }

// ❌ WRONG - Locations owning Goals
public class Location
{
    public List<Goal> Goals { get; set; } // NO! Placement, not ownership!
}

// ❌ WRONG - NPCs owning Obstacles
public class NPC
{
    public List<Obstacle> Obstacles { get; set; } // NO! Context, not ownership!
}

// ❌ WRONG - Goals owning Obstacles (backwards!)
public class Goal
{
    public List<Obstacle> Obstacles { get; set; } // NO! Obstacles own Goals!
}
```

### CORRECT PATTERNS

```csharp
// ✅ CORRECT - Obligation spawns Obstacles
public class Obligation
{
    public List<string> ActiveObstacleIds { get; set; }  // Owns obstacles by ID
    public List<ObligationPhase> Phases { get; set; }    // Phase structure
}

// ✅ CORRECT - Obstacle owns Goals
public class Obstacle
{
    public string ObligationId { get; set; }      // Parent reference
    public List<string> GoalIds { get; set; }     // Owns goals by ID
    public int RequiredGoalsToDefeat { get; set; }
}

// ✅ CORRECT - Goal references placement context
public class Goal
{
    public string ObstacleId { get; set; }        // Parent reference
    public string LocationId { get; set; }        // PLACEMENT context
    public string NpcId { get; set; }             // PLACEMENT context (optional)
    public ChallengeType ChallengeType { get; set; }  // Social/Mental/Physical
    public List<GoalCard> GoalCards { get; set; } // OWNS victory conditions
}

// ✅ CORRECT - GoalCard is tactical mechanic
public class GoalCard
{
    public string Id { get; set; }
    public string Title { get; set; }
    public GoalCardCosts Costs { get; set; }      // Resources to play
    public GoalCardEffect Effect { get; set; }    // Mechanical outcome
}
```

---

## SYSTEM OVERVIEW

Wayfarer is a **low-fantasy tactical RPG** with **three parallel challenge systems** (Social, Mental, Physical) built with **C# ASP.NET Core** and **Blazor Server**. The architecture follows **clean architecture principles** with strict **dependency inversion** and **single responsibility** patterns.

### Core Design Philosophy
- **GameWorld as Single Source of Truth**: All game state flows through GameWorld with zero external dependencies
- **Three Parallel Tactical Systems**: Social (conversations), Mental (investigations), Physical (obstacles) with equivalent depth
- **Strategic-Tactical Bridge**: Goals are first-class entities that connect strategic planning to tactical execution
- **Static Content Loading**: JSON content parsed once at startup without DI dependencies
- **Facade Pattern**: Business logic coordinated through specialized facades
- **Authoritative UI Pattern**: GameScreen owns all UI state, children communicate upward
- **No Shared Mutable State**: Services provide operations, not state storage

---

## CONTENT PIPELINE ARCHITECTURE

### 1. JSON Content Structure

**Location**: `src/Content/Core/*.json`

**Package Loading Order** (numbered files loaded alphabetically):
```
01_foundation.json     → Player stats, time blocks, base configuration
03_npcs.json          → NPC definitions with personalities and initial tokens
04_connections.json   → Routes and travel connections
05_goals.json         → Goal definitions (strategic-tactical bridge)
06_gameplay.json      → Venues, locations, game rules
07_equipment.json     → Items and equipment definitions
08_social_cards.json  → Social challenge cards (conversations)
09_mental_cards.json  → Mental challenge cards (investigations)
10_physical_cards.json → Physical challenge cards (obstacles)
12_challenge_decks.json → Deck configurations for all three systems
13_investigations.json  → Multi-phase investigation templates
14_knowledge.json      → Knowledge entries and discovery system
```

**Content Relationships**:
- **Goals**: First-class entities with `npcId` OR `locationId` for assignment
- **Challenge Decks**: Reference card IDs for Social/Mental/Physical systems
- **Investigations**: Reference goals via phase definitions
- **Cards**: Bind to unified 5-stat system (Insight/Rapport/Authority/Diplomacy/Cunning)
- All relationships use string IDs for loose coupling

### 2. Static Parser Layer

**Location**: `src/Content/*Parser.cs`

**Parser Responsibilities**:
```csharp
SocialCardParser     → SocialCardDTO → SocialCard
MentalCardParser     → MentalCardDTO → MentalCard
PhysicalCardParser   → PhysicalCardDTO → PhysicalCard
NPCParser            → NPCDTO → NPC
VenueParser          → VenueDTO → Venue
LocationParser       → LocationDTO → Location
GoalParser           → GoalDTO → Goal
InvestigationParser  → InvestigationDTO → Investigation
KnowledgeParser      → KnowledgeDTO → Knowledge
```

**CRITICAL PARSER PRINCIPLES**:
- **PARSE AT THE BOUNDARY**: JSON artifacts NEVER pollute domain layer
- **NO JsonElement PASSTHROUGH**: Parsers MUST convert to strongly-typed objects
- **NO Dictionary<string, object>**: Use proper typed properties on domain models
- **JSON FIELD NAMES MUST MATCH C# PROPERTIES**: No JsonPropertyName attributes to hide mismatches
- **STATELESS**: Parsers are static classes with no side effects
- **SINGLE PASS**: Each parser converts DTO to domain entity in one operation
- **CATEGORICAL → MECHANICAL TRANSLATION**: Parsers translate categorical JSON properties to absolute mechanical values through catalogues (see Categorical Properties Pattern below)

### 3. Categorical Properties → Dynamic Scaling Pattern (AI Content Generation)

**CRITICAL ARCHITECTURE DECISION: Why JSON uses categorical properties instead of absolute values**

**The Problem: AI-Generated Runtime Content**

AI-generated content (procedural generation, LLM-created entities, user-generated content) CANNOT specify absolute mechanical values because AI doesn't know:
- Current player progression level (Level 1 vs Level 10)
- Existing game balance (what items/cards/challenges already exist)
- Global difficulty curve (early game vs late game tuning)
- Economy state (coin inflation, resource scarcity)

**The Solution: Relative Categorical Properties + Dynamic Scaling Catalogues**

```
AI generates JSON with categorical properties (relative descriptions)
    ↓
Parser reads current game state (player level, difficulty mode, etc.)
    ↓
Catalogue translates categorical → absolute values WITH SCALING
    ↓
Domain entity receives scaled mechanical values
```

**Example: Equipment Durability**

```csharp
// JSON (AI-generated or hand-authored): Categorical property
{
  "id": "worn_rope",
  "name": "Worn Climbing Rope",
  "durability": "Fragile"    // ← RELATIVE category, not absolute value
}

// Parser translates using catalogue + game state
DurabilityType durability = ParseEnum(dto.Durability);  // Fragile
int playerLevel = gameWorld.Player.Level;                // Current: 3
DifficultyMode difficulty = gameWorld.CurrentDifficulty; // Normal

(int uses, int repairCost) = EquipmentDurabilityCatalog.GetDurabilityValues(
    durability, playerLevel, difficulty);

// Catalogue scales based on game state:
// Level 1:  Fragile → 2 uses, 10 coins
// Level 5:  Fragile → 4 uses, 25 coins  (scaled up for progression)
// Level 10: Fragile → 6 uses, 40 coins  (continues scaling)

// CRITICAL: Fragile ALWAYS weaker than Sturdy (relative consistency maintained)
```

**Example: Card Effects**

```csharp
// JSON: Categorical move type
{
  "conversationalMove": "Remark",
  "boundStat": "Rapport",
  "depth": 2
}

// Parser translates with scaling
CardEffectFormula effect = SocialCardEffectCatalog.GetEffectFromCategoricalProperties(
    ConversationalMove.Remark,
    PlayerStatType.Rapport,
    depth: 2,
    cardId,
    playerLevel);  // ← Scaling factor

// Early game (Level 1): Remark/Rapport/Depth2 → +4 Understanding
// Late game (Level 5): Remark/Rapport/Depth2 → +6 Understanding (scaled)
```

**Why This Architecture Exists:**

1. **AI Content Generation**: AI describes entities relatively ("Fragile rope", "Cunning NPC") without needing to know absolute game values
2. **Dynamic Difficulty Scaling**: Same content scales automatically as player progresses
3. **Consistent Relative Balance**: "Fragile" ALWAYS weaker than "Sturdy" regardless of scaling factors
4. **Future-Proof**: Supports procedural generation, LLM content, user mods, runtime content
5. **Centralized Balance**: Change ONE catalogue formula → ALL entities of that category scale consistently

**Catalogue Implementation Requirements:**

```csharp
// Location: src/Content/Catalogues/*Catalog.cs
public static class EquipmentDurabilityCatalog
{
    // Context-aware scaling function
    public static (int exhaustAfterUses, int repairCost) GetDurabilityValues(
        DurabilityType durability,
        int playerLevel,           // ← Scaling context
        DifficultyMode difficulty) // ← Scaling context
    {
        // Base values for each category
        int baseUses = durability switch
        {
            DurabilityType.Fragile => 2,
            DurabilityType.Sturdy => 5,
            DurabilityType.Durable => 8,
            _ => throw new InvalidOperationException($"Unknown durability: {durability}")
        };

        int baseRepair = durability switch
        {
            DurabilityType.Fragile => 10,
            DurabilityType.Sturdy => 25,
            DurabilityType.Durable => 40,
            _ => throw new InvalidOperationException($"Unknown durability: {durability}")
        };

        // Dynamic scaling based on game state
        float levelScaling = 1.0f + (playerLevel * 0.2f); // +20% per level
        float difficultyScaling = difficulty switch
        {
            DifficultyMode.Easy => 1.2f,
            DifficultyMode.Normal => 1.0f,
            DifficultyMode.Hard => 0.8f,
            _ => 1.0f
        };

        int scaledUses = (int)(baseUses * levelScaling * difficultyScaling);
        int scaledRepair = (int)(baseRepair * levelScaling * difficultyScaling);

        return (scaledUses, scaledRepair);
    }
}
```

**Existing Catalogues:**
- `SocialCardEffectCatalog` (src/Content/Catalogs/SocialCardEffectCatalog.cs)
- `MentalCardEffectCatalog` (src/Content/Catalogs/MentalCardEffectCatalog.cs)
- `PhysicalCardEffectCatalog` (src/Content/Catalogs/PhysicalCardEffectCatalog.cs)
- `EquipmentDurabilityCatalog` (src/Content/Catalogs/EquipmentDurabilityCatalog.cs)

**When to Use Categorical Properties:**

Ask these questions for ANY numeric property in a DTO:
1. "Could AI generate this entity at runtime without knowing global game state?"
2. "Should this value scale with player progression or difficulty?"
3. "Is this RELATIVE (compared to similar entities) rather than ABSOLUTE?"

If YES → Create categorical enum + scaling catalogue
If NO → Consider if it's truly a design-time constant (rare - most values should scale)

**Anti-Pattern: Hardcoded Absolute Values in JSON**

```json
// ❌ WRONG - Absolute values break AI generation and scaling
{
  "exhaustAfterUses": 2,
  "repairCost": 10,
  "understanding": 4,
  "momentum": 2
}

// ✅ CORRECT - Categorical properties enable AI + scaling
{
  "durability": "Fragile",
  "conversationalMove": "Remark",
  "depth": 2
}
```

### 4. Content Loading Orchestration

**Location**: `src/Content/PackageLoader.cs` & `GameWorldInitializer.cs`

**Loading Sequence**:
```
Startup → GameWorldInitializer.CreateGameWorld()
       → PackageLoader.LoadContent()
       → Static Parsers (for each content type)
       → Domain Objects → GameWorld Population
```

**Initialization Architecture**:
- `GameWorldInitializer` is **STATIC** - no DI dependencies
- Creates GameWorld **BEFORE** service registration completes
- Prevents circular dependencies during ServerPrerendered mode
- Content loaded once at startup, never reloaded

---

## ACTION EXECUTION PIPELINE

**Location**: `src/Content/Parsers/*ActionParser.cs`, `src/GameState/*Action.cs`, `src/Services/GameFacade.cs`

### 1. Action System Overview

**Two Action Types**:
- **PlayerActions**: Global actions available everywhere (e.g., "Check Belongings", "Wait")
- **LocationActions**: Context-specific actions available at certain locations (e.g., "Travel", "Rest", "Work")

**Architecture Goal**: Eliminate "Dictionary Disease" (string-based matching) through strongly-typed enum catalogues with parser validation and single-point dispatch through GameFacade.

### 2. Enum Catalogues (Action Type Definition)

**Location**: `src/Content/PlayerActionType.cs`, `src/Content/LocationActionType.cs`

**PlayerActionType Enum** - Global actions available everywhere:
```csharp
public enum PlayerActionType
{
    CheckBelongings,  // View inventory/equipment screen
    Wait              // Skip 1 time segment, no resource recovery
}
```

**LocationActionType Enum** - Location-specific actions:
```csharp
public enum LocationActionType
{
    Travel,      // Navigate to connected routes
    Rest,        // Recover +1 health, +1 stamina (requires "rest"/"restful" property)
    Work,        // Earn coins based on location opportunities
    Investigate  // Gain location familiarity through observation
}
```

**Purpose**: Single source of truth for valid action types, enables compile-time type safety and parser validation.

### 3. Domain Entities with Strong Typing

**Location**: `src/GameState/PlayerAction.cs`, `src/GameState/LocationAction.cs`

**PlayerAction** - Global action entity:
```csharp
public class PlayerAction
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public PlayerActionType ActionType { get; set; }  // STRONGLY TYPED ENUM
    public Dictionary<string, int> Cost { get; set; }
    public int TimeRequired { get; set; }
    public int Priority { get; set; }
}
```

**LocationAction** - Location-specific action entity:
```csharp
public class LocationAction
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public LocationActionType ActionType { get; set; }  // STRONGLY TYPED ENUM
    public Dictionary<string, int> Cost { get; set; }
    public Dictionary<string, int> Reward { get; set; }
    public int TimeRequired { get; set; }
    public List<string> Availability { get; set; }
    public int Priority { get; set; }
    public string InvestigationId { get; set; }
    public List<LocationPropertyType> RequiredProperties { get; set; }  // Property-based matching
    public List<LocationPropertyType> OptionalProperties { get; set; }
    public List<LocationPropertyType> ExcludedProperties { get; set; }
}
```

**Critical**: `ActionType` is strongly-typed enum, NOT string. This eliminates runtime string matching errors.

### 4. Parsers with Enum Validation

**Location**: `src/Content/Parsers/PlayerActionParser.cs`, `src/Content/Parsers/LocationActionParser.cs`

**PlayerActionParser** - Validates actionType against enum catalogue:
```csharp
public static PlayerAction ParsePlayerAction(PlayerActionDTO dto)
{
    ValidateRequiredFields(dto);

    // ENUM VALIDATION - throws InvalidDataException if unknown action type
    if (!Enum.TryParse<PlayerActionType>(dto.ActionType, true, out PlayerActionType actionType))
    {
        string validTypes = string.Join(", ", Enum.GetNames(typeof(PlayerActionType)));
        throw new InvalidDataException(
            $"PlayerAction '{dto.Id}' has unknown actionType '{dto.ActionType}'. " +
            $"Valid types: {validTypes}");
    }

    return new PlayerAction
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        ActionType = actionType,  // Strongly typed enum assigned
        Cost = dto.Cost ?? new Dictionary<string, int>(),
        TimeRequired = dto.TimeRequired,
        Priority = dto.Priority
    };
}
```

**LocationActionParser** - Validates actionType and converts property strings to enums:
```csharp
public static LocationAction ParseLocationAction(LocationActionDTO dto)
{
    ValidateRequiredFields(dto);

    // ENUM VALIDATION - throws InvalidDataException if unknown action type
    if (!Enum.TryParse<LocationActionType>(dto.ActionType, true, out LocationActionType actionType))
    {
        string validTypes = string.Join(", ", Enum.GetNames(typeof(LocationActionType)));
        throw new InvalidDataException(
            $"LocationAction '{dto.Id}' has unknown actionType '{dto.ActionType}'. " +
            $"Valid types: {validTypes}");
    }

    return new LocationAction
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        ActionType = actionType,  // Strongly typed enum assigned
        Cost = dto.Cost ?? new Dictionary<string, int>(),
        Reward = dto.Reward ?? new Dictionary<string, int>(),
        TimeRequired = dto.TimeRequired,
        Availability = dto.Availability ?? new List<string>(),
        Priority = dto.Priority,
        InvestigationId = dto.InvestigationId,
        RequiredProperties = ParseLocationProperties(dto.RequiredProperties),
        OptionalProperties = ParseLocationProperties(dto.OptionalProperties),
        ExcludedProperties = ParseLocationProperties(dto.ExcludedProperties)
    };
}

// Converts property strings to LocationPropertyType enums
private static List<LocationPropertyType> ParseLocationProperties(List<string> propertyStrings)
{
    // Returns empty list if null, logs warning if property doesn't match enum
}
```

**Parser Integration**: `PackageLoader.cs` calls these parsers during content loading:
```csharp
// In PackageLoader.LoadLocationActions()
LocationAction action = LocationActionParser.ParseLocationAction(dto);

// In PackageLoader.LoadPlayerActions()
PlayerAction action = PlayerActionParser.ParsePlayerAction(dto);
```

**Why This Matters**: Unknown action types crash at startup with descriptive error messages, preventing runtime bugs from malformed JSON.

### 5. JSON Content Definition

**Location**: `src/Content/Core/01_foundation.json`

**PlayerAction JSON Example**:
```json
{
  "playerActions": [
    {
      "id": "wait",
      "name": "Wait",
      "description": "Pass time without activity. Skips 1 time segment with no resource recovery.",
      "actionType": "Wait",
      "timeRequired": 1,
      "priority": 200
    },
    {
      "id": "check_belongings",
      "name": "Check Belongings",
      "description": "Review your current equipment and inventory.",
      "actionType": "CheckBelongings",
      "priority": 100
    }
  ]
}
```

**LocationAction JSON Example**:
```json
{
  "locationActions": [
    {
      "id": "rest",
      "name": "Rest",
      "description": "Take time to rest and recover. Restores +1 Health and +1 Stamina.",
      "actionType": "Rest",
      "timeRequired": 1,
      "requiredProperties": ["rest", "restful"],
      "priority": 50
    },
    {
      "id": "travel",
      "name": "Travel",
      "description": "Choose a route to travel to a connected location.",
      "actionType": "Travel",
      "priority": 10
    }
  ]
}
```

**Property-Based Matching**: LocationActions use `requiredProperties`, `optionalProperties`, and `excludedProperties` to match against location properties. For example, "Rest" action only appears at locations with "rest" or "restful" property.

### 6. GameFacade Orchestration (Single Dispatch Point)

**Location**: `src/Services/GameFacade.cs`

**GameFacade Methods** - Single point of entry for action execution:
```csharp
// Execute PlayerAction by enum type
public async Task ExecutePlayerAction(PlayerActionType actionType)
{
    switch (actionType)
    {
        case PlayerActionType.CheckBelongings:
            // Navigate to equipment screen
            break;

        case PlayerActionType.Wait:
            // Delegate to ResourceFacade for time/hunger progression
            await _resourceFacade.ExecuteWait();
            break;

        default:
            throw new InvalidOperationException($"Unknown PlayerActionType: {actionType}");
    }
}

// Execute LocationAction by enum type
public async Task ExecuteLocationAction(LocationActionType actionType, string locationId)
{
    switch (actionType)
    {
        case LocationActionType.Travel:
            // Navigate to travel screen
            break;

        case LocationActionType.Rest:
            // Delegate to ResourceFacade for recovery
            await _resourceFacade.ExecuteRest();
            break;

        case LocationActionType.Work:
            // Delegate to ResourceFacade for work rewards
            await _resourceFacade.PerformWork();
            break;

        case LocationActionType.Investigate:
            // Delegate to LocationFacade for familiarity gain
            await _locationFacade.InvestigateLocation(locationId);
            break;

        default:
            throw new InvalidOperationException($"Unknown LocationActionType: {actionType}");
    }
}
```

**Why GameFacade**: Follows existing patterns like `ExecuteListen()`, `PerformWork()`, `TravelToDestinationAsync()`. GameFacade orchestrates specialized facades while keeping them decoupled.

### 7. Specialized Facade Implementation

**Location**: `src/Subsystems/Resource/ResourceFacade.cs`

**ResourceFacade.ExecuteRest()** - Example action handler:
```csharp
public async Task ExecuteRest()
{
    Player player = _gameWorld.GetPlayer();

    // Advance 1 time segment
    await _timeFacade.AdvanceSegments(1);

    // Hunger increases by +5 per segment (automatic via time progression)
    // No need to manually modify hunger here

    // Resource recovery
    player.Health = Math.Min(player.Health + 1, player.MaxHealth);      // +1 health (16.7% of 6-point max)
    player.Stamina = Math.Min(player.Stamina + 1, player.MaxStamina);   // +1 stamina (16.7% of 6-point max)
}
```

**ResourceFacade.ExecuteWait()** - Example action handler:
```csharp
public async Task ExecuteWait()
{
    // Advance 1 time segment
    await _timeFacade.AdvanceSegments(1);

    // Hunger increases by +5 per segment (automatic via time progression)
    // No resource recovery - just passing time
}
```

### 8. UI Layer Integration

**Location**: `src/Pages/Components/LocationContent.razor.cs`

**BEFORE** (String Matching - Anti-Pattern):
```csharp
// WRONG - scattered string matching
if (action.ActionType == "travel")
{
    // Handle travel inline
}
else if (action.ActionType == "rest")
{
    // Handle rest inline
}
```

**AFTER** (Enum Dispatch through GameFacade):
```csharp
// CORRECT - enum-based dispatch through GameFacade
private async Task HandleLocationAction(LocationAction action)
{
    await GameFacade.ExecuteLocationAction(action.ActionType, currentLocationId);
    await RefreshUI();
}

private async Task HandlePlayerAction(PlayerAction action)
{
    await GameFacade.ExecutePlayerAction(action.ActionType);
    await RefreshUI();
}
```

**Why This Works**: UI layer is dumb display that calls GameFacade with strongly-typed enums. GameFacade handles all dispatch logic. No string matching in UI layer.

### 9. Complete Vertical Slice Example: "Rest" Action

**Complete Flow from JSON to Execution**:

```
1. JSON DEFINITION (01_foundation.json)
   {
     "id": "rest",
     "name": "Rest",
     "actionType": "Rest",  // String in JSON
     "requiredProperties": ["rest", "restful"]
   }

2. PARSER VALIDATION (LocationActionParser.cs)
   - Reads JSON via PackageLoader
   - Validates "Rest" against LocationActionType enum
   - Throws InvalidDataException if "Rest" not in enum
   - Converts string "Rest" to LocationActionType.Rest enum
   - Converts property strings to LocationPropertyType enums

3. GAMEWORLD STORAGE
   - Parsed LocationAction stored in GameWorld.LocationActions
   - ActionType is LocationActionType.Rest (strongly typed)

4. UI DISPLAY (LocationContent.razor.cs)
   - Fetches available LocationActions for current location
   - Filters by property matching (location has "rest" or "restful")
   - Displays "Rest" card in UI

5. USER INTERACTION
   - Player clicks "Rest" card
   - UI calls: await HandleLocationAction(action)

6. GAMEFACADE DISPATCH (GameFacade.cs)
   - UI calls: await GameFacade.ExecuteLocationAction(LocationActionType.Rest, locationId)
   - GameFacade switches on action.ActionType enum
   - Case LocationActionType.Rest: delegates to ResourceFacade.ExecuteRest()

7. SPECIALIZED FACADE EXECUTION (ResourceFacade.cs)
   - ResourceFacade.ExecuteRest() executes game logic:
     - Advances 1 time segment via TimeFacade
     - Recovers +1 health
     - Recovers +1 stamina
     - Hunger increases +5 (automatic via time progression)

8. STATE UPDATE
   - Player resources updated in GameWorld
   - UI refreshes with new state
```

### 10. Critical Principles

**1. Enum Catalogues Prevent Runtime Errors**
- All valid action types defined in enums
- Parsers validate JSON against enums at startup
- Unknown action types crash with descriptive errors BEFORE game starts
- NO runtime string matching errors

**2. Single Dispatch Point in GameFacade**
- ALL action execution goes through GameFacade methods
- NO scattered string matching across multiple files
- GameFacade orchestrates specialized facades
- Follows existing patterns (ExecuteListen, PerformWork, etc.)

**3. Strong Typing Throughout Pipeline**
- JSON strings converted to enums by parsers
- Domain entities use enum types
- GameFacade switches on enums
- UI passes enums to GameFacade
- NO string comparisons at runtime

**4. Property-Based Matching for LocationActions**
- LocationActions use RequiredProperties/OptionalProperties/ExcludedProperties
- Locations tagged with LocationPropertyType enums
- Actions automatically filtered by property matching
- NO manual ID lists or hardcoded availability

**5. Specialized Facades Handle Domain Logic**
- GameFacade delegates to ResourceFacade, TimeFacade, LocationFacade, etc.
- Each facade encapsulates ONE business domain
- Facades remain decoupled (never call each other directly)
- GameFacade orchestrates cross-facade operations

**6. No Dictionary Disease**
- NO string-based actionType matching
- NO Dictionary<string, object> for actions
- NO runtime type checking or casting
- Strong typing enforced from JSON → Parser → Domain → UI

---

## GAMEWORLD STATE MANAGEMENT

**Location**: `src/GameState/GameWorld.cs`

### GameWorld Responsibilities

**State Collections**:
```csharp
// Core Entities
public List<NPC> NPCs { get; set; }
public List<Venue> Venues { get; set; }
public List<LocationEntry> Locations { get; set; }
private Player Player { get; set; }

// Three Parallel Tactical Systems - Card Templates
public List<SocialCard> SocialCards { get; set; }
public List<MentalCard> MentalCards { get; set; }
public List<PhysicalCard> PhysicalCards { get; set; }

// Three Parallel Tactical Systems - Challenge Decks
public Dictionary<string, SocialChallengeDeck> SocialChallengeDecks { get; }
public Dictionary<string, MentalChallengeDeck> MentalChallengeDecks { get; }
public Dictionary<string, PhysicalChallengeDeck> PhysicalChallengeDecks { get; }

// Strategic-Tactical Bridge
public Dictionary<string, Goal> Goals { get; }

// Investigation System
public List<Investigation> Investigations { get; }
public InvestigationJournal InvestigationJournal { get; }
public Dictionary<string, Knowledge> Knowledge { get; }

// Player Stats System
public List<PlayerStatDefinition> PlayerStatDefinitions { get; set; }
public StatProgression StatProgression { get; set; }
```

**State Operations**:
```csharp
GetPlayer() → Single player instance
GetPlayerResourceState() → Current player resources
GetLocation(string locationId) → Location by ID
GetAvailableStrangers() → NPCs available at venue/time
RefreshStrangersForTimeBlock() → Time-based NPC availability
ApplyInitialPlayerConfiguration() → Apply starting conditions from JSON
```

### CRITICAL GAMEWORLD PRINCIPLES

**1. Zero External Dependencies**
- GameWorld NEVER depends on services, managers, or external components
- All dependencies flow **INWARD** toward GameWorld, never outward
- GameWorld does **NOT** create any managers or services

**2. Single Source of Truth**
- ALL game state lives in GameWorld - no parallel state in services
- Services read/write to GameWorld but don't maintain their own copies
- NO SharedData dictionaries or TempData storage

**3. No Business Logic**
- GameWorld contains **STATE**, not **BEHAVIOR**
- Business logic belongs in services/facades, not GameWorld
- GameWorld provides data access, not game rules

---

## SERVICE & SUBSYSTEM LAYER

**Location**: `src/Services/GameFacade.cs` & `src/Subsystems/*/`

### Service Architecture Hierarchy

```
GameFacade (Pure Orchestrator)
├── THREE PARALLEL TACTICAL SYSTEMS
│   ├── SocialFacade (Social challenges - conversations with NPCs)
│   ├── MentalFacade (Mental challenges - investigations at locations)
│   └── PhysicalFacade (Physical challenges - obstacles at locations)
├── SUPPORTING SYSTEMS
│   ├── LocationFacade (Movement and spot management)
│   ├── ResourceFacade (Health, hunger, coins, stamina)
│   ├── TimeFacade (Time progression and segments)
│   ├── TravelFacade (Route management and travel)
│   ├── TokenFacade (Relationship tokens)
│   ├── NarrativeFacade (Messages and observations)
│   └── ExchangeFacade (NPC trading system)
└── INVESTIGATION & CONTENT
    ├── InvestigationActivity (Multi-phase investigation management)
    ├── InvestigationDiscoveryEvaluator (Discovery trigger evaluation)
    ├── KnowledgeService (Knowledge grants and discovery)
    └── GoalCompletionHandler (Goal completion and rewards)
```

### Facade Responsibilities

**GameFacade** - Pure orchestrator for UI-Backend communication
- Delegates ALL business logic to specialized facades
- Coordinates cross-facade operations (e.g., completing goals triggers investigations)
- Handles UI-specific orchestration
- NO business logic - only coordination

**Three Parallel Tactical System Facades** - Equivalent depth challenge systems
- `SocialFacade`: Initiative/Momentum/Doubt/Cadence conversation mechanics with NPCs
- `MentalFacade`: Progress/Attention/Exposure/Leads investigation mechanics at locations
- `PhysicalFacade`: Breakthrough/Exertion/Danger/Aggression obstacle mechanics at locations
- Each follows same architectural pattern: Builder/Threshold/Session resources + Binary actions
- All three systems use unified 5-stat progression (Insight/Rapport/Authority/Diplomacy/Cunning)

**Supporting Facades** - Game systems that integrate with tactical challenges
- `ExchangeFacade`: Separate NPC trading system (instant exchanges, not conversations)
- `LocationFacade`: Movement validation, location properties, spot management
- `ResourceFacade`: Permanent resources (Health, Stamina, Hunger, Focus, Coins)
- Each facade encapsulates ONE business domain

### Subsystem Organization

**Location**: `src/Subsystems/[Domain]/`

```
THREE PARALLEL TACTICAL SYSTEMS (Equivalent Depth)
Social/         → Social challenges: Card mechanics, momentum, conversation sessions
Mental/         → Mental challenges: Investigation mechanics, leads, observation system
Physical/       → Physical challenges: Obstacle mechanics, aggression, combo execution

SUPPORTING SYSTEMS
Exchange/       → NPC trading, inventory validation
Location/       → Movement, spot properties, location actions
Resource/       → Health, hunger, coins, stamina, focus management
Time/           → Segment progression, time block transitions
Token/          → Relationship tokens, connection tracking
Travel/         → Route discovery, travel validation
Narrative/      → Message system, observation rewards

INVESTIGATION & CONTENT SYSTEMS
Investigation/  → Multi-phase investigation lifecycle, goal spawning
Knowledge/      → Knowledge discovery, secrets, world state changes
Goals/          → Strategic-tactical bridge, victory conditions
```

---

## UI COMPONENT ARCHITECTURE

**Location**: `src/Pages/*.razor` & `src/Pages/Components/*.razor`

### Authoritative Page Pattern

**GameScreen.razor** - Single authoritative parent
- Owns ALL screen state and manages child components directly
- Provides outer structure (resources bar, headers, time display)
- Child components rendered INSIDE GameScreen's container
- Children call parent methods directly via CascadingValue

**Child Components** - Screen-specific content only
```
THREE PARALLEL TACTICAL SYSTEMS
ConversationContent.razor  → Social challenges (conversations with NPCs)
MentalContent.razor        → Mental challenges (investigations at locations)
PhysicalContent.razor      → Physical challenges (obstacles at locations)

SUPPORTING SCREENS
LocationContent.razor      → Location exploration UI
ExchangeContent.razor      → NPC trading interface
TravelContent.razor        → Route selection and travel

INVESTIGATION MODALS
InvestigationDiscoveryModal.razor  → Investigation discovery notifications
InvestigationActivationModal.razor → Investigation intro completion
InvestigationProgressModal.razor   → Phase completion notifications
InvestigationCompleteModal.razor   → Investigation completion rewards
```

### Component Communication Pattern

**Direct Parent-Child Communication**:
```csharp
// Child receives parent reference
<CascadingValue Value="@this">
  <LocationContent OnActionExecuted="RefreshUI" />
</CascadingValue>

// Child calls parent methods directly
GameScreen.StartConversation(npcId, requestId)
GameScreen.NavigateToQueue()
GameScreen.HandleTravelRoute(routeId)
```

**Context Objects for Complex State**:
```csharp
THREE PARALLEL TACTICAL SYSTEMS
SocialChallengeContext   → Complete conversation state (Social challenge)
MentalSession            → Investigation state (Mental challenge)
PhysicalSession          → Obstacle state (Physical challenge)

SUPPORTING CONTEXTS
ExchangeContext          → NPC trading session state
TravelDestinationViewModel → Route and destination display state
LocationScreenViewModel  → Location exploration state
```

### CRITICAL UI PRINCIPLES

**1. UI is Dumb Display Only**
- NO game logic in Razor components
- NO attention costs or availability logic in UI
- Backend determines ALL game mechanics through facades

**2. No Shared Mutable State**
- Services provide operations, NOT state storage
- NavigationCoordinator handles navigation ONLY, not data passing
- State lives in components, not services

**3. Screen Component Constraints**
- Screen components NEVER define their own game-container or headers
- GameScreen provides outer structure, children provide content only
- Screen components wrapped in semantic classes like 'conversation-content'

---

## DATA FLOW PATTERNS

### Complete Pipeline Flow

```
JSON Files (Content Definition)
    ↓ [Static Parsers]
Domain Models (Strongly Typed Objects)
    ↓ [GameWorldInitializer]
GameWorld (Single Source of Truth)
    ↓ [Service Facades]
Business Logic Operations
    ↓ [Context Objects]
UI Components (User Interface Display)
    ↓ [User Actions]
Service Facades (State Updates)
    ↓ [GameWorld Updates]
State Persistence
```

### Request/Response Flow

**User Action → UI Response**:
```
1. User clicks UI element
2. Component calls GameScreen method
3. GameScreen calls GameFacade method
4. GameFacade orchestrates subsystem facades
5. Facades execute business logic
6. Facades update GameWorld state
7. GameScreen refreshes UI with new state
```

### Context Creation Pattern

**Complex Operations Use Dedicated Contexts**:
```csharp
// Context created atomically BEFORE navigation
ConversationContext context = await GameFacade.CreateConversationContext(npcId, requestId);

// Context contains ALL data needed for operation
context.NpcInfo = npc data
context.LocationInfo = current location
context.PlayerResources = resource state
context.Session = conversation session

// Context passed as single parameter to child component
<ConversationContent Context="@context" />
```

---

## CRITICAL ARCHITECTURAL PRINCIPLES

### 1. Dependency Inversion
- **All dependencies flow INWARD toward GameWorld**
- GameWorld has zero external dependencies
- Services depend on GameWorld, not vice versa
- UI depends on services, services never depend on UI

### 2. Single Responsibility
- Each facade handles exactly ONE business domain
- GameWorld provides state access, not business logic
- UI components render state, don't contain game rules
- Parsers convert data formats, don't store state

### 3. Immutable Content Pipeline
- JSON content loaded once at startup
- Static parsers create immutable domain objects
- Content never reloaded or modified at runtime
- Domain models are data containers, not active objects

### 4. Clean Architecture Boundaries
```
UI Layer          → GameScreen, Components (Blazor)
Application Layer → GameFacade, Specialized Facades
Domain Layer      → GameWorld, Domain Models
Infrastructure    → Parsers, JSON Files
```

### 5. No Abstraction Over-Engineering
- **NO interfaces unless absolutely necessary**
- **NO inheritance hierarchies** - use composition
- **NO abstract base classes** - keep code direct
- Concrete classes only, straightforward implementations

### 6. State Isolation
- **NO parallel state tracking** across multiple objects
- When duplicate state found, identify single source of truth
- Other objects delegate to canonical source
- **NO caching layers** that can become stale

---

## COMPONENT DEPENDENCIES

### Core Dependencies Graph

```
GameScreen
├── Requires: GameFacade
├── Manages: All screen navigation
└── Children: LocationContent, ConversationContent, etc.

GameFacade
├── Requires: GameWorld + All Specialized Facades
├── Provides: Orchestration layer
└── Dependencies: ConversationFacade, LocationFacade, etc.

Specialized Facades
├── Require: GameWorld + Domain-specific managers
├── Provide: Business logic for specific domains
└── Update: GameWorld state through operations

GameWorld
├── Requires: Nothing (zero external dependencies)
├── Provides: Single source of truth for all state
└── Contains: All domain model collections
```

### Service Registration Pattern

```csharp
// GameWorld created BEFORE service registration
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();

// Services registered with GameWorld dependency
services.AddSingleton(gameWorld);
services.AddScoped<ConversationFacade>();
services.AddScoped<GameFacade>();
```

### Critical Integration Points

**1. Three Parallel Tactical Systems Integration**
- **SocialFacade**: Social challenges (conversations) at NPCs
  - Initiative/Momentum/Doubt/Cadence resource system
  - Integrates with TokenFacade for relationship tokens
  - Integrates with KnowledgeService for knowledge/secrets
  - Personality rules (Proud, Devoted, Mercantile, Cunning, Steadfast)

- **MentalFacade**: Mental challenges (investigations) at Locations
  - Progress/Attention/Exposure/Leads resource system
  - Pauseable session model (can leave and return)
  - Location properties (Delicate, Obscured, Layered, Time-Sensitive, Resistant)

- **PhysicalFacade**: Physical challenges (obstacles) at Locations
  - Breakthrough/Exertion/Danger/Aggression resource system
  - One-shot session model (must complete in single attempt)
  - Challenge types (Combat, Athletics, Finesse, Endurance, Strength)

**2. Goals as Strategic-Tactical Bridge**
- Goals are first-class entities in `GameWorld.Goals` dictionary
- Goals assigned to NPCs (`NPC.ActiveGoals`) for Social challenges
- Goals assigned to Locations (`Location.ActiveGoals`) for Mental/Physical challenges
- Investigations dynamically spawn goals when phase requirements met
- GoalCards define tiered victory conditions (8/12/16 momentum thresholds)

**3. Investigation System Integration**
- InvestigationActivity manages multi-phase lifecycle (Potential → Discovered → Active → Complete)
- Discovery triggers: ImmediateVisibility, EnvironmentalObservation, Conversational, Item, Obligation
- Dynamic goal spawning: Phase completion spawns next phase's goals at NPCs/Locations
- Knowledge system: Investigations grant knowledge that unlocks future phases

**4. Unified 5-Stat Progression**
- All cards across all three systems bind to: Insight/Rapport/Authority/Diplomacy/Cunning
- Stat levels determine card depth access (Level 1: depths 1-2, Level 3: depths 1-4, etc.)
- Playing cards grants XP to bound stat
- Stats manifest differently per system (Insight = pattern recognition in Mental, structural analysis in Physical, reading people in Social)

**5. Location System Integration**
- LocationFacade coordinates NPC placement at locations
- Integrates with TravelFacade for route validation
- Coordinates with TimeFacade for time-based availability
- Manages ActiveGoals for Mental/Physical challenges

**6. Resource System Integration**
- ResourceFacade manages permanent resources (Health, Stamina, Focus, Hunger, Coins)
- **Mental challenges cost Focus** (concentration depletes)
- **Physical challenges cost Health + Stamina** (injury risk + exertion)
- **Social challenges cost nothing permanent** (but take time)
- Integrates with TimeFacade for time-based resource changes

---

## DEVELOPMENT GUIDELINES

### Before Making Any Changes

1. **Read this entire architecture document**
2. **Identify which layer you're modifying** (Content/Domain/Service/UI)
3. **Trace dependencies** using search tools to find all references
4. **Understand the impact radius** of your changes
5. **Verify architectural principles** are maintained

### Adding New Features

1. **Determine the business domain** - which facade should own it?
2. **Check if GameWorld state** needs new properties
3. **Design the service interface** following existing patterns
4. **Create context objects** for complex UI operations
5. **Follow the UI component hierarchy** - GameScreen → Child Components

### Modifying Existing Systems

1. **Never break the dependency flow** - dependencies always flow inward
2. **Never add shared mutable state** - use GameWorld as single source
3. **Never put business logic in UI** - keep facades responsible for rules
4. **Never bypass the facade layer** - UI must go through GameFacade

---

**This architecture ensures clean separation of concerns, predictable data flow, and maintainable code structure while supporting the complex conversation-based RPG mechanics of Wayfarer.**